import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { dirname } from 'node:path';
const __dirname = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(__dirname, '..', '..', '..');
function loadMap() {
    const p = join(repoRoot, 'tools', 'fmf-hook-scanner', 'mapping', 'game2framework-map.json');
    const raw = readFileSync(p, 'utf8');
    return JSON.parse(raw);
}
function* walkFiles(dir, exts) {
    for (const name of readdirSync(dir)) {
        const full = join(dir, name);
        const st = statSync(full);
        if (st.isDirectory()) {
            if (name === 'node_modules' || name === 'build' || name === 'bin' || name === 'obj')
                continue;
            yield* walkFiles(full, exts);
        }
        else if (st.isFile()) {
            const dot = name.lastIndexOf('.');
            const ext = dot >= 0 ? name.slice(dot) : '';
            if (exts.has(ext))
                yield full;
        }
    }
}
function main() {
    const dryRun = !process.argv.includes('--apply');
    const map = loadMap();
    const keys = Object.keys(map.map);
    const patterns = keys.map((k) => ({ from: k, to: map.map[k] }));
    const hits = [];
    const scanRoots = [join(repoRoot, 'framework'), join(repoRoot, 'mods'), join(repoRoot, 'plugins')];
    const exts = new Set(['.cs']);
    for (const root of scanRoots) {
        if (!existsSync(root) || !statSync(root).isDirectory())
            continue;
        for (const file of walkFiles(root, exts)) {
            const lines = readFileSync(file, 'utf8').split(/\r?\n/);
            lines.forEach((line, idx) => {
                for (const { from, to } of patterns) {
                    if (line.includes(from))
                        hits.push({ file, from, to, line: idx + 1 });
                }
            });
        }
    }
    console.log(dryRun ? 'Dry run (use --apply to perform replacement — not implemented yet)' : 'Apply mode not implemented');
    console.log(`Mapping rules: ${patterns.length}`);
    console.log(`Hits: ${hits.length}`);
    for (const h of hits.slice(0, 50)) {
        console.log(`${h.file}:${h.line}  ${h.from} → ${h.to}`);
    }
    if (hits.length > 50)
        console.log(`… ${hits.length - 50} more`);
}
main();
