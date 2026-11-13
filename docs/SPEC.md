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
- Status: `done`
- Owner: `codex`
- Depends On: `FOUND-04`
- Summary: Populate WorldView with a small garage, a walkable player, and a recognizable PC station using reusable prefabs and separate blockers and reach triggers.
- Acceptance: In Main Play Mode, the player can walk with WASD or arrow keys to and away from the PC station. Movement is frame-rate independent, diagonal movement does not increase speed, and the player cannot pass through walls or the PC's physical blocker. An independently tunable trigger defines station reach without blocking floor clearance. The orthographic camera shows the playable area and player without overlapping the anchored HUD; verify at 800x600 and 1280x720. Play Mode tests cover movement and collisions. Walking remains usable while the simulation is paused, and existing clock controls still work. This task adds no contract panel or hardware-purchase station.
- Completion: Added reusable Garage, Player, and PCStation prefabs with labeled prototype sprites, wall/desk blockers, and an independent PC reach trigger. Added WASD/arrow input and normalized fixed-step Rigidbody2D movement. The camera fits the garage in a world viewport reserved between the HUD bars, updating after Canvas layout.
- Validation: Confirmed three new Play Mode tests failed without the world setup. Final Unity 6000.6.9f1 Editor and standalone Windows Play Mode suites passed 7/7 each, covering physics-step-independent/diagonal movement, wall/desk collisions, nonblocking reach trigger, walking while paused, existing clock controls, and camera/HUD bounds at 800x600, 1280x720, and 1920x1080. Visually inspected screenshots at both required sizes. No simulation logic changed; Edit Mode suite was not rerun.
- Commit: `54bf5f2` (prefabs, art, movement/camera scripts, scene, tests, README); claim: `423877c`.
- Handoff: P2-06 should add the prompt and gated PC panel using PCStation/Reach (CircleCollider2D radius 2), separate from PCStation/Blocker (BoxCollider2D). Player has PlayerMovement and PlayerKeyboardInput; walking uses physics at 3 world units/second and remains active during simulation pause. Camera viewport follows the scaled HUD boundaries. Art is explicitly provisional; no panel, contract UI, or hardware station was added.

### P2-06 Gate the PC station panel by player reach
- Status: `done`
- Owner: `codex`
- Depends On: `P2-05`
- Summary: Add an interaction prompt and a closable PC station panel shell that opens only while the player is within reach.
- Acceptance: The panel starts closed. Within the PC trigger, an E-key interaction opens it and a visible prompt explains the control; the same input out of reach leaves it closed. The panel closes through its close control or Escape and closes when the player leaves reach, with the prompt updating accordingly. UI input does not trigger station actions behind the panel. The panel uses anchored RectTransforms and Canvas scaling, preserves access to the date/speed HUD, and does not create a second clock or session. Play Mode tests verify outside/inside/exit/re-entry behavior and panel closure. Hosting controls are added in P2-07.
- Completion: Added a scaled, anchored PC panel shell, Close control, and [E] interaction prompt. PcStationInteraction opens only while the player collider touches the existing reach trigger, consumes repeated interaction while open, closes through Close/Escape or leaving reach, and respects text-input focus. Date/speed HUD remains accessible and the existing session/clock is preserved.
- Validation: Confirmed three new Play Mode cases failed before scene/controller implementation. Final Unity 6000.6.9f1 Editor and standalone Windows Play Mode suites passed 11/11 each. Verified out-of-reach rejection, in-reach opening, exit/re-entry, repeated input, Close/Escape priority, text-input focus, unchanged session/clock, panel bounds at 800x600 and 1280x720, and real Close/time-control raycast targets. Visually inspected open-panel screenshots at both sizes. No simulation changes; Edit Mode suite was not rerun.
- Commit: `cb1b7ce` (controller, panel scene, tests, README); claim: `9966083`.
- Handoff: P2-07 should bind offers, accepted contracts, assignments, utilization, and monthly finances into the existing PcPanel. PcStationInteraction is a scene component under WorldView with serialized references to PCStation/Reach, the player collider, panel, prompt, and Close button. TryOpen/Close and IsOpen/IsWithinReach expose panel lifecycle; no hosting controls, extra session, or additional station was added.

