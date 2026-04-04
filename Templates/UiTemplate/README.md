# UI Template (React/Vite)

Starter template for FMF UI replacement mods.

- Source workspace: `react-ui/`
- Runtime export target: `FMF.UIReplacementMod/react-app.*`
- Browser bridge: `window.fmfBridge.invoke(action, payload)` with fallback `postMessage`

## Bridge contract

The UI keeps the action labels aligned with the C# wrapper lookup used by `DC2WebBridge`:

- `Continue`
- `New Game`
- `Multiplayer`
- `Settings`
- `Exit`

The template also exposes a `Sync State` command for future C# snapshot refresh flows.
