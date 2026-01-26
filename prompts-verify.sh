#!/usr/bin/env sh
# prompts-verify.sh | LICENSED UNDER THE 𝗘𝗨𝗣𝗟 [eupl.eu/1.2/en]
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
PROMPTS_ROOT=${MONOCOQUE_PROMPTS_ROOT:-"$SCRIPT_DIR/../[monocoque.prompts]"}

echo "[verify] dotnet build (Release)"
dotnet build "$SCRIPT_DIR/PromptPack.Verify/PromptPack.Verify.csproj" -c Release

echo "[verify] dotnet test (Release)"
dotnet test --project "$SCRIPT_DIR/PromptPack.Verify/PromptPack.Verify.csproj" -c Release

echo "[verify] run verifier"
dotnet run --project "$SCRIPT_DIR/PromptPack.Verify/PromptPack.Verify.csproj" -c Release -- \
  --app verify-pack --prompts-root "$PROMPTS_ROOT"

echo "[verify] OK"
