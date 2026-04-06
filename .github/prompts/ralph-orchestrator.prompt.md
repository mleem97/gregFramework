# /dispatch — Ralph Orchestrator Prompt (non-agent reference)

This file is a non-agent reference prompt.
Do NOT add agent frontmatter here.

Primary runtime entries:
- `.github/agents/GregLoop.agent.md`
- `.github/agents/PlanPlus.agent.md`

## Operating Intent

Use this prompt as the orchestration reference for creating deeply atomic Ralph tasks.
The orchestrator does not implement production code directly.

## Phase 0 (Mandatory): Context Framing

1. Normalize request into explicit engineering outcomes.
2. Define constraints, non-goals, and completion contract.
3. Map affected files/functions/tests/docs.
4. Record risks, assumptions, and unknowns.
5. Use `#runSubagent` for deep recon when needed.

## Phase 1: Deep Atomic Planning

1. Build 3-10 phases with clear sequencing.
2. Split each phase into atomic tasks (one focused concern each).
3. Define dependencies and acceptance criteria per task.
4. Define TDD intent per task (red -> green -> verify).
5. Pause for user plan approval before dispatching.

## Phase 2: Ralph Dispatch

For each approved atomic task, create one `.todo` file:
- Path: `.ralph/tasks/[001-999]_[short_descriptive_name].todo`

Every `.todo` must include:
1. Task title
2. Objective
3. Scope (files/functions)
4. Constraints and non-goals
5. Ordered implementation steps
6. Tests to write first (expected initial failure)
7. Acceptance criteria checklist
8. Definition of done
9. Required worker output format

## Phase 3: Coordination and Recovery

1. Use `python script.py status` for queue inspection.
2. Revert stale `.lock` (>10 minutes) back to `.todo`.
3. Sync repository workflow-rule updates to:
	- `.github/copilot-instructions.md`
	- `.gemini/instructions.md`

## Mandatory Pause Points

1. After presenting plan synopsis.
2. After dispatch summary creation.

Do not continue without explicit user confirmation.
