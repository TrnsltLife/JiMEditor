#!/usr/bin/env bash
# Cross-publish a self-contained Mac build, then wrap it in a double-clickable .app.
# Requires: .NET 9 SDK. App bundling works on any OS; ring.icns generation needs a Mac once.
#
#   ./publish-mac.sh              # osx-arm64 + Your Journey Editor v{ver} Apple Silicon.app
#   ./publish-mac.sh osx-x64      # … v{ver} Intel.app
#   ./publish-mac.sh --no-bundle  # publish folder only
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
RID="osx-arm64"
BUNDLE=1

for arg in "$@"; do
	case "$arg" in
		--no-bundle)
			BUNDLE=0
			;;
		osx-arm64|osx-x64)
			RID="$arg"
			;;
		-h|--help)
			echo "Usage: $0 [osx-arm64|osx-x64] [--no-bundle]"
			exit 0
			;;
		*)
			echo "Unknown argument: $arg" >&2
			echo "Usage: $0 [osx-arm64|osx-x64] [--no-bundle]" >&2
			exit 1
			;;
	esac
done

PROJECT="$ROOT/src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj"

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

if [[ "$BUNDLE" -eq 1 ]]; then
	"$ROOT/bundle-mac-app.sh" "$RID"
fi
