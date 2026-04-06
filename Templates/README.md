# FrikaModFramework Templates

This directory contains starter templates to help you build mods and plugins on top of the **FrikaModFramework (FrikaMF)**.

## Available Templates

- `StandaloneModTemplate/`: A basic C# class library project pre-configured to build a standalone mod that depends on FrikaMF.
- `FMF.PluginTemplate/`: A starter template for creating plugins that extend the core framework's capabilities globally before mods are loaded.
- `UiTemplate/`: A React/Vite-based starter for building custom HTML/CSS/JS user interfaces that bridge with the game's UI through FrikaMF.

## How to use the Framework

The FrikaMF is loaded as a MelonLoader Plugin. This means it initializes early and provides foundational event hooks and routing for your mods.

To consume the framework in your mod:
1. Reference `FrikaModdingFramework.dll` in your `.csproj`.
2. Access the event dispatcher and hooks using the canonical `FFM.*` namespace (e.g., `FFM.Store.Cart.OnCheckedOut`).
3. (Optional) For native Rust mods, interface via the FFI bridge provided by the core layer.

## Creating a Plugin to Extend Framework Functions

If you want to add new framework-level capabilities (like a new global networking stack or a custom asset exporter) rather than a gameplay mod, you should create a Plugin:

1. Copy the `FMF.PluginTemplate/` to your workspace.
2. In your main entry class, inherit from `MelonPlugin` (instead of `MelonMod`).
3. This ensures your extension runs early in the MelonLoader lifecycle, allowing it to register its own global event handlers or modify engine settings before gameplay mods execute.
4. Distribute your compiled `.dll` to users, instructing them to place it in the `Plugins/` folder (or `Mods/` if explicitly supported as a late-binding plugin).

## Using the UI Template

The `UiTemplate` uses Vite and React.
1. Navigate to `Templates/UiTemplate/react-ui`.
2. Run `npm install` and `npm run dev`.
3. The UI uses `window.fmfBridge.invoke(action, payload)` to communicate with the C# layer (`DC2WebBridge.cs`).
4. To export your UI for mod distribution, run `npm run export:mod`. This will package the HTML/CSS/JS assets into a folder that can be bundled with your C# mod.