### P2-07 Bind hosting offers, assignments, utilization, and cash to the UI
- Status: `done`
- Owner: `codex`
- Depends On: `P2-03`, `P2-04`, `P2-06`
- Summary: Connect the PC panel and finance HUD to the shared session so the player can perform and inspect the hosting loop.
- Acceptance: Walking to the PC and opening its panel shows both available offers with requirements and monthly revenue, accepted contracts with assignment state, and the PC's used/total CPU, RAM, storage, and bandwidth with units. Separate accept and assign controls call the session operations and visibly update the same state; unavailable actions are disabled or explain the rejection without corrupting state. Cash starts at $150 in the HUD, updates after billing, and the player can inspect the last monthly revenue/expense/net result. Play Mode tests use UI controls to host a website, verify resource usage and cash after rollover, and confirm out-of-reach interaction cannot open the panel. Check panel and HUD readability at 800x600 and 1280x720. Capacity-growth alerts and satisfaction remain deferred.
- Completion: Added a reusable HostingOfferRow prefab and HostingPanel presenter for authored offers, requirements/revenue, acceptance and hosted state, separate Accept/Assign controls, live used/total resources, inline feedback, and last-month revenue/expenses/net. GameRoot now displays live cash in the HUD. All controls operate on the existing shared session.
- Validation: Added Play Mode tests first and confirmed missing UI bindings (two failing cases). Final Unity 6000.6.9f1 Editor and standalone Windows Play Mode suites passed 14/14 each. Used actual pointer targets for accepting/assigning both offers, verified totals of 2 cores/3 GB RAM/30 GB storage/8 Mbps, $310 cash and $160 monthly net after rollover, duplicate rejection and disabled actions, out-of-reach gating, and text bounds/heights at 800x600 and 1280x720. Visually inspected both layouts. No simulation rules changed; Edit Mode suite was not rerun.
- Commit: `c272835` (presenters, row prefab, Main/HUD binding, tests, README); claim: `9bceda2`.
- Handoff: Next priority is P2-08 versioned JSON session saves. Hosting presenters read GameRoot.Session for current state and action handling; GameRoot owns the cash label binding. Rows are created from the catalog on first panel open. P2-09 must refresh/rebind presentation when replacing a session. Alerts remain a placeholder; no growth, satisfaction, or save/load controls were added.

### P2-08 Implement the versioned JSON session save round trip
- Status: `done`
- Owner: `codex`
- Depends On: `P2-04`
- Summary: Add versioned plain C# save models and JSON serialization/restoration for the Phase 2 session, extending the clock with validated state restoration.
- Acceptance: Saves contain save_version, game_version, timestamp, simulation date, fractional-day progress, selected speed, cash, cumulative operating profit, hardware runtime/content IDs, accepted contract runtime/content IDs, assignments, paid-month counts, and processed billing state. Recompute capacities and demand from catalog definitions and restored assignments rather than scene references. Capture between session updates after all crossed boundaries are processed. Edit Mode tests round-trip a partially completed day shortly before month end, restore into a fresh session, and verify matching state and exactly one next monthly payment, including a save immediately after billing that must not replay it. Unsupported versions, malformed data, invalid progress/date, unknown content IDs, duplicate runtime IDs, and dangling assignments are rejected without changing a live session. Keep the format scoped to Phase 2; migrations and later-phase systems are deferred.
- Completion: Added required-field version 1 JSON save models, UTC/game-version metadata, safe between-update capture, validated silent clock restoration, and fresh-session reconstruction preserving runtime IDs, finances, assignments, paid-month counts, and processed billing history. Resources are rebuilt from catalog definitions. Invalid input is rejected without replacing live state, including trailing JSON and inconsistent/incomplete monthly ledgers.
- Validation: Confirmed pre-implementation failures (12 cases). Final Unity 6000.6.9f1 Edit Mode suite passed 86/86 and standalone Windows Play Mode suite passed 15/15, including JSON round trips in the player. Verified saves before/after month rollover, one payment per period and the next billing cycle, all saved speeds, stable IDs, catalog-based resource reconstruction, rejected over-capacity restoration, capture guards, malformed/unsupported data, invalid progress/date/IDs/assignments/counters/finances, and unchanged live state on failure. Editor Play Mode also passed 15/15 before the final trailing-input hardening.
- Commit: `3e62e12` (save models/serializer, restore validation, clock guard, tests, docs); claim: `e82c594`.
- Handoff: P2-09 should capture with SessionSaveJson.Capture(session, Application.version) between updates, write ToJson(snapshot) to the documented slot, and replace GameRoot.Session only after TryLoad succeeds. Billing history is the processed-period ledger and rebuilds the duplicate guard without emitting ticks. Version 1 is scoped to one starter PC and Phase 2 finances (cash = $150 + operating profit); extend those invariants before purchases. No disk I/O, player save/load controls, or live presentation rebinding was added.

