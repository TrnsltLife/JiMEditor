#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
dotnet build "$ROOT/JiMEditor-Avalonia.sln"
