## Spec

This file is the shared task queue for all agents working in this repo.

## Status Legend
- `todo`: ready to claim
- `in_progress`: currently owned by one agent
- `blocked`: cannot proceed until the blocker is resolved
- `done`: completed and validated for the scoped task

## Coordination Rules
- Claim work by changing a task from `todo` to `in_progress` and setting `Owner`.
- Do not edit another agent's `in_progress` task without an explicit handoff or user direction.
- When work is finished, change `Status` to `done`, move the task to `Completed Work`, update `docs/README.md`, and fill in `Completion`, `Validation`, `Commit`, and `Handoff` notes.
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

### GAME-13 Computer Workstation Hotspot And Floor Clearance
- Status: `todo`
- Owner: `unassigned`
- Depends On: `GAME-12`
- Summary: Rework the top-right computer workstation interaction area so the usable hotspot sits on the actual bench clutter while the rest of the garage floor stays open.
- Acceptance: The player can trigger the computer workstation only in the top-right workbench nook where the workstation art is painted, the interaction no longer reaches too far into the open room, and the center-right floor remains free to roam when the player is not intentionally stepping into that bench area.
- Completion:
- Validation:
- Commit:
- Handoff:

### SCENE-01 Clutter-Aligned Collision Cleanup
- Status: `todo`
- Owner: `unassigned`
- Depends On: `GAME-12`, `GAME-13`
- Summary: Replace the current broad rectangular blocker assumptions with collision placement that follows the actual clutter footprint at the board and workbench instead of blocking open floor.
- Acceptance: Physical collision only covers the painted desk, shelf, and bench clutter that should stop the player, the large open center floor remains navigable, and neither upper-corner workstation relies on an oversized hidden rectangle that extends well past the visible furniture.
- Completion:
- Validation:
- Commit:
- Handoff:

### TEST-05 Clutter Interaction And Free-Roam Regression Coverage
- Status: `todo`
- Owner: `unassigned`
- Depends On: `SCENE-01`
- Summary: Extend headless coverage so the upper-corner workstation hotspots and nearby floor clearance are validated against the authored garage clutter layout.
- Acceptance: Automated validation proves the contracts-board hotspot stays in the top-left clutter pocket, the computer workstation hotspot stays in the top-right bench pocket, and the surrounding center floor remains intentionally walkable rather than blocked by oversized hidden collision under `godot --headless --script docs/test.gd`.
- Completion:
- Validation:
- Commit:
- Handoff:

## Completed Work

### GAME-12 Contract Board Hotspot And Floor Clearance
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `none`
- Summary: Rework the top-left contract-board interaction area so the usable hotspot sits on the actual contract-board clutter while the surrounding open floor stays walkable.
- Acceptance: The player can trigger the contracts board only in the top-left board nook where the board and small desk clutter are painted, the interaction no longer floats out in the open floor, and the center-left floor remains free to roam when the player is not intentionally stepping into that board area.
- Completion: Replaced the loose board-side radius with a small rectangular interaction hotspot in the shared workstation script, moved the contract-board anchor deeper into the top-left desk nook, and added regression coverage that proves the nook remains interactable while nearby open floor no longer triggers the board.
- Validation: `godot --headless --script docs/test.gd`
- Commit: `feat: GAME-12 contract board hotspot and floor clearance`
- Handoff: The top-left board hotspot is now pinned to the visible planning nook. `GAME-13` can mirror this tighter hotspot approach on the top-right workstation without reopening the board interaction rules.

### TEST-04 Collision And Resize Regression Coverage
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `ART-07`, `UI-13`, `UI-14`
- Summary: Extend the shared headless coverage to lock the new collision anchors and resized gameplay/UI behavior in place.
- Acceptance: Automated validation proves the upper-left board collision and upper-right workstation collision stay aligned to their intended scene zones, and it adds coverage for the chosen gameplay-area and UI scaling rules so future resize regressions are caught under `godot --headless --script docs/test.gd`.
- Completion: Consolidated the new garage-anchor, gameplay-camera, and responsive-UI assertions under one explicit collision-and-resize regression pass in `docs/test.gd`, and added a direct resize-hook requirement for the contracts board panel so both shell-side public layout refresh paths stay covered.
- Validation: `godot --headless --script docs/test.gd`
- Commit: `test: TEST-04 collision and resize regression coverage`
- Handoff: The queue is now fully covered for the current collision and scaling rework. Future tweaks to garage anchors, gameplay zoom, or responsive shell sizing should extend the named collision-and-resize regression pass instead of adding one-off checks elsewhere.

