#!/usr/bin/env bash
# Wrap a self-contained publish folder into a double-clickable .app (Avalonia macOS docs).
# Usage:
#   ./bundle-mac-app.sh                 # default RID osx-arm64
#   ./bundle-mac-app.sh osx-x64
#   ./bundle-mac-app.sh /path/to/publish
#
# Expects publish output from ./publish-mac.sh (or equivalent).
# Icon: packaging/macos/ring.icns (from ring.ico via packaging/macos/make-ring-icns.py).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
APP_NAME="Your Journey Editor"
EXEC_NAME="JiMEditor.Avalonia"
PACKAGING="$ROOT/packaging/macos"
INFO_PLIST="$PACKAGING/Info.plist"
ICON_ICNS="$PACKAGING/ring.icns"

resolve_publish_dir() {
	local arg="${1:-osx-arm64}"
	if [[ -d "$arg" && -f "$arg/$EXEC_NAME" ]]; then
		echo "$arg"
		return
	fi
	case "$arg" in
		osx-arm64|osx-x64)
			echo "$ROOT/src/JiMEditor.Avalonia/bin/Release/net9.0/$arg/publish"
			;;
		*)
			echo "Usage: $0 [osx-arm64|osx-x64|/path/to/publish]" >&2
			exit 1
			;;
	esac
}

PUBLISH="$(resolve_publish_dir "${1:-osx-arm64}")"
if [[ ! -f "$PUBLISH/$EXEC_NAME" ]]; then
	echo "Publish output not found: $PUBLISH/$EXEC_NAME" >&2
	echo "Run ./publish-mac.sh first (or pass a publish directory)." >&2
	exit 1
fi

if [[ ! -f "$INFO_PLIST" ]]; then
	echo "Missing $INFO_PLIST" >&2
	exit 1
fi

if [[ ! -f "$ICON_ICNS" ]]; then
	if [[ "$(uname -s)" == "Darwin" ]]; then
		echo "Generating ring.icns from ring.ico…"
		python3 "$PACKAGING/make-ring-icns.py"
	else
		echo "Missing $ICON_ICNS — generate on a Mac with: python3 packaging/macos/make-ring-icns.py" >&2
		exit 1
	fi
fi

# Place the .app next to the publish folder (…/osx-arm64/Your Journey Editor.app).
APP_DIR="$(cd "$PUBLISH/.." && pwd)/${APP_NAME}.app"
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS" "$APP_DIR/Contents/Resources"

cp "$INFO_PLIST" "$APP_DIR/Contents/Info.plist"
cp "$ICON_ICNS" "$APP_DIR/Contents/Resources/ring.icns"
# Avalonia docs: copy entire publish output into Contents/MacOS/
cp -a "$PUBLISH"/. "$APP_DIR/Contents/MacOS/"
chmod +x "$APP_DIR/Contents/MacOS/$EXEC_NAME" || true

echo "Bundled: $APP_DIR"
echo "Double-click in Finder, or: open \"$APP_DIR\""
echo "Note: unsigned builds may need Right-click → Open the first time (Gatekeeper)."
