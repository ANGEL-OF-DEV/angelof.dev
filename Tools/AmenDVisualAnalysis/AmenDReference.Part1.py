#!/usr/bin/env python3
"""AMEN.D visual static-analysis host library.

This module is the smallest build-host projection of the AMEN.D 1.8.0
reference architecture. It keeps artifact identity in the XXH3 family,
qualifies the semantic truth source, separates intrinsic measurements from
frame-relative constraints, emits SARIF-compatible diagnostics, and supports
content-addressed incremental reuse.
"""
from __future__ import annotations

import ctypes
import ctypes.util
import json
import math
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Iterable, Mapping, Sequence

import cv2
import numpy as np

TOOL_NAME = "AMEN.D Visual Static Analysis"
TOOL_VERSION = "1.9.0"


# ---------------------------------------------------------------------------
# XXH3 identity
# ---------------------------------------------------------------------------

try:
    import xxhash as _xxhash_module  # type: ignore[import-not-found]
except ImportError:  # pragma: no cover - exercised when package is unavailable.
    _xxhash_module = None


class _XXH128Hash(ctypes.Structure):
    _fields_ = [("low64", ctypes.c_uint64), ("high64", ctypes.c_uint64)]


_XXH_LIBRARY: ctypes.CDLL | None = None


def _load_xxh_library() -> ctypes.CDLL:
    global _XXH_LIBRARY
    if _XXH_LIBRARY is not None:
        return _XXH_LIBRARY
    library_name = ctypes.util.find_library("xxhash")
    if not library_name:
        raise RuntimeError(
            "XXH3 backend unavailable. Install the Python 'xxhash' package or libxxhash."
        )
    library = ctypes.CDLL(library_name)
    library.XXH3_64bits.argtypes = [ctypes.c_void_p, ctypes.c_size_t]
    library.XXH3_64bits.restype = ctypes.c_uint64
    library.XXH3_128bits.argtypes = [ctypes.c_void_p, ctypes.c_size_t]
    library.XXH3_128bits.restype = _XXH128Hash
    _XXH_LIBRARY = library
    return library


def xxh3_bytes(data: bytes) -> dict[str, str]:
    """Return primary XXH3-128 and compact XXH3-64 identities."""
    if _xxhash_module is not None:
        return {
            "algorithm_primary": "XXH3-128",
            "xxh3_128_hex": _xxhash_module.xxh3_128_hexdigest(data),
            "xxh3_64_hex": _xxhash_module.xxh3_64_hexdigest(data),
        }
    library = _load_xxh_library()
    raw = data if data else b"\x00"
    buffer = ctypes.create_string_buffer(raw)
    pointer = ctypes.cast(buffer, ctypes.c_void_p)
    h64 = int(library.XXH3_64bits(pointer, len(data)))
    h128 = library.XXH3_128bits(pointer, len(data))
    return {
        "algorithm_primary": "XXH3-128",
        "xxh3_128_hex": f"{int(h128.high64):016x}{int(h128.low64):016x}",
        "xxh3_64_hex": f"{h64:016x}",
    }


def to_builtin(value: Any) -> Any:
    if isinstance(value, Mapping):
        return {str(key): to_builtin(item) for key, item in value.items()}
    if isinstance(value, (list, tuple, set)):
        return [to_builtin(item) for item in value]
    if isinstance(value, np.ndarray):
        return value.tolist()
    if isinstance(value, np.integer):
        return int(value)
    if isinstance(value, np.floating):
        number = float(value)
        return number if math.isfinite(number) else None
    if isinstance(value, float):
        return value if math.isfinite(value) else None
    return value


def canonical_json_bytes(value: Any) -> bytes:
    return json.dumps(
        to_builtin(value), ensure_ascii=False, sort_keys=True, separators=(",", ":")
    ).encode("utf-8")


def xxh3_json(value: Any) -> dict[str, str]:
    return xxh3_bytes(canonical_json_bytes(value))


def xxh3_file(path: str | Path) -> dict[str, str]:
    return xxh3_bytes(Path(path).read_bytes())


def read_json(path: str | Path) -> Any:
    return json.loads(Path(path).read_text(encoding="utf-8"))


