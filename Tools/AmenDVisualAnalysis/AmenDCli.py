#!/usr/bin/env python3
"""Command-line host for AMEN.D visual static analysis."""
from __future__ import annotations

import argparse
import copy
import json
import sys
import time
from pathlib import Path
from typing import Any, Mapping

import AmenDReference as ar


OUTPUT_FILENAMES = {
    "Results": "AmenD.VisualAnalysis.Results.json",
    "Summary": "AmenD.VisualAnalysis.Summary.json",
    "Sarif": "AmenD.VisualAnalysis.sarif",
    "Provenance": "AmenD.VisualAnalysis.prov.json",
    "ProblemMatcher": "AmenD.VisualAnalysis.ProblemMatcher.txt",
}


def _resolve(root: Path, value: str | Path) -> Path:
    path = Path(value)
    return path if path.is_absolute() else root / path


def _display_path(root: Path, path: Path) -> str:
    try:
        return path.resolve().relative_to(root.resolve()).as_posix()
    except ValueError:
        return str(path.resolve())


def _load_config(path: Path) -> dict[str, Any]:
    config = ar.read_json(path)
    if config.get("Schema") != "AmenD.VisualAnalysis/1.9.0":
        raise ValueError("Config Schema must be AmenD.VisualAnalysis/1.9.0")
    if not isinstance(config.get("Assets"), list) or not config["Assets"]:
        raise ValueError("Config must contain at least one asset")
    ids = [str(asset.get("AssetId", "")) for asset in config["Assets"]]
    if any(not value for value in ids) or len(ids) != len(set(ids)):
        raise ValueError("AssetId values must be non-empty and unique")
    return config


def _load_baseline(path: Path | None) -> dict[str, Any] | None:
    if path is None or not path.exists():
        return None
    baseline = ar.read_json(path)
    if baseline.get("Schema") != "AmenD.VisualAnalysis.Baseline/1.9.0":
        raise ValueError("Baseline Schema must be AmenD.VisualAnalysis.Baseline/1.9.0")
    return baseline


def _baseline_index(baseline: Mapping[str, Any] | None) -> dict[str, Mapping[str, Any]]:
    if baseline is None:
        return {}
    return {str(item["AssetId"]): item for item in baseline.get("Assets", [])}


def _cache_file(cache_directory: Path, asset_id: str) -> Path:
    safe = "".join(character if character.isalnum() or character in "._-" else "_" for character in asset_id)
    return cache_directory / f"{safe}.Core.json"


def _load_or_analyze_core(
    *,
    asset_config: Mapping[str, Any],
    repository_root: Path,
    analysis_config: Mapping[str, Any],
    cache_directory: Path,
    use_cache: bool,
) -> tuple[dict[str, Any], dict[str, Any]]:
    started = time.perf_counter()
    asset_path = _resolve(repository_root, str(asset_config["Path"])).resolve()
    truth_source = dict(asset_config.get("TruthSource", {"Mode": "Auto"}))
    key = ar.core_analysis_key(asset_path, truth_source, analysis_config)
    cache_path = _cache_file(cache_directory, str(asset_config["AssetId"]))
    if use_cache and cache_path.exists():
        cached = ar.read_json(cache_path)
        if cached.get("CacheKey", {}).get("xxh3_128_hex") == key["xxh3_128_hex"]:
            return cached["Core"], {
                "CoreCache": "Hit",
                "CachePath": _display_path(repository_root, cache_path),
                "CacheKey": key,
                "Diagnostics": "Recomputed",
                "CoreDurationMs": (time.perf_counter() - started) * 1000.0,
            }
    core = ar.analyze_core(
        asset_path,
        truth_source,
        analysis_config,
        logical_path=str(asset_config["Path"]),
    )
    if use_cache:
        ar.write_json(
            cache_path,
            {
                "Schema": "AmenD.VisualAnalysis.CoreCache/1.9.0",
                "CacheKey": key,
                "Core": core,
            },
        )
    return core, {
        "CoreCache": "Miss",
        "CachePath": _display_path(repository_root, cache_path),
        "CacheKey": key,
        "Diagnostics": "Recomputed",
        "CoreDurationMs": (time.perf_counter() - started) * 1000.0,
    }


