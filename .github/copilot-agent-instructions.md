# Copilot Agent Instructions (playbook)

When asked to implement or suggest a pattern, follow this search-and-return strategy.

1) Look up `playbook/manifest.json` for a short programmatic mapping (key → path).
2) If manifest lacks a match, consult `playbook/meta.yaml` for human-facing items and tags.
3) Prefer items with `example_commands` or runnable `examples/*` folders.
4) Output format:
   - Short plan (3 bullets)
   - Files to change (paths)
   - Minimal patch or snippet (only changed lines)
   - One-line verification command

Example prompt to the agent:
"Use Playbook/manifest.json entry `cli/ui/spectre-sample-v1` and produce a new `status` command using Spectre.Console with progress + table. Provide Program.cs patch and test command."

Search keys: tags (preferred), owner, title, example_commands.
