# syntax=docker/dockerfile:1
# Single container: static Docusaurus (wiki build) + FrikaMF MCP (Streamable HTTP on /mcp).
# Build context: repository root.

FROM node:20-alpine AS wiki-build
WORKDIR /build
ENV NODE_OPTIONS="--max-old-space-size=4096"
COPY wiki/package.json wiki/package-lock.json ./
RUN npm ci --ignore-scripts
COPY wiki/ ./
COPY docs/ /docs/
RUN npm run build

FROM node:20-alpine
WORKDIR /app
ENV NODE_ENV=production
ENV FMF_MCP_DATA_ROOT=/app/data

COPY mcp-server/package.json mcp-server/package-lock.json ./
RUN npm ci --omit=dev && npm cache clean --force

COPY mcp-server/src ./src
COPY docs/ ./data/docs/
COPY CONTRIBUTING.md ./data/CONTRIBUTING.md
COPY FrikaModFramework/fmf_hooks.json ./data/fmf_hooks.json

COPY --from=wiki-build /build/build ./static

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -qO- http://localhost:3000/health || exit 1

EXPOSE 3000
CMD ["node", "src/index.mjs", "--http", "--host", "0.0.0.0", "--port", "3000", "--static", "/app/static", "--data-root", "/app/data"]
