#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'USAGE'
Usage: Scripts/install-dotnet.sh [--version <ver>] [--install-dir <dir>]
Defaults to SDK version from global.json if present.
USAGE
}

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GLOBAL_JSON="${ROOT_DIR}/global.json"
INSTALL_DIR="${DOTNET_INSTALL_DIR:-${HOME}/.dotnet}"
VERSION=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="${2:-}"
      shift 2
      ;;
    --install-dir)
      INSTALL_DIR="${2:-}"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      VERSION="$1"
      shift
      ;;
  esac
done

if [[ -z "${VERSION}" && -f "${GLOBAL_JSON}" ]]; then
  VERSION="$(grep -m1 '"version"' "${GLOBAL_JSON}" \
    | sed -E 's/.*"version": *"([^"]+)".*/\1/')"
fi

if [[ -z "${VERSION}" ]]; then
  echo "Error: .NET SDK version not provided." >&2
  exit 2
fi

if command -v curl >/dev/null 2>&1; then
  FETCH="curl -sSL"
elif command -v wget >/dev/null 2>&1; then
  FETCH="wget -qO-"
else
  echo "Error: curl or wget is required." >&2
  exit 3
fi

mkdir -p "${INSTALL_DIR}"
echo "Installing .NET SDK ${VERSION} to ${INSTALL_DIR}"

${FETCH} "https://dot.net/v1/dotnet-install.sh" \
  | bash /dev/stdin --version "${VERSION}" --install-dir "${INSTALL_DIR}"

if [[ ":${PATH}:" != *":${INSTALL_DIR}:"* ]]; then
  echo "Add to PATH: export PATH=\"${INSTALL_DIR}:\$PATH\""
fi

echo "dotnet version:"
if [[ -x "${INSTALL_DIR}/dotnet" ]]; then
  "${INSTALL_DIR}/dotnet" --version
else
  echo "dotnet not found in ${INSTALL_DIR}." >&2
  exit 4
fi