### UI-14 Responsive HUD And Station Panel Scaling
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `UI-13`
- Summary: Rework the docked HUD and station-facing panels so they respond to window size changes without becoming cramped, oversized, or unreadable.
- Acceptance: The main dock, right-side alerts, contracts board panel, and station panel remain readable after window resizing, preserve access to core controls, and avoid covering an unreasonable amount of the playable garage area at common small and large window sizes.
- Completion: Added a shared resize-aware shell layout pass in `main.gd` that scales the main dock, log, right-side alerts, and station panel by viewport width while keeping the top-right station card inside the window height; reworked the contracts board panel into the same responsive pattern with centered width and height clamps plus a wrapping action row and flexible scroll width.
- Validation: `godot --headless --script docs/test.gd`
- Commit: `feat: UI-14 responsive hud and station panel scaling`
- Handoff: The shell and station-facing UI now respond to viewport size through public layout refresh methods. `TEST-04` can concentrate on consolidating and hardening the resize/collision regression coverage instead of introducing the responsive behavior itself.

### UI-13 Gameplay Area Window Scaling
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `ART-07`
- Summary: Make the playable garage view scale cleanly with window resizing so the room stays readable and traversal remains practical across smaller and larger window sizes.
- Acceptance: The gameplay area expands and contracts with window size through project or camera scaling instead of staying effectively fixed, and the player, stations, and main walkable paths remain readable and playable after resize changes.
- Completion: Added a resize-aware gameplay camera zoom path on the player, established a 1280x720 baseline viewport in project settings, and clamped the room view so smaller windows stay legible while larger windows reveal more of the garage floor without drifting too far out.
- Validation: `godot --headless --script docs/test.gd`
- Commit: `feat: UI-13 gameplay area window scaling`
- Handoff: The gameplay camera now handles world readability across window sizes. `UI-14` can focus on dock, board, and station-panel responsiveness without needing to redesign the world-scaling behavior first.

### ART-07 Garage Interaction Collision Realignment
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `none`
- Summary: Rework the upper-corner blocker collisions so the contract board and computer workstation align with the authored room art in the unified garage scene.
- Acceptance: The contract board collision sits in the upper-left corner where the painted board area lives, the computer workstation collision sits in the upper-right corner where the painted workbench lives, and the player can still navigate the center floor without clipping through those scene anchors.
- Completion: Moved the contract-board and workstation anchors deeper into the authored upper-left and upper-right furniture zones, resized the matching blocker rectangles to fit those painted footprints more closely, and refreshed the garage alignment notes to describe the tighter placement.
- Validation: `godot --headless --script docs/test.gd`
- Commit: `feat: ART-07 garage interaction collision realignment`
- Handoff: The collision-alignment slice is done and green. `UI-13` can now take over the next resize/scaling pass without revisiting these garage anchor positions unless the room art changes again.

### UI-12 Workstation Panel Wrapping Fix
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `UI-08`, `UI-11`
- Summary: Prevent the computer workstation panel from expanding off-screen when upgrade labels and benefit text get longer.
- Acceptance: The workstation panel remains readable on screen after opening it, and cycling upgrade options no longer pushes the panel left over neighboring HUD panels.
- Completion: Switched the workstation action row to a wrapping layout, enabled wrapping on the under-button hint copy, and extended headless coverage so the action container still refreshes correctly while keeping the panel readable.

