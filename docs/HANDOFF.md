# Pick Me Up: Infinite Gacha V4 — Development Handoff

## Project Overview
Mobile idle gacha RPG based on the Korean webtoon "Pick Me Up: Infinite Gacha". Built with Unity 6 LTS, C# 9. The player summons heroes, builds a roster, climbs a procedural tower, and engages in deterministic auto-battles.

## Repository
https://github.com/aymenhayderhtml-pixel/pick-me-up-v4

## Current State (All Implemented)

### Architecture
- **ServiceRegistry** (`Assets/Scripts/Core/ServiceRegistry.cs`): Thread-safe static generic service locator. Services registered in `BootInstaller.Awake()`.
- **BootInstaller** (`Assets/Scripts/Core/BootInstaller.cs`): Entry point. Registers all services, loads definitions, transitions to Hub scene. Scene flow: Boot.unity (index 0) → Hub.unity (index 1).

### Services (Registered in Order)
1. `IGameStateService` / `GameStateService` — State machine (Boot, Hub, Combat, Saving)
2. `IEventBus` / `EventBus` — Pub/sub with error isolation
3. `ISaveLoadService` / `SaveLoadService` — JSON + XOR encryption, PlayerPrefs storage (functional but not yet wired to game state)
4. `IIdleProgressionService` / `IdleProgressionService` — Stub (returns zero)
5. `IGachaService` / `GachaService` — Random pull from hero definitions, list-based pity tracking, auto-adds to roster
6. `IHeroRosterService` / `HeroRosterService` — Thread-safe hero collection, events for add/remove
7. `ICombatEngineService` / `CombatEngineService` — Deterministic turn-based combat, headless, elemental advantages, crit, skills/energy
8. `ITowerService` / `TowerService` — Procedural floor generation, 5 node types, enemy scaling, combat integration

