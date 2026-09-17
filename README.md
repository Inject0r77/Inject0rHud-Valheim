# Inject0r HUD

Inject0r HUD adds a small set of client-side quality-of-life widgets to Valheim without changing gameplay or requiring anything on the server.

The HUD can stay simple — just a couple of timers — or be split into separate movable blocks with durability, FPS/Ping and world-object information.

## Features

- Active timed positive effects.
- Equipment durability with native item icons.
- Optional Smart Durability modes.
- Separate draggable/resizable Timers and Durability blocks.
- Snap to screen edges and center.
- Small FPS and Ping widgets.
- Built-in and custom HUD profiles.
- Profile import/export through a compact code.
- Live configuration in-game with **F10**.

### World hover info

Look at an object to see extra information without opening another window.

Supported right now:

- picked berry bushes and other respawning `Pickable` objects;
- planted crops;
- beehives — honey count and next honey timer when the client has an exact value;
- fermenters — contents and remaining fermentation time;
- Smelter-based production objects — queue, fuel and next output timer when Valheim exposes enough information.

Windmills do not get a fake exact ETA: their production speed changes with wind, so Inject0r HUD only shows information that can be read reliably.

Each world-hover module can be disabled separately.

## Languages

The mod has its own interface language setting under **F10 → Language**.

Available in 0.5.0:

- English — default
- Русский
- Қазақша
- 简体中文
- Auto

`Auto` follows Valheim when the detected language is supported. Otherwise the mod falls back to English.

Vanilla item names and status-effect names still use Valheim's own localization whenever possible.

## Controls

| Key | Action |
| --- | --- |
| **F8** | Show / hide Inject0r HUD |
| **F10** | Open edit mode and settings |

In edit mode, HUD blocks can be dragged and resized directly on screen.

## Client-side only

Only the player using Inject0r HUD needs to install it.

The mod does not use ServerSync or custom RPCs and does not write to the world or character save. It only reads information already available to the client and draws additional UI.

If the client cannot determine a trustworthy timer, the timer is omitted instead of guessed.

## Installation

The easiest way is through a Thunderstore-compatible mod manager.

For a manual install:

1. Install **BepInExPack for Valheim**.
2. Copy `Inject0rHUD.dll` to:

```text
BepInEx/plugins/Inject0rHUD/
```

3. Start Valheim once. The config file will be created automatically.

## Configuration

Most settings can be changed live with **F10**.

The regular BepInEx config is stored in:

```text
BepInEx/config/inject0r.Inject0rHUD.cfg
```

Custom HUD profiles are stored separately in:

```text
BepInEx/config/Inject0rHUD.profiles
```

## Compatibility notes

Inject0r HUD is intentionally read-only and tries to fail safely if a Valheim update changes an internal field or method.

A server can still block client mods through its own whitelist or anti-cheat setup.

If a world-object timer disappears after a game update, check the mod page for an updated build before assuming the object itself is broken.

## Building from source

Requirements:

- .NET SDK
- Valheim
- BepInExPack for Valheim

PowerShell:

```powershell
$env:VALHEIM_DIR = "E:\SteamLibrary\steamapps\common\Valheim"
.\build.ps1
```

`build.ps1` can also use `BEPINEX_DIR` if BepInEx is installed somewhere unusual.

To create the Thunderstore package after a successful Release build:

```powershell
.\package.ps1
```

The resulting archive is written to `dist/`.

## Current version

**0.5.0**

See [CHANGELOG.md](CHANGELOG.md) for release notes.

## License

MIT
