---
title: "Agent Startup Instructions"
summary: "Canonical, machine-readable startup workflow for agents."
version: 1
canonical: "AGENTS.yml.md"
json_cache: "AGENTS.yml.md.json"
lookup_order:
  - "AGENTS.yml.md.json"
  - "AGENTS.yml.md"
startup:
  entrypoint_command: "ffrwd agent start <model> --emit=path"
  entrypoint_note: "Runs init, frontmatter extract, and doctrine load."
  checklist_scripts:
    - "Scripts/agent-startup.cmd"
    - "Scripts/agent-startup.ps1"
    - "Scripts/agent-startup.sh"
  ensure_ffrwd:
    build_command: "dotnet build Sources/ffrwd/ffrwd.csproj"
    note: "Use ffrwd.cmd on Windows or ./ffrwd on Unix from repo root."
  identity:
    command: "ffrwd identity get <model>"
  worktree:
    command: "ffrwd agent init <model>"
    emit: "path"
    next_step: "Change directory to the printed worktree path."
  frontmatter:
    extract_command: "ffrwd frontmatter extract-all"
    extract_single_command: "ffrwd frontmatter extract AGENTS.yml.md"
    rule: "Skip if JSON is newer unless --force is used."
  doctrine:
    manifest_command: "ffrwd agent doctrine --emit json"
    protocol_json: "Doctrine/Prindiples-And-Protocols.yml.md.json"
    fallback: "If missing in worktree, use repo root JSON."
response_format:
  start_with_numbered_list: true
  numbered_lists_only: true
notes:
  - "AGENTS.yml.md is canonical; AGENTS.yml.md.json is generated."
  - "AGENTS.md is not used."
  - "Frontmatter is the source of truth for agents."
tags: [agents, startup, doctrine]
---

# Agents

This file is canonical. The frontmatter above is the machine-readable source.