### P2-09 Add disk save/load controls and rebind the restored session
- Status: `done`
- Owner: `codex`
- Depends On: `P2-07`, `P2-08`
- Summary: Store the Phase 2 JSON save under Application.persistentDataPath and expose explicit Save and Load controls in Main.
- Acceptance: Save writes one documented local slot at a completed session-update boundary, using a temporary file and replacement so a failed write preserves the previous save. Load validates the entire save before replacing the active session; GameRoot, the clock HUD, cash display, and an open PC panel all bind to the restored session without duplicate billing subscriptions or stale button actions. Missing/corrupt/unsupported saves and I/O failures show a concise message and preserve current play state. Saving does not require station reach; loading closes or refreshes the panel consistently. Player transform, interaction triggers, and panel-open state are not serialized in this first format. Play Mode tests use an isolated test save path, verify controls and rebinding, and clean up their own files without touching a player's slot. No autosave or slot-management UI is required.
- Completion: Added SessionSaveSlot with flushed temporary-file writes and atomic replacement of company-v1.json under Application.persistentDataPath. Added HUD Save/Load buttons and concise feedback, deferred file actions until LateUpdate, and validated session replacement with immediate HUD refresh and closed/rebuilt PC presentation. Player/world state remains untouched. Documented the slot and workflow.
- Validation: Confirmed four new Play Mode cases failed before implementation. Final Unity 6000.6.9f1 Edit Mode suite passed 86/86; Editor and standalone Windows Play Mode suites passed 19/19 each. Verified real Save/Load pointer targets, saving out of reach, restored partial day/cash/date/speed, repeated loads without duplicate billing or stale panel actions, unchanged player position, missing/corrupt/unsupported/read-locked saves, successful atomic overwrite, failed write preserving prior bytes, and capture after all crossed monthly ticks. Tests used unique temporary slots and cleaned their files. Visually inspected 800x600 and 1280x720 HUD layouts.
- Commit: `2689ea9` (slot I/O, HUD controls, rebinding, scene, tests, docs); claim: `99062d7`.
- Handoff: P2-10 should verify the full fresh-game hosting/save/resume loop and record Phase 2 exit evidence. Save and Load are explicit single-slot actions; load always closes the PC panel and its next open rebuilds rows. GameRoot.ConfigureSaveSlot accepts an isolated backend path for tests. Default slot: Application.persistentDataPath/company-v1.json. No autosave, slot manager, or Phase 3 mechanics were added.

