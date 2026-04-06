# Ralph Loop Prompt (non-agent reference)

This file is a non-agent reference prompt.
Do NOT add agent frontmatter here.

## Purpose

Provide safe operational loop behavior for Ralph queue maintenance.

## Loop Sequence

1. **Initialize Environment**
	- Run `python script.py` to ensure `.ralph/` structure exists.

2. **Inspect Queue State**
	- Run `python script.py status`.
	- Capture counts for `.todo`, `.lock`, `.done`.

3. **Detect Stale Locks**
	- Check `.ralph/tasks/*.lock` last write time.
	- Mark files older than 10 minutes as stale.

4. **Recover Stale Work**
	- Rename stale `.lock` -> `.todo`.
	- Record each recovered filename.

5. **Report Health Summary**
	- Return queue counts, recoveries made, and remaining blockers.

## Safety Rules

1. Never modify `.done` tasks.
2. Never delete task files during recovery.
3. Only perform lock requeue when staleness threshold is met.
4. Preserve deterministic alphabetical task ordering.
