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
- Status: `done`
- Owner: `codex`
- Depends On: `FOUND-01`
- Summary: Implement a plain C# clock that tracks the simulation date and advances with elapsed time at pause, 1x, 2x, 4x, or 8x.
- Acceptance: Edit Mode tests verify that equal elapsed time produces proportional advancement at each running speed, pause preserves the date and partial-day progress, and one update crossing multiple month boundaries and a year boundary emits each expected tick exactly once in the order defined in the TDD.
- Completion: Added engine-independent SimulationClock with Gregorian date, fractional day progress, pause and 1x/2x/4x/8x speeds, and ordered outgoing day/month/year events. Documented the API and added Edit Mode coverage.
- Validation: Confirmed the pre-implementation suite failed (12 failures). Final Unity 6000.6.9f1 Edit Mode suite passed 13/13 cases, covering proportional speed, pause/resume and speed changes, one update across 14 month ends and a year end including leap day, repeated small updates, and invalid inputs. No scene or HUD changes; Play Mode suite was not rerun.
- Commit: `b2523bd` (clock, tests, docs); claim: `97e73ca`.
- Handoff: FOUND-04 should reference InfrastructureTycoon.Simulation, create a SimulationClock in GameRoot, call Advance with real elapsed seconds, and bind Date and Speed to the HUD. Starts January 1, 1995 at Normal; two real seconds per day. Events carry the outgoing date; callbacks see the next date at zero day progress. No billing or persistence is implemented.

### FOUND-04 Connect the clock to the HUD and verify Phase 1
- Status: `done`
- Owner: `codex`
- Depends On: `FOUND-02`, `FOUND-03`
- Summary: Have GameRoot drive the clock and bind the HUD date and speed controls to the session.
- Acceptance: In Main Play Mode, the HUD shows the starting date and selected speed. Each speed control changes the rate of date advancement, pause freezes the date, and resuming continues from the same simulation state. Verify the interaction in Play Mode.
- Completion: GameRoot now creates and drives the simulation clock using real elapsed time. Main shows the date and selected speed, with Pause/1x/2x/4x/8x buttons and an EventSystem. Selected speed is highlighted; pause preserves session progress. Phase 1 exit criteria are verified and documented.
- Validation: Added failing Play Mode coverage first (2 failures). Final Unity 6000.6.9f1 suites passed: Edit Mode 13/13, Editor Play Mode 3/3, standalone Windows Play Mode 3/3. Verified starting date, all speed rates, actual UI raycast targets, date rollover, pause/resume without resetting fractional progress, and resized HUD at 1280x720, 800x600, and 1920x1080. Visually inspected screenshots at all sizes.
- Commit: `6b5a1bc` (implementation, scene, tests, docs); claim: `8ac1eb1`.
- Handoff: Phase 1 is complete. Open Main and enter Play Mode to use the time controls. Cash and alerts remain placeholders, and WorldView remains empty. No further todo tasks are queued; define scoped Phase 2 tasks from docs/phases before beginning hosting contracts, walking/station interaction, billing, or saves.

## Phase 2 Queue — First hosting contract (Era 1)

Work in the priority order below when dependencies are satisfied. Use the provisional starter values in `docs/GDD` and the architecture and save contract in `docs/TDD`. Keep upgrades, PhotoSnap growth, bottleneck alerts, tower servers, and customer satisfaction in their later phases. Garage visuals may be simple, clearly labeled prototype art; this phase requires playable interactions rather than a finished art pass.