### P2-10 Verify the complete Phase 2 hosting and resume loop
- Status: `done`
- Owner: `codex`
- Depends On: `P2-01`, `P2-02`, `P2-03`, `P2-04`, `P2-05`, `P2-06`, `P2-07`, `P2-08`, `P2-09`
- Summary: Exercise the integrated Phase 2 exit criteria and document the player workflow and verification evidence.
- Acceptance: From a fresh Main session, demonstrate that interaction out of reach leaves the PC panel closed, then walk within reach, accept and assign a website through the panel, and inspect matching utilization and cash. Save shortly before month rollover with nonzero fractional-day progress, reload, verify that progress and assignment restore, and advance across rollover for exactly one correct payment and expense. Save/reload after that payment and verify it is not repeated; the following month still bills once. Repeat loading to catch duplicate subscriptions, and confirm pause/resume and speed controls still operate on the restored session. Run all Edit Mode and Play Mode suites in Unity 6000.6.9f1, including real pointer-hit and responsive-layout checks in the standalone test player where required. Record results and any remaining prototype-art caveat in docs/phases and README, and mark Phase 2 verified only after these checks pass. Do not start Phase 3 mechanics in this task.
- Completion: Added an integrated fresh-game regression covering actual physics walking, reach gating, UI hosting, pre-rollover save/restore, post-payment save/repeated loads, and the following billing cycle. Recorded the player workflow, validation evidence, reproduction instructions, and prototype-art caveat in README and docs/phases. Phase 2 exit criteria are verified.
- Validation: Verification-only task: the new regression passed against existing runtime behavior without production changes. Final Unity 6000.6.9f1 full suites passed: Edit Mode 86/86, Editor Play Mode 20/20, standalone Windows Play Mode 20/20. Verified the January 31 snapshot at 0.75 day progress, stable assignment/IDs, one $100 revenue/$20 expense payment to $230 cash, three post-payment loads without replay, and the next month at $310/two paid months. Checked restored pause/resume/speeds, actual pointer targets, full responsive suites, cleaned isolated save directories, and visually inspected resumed-state screenshots at 800x600 and 1280x720.
- Commit: `923ba41` (integration test, README, phase evidence); claim: `56ca2ef`.
- Handoff: Phase 2 is complete and all current queued tasks are done. Define scoped Phase 3 tasks from docs/phases before implementing capacity upgrades, the hardware station, PhotoSnap growth, or bottleneck alerts. Prototype art and placeholder alerts remain intentional. Extend version 1 save invariants when purchases/new financial state are introduced; do not lose existing billing/save regression coverage.

## Phase 3 Queue — Capacity upgrades and alerts (Era 1)

Take ready tasks in the priority order below. Preserve the Phase 2 hosting, walking, billing, and save/resume loop. Introduce save compatibility before enabling purchases and growth: the current loader assumes cash equals $150 plus operating profit and validates restored assignments as new allocations, both of which must change for this phase. Keep tower unlocks/purchases, additional hosting devices, satisfaction, SLA penalties, and rack systems in later phases. Over-capacity operation in Phase 3 produces diagnostic alerts without changing the existing revenue or expense rules.

Use one purchasable upgrade per resource on the starter PC for this phase. The CPU upgrade is the GDD's $120 for +2 cores. New provisional starting values for planning are RAM $80 for +4 GB, storage $100 for +250 GB, and internet $90 for +10 Mbps; record them as provisional in the GDD when authoring the content. No repeat purchases or upgrade tiers are required.

### P3-01 Author upgrade and PhotoSnap milestone definitions
- Status: `done`
- Owner: `codex`
- Depends On: `P2-10`
- Summary: Extend the authored catalog with four starter-PC upgrades and the first PhotoSnap growth milestone, using immutable runtime definitions and stable content IDs.
- Acceptance: Assets/Data contains CPU, RAM, storage, and internet upgrade definitions with explicit stable IDs, target hardware content ID, price, and capacity delta matching the Phase 3 provisional values above. The PhotoSnap milestone definition has a stable ID, targets offer.photosnap, becomes eligible after two paid months, and changes demand to 2 cores while retaining 2 GB RAM, 20 GB storage, 4 Mbps, and $100 monthly revenue. Edit Mode validation rejects missing/duplicate IDs, unknown targets, invalid money/resource values, zero-effect upgrades, and invalid milestone thresholds. Definitions convert to engine-independent data without mutable session state. Update docs/GDD with the new provisional upgrade values; preserve existing starter/billing values and verify Unity imports assets and metadata.
- Completion: Authored CPU/RAM/storage/internet upgrade assets and the PhotoSnap two-paid-month growth asset, referenced by StarterCatalog. Added immutable UpgradeData/GrowthMilestoneData, validated catalog targets/global IDs, and retained the legacy two-argument catalog constructor. Recorded provisional prices/deltas in docs/GDD and content guidance in README; starter offers and billing values remain unchanged.
- Validation: Confirmed ten new Edit Mode failures before implementation. Final Unity 6000.6.9f1 suites passed: Edit Mode 98/98 and Editor/standalone Windows Play Mode 20/20 each. Verified exact authored values, stable IDs across renamed copies, independent runtime snapshots/list copies, missing/duplicate IDs and references, unknown targets, invalid prices/resources/thresholds, zero-effect upgrades, asset imports, and the full existing Phase 2 loop.
- Commit: `552df9a` (assets, definitions/catalog validation, tests, GDD/README); claim: `e8d58db`.
- Handoff: P3-02 should consume HostingCatalog.Upgrades and Milestones. Upgrade CapacityDelta is additive; milestone Requirements is the absolute grown workload, not a delta. IDs: upgrade.starter-cpu/ram/storage/internet and growth.photosnap.cpu-2. Targets: hardware.starter-pc and offer.photosnap. Definitions are metadata only: no effective-state mutation, purchases, automatic growth, save-format changes, or new UI was introduced.

