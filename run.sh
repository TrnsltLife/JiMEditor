#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
dotnet run --project "$ROOT/src/JiMEditor.Avalonia/JiMEditor.Avalonia.csproj"
