---
title: "Agents"
summary: "Doctrine for CLI-driven agent tasking and workflow."
schema_version: 1
tags: [doctrine, agents, workflow]
agent_purpose:
  - "Deterministic task intake and ownership via CLI"
  - "Reduce idle decisions; focus on delivery"
task_intake:
  - "If no explicit instruction, use agent tasking in ffrwd"
  - "Build tools and init worktree before tasking"
  - "Build: dotnet build Sources/ffrwd/ffrwd.csproj"
  - "Init: ffrwd agent init <model>, then cd into path"
  - "Wrapper: ffrwd.cmd (Windows) or ./ffrwd (Unix) from repo root"
cli_responsibilities:
  - "Discover repo work (reviews, cleanup, archiving)"
  - "Record task claims, state transitions, ownership"
agent_responsibilities:
  - "Claim tasks only via CLI"
  - "Update task state on meaningful progress"
  - "Follow workflow rules to completion or escalation"
task_states:
  - "discovered -> claimed -> in-progress -> done"
  - "claimed/in-progress -> blocked -> in-progress or done"
  - "claimed/in-progress -> abandoned -> reclaimed or reassigned"
abandoned_and_reassignment:
  - "No update within claim_timeout: mark abandoned"
  - "No reclaim within abandoned_timeout: requeue"
  - "Same identity may reclaim without error"
timeouts:
  claim_timeout: "4h"
  abandoned_timeout: "4h"
records:
  task_queue: "Artifacts/Agent/Queue.yml"
  audit_log: "Artifacts/Agent/History.yml"
system_branch:
  name: "system/main"
  operator: "ffrwd"
  root: "Artifacts/System/"
  tasks_root: "Artifacts/System/Tasks/"
  sequence_file: "Artifacts/System/Tasks/sequence.json"
  notes_path_template: "Artifacts/System/Tasks/<task_id>/notes.md"
task_sources:
  - id: "todo"
    doctrine: "Doctrine/TODO.yml.md"
  - id: "backlog"
    doctrine: "Doctrine/Backlog.yml.md"
memory_branch:
  name: "contributors/<identity>/self/main"
  allowed: "Notes, prompts, TODOs, run logs, metadata"
  prohibited: "Code changes, shared artifacts, releases, secrets"
  storage: "Artifacts/Agent/Notes/<identity>/"
  merge_policy: "Never merge or PR to shared branches"
  lifecycle: "Commit after task; prune on rotation"
  howto: "Playbook/How2s/agent-memory-notes.yml.md"
---

# Agents

## Purpose
- Provide deterministic task intake and ownership via CLI.
- Reduce idle decisions and focus on delivery.

## Task Intake
- If no explicit instruction, use the agent tasking command in `ffrwd`.
- Build tools and initialize a worktree before tasking.
  - `dotnet build Sources/ffrwd/ffrwd.csproj`
  - `ffrwd agent init <model>` then `cd` into the printed path.
  - Use `ffrwd.cmd` on Windows or `./ffrwd` on Unix if ffrwd is not on PATH.

## CLI Responsibilities
- Discover work needed around the repo (reviews, cleanup, archiving).
- Record task claims, state transitions, and ownership.

## Agent Responsibilities
- Claim tasks only via CLI.
- Update task state on meaningful progress.
- Follow workflow rules to completion or escalation.

## Agent Memory Branch
- Each agent maintains a private main branch for notes and memories.
- Branch name: `contributors/<identity>/self/main`.
- Allowed content: notes, prompts, TODOs, run logs, metadata.
- Prohibited: code changes, shared artifacts, releases, secrets.
- Storage path: `Artifacts/Agent/Notes/<identity>/`.
- Merge policy: never merge or PR to shared branches.
- Lifecycle: commit after task; prune on rotation.
- How-to: `Playbook/How2s/agent-memory-notes.yml.md`.

## Task States
- discovered -> claimed -> in-progress -> done
- claimed/in-progress -> blocked -> in-progress or done
- claimed/in-progress -> abandoned -> reclaimed or reassigned

## Abandoned And Reassignment
- If no update within `claim_timeout`, mark abandoned.
- If abandoned and no reclaim within `abandoned_timeout`, requeue.
- Same identity may reclaim without error.

## Timeouts
- claim_timeout: 4h
- abandoned_timeout: 4h

## Records
- Task queue: `Artifacts/Agent/Queue.yml`
- Audit log: `Artifacts/Agent/History.yml`

## System Branch
- Branch: `system/main` (operated by `ffrwd` only).
- Root: `Artifacts/System/`.
- Task notes: `Artifacts/System/Tasks/<task_id>/notes.md`.
- Task ID sequence: `Artifacts/System/Tasks/sequence.json`.

## Task Sources
- TODOs: `Doctrine/TODO.yml.md`.
- Backlog: `Doctrine/Backlog.yml.md`.