### UI-11 Workstation Upgrade Benefit Copy
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `UI-08`, `UI-09`
- Summary: Add concise explanatory copy under the workstation upgrade buttons so the currently selected hardware item clearly states what it improves and why it helps.
- Acceptance: The computer workstation shows a brief grounded description beneath the upgrade buttons for the selected item, and the copy updates as the player cycles through available upgrades or buys one.
- Completion: Added a selected-upgrade explainer in the workstation hint area that combines grounded flavor text with concrete capacity and support-benefit callouts, and extended headless coverage so the under-button copy stays in sync while cycling and purchasing upgrades.

### UI-10 Startup Finances Purchase Refresh
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-11`, `UI-09`
- Summary: Refresh the startup-finances HUD immediately after workstation purchases so visible cash and expenses match the updated economy state.
- Acceptance: Buying an upgrade through the computer workstation updates the visible startup-finances cash readout right away instead of waiting for another screen refresh.
- Completion: Added an immediate economy HUD refresh to the workstation purchase flow and extended headless coverage to verify the visible startup-finances cash label changes after buying an upgrade from the station panel.

### UI-09 Workstation Action Refresh
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `UI-08`, `GAME-11`
- Summary: Refresh the workstation action row after upgrade cycling and purchases so the visible buy button always reflects the current next available item.
- Acceptance: Using `Previous Upgrade` or `Next Upgrade` updates the buy button label to the newly selected upgrade, and buying an item removes that option from the action row while advancing the buy button to the next available upgrade.
- Completion: Refreshed the station panel action row, title, subtitle, hint, and details after successful workstation actions so cycling and purchases immediately update the visible buy button; added headless coverage that checks cycling updates the label and purchase advances it to the next available upgrade.

### UI-08 Workstation Upgrade Price Label
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-11`
- Summary: Update the workstation purchase action text so the selected upgrade advertises its upfront cost and monthly upkeep directly in the button label.
- Acceptance: The computer workstation no longer shows a generic `Purchase Upgrade` action; instead it displays the currently selected upgrade name plus its cash cost and recurring monthly expense in grounded concise copy.
- Completion: Replaced the static workstation purchase label with selected-upgrade-aware button text and status copy that surface the current upgrade name, upfront cost, and monthly upkeep directly in the station panel, and added headless coverage to lock the visible workstation action label in place.

### GAME-11 Upgrade Purchase Finance Fix
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-06`, `TEST-03`
- Summary: Make workstation upgrade purchases deduct cash immediately and add modest recurring expense pressure so early hardware growth affects both startup finances and monthly overhead.
- Acceptance: Buying a workstation upgrade reduces cash on the spot, refuses the purchase when cash is insufficient, and increases recurring monthly expenses in a grounded small-shop way that is reflected in the existing economy snapshot/UI.
- Completion: Fixed workstation hardware purchases so upgrade cost is deducted from cash immediately, added insufficient-funds protection, introduced modest per-upgrade monthly upkeep values in the data catalog, and extended headless coverage to verify cash loss, expense growth, purchase gating, and funded multi-upgrade progression.

### TEST-03 Mitigation Regression Pass
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-09`, `GAME-10`, `UI-07`
- Summary: Expand headless coverage to verify that level-two mitigation actions and preparedness upgrades actually reduce support fallout while preserving the current progression and incident loop.
- Acceptance: Automated validation proves at least one level-two incident can be softened or cleared by an active mitigation action, proves at least one preparedness upgrade changes incident outcomes, and keeps `godot --headless --script docs/test.gd` green.
- Completion: Extended the shared headless harness with direct baseline-versus-mitigated comparisons for backup-check, after-hours, and traffic-spike incident paths, including manual response cost/clearance checks and preparedness-driven reductions in penalty, satisfaction loss, and response effort while keeping the full scene regression pass green.

