# Infrastructure Tycoon

A PC-first Unity 2D management simulation about growing a single-PC hosting business into a cloud infrastructure provider.

## Development

Use **Unity 6.6 / 6000.6.9f1** (revision `8c45277fb9c5`), matching `ProjectSettings/ProjectVersion.txt`. This project deliberately uses the installed supported release rather than LTS.

Add this repository root in Unity Hub and open it with that editor. Allow Package Manager and asset import to finish. Commit Assets (including `.meta` files), Packages manifests and lockfile, and ProjectSettings; generated editor folders are ignored.

The foundation uses the built-in renderer, 2D editor defaults, Sprite tooling, uGUI, and Unity Test Framework. Open `Assets/Scenes/Main.unity` and enter Play Mode to see the anchored HUD shell. Date, cash, speed, and alerts are placeholders; no simulation is connected yet. The plain C# SimulationClock is implemented; `FOUND-04` binds its date and speed controls. Run the Edit Mode clock tests and Play Mode scene suite from Window > General > Test Runner. The same suite can run in a standalone Windows test player to verify real window resizing at 1280x720, 800x600, and 1920x1080; screenshots are saved under Application.temporaryCachePath.

Design and task queue: [GDD](docs/GDD), [TDD](docs/TDD), [phases](docs/phases), [SPEC](docs/SPEC.md). Contributor workflow: [AGENTS](docs/AGENTS.md).
