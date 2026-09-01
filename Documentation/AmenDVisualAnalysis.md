# AMEN.D Visual Static Analysis

`AmenD.VisualAnalysis.json` declares visual assets, truth-source adapters,
multigrid settings and design constraints. The analyzer keeps intrinsic
geometry separate from frame-relative placement and records all artifact
identities with XXH3-128 plus an XXH3-64 companion.

## Local setup

```text
python -m pip install -r Tools/AmenDVisualAnalysis/Requirements.txt
```

## Analyze

```text
npm run AmenD:Analyze
```

Results are written below `.data/AmenDVisualAnalysis/`, which is already
covered by the repository's `.data/` ignore rule.

## Baseline lifecycle

The baseline gates translation-normalized intrinsic geometry. A whole-group
translation changes source bytes and frame-relative measurements but preserves
the intrinsic field identity. An internal geometry change fails with
`AODV9004` until an explicitly reviewed baseline is created:

```text
npm run AmenD:CreateBaseline
```

Baseline updates are not automatic fixes.

## Diagnostic model

- `AODV1001` reports a visual group that violates a declared frame-centre anchor.
- `AODV2501` suppresses unstable principal-direction claims near a degenerate eigenspace.
- `AODV3102` reports insufficient terminal lattice reconstruction.
- `AODV3103` records material lattice-phase sensitivity.
- `AODV5202` qualifies RGB-derived geometry when vector or alpha truth is unavailable.
- `AODV9003` records source-byte drift separately from intrinsic geometry.
- `AODV9004` gates unreviewed translation-normalized geometry changes.

The GitHub Actions integration runs before the existing Nuxt build. Analyzer
errors block deployment; warnings remain visible without adding ordinary design
friction. SARIF publication is best-effort so a reporting-service failure does
not replace the analyzer's own gate.
