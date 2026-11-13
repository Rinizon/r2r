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
- Follow the claim and handoff procedure in [AGENTS.md](AGENTS.md). Check for overlapping edits before claiming work.
- Stage only task-owned changes and verify that pre-existing unrelated changes are preserved.
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

### FOUND-01 Initialize the Unity 2D project
- Status: `todo`
- Owner: `unassigned`
- Depends On: `none`
- Summary: Create the Unity 2D project at the repository root and record the editor version.
- Acceptance: A clean checkout opens, imports, and compiles in the chosen Unity LTS version recorded in `ProjectSettings/ProjectVersion.txt`. Track package manifests and lockfiles and asset `.meta` files. Ignore generated Unity directories such as Library, Temp, Logs, and Obj.
- Completion:
- Validation:
- Commit:
- Handoff:

### FOUND-02 Create the main scene and HUD shell
- Status: `todo`
- Owner: `unassigned`
- Depends On: `FOUND-01`
- Summary: Create `Main.unity` with a GameRoot, orthographic camera, WorldView root, and anchored HUD on a Canvas with a Canvas Scaler.
- Acceptance: Opening Main in Play Mode shows a visible HUD without scene errors, and the HUD stays anchored when the Game view is resized.
- Completion:
- Validation:
- Commit:
- Handoff:

### FOUND-03 Implement the C# simulation clock
- Status: `todo`
- Owner: `unassigned`
- Depends On: `FOUND-01`
- Summary: Implement a plain C# clock that tracks the simulation date and advances with elapsed time at pause, 1x, 2x, 4x, or 8x.
- Acceptance: Edit Mode tests verify that equal elapsed time produces proportional advancement at each running speed, pause preserves the date and partial-day progress, and one update crossing multiple month boundaries and a year boundary emits each expected tick exactly once in the order defined in the TDD.
- Completion:
- Validation:
- Commit:
- Handoff:

### FOUND-04 Connect the clock to the HUD and verify Phase 1
- Status: `todo`
- Owner: `unassigned`
- Depends On: `FOUND-02`, `FOUND-03`
- Summary: Have GameRoot drive the clock and bind the HUD date and speed controls to the session.
- Acceptance: In Main Play Mode, the HUD shows the starting date and selected speed. Each speed control changes the rate of date advancement, pause freezes the date, and resuming continues from the same simulation state. Verify the interaction in Play Mode.
- Completion:
- Validation:
- Commit:
- Handoff:
