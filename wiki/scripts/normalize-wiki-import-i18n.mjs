/**
 * Split legacy wiki-import pairs (Base.md = DE, Base-en.md = EN) into:
 * - docs/wiki-import/**           → default locale (en), Docusaurus
 * - wiki/i18n/de/docusaurus-plugin-content-docs/current/wiki-import/** → Deutsch
 *
 * German-only pages (no Base-en.md) → DE in i18n/de, EN stub in docs/wiki-import.
 *
 * Usage (from repo root):
 *   node wiki/scripts/normalize-wiki-import-i18n.mjs
 *   node wiki/scripts/normalize-wiki-import-i18n.mjs --dry-run
 */

import {
  existsSync,
  mkdirSync,
  readFileSync,
  readdirSync,
  rmSync,
  statSync,
  writeFileSync,
} from 'node:fs';
import { basename, dirname, join, relative, resolve } from 'node:path';

const repoRoot = resolve(import.meta.dirname, '..', '..');
const docsWikiImport = join(repoRoot, 'docs', 'wiki-import');
const deWikiImport = join(
  repoRoot,
  'wiki',
  'i18n',
  'de',
  'docusaurus-plugin-content-docs',
  'current',
  'wiki-import',
);

const dryRun = process.argv.includes('--dry-run');

function englishStub(title) {
  return `---
title: ${title}
description: English translation pending; use the Deutsch locale for the full legacy page.
---

:::note
This page is available in **German** in the legacy wiki import. Use the language menu (**Deutsch**) for the full text, or contribute an English translation under \`docs/wiki-import\`.
:::

`;
}

function ensureDir(p) {
  mkdirSync(p, { recursive: true });
}

function walkMarkdownFiles(root, base = root) {
  const out = [];
  for (const name of readdirSync(root)) {
    const full = join(root, name);
    const st = statSync(full);
    if (st.isDirectory()) {
      out.push(...walkMarkdownFiles(full, base));
    } else if (st.isFile() && name.toLowerCase().endsWith('.md')) {
      out.push(relative(base, full).replace(/\\/g, '/'));
    }
  }
  return out;
}

function toEnSibling(rel) {
  if (!rel.endsWith('.md')) return null;
  const d = dirname(rel);
  const base = basename(rel, '.md');
  return d === '.' ? `${base}-en.md` : `${d}/${base}-en.md`;
}

function main() {
  if (!existsSync(docsWikiImport)) {
    console.error(`Missing ${docsWikiImport}`);
    process.exit(1);
  }

  const allRel = new Set(walkMarkdownFiles(docsWikiImport));
  const pairBases = new Set();
  const germanOnly = [];

  for (const rel of allRel) {
    const file = basename(rel);
    if (file.endsWith('-en.md')) continue;

    const enRel = toEnSibling(rel);
    if (enRel && allRel.has(enRel)) {
      pairBases.add(rel);
      continue;
    }

    germanOnly.push(rel);
  }

  // Paired: Base.md + Base-en.md
  for (const relBase of pairBases) {
    const enRel = toEnSibling(relBase);
    const pathBase = join(docsWikiImport, relBase);
    const pathEn = join(docsWikiImport, enRel);
    const german = readFileSync(pathBase, 'utf8');
    const english = readFileSync(pathEn, 'utf8');
    const deTarget = join(deWikiImport, relBase);
    const enTarget = join(docsWikiImport, relBase);

    if (dryRun) {
      console.log(`[pair] ${relBase} + ${enRel} → EN docs + DE i18n`);
      continue;
    }

    ensureDir(dirname(deTarget));
    writeFileSync(deTarget, german, 'utf8');
    writeFileSync(enTarget, english, 'utf8');
    rmSync(pathEn);
    console.log(`[pair] ${relBase}`);
  }

  // English-only sibling (Home-en.md without Home.md) — rare
  for (const rel of allRel) {
    if (!rel.endsWith('-en.md')) continue;
    const baseRel = rel.replace(/-en\.md$/, '.md');
    if (pairBases.has(baseRel)) continue;

    const pathEn = join(docsWikiImport, rel);
    const pathBase = join(docsWikiImport, baseRel);
    const english = readFileSync(pathEn, 'utf8');

    if (dryRun) {
      console.log(`[en-only] ${rel} → ${baseRel}`);
      continue;
    }

    ensureDir(dirname(pathBase));
    writeFileSync(pathBase, english, 'utf8');
    rmSync(pathEn);
    console.log(`[en-only] ${rel} → ${baseRel}`);
  }

  // German-only: no Base-en.md (pair bases are excluded from germanOnly)
  for (const rel of germanOnly) {
    const pathBase = join(docsWikiImport, rel);
    const german = readFileSync(pathBase, 'utf8');
    const deTarget = join(deWikiImport, rel);
    const titleMatch = german.match(/^title:\s*(.+)$/m);
    const title = titleMatch ? titleMatch[1].trim().replace(/"/g, '\\"') : basename(rel, '.md');

    if (dryRun) {
      console.log(`[de-only] ${rel} → i18n/de + EN stub`);
      continue;
    }

    ensureDir(dirname(deTarget));
    writeFileSync(deTarget, german, 'utf8');
    writeFileSync(pathBase, englishStub(title), 'utf8');
    console.log(`[de-only] ${rel}`);
  }

  console.log(dryRun ? 'Dry run complete.' : 'Done. Next: cd wiki && npm run build');
}

main();
