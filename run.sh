#!/usr/bin/env bash
# Run the Avalonia editor (builds if needed).
#   ./run.sh           → Debug (default)
#   ./run.sh --release → Release
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
			echo "  (default)   Run Debug configuration"
			echo "  --release   Run Release configuration"
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

echo "Running configuration: $CONFIG"
dotnet run --project "$ROOT/src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj" -c "$CONFIG"
