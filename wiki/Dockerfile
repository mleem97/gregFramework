# Build context: repository root (not wiki/).
# Development: runs Docusaurus dev server with hot reload.
# Production: use the root Dockerfile instead (static build + MCP).
FROM node:20-alpine
WORKDIR /app
ENV NODE_OPTIONS="--max-old-space-size=4096"
COPY wiki/package.json wiki/package-lock.json ./
RUN npm ci --ignore-scripts
COPY wiki/ ./
COPY docs/ /docs/
EXPOSE 3000
CMD ["npm", "run", "start", "--", "--host", "0.0.0.0", "--port", "3000"]
