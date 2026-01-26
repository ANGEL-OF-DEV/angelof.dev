---
authority: "angelof.dev governance"
doctrine_location: "Doctrine/"

core_principles:
  enforce_doctrine: true
  minimal_changes: "Propose only when absolutely necessary"
  document_rationale: true
  strict_validation: "All validations/tests must pass before commit"
  avoid_ambiguity: "Never accept ambiguous instructions; require clarification"
  doctrine_frontmatter_required: true
  yml_md_frontmatter_required: true

instruction_handling:
  verify_against_doctrine: true
  on_conflict:
    action: "Propose minimal doctrine change to resolve conflict"
    requirements:
      - document_rationale: true
      - show_minimality: true
      - include_todo: true
response_format:
  start_with_numbered_list: true
  numbered_lists_only: true
  list_item_max_length: 80

todo_tracking:
  enabled: true
  purpose: "Lightweight backlog for small, low-ceremony items"
  index: "Doctrine/TODO.yml.md"
  items_path: "Doctrine/TODO/"
  item_extension: ".yml.md"
  item_frontmatter_fields:
    - tags
  format: "Append-only index log"
  promotion: "Promote to backlog when scope grows"

   # Search Guidance
   # TODOs are tracked in both the index file (`Doctrine/TODO.yml.md`) and individual files in `Doctrine/TODO/`.
   # Simple file or code comment searches may miss structured YAML/Markdown entries.
   # Use broad search patterns (including the index and all files in `Doctrine/TODO/`) to ensure all tracked items are found.
   # For best results, search for keywords in both code and doctrine tracking files, not just code comments.

tools_and_commands:
  index: "Doctrine/Tools.yml.md"
  tools_path: "Doctrine/Tools/"
  tool_card_template: "Doctrine/Tools/Templates/Tool-Card.Template.yml.md"
  issues_index: "Doctrine/Tools/Issues.yml.md"
  issues_path: "Doctrine/Tools/Issues/"
  command_anchor_convention: "Use anchors per command in tool cards"
  dynamic_records_root: "Artifacts/Tooling/"
  agent_setup:
    build_command: "dotnet build Sources/ffrwd/ffrwd.csproj"
    init_command: "ffrwd agent init <model>"
    note: "Build local tools if needed; then init worktree."
    wrapper_note: "Use ffrwd.cmd on Windows or ./ffrwd on Unix from repo root."
    next_step: "Change directory to the printed worktree path."
agent_tasking:
  doctrine: "Doctrine/Agents.yml.md"
  tool: "ffrwd"
  command_source: "Doctrine/Tools/ffrwd.yml.md"
  states: [discovered, claimed, in-progress, blocked, abandoned, done]
  claim_timeout: "4h"
  abandoned_timeout: "4h"
  memory_branch:
    name: "contributors/<identity>/self/main"
    purpose: "Agent-only notes and memories"
    allowed: "Notes, prompts, TODOs, run logs, metadata"
    prohibited: "Code changes, shared artifacts, releases, secrets"
    storage: "Artifacts/Agent/Notes/<identity>/"
    merge_policy: "Never merge or PR to shared branches"
    lifecycle: "Commit after task; prune on rotation"

change_process:
  planning:
    create_detailed_todo_list: true
    map_tasks_to_doctrine: true
  execution:
    follow_todo_meticulously: true
    update_validators_schemas_tests_docs: true
    run_all_validations_and_tests: true
    block_commit_if_failures: true
  review:
    ensure_alignment_with_doctrine: true
    propose_adjustments_if_discrepant: true

doctrine_modification:
  require_stakeholder_review_and_approval: true
  document_all_changes_comprehensively: true
  include_rationale_tests_and_migration_steps: true

quality_and_compliance:
  continuous_monitoring: "Regular reviews of processes and practices"
  auditing: "Periodic audits; record findings and corrective actions"
  metrics: "Define and track compliance and governance effectiveness metrics"
  reporting: "Comprehensive summaries of actions, changes, and alignment with doctrine"

communication_and_collaboration:
  clearly_articulate_principles: true
  stakeholder_engagement: "Involve relevant parties for buy-in and approval"
  transparency: "Openly document actions and decisions"
  conflict_resolution: "Refer to doctrine and document outcomes"

