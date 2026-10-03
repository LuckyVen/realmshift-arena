# REALMSHIFT ARENA

A complete playable Godot 4.5.1 / C# arena campaign made for John Gabriel Venenoso.

**40 campaign levels • 4 large realms • 4 three-phase guardians • 8 weapons • 5 elements • 8 heroes • 28 enemy variants • 67 relics • character creation • persistent discoveries • endless mode**

## Play the Windows game

1. Download the complete ZIP and right-click it → **Extract All**.
2. Open **WindowsGame**.
3. Double-click **RealmshiftArena.exe**.
4. Click **START**, choose a traveler, customize their appearance if you want, select a weapon and element, and click **ENTER THE REALM**.

Keep the EXE, the `.pck`, and the `data_RealmshiftArena_windows_x86_64` folder together. The exported game includes its .NET runtime; editing the source requires the SDK separately.

## Open the editable source

1. Install the **.NET 8 SDK, Windows x64** from https://dotnet.microsoft.com/en-us/download/dotnet/8.0 — select **SDK**, not only Runtime.
2. Download **Godot 4.5.1 .NET, Windows x64** from https://godotengine.org/download/archive/4.5.1-stable/.
3. Extract Godot, keeping its accompanying `GodotSharp` folder.
4. Open the Godot .NET editor. Click **Import** and select `Source/RealmshiftArena/project.godot`.
5. Let the assets import. Click **Build** in the editor and then press **F5**.

Naka-configure na ang scenes, scripts, Input Map, collision layers, camera, resources, and UI. Hindi mo kailangang isa-isang i-attach ang scripts or gumawa ulit ng nodes.

For VS Code, open `Source/RealmshiftArena`, open a new terminal, and run:

```powershell
dotnet build
```

Use Godot **F5** to launch the full game. `Scenes/Main.tscn` is the entry scene.

## Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Aim | Mouse | Right stick |
| Primary | Left mouse | RT |
| Bow | Hold primary, release to shoot | Hold RT, release |
| Weapon skill | Right mouse | LT |
| Dash | Space | A |
| Hero ability | Q | Y |
| Interact | E | X |
| Switch slots | 1 / 2 | LB / RB |
| Pause | Esc | Start |

Controls can be rebound in Settings → Controls. Rebinding an occupied keyboard key swaps the bindings. Auto aim, reduced flashes, screen shake, particle density, damage numbers, sound levels, fullscreen, resolution, and VSync are configurable.

## Your first run

Aster and Lyra are available immediately. The wand, longbow, and rune blade are initial weapons. The first run teaches movement, dash, and attacking; Enter skips training. Caches and healing shrines are marked on the minimap. Approach one and press E.

- Defeating enemies drops XP. Rank increases pause the run and offer three relic cards.
- **Campaign LEVEL** means the current wave; **RANK** means your XP progression within this run.
- Clear all remaining spawns and enemies to advance a normal level.
- Guardians appear at levels **10, 20, 30, and 40**.
- After a guardian, the ground changes outward from the arena, scenery reacts, the realm is rebuilt, collision routes change, and a gate opens. The minimap shows the gate. Press E beside it to continue.
- The first guardian unlocks weapon slot 2. Later guardians unlock more starting weapons.
- Caches can reveal any of the eight weapon classes and any of six rarity tiers, even before that weapon is available as a starting loadout. Taking the weapon records its discovery.
- Realm Shards persist. Spend 60 in Characters to unlock another traveler. Cosmetics change appearance; each traveler's Q ability determines their role.
- Defeat the final guardian to unlock **Realm Collapse**, beginning at level 41 with mixed terrain and escalating returning bosses.

Health has a damage system, recovery pickups, regeneration relics, armor, and shields. Dash gives a short invulnerability window. Enemy projectiles are rose-colored; persistent hostile zones and beams warn before dealing damage.

## Contents

- `Scenes/Main.tscn` — complete startup/menu/campaign entry.
- `Scenes/Characters/Player.tscn` — configured player body, collision, and layered avatar.
- `Scripts/` — modular C# systems.
- `Resources/` — editable Godot Resources for weapons, enemy stats, heroes, realms, and relics.
- `Assets/` — original pixel sprites, font, tiles, props, VFX icons, and synthesized music/SFX.
- `Tools/` — asset and resource generation tools.
- `Docs/Architecture.md` — node hierarchy, attachment map, collision setup, extension guide, and phase coverage.
- `Docs/Testing.md` — verification evidence and manual playtest checklist.

## Build another Windows version

Open Project → Export → **Windows Desktop**. Install the **matching 4.5.1 .NET export templates** when prompted. Set the export destination and click Export Project. The .NET SDK must be installed for this step.

An optional Linux QA export preset is included for developers testing packaged resource loading.

## Saves

Godot stores the profile in `user://realmshift-save.json`. On Windows this is normally under `%APPDATA%\Godot\app_userdata\REALMSHIFT ARENA\`. Shards, hero/weapon unlocks, mastery, boss records, highest level, achievements, relic/enemy discoveries, settings, and the custom avatar persist. Temporary run relics and health reset at the next run. Writes use a temporary file followed by replacement; corrupt profiles are preserved with a `.corrupt` suffix.

## Release status

This is a complete playable campaign build with compact original pixel art and synthesized audio. Levels are wave-driven, using reusable enemy variants and systems across large seeded battlegrounds. It is not a claim of a finished commercial Steam release. Native Windows hardware testing, wider balance playtesting, bespoke frame-by-frame animation refinement, a composed final soundtrack, Steamworks integration, and release certification remain production work. The current Windows export was produced successfully; runtime integration and visual QA were performed on Linux with the matching Godot engine.