### UI-07 Mitigation Readiness Surfacing
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-09`, `GAME-10`, `UI-06`
- Summary: Extend the docked shell and contract-facing summaries so the player can quickly see which mitigations are available, active, or missing before taking on level-two work.
- Acceptance: Operations/contracts/progress copy calls out mitigation readiness for higher-support contracts in concise grounded language, including whether the current shop is prepared for backup checks, after-hours requests, or traffic-spike issues.
- Completion: Added a shared mitigation-readiness summary that spells out active coverage, purchasable prep, missing hardware readiness, and the matching manual response for backup checks, after-hours requests, and traffic-spike issues across the operations, contracts, and progress shell panels; expanded headless checks to keep those level-two support callouts visible.

### GAME-10 Level 2 Preparedness Upgrades
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-09`, `GAME-06`
- Summary: Introduce a small preparedness layer for tier-two contracts through modest early infrastructure and service upgrades such as backup discipline, better connectivity, or light monitoring that reduce incident frequency or severity.
- Acceptance: The game exposes one or more humble mitigation upgrades that fit the `garage PC -> tower server -> small support add-ons` phase, and owning them measurably softens the warnings/issues attached to tier-two contracts without trivializing the support loop.
- Completion: Added modest preparedness upgrades to the workstation ladder, including a `Support Runbook Binder` and `Business Line Modem`, and tied those plus the existing `Backup NAS Box` into deterministic incident softening so backup-check and line-instability issues hit less hard and cost less to resolve; extended headless checks to lock the new upgrade IDs and softened issue severity in place.

### GAME-09 Level 2 Mitigation Actions
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-08`
- Summary: Add grounded response actions that let the player actively mitigate tier-two support issues, such as running a backup verification pass, scheduling a maintenance night, pausing nonessential hosting, or clearing an after-hours request before it becomes a penalty.
- Acceptance: When level-two support incidents are live, the player has specific actions beyond the blanket hosting hold that can reduce or clear the associated warning/penalty pressure; the actions read like small-hosting-company operations work rather than enterprise automation.
- Completion: Added manual mitigation actions to the contracts response panel for backup checks, after-hours/community queue work, and shaky small-office internet lines; tied those actions into live incident resolution with modest cash costs and refreshed warning/UI state; and expanded headless coverage so a live support issue can be cleared without falling back to the blanket hosting hold.

### TEST-02 Phase 2 Coverage Pass
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `CODE-03`, `GAME-05B`, `GAME-08`, `UI-06`
- Summary: Extend headless coverage for the rebalanced early progression and support systems while preserving the current MVP loop checks.
- Acceptance: Automated validation covers the tuned early hardware ladder, contract unlock behavior, and at least one support-incident scenario, and the existing loop remains green under `godot --headless --script docs/test.gd`.
- Completion: Tightened phase 2 headless coverage to explicitly lock the humble early upgrade ladder labels and tone, verify contract-lock and support-incident persistence through save/load, and preserve the full MVP loop plus unlock/support regression coverage under the shared `godot --headless --script docs/test.gd` harness.

### UI-06 Operations And Contracts Surfacing
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-06`, `GAME-08`
- Summary: Update the docked UI summaries so progression, contract locks, and support incidents are visible without opening every station panel, while preserving the humble startup tone of the early game.
- Acceptance: The operations/contracts/progress tabs expose upgrade state, locked-versus-available contract context, and live support pressure in a concise readable way that matches current retro UI patterns and reinforces the `garage PC -> tower server` progression story.
- Completion: Reworked the docked operations/contracts/progress/log summaries to surface the current hardware stage, next contract gate, board focus, and live support pressure directly in the shell, while keeping the copy grounded in the humble garage-to-tower-server phase and expanding headless checks to lock the higher-signal summaries in place.

