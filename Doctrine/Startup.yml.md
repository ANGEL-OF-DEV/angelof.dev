---
title: "Agent Startup Checklist"
summary: "Required steps for agent initialization in this workspace."
tags: [startup, checklist, doctrine, agent]
last_reviewed: 2026-01-18
status: stable
---

# Agent Startup Checklist

Use `Scripts/agent-startup.cmd` on Windows or `Scripts/agent-startup.sh` on Unix
to run this checklist and log actions under `logs/startup/`.

1. Load workspace context, folders, files, and environment info.
2. Run or preload required agent tasks. The ffrwd agent must be called.
3. Refresh doctrine, playbook, and TODO state if requested or if changes detected.
4. Confirm formatting and response rules (numbered lists, line length, etc.).
5. Initialize tool mappings and supported operations.
6. Reference this checklist in Playbook.md and onboarding docs.
7. Validate startup by checking for errors or missing steps.
8. Log or report completion status (success, warnings, errors).
9. Await user input or next action, ready to execute tasks or answer questions.
