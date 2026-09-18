# Changelog

## 0.6.0

### Added

- Added the new **Ship Widget**, shown while controlling a ship.
  - Hull health.
  - Speed in knots and m/s.
  - Wind strength and relative direction.
  - Propulsion / sail state.
  - Sail efficiency.
  - Rudder position.
- Added the new **World Time Widget**.
  - Current Valheim day.
  - Approximate 24-hour in-game clock.
  - Next sunrise / sunset time and countdown.
- Added independent enable/disable, opacity and positioning controls for Ship and World Time widgets.
- Added a **whole-station production hover** option for Smelter-based stations.
  - When enabled, production information can be viewed while aiming at the station body/output area.
  - When disabled, information is only shown on Valheim's native input/output interaction points.
- Profile export format advanced to `IHUD6`.
- Added profile support for Ship Widget, World Time Widget and independent Timers / Durability scales.
- Older `IHUD4` and `IHUD5` profile codes remain import-compatible.

### Improved

- Production hover information now hooks the actual Valheim Smelter interaction callbacks.
- Smelter-based stations can show queue size, available fuel where applicable and next-output countdown when a reliable ETA is available.
- Beehive next-honey countdown now updates smoothly between Valheim's slower internal production updates using read-only projected progress.
- Improved contextual hover support without adding world writes, custom RPCs or server requirements.

### Fixed

- Fixed separate **Timers** and **Durability** panels sharing the same resize scale.
- Fixed production information not appearing on Smelter-based stations because Valheim routes their hover text through separate interaction callbacks.
- Fixed duplicate production information when whole-station hover and a native interaction point overlap.
- Fixed F10 Edit Mode mouse capture / gameplay input so moving the mouse in the editor no longer rotates the player or camera.

### Notes

- Windmill wall-clock ETA is intentionally omitted because its production rate changes with live wind strength.
- Inject0r HUD remains client-side only.
- No ServerSync, custom RPCs, world writes, character-save writes, inventory modification or gameplay-stat changes were added.

## 0.5.0

- Added a standalone Inject0r HUD localization framework.
- Added English, Russian, Kazakh and Simplified Chinese UI translations.
- English is now the default interface language.
- Added F10 Language tab with manual EN/RU/KK/ZH-CN selection and optional Auto mode.
- Added English fallback for missing translation keys.
- Localized the settings UI, HUD section labels, edit-mode helper text, diagnostics, profile UI/status messages and custom world-hover labels.
- Kept native Valheim item/StatusEffect names on Valheim's own localization path where possible.
- Added beehive hover information: honey amount and next-honey countdown using vanilla `product` progress and `m_secPerUnit`.
- Added fermenter hover information: contents and exact remaining time using vanilla `GetFermentationTime()` and `m_fermentationDuration`.
- Added Smelter-based production hover information: queue, fuel and next-output timer when a reliable ETA can be derived from vanilla `bakeTimer` and `m_secPerProduct`.
- Windmills deliberately omit the wall-clock ETA because live wind changes throughput; queue/state is still shown.
- Added separate toggles and opacity controls for beehives, fermenters and production stations.
- Profile export format advanced to `IHUD5`; 0.5.0 still imports older `IHUD4` profile codes.
- No ServerSync, custom RPC, world writes, character-save writes or gameplay stat changes added.

## 0.4.2

- Removed Forsaken Power cooldown from the custom TIMERS block; Valheim already displays that cooldown in its native HUD.
- Replaced separate Rested/cooldown readers with a generic active-positive-effect timer reader.
- TIMERS now shows currently active timed positive StatusEffects and their actual remaining effect duration.
- Active Forsaken Powers now count down their active buff duration rather than the ability cooldown.
- Negative/debuff effects and permanent `ttl <= 0` effects are excluded.
- Kept legacy config/profile fields internally so existing 0.4.x profile export codes remain import-compatible.
- No server-side component, ServerSync, custom RPC, world writes, or character-save writes added.

## 0.4.1

- Localized Rested timer names through the actual Valheim StatusEffect token.
- Localized Forsaken Power cooldown names by resolving the registered guardian-power StatusEffect from ObjectDB.
- Added safe token fallbacks for compatible/modded guardian powers.
- Replaced the fixed F10 settings body with a clipped scroll view.
- Added independent scroll position per F10 settings tab.
- Added dynamic wrapped checkbox height for long Russian labels.
- Added safer slider label spacing so Russian text cannot overlap the following control.
- No server-side component, ServerSync, custom RPC, world writes, or character-save writes added.