### GAME-08 Support Incident Loop
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-07`
- Summary: Turn contract risk and support tags into lightweight early-game support incidents that fit the garage-hosting phase, such as maintenance hiccups, consumer internet trouble, storage pressure, and after-hours client requests.
- Acceptance: Active contracts can generate simple incident states tied to their metadata, the incidents read like small-hosting-company problems rather than enterprise datacenter failures, and the monthly/business loop reflects whether support issues were handled or neglected.
- Completion: Added persistent support incidents generated from live contract risk/support tags, tied them into the existing hosting-hold response flow, reported handled-versus-neglected incidents through monthly results and warning copy, and expanded headless checks to cover both ignored and properly handled support issues.

### GAME-07 Contract Tier Unlocks
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `CODE-02`, `GAME-06`
- Summary: Introduce a simple unlock model that gates higher-tier contracts behind grounded early progression conditions such as profit milestones, purchased tower-era hardware, completed work, or modest available capacity.
- Acceptance: Starter leads remain available at the beginning, small-business and forum/community-style work unlocks only after the intended early hardware/profit milestones, and the board/UI can explain why a more advanced lead is or is not available.
- Completion: Added a master contract pool with unlock-aware board filtering, introduced modest tier-two forum and small-office leads gated behind the first tower server and completed starter work, surfaced locked-versus-open contract reasoning in the board/contracts/progress summaries, and expanded headless coverage for the new unlock behavior.

### GAME-06 Expansion Purchase Flow
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-05B`
- Summary: Extend the workstation and progression flow so the player can review and purchase the rebalanced early hardware upgrades from the game loop.
- Acceptance: Modest early upgrades can be surfaced, purchased, and persisted through save/load; the workstation/progress UI reflects purchased versus available hardware cleanly without implying rack-scale infrastructure too early.
- Completion: Added upgrade browsing and purchase actions to the computer workstation, persisted the selected available upgrade through save/load, exposed purchased-versus-available upgrade state through the runtime expansion targets, and expanded the progress/workstation copy to show the early ladder more clearly.

### GAME-05B Early Hardware Ladder Rebalance
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `GAME-05`
- Summary: Retune the early upgrade ladder so it stays in the hobbyist-to-small-office range from the design docs, favoring modest PC and tower-server-era gains over dramatic rack-scale jumps.
- Acceptance: Early upgrade data reads like grounded starter infrastructure, the first few purchases feel intentionally limited, and progression copy clearly frames the arc as `Starter PC -> upgraded PC -> entry-level tower server -> small support/storage add-ons`.
- Completion: Rebalanced the early ladder away from rack-style escalation by renaming the first three upgrades to `Refurb Tower Server`, `Second Tower Server`, and `Backup NAS Box`, reducing their capacity jumps and costs to small-office levels, and updating validation to lock the humbler progression curve in place.

### GAME-05 Upgrade Ladder Data Slice
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `CODE-02`
- Summary: Add a small data-driven upgrade ladder beyond the first external server, including at least two new hardware purchases with distinct cost and capacity profiles.
- Acceptance: The project contains reusable data entries for multiple upgrade targets, progression copy reflects more than one next purchase, and shared capacity can grow through more than a single expansion step.
- Completion: Added a reusable upgrade catalog plus new `Managed Rack Server` and `Backup NAS Cluster` hardware entries, generalized capacity and cost calculations to aggregate purchased upgrades from data, advanced next-cost/progress copy through the ladder, and extended headless validation to cover multi-upgrade growth.

### CODE-03 Interaction Ownership Cleanup
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `none`
- Summary: Remove overlapping workstation interaction responsibilities between `main.gd` and `interaction_controller.gd` so focus, input, and panel opening flow through one clear owner.
- Acceptance: Contract board and workstation interactions still work in-scene, player interaction state remains correct, and the codebase no longer duplicates nearest-station/input handling across two systems.
- Completion: Kept nearest-station focus and `interact` input in `main.gd`, reduced `InteractionController` to a thin bridge for station-specific panel behavior, routed contract-board interaction through one public show path, and updated validation to target the public interaction surface.

### CODE-02 Contract Data Model Consolidation
- Status: `done`
- Owner: `agent-alpha`
- Depends On: `none`
- Summary: Consolidate contract loading and runtime contract shaping so the game has one authoritative path for IDs, tiers, risk/support metadata, and future unlock fields.
- Acceptance: Contract definitions load through one shared runtime path instead of split logic, starter contracts keep current behavior, and tests still pass with stable contract IDs and snapshots.
- Completion: Centralized starter contract shaping in `ContractManager`, moved `main.gd` off workstation JSON loading, added stable slug-based IDs plus default tier/unlock fields, and extended headless checks to lock the shared contract model in place.
