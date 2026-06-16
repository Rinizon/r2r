## Project Setup
Phase 1 now has a minimal Godot project scaffold with a main scene and modular player and workstation sub-scenes. This keeps the MVP lightweight while establishing the structure future movement, interactions, and gameplay systems can build on.

## UI Scaling
The HUD now sits inside a responsive dock layout instead of relying on fixed screen offsets for each major panel. Startup finances, clock controls, and the right-side alert stack stay docked while shrinking more gracefully with the game window, which gives the interface a sturdier retro management sim foundation.

The gameplay camera now also reacts to window size instead of leaving the garage view effectively fixed. Smaller windows keep the room zoomed in enough to stay readable, while larger windows ease back to reveal more of the floor without making the startup space feel too distant.

The dock shell, station panel, and contracts board panel now also adjust their footprint with the window instead of relying on one fixed card size. That keeps the controls readable on tighter windows while preventing the UI from taking an oversized bite out of the garage scene on larger displays.

The shared headless suite now treats garage collision anchors and resize behavior as one explicit regression slice. It covers the upper-corner board and workstation alignment plus the gameplay camera and responsive UI layout hooks, which makes future room or shell tuning safer.

The contract board interaction now sits in a tighter top-left nook instead of spilling out across nearby floor space. That makes the board feel attached to the actual planning-desk clutter and leaves the surrounding garage floor freer to walk through.

## Main Tabs
The main left-side management area now uses top-level `Operations`, `Contracts`, and `Progress` tabs instead of leaving each major panel as a separate free-standing block. `Operations` is the default view and now absorbs the startup finances and clock controls, while the other two tabs establish scalable homes for future contract and progression-focused UI.

The `Contracts` and `Progress` tabs now surface live business state instead of placeholder copy. They summarize the current pipeline, hosting activity, completed or cancelled work, and the next infrastructure goal so the dock shell reads like an active management console.

The `Contracts` tab is now split into separate sections for available leads, accepted queue work, active hosting, and outcomes. That makes it easier to scan what still needs to be accepted, what is waiting on the workstation, what is already live, and how past work resolved.

Starter contracts now also carry visible `Risk` and `Support` tags through the contract UI. That gives the player an early read on which jobs are safer, which ones imply heavier client expectations, and how different leads might shape the business even before deeper systems are added.

The `Contracts` tab now also includes a lightweight issue-response panel with a new-hosting hold. When service warnings are active, enabling the hold pauses further activation and throttles overloaded contracts so satisfaction and penalties are protected at reduced revenue until the player clears the hold.

## Onboarding Flow
The operations tab now includes a compact `Startup Loop` panel with short narrative callouts for the early garage flow. It points the player from the contract board to the workstation, warns about overload risk, and then shifts into the first server expansion goal once hosting is live.

## Art Foundation
The garage scene now has a shared environment foundation backdrop built around a warmer DOS-era management sim look instead of relying only on flat placeholder polygons. Its palette and material roles are also documented in a small JSON descriptor so later shell, prop, and polish passes can extend one consistent visual language.

## Garage Shell
The room shell now reads as a smaller converted residential garage instead of a broader office-like backdrop. A roll-up door, side entry, windows, and built-in DIY bench now establish the startup-from-home feel while still leaving the center floor open for gameplay and later prop clutter.

## Prop Pass
The garage now includes a reusable electronics-bench prop layer focused on wall-mounted storage and work surfaces first. Pegboard tools, parts drawers, soldering gear, spare hardware, and bench clutter now push the room closer to an old-school startup garage without choking off the playable floor.

## Scene Polish
The garage now has a final polish layer tuned for a warmer late-night startup mood rather than a flat daytime read. Softer edge falloff, warmer utility glow, and clearer emphasis around the contract board, workstation bench, and expansion edge make the room feel more cohesive without obscuring the playable floor.

## Scene Reset
The garage scene now uses a single authored room image instead of stacking separate shell, prop, and polish overlays on top of each other. That removes the duplicated bench and clutter problem and gives the converted residential startup garage one coherent visual source of truth again.