## 0.4.0

- Added native Valheim item icons to durability rows.
- Added optional Smart Durability modes: Off, BelowThreshold, CurrentItemOnly.
- Added optional CompactInCombat mode; disabled by default.
- Added built-in Minimal / Compact / Full profiles.
- Added custom profile create/save/rename/delete.
- Added live profile switching.
- Added one-line profile import/export with clipboard workflow.
- Added optional separate Timers and Durability blocks.
- Added screen-edge and screen-center snapping.
- Added per-section reset actions.
- Preserved live in-game opacity controls for bush/resource and planted-crop hover timers.
- Added Rested / Forsaken Power toggles and Timers reset to the Widgets tab.
- Added independent draggable FPS and Ping cubes.
- Added per-widget value/label/opacity settings.
- Added local network ping reading with safe fallback.
- Added F10 Diagnostics tab with mod / Valheim / Unity / BepInEx information.
- Preserved client-only operation: no ServerSync, custom RPC, world writes, or character-save writes.

## 0.3.0

- Added an in-game live settings panel to F10 Edit Mode.
- HUD background opacity now changes live with a slider.
- HUD scale and font size can be changed live.
- Added in-game section-header toggle.
- Added in-game durability display mode switch.
- Added in-game toggles for Rested, Forsaken Power cooldown, and durability modules.
- Split world hover timers into separate Pickable and Plant toggles.
- Added separate live opacity controls for bush/resource timers and planted-crop timers.
- Added live timer opacity previews to the settings panel.
- Added a Reset HUD Appearance button.
- Settings save when F10 Edit Mode closes.
- Remains client-side only; no ServerSync, custom RPC, world writes, or character-save writes.

## 0.2.1

- Fixed build against raw Valheim 1.0.14 assemblies where `Pickable.m_nview`, `Plant.m_nview`, `Plant.GetGrowTime()` and `Plant.TimeSincePlanted()` are not directly exposed to the compiler.
- World hover timers now obtain `ZNetView` through Unity components and call Valheim plant timing methods defensively through reflection.
- Added a fallback path for plant timing using `plantTime`, `m_growTime`, and `m_growTimeMax`.
- Restored the missing `Inject0rHUD.Util` import in `DurabilityService`.
- No server component, ServerSync, custom RPC, world writes, or character-save writes added.

## 0.2.0

- Removed active food timers from the custom HUD because Valheim already shows them in the vanilla HUD.
- Added client-side world hover timers:
  - picked/respawning bushes and other Pickable objects;
  - healthy planted crops.
- Added F10 HUD Edit Mode.
- Added mouse drag repositioning.
- Added resizing from all four edges and all four corners.
- Added gameplay-input suppression and cursor release while Edit Mode is active.
- Added persistent position/size saving.
- Added responsive value widths and ellipsis clipping so names do not overlap values at smaller sizes.
- Added a minimum readable panel width and minimum font size.
- Added Harmony reference from BepInEx core; no additional Thunderstore dependency is required.
- Still no ServerSync, custom RPC, world writes, character-save writes, or server installation.

## 0.1.3

- Durability is now shown as exact units by default, e.g. `200/200`, instead of only `100%`.
- Added `Durability.ValueMode`: `Units`, `Percent`, or `Both`.
- Made the default HUD layout more compact: smaller padding, rows, headers, bars, font, and panel width.
- Reduced default background opacity slightly.

## 0.1.1

- Removed the compile-time dependency on Valheim's `Localization` type.
- Added defensive runtime localization lookup so current/future Valheim assembly layout changes do not stop the mod from compiling.
- Localization failure is now cosmetic only; the HUD falls back to raw item tokens/names.

## 0.1.0

Build fix:

- Added `assembly_utils.dll` reference required for Valheim `Localization`.
- Added `UnityEngine.TextRenderingModule.dll` reference required for `TextAnchor` and `FontStyle`.

Initial MVP.

- Added client-only BepInEx plugin.
- Added food remaining timers.
- Added Rested remaining timer.
- Added Forsaken Power cooldown timer.
- Added equipped-item durability HUD.
- Added durability warning and critical thresholds.
- Added configurable HUD position, scale, font size and F8 visibility toggle.
- Added defensive reflection and throttled non-fatal module logging.
- Added one-click build/package PowerShell scripts.
- No Harmony patches, ServerSync, RPC, world writes or character-save writes.
