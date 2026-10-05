# Realmshift Arena 1.2 — Wilds & Wardens

1. **What changed.** Continued your uploaded 1.1 project with a new HUD, original weapon art, richer realm foliage and stronger guardians. Existing saves, campaign progression, characters, elements, weapon mechanics, tree depth and animated combat VFX remain in use.

2. **HUD redesign.** Replaced the large bottom status rectangle and top-right score box. Equipment and skill glyphs occupy the bottom center; health/dash sit top-left; a circular radar sits top-right. Campaign level, realm and boss health remain readable. XP uses a thin rail below equipment.

3. **Hotbar.** Two framed slots show the actual equipped weapon icons, 1/2 keys, elemental badges, rarity pips, locked state and an active gold rim/pointer. Keyboard and controller switching retain their existing behavior.

4. **Ultimate/passive readiness.** Q and RMB glyphs show a restrained ready halo or a filling cooldown arc, using the real timers. The hero also gains a segmented ground aura and orbiting sparks that disappear after casting and return on recharge. Reduced Flash suppresses pulsing. Spellbook/orb passives retain their automatic behavior.

5. **Health and dash.** A heart badge and red health bar replace the old name/numeric-HP/rank panel. A delayed golden strip shows recent damage, and cyan shows shields. The adjacent dash icon has charge/recharge pips without the DASH word.

6. **Minimap.** The north-up circular radar follows the player within a 720-unit range and samples real path/river geometry. Red dots mark live foes, a larger red diamond marks guardians, gold marks caches, cyan marks shrines and violet points toward the gate. A red proximity ring keeps nearby foes visible beneath the player marker. Compact shard/foe counters sit below the map; score remains available in run results.

7. **Weapons.** Eight original grid-aligned SVG designs improve silhouettes, materials and contrast in equipment, armory, cache choices and held weapons. The floating spellbook and bow's nocked arrow are improved. All eight attack systems and the existing animated elemental effects remain.

8. **Environment.** Twenty-four original three-pose foliage sheets add grass, reeds/leaves, flowers, moss bushes and mushrooms in four realm palettes. Deterministic clusters, wind and irregular path edges enrich the floor. Tall plants share actor Y sorting and fade near the hero. Decoration is bounded, camera-culled and placed on walkable ground without adding collisions. Low VFX uses a still wind pose.

9. **Boss balance.** Increased health, modest resistance, damage and phase pressure. Existing warning durations, shapes, phase thresholds and patterns remain. Phase 2/3 add 5%/10% attack damage; returning guardians gain 140 HP per level beyond 40. Human playtesting is still needed to assess difficulty across relic builds.

| Guardian | Previous HP | New HP | Base damage increase | Resistance |
|---|---:|---:|---:|---:|
| Rootbound Colossus | 1,100 | 1,850 | 12% | 6% |
| Forge Tyrant | 1,850 | 3,250 | 15% | 8% |
| Frostbound Sovereign | 2,600 | 5,100 | 18% | 10% |
| Realmbreaker | 3,350 | 7,400 | 20% | 12% |

Warning windows remain 0.85–1.30 seconds, and phase-3 attack intervals remain above 1.42 seconds.

10. **Important files.** New: `Scripts/UI/HUDWidgets.cs`, `Scripts/UI/CircularMinimap.cs`, `Scripts/Player/HeroReadiness.cs`, `Scripts/World/GroundDressing.cs`, `Scripts/World/FoliageProp.cs`, `Scripts/Bosses/BossTuning.cs`. Updated: HUDController, UIManager, InputBindings, GameTypes/Art, PlayerController, WeaponSystem, WorldManager, BossBase, SmokeTest, PolishCapture, export presets and documentation. New art is under `Assets/Art/Weapons/Polished/`, `Assets/Art/UI/HUD/` and `Assets/Art/Foliage/`; reproduce it with `Tools/generate_polish_art.py` using standard Python. All new assets are original.

11. **Verification and your inspection.** Clean compilation: zero warnings/errors. Both source and packaged Linux release pass 83 integration checks, including legacy saves, attacks, readiness, slot locking, all four boss/portal transitions and bounded foliage. All 115 gameplay `.tres` resources remain byte-identical to your upload; boss tuning lives in C#. The rendered release passes 515 checks across 52 scenes at actual 640×360, 1280×720 and 1920×1080 sizes. The ten-second preview uses an isolated QA profile with invulnerability and durable targets. Full evidence is in `Docs/Integration-Results.txt`.

On your Windows PC, test health/shield loss, dash recharge, switching 1/2, Q/RMB cooldowns, red enemy/boss radar markers and gate directions. Inspect all four foliage themes, Low/High VFX, Reduced Flash, fullscreen, rebinding and controller input. Fight every guardian with a normal relic build and record dodge comfort and fight duration. The Windows export is complete; native Windows hardware/FPS/controller behavior and human boss balance have not been tested here.
