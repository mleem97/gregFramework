/** Idempotent: add unique Docusaurus sidebar keys for wiki-import subfolders. */
import fs from 'node:fs';
import path from 'node:path';

const pairs = [
  ['Contributors/Guides', 'wiki-import-contributors-guides'],
  ['Contributors/Reference', 'wiki-import-contributors-reference'],
  ['Contributors/Troubleshooting', 'wiki-import-contributors-troubleshooting'],
  ['EndUser/Guides', 'wiki-import-enduser-guides'],
  ['EndUser/Reference', 'wiki-import-enduser-reference'],
  ['EndUser/Troubleshooting', 'wiki-import-enduser-troubleshooting'],
  ['ModDevs/Guides', 'wiki-import-moddevs-guides'],
  ['ModDevs/Reference', 'wiki-import-moddevs-reference'],
  ['ModDevs/Troubleshooting', 'wiki-import-moddevs-troubleshooting'],
  ['TechnicalReference/Guides', 'wiki-import-techref-guides'],
  ['TechnicalReference/Reference', 'wiki-import-techref-reference'],
  ['TechnicalReference/Troubleshooting', 'wiki-import-techref-troubleshooting'],
];

const bases = [
  path.resolve(import.meta.dirname, '../../docs/wiki-import'),
  path.resolve(
    import.meta.dirname,
    '../i18n/de/docusaurus-plugin-content-docs/current/wiki-import',
  ),
];

for (const base of bases) {
  if (!fs.existsSync(base)) continue;
  for (const [rel, key] of pairs) {
    const dir = path.join(base, rel);
    if (!fs.existsSync(dir)) continue;
    const target = path.join(dir, '_category_.json');
    const body = JSON.stringify({ key }, null, 2) + '\n';
    fs.writeFileSync(target, body);
    console.log('wrote', target);
  }
}
