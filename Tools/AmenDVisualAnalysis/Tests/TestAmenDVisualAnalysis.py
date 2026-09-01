from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path

import cv2
import numpy as np

HERE = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(HERE))

import AmenDReference as ar  # noqa: E402


class AmenDVisualAnalysisTests(unittest.TestCase):
    def _write_flat_background_image(self, path: Path, offset_x: int = 0) -> None:
        image = np.zeros((80, 80, 3), dtype=np.uint16)
        image[:, :] = np.array([2300, 800, 300], dtype=np.uint16)  # BGR background
        cv2.rectangle(image, (22 + offset_x, 18), (58 + offset_x, 62), (9000, 48000, 62000), -1)
        cv2.circle(image, (40 + offset_x, 30), 7, (62000, 12000, 3000), -1)
        self.assertTrue(cv2.imwrite(str(path), image))

    def test_xxh3_is_deterministic(self) -> None:
        self.assertEqual(ar.xxh3_json({"B": 2, "A": 1}), ar.xxh3_json({"A": 1, "B": 2}))
        self.assertEqual(ar.xxh3_json({"A": 1})["algorithm_primary"], "XXH3-128")

    def test_rgb_border_adapter_preserves_translation_intrinsic_identity(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            first_path = root / "First.png"
            second_path = root / "Second.png"
            self._write_flat_background_image(first_path, 0)
            self._write_flat_background_image(second_path, 2)
            first, first_truth = ar.load_semantic_field(first_path, {"Mode": "RgbBorderDistance"})
            second, second_truth = ar.load_semantic_field(second_path, {"Mode": "RgbBorderDistance"})
            first_geometry = ar.weighted_geometry(first)
            second_geometry = ar.weighted_geometry(second)
            self.assertEqual(first_truth["Adapter"], "RgbBorderDistance")
            self.assertEqual(second_truth["Adapter"], "RgbBorderDistance")
            self.assertAlmostEqual(
                second_geometry["CentroidPx"]["X"] - first_geometry["CentroidPx"]["X"],
                2.0,
                places=10,
            )
            self.assertEqual(
                ar.translation_normalized_field_identity(first)["xxh3_128_hex"],
                ar.translation_normalized_field_identity(second)["xxh3_128_hex"],
            )

    def test_internal_geometry_change_breaks_intrinsic_identity(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            root = Path(temporary)
            source_path = root / "Source.png"
            changed_path = root / "Changed.png"
            self._write_flat_background_image(source_path, 0)
            image = cv2.imread(str(source_path), cv2.IMREAD_UNCHANGED)
            image[24:30, 24:30] = image[0, 0]
            self.assertTrue(cv2.imwrite(str(changed_path), image))
            source, _ = ar.load_semantic_field(source_path, {"Mode": "RgbBorderDistance"})
            changed, _ = ar.load_semantic_field(changed_path, {"Mode": "RgbBorderDistance"})
            self.assertNotEqual(
                ar.translation_normalized_field_identity(source)["xxh3_128_hex"],
                ar.translation_normalized_field_identity(changed)["xxh3_128_hex"],
            )

    def test_design_constraint_is_separate_from_core_cache_key(self) -> None:
        with tempfile.TemporaryDirectory() as temporary:
            path = Path(temporary) / "Asset.png"
            self._write_flat_background_image(path, 0)
            truth = {"Mode": "RgbBorderDistance"}
            analysis = {"RsmeLevels": [12], "RsmePhasesPx": [[0, 0]]}
            first = ar.core_analysis_key(path, truth, analysis)
            design_contract = {"HorizontalAnchor": {"Mode": "FrameCenter", "TolerancePx": 1}}
            changed_contract = copy.deepcopy(design_contract)
            changed_contract["HorizontalAnchor"]["TolerancePx"] = 2
            second = ar.core_analysis_key(path, truth, analysis)
            self.assertNotEqual(design_contract, changed_contract)
            self.assertEqual(first, second)

    def test_sarif_is_2_1_and_retains_visual_evidence(self) -> None:
        diagnostic = ar.Diagnostic(
            "AODV1001",
            "Off-centre",
            "Synthetic test diagnostic",
            "warning",
            "Synthetic",
            "public/Synthetic.png",
            {"OffsetPx": 2.0},
            {"ImageRegionPx": {"X": 0, "Y": 0, "Width": 10, "Height": 10}},
        )
        sarif = ar.sarif_log([diagnostic])
        self.assertEqual(sarif["version"], "2.1.0")
        result = sarif["runs"][0]["results"][0]
        self.assertEqual(result["ruleId"], "AODV1001")
        self.assertEqual(result["properties"]["evidence"]["OffsetPx"], 2.0)


if __name__ == "__main__":
    unittest.main()
