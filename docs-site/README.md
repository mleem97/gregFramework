# FrikaMF Docusaurus Site

This folder hosts the audience-first documentation site.

## Commands

```bash
npm install
npm run start
npm run build
npm run wiki:sync
```

## Docker Compose

From repository root:

```bash
docker compose up docs
```

Build static site:

```bash
docker compose run --rm docs-build
```