### P2-01 Author starter hardware and website definitions
- Status: `done`
- Owner: `codex`
- Depends On: `FOUND-04`
- Summary: Add ScriptableObject definitions and a starter catalog for the PC and two website offers, with stable content IDs and conversion to engine-independent runtime data.
- Acceptance: Authored assets under Assets/Data define the starter PC (2 CPU cores, 4 GB RAM, 250 GB storage, 10 Mbps bandwidth, $20 monthly expense), PhotoSnap (1 core, 2 GB RAM, 20 GB storage, 4 Mbps, $100 monthly revenue), and the second website (1 core, 1 GB RAM, 10 GB storage, 4 Mbps, $80 monthly revenue). IDs are explicit and independent of asset names, paths, or instance IDs. Edit Mode validation rejects missing/duplicate IDs and invalid resource or money values. Definitions remain immutable during play; loading the catalog does not require scene-object references. Verify Unity imports all assets and metadata.
- Completion: Added HardwareDefinition, ContractTemplate, and StarterCatalog ScriptableObjects with authored PC/PhotoSnap/local-website assets matching the GDD. ToRuntime creates immutable engine-independent resource, hardware, offer, and catalog data with stable explicit IDs and decimal money. Added content validation and contributor guidance.
- Validation: Confirmed failing Edit Mode tests before implementation (11 failures). Final Unity 6000.6.9f1 Edit Mode suite passed 29/29 (16 catalog cases plus 13 clock cases); Play Mode regression suite passed 3/3. Verified asset import/reference resolution, exact starter values, missing/duplicate IDs, invalid resources/money, missing asset references, and runtime snapshot isolation from authored changes.
- Commit: `b547b96` (assets, definitions, runtime data, tests, README); claim: `01df260`.
- Handoff: P2-02 should reference Assets/Data/StarterCatalog.asset from GameRoot and pass ToRuntime() data into the new session. Content namespace is InfrastructureTycoon.Content; immutable data lives in the engine-independent Simulation assembly, ScriptableObjects in Runtime. IDs: hardware.starter-pc, offer.photosnap, offer.local-website. Money fields are authored as whole dollars. No session, workload assignment, or billing was added.

### P2-02 Create the starter company session and capacity model
- Status: `done`
- Owner: `codex`
- Depends On: `P2-01`
- Summary: Introduce a plain C# session owning the existing clock, company finances, starter hardware instance, and four-resource capacity state, and compose it from GameRoot.
- Acceptance: A new session starts January 1, 1995 at 1x with $150 cash, zero operating profit, one starter PC, and no accepted contracts. Hardware instances have stable runtime IDs distinct from content IDs. CPU, RAM, storage, and bandwidth expose capacity, used, and available values with consistent units. Edit Mode tests verify starter state, resource arithmetic, and independent sessions without mutating authored definitions. Main still loads and the existing date/speed controls and clock tests pass after GameRoot adopts the session; no purchases or progression systems are added.
- Completion: Added engine-independent CompanySession with starter finances, owned clock, one uniquely identified HardwareInstance, and an empty read-only accepted-contract collection. Added four-resource arithmetic and immutable capacity/used/available snapshots. GameRoot now composes the session from the authored catalog assigned in Main and preserves its Clock accessor.
- Validation: Confirmed five new Edit Mode tests failed before implementation. Final Unity 6000.6.9f1 Edit Mode suite passed 34/34 and Play Mode suite passed 4/4, covering starter state, resource arithmetic and bounds, independent sessions/runtime IDs, authored-data isolation, scene catalog binding, fresh scene sessions, and existing date/speed/pause controls.
- Commit: `cd7137e` (session, capacity, scene composition, tests, docs); claim: `a3fcb26`.
- Handoff: P2-03 should add offer acceptance and assignment operations to CompanySession and populate its currently empty contract collection. HardwareInstance.Id is a generated runtime ID distinct from Definition.Id; Resources tracks capacity and actual used demand, with Available clamped at zero. ResourceAmounts supports validated addition/subtraction. Cash and alerts remain HUD placeholders until P2-07; no acceptance, assignment, billing, purchases, or progression behavior was added.

### P2-03 Accept offers and assign website workloads
- Status: `done`
- Owner: `codex`
- Depends On: `P2-02`
- Summary: Add session operations to accept an available offer and assign its workload to the starter PC.
- Acceptance: Either authored offer can be accepted once, with an identifiable runtime contract and a separate assignment to the PC. Accepting an offer alone consumes no capacity and earns no revenue; assignment makes it hosted and consumes all four authored resources. Both starter offers fit together at 2 cores, 3 GB RAM, 30 GB storage, and 8 Mbps. Edit Mode tests cover each offer, combined usage, duplicate acceptance/assignment, unknown IDs, and rejection of a new assignment that exceeds any one resource. Failed operations leave contracts, assignments, cash, and resource totals intact and return a reason usable by the UI. No workload growth, contract cancellation, or multi-device management is required.
- Completion: Added TryAcceptOffer and TryAssignContract operations with runtime contract IDs, separate hosted assignment state, available-offer filtering, cumulative four-resource allocation, and readable failure reasons. Duplicate/unknown/cross-session IDs and over-capacity assignments are rejected before changing state; finances and authored definitions remain intact.
- Validation: Confirmed 12 new Edit Mode cases failed before implementation. Final Unity 6000.6.9f1 Edit Mode suite passed 46/46 and Play Mode regression suite passed 4/4. Covered each authored offer, combined exact CPU fit and other totals, acceptance versus assignment, duplicates, invalid IDs, cross-session IDs, independent sessions, and atomic rejection when any one resource is cumulatively exceeded.
- Commit: `98513a8` (session operations, tests, README); claim: `16d74bb`.
- Handoff: P2-04 should bill only AcceptedContracts whose IsHosted is true, using Definition.MonthlyRevenue and AssignedHardwareId. Acceptance uses an offer content ID; assignment uses contract and hardware runtime IDs. Success returns an empty reason; failure returns a displayable reason with no mutation. No billing, UI hosting controls, cancellation, growth, or reassignment was added.

