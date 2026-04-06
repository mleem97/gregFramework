# AI Global Instructions — gregFramework (FrikaMF)

## 1. Project Context
FrikaModdingFramework (FrikaMF) is an unofficial, community-driven modding framework for the game "Data Center". It supports cross-language modding (C# and Rust).

## 2. The Golden Rule: ENGLISH ONLY
Absolutely EVERYTHING generated, modified, or written by the AI in this repository MUST be in English. 
- Source code (variables, methods, classes)
- Inline code comments
- Documentation (Markdown files, Wikis, READMEs)
- Commit messages
- AI Prompts and task descriptions
The only exception is if you are directly editing localization or translation mapping files.

## 3. Unified Ralph-Dispatcher Workflow
This repository has migrated to an asynchronous, file-based task management system located in the `.ralph/` directory. Do not generate static, single-purpose agents anymore.

**Available Commands for the User:**
* `/dispatch [Task Description]` -> Triggers the Orchestrator. It analyzes the request, breaks it down into small `.todo` files inside `.ralph/tasks/`, cleans up obsolete files, and syncs these instructions.
* `/ralph-worker` -> Triggers the Worker. It grabs exactly ONE `.todo` file, executes the code changes, creates an atomic conventional commit, and renames the file to `.done`.

## 3b. Live-Sync and `lib/references/`

This repository supports a **Live-Sync** workflow for *Data Center* updates: MelonLoader regenerates IL2CPP interop DLLs under the game install. After running `python tools/refresh_refs.py`, **`lib/references/MelonLoader/`** (when populated) is the **authoritative type surface** for AI-assisted C# edits and MSBuild (`FrikaMF.csproj` prefers it when `net6/MelonLoader.dll` exists). The live Steam game folder remains the runtime truth for executing the game. Use `tools/diff_assembly_metadata.py` to compare snapshots after updates.

## 4. AI Behavior: Auto-Sync & Maintenance
All AI instances must treat this file as the absolute Single Source of Truth. If the user defines a new operational rule, coding standard, or workflow step, the Orchestrator AI is strictly mandated to revise these instructions and immediately mirror the updates to BOTH `.github/copilot-instructions.md` AND `.gemini/instructions.md`. Keep the workspace clean of obsolete prompts.

## 5. Agent Boundary Rule (Single Agent Entry)
Only .github/agents/ralph-orchestrator.prompt.md may declare mode: 'agent'.
All files in .github/prompts/ must remain non-agent prompt documents.
