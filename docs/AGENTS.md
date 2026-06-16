## Purpose
This file defines how multiple agents should work inside this repo against a shared `docs/SPEC.md`.
It is the operating contract for coordination, scope control, and reporting.

## Goals
- Let multiple agents work safely without duplicating effort or overwriting each other.
- Keep implementation aligned to the shared spec and current repo state.
- Favor small, reviewable changes with clear ownership and handoff.

## Scope
- This file defines agent behavior, coordination rules, and default workflow.
- `docs/SPEC.md` defines the work queue, task status, ownership, and acceptance intent.
- `docs/README.md` records brief user-facing summaries of completed work.
- The current code, scenes, data files, and tests are the final authority on implementation details when docs lag behind reality.

## Source Of Truth Order
When instructions conflict, use this order:
1. Direct user request.
2. Current task entry in `docs/SPEC.md`.
3. Existing code, tests, scenes, and data.
4. `docs/README.md`.
5. General preferences in this file.

If two sources conflict in a way that could cause a wrong implementation, stop and ask.

## Shared Principles
- Preserve user intent before pursuing optional improvements.
- Keep changes minimal, local, and reversible unless broader restructuring is requested.
- Prefer correctness, clear behavior, and passing validation over speed or extra polish.
- Do not silently refactor unrelated systems while completing a scoped task.
- Prefer extending existing data files, scene structure, and UI patterns before introducing parallel systems.
- Keep the project grounded in its current DOS/90s management-sim direction unless the user asks for a change.

## Multi-Agent Coordination
- Treat `docs/SPEC.md` as the shared queue, not as passive documentation.
- Every task should have a stable task ID and a status.
- Valid statuses are `todo`, `in_progress`, `blocked`, and `done`.
- An agent may claim only one task at a time unless the user explicitly requests broader ownership.
- To claim a task, update its status from `todo` to `in_progress` and fill in the owner field.
- Do not modify another agent's `in_progress` task unless the spec entry is explicitly handed off or the user asks for intervention.
- If a task is blocked, update the spec entry with a short blocker note and a handoff note so the next agent does not repeat the same dead end.
- When a task is complete, set it to `done`, clear any blocker note, and leave a short completion note plus a short handoff summary of the final state.
- Every task handoff should make it obvious to a fresh agent what was changed, how it was validated, what commit contains it, and whether any follow-up risk remains.

## Shared Git Workflow
- Work directly in the shared working branch the user has open unless the user explicitly asks for a separate branch workflow.
- Treat the current checked-out branch as the source of truth for ongoing work. Do not create, switch, merge, or delete branches unless the user explicitly asks.
- Keep each commit scoped to one spec task when practical. Do not bundle unrelated cleanup or future work into the same commit.
- Treat `docs/SPEC.md` as a coordination file first and an implementation artifact second. Avoid mixing broad queue reshuffling with code changes.
- If you need to claim a task, touch only that task block in `docs/SPEC.md`. Do not reorder unrelated tasks, rewrap completed sections, or clean up nearby formatting while claiming work.
- Prefer leaving the claim update and the implementation changes as separate commits when practical, but never at the expense of leaving the repo dirty between tasks.
- Do not resolve conflicts by deleting another agent's work. Preserve both sides, then narrow back down to the task boundary intentionally.
- Before marking a task `done`, make sure all task changes are committed and `git status --short` is clean.

## Clean Git Rules
- Before editing, inspect `git status --short` and note any unrelated workspace changes.
- Do not revert or overwrite files changed by another agent unless the user explicitly asks you to take over that work.
- Keep your diff limited to files required for the current task, plus the necessary `docs/SPEC.md` and `docs/README.md` updates.
- Before committing, review `git diff --stat` and `git status --short` to confirm there is no unrelated noise, generated output, or accidental formatting churn.
- Use clear commit messages that include the task ID, for example `feat: ART-06 player sprite pass`.
- The last step of every task is: commit the task work, update `docs/README.md`, update `docs/SPEC.md`, and confirm `git status --short` is clean before asking for another task.
- Stop after the task is complete. Do not stack extra speculative cleanup commits onto the same branch.

## Task Selection
- Default to the highest-priority `todo` item in `docs/SPEC.md`.
- Respect dependencies listed in the spec. Do not start dependent work early unless the user asks.
- If there is no `todo` item, do not invent new scope. Report that the queue is complete and ask whether to add more work.

## Definition Of Done
Use the smallest appropriate completion bar for the task type.

### Code Tasks
- Add or update a failing test first when the task is behavior-driven and the repo has a meaningful automated test path for it.
- Make the minimum implementation change required to pass.
- Run the relevant tests after the change.
- Update `docs/README.md` with a short note for the completed task.
- Update the spec status, completion note, validation note, and handoff note.
- Commit the completed task and leave the repo clean before treating the task as finished.

### Content Or Art Tasks
- Make only the scoped asset, scene, or tuning change required by the task.
- Run any validation needed for assets to resolve correctly.
- Verify the related scene or behavior still loads cleanly.
- Update `docs/README.md` with a short note for the completed task.
- Update the spec status, completion note, validation note, and handoff note.
- Commit the completed task and leave the repo clean before treating the task as finished.

### Docs Or Planning Tasks
- Keep changes concrete, accurate, and consistent with current repo state.
- Do not mark speculative ideas as completed implementation.
- Update `docs/README.md` when the docs change affects contributor or player-visible workflow.
- Update the spec status, completion note, validation note, and handoff note if the docs task itself is tracked there.
- Commit the completed task and leave the repo clean before treating the task as finished.

## Testing
- Use the existing project validation path when relevant, currently `godot --headless --script docs/test.gd`.
- Do not force unrelated tests to change just to satisfy a scoped task.
- If a task cannot reasonably have a failing test first, state why in the final report.
- If new art assets are added, run the needed Godot import or validation step before rerunning tests so resources resolve correctly.

## Change Boundaries
- Do not pull future spec work into the current task without user approval.
- If a requested fix overlaps multiple future tasks, implement only the smallest slice needed for the current item and note the follow-up.
- Do not change balance, progression pacing, or overall art direction unless the task requires it or the user approves it.
- When a temporary workaround is necessary, keep it explicit, small, and easy to replace later.

## When To Ask
- Ask before making broad structural changes or changing project-wide patterns.
- Ask when two reasonable implementation paths have meaningfully different long-term consequences.
- Ask when the user request conflicts with the current spec boundary.
- Ask when the spec and repo state disagree and the correct interpretation is not obvious.

## Forbidden Shortcuts
- Do not mark a task `done` if validation has not been run when validation is relevant.
- Do not combine multiple spec tasks into one implementation pass unless the user explicitly asks for that.
- Do not present placeholder art, incomplete behavior, or partial wiring as finished work without clearly labeling it.
- Do not overwrite another agent's claimed work silently.
- Do not leave temporary generated assets, editor artifacts, or unrelated workspace noise behind when it can be cleaned up safely.

## Reporting Format
After completing a scoped task, report:
- What changed.
- Which spec task ID was updated.
- Which commit contains the task.
- Whether validation ran and what passed or did not run.
- Any caveats, blockers, or follow-up notes.
- A short recommendation for the next best task.

Keep the report concise and tied to the completed task rather than giving a broad project recap.

## "Next" Workflow
When the user says `next`:
1. Open `docs/SPEC.md`.
2. Find the highest-priority task with status `todo` whose dependencies are satisfied.
3. Claim it by changing its status to `in_progress` and setting the owner.
4. Review the task acceptance intent and related repo files.
5. If the task is behavior-driven, write or update one failing test first when practical.
6. Implement only the scoped change needed for that task.
7. Run the relevant validation.
8. Update `docs/README.md` with a short note for the completed task.
9. Mark the task `done` in `docs/SPEC.md` and fill in completion, validation, commit, and handoff notes.
10. Commit the task and confirm `git status --short` is clean.
11. Stop and report results before taking another task.

## Reusable Multi-Agent Prompt
Use or adapt this prompt when launching a new agent for this repo:

```text
You are one of several agents working in parallel in this repository.

Your agent name: <agent-name>
Your assigned task ID: <task-id or "pick-next">

Follow this operating contract exactly:

1. Read `docs/AGENTS.md` first, then read `docs/SPEC.md`, then inspect only the repo files needed for your scoped task.
2. Use `docs/SPEC.md` as the shared queue. If your assignment is `pick-next`, choose the highest-priority `todo` task whose dependencies are satisfied.
3. Work on exactly one task. Do not combine tasks, do not pull in future scope, and do not refactor unrelated systems.
4. Before coding, confirm the task is not already owned by another agent. If it is, stop and report the conflict instead of proceeding.
5. Work in the currently checked-out shared branch unless the user explicitly asks for a branch. Do not create or switch branches on your own.
6. Keep git clean:
   - inspect `git status --short` before making changes
   - do not overwrite unrelated user or agent changes
   - touch only files required for your task
   - keep `docs/SPEC.md` edits minimal and limited to your task block
   - ensure the repo is clean again before asking for another task
7. If the task is behavior-driven and the repo supports it, add or update a failing test first.
8. Implement only the smallest change that satisfies the task acceptance criteria.
9. Run relevant validation, currently `godot --headless --script docs/test.gd`, when it applies.
10. When complete:
   - mark the task `done` in `docs/SPEC.md`
   - add short `Completion`, `Validation`, `Commit`, and `Handoff` notes
   - update `docs/README.md` for every completed task
   - commit the task work
   - review `git status --short` and `git diff --stat` for cleanliness
11. Report back with:
   - task ID
   - files changed
   - commit hash
   - validation run
   - blockers or caveats
   - recommended next task

Important guardrails:
- Never edit another agent's `in_progress` task without an explicit handoff.
- Never bundle queue cleanup, broad formatting, or opportunistic fixes into this branch.
- Never leave the repo in a dirtier state than required for your scoped task.
- If `docs/SPEC.md`, the codebase, and the task disagree, stop and report the mismatch before implementing.
```

## Spec Authoring Rules
To keep `docs/SPEC.md` reusable across agents:
- Give every task a stable ID like `ART-01` or `UI-03`.
- Include a status on every task.
- Keep each task independently completable where possible.
- List dependencies explicitly when order matters.
- Write acceptance intent in terms of observable outcomes, not implementation guesses.
- Prefer small tasks that can be completed and verified in one focused pass.