### P3-02 Model effective capacities, demand, purchases, and milestone state
- Status: `done`
- Owner: `codex`
- Depends On: `P3-01`
- Summary: Extend plain C# runtime state so hardware upgrades and applied contract milestones can alter effective capacity and demand without mutating authored definitions.
- Acceptance: Hardware exposes base and effective capacity, applied upgrade IDs, and purchase records containing runtime hardware ID, upgrade content ID, and amount paid. Contracts expose effective requirements separately from their authored base requirements and retain applied milestone IDs. Pure resource calculations combine base capacity with recorded upgrade deltas and derive demand from applied milestones; used demand remains visible above capacity while available headroom stays at zero for an overused dimension. Fresh companies still have $150, no purchases/milestones, and the Phase 2 resource totals. Edit Mode tests verify all four resource deltas, the authored PhotoSnap demand change, independent sessions, immutable definitions, and stable runtime IDs. This task adds state/calculation support only; purchasing and automatic growth are separate tasks.
- Completion: Added immutable UpgradePurchase records, explicit state constructors, read-only applied upgrade/milestone IDs, and base/effective resource properties on runtime instances. ProgressionResources validates recorded state and calculates additive capacity, absolute applied workload profiles, and assigned usage/headroom. New assignments now read effective requirements; fresh sessions retain Phase 2 state.
- Validation: Confirmed seven new Edit Mode failures before calculation implementation. Final Unity 6000.6.9f1 suites passed: Edit Mode 107/107 and Editor/standalone Windows Play Mode 20/20 each. Covered all four deltas, combined capacity, historical amounts paid, PhotoSnap demand, overload visibility, unrelated/unassigned contracts, input snapshot isolation, stable IDs, independent sessions, invalid records/targets/eligibility, profile ordering, and the complete existing hosting/save loop.
- Commit: `a230824` (runtime state, pure calculations, tests, README); claim: `a521f3a`.
- Handoff: P3-03 can use explicit HardwareInstance/ContractInstance state constructors with catalog definitions, then ProgressionResources.CalculateUsage and internal SetUsedResources to rebuild allocations without treating restored overloads as new assignments. Upgrade records retain historical AmountPaid; profile requirements replace rather than add to base demand. State constructors do not spend cash or apply eligible milestones automatically. Controlled live purchase/growth commits remain in P3-04/P3-05. Version 1 persistence and UI behavior are unchanged.

### P3-03 Extend saves for Phase 3 and migrate Phase 2 saves
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-02`
- Summary: Introduce save version 2 for purchases and milestones while retaining the existing manual slot and supporting version 1 Phase 2 saves.
- Acceptance: Version 2 saves preserve purchase records/amounts paid, applied upgrade and milestone IDs, and all Phase 2 clock, contract, finance, and billing data. Cash validates as $150 plus cumulative operating profit minus recorded purchase spending; purchases do not reduce operating profit. Load rebuilds effective capacity and demand from the current catalog and stored IDs. Restoring existing hosted assignments allows a valid growth-induced overload; the new-assignment capacity guard remains enforced during normal play. Reject unknown upgrade/milestone IDs, duplicate purchases/milestones, invalid targets/costs, inconsistent milestone/paid-month state, dangling assignments, inconsistent financial totals, and malformed histories without changing live state. Version 1 fixtures migrate with no purchases, preserve IDs/paid-month counts/billing, and apply the authored PhotoSnap milestone if its saved paid-month count already meets the threshold; migration emits no billing ticks or extra payment. Keep Application.persistentDataPath/company-v1.json as the existing slot path (schema version is inside JSON), write version 2 on the next explicit Save, and reject unsupported future versions safely. Edit Mode tests cover migration, upgraded/overloaded round trips, partial days, and exactly one next payment; update format documentation and preserve atomic I/O tests.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-04 Implement atomic starter-PC upgrade purchases
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-03`
- Summary: Add session purchase operations that validate the upgrade, hardware, affordability, and one-time purchase rule before changing money or capacity.
- Acceptance: Each authored upgrade can be bought once for the starter PC, subtracting its price from cash, recording the amount paid, and updating only the relevant effective capacity. Buying CPU from a fresh $150 company leaves $30 cash and 4 CPU cores with operating profit unchanged at zero; attempting the $80 RAM upgrade next fails with no state change. Edit Mode tests cover all four upgrades, exact-price affordability, insufficient cash, unknown/cross-session hardware IDs, invalid upgrade IDs, and duplicate purchases. Every failure returns a UI-readable reason and preserves cash, profit, purchase records, assignments, and all resource totals. Purchased upgrades round-trip through version 2 saves and do not alter recurring PC expense. No tower or additional hardware purchases are introduced.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-05 Apply PhotoSnap growth after its second paid month
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-03`, `P3-04`
- Summary: Apply the authored PhotoSnap milestone once after the qualifying monthly payment and recalculate its hosted demand.
- Acceptance: PhotoSnap stays at its starter demand before two paid months; accepted but unassigned time does not advance the milestone. After the second payment has updated PaidMonths and finances, apply the milestone once, changing CPU demand from 1 to 2 cores with the other requirements and revenue unchanged. Two hosted starter websites then use 3 cores on the unupgraded 2-core PC without dropping or reassigning either contract; PhotoSnap alone uses 2/2 cores. Edit Mode tests verify billing-before-growth order, a single update crossing multiple qualifying months, one-time application, independent sessions, pre-purchased CPU headroom, and saves/reloads immediately before and after the milestone without duplicate growth/payment. PaidMonths and milestone IDs remain consistent with migrated version 1 saves. No satisfaction, churn, penalties, further growth stages, or tower unlock behavior is added.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-06 Derive resource bottlenecks from effective demand and capacity
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-04`, `P3-05`
- Summary: Add engine-independent diagnostics that identify overloaded resources and the contracts assigned to the affected hardware.
- Acceptance: Diagnostics report hardware identity, constrained resource, used demand, effective capacity, shortfall, and affected contract identities for CPU, RAM, storage, and bandwidth. Exact capacity is not an overload; multiple constrained resources are all reported. Both starter sites after PhotoSnap growth identify CPU at 3/2 cores; buying the CPU upgrade changes this to 3/4 and clears the CPU diagnostic immediately. Edit Mode tests cover each resource independently, multiple bottlenecks, equality, growth, relevant versus irrelevant upgrades, and recomputation after restoring an overloaded save. Diagnostics are derived from current state rather than persisted as authoritative save fields and do not change revenue, expenses, assignments, or customer state. HUD presentation is separate.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-07 Add the walkable hardware station and gated panel
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P2-10`
- Summary: Add a recognizable hardware station prefab, independent blocker/reach trigger, interaction prompt, and closable panel shell using the existing garage interaction pattern.
- Acceptance: The player can walk to and away from both stations with WASD/arrows, including while simulation time is paused. Hardware interaction out of reach leaves its panel closed; within reach, E opens it, and Close/Escape or leaving reach closes it. At most one station panel opens for an interaction; panel/UI input cannot trigger a second station behind it. Blockers and triggers remain independently tunable, floor routes to both stations remain clear, and both stations fit the camera view below the HUD at 800x600 and 1280x720. Loading a session closes either station panel and preserves player/world placement. Play Mode tests cover movement, collisions, reach/re-entry, station selection, and existing PC behavior. Reuse the existing interaction pattern rather than introducing unrelated input systems; purchase bindings follow in P3-08.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-08 Bind upgrade purchases and effective hosting state to station UI
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-04`, `P3-05`, `P3-07`
- Summary: Connect the hardware panel to session purchase operations and update the PC panel to display effective capacity and grown contract requirements.
- Acceptance: Within hardware-station reach, the panel lists all four upgrades with prices, current cash, current capacity, resulting capacity, and purchase status. Purchase buttons call the shared session operation; unaffordable/already-owned actions are disabled or explain rejection. A successful purchase refreshes cash and capacity immediately. The PC panel reflects PhotoSnap's effective requirements and the upgraded PC totals rather than cached authored base values. Reopening panels and loading upgraded/grown sessions rebuilds current views and button actions without duplicate listeners or stale session references. Play Mode tests buy each upgrade through UI, verify the $120 CPU transaction, reject an unaffordable second purchase without state changes, and exercise save/load rebinding for both panels. Inspect readable layouts and actual button hit targets at 800x600 and 1280x720. No alerts beyond existing action feedback are required in this task.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-09 Show growth and bottleneck information in the HUD and PC panel
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-06`, `P3-08`
- Summary: Replace the alerts placeholder with current resource diagnostics and explain PhotoSnap's growth in the hosting view.
- Acceptance: With both starter sites hosted, the second paid month visibly explains PhotoSnap's CPU increase and shows a CPU bottleneck tied to the starter PC and affected hosting contracts. The HUD provides a compact constrained-resource summary, and the PC panel exposes demand versus capacity and the cause in readable units. Purchasing CPU headroom clears the overload indication once usage fits; an unrelated upgrade leaves a CPU alert active. With no overload, the HUD clearly reports no capacity alerts. Reloading an overloaded or upgraded save reconstructs the correct messages without replaying the growth event or retaining stale alerts. Play Mode tests cover appearance, relevant/irrelevant upgrade effects, clearance, and reload; verify all resource names, readable text, and HUD/time/save-control access at the required window sizes. This is diagnostic feedback only, with no service penalties or satisfaction simulation.
- Completion:
- Validation:
- Commit:
- Handoff:

### P3-10 Verify the complete Phase 3 bottleneck and upgrade loop
- Status: `todo`
- Owner: `unassigned`
- Depends On: `P3-01`, `P3-02`, `P3-03`, `P3-04`, `P3-05`, `P3-06`, `P3-07`, `P3-08`, `P3-09`
- Summary: Exercise the integrated Phase 3 exit criteria, preserve Phase 2 regressions, and document the player workflow and evidence.
- Acceptance: From a fresh game, use walking and station UI to host both starter sites, reach the second paid month, identify CPU at 3/2 cores, walk to the hardware station, and buy the $120 CPU upgrade to reach 3/4 cores and clear the alert. Before purchase, cash is $470 and operating profit $320; afterward cash is $350 and operating profit remains $320. In a separate fresh-company case, buy CPU for $120 and reject the $80 RAM purchase with $30 remaining, preserving cash/capacity/records; verify interaction fails out of reach at both stations. Save/reload before growth, while overloaded, and after purchase; verify partial days, milestone/purchase IDs, demand/capacity, alerts, and exactly one subsequent monthly payment without replaying growth or purchases. Include a legacy version 1 save migration and restored-panel actions. Run all Edit Mode and Play Mode suites in Unity 6000.6.9f1, including standalone pointer/layout checks and Phase 2 regressions. Update docs/phases, README, and provisional balance notes with results and remaining prototype-art caveats; mark Phase 3 verified only after these checks pass. Do not implement Phase 4 unlocks or tower purchases.
- Completion:
- Validation:
- Commit:
- Handoff:
