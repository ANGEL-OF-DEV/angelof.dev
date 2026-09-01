from __future__ import annotations

def build_diagnostics(
    asset_config: Mapping[str, Any],
    core: Mapping[str, Any],
    baseline_asset: Mapping[str, Any] | None,
) -> list[Diagnostic]:
    asset_id = str(asset_config["AssetId"])
    asset_path = str(asset_config["Path"])
    diagnostics: list[Diagnostic] = []
    constraints = dict(asset_config.get("DesignConstraints", {}))

    horizontal = dict(constraints.get("HorizontalAnchor", {}))
    if horizontal.get("Mode") == "FrameCenter":
        tolerance = float(horizontal.get("TolerancePx", 0.75))
        offset = float(core["FrameAlignment"]["RepresentativeAxisOffsetPx"])
        if abs(offset) > tolerance:
            diagnostics.append(
                Diagnostic(
                    "AODV1001",
                    "Visual group is horizontally off-centre",
                    f"{asset_id} is displaced {offset:+.3f} px from the declared frame-centre anchor; tolerance is ±{tolerance:.3f} px.",
                    str(horizontal.get("Severity", "warning")).lower(),
                    asset_id,
                    asset_path,
                    {
                        "RepresentativeAxisOffsetPx": offset,
                        "TolerancePx": tolerance,
                        "FrameAlignment": core["FrameAlignment"],
                    },
                    {"FixProvider": "CenterCompleteVisualGroup", "FixExecuted": False},
                )
            )

    eigen_threshold = float(constraints.get("MinimumPrincipalAxisEigenGapRatio", 0.005))
    eigen_gap = float(core["Intrinsic"]["EigenGapRatio"])
    if eigen_gap < eigen_threshold:
        diagnostics.append(
            Diagnostic(
                "AODV2501",
                "Principal-axis direction is ill-conditioned",
                f"{asset_id} has eigen-gap ratio {eigen_gap:.6g}, below {eigen_threshold:.6g}; retain the eigenspace and suppress a unique direction claim.",
                "note",
                asset_id,
                asset_path,
                {"EigenGapRatio": eigen_gap, "Threshold": eigen_threshold},
                {"FixProvider": None},
            )
        )

    lattice_policy = dict(constraints.get("Lattice", {}))
    levels = core["RSME"]["Levels"]
    if levels:
        terminal = levels[-1]
        minimum_iou = float(terminal["IoU"]["Minimum"])
        spread = float(terminal["IoU"]["Spread"])
        required_iou = float(lattice_policy.get("MinimumTerminalIoU", 0.80))
        maximum_spread = float(lattice_policy.get("MaximumTerminalPhaseSpread", 0.08))
        if minimum_iou < required_iou:
            diagnostics.append(
                Diagnostic(
                    "AODV3102",
                    "Uniform lattice does not preserve enough semantic structure",
                    f"{asset_id} terminal lattice minimum IoU is {minimum_iou:.4f}; required minimum is {required_iou:.4f}.",
                    str(lattice_policy.get("InsufficiencySeverity", "warning")).lower(),
                    asset_id,
                    asset_path,
                    {"TerminalLevel": terminal, "RequiredMinimumIoU": required_iou},
                    {"Disposition": "Use adaptive refinement or exact geometry for sensitive atoms."},
                )
            )
        if spread > maximum_spread:
            diagnostics.append(
                Diagnostic(
                    "AODV3103",
                    "Lattice result is phase-sensitive",
                    f"{asset_id} terminal lattice IoU spread is {spread:.4f}; permitted spread is {maximum_spread:.4f}.",
                    "note",
                    asset_id,
                    asset_path,
                    {"TerminalLevel": terminal, "MaximumSpread": maximum_spread},
                    {"Disposition": "Treat occupancy as a phase ensemble, not a single-grid fact."},
                )
            )

    if core["TruthSource"]["TruthClass"] != "ExplicitAlpha":
        diagnostics.append(
            Diagnostic(
                "AODV5202",
                "Semantic foreground uses a qualified raster adapter",
                f"{asset_id} has no material alpha channel; geometry was inferred using {core['TruthSource']['Adapter']}.",
                "note",
                asset_id,
                asset_path,
                core["TruthSource"],
                {"TruthSourcePrecedence": ["Vector", "Alpha", "QualifiedRaster"]},
            )
        )

    if baseline_asset is not None:
        current_source = core["Source"]["Identity"]["xxh3_128_hex"]
        baseline_source = baseline_asset["SourceIdentity"]["xxh3_128_hex"]
        if current_source != baseline_source:
            diagnostics.append(
                Diagnostic(
                    "AODV9003",
                    "Source bytes differ from baseline",
                    f"{asset_id} source XXH3-128 differs from baseline; intrinsic geometry is checked separately.",
                    "note",
                    asset_id,
                    asset_path,
                    {"Current": core["Source"]["Identity"], "Baseline": baseline_asset["SourceIdentity"]},
                    {"BaselinePolicy": "Observe source-byte changes; gate translation-normalized intrinsic changes."},
                )
            )
        current_intrinsic = core["Intrinsic"]["TranslationNormalizedFieldIdentity"]["xxh3_128_hex"]
        baseline_intrinsic = baseline_asset["Intrinsic"]["TranslationNormalizedFieldIdentity"]["xxh3_128_hex"]
        if current_intrinsic != baseline_intrinsic:
            diagnostics.append(
                Diagnostic(
                    "AODV9004",
                    "Intrinsic visual geometry differs from baseline",
                    f"{asset_id} translation-normalized semantic field differs from the admitted baseline.",
                    "error",
                    asset_id,
                    asset_path,
                    {
                        "Current": core["Intrinsic"]["TranslationNormalizedFieldIdentity"],
                        "Baseline": baseline_asset["Intrinsic"]["TranslationNormalizedFieldIdentity"],
                    },
                    {"FixProvider": None, "BaselineUpdateRequiresReview": True},
                )
            )

    return diagnostics


