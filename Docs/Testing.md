# Validation and playtesting

## Automated and visual validation

The project is compiled with Godot.NET.Sdk **4.5.1**, targeting **net8.0**. The final source is checked with the C# compiler and matching Godot .NET engine. Validation covers:

- Catalog sizes: 8 weapons, 28 enemy variants, 8 heroes, 4 realms, 67 relics.
- Construction of all menu pages, save roundtrip, avatar persistence.
- Actual projectiles defeating enemies and bounded projectile reuse.
- Pause freezing enemy movement; XP freezing the run for three upgrade choices.
- All four scheduled guardians; health-driven phase changes; all four victories.
- Physical world rebuild and portal progression after the first three guardians.
- Level-40 victory, endless unlock, level-41 entry, and return-to-menu cleanup.
- Invoking all eight primary and secondary weapon implementations.
- Packaged resource discovery, because editor-only filesystem assumptions can fail after export.
- Rendered menus, character selection, customization, loadout, every realm, boss HUD, world shift, armory, and settings.

The smoke harness uses its own `smoke-save.json`; it does not replace a normal player profile. Run it from a terminal using the full path to a Godot .NET executable:

```powershell
& "C:\Path\To\Godot_v4.5.1-stable_mono_win64.exe" --headless --path . -- --smoke
```

Successful output ends in `SMOKE PASS`. The harness is intentionally allowed to defeat guardians immediately after checking phase transitions; it proves progression wiring, not human combat balance.

## Manual playtest

1. Make a fresh normal profile, start Aster with the wand, and complete the tutorial.
2. Dash into a visible enemy projectile and confirm the immunity window; wait for recharge and repeat.
3. Collect XP and choose each of the three card positions over different ranks. Confirm the run pauses, resumes, and the selected effect is visible.
4. Try every weapon. With the bow, hold then release; with chakram, observe the return; with staff and book, watch the field lifetime; with orbs, walk past a target without firing.
5. Compare fire burn, frost slow, lightning chains, nature root, and arcane detonation.
6. Open a cache, replace the active weapon, skip another cache for shards, and confirm armory discovery after returning to the menu.
7. Traverse each river bridge; after the second world shift, walk across the frozen channel.
8. Observe every guardian's telegraphs and all three phases. Check that a defeated guardian cannot continue damaging the player during the transformation.
9. Enter each gate with E; verify the level advances to 11, 21, and 31. Defeat level 40 and select Realm Collapse from the menu.
10. Change every appearance category. Save, restart the app, and confirm hair/body/outfit/colors/accessory/expression/aura persist.
11. Test keyboard rebinding, controller input, volume sliders, fullscreen, each window size, VSync, and accessibility options.
12. Test defeat, restart, menu return, and quitting during a run. Confirm permanent currency/unlocks persist and run-only relics reset.

## Limits of verification

Build and runtime checks were performed on Linux, including a real OpenGL-rendered visual pass. The Windows x64 package is cross-exported with the official matching .NET templates and a bundled runtime. It still needs a native Windows playtest on the user's machine; the test results do not establish hardware compatibility or finished commercial balance. No Steamworks connection is included. Input rebinding is implemented for keyboard actions; gamepad mappings use the included defaults.
