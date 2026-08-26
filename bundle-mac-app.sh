#!/usr/bin/env bash
# Wrap a self-contained publish folder into a double-clickable .app (Avalonia macOS docs).
# Usage:
#   ./bundle-mac-app.sh                 # default RID osx-arm64
#   ./bundle-mac-app.sh osx-x64
#   ./bundle-mac-app.sh /path/to/publish
#
# Output name: "Your Journey Editor v{appVersion} {Apple Silicon|Intel}.app"
# Version is read from Utils.appVersion in JiME/Common/Common.cs.
#
# Expects publish output from ./publish-mac.sh (or equivalent).
# Icon: packaging/macos/ring.icns (from ring.ico via packaging/macos/make-ring-icns.py).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
APP_BASE_NAME="Your Journey Editor"
EXEC_NAME="JiMEditor.Avalonia"
PACKAGING="$ROOT/packaging/macos"
INFO_PLIST="$PACKAGING/Info.plist"
ICON_ICNS="$PACKAGING/ring.icns"
COMMON_CS="$ROOT/src/JiMEditor.Avalonia/JiME/Common/Common.cs"

read_app_version() {
	local ver
	ver="$(sed -nE 's/.*appVersion[[:space:]]*=[[:space:]]*"([^"]+)".*/\1/p' "$COMMON_CS" | head -n 1)"
	if [[ -z "$ver" ]]; then
		echo "Could not read appVersion from $COMMON_CS" >&2
		exit 1
	fi
	echo "$ver"
}

# Friendly arch label for the .app name (and Finder).
# Apple Silicon = arm64 (M1/M2/M3/…); Intel = x64.
arch_label_for_rid() {
	case "$1" in
		osx-arm64) echo "Apple Silicon" ;;
		osx-x64) echo "Intel" ;;
		*)
			echo "Unknown RID for arch label: $1" >&2
			exit 1
			;;
	esac
}

infer_rid_from_publish() {
	local publish="$1"
	local parent
	parent="$(basename "$(cd "$publish/.." && pwd)")"
	case "$parent" in
		osx-arm64|osx-x64) echo "$parent" ;;
		*)
			echo "Cannot infer RID from publish path (expected …/osx-arm64/publish or …/osx-x64/publish): $publish" >&2
			exit 1
			;;
	esac
}

resolve_publish_and_rid() {
	local arg="${1:-osx-arm64}"
	if [[ -d "$arg" && -f "$arg/$EXEC_NAME" ]]; then
		PUBLISH="$arg"
		RID="$(infer_rid_from_publish "$PUBLISH")"
		return
	fi
	case "$arg" in
		osx-arm64|osx-x64)
			RID="$arg"
			PUBLISH="$ROOT/src/JiMEditor.Avalonia/bin/Release/net9.0/$arg/publish"
			;;
		*)
			echo "Usage: $0 [osx-arm64|osx-x64|/path/to/publish]" >&2
			exit 1
			;;
	esac
}

resolve_publish_and_rid "${1:-osx-arm64}"

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

VERSION="$(read_app_version)"
ARCH_LABEL="$(arch_label_for_rid "$RID")"
APP_NAME="${APP_BASE_NAME} v${VERSION} ${ARCH_LABEL}"

# Place the .app next to the publish folder
# (e.g. …/osx-arm64/Your Journey Editor v0.50 Apple Silicon.app).
PARENT="$(cd "$PUBLISH/.." && pwd)"
APP_DIR="${PARENT}/${APP_NAME}.app"
rm -rf "$APP_DIR"
mkdir -p "$APP_DIR/Contents/MacOS" "$APP_DIR/Contents/Resources"

# Stamp version into Info.plist for Finder Get Info / About.
sed \
	-e "s|<string>1.0.0</string>|<string>${VERSION}</string>|g" \
	-e "s|<string>Journey Editor</string>|<string>${APP_BASE_NAME}</string>|" \
	-e "s|<string>Your Journey Editor</string>|<string>${APP_NAME}</string>|" \
	"$INFO_PLIST" > "$APP_DIR/Contents/Info.plist"

cp "$ICON_ICNS" "$APP_DIR/Contents/Resources/ring.icns"
# Avalonia docs: copy entire publish output into Contents/MacOS/
cp -a "$PUBLISH"/. "$APP_DIR/Contents/MacOS/"
chmod +x "$APP_DIR/Contents/MacOS/$EXEC_NAME" || true

echo "Bundled: $APP_DIR"
echo "Double-click in Finder, or: open \"$APP_DIR\""
echo "Note: unsigned builds may need Right-click → Open the first time (Gatekeeper)."
