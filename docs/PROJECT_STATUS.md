# Project Status: Pick Me Up V4

## Implemented
- **ServiceRegistry**: Thread-safe static locator.
- **BootInstaller**: Registers all services, loads definitions, transitions to Hub scene.
- **Services**:
  - `GameStateService` – state machine (Boot, Hub, Combat, Saving)
  - `EventBus` – pub/sub with error isolation
  - `DataService` – MonoBehaviour that loads `HeroDefinition`, `SkillDefinition`, `TraitDefinition` from Resources
  - `SaveLoadService` – JSON save with XOR encryption, PlayerPrefs storage
  - `IdleProgressionService` – stub (returns zero)
  - `GachaService` – simple random pull, list-based pity tracking stubs
- **Data Models**:
  - ScriptableObjects: `HeroDefinition`, `SkillDefinition`, `TraitDefinition`
  - Runtime: `HeroInstance`, `SkillState`, `TraitState`
  - Save: `GameSaveData` with serializable pity lists (no dictionaries)
- **UI**:
  - `HubView` – displays hero count and first hero info; public `SetHeroText`
  - `SummonButton` – triggers gacha pull and updates display
- **Editor**:
  - `CreateSampleData` – generates sample assets in Resources

## Architecture
- Service Locator pattern, all services registered in `BootInstaller.Awake()`
- Static data via `Resources.LoadAll`; future migration to Addressables planned
- Pure C# data classes for runtime and save; no `MonoBehaviour` dependency in models
- Scene flow: Boot.unity (index 0) → loads Hub.unity

## Service Registration Order
1. `IGameStateService`
2. `IEventBus`
3. `ISaveLoadService`
4. `IIdleProgressionService`
5. `IGachaService`
6. `IDataService` (MonoBehaviour added to BootInstaller GameObject)

## Not Yet Implemented
- Real gacha rates / pity system / weighted pulls
- Hero roster management (view all owned heroes)
- Combat simulation engine (deterministic)
- Tower floor generation
- Idle progression formula
- Complete UI (roster, combat visuals, tower map)
- Ascension / synthesis logic

## Next Steps (as planned by advisor)
1. **Hero Roster Manager** – save pulled heroes, display in scrollable list
2. **Combat Engine** – deterministic headless simulator with formulas
3. **Tower Generator** – procedural floor nodes

## Scene Setup Required
- **Boot.unity**: GameObject "BootLoader" with `BootInstaller` script. Ensure it's scene index 0.
- **Hub.unity**: Canvas → Text ("displayText") + Button. GameObject "HubUI" with `HubView`. Button has `SummonButton`. Add to Build Settings.

## How to Continue
- After cloning, run the editor tool to create sample data.
- All services are accessed via `ServiceRegistry.Resolve<T>()`.
- New systems should define an interface in `Services/`, implement it, register in `BootInstaller`.
- Save data uses `GameSaveData`; update migration if schema changes.