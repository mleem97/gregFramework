# /ralph-worker — Task Execution Prompt (non-agent reference)

This file is a non-agent reference prompt.
Do NOT add agent frontmatter here.

## Mission

Execute exactly one atomic Ralph task per run with strict scope discipline.

## Execution Protocol

1. **Claim**
	- Select alphabetically first `.todo` in `.ralph/tasks/`.

2. **Lock**
	- Rename `.todo` -> `.lock` immediately.

3. **Read and Frame**
	- Extract objective, scope, constraints, and acceptance criteria.
	- Reject out-of-scope changes.

4. **Implement with TDD Discipline**
	- Write/adjust tests first where required by task.
	- Run tests expecting initial failure (red).
	- Implement minimal code to satisfy acceptance criteria (green).
	- Run required checks again (verify).
	- Prefer building against **`lib/references/`** when present (after `tools/refresh_refs.py`); it is the authoritative interop surface for C# work in this repo.

5. **Quality Gate**
	- Confirm all acceptance criteria are satisfied.
	- Confirm no unrelated files were changed.

6. **Commit Atomically**
	- Commit only touched files for this task.
	- Use English Conventional Commit format.
	- For IL2CPP hook or interop fixes after a game update, prefer: `fix(game-update): fix hook for [ClassName]` (use the actual affected game or framework type name).

7. **Complete**
	- Rename `.lock` -> `.done`.
	- Output structured summary:
	  - Objective completed
	  - Files/functions/tests changed
	  - Checks executed and results

## Hard Rules

1. One task per run only.
2. No speculative refactors outside scope.
3. No edits to orchestration docs unless explicitly required by task.
4. English-only for code comments, docs, and commit messages.
