# Infrastructure Tycoon

A PC-first Unity 2D management simulation about growing a single-PC hosting business into a cloud infrastructure provider.

## Development

Use **Unity 6.6 / 6000.6.9f1** (revision `8c45277fb9c5`), matching `ProjectSettings/ProjectVersion.txt`. This project deliberately uses the installed supported release rather than LTS.

Add this repository root in Unity Hub and open it with that editor. Allow Package Manager and asset import to finish. Commit Assets (including `.meta` files), Packages manifests and lockfile, and ProjectSettings; generated editor folders are ignored.

The foundation uses the built-in renderer, 2D editor defaults, Sprite tooling, uGUI, and Unity Test Framework. No playable scene or gameplay code exists yet. `FOUND-02` adds Main and the HUD; `FOUND-03` adds the simulation clock. Once tests exist, run them from Window > General > Test Runner.

Design and task queue: [GDD](docs/GDD), [TDD](docs/TDD), [phases](docs/phases), [SPEC](docs/SPEC.md). Contributor workflow: [AGENTS](docs/AGENTS.md).
