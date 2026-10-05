# Validation and playtesting

## Automated and visual validation — 1.2

The project is compiled with Godot.NET.Sdk **4.5.1**, targeting **net8.0**. The final source is checked with the C# compiler and matching Godot .NET engine. The 1.2 integration suite passes **83 checks** on both the source and the Linux release export. Validation covers:

- Catalog sizes: 8 weapons, 28 enemy variants, 8 heroes, 4 realms, 67 relics.
- Construction of all menu pages, save roundtrip, avatar persistence, and compatibility with a version-1 progression/settings fixture.
- Actual movement, dash motion/immunity, and spawned dash visual feedback.
- Actual projectiles defeating enemies and bounded projectile reuse.
- Pause freezing enemy movement; XP freezing the run for three upgrade choices.
- All four scheduled guardians; health-driven phase changes; all four victories.
- Physical world rebuild and portal progression after the first three guardians.
- Level-40 victory, endless unlock, level-41 entry, and return-to-menu cleanup.
- Each of the eight primaries produces its actual projectile or melee effect; all eight secondary actions execute.
- Filling the 512-record VFX pool, confirming its fixed cap, and verifying expiry back to zero active effects.
- Shared Y-sorted scene membership and foot-position origins for the player/scenery.
- Packaged resource discovery, because editor-only filesystem assumptions can fail after export.
- 515 structural layout/capacity checks across 52 rendered visual scenarios: all major menus and dialogs, tree behind/front/side, all five elements, all eight weapons, four boss telegraphs, world shift, quality settings and real 640x360/1280x720/1920x1080 viewports.
- Byte comparison confirms all 115 gameplay `.tres` resources are unchanged; boss tuning is isolated in BossTuning.cs.
- Real hero casting changes readiness, valid and locked hotbar switching behave correctly, all redesigned icons import, and radar projections stay within bounds.
- Dressing is present, bounded and collision/path-safe; each boss has increased health, bounded resistance and phase pressure.
- Rendered red enemy markers, ready/cooldown cues, locked/active slots, all four foliage palettes, shields and health loss are inspected at multiple sizes.

The smoke harness uses its own `smoke-save.json`; it does not replace a normal player profile. Run it from a terminal using the full path to a Godot .NET executable:

```powershell
& "C:\Path\To\Godot_v4.5.1-stable_mono_win64.exe" --headless --path . -- --smoke
```

Successful output ends in `SMOKE PASS`. The harness is intentionally allowed to defeat guardians immediately after checking phase transitions; it proves progression wiring, not human combat balance.

## Manual playtest

1. Start with your existing profile and confirm its unlocks/settings. To test the tutorial separately, back up your profile before using a fresh profile; start Aster with the wand.
2. Dash into a visible enemy projectile and confirm the immunity window; wait for recharge and repeat.
3. Collect XP and choose each of the three card positions over different ranks. Confirm the run pauses, resumes, and the selected effect is visible.
4. Try every weapon. With the bow, hold then release; with chakram, observe the return; with staff and book, watch the field lifetime; with orbs, walk past a target without firing.
5. Compare fire burn, frost slow, lightning chains, nature root, and arcane detonation.
6. Open a cache, replace the active weapon, skip another cache for shards, and confirm armory discovery after returning to the menu.
7. Traverse each river bridge; after the second world shift, walk across the frozen channel.
8. Observe every guardian's telegraphs and all three phases. Check that a defeated guardian cannot continue damaging the player during the transformation.
9. Enter each gate with E; verify the level advances to 11, 21, and 31. Defeat level 40 and select Realm Collapse from the menu.
10. Change every appearance category. Save, restart the app, and confirm hair/body/outfit/colors/accessory/expression/aura persist.
11. Open Credits/Characters/Loadout and each relic/cache card at all three window sizes. Confirm wrapping/padding and equal card edges. Walk above/below/beside tree canopies and watch the conditional fade return to opaque.
12. Test keyboard rebinding, controller input, volume sliders, fullscreen, each window size, VSync, and accessibility options.
13. Test defeat, restart, menu return, and quitting during a run. Confirm permanent currency/unlocks persist and run-only relics reset.

The visual capture harness uses `-- --polish-capture=<absolute-directory>`; it also uses the isolated QA profile. It creates 52 PNG screenshots and 300 captured frames for a ten-second 30-fps showcase. The preview uses invulnerability and durable targets in the isolated QA profile, so it can demonstrate visuals without entering upgrade/result dialogs. Native audio is disabled during image capture.

## Limits of verification

Build and runtime checks were performed on Linux, including a real OpenGL-rendered visual pass. The Windows x64 package is cross-exported with the official matching .NET templates and a bundled runtime. It still needs a native Windows playtest on the user's machine; the test results do not establish hardware compatibility or finished commercial balance. No Steamworks connection is included. Input rebinding is implemented for keyboard actions; gamepad mappings use the included defaults.

The software-rendered Xvfb/Mesa capture environment cannot enable VSync and emits a one-time driver warning. It produces no repeating game errors. The final build, packaged headless integration run, export and shutdown complete without game errors. This is not a hardware performance benchmark; particle caps/expiry were stress-checked, and native Windows FPS/driver/controller behavior remains a manual check.

## New HUD / environment checks

14. Observe the heart bar while damaged/healing; shield is a cyan strip, and dash pips beside it drain/refill without a DASH label. Try a charge upgrade.
15. Switch 1/2 or LB/RB. Confirm the gold equipment frame moves and the icon/element/key remain clear. On a fresh profile, slot 2 remains locked until the first guardian.
16. Cast Q and RMB: the corresponding glyph dims and its cooldown arc fills. Hero aura disappears during cooldown and returns when ready. Try Reduced Flash.
17. Check red radar dots against nearby enemies, the bigger guardian marker, gold caches, cyan shrines, north orientation and violet gate pointer. Traverse a bridge while watching the map.
18. Inspect all four foliage themes while moving around props. Decorative plants must not obstruct movement/projectiles; standing close to tall plants reduces occlusion.
19. Play every guardian normally. Record fight duration with your relic build, damage taken and dodge comfort; initial statistical tuning is not a substitute for human balance testing.
