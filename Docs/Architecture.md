# Scene and system guide

## Entry and runtime hierarchy

All runtime children are constructed in `GameManager._Ready()`; all necessary properties are set in code. Import the project and build it rather than rebuilding this hierarchy manually.

```text
RealmshiftArena (Node2D, GameManager.cs)
  WorldDepth (Node2D, YSortEnabled; common foot-position sort)
    Scenery (Node2D, YSortEnabled)
      WorldProp nodes with split trunk/canopy rendering
      Up to 80 non-colliding FoliageProp nodes
    Player, pooled enemies, and active guardian
  World (Node2D, WorldManager.cs; ground at z=-10)
    GroundDressing (bounded visual records, z=-9)
    EnvironmentCollisions (Node2D)
      StaticBody2D + CollisionShape2D for walls, river, props
  Camera (Camera2D, CameraController.cs)
  Enemies (Node2D, EnemyDirector.cs)
    Owns 140 pooled enemies and guardian state; visual bodies are children of WorldDepth
  Projectiles (Node2D, ProjectilePool.cs; 700 reusable records)
  CombatZones (Node2D, CombatSystem.cs; 64-zone limit)
  Pickups (Node2D, PickupManager.cs; 360 reusable records)
  Effects (Node2D, EffectsManager.cs; 512 reusable records)
  Audio (Node, AudioManager.cs)
    Music player + 12 SFX voices
  Interface (CanvasLayer, UIManager.cs)
    Root (Control, 640 x 360 logical coordinates)
      HUDController (heart/dash, equipment slots, ability glyphs)
        CircularMinimap (terrain cache + live markers)
      RealmShiftOverlay
      Damage overlay
      Announcement labels
      Current menu / modal page
  Player during a run (CharacterBody2D, PlayerController.cs)
    CollisionShape2D (CircleShape2D; radius 6, offset 0,-4)
    Avatar (Node2D, AvatarRenderer.cs)
    WeaponSystem (Node2D, WeaponSystem.cs)
    HeroReadiness (Node2D, timer-driven aura)
```

`Scenes/Characters/Player.tscn` contains the configured player nodes. `Scenes/Main.tscn` attaches GameManager to the root. Enemy collision nodes and all menu controls are created by their scripts. Resource classes attach to their `.tres` files, not scene nodes. PlayerStats, ExperienceSystem, UpgradeSystem, LevelManager, and WorldShiftManager are plain C# state objects owned by GameManager or PlayerController.

## Responsibilities and extension points

| Files | Purpose |
|---|---|
| PlayerController / PlayerStats | Movement, acceleration, dash charges, health/shield/armor, aim, Q abilities |
| AvatarRenderer | 4 directions, 6-frame run cycles, body/hair/outfit layering, accessories, auras, action squash and flash |
| WeaponSystem / WeaponData | Loadouts, charge/release, cooldown, weapon-specific primary and secondary actions |
| ProjectilePool | Bounded reuse, swept segment hit tests, return paths, piercing, friendly/hostile rendering |
| CombatSystem | Critical damage, elemental effects, lightning chains, arcs, fields, hostile telegraphs |
| EnemyBase / EnemyDirector / EnemyData | Reusable enemies, spatial buckets, separation, rewards, elite modifiers, pooling |
| BossBase | Guardian-specific patterns, 3 health phases, phase feedback, final defeat |
| LevelManager | 40 waves, spawn budget, miniboss levels, guardian scheduling, endless escalation |
| WorldManager / WorldShiftManager | 2816 x 2112 maps, obstacles and bridges, minimap data, collision-safe routes, visual transitions |
| ExperienceSystem / UpgradeSystem / UpgradeData | XP ranks, pending choices, filtered relics, run-only ranks and effects |
| PickupManager / EffectsManager / AudioManager | Rewards, feedback, visual/audio caps, realm music and transition cues |
| VfxSprites / WorldProp / RealmShiftOverlay | Original frame rendering, shared scenery depth, contextual canopy fade, realm energy sweep |
| SaveManager / InputBindings | Atomic profile writes, recovery, persisted options, key and controller input |
| UIManager / UIFactory / HUDController | Intro, menus, selection, creator, armory, archive, loadout, pause, settings, upgrade cards, cache and results |

Weapons, enemies, heroes, realms, and upgrades load from actual Godot Resources. The upgrade directory uses `ResourceLoader.ListDirectory`, which preserves logical resource names in exported packages; do not replace this with raw filesystem enumeration of `.tres` files.

To adjust content, edit the `.tres` resource in Godot's Inspector. No Inspector values are missing from the included scenes. Weapon damage, interval, projectile speed, range, ability cooldown, and unlock gate are resource properties. Relics specify their effect key, value, weapon/element requirement, and max rank. Add a relic Resource to `Resources/Upgrades`; Catalog loads it automatically.

To add a new weapon *class*, extend WeaponKind, provide a WeaponData resource and icon, implement its two actions in WeaponSystem, and expand the fixed initial catalog/loadout loops. Existing weapon variations and balance changes only need resource edits.

## Combat and collision

Input actions are installed by InputBindings at startup and reinstalled after rebinding, preserving controller events. The default keyboard controls are in its `Defaults` dictionary.

| Layer | Name | Current handling |
|---|---|---|
| 1 | Player | CharacterBody2D; mask 3 only |
| 2 | Enemies | CharacterBody2D; mask 3 only; separation uses spatial buckets |
| 3 | Environment | StaticBody2D; no query mask |
| 4 | Player Projectiles | Reserved; records use swept analytic tests against enemies and map obstacles |
| 5 | Enemy Projectiles | Reserved; swept analytic tests against player |
| 6 | Pickups | Reserved; distance and attraction tests |
| 7 | Player Melee | Reserved; spatial query + angle/range |
| 8 | Enemy Melee | Reserved; contact distance + player immunity |
| 9 | Interactables | Reserved; E + proximity test |
| 10 | Boss Objects | Reserved; guardian currently shares the enemy damage and analytic query system |

