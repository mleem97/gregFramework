# Web UI Bridge (DC2WEB) EN

Last updated: 2026-04-03

This page describes the framework web-style UI system (`DC2WebBridge`) and the in-game `Mod Settings` menu integration.

## Overview

- Entry point: `FrikaMF/DC2WebBridge.cs`
- Mod menu: `FrikaMF/ModSettingsMenuBridge.cs`
- Hook integration: `FrikaMF/HarmonyPatches.cs`

Boundary:

- `DC2WebBridge` is a Unity-side UI/styling bridge.
- `DC2WebBridge` is not a generic HTTP/WebSocket FFI transport layer.

## What is currently supported

- UI styling from `HTML`/`CSS`
- Utility styling frameworks: `TailwindCSS`, `SASS`/`SCSS`
- Script-style sources: `JavaScript` / `TypeScript`
- React-oriented adapter path: `React JSX/TSX`
- Image types: `SVG` (preferred), `PNG`, `JPG/JPEG`, `BMP`, `GIF`, `TGA`

## In-game Mod Settings menu

Clicking `Settings` in the main menu opens a chooser:

- `Game Settings`
- `Mod Settings`

The `Mod Settings` panel can toggle runtime options:

- `DC2WEB Bridge enabled`
- `Unity UI Modernizer enabled`
- `Replace MainMenu Settings with Web overlay`

## Image support and SVG strategy

`DC2WebBridge` supports runtime sprite creation from `Dc2WebImageAsset`.

- Raster images are loaded through Unity `Texture2D.LoadImage(...)`.
- SVG follows a prioritized path and is rasterized at runtime to the target size.

Note: The internal SVG raster path is intentionally lightweight and focuses on common/simple shapes and fills. For advanced SVGs (complex paths, filters, masks), pre-bake assets in your mod pipeline and provide raster fallback.

## From basic HTML to React-style apps

The system is adapter-driven:

- `Basic HTML/CSS`: direct translation into a Unity style profile
- `Tailwind/SASS`: translated into CSS variables/properties
- `JS/TS`: heuristic extraction for style fields (`backgroundColor`, `color`, `fontSize`, ...)
- `React`: adapter reads `className` and inline style-like content and translates to a profile

### React support note

The framework does not embed a full browser DOM/JS runtime. DC2WEB uses translation adapters that map web-like source bundles to Unity UI profiles and overlays. For larger app-like UIs, use a precompiled descriptor (`Dc2WebAppDescriptor`) with clear style and asset contracts.

## Example: register a web app

```csharp
DC2WebBridge.RegisterWebApp(new Dc2WebAppDescriptor
{
    ScreenKey = "MainMenuReact",
    ReplaceExistingUi = true,
    Framework = "react-ts",
    Html = "<div id='root'><h1>DC2WEB React UI</h1><p>Runtime-translated app skin</p></div>",
    Css = ":root{--panel-color:#111827dd;--text-color:#f9fafb;--accent:#60a5fa;}",
    Script = "const App = () => <div className='bg-slate-900 text-slate-100 text-3xl'>React UI</div>;",
});
```

## Recommended workflow

1. Start with a simple `HTML/CSS` bundle.
2. Add `Tailwind/SASS` sources as needed.
3. Use `Dc2WebImageAsset` for icons/graphics (SVG-first).
4. For larger UI flows, move to app descriptor bundles (`React/TS`) with design tokens.
5. Tune behavior in-game through `Mod Settings`.

## Related links

- [Framework Features & Use Cases](/wiki/wiki-import/Framework-Features-Use-Cases)
- [FFI Bridge Reference](/wiki/wiki-import/FFI-Bridge-Reference)
- [Mod-Developer (Debug) EN](Mod-Developer-Debug-en)
- [Contributors (Debug) EN](Contributors-Debug-en)
- [Web UI Bridge (DC2WEB)](/wiki/wiki-import/Web-UI-Bridge)