def severity_rank(severity: str) -> int:
    return {"none": -1, "note": 0, "info": 0, "warning": 1, "error": 2}.get(
        severity.lower(), 0
    )


def sarif_log(diagnostics: Sequence[Diagnostic]) -> dict[str, Any]:
    used_rule_ids = sorted({diagnostic.diagnostic_id for diagnostic in diagnostics})
    rules = []
    for rule_id in used_rule_ids:
        rule = RULES[rule_id]
        rules.append(
            {
                "id": rule_id,
                "name": rule["Name"],
                "shortDescription": {"text": rule["ShortDescription"]},
                "defaultConfiguration": {"level": rule["DefaultSeverity"]},
                "properties": {"tags": ["visual-analysis", "AMEN.D"]},
            }
        )
    results = []
    for diagnostic in diagnostics:
        level = "note" if diagnostic.severity in {"note", "info"} else diagnostic.severity
        results.append(
            {
                "ruleId": diagnostic.diagnostic_id,
                "level": level,
                "message": {"text": diagnostic.message},
                "locations": [
                    {
                        "physicalLocation": {
                            "artifactLocation": {"uri": diagnostic.asset_path},
                            "region": {"startLine": 1, "startColumn": 1},
                        },
                        "properties": {
                            "assetId": diagnostic.asset_id,
                            **to_builtin(diagnostic.properties),
                        },
                    }
                ],
                "properties": {
                    "assetId": diagnostic.asset_id,
                    "evidence": to_builtin(diagnostic.evidence),
                    **to_builtin(diagnostic.properties),
                },
            }
        )
    return {
        "$schema": "https://json.schemastore.org/sarif-2.1.0.json",
        "version": "2.1.0",
        "runs": [
            {
                "tool": {
                    "driver": {
                        "name": TOOL_NAME,
                        "version": TOOL_VERSION,
                        "informationUri": "https://angelof.dev",
                        "rules": rules,
                    }
                },
                "automationDetails": {"id": "AmenD.VisualAnalysis/"},
                "results": results,
            }
        ],
    }


def prov_graph(
    config_path: str,
    asset_records: Sequence[Mapping[str, Any]],
    output_identities: Mapping[str, Any],
) -> dict[str, Any]:
    graph: list[dict[str, Any]] = [
        {
            "@id": "amend:activity/analyze-config",
            "@type": "prov:Activity",
            "amend:toolVersion": TOOL_VERSION,
            "amend:configPath": config_path,
        }
    ]
    for record in asset_records:
        asset_id = str(record["AssetId"])
        graph.extend(
            [
                {
                    "@id": f"amend:entity/source/{asset_id}",
                    "@type": "prov:Entity",
                    "amend:path": record["Path"],
                    "amend:identity": record["Core"]["Source"]["Identity"],
                },
                {
                    "@id": f"amend:entity/observation/{asset_id}",
                    "@type": "prov:Entity",
                    "amend:identity": record["Core"]["CoreIdentity"],
                    "prov:wasGeneratedBy": {"@id": "amend:activity/analyze-config"},
                    "prov:wasDerivedFrom": {"@id": f"amend:entity/source/{asset_id}"},
                },
            ]
        )
    graph.append(
        {
            "@id": "amend:entity/output-set",
            "@type": "prov:Entity",
            "amend:identities": output_identities,
            "prov:wasGeneratedBy": {"@id": "amend:activity/analyze-config"},
        }
    )
    return {
        "@context": {
            "prov": "http://www.w3.org/ns/prov#",
            "amend": "https://angelof.dev/ns/amend#",
        },
        "@graph": graph,
    }
