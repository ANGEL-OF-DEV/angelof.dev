---
# Condensed, machine-friendly playbook summary for GitHub Copilot Agent consumption
version: 1.0
title: "Angel of Dev — Playbook (essence)"
summary: Canonical, example-first operational playbook: small, testable, reviewable runbooks and recipes that implement doctrine. Optimized for human and agent consumption.
purpose: Provide executable guidance (recipes, patterns, how-tos, checklists) that translate doctrine into predictable, auditable actions.

audience:
- contributors
- reviewers
- agents

scope:
includes:
- recipes: short, copy-pasteable examples (one concept per file)
- patterns: architecture and code patterns with rationale
- howtos: stepwise procedures for common tasks
- checklists: pre-merge and release checklists
- examples: minimal runnable demos
- meta.yaml: machine-readable index
excludes:
- long-form reference docs (reside in Docs/)
- speculative experiments (go to Workshop/)
- meeting notes / unreviewed content

layout:
root: Playbook/
recommended_files:
- Recipes/**
- Patterns/**
- How2s/**
- CheckLists/**
- Examples/**
- meta.yml
- manifest.gen.json (generated)

authoring_rules:
- "One concept per file; keep files small and focused."
- "Frontmatter required: title, summary, owner, tags, last_reviewed, status (draft|stable|deprecated)."
- "Example-first: start with a minimal runnable example/command."
- "Rationale: 2–4 sentences after example."
- "When-to-use / when-not-to-use: explicit boundaries."
- "Validation: include tests/verification commands when applicable."
- "State tool versions if guidance depends on them."

agent_setup:
  build_command: "dotnet build Sources/ffrwd/ffrwd.csproj"
  init_command: "ffrwd agent init <model>"
  wrapper_note: "Use ffrwd.cmd on Windows or ./ffrwd on Unix from repo root."
  intent: "Build local tools if missing; then init worktree."
  next_step: "Change directory to the printed worktree path."
agent_tasking:
  command_source: "Doctrine/Tools/ffrwd.yml.md"
  claim_timeout: "4h"
  abandoned_timeout: "4h"
  doctrine: "Doctrine/Agents.yml.md"

tone_and_style:
- "Actionable, concise, minimal fluff."
- "Prefer numbered steps and code blocks."
- "State explicit assumptions (env, tools, preconditions)."
- "Every sentence must add value."

governance_and_maintenance:
review_cadence: "every 1 month (update last_reviewed)"
change_protocol: "delta + rationale + impact; document in change_logs"
deprecation: "mark status: deprecated; link migration steps"
enforcement:
- "CI validates frontmatter and example presence"
- "pre-commit hooks enforce formatting"

operational_flow: |
1. TASKING: If no explicit instruction, claim next task via CLI.
2. INTAKE: Accept explicit instruction; if ambiguous, halt and request clarification.
3. DOCTRINE VERIFICATION: Check against Doctrine/; if conflict, propose minimal doctrine change with rationale and await approval.
4. TOOLS CHECK: Review tool cards and issues for used tools.
5. PLANNING: Produce ordered todo list mapping tasks to doctrine sections and required validators/tests/docs.
6. EXECUTION: Run tasks in order; update validators, schemas, tests, docs. Do not reorder.
7. VALIDATION: Run local validators/tests; capture results. On failure: stop, fix, re-run.
8. REVIEW: Verify alignment, check side-effects, propose corrections.
9. DOCUMENTATION: Record what/why/how; update change logs and approvals.
10. APPROVAL: Obtain stakeholder approvals for doctrine changes before merge.
11. COMMIT: Commit only after validations pass, docs complete, approvals recorded.
12. MONITORING: Post-change monitoring, schedule audits for elevated risk, capture lessons learned.

enforcement_rules:
- "Doctrine overrides other instructions."
- "No ambiguous execution."
- "No unvalidated commits."
- "No undocumented changes."
 - "All files under Doctrine/** must include frontmatter."
 - "All *.yml.md files anywhere in the repo must include frontmatter."
response_format:
  start_with_numbered_list: true
  numbered_lists_only: true
  list_item_max_length: 80

machine_friendliness:
index: meta.yml (YAML frontmatter index)
api_manifest: manifest.json (generated from frontmatter)
tags: [security, ci, testing, performance, release, onboarding]
expectations:
- "Files parseable by agents: required frontmatter + example block + validation snippet."
- "meta.yml must include title, tags, owner, last_reviewed."

non_goals:
- "Not a replacement for full reference documentation."
- "Not for speculative experiments or meeting notes."

quick_checks:
- "Every new playbook file: has frontmatter, example block, verification command."
- "Every doctrine change: delta + rationale + approvals in changelog."
- "CI must fail on missing frontmatter or missing example."
- "Every tool used: reviewed tool card and issues index."

contact:
owner: "jar.of.slav@angelof.dev"
repository: "ANGEL-OF-DEV/angelof.dev"

# Minimal human-readable summary line for agents:
one_liner: "Example-first, frontmatter-required playbook enforcing doctrine-aligned, validated, reviewed, and machine-indexed runbooks."
---

## Response Formatting Rule:
- Always start responses with a numbered list.
- Use numbered lists only.
- Keep each list item to 80 characters or fewer.

## Frontmatter Requirements:
- All files under `Doctrine/**` must include frontmatter.
- All `*.yml.md` files anywhere in the repo must include frontmatter.

## TODO Tracking:
- TODOs are a lightweight backlog for small, less defined work.
- Record TODOs in `Doctrine/TODO.yml.md` as an append-only index.
- Store individual TODO files in `Doctrine/TODO/` as `*.yml.md`.
- TODO file frontmatter includes `tags` only (for now).
- Promote to backlog when scope or impact grows.

## Tools & Commands:
- Review tool card and issues index before using a tool.
- Maintain tool cards under `Doctrine/Tools/`.
- Use one tool card per tool with command anchors.
- Keep issue records under `Doctrine/Tools/Issues/`.
- Keep tool cards static; store dynamic records in `Artifacts/Tooling/`.
- Tag tool cards with tool-name, platform, domain, owner, lifecycle.
- Build local tools if missing: `dotnet build Sources/ffrwd/ffrwd.csproj`.
- Use `ffrwd agent init <model>` to create or locate agent worktrees.
- Use `ffrwd.cmd` on Windows or `./ffrwd` on Unix when ffrwd is not on PATH.
- Change directory to the printed worktree path and continue work there.
- Use ffrwd tasking to claim next work; see `Doctrine/Agents.yml.md`.

## Agent Startup Routines & Triggers
- Agents must follow the startup checklist in `Doctrine/Startup.yml.md` before executing any tasks.
- The checklist includes workspace context loading, agent/task preloading (e.g., ffrwd), doctrine/playbook refresh, formatting rule confirmation, tool mapping initialization, and startup validation.
- Reference this checklist in onboarding documentation and CI setup.
- Use `Scripts/agent-startup.cmd` or `Scripts/agent-startup.sh` to log startup.

## Reference
- Startup checklist: `Doctrine/Startup.yml.md`