## Node Alignment
The player spawn now starts closer to the center of the garage, and the major interactive nodes are lined up more closely with the painted board, bench, and expansion zones in the unified room. The main bench collision is also broadened into a more believable shaped blocker so the garage layout reads more like the scene the player actually sees.

The contract board and computer workstation collision anchors now sit tighter to the authored upper-left planning desk and upper-right workbench. That makes the room's two first-stop interaction zones line up more naturally with the painted garage furniture instead of feeling offset from the art.

## Detail Pass
The unified garage room now carries a richer but still grounded detail pass, with more painted-in startup clutter and support objects built directly into the composition. Most of that extra detail sits around the center edges, board side, bench side, and expansion edge so the room feels more lived in without sacrificing the main walking space.

## Readability Tune
The final garage pass now brightens the contract board and workstation a little more evenly so both read as clear gameplay anchors at a glance. The expansion corner remains visible as a softer secondary goal, while the room keeps its late-night mood and open center floor.

## Collapsible Log
A shared log sidebar now sits beside the main management panel and tracks both contract work and progression goals in one place. It can collapse down to a thin labeled strip when the player wants less clutter, then expand again without losing access to the current contract queue or upgrade goals.

## UI Polish
The docked management shell now has a more cohesive retro panel tint across the main panel, log sidebar, and right-side alerts so the interface reads as one system instead of a set of unrelated boxes. The shell is also validated as a whole, including tab switching and log collapse/reopen behavior, which makes the scaled UI structure more stable for future feature work.

In fullscreen, the docked UI now stays compact instead of stretching its panels across the entire window. Extra horizontal space is left to the garage scene, which keeps the management shell readable without overwhelming the play area.

The main management panel and shared log panel also stay compact vertically, so they end around the clock controls instead of dragging further down the screen. That keeps the garage scene readable underneath the dock shell even on very tall fullscreen displays.

The workstation interaction panel now opens flush to the right side of the playable area and matches the live vertical length of the operations shell instead of overlapping the HUD. It uses the same retro panel treatment as the rest of the management shell and includes its own `Close` button in addition to the existing cancel/interaction close flow.

The computer workstation panel now wraps its upgrade controls and explanatory text instead of growing sideways off-screen. That keeps the longer buy labels and under-button upgrade descriptions readable without overlapping the neighboring dock panels.

The operations tab now keeps its startup finances focused on live cashflow, while the next planned spend has been moved into the progress tab. That gives upgrade planning its own place in the dock without crowding the day-to-day operations view.

## Player Movement
The player now uses `CharacterBody2D` movement with WASD input and a smoothed camera for readable top-down navigation. A simple garage-office layout with walls, a desk zone, and starter props is in place so movement happens in an intentional MVP space instead of an empty scene.

## Workstations
The starter garage now includes a computer workstation and a billing/contracts board, each with its own lightweight interaction panel and visual feedback. The computer shows the initial hardware limits, while the contracts board lists small website-hosting jobs that consume those starter resources and hint at future expansion.

## Gameplay Loop
The starter loop now connects contract review and hosting activation across the two stations. Contracts are reviewed at the board, then activated from the computer workstation, which shows both total and used capacity so the player can see the machine filling up as they take on work.

## Panel Controls
The workstation panels now include real action buttons and a shared status area instead of being read-only info views. This establishes a reusable UI control pattern for both the computer workstation and the contracts board, while keeping the actual contract-selection and activation logic for the next step.

## Contract Actions
The contracts board now uses `Previous` and `Next` controls to browse starter jobs, while the computer workstation provides `Activate Hosting` and `Clear Selection` actions for the currently chosen contract. This makes the panel flow explicit and keeps selection, activation, and capacity changes visible inside the UI instead of relying on hidden state changes.

## Expansion Setup
The room now includes a dedicated external server unit as the first visible expansion target beyond the starter desk machine. Its panel previews the extra capacity it can bring and immediately hints at broader support responsibilities like overflow hosting and managed backups.

