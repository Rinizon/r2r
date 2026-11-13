## Spec

This file is the shared task queue for all agents working in this repo. Implementation targets Unity and C# as described in `docs/TDD`. Only the design and planning documents exist so far. All implementation tasks start as `todo`.

## Status Legend
- `todo`: ready to claim
- `in_progress`: currently owned by one agent
- `blocked`: cannot proceed until the blocker is resolved
- `done`: completed and validated for the scoped task

## Coordination Rules
- Claim work by changing a task from `todo` to `in_progress` and setting `Owner`.
- Do not edit another agent's `in_progress` task without an explicit handoff or user direction.
- When work is finished, change `Status` to `done`, update the affected project docs, and fill in `Completion`, `Validation`, `Commit`, and `Handoff` notes.
- Ensure `git status --short` is clean before claiming or continuing the next task.
- Ensure `git status --short` is clean again after committing the finished task and before prompting for the next one.
- Use the current shared branch unless the user explicitly asks for a branch workflow.
- If all tasks are `done`, stop and ask the user to add or prioritize new work.

## Task Template

Use this format for all new tasks:

### TASK-ID Short Title
- Status: `todo`
- Owner: `unassigned`
- Depends On: `none`
- Summary: One sentence describing the scoped change.
- Acceptance: Observable outcome that confirms the task is complete.
- Completion:
- Validation:
- Commit:
- Handoff:

Notes:
- Use a stable ID like `UI-05`, `GAME-02`, or `ART-06`.
- Keep each task small enough for one focused pass.
- Set `Depends On` to one or more task IDs when order matters.
- Leave `Completion`, `Validation`, `Commit`, and `Handoff` blank until the task is finished.

## Active Queue

Add all new `todo`, `in_progress`, and `blocked` tasks in this section.
Keep the highest-priority ready work at the top so agents can claim it quickly.
