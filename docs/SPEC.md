## Spec

This file is the shared task queue for all agents working in this repo. Implementation targets Unity and C# as described in `docs/TDD`. The Unity 2D foundation uses Unity 6000.6.9f1. Gameplay implementation follows the task queue below.

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
- Status: `done`
- Owner: `codex`
- Depends On: `none`
- Summary: Create the Unity 2D project at the repository root and record the editor version.
- Acceptance: A clean checkout opens, imports, and compiles in the selected Unity 6000.6.9f1 supported release recorded in `ProjectSettings/ProjectVersion.txt`. Track package manifests and lockfiles and asset `.meta` files. Ignore generated Unity directories such as Library, Temp, Logs, and Obj.
- Completion: Initialized the root Unity 2D project with 2D defaults, Sprite tooling, uGUI, Test Framework, tracked folder metadata, package lockfile, and generated-directory ignores. Updated version guidance per user approval.
- Validation: Unity 6000.6.9f1 batch import and package compilation passed in the workspace and a clean staged-source copy (exit 0). Clean import preserved source files after line-ending normalization; generated folders are ignored. No gameplay tests exist for this setup-only task. Unity recovered automatically from a Hub licensing protocol mismatch by launching its matching licensing client.
- Commit: `26d3a94` (implementation and version docs); claim: `ea2bf3a`.
- Handoff: Open the repository root with Unity 6000.6.9f1. Foundation only; no playable scene or gameplay scripts. Next: FOUND-02 creates Main and the HUD shell.

### FOUND-02 Create the main scene and HUD shell
- Status: `done`
- Owner: `codex`
- Depends On: `FOUND-01`
- Summary: Create `Main.unity` with a GameRoot, orthographic camera, WorldView root, and anchored HUD on a Canvas with a Canvas Scaler.
- Acceptance: Opening Main in Play Mode shows a visible HUD without scene errors, and the HUD stays anchored when the Game view is resized.
- Completion: Added Main.unity with GameRoot, WorldView, an orthographic camera, and a screen-space HUD with Canvas Scaler and anchored top/status bars. Registered Main in build settings and documented opening the scene and running tests.
- Validation: Added a Play Mode test first and confirmed failure without Main. Final Editor Play Mode suite passed (1/1); standalone Windows Play Mode suite passed (1/1), verifying actual 1280x720, 800x600, and 1920x1080 window sizes, HUD bounds, anchors, labels, and no unexpected scene logs. Visually inspected screenshots at all three sizes.
- Commit: `e63d9c0` (scene, tests, build settings, README); claim: `0da0960`.
- Handoff: Open Assets/Scenes/Main.unity and enter Play Mode. HUD values are explicit placeholders; GameRoot is an empty composition root. No world art or simulation bindings yet. Next: FOUND-03 implements the plain C# clock; FOUND-04 connects it to the HUD.

### FOUND-03 Implement the C# simulation clock
- Status: `in_progress`
- Owner: `codex`
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
