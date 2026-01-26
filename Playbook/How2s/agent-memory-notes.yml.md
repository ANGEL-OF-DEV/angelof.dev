---
title: "Agent Memory Notes"
summary: "Store agent notes on the self branch only."
owner: "jar.of.slav@angelof.dev"
tags: [agents, memory, notes, workflow]
last_reviewed: "2026-01-18"
status: "draft"
---

# Agent Memory Notes

Example:

```sh
mkdir -p Artifacts/Agent/Notes/aid-<model>-00
printf "%s\n" "Task notes." > \
  Artifacts/Agent/Notes/aid-<model>-00/2026-01-18.md
git checkout contributors/aid-<model>-00/self/main
git add Artifacts/Agent/Notes/aid-<model>-00/2026-01-18.md
git commit -m "notes: task context"
```

Rationale:
Keep notes isolated on the agent branch to avoid noisy shared history.
This supports private context while keeping shared branches clean.

When to use:
- Capture task context, decisions, and next steps.
- Record prompts, run logs, and short TODOs.

When not to use:
- Anything intended for shared documentation or code changes.
- Secrets, credentials, or user data.

Steps:
1. Verify you are on `contributors/<identity>/self/main`.
2. Write notes under `Artifacts/Agent/Notes/<identity>/`.
3. Commit notes after each task or major context change.

Validation:
- `git status -sb` shows the self branch.
- `git ls-files Artifacts/Agent/Notes/<identity>/` lists notes only.