def analyze_config(
    *,
    config_path: Path,
    output_directory: Path,
    fail_on: str,
    use_cache: bool,
    baseline_override: Path | None = None,
) -> tuple[dict[str, Any], int]:
    run_started = time.perf_counter()
    config_path = config_path.resolve()
    repository_root = config_path.parent
    config = _load_config(config_path)
    output_directory = output_directory.resolve()
    output_directory.mkdir(parents=True, exist_ok=True)
    cache_directory = output_directory / "Cache"
    cache_directory.mkdir(parents=True, exist_ok=True)

    configured_baseline = config.get("BaselinePath")
    baseline_path = baseline_override or (
        _resolve(repository_root, str(configured_baseline)).resolve() if configured_baseline else None
    )
    baseline = _load_baseline(baseline_path)
    baseline_by_asset = _baseline_index(baseline)
    analysis_config = dict(config.get("Analysis", {}))

    asset_records: list[dict[str, Any]] = []
    all_diagnostics: list[ar.Diagnostic] = []
    for asset_config in config["Assets"]:
        core, execution = _load_or_analyze_core(
            asset_config=asset_config,
            repository_root=repository_root,
            analysis_config=analysis_config,
            cache_directory=cache_directory,
            use_cache=use_cache,
        )
        asset_id = str(asset_config["AssetId"])
        diagnostic_started = time.perf_counter()
        diagnostics = ar.build_diagnostics(
            asset_config,
            core,
            baseline_by_asset.get(asset_id),
        )
        execution["DiagnosticDurationMs"] = (time.perf_counter() - diagnostic_started) * 1000.0
        all_diagnostics.extend(diagnostics)
        asset_records.append(
            {
                "AssetId": asset_id,
                "Path": str(asset_config["Path"]),
                "Execution": execution,
                "Core": core,
                "Diagnostics": [item.as_json() for item in diagnostics],
            }
        )

    counts = {"note": 0, "warning": 0, "error": 0}
    for diagnostic in all_diagnostics:
        key = "note" if diagnostic.severity in {"note", "info"} else diagnostic.severity
        counts[key] = counts.get(key, 0) + 1

    results = {
        "Schema": "AmenD.VisualAnalysis.Results/1.9.0",
        "Tool": {"Name": ar.TOOL_NAME, "Version": ar.TOOL_VERSION},
        "Config": {
            "Path": _display_path(repository_root, config_path),
            "Identity": ar.xxh3_file(config_path),
        },
        "Baseline": {
            "Path": _display_path(repository_root, baseline_path) if baseline_path else None,
            "Loaded": baseline is not None,
            "Identity": ar.xxh3_file(baseline_path) if baseline_path and baseline_path.exists() else None,
        },
        "Assets": asset_records,
        "DiagnosticCounts": counts,
        "Status": "FAIL" if counts.get("error", 0) else "PASS_WITH_WARNINGS" if counts.get("warning", 0) else "PASS",
    }
    results["Identity"] = ar.xxh3_json(results)

    sarif = ar.sarif_log(all_diagnostics)
    problem_lines = []
    for diagnostic in all_diagnostics:
        problem_lines.append(
            f"{diagnostic.asset_path}:1:1: {diagnostic.severity} {diagnostic.diagnostic_id}: {diagnostic.message}"
        )
    summary = {
        "Schema": "AmenD.VisualAnalysis.Summary/1.9.0",
        "Status": results["Status"],
        "AssetCount": len(asset_records),
        "DiagnosticCounts": counts,
        "Cache": {
            "Hits": sum(record["Execution"]["CoreCache"] == "Hit" for record in asset_records),
            "Misses": sum(record["Execution"]["CoreCache"] == "Miss" for record in asset_records),
        },
        "FailOn": fail_on,
        "DurationMsBeforeOutputWrite": (time.perf_counter() - run_started) * 1000.0,
        "CoreDurationMs": sum(float(record["Execution"]["CoreDurationMs"]) for record in asset_records),
        "DiagnosticDurationMs": sum(float(record["Execution"]["DiagnosticDurationMs"]) for record in asset_records),
    }

    result_path = output_directory / OUTPUT_FILENAMES["Results"]
    summary_path = output_directory / OUTPUT_FILENAMES["Summary"]
    sarif_path = output_directory / OUTPUT_FILENAMES["Sarif"]
    problem_path = output_directory / OUTPUT_FILENAMES["ProblemMatcher"]
    provenance_path = output_directory / OUTPUT_FILENAMES["Provenance"]
    ar.write_json(result_path, results)
    ar.write_json(summary_path, summary)
    ar.write_json(sarif_path, sarif)
    problem_path.write_text("\n".join(problem_lines) + ("\n" if problem_lines else ""), encoding="utf-8")

    provisional_identities = {
        "Results": ar.xxh3_file(result_path),
        "Summary": ar.xxh3_file(summary_path),
        "Sarif": ar.xxh3_file(sarif_path),
        "ProblemMatcher": ar.xxh3_file(problem_path),
    }
    provenance = ar.prov_graph(
        _display_path(repository_root, config_path), asset_records, provisional_identities
    )
    ar.write_json(provenance_path, provenance)

    embedded_outputs = {
        "Results": {"Path": _display_path(repository_root, result_path), "Identity": ar.xxh3_file(result_path)},
        "Sarif": {"Path": _display_path(repository_root, sarif_path), "Identity": ar.xxh3_file(sarif_path)},
        "Provenance": {"Path": _display_path(repository_root, provenance_path), "Identity": ar.xxh3_file(provenance_path)},
        "ProblemMatcher": {"Path": _display_path(repository_root, problem_path), "Identity": ar.xxh3_file(problem_path)},
    }
    summary["Outputs"] = embedded_outputs
    ar.write_json(summary_path, summary)
    outputs = {
        **embedded_outputs,
        "Summary": {"Path": _display_path(repository_root, summary_path), "Identity": ar.xxh3_file(summary_path)},
    }

    threshold = ar.severity_rank(fail_on)
    maximum = max((ar.severity_rank(item.severity) for item in all_diagnostics), default=-1)
    exit_code = 1 if threshold >= 0 and maximum >= threshold else 0
    return {"Results": results, "Summary": summary, "Outputs": outputs, "ProblemLines": problem_lines}, exit_code