Masks are bit fields: environment layer 3 is integer `4`. Projectiles, pickups and melee intentionally use bounded records and analytic queries rather than a physics node per effect. Pools therefore do not instantiate hundreds of projectile/pickup nodes per second. The player does not physically block on enemies, which allows reliable dodging; contact damage supplies the threat.

Fire applies damage over time; frost slows; lightning jumps to nearby targets and briefly stuns smaller foes; nature roots and heals within nature fields; arcane emphasizes large impact/zone spells and its attunement strengthens direct arcane damage. Relics modify range, count, piercing, fields, regeneration, cooldowns, and other run stats. Elite variants have flaming, stormcharged, frozen, arcane, swift, armored, and splitting modifiers.

## Realms and campaign

| Levels | Realm | Geography / rules | Guardian |
|---|---|---|---|
| 1–10 | Greenward Basin | Forest, river, bridges, shrine courtyards | Rootbound Colossus |
| 11–20 | Emberglass Wastes | Crystal/rock cover, shifted lava channel, eruption warnings | Forge Tyrant |
| 21–30 | Frostveil Citadel | Frozen channel is traversable, stone courts and ice props, frost hazards | Frostbound Sovereign |
| 31–40 | Astral Rupture | Crystal ruins, a different rift channel, astral beam/zone hazards | Realmbreaker |

Every battleground is 176 x 132 tiles at 16 x 16, roughly 26 screen areas. Visible tile drawing is restricted to the camera region. Landmark and chest locations support exploration, while enemy spawns stay away from the player's immediate position. Route steering directs enemies toward a bridge when player and enemy occupy opposite river banks.

The world shift is a timed gameplay state: combat pauses, music and effects change, cracks radiate across the floor, tile colors reveal outward, scenery shifts, props and collision geography rebuild, the player is relocated to a safe nearby point if necessary, and an interactable portal appears. The final shift leads to victory. Endless mode mixes tile regions and continues escalating waves; returning guardians remain scheduled every ten levels.

## Phase coverage

All 15 phases are assembled into the included project: foundation; combat; first weapons; enemies; XP/relic progression; the first realm; first guardian; world shift; traveler selection/creator; remaining realms; remaining guardians; saved meta progression; menus/armory/archive; original pixel/SFX/music feedback; bounded pools, export compatibility, and integration checks. The compact art/animation scope and commercial release work are described in README rather than represented as finished Steam production work.

## Rendering and polish in 1.1

`WorldDepth` and its nested `Scenery` both enable Y sorting. Player, enemies, guardians and props use relative z=0 and a foot-position origin. The ground remains at z=-10; field visuals at z=-2 stay under characters; impacts and warning trajectories at z=18; pooled projectiles at z=22; interface at CanvasLayer 50. Decorative effects cannot cover the HUD, and hostile shots sit above impact decoration.

`WorldProp` draws the original tree texture in trunk/shadow and canopy slices, without relocating its collider or changing map generation. Canopy alpha eases to 82% only when it actually overlaps the hero behind it, then restores; depth sorting is always authoritative. Rocks, shrines and ruin walls share the same depth system.

`VfxSprites` selects authored sheet frames, handles directional rotation, and shares visuals among shots, fields and impacts. Each projectile retains up to eight trail positions without spawning nodes. `EffectsManager` owns 512 reusable records and releases references at expiry. Warning effects can replace decorative particles when the pool is full. VFX quality adjusts trail samples and particle density; High adds small stepped glow sprites. Reduced Flash suppresses glow/pulse flashes, and Screen Shake can be disabled.

The UI uses bounded text slots, wrapped flowing credits, equal expanding relic cards, consistent button style/padding, and a TrueType version of the same original pixel alphabet for stable label layout. Small combat HUD text retains the original bitmap font. Both fonts are released at shutdown. Window sizing updates Godot's Window rather than only its display surface. Audio stops before the engine exits so queued mixer references can drain.

## HUD, dressing and encounter tuning in 1.2

`HUDWidgets` centralizes the pixel frames, glyph halos and aligned text. `HUDController` renders actual equipment and timer state; no gameplay is stored in widgets. Health uses a transient damage-trail value. `WeaponSystem.AbilityDuration` retains the duration of the action that started the shared cooldown, so switching slots does not misrepresent its progress. `InputBindings.Label` handles mouse actions and rebinding safely.

`CircularMinimap` projects local world positions into a 720-unit north-up radar. It refreshes terrain every 0.25 seconds or after movement exceeds 12 units, then reuses its ImageTexture. Live markers update with drawing. Terrain uses `WorldManager.GroundKindAt`; the boss and gate can clamp to the rim. Temporary images are disposed immediately and the radar texture is released at exit.

`GroundDressing` uses a separate deterministic hash and at most 2,000 records. It draws only the camera region. Original map RNG, walls, blocked positions, routes and landmarks are untouched. Its floor layer is z=-9; up to 80 `FoliageProp` nodes use z=0 in shared Scenery. The player-ready aura is z=-1, so it stays beneath actors and above terrain. The 24 foliage sheets contain three wind poses; Low uses a static pose.

`BossTuning` holds four immutable encounter profiles. BossBase uses their health, damage multiplier, damage-taken fraction, movement speed and phase interval. Player/weapon resources and the original warning windows/patterns remain independent. See Polish-Report.md for exact values.

`Art.Weapon` resolves eight code-native SVG designs consistently across the held weapon, equipment HUD, armory and cache cards. Existing original PNGs and animated elemental VFX remain available. Regeneration uses Tools/generate_polish_art.py.
