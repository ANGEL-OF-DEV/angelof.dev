---
title: "Tool: ffrwd"
summary: "CLI for agent identity, worktrees, and frontmatter extraction."
tags: [tool-name:ffrwd, platform:cross-platform, domain:cli, owner:angelof.dev, lifecycle:active]
aliases: ["ffrwd", "fast-forward", "ffrwd identity get", "ffrwd agent init"]
---

# Tool: ffrwd

## Purpose
Provide agent identity, initialize worktrees, and extract frontmatter JSON.

## Aliases
- Names: ffrwd, fast-forward
- Commands: `ffrwd identity get`, `ffrwd agent init`, `ffrwd agent start`
- Commands: `ffrwd agent doctrine`, `ffrwd agent task next`
- Commands: `ffrwd frontmatter extract`, `ffrwd frontmatter extract-all`

## When To Use
- Decision rules: need identity or a worktree for an agent.
- Example: initializing a new agent worktree.

## When Not To Use
- Anti-pattern: using for unrelated git operations.
- Risk: unintended worktree creation.

## Opportunity Cost / Time To Value
- Faster than manual worktree setup, minimal setup time.

## Quick Start
- Install: `dotnet build Sources/ffrwd/ffrwd.csproj`
- Auth: none
- Env vars: none
- Wrapper (Windows): `ffrwd.cmd` from repo root
- Wrapper (Unix): `./ffrwd` from repo root
- Agent start (sh): `Scripts/agent-start.sh gpt-5 -- <command>`
- Agent start (pwsh): `.\Scripts\agent-start.ps1 -Model gpt-5 -- <command>`

## Commands
### identity get
```text
ffrwd identity get gpt-5
```
Expected output:
```text
aid-gpt-5-00
```

### agent init
```text
ffrwd agent init gpt-5
```
Expected output (auto on unix):
```text
cd /mnt/a/src/fast-forward.aid-gpt-5-00
```
Expected output (auto on Windows):
```text
Set-Location C:\src\fast-forward.aid-gpt-5-00
```

### agent init --emit=path
```text
ffrwd agent init gpt-5 --emit=path
```
Expected output:
```text
/mnt/a/src/fast-forward.aid-gpt-5-00
```

### agent start
```text
ffrwd agent start gpt-5 --emit=path
```
Expected output:
```text
/mnt/a/src/fast-forward.aid-gpt-5-00
```
Notes:
- Extracts frontmatter in repo root and worktree before returning the path.
- Falls back to repo-root doctrine JSON if the worktree copy is missing.

### agent doctrine
```text
ffrwd agent doctrine --emit json
```
Expected output:
```json
{
  "source": "Doctrine/Prindiples-And-Protocols.yml.md.json",
  "doctrine_files": [
    "Doctrine/Prindiples-And-Protocols.yml.md.json",
    "Doctrine/Agents.yml.md.json",
    "Doctrine/Tools.yml.md.json",
    "Doctrine/Tools/ffrwd.yml.md.json",
    "Doctrine/Tools/Templates/Tool-Card.Template.yml.md.json",
    "Doctrine/Tools/Issues.yml.md.json",
    "Doctrine/TODO.yml.md.json"
  ],
  "doctrine_directories": [
    "Doctrine/",
    "Doctrine/TODO/",
    "Doctrine/Tools/",
    "Doctrine/Tools/Issues/",
    "Artifacts/Tooling/"
  ]
}
```

### agent task next
```text
ffrwd agent task next --task-source todo
```
Expected output:
```json
{
  "task_id": 1,
  "notes_path": "Artifacts/System/Tasks/1/notes.md",
  "source": {
    "type": "todo",
    "index": "Doctrine/TODO.yml.md",
    "entry_index": 0,
    "path": "Doctrine/TODO/202601--snapshots-metadata.yml.md",
    "title": "Snapshot repository config/deps metadata",
    "status": "open",
    "owner": "TBD"
  },
  "sequence": {
    "id": "todo-backlogify",
    "display_mode": "sequential",
    "display_prefix": "T",
    "steps": [
      {
        "display_id": "T1",
        "title": "Restate intent and scope",
        "description": "Rephrase TODO; define boundaries and outcome."
      },
      {
        "display_id": "T2",
        "title": "Collect context and references",
        "description": "Gather links, code refs, and dependencies."
      }
    ]
  }
}
```

### frontmatter extract
```text
ffrwd frontmatter extract Doctrine/Prindiples-And-Protocols.yml.md
```
Expected output:
```text
Doctrine/Prindiples-And-Protocols.yml.md.json
```

### frontmatter extract-all
```text
ffrwd frontmatter extract-all
```
Expected output:
```text
Doctrine/Prindiples-And-Protocols.yml.md.json
Doctrine/Agents.yml.md.json
Doctrine/Tools.yml.md.json
```

## Parameters And Flags
- `<model>`: required, letters/digits/dash/underscore/dot.
- `agent init --emit <format>`: auto (default), path, sh, pwsh, cmd.
- `agent start --emit <format>`: auto (default), path, sh, pwsh, cmd.
- `agent start --force`: overwrite JSON output.
- `agent start --pretty`: pretty-print JSON output.
- `agent start --source <path>`: doctrine protocol JSON path.
- `agent doctrine --emit <format>`: json (default), yaml.
- `agent doctrine --source <path>`: doctrine protocol JSON path.
- `agent task next --source <path>`: doctrine protocol JSON path.
- `agent task next --task-source <id>`: task source id (default: todo).
- `agent task next --pretty`: pretty-print JSON output.
- `frontmatter extract --force`: overwrite JSON output.
- `frontmatter extract --pretty`: pretty-print JSON output.
- `frontmatter extract-all --force`: overwrite JSON output.
- `frontmatter extract-all --pretty`: pretty-print JSON output.

## Failure Modes And Safety Notes
- Repo not found: run inside a git worktree or repo.
- Existing path: tool increments index and logs a TODO.

## Observability And Monitoring
- Logs: stderr output from command failures.
- Metrics: none.
- Alerts: none.
- Runbook links: none.

## Measurability And Success Metrics
- Worktree path printed and created.

## Alternatives
- Manual git worktree commands (more error prone).
- Custom scripts (less standardized).

Decision table (use when 3+ options):
| Option | Pros | Cons | When to choose |
| --- | --- | --- | --- |
| ffrwd | Standardized | Limited scope | Agent setup only |

## Issues
- Index: `Doctrine/Tools/Issues.yml.md`
- Tool issues: `Doctrine/Tools/Issues/ffrwd/`

## Dynamic Records
- Change log: `Artifacts/Tooling/ffrwd/changes.yml`
- Tested versions: `Artifacts/Tooling/ffrwd/versions.yml`
- Known issues: `Artifacts/Tooling/ffrwd/issues.yml`
- Verification checklist: `Artifacts/Tooling/ffrwd/verification.yml`


## Search Guidance
When searching for TODOs or doctrine items, note that:
- TODOs are tracked in both `Doctrine/TODO.yml.md` (index) and individual files in `Doctrine/TODO/`.
- Simple file or code comment searches may miss structured YAML/Markdown entries.
- Use broad search patterns (including `Doctrine/TODO.yml.md` and all files in `Doctrine/TODO/`) to ensure all tracked items are found.
- For best results, search for keywords in both code and doctrine tracking files, not just code comments.

## Ownership And Support
- Owner: angelof.dev
- Escalation: repo owners

## See Also
- `Doctrine/Tools.yml.md`
