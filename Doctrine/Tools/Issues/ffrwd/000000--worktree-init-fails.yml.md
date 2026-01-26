---
title: "ffrwd agent init fails with repo not found"
summary: "Agent init cannot locate repository root."
tags: [tool-name:ffrwd, platform:cross-platform, domain:cli, owner:angelof.dev, lifecycle:active]
---

# ffrwd agent init fails with repo not found

## Context
- Tool: ffrwd
- Scope: running outside a git repository.

## Check
```text
pwd
```
Expected output:
```text
<path is not inside repo>
```

## Confirm
```text
git rev-parse --show-toplevel
```
Expected output:
```text
fatal: not a git repository (or any of the parent directories): .git
```

## Fix
```text
cd /mnt/a/src/fast-forward
```
Expected output:
```text
<no output>
```

## Prevent
- Run ffrwd from within a repo worktree.

## Workaround (Temporary, expires 2099-12)
- Scope: one-off runs for testing.
- Limitations: no repo-specific behavior.
- Risk level: low.
- Rollback: none.
```text
ffrwd agent init <model>
```

## Long-Term Fix (Preferred)
- Use the correct repo root before running.

## Escalation
- When to escalate: repo root detection still fails.
- Contact: repo owners.
