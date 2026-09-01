from __future__ import annotations

def weighted_geometry(field: np.ndarray) -> dict[str, Any]:
    values = np.asarray(field, dtype=np.float64)
    height, width = values.shape
    mass = float(values.sum())
    if mass <= 0:
        return {
            "WeightedAreaPx2": 0.0,
            "CentroidPx": None,
            "BoundingBoxIndexPx": None,
            "CovariancePx2": [[0.0, 0.0], [0.0, 0.0]],
            "EigenvaluesPx2": [0.0, 0.0],
            "EigenGapRatio": 0.0,
            "UnnormalizedSecondMomentPx4": 0.0,
        }

    yy, xx = np.indices((height, width), dtype=np.float64)
    centroid_x = float((values * xx).sum() / mass)
    centroid_y = float((values * yy).sum() / mass)
    delta_x = xx - centroid_x
    delta_y = yy - centroid_y
    variance_x = float((values * delta_x * delta_x).sum() / mass)
    variance_y = float((values * delta_y * delta_y).sum() / mass)
    covariance_xy = float((values * delta_x * delta_y).sum() / mass)
    covariance = np.array(
        [[variance_x, covariance_xy], [covariance_xy, variance_y]], dtype=np.float64
    )
    eigenvalues, eigenvectors = np.linalg.eigh(covariance)
    order = np.argsort(eigenvalues)[::-1]
    eigenvalues = eigenvalues[order]
    principal = eigenvectors[:, order[0]]
    angle = float(math.degrees(math.atan2(principal[1], principal[0])) % 180.0)
    nonzero = np.argwhere(values >= 0.5)
    if nonzero.size:
        y_min, x_min = nonzero.min(axis=0)
        y_max, x_max = nonzero.max(axis=0)
        bounding_box = {
            "XMin": int(x_min),
            "XMax": int(x_max),
            "YMin": int(y_min),
            "YMax": int(y_max),
            "Width": int(x_max - x_min + 1),
            "Height": int(y_max - y_min + 1),
        }
    else:
        bounding_box = None
    gap = float(eigenvalues[0] - eigenvalues[1])
    gap_ratio = float(gap / max(abs(float(eigenvalues[0])), 1e-30))
    return {
        "WeightedAreaPx2": mass,
        "CentroidPx": {"X": centroid_x, "Y": centroid_y},
        "BoundingBoxIndexPx": bounding_box,
        "CovariancePx2": covariance.tolist(),
        "EigenvaluesPx2": [float(eigenvalues[0]), float(eigenvalues[1])],
        "PrincipalAxisAngleDegreesModulo180": angle,
        "EigenGapPx2": gap,
        "EigenGapRatio": gap_ratio,
        "UnnormalizedSecondMomentPx4": float(mass * (variance_x + variance_y)),
        "FrameOffsetNormalized": {
            "X": (centroid_x - (width - 1) / 2.0) / width,
            "Y": (centroid_y - (height - 1) / 2.0) / height,
        },
    }


def binary_overlap(first: np.ndarray, second: np.ndarray, threshold: float = 0.5) -> dict[str, Any]:
    left = np.asarray(first) >= threshold
    right = np.asarray(second) >= threshold
    intersection = int(np.logical_and(left, right).sum())
    union = int(np.logical_or(left, right).sum())
    total = int(left.sum()) + int(right.sum())
    return {
        "IntersectionPx": intersection,
        "UnionPx": union,
        "IoU": float(intersection / union) if union else 1.0,
        "Dice": float(2 * intersection / total) if total else 1.0,
        "XorPx": int(np.logical_xor(left, right).sum()),
    }


def reflect_about_vertical_axis(field: np.ndarray, axis_x: float) -> np.ndarray:
    height, width = field.shape
    result = np.zeros_like(field)
    for source_x in range(width):
        target_x = int(round(2.0 * axis_x - source_x))
        if 0 <= target_x < width:
            result[:, target_x] = np.maximum(result[:, target_x], field[:, source_x])
    return result


