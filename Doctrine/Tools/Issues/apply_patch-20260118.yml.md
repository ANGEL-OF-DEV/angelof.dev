---
title: "apply_patch context sensitivity issue"
tags: [tool, apply_patch, issue, workaround]
owner: "aid-gpt-4.1-00"
status: open
---
- Problem: apply_patch may fail if file context does not match exactly (e.g., blank lines, formatting, encoding, or concurrent edits).
- Example: Adding a TODO entry after a header may fail if extra blank lines or formatting changes exist.
- Workaround: Use insert_edit_into_file for simple, single-location edits, as it is less sensitive to context and formatting.
- Recommendation: Prefer insert_edit_into_file for robust, single-point changes; use apply_patch for complex, multi-region edits only when context is certain.
