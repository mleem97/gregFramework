# Steam Workshop upload (steamcmd)

Requires [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD) on `PATH` or set `STEAMCMD_BIN` to the executable.

## Usage (Linux / macOS / Git Bash on Windows)

```bash
chmod +x upload.sh
export STEAM_USERNAME='your_account'
export STEAM_PASSWORD='your_password_or_sentry'
./upload.sh /path/to/workshop_item.vdf
```

Steam Guard may require a code at first login — run interactively.

## Windows

Use Git Bash or WSL to run `upload.sh`, or invoke `steamcmd` with the same arguments manually.

## Security

- Prefer **Steam Guard** and **limited** accounts for automation.
- Never commit credentials; use CI secrets if you automate uploads.
