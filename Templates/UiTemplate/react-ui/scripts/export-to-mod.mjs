import { mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

const projectRoot = resolve(process.cwd());
const runtimeDir = resolve(projectRoot, '..', '..', '..', 'StandaloneMods', 'FMF.UIReplacementMod', 'FMF.UIReplacementMod');

const html = `
<div class='fmf-shell'>
  <div class='fmf-shell__glow'></div>
  <div class='fmf-shell__copy'>
    <p class='eyebrow'>Frika Mod Framework</p>
    <h1>Modern React UI with C# bridge support</h1>
    <p>FMF can resolve the action labels below directly, letting the wrapper route main menu commands without extra glue.</p>
  </div>
  <div class='fmf-shell__actions'>
    <button>Continue</button>
    <button>New Game</button>
    <button>Multiplayer</button>
    <button>Settings</button>
    <button>Exit</button>
  </div>
</div>`;

const css = readFileSync(join(projectRoot, 'src', 'styles.css'), 'utf8');
const tsx = readFileSync(join(projectRoot, 'src', 'main.tsx'), 'utf8');

mkdirSync(runtimeDir, { recursive: true });
writeFileSync(join(runtimeDir, 'react-app.html'), html.trim(), 'utf8');
writeFileSync(join(runtimeDir, 'react-app.css'), css, 'utf8');
writeFileSync(join(runtimeDir, 'react-app.tsx'), tsx, 'utf8');

console.log(`Exported React UI assets to: ${runtimeDir}`);
