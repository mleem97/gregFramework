import { copyFileSync, mkdirSync, existsSync } from 'node:fs';
import { dirname, join } from 'node:path';

const root = process.cwd();
const distIndex = join(root, 'dist', 'index.html');
const outDir = join(root, '..', '..', 'FMF.UIReplacementMod');

mkdirSync(outDir, { recursive: true });

if (existsSync(distIndex)) {
  copyFileSync(distIndex, join(outDir, 'react-app.html'));
}
