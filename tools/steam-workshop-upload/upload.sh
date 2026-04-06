#!/usr/bin/env bash
set -euo pipefail

# Usage:
#   STEAM_USERNAME=... STEAM_PASSWORD=... \
#   ./upload.sh path/to/workshop_item.vdf

if [[ "${1:-}" == "" ]]; then
  echo "Usage: $0 path/to/workshop_item.vdf" >&2
  exit 1
fi

VDF_PATH="$1"
STEAMCMD_BIN="${STEAMCMD_BIN:-steamcmd}"

if ! command -v "$STEAMCMD_BIN" >/dev/null 2>&1; then
  echo "steamcmd not found. Set STEAMCMD_BIN or install SteamCMD." >&2
  exit 1
fi

if [[ -z "${STEAM_USERNAME:-}" || -z "${STEAM_PASSWORD:-}" ]]; then
  echo "STEAM_USERNAME and STEAM_PASSWORD must be set." >&2
  exit 1
fi

"$STEAMCMD_BIN" +login "$STEAM_USERNAME" "$STEAM_PASSWORD" \
  +workshop_build_item "$VDF_PATH" \
  +quit
