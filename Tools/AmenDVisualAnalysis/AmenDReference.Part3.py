from __future__ import annotations

def analyze_core(
    source_path: str | Path,
    truth_source: Mapping[str, Any],
    analysis_config: Mapping[str, Any],
    *,
    logical_path: str | None = None,
) -> dict[str, Any]:
    semantic_field, truth_evidence = load_semantic_field(source_path, truth_source)
    geometry = weighted_geometry(semantic_field)
    levels = [int(value) for value in analysis_config.get("RsmeLevels", [24, 48, 96])]
    phases = [
        (float(item[0]), float(item[1]))
        for item in analysis_config.get("RsmePhasesPx", [[0, 0], [1, 0], [0, 1], [1, 1]])
    ]
    result = {
        "Schema": "AmenD.VisualAnalysis.Core/1.9.0",
        "Tool": {"Name": TOOL_NAME, "Version": TOOL_VERSION},
        "Source": {"Path": logical_path or str(source_path), "Identity": xxh3_file(source_path)},
        "TruthSource": truth_evidence,
        "Geometry": geometry,
        "Intrinsic": {
            "TranslationNormalizedFieldIdentity": translation_normalized_field_identity(semantic_field),
            "WeightedAreaPx2": geometry["WeightedAreaPx2"],
            "BoundingBoxDimensionsPx": {
                "Width": geometry["BoundingBoxIndexPx"]["Width"] if geometry["BoundingBoxIndexPx"] else 0,
                "Height": geometry["BoundingBoxIndexPx"]["Height"] if geometry["BoundingBoxIndexPx"] else 0,
            },
            "CovariancePx2": geometry["CovariancePx2"],
            "EigenvaluesPx2": geometry["EigenvaluesPx2"],
            "EigenGapRatio": geometry["EigenGapRatio"],
            "UnnormalizedSecondMomentPx4": geometry["UnnormalizedSecondMomentPx4"],
        },
        "FrameAlignment": frame_alignment(semantic_field, geometry),
        "RSME": rsme(
            semantic_field,
            levels,
            phases,
            threshold=float(analysis_config.get("OccupancyThreshold", 0.5)),
            aspect_preserving=bool(analysis_config.get("AspectPreservingLattice", True)),
        ),
    }
    result["CoreIdentity"] = xxh3_json(result)
    return result


def core_analysis_key(
    source_path: str | Path,
    truth_source: Mapping[str, Any],
    analysis_config: Mapping[str, Any],
) -> dict[str, Any]:
    payload = {
        "ToolVersion": TOOL_VERSION,
        "SourceIdentity": xxh3_file(source_path),
        "TruthSource": truth_source,
        "AnalysisConfig": analysis_config,
    }
    return xxh3_json(payload)


# ---------------------------------------------------------------------------
# Baselines and diagnostics
# ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Diagnostic:
    diagnostic_id: str
    title: str
    message: str
    severity: str
    asset_id: str
    asset_path: str
    evidence: Mapping[str, Any]
    properties: Mapping[str, Any]

    def as_json(self) -> dict[str, Any]:
        return {
            "DiagnosticId": self.diagnostic_id,
            "Title": self.title,
            "Message": self.message,
            "Severity": self.severity,
            "AssetId": self.asset_id,
            "AssetPath": self.asset_path,
            "Evidence": to_builtin(self.evidence),
            "Properties": to_builtin(self.properties),
        }


RULES: dict[str, dict[str, Any]] = {
    "AODV1001": {
        "Name": "EmblemAxisDiffersFromFrameCenter",
        "ShortDescription": "The declared visual group is horizontally displaced from its frame anchor.",
        "DefaultSeverity": "warning",
    },
    "AODV2501": {
        "Name": "PrincipalAxisIllConditioned",
        "ShortDescription": "A unique principal direction is not reliable in a nearly isotropic eigenspace.",
        "DefaultSeverity": "note",
    },
    "AODV3102": {
        "Name": "UniformLatticeInsufficient",
        "ShortDescription": "The terminal uniform lattice does not reconstruct the semantic field reliably.",
        "DefaultSeverity": "warning",
    },
    "AODV3103": {
        "Name": "LatticePhaseSensitive",
        "ShortDescription": "Lattice reconstruction changes materially across nearby phases.",
        "DefaultSeverity": "note",
    },
    "AODV5202": {
        "Name": "RasterTruthSourceQualified",
        "ShortDescription": "Foreground geometry is inferred from RGB because explicit alpha or vector truth is absent.",
        "DefaultSeverity": "note",
    },
    "AODV9003": {
        "Name": "SourceBytesDifferFromBaseline",
        "ShortDescription": "Source bytes differ from the recorded baseline while intrinsic geometry may still agree.",
        "DefaultSeverity": "note",
    },
    "AODV9004": {
        "Name": "IntrinsicGeometryDiffersFromBaseline",
        "ShortDescription": "Translation-normalized semantic geometry differs from the admitted baseline.",
        "DefaultSeverity": "error",
    },
}


def create_baseline_asset(asset_id: str, asset_path: str, core: Mapping[str, Any]) -> dict[str, Any]:
    return {
        "AssetId": asset_id,
        "AssetPath": asset_path,
        "SourceIdentity": core["Source"]["Identity"],
        "TruthSourceAdapter": core["TruthSource"]["Adapter"],
        "Intrinsic": core["Intrinsic"],
        "FrameAlignment": core["FrameAlignment"],
        "CoreIdentity": core["CoreIdentity"],
    }


