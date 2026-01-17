#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: Scripts/agent-start.sh <model> [-- <command>]" >&2
  exit 2
fi

model="$1"
shift

if [[ "${1:-}" == "--" ]]; then
  shift
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"

cd "$repo_root"

dotnet build "Sources/ffrwd/ffrwd.csproj" >/dev/null
worktree_path="$(
  dotnet run --no-build --project "Sources/ffrwd/ffrwd.csproj" -- \
    agent start "$model" --emit=path
)"

if [[ -z "$worktree_path" ]]; then
  echo "Error: failed to resolve worktree path." >&2
  exit 1
fi

cd "$worktree_path"

if [[ $# -gt 0 ]]; then
  exec "$@"
fi

exec "${SHELL:-/bin/bash}"
