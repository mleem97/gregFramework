import { mkdirSync, readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join, resolve } from 'node:path';

const projectRoot = resolve(process.cwd());
const repoRoot = resolve(projectRoot, '..');
const wikiDir = join(repoRoot, '.wiki');
const outDir = join(projectRoot, 'docs', 'wiki-import');

mkdirSync(outDir, { recursive: true });

function collectMarkdownFiles(rootDir, prefix = '') {
  const entries = readdirSync(join(rootDir, prefix), { withFileTypes: true });
  const results = [];

  for (const entry of entries) {
    const relativePath = prefix ? join(prefix, entry.name) : entry.name;
    if (entry.isDirectory()) {
      results.push(...collectMarkdownFiles(rootDir, relativePath));
      continue;
    }

    if (entry.isFile() && entry.name.toLowerCase().endsWith('.md')) {
      results.push(relativePath);
    }
  }

  return results;
}

const files = collectMarkdownFiles(wikiDir);

for (const file of files) {
  const source = join(wikiDir, file);
  const sanitizedRelative = file.replace(/\s+/g, '-');
  const target = join(outDir, sanitizedRelative);
  const targetDir = resolve(target, '..');
  mkdirSync(targetDir, { recursive: true });
  const raw = readFileSync(source, 'utf8');
  writeFileSync(target, raw, 'utf8');
}

console.log(`Synced ${files.length} wiki files to ${outDir}`);
