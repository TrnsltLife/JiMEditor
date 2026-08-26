#!/bin/bash
# Clear Gatekeeper quarantine on the Your Journey Editor .app next to this script.
# Unsigned GitHub downloads are often blocked as "damaged"; this fixes that copy.
# Usage: unzip the release, then double-click this file (Right-click → Open if needed).
cd "$(dirname "$0")" || exit 1

shopt -s nullglob
apps=( "Your Journey Editor"*.app )

if (( ${#apps[@]} == 0 )); then
	echo "No 'Your Journey Editor'*.app found next to this script."
	echo "Put this file in the same folder as the app (after unzipping the release), then run it again."
	read -r -p "Press Enter to close…"
	exit 1
fi

for app in "${apps[@]}"; do
	echo "Clearing Gatekeeper quarantine on: $app"
	xattr -cr "$app"
done

echo
echo "Done. You can double-click the app now."
read -r -p "Press Enter to close…"
