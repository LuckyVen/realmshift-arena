# Scene and system guide

## Entry and runtime hierarchy

All runtime children are constructed in `GameManager._Ready()`; all necessary properties are set in code. Import the project and build it rather than rebuilding this hierarchy manually.

```text
RealmshiftArena (Node2D, GameManager.cs)
  World (Node2D, WorldManager.cs)
    Scenery (Node2D, YSortEnabled)
      Sprite2D props and landmarks
    EnvironmentCollisions (Node2D)
      StaticBody2D + CollisionShape2D for walls, river, props
  Camera (Camera2D, CameraController.cs)
  Enemies (Node2D, EnemyDirector.cs)
    140 pooled CharacterBody2D enemies (EnemyBase.cs)
    BossBase when a guardian is scheduled
  Projectiles (Node2D, ProjectilePool.cs; 700 reusable records)
  CombatZones (Node2D, CombatSystem.cs; 64-zone limit)
  Pickups (Node2D, PickupManager.cs; 360 reusable records)
  Effects (Node2D, EffectsManager.cs; 450-effect limit)
  Audio (Node, AudioManager.cs)
    Music player + 12 SFX voices
  Interface (CanvasLayer, UIManager.cs)
    Root (Control, 640 x 360 logical coordinates)
      HUDController
      Damage overlay
      Announcement labels
      Current menu / modal page
  Player during a run (CharacterBody2D, PlayerController.cs)
    CollisionShape2D (CircleShape2D; radius 6, offset 0,-4)
    Avatar (Node2D, AvatarRenderer.cs)
    WeaponSystem (Node2D, WeaponSystem.cs)
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