## Expansion Flow
The computer workstation can now purchase the external server unit directly from the panel, after which the server appears in the room and immediately joins a shared hardware capacity pool. That shared pool is reflected back in the computer panel so the upgrade feels like a concrete progression step instead of a disconnected room prop.

The starter workstation and first external server now read their hardware stats from JSON files instead of embedding those values directly in the main scene. That keeps the current MVP conservative while making future hardware additions easier to extend through data instead of scene-only edits.

## Economy HUD
The game now includes an always-visible economy HUD with a lightweight dashboard view of cash, recurring income, recurring expenses, and the next notable cost. This gives the project a more immediate tycoon feel without yet pulling the player into a heavy finance simulation.

## Contract Lifecycle
Starter contracts now behave like short-term business units with a defined monthly payout and a fixed duration in months. Once activated, they stay active for their listed term, pay on a monthly-style tick, and then complete automatically so the player can see a full loop from work accepted to revenue earned.

## Financial Pressure
Monthly infrastructure costs now create light pressure through power and internet expenses that scale with active hosting and purchased hardware. This keeps the economy readable while making growth decisions feel more meaningful, since monthly payouts no longer translate directly into pure profit.

## Quality Signals
Active contracts now track a simple satisfaction score, with warnings appearing first when hosting quality gets tight and satisfaction slipping before money is affected. Those issues now also raise closable popup alerts that explain the current service problem and warn the player about escalating repercussions like satisfaction loss, penalties, and cancellations.

The automated checks now cover the core contract loop as well as the docked UI shell. They verify accept and activate flow, monthly payouts and expenses, overload penalties and cancellations, and immediate in-month warning alerts when the player pushes past safe capacity.

## Contract Board Flow
The contracts board now supports accepting leads directly from the panel instead of just browsing them. Accepted contracts are removed from the board, moved into a hosting queue for the computer workstation, and the game now starts with several distinct starter leads instead of a nearly single-contract flow.

The board also now opens a dedicated contracts UI when the player interacts with it in the garage. That panel lists the live available leads, lets the player click to select one directly, and surfaces its monthly revenue, duration, and support expectations before sending that choice back into the hosting flow.

## Over-Capacity Hosting
The player can now activate more hosting work than the current hardware pool can safely support, which pushes the game into a true degraded-service state instead of hard-blocking progress. When the shared pool is overloaded, contracts lose satisfaction, warnings are logged first, penalties begin if the issue persists, and unresolved failures can eventually cancel affected contracts.

## Time Controls
The HUD now uses a clearer Sims-style playback strip with `Start`, `Stop`, `Faster`, and `Reset` controls alongside the visible month, date, and clock. Time steps through a tighter `1x`, `2x`, and `3x` ladder, the active speed is echoed through highlighted speed-state buttons, and the whole clock panel shifts into a visibly paused state when time is stopped while keeping `1x` as the neutral default.

The clock controls are now also validated through the actual UI wiring instead of only through direct method calls. That keeps the visible buttons and the underlying clock behavior in sync, which makes the control flow sturdier for future time-system changes.

For development, the clock panel now also includes a `Next Month` shortcut button that jumps directly to the next monthly rollover. It uses the same month-end processing path as normal time advancement, so payouts, expenses, and summary updates still happen consistently.

## Calendar Economy
The visible calendar now drives the monthly business loop instead of acting as a disconnected display. When the in-game date rolls into a new month, active contracts pay out, recurring infrastructure expenses apply, and the economy HUD stays in sync with the new financial state automatically.

## Monthly Summary
Month rollover now leaves behind a lightweight summary panel so the player can immediately see what changed in the business. The summary calls out payouts, expenses, warnings, and whether any contracts completed or were cancelled, and it can now be dismissed with its `Close` button or the normal cancel input.

The operations tab now includes simple `Continue` and `Reset` session controls backed by a local `user://` JSON save. Progress auto-saves across the current MVP systems, and resetting returns the run to the default garage startup state while clearing the local file.