def frame_alignment(field: np.ndarray, geometry: Mapping[str, Any]) -> dict[str, Any]:
    height, width = field.shape
    frame_center_x = (width - 1) / 2.0
    centroid = geometry.get("CentroidPx")
    bounding_box = geometry.get("BoundingBoxIndexPx")
    if centroid is None or bounding_box is None:
        return {"Status": "Empty", "FrameCenterXPx": frame_center_x}

    bbox_midpoint_x = (float(bounding_box["XMin"]) + float(bounding_box["XMax"])) / 2.0
    left_margin = int(bounding_box["XMin"])
    right_margin = int(width - 1 - int(bounding_box["XMax"]))
    search_radius = max(4, int(round(width * 0.03)))
    candidates: list[dict[str, float]] = []
    for half_step in range(-2 * search_radius, 2 * search_radius + 1):
        axis = frame_center_x + half_step / 2.0
        reflected = reflect_about_vertical_axis(field, axis)
        score = binary_overlap(field, reflected)["IoU"]
        candidates.append({"AxisXPx": axis, "ReflectionIoU": float(score)})
    best = max(candidates, key=lambda item: (item["ReflectionIoU"], -abs(item["AxisXPx"] - frame_center_x)))
    evidence_axes = [float(centroid["X"]), bbox_midpoint_x, float(best["AxisXPx"])]
    representative_axis = float(np.median(evidence_axes))
    return {
        "Status": "Measured",
        "FrameCenterXPx": frame_center_x,
        "CentroidAxisXPx": float(centroid["X"]),
        "CentroidOffsetPx": float(centroid["X"] - frame_center_x),
        "BoundingBoxMidpointXPx": bbox_midpoint_x,
        "BoundingBoxMidpointOffsetPx": bbox_midpoint_x - frame_center_x,
        "BestReflectionAxisXPx": float(best["AxisXPx"]),
        "BestReflectionAxisOffsetPx": float(best["AxisXPx"] - frame_center_x),
        "BestReflectionIoU": float(best["ReflectionIoU"]),
        "RepresentativeAxisXPx": representative_axis,
        "RepresentativeAxisOffsetPx": representative_axis - frame_center_x,
        "SemanticMarginsPx": {
            "Left": left_margin,
            "Right": right_margin,
            "Top": int(bounding_box["YMin"]),
            "Bottom": int(height - 1 - int(bounding_box["YMax"])),
        },
        "HorizontalMarginImbalancePx": abs(left_margin - right_margin),
        "EvidenceAxesXPx": evidence_axes,
    }


def translation_normalized_field_identity(field: np.ndarray) -> dict[str, Any]:
    binary = np.asarray(field) >= 0.5
    nonzero = np.argwhere(binary)
    if not nonzero.size:
        return xxh3_json({"Shape": [0, 0], "FieldQ12": []})
    y_min, x_min = nonzero.min(axis=0)
    y_max, x_max = nonzero.max(axis=0)
    crop = np.asarray(field[y_min : y_max + 1, x_min : x_max + 1], dtype=np.float64)
    quantized = np.rint(np.clip(crop, 0.0, 1.0) * 4095.0).astype("<u2", copy=False)
    header = canonical_json_bytes({"Shape": list(crop.shape), "Quantization": "Q12"})
    return xxh3_bytes(header + b"\x00" + quantized.tobytes(order="C"))


def _cell_partition(size: int, cells: int, phase: float) -> list[tuple[int, int]]:
    if cells <= 0:
        raise ValueError("Cell count must be positive")
    edges = np.linspace(0.0, float(size), cells + 1) + float(phase)
    edges[0] = 0.0
    edges[-1] = float(size)
    integers = np.clip(np.rint(edges).astype(int), 0, size)
    integers = np.maximum.accumulate(integers)
    return [(int(integers[index]), int(integers[index + 1])) for index in range(cells)]


