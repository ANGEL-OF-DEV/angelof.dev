#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: Scripts/agent-startup.sh <model> [--log-dir <path>]" >&2
  exit 2
fi

model="$1"
shift

log_dir=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --log-dir)
      log_dir="$2"
      shift 2
      ;;
    --)
      shift
      break
      ;;
    *)
      break
      ;;
  esac
done

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"

if [[ -z "$log_dir" ]]; then
  log_dir="${repo_root}/logs/startup"
fi

mkdir -p "$log_dir"

timestamp="$(date +%Y%m%d-%H%M%S)"
log_path="${log_dir}/startup-${timestamp}.log"
latest_path="${log_dir}/latest.log"
state_path="${log_dir}/state.txt"
status_path="${log_dir}/status.json"

errors=()
warnings=()

log() {
  local stamp
  stamp="$(date +"%Y-%m-%dT%H:%M:%S%z")"
  echo "${stamp} $*" | tee -a "$log_path"
}

add_error() {
  errors+=("$1")
  log "ERROR: $1"
}

add_warning() {
  warnings+=("$1")
  log "WARN: $1"
}

RUN_CMD_STATUS=0
RUN_CMD_OUTPUT=""
run_cmd() {
  local label="$1"
  local critical="$2"
  shift 2
  log "COMMAND: $label"
  set +e
  local output
  output="$("$@" 2>&1)"
  local status=$?
  set -e
  if [[ -n "$output" ]]; then
    while IFS= read -r line; do
      log "OUTPUT: $line"
    done <<< "$output"
  fi
  log "STATUS: $status"
  if [[ $status -ne 0 ]]; then
    local message="${label} failed with exit code ${status}."
    if [[ "$critical" == "critical" ]]; then
      add_error "$message"
    else
      add_warning "$message"
    fi
  fi
  RUN_CMD_STATUS=$status
  RUN_CMD_OUTPUT="$output"
}

log "STEP 1: Workspace context"
log "Repo root: $repo_root"
log "OS: $(uname -a)"
log "User: $(whoami)"
log "Shell: ${SHELL:-}"
log "TERM: ${TERM:-}"
log "TERM_PROGRAM: ${TERM_PROGRAM:-}"
log "WT_SESSION: ${WT_SESSION:-}"
log "VSCODE_PID: ${VSCODE_PID:-}"

log "Workspace directories:"
while IFS= read -r -d '' dir; do
  rel="${dir#"$repo_root"/}"
  log "DIR: ${rel:-.}"
done < <(find "$repo_root" -type d -print0)

log "Workspace files:"
while IFS= read -r -d '' file; do
  rel="${file#"$repo_root"/}"
  log "FILE: ${rel:-.}"
done < <(find "$repo_root" -type f -print0)

log "STEP 2: Preload agent tasks"
ffrwd_cmd="${repo_root}/ffrwd"
if [[ ! -x "$ffrwd_cmd" ]]; then
  ffrwd_cmd="ffrwd"
  if ! command -v "$ffrwd_cmd" >/dev/null 2>&1; then
    add_error "ffrwd wrapper not found or not executable."
  fi
fi

run_cmd "dotnet build ffrwd" critical dotnet build \
  "Sources/ffrwd/ffrwd.csproj"

worktree_path=""
if command -v "$ffrwd_cmd" >/dev/null 2>&1 || [[ -x "$ffrwd_cmd" ]]; then
  run_cmd "ffrwd agent start" critical "$ffrwd_cmd" agent start \
    "$model" --emit=path
  if [[ -n "$RUN_CMD_OUTPUT" ]]; then
    worktree_path="$(printf '%s\n' "$RUN_CMD_OUTPUT" | awk 'NF{print $0}' | tail -n 1)"
  fi
  if [[ -z "$worktree_path" ]]; then
    add_error "Failed to resolve worktree path from ffrwd output."
  else
    log "Worktree path: $worktree_path"
  fi
fi

log "STEP 3: Refresh doctrine, playbook, and TODO"
doctrine_root="${repo_root}/Doctrine"
playbook_root="${repo_root}/Playbook"
todo_index="${doctrine_root}/TODO.yml.md"

required_files=(
  "${repo_root}/AGENTS.yml.md"
  "${doctrine_root}/Startup.yml.md"
  "${doctrine_root}/Playbook.md"
  "${todo_index}"
)

for req in "${required_files[@]}"; do
  if [[ ! -f "$req" ]]; then
    add_error "Required file missing: ${req#"$repo_root"/}"
  else
    log "Required file present: ${req#"$repo_root"/}"
  fi
done

state_tmp="${log_dir}/state.tmp"
: > "$state_tmp"

if [[ -d "$doctrine_root" ]]; then
  while IFS= read -r -d '' file; do
    rel="${file#"$repo_root"/}"
    mtime="$(stat -c '%Y' "$file")"
    size="$(stat -c '%s' "$file")"
    printf '%s|%s|%s\n' "$rel" "$mtime" "$size" >> "$state_tmp"
  done < <(find "$doctrine_root" -type f -print0)
fi

if [[ -d "$playbook_root" ]]; then
  while IFS= read -r -d '' file; do
    rel="${file#"$repo_root"/}"
    mtime="$(stat -c '%Y' "$file")"
    size="$(stat -c '%s' "$file")"
    printf '%s|%s|%s\n' "$rel" "$mtime" "$size" >> "$state_tmp"
  done < <(find "$playbook_root" -type f -print0)
fi

sort "$state_tmp" -o "$state_tmp"

