# SaveDuringPlay

A lightweight, zero-overhead framework for Unity that automatically preserves component state modifications made during Play Mode back into Edit Mode.

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Unlike standard engine mechanisms or Cinemachine's specific `SaveDuringPlay` tool, this package offers a generalized, non-invasive architecture optimized for game balancing, level design tweaking, and real-time inspector modification without manual data copying.

## Table of Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Installation](#installation)
- [Usage](#usage)
- [Performance & Production Stripping](#performance--production-stripping)
- [License](#license)

## Features

- **Zero-Configuration Attribute Syntax:** Simply decorate any custom `MonoBehaviour` script with `[SaveDuringPlay]`.
- **Rename & Reparent Robustness:** Powered by an automated, serialized GUID architecture that tracks components even if objects are renamed or shifted in the hierarchy during runtime.
- **Production-Stripped Design:** The runtime footprint compiles down to a completely empty shell in standalone builds, ensuring **zero** memory and processing overhead in production.
- **Full Undo Support:** All modifications restored upon exiting Play Mode are fully integrated into Unity's `Undo` ecosystem (Ctrl+Z supported).
- **Multi-Scene & Deactivated Object Support:** Seamlessly caches state variables across additive scene configurations and handles inactive GameObjects.

## How It Works

When entering or exiting Play Mode, Unity completely flushes the C# domain and destroys runtime clones. `SaveDuringPlay` bypasses this by utilizing a decoupled editor serialization bridge:

1. **Injection:** The framework automatically injects a hidden, serialized GUID anchor onto target GameObjects via custom Editor hooks upon script compilation or selection.
2. **Snapshot Caching:** Upon clicking **Stop** (`ExitingPlayMode`), the tool evaluates all registered targets, serializes their public and serialized states into a strict string-to-string JSON dictionary cache, and bridges across the boundary.
3. **Restoration:** Once settled in Edit Mode (`EnteredEditMode`), the cache is decompressed and safely overwritten onto the freshly reloaded Edit-mode component structures via `EditorJsonUtility`.

## Installation

### Via Git URL (Recommended)

Requires Unity 2022.3 or higher. Open `Window -> Package Manager`, click the `+` icon, select `Add package from git URL...`, and enter the following URL:

[https://github.com/ProjectGrinder/SaveDuringPlay.git?path=src/SaveDuringPlay/Assets/Plugins/SaveDuringPlay](https://github.com/ProjectGrinder/SaveDuringPlay.git?path=src/SaveDuringPlay/Assets/Plugins/SaveDuringPlay)

## Usage

Simply slap the `[SaveDuringPlay]` attribute onto any class inheriting from `MonoBehaviour`.

```csharp
using UnityEngine;
using Takayama.SaveDuringPlay;

[SaveDuringPlay]
public class EnemyBalancer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.5f;
    [SerializeField] private int maxHealth = 100;
    
    // Tweak these values dynamically in the Inspector while playing!
    // When you click Stop, your changes are permanently preserved.
}
```

That's it! The workflow is completely transparent. The backing system takes care of the infrastructure silently behind the scenes.

## Performance & Production Stripping

Architectural efficiency is a core priority of this framework. To prevent editor tooling data from leaking into deployment pipelines, the runtime markers use conditional compilation boundaries combined with `ISerializationCallbackReceiver`:

```csharp
// High-level overview of the internal compilation footprint in standalone builds:
public sealed class SaveDuringPlayMarker : MonoBehaviour 
{
    // All GUID allocations, reflection lookups, and serialization logic 
    // are entirely stripped out by the compiler in non-editor environments.
}
```

In production standalone builds, the component has **zero fields**, consumes **zero bytes of memory**, and performs **no lifecycle execution**.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