def lattice_approximation(
    field: np.ndarray,
    cells_x: int,
    cells_y: int,
    *,
    threshold: float = 0.5,
    phase_x: float = 0.0,
    phase_y: float = 0.0,
) -> dict[str, Any]:
    values = np.asarray(field, dtype=np.float64)
    height, width = values.shape
    x_ranges = _cell_partition(width, int(cells_x), phase_x)
    y_ranges = _cell_partition(height, int(cells_y), phase_y)
    x0 = np.array([item[0] for item in x_ranges], dtype=np.int64)
    x1 = np.array([item[1] for item in x_ranges], dtype=np.int64)
    y0 = np.array([item[0] for item in y_ranges], dtype=np.int64)
    y1 = np.array([item[1] for item in y_ranges], dtype=np.int64)
    integral = np.pad(values.cumsum(axis=0).cumsum(axis=1), ((1, 0), (1, 0)))
    sums = (
        integral[y1[:, None], x1[None, :]]
        - integral[y0[:, None], x1[None, :]]
        - integral[y1[:, None], x0[None, :]]
        + integral[y0[:, None], x0[None, :]]
    )
    areas = (y1 - y0)[:, None] * (x1 - x0)[None, :]
    fractions = np.divide(sums, areas, out=np.zeros_like(sums), where=areas > 0)
    occupied = fractions >= threshold
    x_index = np.searchsorted(x1, np.arange(width), side="right")
    y_index = np.searchsorted(y1, np.arange(height), side="right")
    x_index = np.clip(x_index, 0, len(x_ranges) - 1)
    y_index = np.clip(y_index, 0, len(y_ranges) - 1)
    reconstructed = occupied[y_index[:, None], x_index[None, :]]
    overlap = binary_overlap(reconstructed, values, threshold)
    signature = xxh3_json(
        {
            "CellsX": cells_x,
            "CellsY": cells_y,
            "PhaseX": phase_x,
            "PhaseY": phase_y,
            "FractionsQ12": np.rint(fractions.ravel() * 4095.0).astype(np.int64).tolist(),
        }
    )
    return {
        "Cells": {"X": int(cells_x), "Y": int(cells_y), "Total": int(cells_x * cells_y)},
        "Points": {
            "X": int(cells_x + 1),
            "Y": int(cells_y + 1),
            "Total": int((cells_x + 1) * (cells_y + 1)),
        },
        "PhasePx": {"X": float(phase_x), "Y": float(phase_y)},
        "MajorityReconstruction": overlap,
        "OccupiedCellCount": int(occupied.sum()),
        "FractionalAreaPx2": float((fractions * areas).sum()),
        "TruthWeightedAreaPx2": float(values.sum()),
        "OccupancySignature": signature,
    }


def rsme(
    field: np.ndarray,
    levels: Sequence[int],
    phases: Sequence[tuple[float, float]],
    *,
    threshold: float = 0.5,
    aspect_preserving: bool = True,
) -> dict[str, Any]:
    height, width = field.shape
    short_axis = min(height, width)
    records: list[dict[str, Any]] = []
    for short_cells in levels:
        if aspect_preserving:
            cells_x = max(1, int(round(short_cells * width / short_axis)))
            cells_y = max(1, int(round(short_cells * height / short_axis)))
        else:
            cells_x = cells_y = int(short_cells)
        samples = [
            lattice_approximation(
                field,
                cells_x,
                cells_y,
                threshold=threshold,
                phase_x=phase_x,
                phase_y=phase_y,
            )
            for phase_x, phase_y in phases
        ]
        ious = [float(sample["MajorityReconstruction"]["IoU"]) for sample in samples]
        signatures = {
            str(sample["OccupancySignature"]["xxh3_128_hex"]) for sample in samples
        }
        records.append(
            {
                "ShortAxisCells": int(short_cells),
                "Cells": samples[0]["Cells"],
                "PhaseCount": len(samples),
                "IoU": {
                    "Minimum": min(ious),
                    "Median": float(np.median(ious)),
                    "Maximum": max(ious),
                    "Spread": max(ious) - min(ious),
                },
                "DistinctOccupancySignatures": len(signatures),
                "Samples": samples,
            }
        )
    return {
        "Levels": records,
        "PhaseCount": len(phases),
        "Threshold": threshold,
        "AspectPreserving": aspect_preserving,
    }


