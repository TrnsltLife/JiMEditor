#!/usr/bin/env bash
# Cross-publish a self-contained Mac build (works from Mac, Windows Git Bash, or CI).
# Requires: .NET 9 SDK.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
RID="${1:-osx-arm64}"
PROJECT="$ROOT/src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj"

case "$RID" in
  osx-arm64|osx-x64) ;;
  *)
    echo "Usage: $0 [osx-arm64|osx-x64]" >&2
    exit 1
    ;;
esac

dotnet publish "$PROJECT" \
  -c Release \
  -r "$RID" \
  --self-contained true \
  -p:UseAppHost=true

OUT="$ROOT/src/JiMEditor.Avalonia/bin/Release/net9.0/$RID/publish"
echo "Published: $OUT"
if [[ -f "$OUT/JiMEditor.Avalonia" ]]; then
  chmod +x "$OUT/JiMEditor.Avalonia" || true
fi