### P2-04 Bill hosted contracts at outgoing month boundaries
- Status: `done`
- Owner: `codex`
- Depends On: `P2-03`
- Summary: Connect session billing to the clock's MonthEnded event, tracking revenue, starter-PC expenses, operating profit, and processed billing periods.
- Acceptance: Each outgoing month collects the authored monthly revenue for contracts hosted at that boundary and deducts the PC's $20 recurring expense once. For this first loop, use full monthly amounts without proration; accepted but unassigned offers earn nothing. Starting from $150, one PhotoSnap-only month ends at $230 cash and $80 cumulative operating profit; hosting both offers ends at $310 and $160. A monthly result identifies its outgoing period, revenue, expenses, and net profit; retain per-contract paid-month counts for later progression. Edit Mode tests verify no payment before rollover, no revenue while unassigned, expenses with no hosted contracts, ordered billing across multiple months/year rollover in one update, and no duplicate billing of an already processed period. Growth and unlocks remain deferred.
- Completion: Connected CompanySession billing to its clock MonthEnded event. Added immutable monthly results, ordered read-only history, last result, processed-period protection, cash/profit updates, recurring hardware expenses, and hosted-contract PaidMonths counters. Full monthly amounts apply without proration; unassigned contracts earn nothing.
- Validation: Confirmed seven new Edit Mode cases failed before implementation. Final Unity 6000.6.9f1 Edit Mode suite passed 53/53 and Play Mode regression suite passed 4/4. Verified GDD cash/profit examples, no early payment, unassigned/empty hosting expenses, late assignment without proration, 14 ordered monthly payments through year-end and leap day, and unchanged state for replayed/invalid periods.
- Commit: `cc21473` (billing, results, tests, docs); claim: `99e67ed`.
- Handoff: Next priority is P2-05 garage and walking. P2-07 can display LastMonthlyResult and live finances; P2-08 must persist billing history/processed periods and contract PaidMonths to avoid duplicate payments after restoration. TryBillMonth is idempotent for completed periods and returns a readable rejection reason. No growth, unlocks, financial UI binding, or persistence was added.

### P2-05 Build the garage and player walking
- Status: `in_progress`
- Owner: `codex`
- Depends On: `FOUND-04`
- Summary: Populate WorldView with a small garage, a walkable player, and a recognizable PC station using reusable prefabs and separate blockers and reach triggers.
- Acceptance: In Main Play Mode, the player can walk with WASD or arrow keys to and away from the PC station. Movement is frame-rate independent, diagonal movement does not increase speed, and the player cannot pass through walls or the PC's physical blocker. An independently tunable trigger defines station reach without blocking floor clearance. The orthographic camera shows the playable area and player without overlapping the anchored HUD; verify at 800x600 and 1280x720. Play Mode tests cover movement and collisions. Walking remains usable while the simulation is paused, and existing clock controls still work. This task adds no contract panel or hardware-purchase station.
- Completion:
- Validation:
- Commit:
- Handoff:

