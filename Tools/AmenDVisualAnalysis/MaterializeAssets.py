#!/usr/bin/env python3
"""Materialize exact binary visual assets from reviewable AMEN.D payload parts."""
from __future__ import annotations

import argparse
import base64
import bz2
import json
from pathlib import Path
from typing import Any, Mapping

import cv2
import numpy as np
import AmenDReference as ar


def _xxh3(path: Path) -> dict[str, str]:
    return ar.xxh3_bytes(path.read_bytes())


def _matches(identity: Mapping[str, Any], expected: Mapping[str, Any]) -> bool:
    return str(identity.get("xxh3_128_hex")) == str(expected.get("xxh3_128_hex"))


def ensure_embedded_assets(config_path: str | Path) -> list[dict[str, Any]]:
    config_path = Path(config_path).resolve()
    root = config_path.parent
    config = json.loads(config_path.read_text(encoding="utf-8"))
    records: list[dict[str, Any]] = []

    for asset in config.get("Assets", []):
        payload = asset.get("EmbeddedPayload")
        if not payload:
            continue
        target = root / str(asset["Path"])
        expected = dict(payload["ExpectedSourceIdentity"])
        if target.exists():
            observed = _xxh3(target)
            if not _matches(observed, expected):
                raise RuntimeError(
                    f"Refusing to overwrite {target}: existing XXH3-128 {observed['xxh3_128_hex']} "
                    f"does not match expected {expected['xxh3_128_hex']}"
                )
            records.append({
                "AssetId": asset["AssetId"],
                "Path": str(target),
                "Action": "VerifiedExisting",
                "Identity": observed,
            })
            continue

        encoding = str(payload.get("Encoding"))
        if encoding != "BZip2Base64Parts":
            raise ValueError(f"Unsupported EmbeddedPayload.Encoding: {encoding}")
        part_paths = [root / str(value) for value in payload.get("PartPaths", [])]
        if not part_paths:
            raise ValueError(f"Embedded payload for {asset['AssetId']} has no PartPaths")
        missing = [str(path) for path in part_paths if not path.is_file()]
        if missing:
            raise FileNotFoundError(f"Missing embedded payload parts: {missing}")

        encoded = "".join(path.read_text(encoding="ascii").strip() for path in part_paths)
        compressed = base64.b64decode(encoded, validate=True)
        raw = bz2.decompress(compressed)
        shape = tuple(int(value) for value in payload["Shape"])
        dtype = np.dtype(str(payload["DType"]))
        expected_bytes = int(np.prod(shape)) * dtype.itemsize
        if len(raw) != expected_bytes:
            raise RuntimeError(
                f"Decoded payload length {len(raw)} does not match expected {expected_bytes}"
            )
        image = np.frombuffer(raw, dtype=dtype).reshape(shape)
        if str(payload.get("PixelOrder")) != "BGR":
            raise ValueError("Only BGR payloads are supported by this materializer")

        target.parent.mkdir(parents=True, exist_ok=True)
        compression = int(payload.get("PngCompression", 9))
        if not cv2.imwrite(str(target), image, [cv2.IMWRITE_PNG_COMPRESSION, compression]):
            raise RuntimeError(f"OpenCV failed to materialize {target}")
        observed = _xxh3(target)
        if not _matches(observed, expected):
            target.unlink(missing_ok=True)
            raise RuntimeError(
                f"Materialized XXH3-128 {observed['xxh3_128_hex']} does not match expected "
                f"{expected['xxh3_128_hex']}"
            )
        records.append({
            "AssetId": asset["AssetId"],
            "Path": str(target),
            "Action": "Materialized",
            "Identity": observed,
        })
    return records


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--Config", type=Path, default=Path("AmenD.VisualAnalysis.json"))
    args = parser.parse_args()
    try:
        print(json.dumps({
            "Schema": "AmenD.MaterializedAssets/1.9.0",
            "Assets": ensure_embedded_assets(args.Config),
        }, indent=2))
        return 0
    except (FileNotFoundError, ValueError, RuntimeError, json.JSONDecodeError) as error:
        print(f"AmenD.VisualAnalysis.json:1:1: error AODV0002: {error}")
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
