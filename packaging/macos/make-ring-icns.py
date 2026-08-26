#!/usr/bin/env python3
"""Build packaging/macos/ring.icns from src/JiMEditor.Avalonia/ring.ico (macOS: sips + iconutil)."""
from __future__ import annotations

import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ICO = ROOT / "src" / "JiMEditor.Avalonia" / "ring.ico"
OUT = ROOT / "packaging" / "macos" / "ring.icns"

# (pixel size, filename) — names must match iconutil's iconset rules exactly.
SIZES: list[tuple[int, str]] = [
	(16, "icon_16x16.png"),
	(32, "icon_16x16@2x.png"),
	(32, "icon_32x32.png"),
	(64, "icon_32x32@2x.png"),
	(128, "icon_128x128.png"),
	(256, "icon_128x128@2x.png"),
	(256, "icon_256x256.png"),
	(512, "icon_256x256@2x.png"),
	(512, "icon_512x512.png"),
	(1024, "icon_512x512@2x.png"),
]


def main() -> int:
	if sys.platform != "darwin":
		print("This converter requires macOS (sips + iconutil).", file=sys.stderr)
		return 1
	if not ICO.is_file():
		print(f"Missing icon: {ICO}", file=sys.stderr)
		return 1

	OUT.parent.mkdir(parents=True, exist_ok=True)
	with tempfile.TemporaryDirectory() as td:
		td_path = Path(td)
		base_png = td_path / "ring.png"
		iconset = td_path / "AppIcon.iconset"
		iconset.mkdir()
		subprocess.check_call(
			["sips", "-s", "format", "png", str(ICO), "--out", str(base_png)],
			stdout=subprocess.DEVNULL,
		)
		for size, name in SIZES:
			tmp = td_path / f"size_{size}.png"
			subprocess.check_call(
				["sips", "-z", str(size), str(size), str(base_png), "--out", str(tmp)],
				stdout=subprocess.DEVNULL,
			)
			dest = iconset / name
			shutil.copy2(tmp, dest)
			print(f"  {name} -> {dest.stat().st_size} bytes")

		names = sorted(p.name for p in iconset.iterdir())
		print("iconset files:", names)
		if len(names) != len(SIZES):
			print(f"Expected {len(SIZES)} files, got {len(names)}", file=sys.stderr)
			return 1

		subprocess.check_call(["iconutil", "-c", "icns", str(iconset), "-o", str(OUT)])

	print(f"Wrote {OUT}")
	return 0


if __name__ == "__main__":
	raise SystemExit(main())
