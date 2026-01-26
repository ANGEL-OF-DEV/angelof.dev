---
title: "Tool: <tool-name>"
summary: "<one sentence purpose>"
tags: [tool-name:<tool>, platform:<platform>, domain:<domain>, owner:<owner>, lifecycle:<lifecycle>]
aliases: ["<common name>", "<command>"]
---

# Tool: <tool-name>

## Purpose
<one clear sentence describing the tool and why it exists>

## Aliases
- Names: <common names>
- Commands: `<tool> <command>`, `<tool> <alias>`

## When To Use
- Decision rules: <rule>
- Example: <example>

## When Not To Use
- Anti-pattern: <anti-pattern>
- Risk: <risk>

## Opportunity Cost / Time To Value
- <tradeoff or time-to-value note>

## Quick Start
- Install: `<install command>`
- Auth: `<auth command or step>`
- Env vars: `<ENV_VAR=value>`

## Commands
### <command-name>
```text
<command>
```
Expected output:
```text
<output>
```

## Parameters And Flags
- `<flag>`: <meaning or gotcha>

## Failure Modes And Safety Notes
- <prod risk, limit, or safety note>

## Observability And Monitoring
- Logs: <location or command>
- Metrics: <metric name>
- Alerts: <alert name>
- Runbook links: <link or path if available>

## Measurability And Success Metrics
- <metric and target>

## Alternatives
- Option A: <tradeoff>
- Option B: <tradeoff>

Decision table (use when 3+ options):
| Option | Pros | Cons | When to choose |
| --- | --- | --- | --- |
| A | <pro> | <con> | <criteria> |

## Issues
- Index: `Doctrine/Tools/Issues.yml.md`
- Tool issues: `Doctrine/Tools/Issues/<tool>/`

## Dynamic Records
- Change log: `Artifacts/Tooling/<tool>/changes.yml`
- Tested versions: `Artifacts/Tooling/<tool>/versions.yml`
- Known issues: `Artifacts/Tooling/<tool>/issues.yml`
- Verification checklist: `Artifacts/Tooling/<tool>/verification.yml`

## Ownership And Support
- Owner: <owner>
- Escalation: <contact>

## See Also
- <related tool or doc link>
