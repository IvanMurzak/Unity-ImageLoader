# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository shape: two Unity projects + one shippable package

This repo is not a single Unity project. It contains **two independent Unity projects** plus the library source that one of them ships:

- **`Unity-Package/`** — the Unity project used to develop and test the actual **Image Loader** library. The distributable package is the subfolder `Unity-Package/Assets/root/` (its own `package.json`, published to OpenUPM as `extensions.unity.imageloader`). Everything else under `Unity-Package/` (ProjectSettings, Library, .sln/.csproj) is Unity scaffolding, not shipped. **All library tests run against this project** (`projectPath: ./Unity-Package`). See `Unity-Package/CLAUDE.md` for the library's internal architecture.
- **`Installer/`** — a *separate* Unity project whose only job is to export a `.unitypackage` ("ImageLoader-Installer") that adds the OpenUPM scoped registry to a consumer's `manifest.json`. Entry point `Installer/Assets/com.IvanMurzak/Image Loader Installer/Installer.cs` runs at editor load (`[InitializeOnLoad]`) and injects the registry; `PackageExporter.ExportPackage` builds the `.unitypackage`. It carries its own `Version` constant that must match the package version.
- **`docs/`**, **`README.md`**, **`bump-version.ps1`**, **`.github/`** — repo-level assets shared by both projects.

When editing library code you almost always work in `Unity-Package/Assets/root/`; the `Installer/` project is only touched for install/registry logic.

## Versioning (must stay in sync across two files)

The package version lives in **two** places that are kept identical:

- `Unity-Package/Assets/root/package.json` → `"version"`
- `Installer/Assets/com.IvanMurzak/Image Loader Installer/Installer.cs` → `public const string Version`

Never edit these by hand independently. Use the script, which updates both atomically:

```powershell
./bump-version.ps1 -NewVersion "7.1.0"          # apply
./bump-version.ps1 -NewVersion "7.1.0" -WhatIf  # preview without writing
```

## Testing

Tests are Unity Test Framework (NUnit) tests inside `Unity-Package/`, split across three assemblies:

- `Tests/Base/` (`Extensions.Unity.ImageLoader.Tests`) — shared utilities, no tests
- `Tests/Editor/` (`…Tests.Editor`) — EditMode
- `Tests/Runtime/` (`…Tests.Runtime`) — PlayMode

**Run locally:** open the `Unity-Package` project → Window → General → Test Runner → *EditMode* / *PlayMode* tab. Run a single test by selecting it in that tree (there is no CLI-single-test shortcut short of Unity batch mode `-runTests -testFilter`).

### Test network is faked, not real
The library issues web requests through the injectable `IWebRequestProvider` (`ImageLoader.settings.webRequestProvider`, default `DefaultWebRequestProvider`). Tests swap in `MockWebRequestProvider`, which routes every URL to an **in-process localhost HTTP server** (`TestHttpServer`) instead of the public internet — this is what makes image-loading tests deterministic. Wiring lives in `Tests/Base/Utils/TestUtils.cs`:

- `TestUtils.BeginHold(url)` / `ReleaseHeld(url)` — park a request in-flight on the server so "cancel-while-loading" states are reproducible. Always pair them.
- Registered URLs resolve to a fast local image; unregistered URLs hit the server's slow route so client-side timeouts fire predictably.

If you add a test that loads an image, register/route it through `TestUtils` — do not point it at a real remote URL.

### Headless-CI caveat
Finalizer/GC-driven reference-cleanup tests are non-deterministic under Unity's conservative collector in `-batchmode`. `TestUtils` skips them when `Application.isBatchMode` is true (they still run in the interactive Editor Test Runner). Don't "fix" such a test by forcing it to run headless — verify it in the Editor.

## CI

Reusable workflow `.github/workflows/test_unity_plugin.yml` runs the Unity Test Runner for one `(unityVersion, testMode)` across a platform matrix (`base`, `windows-mono`) on ubuntu Docker images.

- **`test_pull_request.yml`** fans that out over Unity 2019.4 → 6000.0 in both `editmode` and `playmode`.
- **`release.yml`** (on tag) runs the full matrix, then signs & packs the UPM tarball (`upm pack` from `Unity-Package/Assets/root`) and exports the Installer `.unitypackage` (Installer project pinned to Unity 2021.3.45f1).

PRs from forks run with `pull_request_target` and require the **`ci-ok`** label; the workflow aborts if a PR also edits files under `.github/workflows/` (secrets safety).
