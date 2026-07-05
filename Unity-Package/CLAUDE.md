# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Unity Package Structure

This is a Unity package project for the **Image Loader** library - an asynchronous image loading system with two-layer caching (Memory and Disk). The package is located at `Assets/root/` within the Unity project.

### Key Architecture Components

1. **Core System (`ImageLoader.cs`)**: Static entry point providing loading methods for `Sprite` and `Texture2D`
2. **Future System**: Async operation wrapper (`IFuture<T>`, `Future<T>`) for handling loading lifecycle
3. **Reference System**: Memory management for `Reference<T>` to prevent memory leaks
4. **Cache Layers**: Memory cache (fast) and Disk cache (persistent) with configurable settings
5. **Consumer System**: Extension methods to directly set loaded images into Unity components

### Assembly Structure

- **`Extensions.Unity.ImageLoader`**: Main runtime assembly (depends on UniTask)
- **`Extensions.Unity.ImageLoader.Tests`**: Shared test utilities
- **`Extensions.Unity.ImageLoader.Tests.Editor`**: Editor-specific tests (NUnit)
- **`Extensions.Unity.ImageLoader.Tests.Runtime`**: Runtime tests (NUnit)
- **`Extensions.Unity.ImageLoader.Samples`**: Example code and usage samples

## Development Commands

### Testing
- **Editor Tests**: Use Unity Test Runner window (Window → General → Test Runner) - Editor tab
- **Runtime Tests**: Use Unity Test Runner window (Window → General → Test Runner) - PlayMode tab
- **Command Line**: Unity supports running tests via batch mode with `-runTests` parameter

### Building/Development
- Unity package development uses Unity Editor directly
- No external build system (npm/gradle/etc) - Unity handles compilation
- Package validation through Unity Package Manager

### Dependencies
- **UniTask 2.5.10**: Async/await support for Unity (via OpenUPM)
- **Unity Test Framework 1.4.6**: Testing framework
- **Unity uGUI 1.0.0**: UI components for image consumers

## Code Architecture Patterns

### Loading Pattern
```csharp
// Basic loading
await ImageLoader.LoadSprite(url)

// With consumer (auto-assignment)
await ImageLoader.LoadSprite(url).Consume(image)

// With lifecycle events
ImageLoader.LoadSprite(url)
    .LoadedFromMemoryCache(sprite => ...)
    .LoadedFromDiskCache(sprite => ...)
    .LoadedFromSource(sprite => ...)
    .Then(sprite => ...)
    .Failed(ex => ...)
    .Consume(image)
    .Forget();
```

### Memory Management
- Use `LoadSpriteRef()` instead of `LoadSprite()` for automatic memory management
- References are disposed when target components are destroyed
- Manual disposal via `reference.Dispose()` or `reference.DisposeOnDestroy(component)`

### Caching System
- Memory cache: Fast access, cleared on low memory
- Disk cache: Persistent across sessions (not available on WebGL)
- Both layers configurable globally via `ImageLoader.settings` or per-request

## Testing Approach

Tests are organized into:
- **Base utilities** (`Assets/root/Tests/Base/Utils/`): Shared test helpers and fake implementations
- **Editor tests**: Unit tests running in editor mode
- **Runtime tests**: Integration tests running in play mode

Test structure uses:
- `FakeFuture`: Mock implementation for testing lifecycle events
- `TestUtils`: Loading utilities with test image URLs
- `FutureListener`: Event tracking for async operations

## Key Files and Locations

- Main API: `Assets/root/Runtime/ImageLoader.cs`
- Future system: `Assets/root/Runtime/Future/Future.cs` and related files
- Extensions: `Assets/root/Runtime/Future/Extensions/FutureEx.*.cs`
- Cache implementation: `Assets/root/Runtime/ImageLoader.Cache.*.cs`
- Sample usage: `Assets/root/Samples/Sample*.cs`
- Package definition: `Assets/root/package.json`

## Common Development Tasks

### Adding New Features
1. Implement core functionality in `Assets/root/Runtime/`
2. Add extension methods in `Assets/root/Runtime/Future/Extensions/` if needed
3. Create sample usage in `Assets/root/Samples/`
4. Add tests in `Assets/root/Tests/Editor/` and `Assets/root/Tests/Runtime/`

### Testing New Code
1. Create test methods in appropriate test assembly
2. Use `TestUtils` helpers for loading operations
3. Use `FutureListener` for tracking async events
4. Test both success and failure scenarios

### Performance Considerations
- Memory management critical due to large Texture2D objects
- Use Reference system for automatic cleanup
- Consider cache settings impact on memory usage
- Test WebGL compatibility (no disk cache)