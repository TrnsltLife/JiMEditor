#!/usr/bin/env bash
# Build the Avalonia solution.
#   ./build.sh           → Debug (default)
#   ./build.sh --release → Release
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"

CONFIG="Debug"
for arg in "$@"; do
	case "$arg" in
		--release|-cRelease|--configuration=Release)
			CONFIG="Release"
			;;
		-h|--help)
			echo "Usage: $0 [--release]"
			echo "  (default)   Debug build"
			echo "  --release   Release build"
			echo ""
			echo "For a self-contained Mac executable, use ./publish-mac.sh instead."
			exit 0
			;;
		*)
			echo "Unknown argument: $arg" >&2
			echo "Usage: $0 [--release]" >&2
			exit 1
			;;
	esac
done

echo "Building configuration: $CONFIG"
dotnet build "$ROOT/JiMEditor-Avalonia.sln" -c "$CONFIG"