### Data Models
- **ScriptableObjects**: `HeroDefinition`, `SkillDefinition`, `TraitDefinition`, `TowerEnemyDatabase`
- **Runtime (Pure C#)**: `HeroInstance`, `SkillState`, `TraitState`
- **Combat**: `CombatUnit`, `CombatSkillState`, `CombatTraitState`, `CombatEvent`, `CombatSnapshot` (in `CombatModels.cs`)
- **Tower**: `TowerNode`, `TowerFloorData`, `TowerRunState`, `TowerEnemyTemplate` (in `TowerModels.cs`)
- **Save**: `GameSaveData` with `MasterAuthorityData`, `GachaPityData` (list-based, no dictionaries for JSON compatibility)
- **Enums**: `GameState`, `ElementType` (Fire/Water/Wood/Light/Dark), `ClassType` (Vanguard/Striker/Caster/Tactician/Ranger), `SkillType` (Active/Passive), `CombatEventType`, `TowerNodeType`
- **Formulas**: `CombatFormulas.cs` — static class, `CalculateDamage()`, `CalculateTurnOrder()`, `GetElementalMultiplier()`. Uses `System.Random` for determinism.

### UI Components (Hub Scene)
- `HubView` — Displays hero definitions and dynamic text
- `SummonButton` — Triggers gacha pull, updates display, refreshes roster
- `RosterView` — Scrollable list of owned heroes (requires ScrollView + prefab setup)
- `RosterButton` — Toggles roster panel
- `TowerButton` — Starts tower run with first 4 roster heroes, auto-resolves first node

### Editor Tool
- `CreateSampleData.cs` — Menu: Tools > PickMeUp > Create Sample Data. Generates sample Hero/Skill/Trait assets in Resources folders.

### Combat System Details
- Turn order by SPD descending, ties broken by seeded Random
- Front row (Position 0) targeted first
- Skills use energy/cooldown system; basic attacks generate 20 energy
- Elemental table: Fire > Wood > Water > Fire (1.3x), Light <-> Dark (1.5x), same element (0.8x)
- Crit check using basis points (10000 = 100%)
- Damage formula: `ATK * multiplier - DEF * 0.5`, minimum 1
- Max 100 turns, timeout = defeat

### Tower System Details
- Floor 1: 2 Combat + 1 Rest
- Boss floors (every 5th): 2 Combat + 1 Rest + 1 Boss
- Regular floors: 3 Combat + 1 Rest + 1 Treasure
- Boss enemies: 3x HP, 2x ATK/DEF
- Elite enemies: 1.3x all stats
- Enemy scaling: `template.StatMultiplier + (floorLevel * 0.1)`
- Rest nodes: restore 30% HP, +20 morale
- Gold reward: `floorLevel * 10 + random(0, floorLevel * 5)`
- XP reward: `floorLevel * 5 + random(0, floorLevel * 3)`

## Scene Setup Required (Not in Repo — Must Create Manually)
- **Boot.unity**: Empty GameObject "BootLoader" with `BootInstaller` script. Scene index 0.
- **Hub.unity**: Canvas with Text ("displayText"), Summon Button, Roster Button, Tower Button, RosterPanel (with ScrollView and Content for RosterView). Scene index 1.

## Critical: Unity-Specific Constraints
- `JsonUtility` cannot serialize `Dictionary<TKey, TValue>` — all save data uses `List<T>` of structs
- All runtime models are `[Serializable]` pure C# classes — no `MonoBehaviour` dependency
- Combat uses `System.Random`, never `UnityEngine.Random`
- `CombatEngineService` and `TowerService` must import `System.Linq`
- `GachaService` must import `PickMeUp.Core` (for `ServiceRegistry`)

## Roadmap — Next Priorities (In Order)

### Priority 4: Save Integration
Wire the existing `SaveLoadService` into the actual game state so progress persists. Currently the save system exists but nothing is saved/loaded.

**What to do:**
- In `GameSaveData`, verify all needed fields exist: `HeroRoster`, `Gems`, `Gold`, `Tickets`, `FloorProgress`, `GachaPityData`, `TowerRunState` (add if missing)
- Create a `SaveManager` or add methods to `BootInstaller` that:
  - On application pause/quit: gather state from `IHeroRosterService`, `IGachaService`, `ITowerService` into a `GameSaveData` object, call `ISaveLoadService.Save()`
  - On startup after services registered: check `ISaveLoadService.HasSave()`, load data, populate roster service and gacha pity from saved data
- Add a "Save" button to Hub UI for manual save during MVP
- Update `BootInstaller.OnApplicationPause` and `OnApplicationQuit` to call save

### Priority 5: Idle Progression
Replace the `IdleProgressionService` stub with actual offline calculation.

**What to do:**
- `CalculateOfflineGains`: use the combat engine to simulate battles at the player's last floor, calculate gold/XP earned over time away
- Cap at `GetMaxOfflineDuration()` (currently 12 hours)
- On game start, check time since last save, calculate rewards, display summary screen
- Hook into `IHeroRosterService` to apply XP to heroes

### Priority 6: Meta Progression
Master Authority skill tree for permanent upgrades.

### Priority 7: LiveOps & Monetization
Remote config, IAP hooks, ad integrations.

## How to Continue Development
1. Clone the repo, open in Unity 6
2. Create Boot.unity and Hub.unity scenes as described above
3. Run Tools > PickMeUp > Create Sample Data
4. All services are accessed via `ServiceRegistry.Resolve<T>()`
5. New systems: define interface in `Services/`, implement in `Services/Implementations/`, register in `BootInstaller.Awake()`
6. Save data goes in `GameSaveData`; update `SchemaVersion` and add migration if structure changes
7. Update `PROJECT_STATUS.md` when completing each priority

## Conversation Context Summary
The developer (aymenhayderhtml-pixel) is building this with AI assistance. The AI advisor reviewed all code for correctness, ensuring Unity 6 compatibility, deterministic systems, and proper architecture. The project has been built incrementally: Foundation → Roster → Combat → Tower. Each phase was reviewed for compilation errors (missing usings, LINQ imports, dictionary serialization issues) before being committed.

The developer does not have Unity access currently and is working purely through code generation and GitHub. All testing must be done by the next developer who opens the project in Unity.

## Key Files Reference
| File | Purpose |
|------|---------|
| `Assets/Scripts/Core/ServiceRegistry.cs` | Service locator |
| `Assets/Scripts/Core/BootInstaller.cs` | Entry point, service wiring |
| `Assets/Scripts/Data/Enums.cs` | All enums |
| `Assets/Scripts/Data/HeroDefinition.cs` | Hero ScriptableObject |
| `Assets/Scripts/Data/HeroInstance.cs` | Runtime hero |
| `Assets/Scripts/Data/GameSaveData.cs` | Save container |
| `Assets/Scripts/Data/CombatModels.cs` | Combat types |
| `Assets/Scripts/Data/TowerModels.cs` | Tower types |
| `Assets/Scripts/Combat/CombatFormulas.cs` | Combat math |
| `Assets/Scripts/Services/Implementations/GachaService.cs` | Summoning |
| `Assets/Scripts/Services/Implementations/CombatEngineService.cs` | Battle simulation |
| `Assets/Scripts/Services/Implementations/TowerService.cs` | Floor generation |
| `docs/PROJECT_STATUS.md` | Current feature checklist |