def write_json(path: str | Path, value: Any) -> None:
    destination = Path(path)
    destination.parent.mkdir(parents=True, exist_ok=True)
    destination.write_text(
        json.dumps(to_builtin(value), ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


# ---------------------------------------------------------------------------
# Truth-source adapters
# ---------------------------------------------------------------------------


def _normalize_image(image: np.ndarray) -> np.ndarray:
    if np.issubdtype(image.dtype, np.integer):
        maximum = float(np.iinfo(image.dtype).max)
    else:
        maximum = float(np.nanmax(image)) if image.size else 1.0
        maximum = maximum if maximum > 1.0 else 1.0
    return np.asarray(image, dtype=np.float64) / maximum


def _border_pixels(rgb: np.ndarray, width_px: int) -> np.ndarray:
    width_px = max(1, min(width_px, rgb.shape[0] // 2, rgb.shape[1] // 2))
    return np.concatenate(
        [
            rgb[:width_px, :, :].reshape(-1, 3),
            rgb[-width_px:, :, :].reshape(-1, 3),
            rgb[:, :width_px, :].reshape(-1, 3),
            rgb[:, -width_px:, :].reshape(-1, 3),
        ],
        axis=0,
    )


def load_semantic_field(
    path: str | Path,
    truth_source: Mapping[str, Any] | None = None,
) -> tuple[np.ndarray, dict[str, Any]]:
    """Load a semantic foreground field while preserving high-bit-depth input.

    Precedence is explicit alpha, then border-background RGB distance, then a
    qualified luminance fallback. The chosen adapter and its thresholds are
    returned as evidence; they are never implicit.
    """
    source_path = Path(path)
    image = cv2.imread(str(source_path), cv2.IMREAD_UNCHANGED)
    if image is None:
        raise FileNotFoundError(f"Unable to decode image: {source_path}")

    config = dict(truth_source or {})
    requested = str(config.get("Mode", "Auto"))
    height, width = image.shape[:2]
    source_depth = str(image.dtype)

    has_alpha = image.ndim == 3 and image.shape[2] == 4
    alpha_channel: np.ndarray | None = None
    if has_alpha:
        alpha_channel = _normalize_image(image[:, :, 3])
        alpha_is_material = bool(np.any(alpha_channel < 1.0 - 1e-12))
    else:
        alpha_is_material = False

    if requested in {"Auto", "Alpha"} and alpha_channel is not None and (
        alpha_is_material or requested == "Alpha"
    ):
        return alpha_channel, {
            "Adapter": "Alpha",
            "TruthClass": "ExplicitAlpha",
            "RequestedMode": requested,
            "SourceDType": source_depth,
            "CanvasPx": {"Width": width, "Height": height},
            "Parameters": {},
        }

    if requested in {"Auto", "RgbBorderDistance"} and image.ndim == 3 and image.shape[2] >= 3:
        rgb = _normalize_image(image[:, :, :3][:, :, ::-1])
        border_ratio = float(config.get("BorderWidthRatio", 0.02))
        border_width = max(1, int(round(min(height, width) * border_ratio)))
        border = _border_pixels(rgb, border_width)
        background = np.median(border, axis=0)
        border_distances = np.linalg.norm(border - background, axis=1)
        quantile = float(config.get("BorderNoiseQuantile", 0.995))
        noise_distance = float(np.quantile(border_distances, quantile))
        minimum_distance = float(config.get("MinimumForegroundDistance", 0.04))
        noise_multiplier = float(config.get("BorderNoiseMultiplier", 2.2))
        low = max(minimum_distance, noise_distance * noise_multiplier)
        transition_width = float(config.get("TransitionWidth", 0.04))
        if transition_width <= 0:
            raise ValueError("TruthSource.TransitionWidth must be positive")
        high = low + transition_width
        distances = np.linalg.norm(rgb - background, axis=2)
        normalized = np.clip((distances - low) / (high - low), 0.0, 1.0)
        field = normalized * normalized * (3.0 - 2.0 * normalized)
        return field, {
            "Adapter": "RgbBorderDistance",
            "TruthClass": "QualifiedRasterInference",
            "RequestedMode": requested,
            "SourceDType": source_depth,
            "CanvasPx": {"Width": width, "Height": height},
            "Parameters": {
                "BorderWidthPx": border_width,
                "BackgroundRgbNormalized": [float(value) for value in background],
                "BorderNoiseQuantile": quantile,
                "ObservedBorderNoiseDistance": noise_distance,
                "LowForegroundDistance": low,
                "HighForegroundDistance": high,
            },
        }

    if requested not in {"Auto", "LuminanceFallback"}:
        raise ValueError(f"Requested truth-source mode is unavailable: {requested}")
    if image.ndim == 2:
        gray = _normalize_image(image)
    else:
        normalized = _normalize_image(image[:, :, :3])
        gray = cv2.cvtColor(normalized.astype(np.float32), cv2.COLOR_BGR2GRAY).astype(np.float64)
    return gray, {
        "Adapter": "LuminanceFallback",
        "TruthClass": "QualifiedFallback",
        "RequestedMode": requested,
        "SourceDType": source_depth,
        "CanvasPx": {"Width": width, "Height": height},
        "Parameters": {},
    }


# ---------------------------------------------------------------------------
# Geometry, symmetry, scale and lattice analysis
# ---------------------------------------------------------------------------


