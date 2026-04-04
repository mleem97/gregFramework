# Web UI Bridge (DC2WEB)

Last updated: 2026-04-03

Diese Seite beschreibt das neue Web-UI-System im Framework (`DC2WebBridge`) und das `Modsettings`-Menü.

## Überblick

- Einstiegspunkt: `FrikaMF/DC2WebBridge.cs`
- Mod-Menü: `FrikaMF/ModSettingsMenuBridge.cs`
- Hook-Integration: `FrikaMF/HarmonyPatches.cs`

Abgrenzung:

- `DC2WebBridge` ist eine UI-/Styling-Brücke innerhalb Unity.
- `DC2WebBridge` ist keine generische HTTP/WebSocket-FFI-Transportebene.

## Was jetzt möglich ist

- UI-Styling aus `HTML`/`CSS`
- Utility-Frameworks: `TailwindCSS`, `SASS`/`SCSS`
- Scriptbasierte Styles: `JavaScript` / `TypeScript`
- React-orientierter Adapterpfad: `React JSX/TSX`
- Bildtypen: `SVG` (bevorzugt), `PNG`, `JPG/JPEG`, `BMP`, `GIF`, `TGA`

## Modsettings Menü (im Spiel)

Beim Klick auf `Settings` im Main Menu öffnet sich ein Auswahlfenster:

- `Game Settings`
- `Mod Settings`

Im `Mod Settings`-Fenster können Framework-Optionen zur Laufzeit gesteuert werden:

- `DC2WEB Bridge aktiv`
- `Unity UI Modernizer aktiv`
- `MainMenu Settings per Web-Overlay ersetzen`

## Bild-Support und SVG-Strategie

`DC2WebBridge` unterstützt direkte Sprite-Erzeugung über `Dc2WebImageAsset`.

- Rasterbilder werden mit Unity `Texture2D.LoadImage(...)` geladen.
- SVG hat einen priorisierten Pfad und wird zur Laufzeit in beliebiger Zielgröße rasterisiert.

Wichtig: Der interne SVG-Rasterizer ist bewusst leichtgewichtig und deckt primär einfache Shapes/Fills ab. Für komplexe SVGs (viele Pfade/Filter/Masken) sollte ein Preprocess/Bake-Schritt in der Mod-Pipeline genutzt werden.

## Von Basic HTML bis React-Apps

Das System arbeitet adapterbasiert:

- `Basic HTML/CSS`: direkte Übernahme in ein Unity-Style-Profil
- `Tailwind/SASS`: Übersetzung in CSS-Variablen/Properties
- `JS/TS`: Heuristiken für Stilfelder (`backgroundColor`, `color`, `fontSize`, ...)
- `React`: `ReactAdapter` liest `className`/Inline-Styles und übersetzt sie in ein Profil

### Hinweis zur React-Unterstützung

Es wird **kein voller Browser/DOM/JS-Runtime-Stack** eingebettet. Stattdessen nutzt DC2WEB einen Übersetzungspfad auf Unity-UI-Stile und Overlays. Für komplexe Apps ist der empfohlene Weg ein precompiled App-Descriptor (`Dc2WebAppDescriptor`) mit klaren Styling-/Asset-Contracts.

## Beispiel: Web-App registrieren

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

## Empfohlener Workflow

1. Start mit einfachem `HTML/CSS` Bundle.
2. Bei Bedarf `Tailwind/SASS`-Quellen ergänzen.
3. `Dc2WebImageAsset` für Icons/Grafiken nutzen (SVG-first).
4. Für größere UIs: App-Descriptor (React/TS) plus klare Design-Tokens.
5. Im Spiel über `Mod Settings` live prüfen und austarieren.

## Relevante Querverweise

- [Framework Features & Use Cases](Framework-Features-Use-Cases)
- [FFI Bridge Reference](FFI-Bridge-Reference)
- [Mod-Developer (Debug)](Mod-Developer-Debug)
- [Contributors (Debug)](Contributors-Debug)
- [Web UI Bridge (DC2WEB) EN](Web-UI-Bridge-en)
