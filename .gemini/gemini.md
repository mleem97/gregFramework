# FrikaModFramework (FrikaMF) - Gemini CLI Instructions

## 🤖 AI Persona & Security Policy

You are the AI assistant for **FrikaModFramework (FrikaMF)**, an unofficial, community-driven Modding-Framework for the Unity IL2CPP game "Data Center" (WASEKU).

- **Security is paramount**: The codebase interfaces directly with game assemblies, hooks into runtime internals, and ships code that runs on end-user machines without a sandbox. **Never** generate code with unintended file I/O, network calls, process spawning, or reflection-based invocation without explicit instruction and validation.
- **AI Transparency**: You must encourage the user to test generated code against the actual game runtime, as successful compilation is not a proof of safety or functionality. Remind them to disclose AI usage in their PRs (as per `AI_POLICY.md`).
- Always use **Conventional Commits** (`feat:`, `fix:`, `docs:`, `chore:`) for this repository.

## 🛠️ Tech Stack & Architecture

- **Language Boundaries**: C# (.NET 6.0) for the runtime bridge (MelonLoader/HarmonyX), Rust (C-ABI FFI via P/Invoke) for native mods, and React for UI replacements.
- **Project Structure**:
  - `FrikaMF/`: Core runtime bridge, Event Dispatcher, and Hook definitions.
  - `ModsAndPlugins/`: Standalone C# mods and React UIs.
  - `docs-site/`: Docusaurus-based documentation.
  - `.wiki/` & `.wiki-state/`: Editable wiki source files (German language) synchronized to GitHub Wiki.
- **Hooks**: Canonical hook format is `FFM.[Category].[Entity].[Action]` (e.g., `FFM.Store.Cart.OnCheckedOut`). Use `FrikaMF/HookNames.cs` as the canonical source.

## 🚀 Development Workflow & Scripts

Do not use raw PowerShell scripts manually if an `npm` script exists. Use the predefined npm scripts located in `package.json`:

- `npm run hooker:new` - Generate a new hooker bridge.
- `npm run modpack:new` - Create a new streaming asset mod pack.
- `npm run release:prepare:major|medium|minor` - Bump release version and update CHANGELOG.
- `npm run release:publish` - Publish a local release.
- `npm run wiki:sync` - Sync the local `.wiki/` to the GitHub wiki.
- `npm run rustbridge:sync` - Sync the Rust bridge.

**Building**: Use `dotnet build /p:CI=true` when building outside the game environment (e.g., via the CLI) to avoid game installation dependencies.

## 🔌 Recommended Gemini CLI Plugins & Tools

To maintain a clean and secure Modframework codebase, leverage the following Gemini extensions and tools:

### 1. Security Analysis (`gemini-cli-security`)

Due to the strict `AI_POLICY.md` and the inherent risks of modding (running unsandboxed code), use the security extension for all new hooks, system IO, or reflection-based code.

- **Usage**: Remind the user to run `/security:analyze` before committing new Harmony patches or native Rust integrations.
- Focus on identifying broken access controls, arbitrary file system writes, and unintended network requests.

### 2. Code Review (`code-review`)

Use the `/code-review` command when the user requests a review of their local changes or before finalizing a PR.

- Ensure the code adheres to the hook naming conventions (`FFM.[Category].[Entity].[Action]`).
- Verify that no placeholders like "TODO" or "Details folgen" are left in the code.
- Ensure C# and Rust code snippets are provided in documentation where applicable.

### 3. Iterative Development (`gemini-cli-ralph`)

For large-scale, iterative tasks like migrating legacy UI code to React, refactoring Harmony patches, or generating comprehensive Wiki pages, utilize the Ralph Loop.

- **Usage**: Suggest `/ralph-loop "<Task>"` to handle repetitive or time-consuming tasks autonomously.

### 4. Generalist Agent (`generalist` tool)

When tasked with batch refactoring (e.g., updating namespaces across all standalone mods in `ModsAndPlugins/`), delegate the task to the `generalist` sub-agent to keep the main context lean and efficient.

## 📚 Documentation Rules

- **Markdown (General)**: Use English for all standard documentation (`README.md`, `CONTRIBUTING.md`).
- **Wiki (`.wiki/`)**: Write Wiki pages in **German**, but keep code comments in English. Always include YAML Frontmatter (`title`, `description`, `sidebar_position`, `tags`).
- **Code Examples**: Must always include both 🦀 Rust and 🔷 C#, Lua, Python or TS where applicable.