training_and_culture:
  train_new_agents: "Emphasize doctrine and governance procedures"
  foster_governance_literacy: true
  promote_accountability_and_ownership: true

innovation_and_change_management:
  evaluate_changes_against_doctrine: true
  propose_minimal_safe_innovations: true
  document_impact_and_migration: true
  scaling_and_deprecation: "Ensure expansions or retirements follow doctrine and are documented"

recordkeeping:
  change_logs: true
  test_results_and_validations: true
  approvals_and_stakeholders: true
  todo_lists_and_execution_records: true

operational_rules:
  never_commit_if_tests_fail: true
  never_implement_ambiguous_instructions: "Seek clarification first"
  maintain_clarity_in_documentation: true
  ensure_tool_compatibility_with_doctrine: true
---

# Governance Protocols at <angelof.dev>

## Core Principles:
1. Adhere strictly to the **doctrine** specified in the `Doctrine/` directory.
2. Propose changes only when absolutely necessary and document them comprehensively.
3. Ensure all changes are validated, tested, and aligned with established governance protocols.
4. Maintain clarity and avoid ambiguity in instructions, decisions, and documentation.
5. Collaborate transparently and uphold ethical decision-making.
6. Engage stakeholders and foster accountability, teamwork, and inclusivity.
7. Promote continuous improvement while preserving core principles of governance.
8. Document all processes and rationale to ensure traceability and clear communication.

## Governance Processes:
### When verifying instructions:
- Verify against the doctrine for compliance.
- If conflicts arise, propose minimal adjustments to the doctrine with full documentation of the rationale.

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
- Keep entries short, with owner and status.

## Tools & Commands:
- Maintain tool cards under `Doctrine/Tools/`.
- Use one tool card per tool with command anchors.
- Keep issue records under `Doctrine/Tools/Issues/`.
- Keep tool cards static; store dynamic records in `Artifacts/Tooling/`.
- Tag tool cards with tool-name, platform, domain, owner, lifecycle.
- Build local tools if missing: `dotnet build Sources/ffrwd/ffrwd.csproj`.
- Use `ffrwd agent init <model>` to create or locate agent worktrees.
- Use `ffrwd.cmd` on Windows or `./ffrwd` on Unix when ffrwd is not on PATH.
- Change directory to the printed worktree path and continue work there.

## Agent Tasking:
- Use the ffrwd agent tasking command for next work.
- Do not self-select tasks; claim via CLI only.
- Default timeouts: claim 4h, abandoned 4h.
- Use `contributors/<identity>/self/main` for notes only.
- See `Doctrine/Agents.yml.md` for workflow and states.

### When implementing changes:
- Create a detailed **todo list** outlining all tasks needed for compliance with the doctrine.
- Update all validators, schemas, tests, and documentation.
- Ensure no changes are committed without passing validations or tests.

### When documenting changes:
- Provide clear:
  - Rationale for the modification.
  - Explanation of alignment with the doctrine.
- Communicate governance changes comprehensively.

### When reviewing changes:
- Check alignment with the doctrine and protocols.
- Propose necessary adjustments to address discrepancies.

### When collaborating:
- Engage stakeholders with adherence to governance principles.
- Promote understanding of doctrine and decision-making processes.

### When managing conflicts:
- Use the doctrine for conflict resolution.
- Ensure governance framework is understood and followed by all parties.

### When updating the doctrine:
- Ensure stakeholder review and approval before implementation.

### When reporting/compliance monitoring:
- Provide thorough summaries of actions, changes, and their alignment with governance protocols.
- Regularly review practices to ensure adherence.

### When scaling or deprecating practices:
- Justify actions within the governance framework.
- Document and communicate changes transparently.

### When training and fostering literacy:
- Emphasize governance principles and the doctrine's role in guiding decisions.
- Promote awareness among all stakeholders.

### When embracing innovation:
- Ensure new approaches align with the governance framework.
- Evaluate processes against the doctrine to maintain integrity.

### When managing risks:
- Identify potential governance issues.
- Develop mitigation strategies that uphold the doctrine.

### When driving strategic initiatives:
- Align efforts with organizational goals while upholding governance principles.

### When auditing and learning:
- Conduct thorough governance reviews.
- Document and integrate lessons learned.
