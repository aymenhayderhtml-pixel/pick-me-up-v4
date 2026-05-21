# Project Status - Technical Handoff

## What Has Been Implemented

### Core Scripts
- **ServiceRegistry.cs** - Thread-safe static generic service locator with Register, Resolve, HasService, Unregister, Clear methods
- **BootInstaller.cs** - MonoBehaviour entry point that initializes all services and loads scenes
- **Enums.cs** - GameState, ElementType, ClassType, SkillType enumerations

### Service Interfaces
- **IGameStateService.cs** - Game state management interface
- **ISaveLoadService.cs** - Save/load persistence interface
- **IDataService.cs** - Data definition loading interface
- **IGachaService.cs** - Gacha pulling interface with pity tracking
- **ICombatSimulationService.cs** - Combat simulation interface (stub)
- **IIdleProgressionService.cs** - Idle progression interface (stub)
- **IEventBus.cs** - Event publishing/subscribing interface

### Service Implementations
- **GameStateService.cs** - Manages game state transitions
- **EventBus.cs** - Decoupled event system with type-safe handlers
- **DataService.cs** - Loads ScriptableObject definitions from Resources
- **SaveLoadService.cs** - PlayerPrefs-based save with XOR encryption
- **IdleProgressionService.cs** - Offline gains calculation (stub)
- **GachaService.cs** - Random hero pulling with pity tracking

### Data Models
- **HeroDefinition.cs** - ScriptableObject for hero base stats and abilities
- **SkillDefinition.cs** - ScriptableObject for skill effects
- **TraitDefinition.cs** - ScriptableObject for passive traits
- **HeroInstance.cs** - Runtime hero instance with level, XP, morale
- **SkillState.cs** - Runtime skill cooldown and energy state
- **TraitState.cs** - Runtime trait activation state
- **GameSaveData.cs** - Top-level save container with serialization support
- **ServiceModels.cs** - Placeholder classes for combat and idle rewards

### UI Components
- **HubView.cs** - Displays hero information in the hub scene
- **SummonButton.cs** - Handles gacha pull button clicks

### Editor Tools
- **CreateSampleData.cs** - Menu tool to generate sample Hero/Skill/Trait assets

---

## Architecture Overview

### ServiceRegistry Pattern
The project uses a static ServiceRegistry as a service locator pattern:
- Thread-safe with locking mechanism
- Generic registration and resolution
- Debug logging for service operations
- Supports MonoBehaviour and pure C# services

### Boot Sequence
1. Unity loads `Boot.unity` scene (index 0)
2. `BootInstaller.Awake()` executes:
   - Marks GameObject as DontDestroyOnLoad
   - Registers GameStateService
   - Registers EventBus
   - Registers SaveLoadService
   - Registers IdleProgressionService
   - Registers GachaService
   - Creates and registers DataService (MonoBehaviour)
   - Calls `dataService.LoadAllDefinitions()`
3. `BootInstaller.Start()` executes:
   - Changes game state to Hub
   - Loads `Hub.unity` scene via SceneManager

### Data Flow
```
BootInstaller → ServiceRegistry → All Services
                      ↓
              DataService loads ScriptableObjects
                      ↓
              GachaService pulls heroes
                      ↓
              HubView displays results
                      ↓
              SaveLoadService persists data
```

---

## Service Registration Order

In `BootInstaller.Awake()`:
1. GameStateService
2. EventBus
3. SaveLoadService
4. IdleProgressionService
5. GachaService
6. DataService (as component)

---

## Current Service Implementations Summary

| Service | Status | Description |
|---------|--------|-------------|
| GameStateService | ✅ Complete | Manages Boot/Hub/Game states |
| EventBus | ✅ Complete | Type-safe pub/sub system |
| SaveLoadService | ✅ Complete | JSON + XOR encryption |
| DataService | ✅ Complete | Loads SO definitions |
| GachaService | ✅ Complete | Random pull with pity |
| IdleProgressionService | 🟡 Stub | Returns zero rewards |
| CombatSimulationService | ❌ Not implemented | Interface only |

