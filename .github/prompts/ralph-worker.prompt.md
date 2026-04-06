# /ralph-worker — Task Execution Prompt (non-agent)

This is a plain prompt document, not an agent declaration.
Follow this execution loop:

1. Claim the alphabetically first `.todo` in `.ralph/tasks/`.
2. Rename `.todo` -> `.lock`.
3. Execute exactly that task.
4. Commit only touched files with an English Conventional Commit.
5. Rename `.lock` -> `.done`.
