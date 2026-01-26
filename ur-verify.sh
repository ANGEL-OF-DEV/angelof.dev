#!/usr/bin/env sh
# ur-verify.sh | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
UR_ROOT=${MONOCOQUE_UR_ROOT:-"$SCRIPT_DIR/../[monocoque.ur]"}

echo "[verify] dotnet build (Release)"
dotnet build "$SCRIPT_DIR/Ur.Tool/Ur.Tool.csproj" -c Release

echo "[verify] dotnet test (Release)"
dotnet test --project "$SCRIPT_DIR/Ur.Tool/Ur.Tool.csproj" -c Release

echo "[verify] run verifier"
dotnet run --project "$SCRIPT_DIR/Ur.Tool/Ur.Tool.csproj" -c Release -- \
  --app verify all --ur-root "$UR_ROOT"

echo "[verify] OK"