if [[ -f "$state_path" ]]; then
  diff_output="$(diff -u "$state_path" "$state_tmp" || true)"
  if [[ -n "$diff_output" ]]; then
    log "CHANGE: state diff detected"
    while IFS= read -r line; do
      log "DIFF: $line"
    done <<< "$diff_output"
  else
    log "CHANGE: no changes detected"
  fi
else
  log "CHANGE: no prior state found"
fi

mv "$state_tmp" "$state_path"

while IFS= read -r -d '' file; do
  json_path="${file}.json"
  if [[ ! -f "$json_path" ]]; then
    add_warning "Missing JSON cache: ${json_path#"$repo_root"/}"
    continue
  fi
  if [[ "$json_path" -ot "$file" ]]; then
    add_warning "Stale JSON cache: ${json_path#"$repo_root"/}"
  fi
done < <(find "$doctrine_root" -name "*.yml.md" -type f -print0)

if [[ -f "${repo_root}/AGENTS.yml.md" ]]; then
  agents_json="${repo_root}/AGENTS.yml.md.json"
  if [[ ! -f "$agents_json" ]]; then
    add_warning "Missing JSON cache: ${agents_json#"$repo_root"/}"
  elif [[ "$agents_json" -ot "${repo_root}/AGENTS.yml.md" ]]; then
    add_warning "Stale JSON cache: ${agents_json#"$repo_root"/}"
  fi
fi

log "STEP 4: Confirm formatting and response rules"
format_files=(
  "${repo_root}/AGENTS.yml.md"
  "${doctrine_root}/Playbook.md"
  "${doctrine_root}/Prindiples-And-Protocols.yml.md"
)

format_found=0
max_found=0
for file in "${format_files[@]}"; do
  if [[ ! -f "$file" ]]; then
    add_warning "Formatting rules file missing: ${file#"$repo_root"/}"
    continue
  fi
  start_with="false"
  numbered_only="false"
  max_value=""
  if grep -Eq "start_with_numbered_list:[[:space:]]*true" "$file"; then
    start_with="true"
    format_found=1
  fi
  if grep -Eq "numbered_lists_only:[[:space:]]*true" "$file"; then
    numbered_only="true"
    format_found=1
  fi
  if grep -Eq "list_item_max_length:[[:space:]]*[0-9]+" "$file"; then
    max_value="$(grep -E "list_item_max_length:[[:space:]]*[0-9]+" "$file" | head -n 1 | awk -F: '{print $2}' | tr -d ' ')"
    max_found=1
    format_found=1
  fi
  log "Format rules in ${file#"$repo_root"/}:"
  log "start_with_numbered_list=${start_with}"
  log "numbered_lists_only=${numbered_only}"
  if [[ -n "$max_value" ]]; then
    log "list_item_max_length=${max_value}"
  fi
done

if [[ $format_found -eq 0 ]]; then
  add_error "No formatting rules found in doctrine/playbook."
fi
if [[ $max_found -eq 0 ]]; then
  add_warning "No line length rule found in doctrine/playbook."
fi

log "STEP 5: Initialize tool mappings"
tools_root="${doctrine_root}/Tools"
if [[ ! -d "$tools_root" ]]; then
  add_warning "Tools directory missing: ${tools_root#"$repo_root"/}"
else
  while IFS= read -r -d '' file; do
    tool_name="$(sed -n 's/^title:[[:space:]]*\"Tool:[[:space:]]*\\([^\"]*\\)\".*/\\1/p' "$file" | head -n 1)"
    if [[ -z "$tool_name" ]]; then
      tool_name="$(sed -n 's/^#\\s*Tool:[[:space:]]*//p' "$file" | head -n 1)"
    fi
    if [[ -z "$tool_name" ]]; then
      tool_name="$(basename "$file" .yml.md)"
    fi
    log "Tool mapping: $tool_name => ${file#"$repo_root"/}"
  done < <(find "$tools_root" -name "*.yml.md" -type f -print0)
fi

if command -v "$ffrwd_cmd" >/dev/null 2>&1 || [[ -x "$ffrwd_cmd" ]]; then
  run_cmd "ffrwd --help" warn "$ffrwd_cmd" --help
  if [[ $RUN_CMD_STATUS -ne 0 ]]; then
    add_warning "ffrwd help check failed."
  fi
fi

log "STEP 6: Validate startup state"
if [[ ${#errors[@]} -gt 0 ]]; then
  log "Startup validation failed."
  for err in "${errors[@]}"; do
    log "ERROR: $err"
  done
else
  log "Startup validation passed."
fi

status="success"
if [[ ${#errors[@]} -gt 0 ]]; then
  status="errors"
elif [[ ${#warnings[@]} -gt 0 ]]; then
  status="warnings"
fi

log "STEP 7: Write status"
escaped_worktree="${worktree_path//\\/\\\\}"
escaped_worktree="${escaped_worktree//\"/\\\"}"
{
  echo "{"
  echo "  \"timestamp\": \"$(date -u +"%Y-%m-%dT%H:%M:%SZ")\","
  echo "  \"status\": \"${status}\","
  echo "  \"warnings\": ${#warnings[@]},"
  echo "  \"errors\": ${#errors[@]},"
  echo "  \"worktree_path\": \"${escaped_worktree}\""
  echo "}"
} > "$status_path"
log "Status: $status"
log "Status file: $status_path"

log "STEP 8: Await user input"
log "Startup complete; ready for next action."

cp "$log_path" "$latest_path"

if [[ ${#errors[@]} -gt 0 ]]; then
  exit 1
fi