### P2-06 Gate the PC station panel by player reach
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-05`
- Summary: Add an interaction prompt and a closable PC station panel shell that opens only while the player is within reach.
- Acceptance: The panel starts closed. Within the PC trigger, an E-key interaction opens it and a visible prompt explains the control; the same input out of reach leaves it closed. The panel closes through its close control or Escape and closes when the player leaves reach, with the prompt updating accordingly. UI input does not trigger station actions behind the panel. The panel uses anchored RectTransforms and Canvas scaling, preserves access to the date/speed HUD, and does not create a second clock or session. Play Mode tests verify outside/inside/exit/re-entry behavior and panel closure. Hosting controls are added in P2-07.
- Completion:
- Validation:
- Commit:
- Handoff:

### P2-07 Bind hosting offers, assignments, utilization, and cash to the UI
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-03`, `P2-04`, `P2-06`
- Summary: Connect the PC panel and finance HUD to the shared session so the player can perform and inspect the hosting loop.
- Acceptance: Walking to the PC and opening its panel shows both available offers with requirements and monthly revenue, accepted contracts with assignment state, and the PC's used/total CPU, RAM, storage, and bandwidth with units. Separate accept and assign controls call the session operations and visibly update the same state; unavailable actions are disabled or explain the rejection without corrupting state. Cash starts at $150 in the HUD, updates after billing, and the player can inspect the last monthly revenue/expense/net result. Play Mode tests use UI controls to host a website, verify resource usage and cash after rollover, and confirm out-of-reach interaction cannot open the panel. Check panel and HUD readability at 800x600 and 1280x720. Capacity-growth alerts and satisfaction remain deferred.
- Completion:
- Validation:
- Commit:
- Handoff:

### P2-08 Implement the versioned JSON session save round trip
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-04`
- Summary: Add versioned plain C# save models and JSON serialization/restoration for the Phase 2 session, extending the clock with validated state restoration.
- Acceptance: Saves contain save_version, game_version, timestamp, simulation date, fractional-day progress, selected speed, cash, cumulative operating profit, hardware runtime/content IDs, accepted contract runtime/content IDs, assignments, paid-month counts, and processed billing state. Recompute capacities and demand from catalog definitions and restored assignments rather than scene references. Capture between session updates after all crossed boundaries are processed. Edit Mode tests round-trip a partially completed day shortly before month end, restore into a fresh session, and verify matching state and exactly one next monthly payment, including a save immediately after billing that must not replay it. Unsupported versions, malformed data, invalid progress/date, unknown content IDs, duplicate runtime IDs, and dangling assignments are rejected without changing a live session. Keep the format scoped to Phase 2; migrations and later-phase systems are deferred.
- Completion:
- Validation:
- Commit:
- Handoff:

### P2-09 Add disk save/load controls and rebind the restored session
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-07`, `P2-08`
- Summary: Store the Phase 2 JSON save under Application.persistentDataPath and expose explicit Save and Load controls in Main.
- Acceptance: Save writes one documented local slot at a completed session-update boundary, using a temporary file and replacement so a failed write preserves the previous save. Load validates the entire save before replacing the active session; GameRoot, the clock HUD, cash display, and an open PC panel all bind to the restored session without duplicate billing subscriptions or stale button actions. Missing/corrupt/unsupported saves and I/O failures show a concise message and preserve current play state. Saving does not require station reach; loading closes or refreshes the panel consistently. Player transform, interaction triggers, and panel-open state are not serialized in this first format. Play Mode tests use an isolated test save path, verify controls and rebinding, and clean up their own files without touching a player's slot. No autosave or slot-management UI is required.
- Completion:
- Validation:
- Commit:
- Handoff:

### P2-10 Verify the complete Phase 2 hosting and resume loop
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-01`, `P2-02`, `P2-03`, `P2-04`, `P2-05`, `P2-06`, `P2-07`, `P2-08`, `P2-09`
- Summary: Exercise the integrated Phase 2 exit criteria and document the player workflow and verification evidence.
- Acceptance: From a fresh Main session, demonstrate that interaction out of reach leaves the PC panel closed, then walk within reach, accept and assign a website through the panel, and inspect matching utilization and cash. Save shortly before month rollover with nonzero fractional-day progress, reload, verify that progress and assignment restore, and advance across rollover for exactly one correct payment and expense. Save/reload after that payment and verify it is not repeated; the following month still bills once. Repeat loading to catch duplicate subscriptions, and confirm pause/resume and speed controls still operate on the restored session. Run all Edit Mode and Play Mode suites in Unity 6000.6.9f1, including real pointer-hit and responsive-layout checks in the standalone test player where required. Record results and any remaining prototype-art caveat in docs/phases and README, and mark Phase 2 verified only after these checks pass. Do not start Phase 3 mechanics in this task.
- Completion:
- Validation:
- Commit:
- Handoff:
