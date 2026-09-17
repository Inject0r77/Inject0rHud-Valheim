<p align="center">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/icon.png" alt="Inject0r HUD logo" width="128" height="128">
</p>

<h1 align="center">Inject0r HUD</h1>

<p align="center">
  A configurable client-side HUD for Valheim.<br>
  Timers, durability, production info, profiles and multilingual UI — without requiring anything on the server.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/version-0.5.5-7c5cff?style=flat-square" alt="Version 0.5.5">
  <img src="https://img.shields.io/badge/game-Valheim-4c8eda?style=flat-square" alt="Valheim">
  <img src="https://img.shields.io/badge/BepInEx-5.4.2350-3fb950?style=flat-square" alt="BepInEx 5.4.2350">
  <img src="https://img.shields.io/badge/type-client--side-2ea043?style=flat-square" alt="Client-side">
  <img src="https://img.shields.io/badge/languages-EN%20%7C%20RU%20%7C%20KZ%20%7C%20CN-orange?style=flat-square" alt="Languages">
  <img src="https://img.shields.io/badge/license-source--available-d73a49?style=flat-square" alt="Source-available license">
</p>

<p align="center">
  <a href="https://github.com/Inject0r77/Inject0rHud-Valheim/issues">Issues</a>
  ·
  <a href="#installation">Installation</a>
  ·
  <a href="#controls">Controls</a>
  ·
  <a href="#roadmap">Roadmap</a>
</p>

---

## Overview

Inject0r HUD adds useful information to Valheim without trying to replace the original interface.

The mod is built around small, optional widgets that can be moved, resized and disabled independently. You can keep it minimal with a couple of timers, or turn it into a more detailed HUD with durability, FPS/Ping, profiles and contextual world information.

Everything is client-side. A normal server does not need Inject0r HUD installed.

## Screenshots

### HUD

![Inject0r HUD timers and durability](https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/hud-main-ts.jpg)

![Inject0r HUD FPS and Ping widgets](https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/hud-fps-ping-ts.jpg)

### Settings

<p align="center">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-layout.webp" alt="Layout settings" width="31%">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-durability.webp" alt="Durability settings" width="31%">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-widgets.webp" alt="Widget settings" width="31%">
</p>

<p align="center">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-profiles.webp" alt="Profile settings" width="31%">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-diagnostics.webp" alt="Diagnostics" width="31%">
  <img src="https://cdn.jsdelivr.net/gh/Inject0r77/Inject0rHud-Valheim@main/assets/screenshots/settings-language.webp" alt="Language settings" width="31%">
</p>

---

## Features

### HUD

- Active timed positive effects.
- Equipment durability.
- Native item icons.
- Durability display as units, percent or both.
- Optional Smart Durability modes.
- Separate movable Timers and Durability blocks.
- Drag & resize directly in edit mode.
- Snap to screen edges and center.
- Adjustable scale, opacity and text size.

### FPS & Ping

Small independent widgets for:

- FPS;
- network ping;
- optional labels;
- separate opacity and position.

Both widgets can be disabled completely.

### World hover info

Look at an object to see extra information without opening another window.

Currently supported:

- picked berry bushes and other respawning `Pickable` objects;
- planted crops;
- beehives;
- fermenters;
- smelters and other compatible production stations.

Examples of information that may be shown:

- time until a bush or resource respawns;
- time until a planted crop grows;
- current honey amount;
- time until the next honey is produced;
- fermenter contents and remaining time;
- production queue;
- available fuel;
- time until the next output.

Inject0r HUD does not guess timers. If the client cannot determine a trustworthy value, the ETA is simply omitted.

### Profiles

Built-in profiles are included for quick setup.

You can also:

- create your own profiles;
- save the current HUD layout and module state;
- rename and delete custom profiles;
- export a profile as a compact code;
- import a profile from another player.

### Languages

Inject0r HUD has its own interface language setting.

Supported languages:

- **English** — default;
- **Русский**;
- **Қазақша**;
- **简体中文**;
- **Auto**.

`Auto` follows the Valheim language when that language is supported. Otherwise Inject0r HUD falls back to English.

Vanilla item names and status-effect names still use Valheim's own localization whenever possible.

---

## Controls

| Key | Action |
| --- | --- |
| `F8` | Show / hide Inject0r HUD |
| `F10` | Open edit mode and settings |

While edit mode is active, HUD blocks can be dragged and resized directly on screen.

---

## Installation

### Thunderstore / r2modman

The recommended installation method is through a Thunderstore-compatible mod manager.

1. Install **BepInExPack for Valheim**.
2. Install **Inject0r HUD**.
3. Launch Valheim.

### Manual installation

1. Install **BepInExPack for Valheim**.
2. Copy `Inject0rHUD.dll` into:

```text
BepInEx/plugins/Inject0rHUD/
```

3. Launch the game.

The configuration file is created automatically on first run.

---

## Configuration

Most settings can be changed live in-game through `F10`.

Main BepInEx config:

```text
BepInEx/config/inject0r.Inject0rHUD.cfg
```

Custom profiles:

```text
BepInEx/config/Inject0rHUD.profiles
```

---

## Client-side behavior

Inject0r HUD is designed to remain read-only from the game's point of view.

It does **not** add:

- ServerSync;
- custom RPCs;
- world writes;
- character-save writes;
- freebuild;
- inventory modification;
- gameplay stat changes.

The mod only reads state already available to the local client and draws additional UI.

A server can still block client mods through its own whitelist or anti-cheat configuration.

---

## Compatibility

The mod currently targets Valheim with BepInEx 5.

Some features read internal Valheim state through reflection so the mod can stay client-side and avoid unnecessary hard dependencies. If a future Valheim update changes one of those internals, the affected module should fail safely instead of modifying the world or character.

If a specific hover timer stops appearing after a game update, check for a newer Inject0r HUD release.

---

## Building from source

Requirements:

- .NET SDK;
- Valheim;
- BepInExPack for Valheim.

PowerShell:

```powershell
$env:VALHEIM_DIR = "E:\SteamLibrary\steamapps\common\Valheim"
.\build.ps1
```

If BepInEx is installed somewhere unusual, set `BEPINEX_DIR` manually.

Example:

```powershell
$env:BEPINEX_DIR = "C:\Path\To\BepInEx\core"
.\build.ps1
```

To create the Thunderstore package after a successful Release build:

```powershell
.\package.ps1
```

The finished archive will be created in:

```text
dist/
```

---

## Roadmap

Planned ideas for future versions:

- context widgets for ships;
- in-game day / time information;
- sunrise and sunset indicators;
- theme system;
- built-in Valheim-style theme;
- custom HUD colors;
- user-created themes;
- more languages;
- more contextual world information where the client exposes reliable data.

The goal is to keep every major module optional so the HUD can stay as simple or as detailed as the player wants.

---

## Contributing

Bug reports and suggestions are welcome through the GitHub issue tracker:

https://github.com/Inject0r77/Inject0rHud-Valheim/issues

When reporting a bug, include:

- Valheim version;
- Inject0r HUD version;
- BepInEx version;
- whether the issue happens in single-player or on a server;
- `LogOutput.log` if the problem causes errors.

---

## License

Inject0r HUD is **source-available, not open source**.

You may use official releases, inspect the source and make local personal modifications. Redistribution, rehosting, republishing, public forks/derivative releases, copying substantial parts into another project, and commercial use are not permitted without prior written permission.

Copyright © 2026 **Inject0r77**. All rights reserved.

See the full [LICENSE](LICENSE) for the exact terms.
