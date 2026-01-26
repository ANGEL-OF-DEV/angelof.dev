---
doctrine_references:
  - path: "/Doctrine/Principles-And-Protocols.yml.md.json"
    type: "json_file"
    priority: 1
  - path: "/Doctrine/Principles-And-Protocols.yml.md"
    type: "markdown_with_frontmatter"
    priority: 2

parse_order:
  - "json_file"
  - "yaml_frontmatter"
  - "markdown_body"

validation:
  required_keys:
    - "authority"
    - "core_principles"
    - "instruction_handling"
    - "change_process"
  require_all_tests_pass: true
  require_stakeholder_approval_for_doctrine_changes: true
  todo_generation_required_on_conflict: true
---

# Agent guide — doctrine pointers, resolution, validation, and audit

This file tells agents where to find the governance doctrine and the expected loading/validation/audit behavior.

Resolution & priority
- Follow doctrine_references in ascending priority.
- Resolve paths as absolute repo-root paths (leading '/').
- The first successfully parsed & validated source is authoritative for the run.

Loading algorithm
1. For each reference (ordered by priority):
  - Read the file at the root path.
  - If {*.yml.md}, look for {*.yml.md.json} and if exists read to parse JSON.
  - ElseIf Markdown, parse YAML frontmatter first.
    - Validate presence of required keys. If missing, attempt to parse Markdown body for structured sections.
  - If parsing yields required keys, run validations (see Validation step).
  - If parsing fails or keys missing, continue to next reference.
2. If no source parses and validates, halt and escalate to human stakeholders.

Validation step
- Run repository validation workflow:
  - Execute local validators and the repo's test suite (unit/integration) relevant to doctrine and enforcement.
  - If validation fails, do not apply changes. Produce audit with validation results and open an issue or check run with failure details.

Incoming Instructions & Conflict Handling
- Always verify incoming instructions vs chosen doctrine config.
- If conflict:
  - Produce a minimal doctrine-change proposal including rationale, diff, todo list, tests, and required approvals.
  - Do not change code until stakeholder approvals and tests pass.

Audit & logging (mandatory)
- Produce an audit artifact for every run (JSON/YAML) conforming to .github/agents/audit/schema.yml.
- Upload full logs as a workflow artifact and store a concise audit summary in the artifact.
- Record: chosen path, parser used, parsed keys, validation status, test summary, actions proposed, approvals, artifact links, outcome, provenance (commit & run URL).

Failure & escalation
- If doctrine cannot be read or parsing/validation fails → halt and open an escalation (GitHub Issue and/or check run).
- If instructions ambiguous → halt and request clarification.