def create_baseline(
    *,
    config_path: Path,
    baseline_path: Path,
    output_directory: Path,
    use_cache: bool,
) -> dict[str, Any]:
    config_path = config_path.resolve()
    repository_root = config_path.parent
    config = _load_config(config_path)
    analysis_config = dict(config.get("Analysis", {}))
    cache_directory = output_directory.resolve() / "Cache"
    cache_directory.mkdir(parents=True, exist_ok=True)
    assets = []
    for asset_config in config["Assets"]:
        core, _ = _load_or_analyze_core(
            asset_config=asset_config,
            repository_root=repository_root,
            analysis_config=analysis_config,
            cache_directory=cache_directory,
            use_cache=use_cache,
        )
        assets.append(
            ar.create_baseline_asset(
                str(asset_config["AssetId"]), str(asset_config["Path"]), core
            )
        )
    baseline = {
        "Schema": "AmenD.VisualAnalysis.Baseline/1.9.0",
        "Tool": {"Name": ar.TOOL_NAME, "Version": ar.TOOL_VERSION},
        "ConfigIdentity": ar.xxh3_file(config_path),
        "Policy": {
            "SourceIdentity": "Observe",
            "TranslationNormalizedIntrinsicGeometry": "Gate",
            "BaselineUpdate": "ExplicitReview",
        },
        "Assets": assets,
    }
    baseline["Identity"] = ar.xxh3_json(baseline)
    ar.write_json(baseline_path, baseline)
    return baseline


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="AmenDVisualAnalysis",
        description="AMEN.D visual static-analysis editor and build host",
    )
    subparsers = parser.add_subparsers(dest="Command", required=True)

    analyze = subparsers.add_parser("AnalyzeConfig", help="Analyze configured visual assets")
    analyze.add_argument("--Config", type=Path, default=Path("AmenD.VisualAnalysis.json"))
    analyze.add_argument("--OutputDirectory", type=Path, default=Path(".data/AmenDVisualAnalysis"))
    analyze.add_argument("--FailOn", choices=["None", "Warning", "Error"], default="Error")
    analyze.add_argument("--NoCache", action="store_true")
    analyze.add_argument("--PrintProblemMatcher", action="store_true")

    verify = subparsers.add_parser("VerifyBaseline", help="Analyze and verify the configured baseline")
    verify.add_argument("--Config", type=Path, default=Path("AmenD.VisualAnalysis.json"))
    verify.add_argument("--OutputDirectory", type=Path, default=Path(".data/AmenDVisualAnalysis"))
    verify.add_argument("--FailOn", choices=["None", "Warning", "Error"], default="Error")
    verify.add_argument("--NoCache", action="store_true")
    verify.add_argument("--PrintProblemMatcher", action="store_true")

    baseline = subparsers.add_parser("CreateBaseline", help="Create or replace a reviewed baseline")
    baseline.add_argument("--Config", type=Path, default=Path("AmenD.VisualAnalysis.json"))
    baseline.add_argument(
        "--Baseline", type=Path, default=Path("AmenD.VisualAnalysis.Baseline.json")
    )
    baseline.add_argument("--OutputDirectory", type=Path, default=Path(".data/AmenDVisualAnalysis"))
    baseline.add_argument("--NoCache", action="store_true")
    return parser


def main() -> int:
    args = _parser().parse_args()
    try:
        if args.Command == "CreateBaseline":
            baseline = create_baseline(
                config_path=args.Config,
                baseline_path=args.Baseline,
                output_directory=args.OutputDirectory,
                use_cache=not args.NoCache,
            )
            print(
                json.dumps(
                    {
                        "Baseline": str(args.Baseline),
                        "Identity": ar.xxh3_file(args.Baseline),
                        "AssetCount": len(baseline["Assets"]),
                    },
                    indent=2,
                )
            )
            return 0

        report, exit_code = analyze_config(
            config_path=args.Config,
            output_directory=args.OutputDirectory,
            fail_on=args.FailOn.lower(),
            use_cache=not args.NoCache,
        )
        if args.PrintProblemMatcher:
            for line in report["ProblemLines"]:
                print(line)
        print(json.dumps(report["Summary"], indent=2))
        return exit_code
    except (FileNotFoundError, ValueError, RuntimeError, json.JSONDecodeError) as error:
        print(f"AmenD.VisualAnalysis.json:1:1: error AODV0001: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
