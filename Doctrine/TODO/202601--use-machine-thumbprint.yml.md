---
tags: [doctrine, identity, notes, tasks]
---

# TODO Use machine thumbprint for note/task IDs

Status: Open
Owner: TBD

Context:
- Add a local machine thumbprint to reduce ID collisions.

Details:
- Use a stable OS identifier, hash it, then truncate.
- Linux: `cat /etc/machine-id | sha256sum | cut -c1-16`.
- macOS id: `system_profiler SPHardwareDataType | awk '/UUID/{print $3}'`.
- Hash id: `printf %s "$ID" | shasum -a 256 | cut -c1-16`.
- Windows (PS): `(Get-CimInstance Win32_ComputerSystemProduct).UUID`.
- Hash with SHA256 and take 8-16 chars for the thumbprint.
- Store locally in `.git/ffrwd/machine-thumbprint` (untracked).
- Optionally add `.git/ffrwd/` to `.git/info/exclude`.
- Use suffix like `<seq>-<thumb>` for note/task IDs.

Additional:
- Do not commit or share the thumbprint value.
- Rotate thumbprint if OS ID changes (reinstall or reset).
- Prefer per-clone ID for shared artifacts; thumbprint is local.
