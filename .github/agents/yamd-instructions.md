Agent instructions: using pre-generated .yml.md.json artifacts

When the agent is asked to analyze or act on a file whose path ends with the `.yml.md` extension, prefer a neighbor JSON artifact with the same name and `.json` appended.

Convention
- Source file: `path/to/file.yml.md`
- Artifact file: `path/to/file.yml.md.json` (exact same dirname and basename, with `.json` appended)

Lookup rule
1. Given an input filename that includes the `.yml.md` extension (for example `Doctrine/Prindiples-And-Protocols.yml.md`), first attempt to open `{that-filename}.json` (i.e., `Doctrine/Prindiples-And-Protocols.yml.md.json`).
2. If the artifact exists, use it. The artifact fields are:
   - `raw_comment` (string)
   - `raw_frontmatter` (string|null)
   - `frontmatter` (object|null)
   - `raw_markdown` (string)
3. Prefer `frontmatter` structured fields (e.g. `frontmatter.title`, `frontmatter.tags`) for metadata-driven tasks (indexing, PR title, labels). Use `raw_*` fields only when needed for fidelity or fallback.

Fallbacks
- If the `.yml.md.json` artifact is missing or outdated, and you have permission to run repo tools in the current environment, run the local parser CLI:
  `/Artifacts/bin/ffwrd yamd.to.json -- <path/to/file.yml.md>`
- If you cannot run the parser locally, request that the artifact be generated (via CI or a repo contributor) or proceed with cautious, best-effort in-prompt parsing of the `.yml.md` text.

Notes
- This repository's CI includes a workflow that will fail a build if artifacts are missing or stale; in CI/PR contexts ensure the artifacts are generated prior to verification (the project prefers artifacts to be produced and committed alongside sources).
- Artifacts are intentionally stored next to sources to make discovery trivial for agents operating in a repo workspace.