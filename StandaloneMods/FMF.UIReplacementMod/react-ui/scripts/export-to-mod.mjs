import { mkdirSync, readFileSync, writeFileSync } from 'node:fs'
import { join, resolve } from 'node:path'

const projectRoot = resolve(process.cwd())
const modsAssetDir = resolve(projectRoot, '..', 'FMF.UIReplacementMod')

const htmlPath = join(projectRoot, 'src', 'App.tsx')
const cssPath = join(projectRoot, 'src', 'styles.css')
const tsxPath = join(projectRoot, 'src', 'App.tsx')

const defaultHtml = "<div id='root'><h1>Frika Modern UI</h1><p>React-inspired animated replacement layer active.</p></div>"
const css = readFileSync(cssPath, 'utf8')
const tsx = readFileSync(tsxPath, 'utf8')

mkdirSync(modsAssetDir, { recursive: true })
writeFileSync(join(modsAssetDir, 'react-app.html'), defaultHtml, 'utf8')
writeFileSync(join(modsAssetDir, 'react-app.css'), css, 'utf8')
writeFileSync(join(modsAssetDir, 'react-app.tsx'), tsx, 'utf8')

console.log('Exported React UI assets to:', modsAssetDir)
