# Pick Me Up: Infinite Gacha V4 - Project Status

## Implemented Services
- [x] `IGameStateService` / `GameStateService`
- [x] `IEventBus` / `EventBus`
- [x] `ISaveLoadService` / `SaveLoadService` (PlayerPrefs + XOR/Base64 stub)
- [x] `IDataService` / `DataService` (Resources loading)
- [x] `IIdleProgressionService` / `IdleProgressionService` (Stub)
- [x] `IGachaService` / `GachaService` (Basic random pull, pity tracking structure)
- [x] `IHeroRosterService` / `HeroRosterService` (Thread-safe collection management)

## Implemented UI Components
- [x] `HubView` (Displays loaded hero definitions and dynamic text)
- [x] `SummonButton` (Triggers gacha pulls and updates UI)
- [x] `RosterView` (Scrollable list displaying owned heroes)
- [x] `RosterButton` (Toggles the Roster UI panel)

## Core Systems Status
- [x] Service Locator / Dependency Injection (`ServiceRegistry`)
- [x] Bootstrapping & Scene Management (`BootInstaller`)
- [x] Static Data Definitions (ScriptableObjects for Heroes, Skills, Traits)
- [x] Runtime Data Models (Pure C# instances)
- [x] Save Data Structure (JSON serializable, Dictionary-free for Unity compatibility)
- [x] Hero Roster Manager (In-memory collection, event-driven UI updates)

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
6. `IHeroRosterService`
7. `IDataService` (MonoBehaviour added to BootInstaller GameObject)

## Not Yet Implemented (Roadmap)
- [ ] **Combat Engine** (Deterministic simulation, turn resolution, headless execution)
- [ ] **Save Integration** (Hooking `IHeroRosterService` and `GachaService` into `GameSaveData` for persistent storage)
- [ ] **Idle Progression** (Actual offline calculation formulas)
- [ ] **Tower Generation** (Procedural floor/node generation)
- [ ] **Meta Progression** (Master Authority skill tree)
- [ ] **LiveOps & Monetization** (Remote config, IAP hooks, Ad integrations)

## Scene Setup Required
- **Boot.unity**: GameObject "BootLoader" with `BootInstaller` script. Ensure it's scene index 0.
- **Hub.unity**: Canvas → Text + Button + RosterButton + RosterPanel (with ScrollView). GameObject "HubUI" with `HubView`. Summon button has `SummonButton`. Roster button has `RosterButton`.

## How to Continue
- After cloning, run the editor tool to create sample data.
- All services are accessed via `ServiceRegistry.Resolve<T>()`.
- New systems should define an interface in `Services/`, implement it, register in `BootInstaller`.
- Save data uses `GameSaveData`; update migration if schema changes.

---
*Last Updated: Current Session*