---

## Data Models Summary

### ScriptableObjects (Editor Assets)
- **HeroDefinition**: heroId, heroName, portrait, element, classType, base stats, skills, traits
- **SkillDefinition**: skillId, skillName, description, type, cooldown, energy, effects
- **TraitDefinition**: traitId, traitName, description, effects

### Runtime Classes
- **HeroInstance**: InstanceId, Level, XP, Ascension, HP, Morale, calculated stats
- **SkillState**: Cooldown, Energy, IsUnlocked
- **TraitState**: IsActive, Stacks

### Save Data
- **GameSaveData**: SchemaVersion, Timestamp, HeroRoster, FloorProgress, currencies, pity data

---

## UI Components

### HubView
- Displays hero count and first hero's stats
- Methods: SetHeroText(), RefreshHeroDisplay()
- Requires Text component reference

### SummonButton
- Listens to Button.onClick
- Calls IGachaService.Pull()
- Displays result via HubView

---

## Editor Tool

### CreateSampleData
Menu: `Tools > PickMeUp > Create Sample Data`

Creates:
- `Assets/Resources/Heroes/Champion.asset`
- `Assets/Resources/Skills/Slash.asset`
- `Assets/Resources/Traits/Brave.asset`

---

## What Is NOT Implemented Yet

### Missing Features
1. **Full Gacha Rates** - Currently uniform random, needs rarity weights
2. **Hero Roster Manager** - No persistent roster management
3. **Combat Engine** - Only interface exists, no implementation
4. **Tower Generation** - No floor/stage system
5. **Idle Formula** - Returns zero rewards
6. **Full UI** - Only basic hub display

### Incomplete Systems
- Master Authority system (placeholder in save data)
- Banner guarantee system (structure exists, not wired)
- Combat simulation (interface only)
- Offline progression calculation (stub)

---

## Next Steps

### Priority 1: Hero Roster Manager
- Add/remove heroes from roster
- Hero detail view
- Level up and ascension UI
- Sort/filter functionality

### Priority 2: Combat Engine
- Turn-based combat simulation
- Skill execution logic
- Damage calculation
- Combat UI (HP bars, action log)

### Priority 3: Tower Generator
- Floor generation algorithm
- Enemy placement
- Reward distribution
- Progress tracking

---

## Scene Setup Required

### Boot.unity
1. Create new scene, save as `Assets/Scenes/Boot.unity`
2. Create empty GameObject named `BootLoader`
3. Add `BootInstaller` component
4. Add scene to Build Settings at index 0

### Hub.unity
1. Create new scene, save as `Assets/Scenes/Hub.unity`
2. Create Canvas (UI > Canvas)
3. Add Text element (UI > Text - Legacy or TextMeshPro)
4. Create empty GameObject named `HubUI`
5. Add `HubView` component, link Text field
6. Create Button (UI > Button)
7. Add `SummonButton` component to Button
8. Optionally link HubView reference in SummonButton
9. Add scene to Build Settings at index 1

---

## How to Continue Development

1. **Set up scenes** as described above
2. **Run sample data tool**: Tools > PickMeUp > Create Sample Data
3. **Press Play** to test current functionality
4. **Implement next priority** from the list above
5. **Add tests** for new systems
6. **Update this document** when adding major features

### Key Files to Extend
- `GachaService.cs` - Add rarity rates, banners
- `HeroInstance.cs` - Add more progression logic
- New: `HeroRosterManager.cs` - Manage collection
- New: `CombatSimulator.cs` - Implement ICombatSimulationService
- New: `TowerGenerator.cs` - Generate floors

### Testing Tips
- Use `Debug.Log()` statements liberally
- Check Console for service registration errors
- Verify Resources folders contain assets after running tool
- Test save/load by modifying data, restarting play mode