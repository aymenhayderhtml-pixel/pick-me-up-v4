# Pick Me Up: Infinite Gacha

A mobile idle gacha RPG based on the Korean webtoon "Pick Me Up: Infinite Gacha" (나를 pick 해줘).

## Description

This project is a Unity 6 LTS implementation of an idle gacha RPG game inspired by the popular Korean webtoon. Players can summon heroes, build their roster, progress through tower floors, and engage in turn-based combat.

## Tech Stack

- **Engine**: Unity 6 LTS
- **Language**: C# (C# 9 compatible)
- **Architecture**: Service-oriented architecture with dependency injection via ServiceRegistry

## Current Features

- ✅ Service architecture with thread-safe ServiceRegistry
- ✅ Boot sequence with scene loading
- ✅ Hub UI with summon button functionality
- ✅ Sample data generator for testing
- ✅ Data models for Heroes, Skills, and Traits
- ✅ Gacha service with pity tracking
- ✅ Save/Load service with encryption
- ✅ Event bus for decoupled communication
- ✅ Game state management

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # BootInstaller, ServiceRegistry
│   ├── Data/           # ScriptableObjects, Enums, Data models
│   ├── Services/       # Interfaces and Implementations
│   ├── UI/             # HubView, SummonButton
│   └── Editor/         # CreateSampleData tool
```

## Setup Instructions

### Prerequisites

1. Unity 6 LTS installed
2. Clone this repository

### Scene Setup

1. **Create Boot.unity scene:**
   - Create empty GameObject named `BootLoader`
   - Add `BootInstaller` component
   - Add this scene to Build Settings at index 0

2. **Create Hub.unity scene:**
   - Create Canvas with UI Text element
   - Add Button for summoning
   - Add `HubView` component to a GameObject
   - Link the Text element to HubView's Display Text field
   - Add `SummonButton` component to the Button GameObject
   - Add this scene to Build Settings at index 1

### Generate Sample Data

1. Open Unity Editor
2. Go to `Tools > PickMeUp > Create Sample Data`
3. This creates sample Hero, Skill, and Trait definitions in Resources folders

### Running the Project

1. Press Play in the Unity Editor
2. The Boot scene will initialize all services
3. The Hub scene will load automatically
4. Click the Summon button to pull a random hero

## Inspiration

This project is based on the Korean webtoon "Pick Me Up: Infinite Gacha" (나를 pick 해줘). 

Read the webtoon: [Webtoon Official Site](https://www.webtoons.com/en/fantasy/pick-me-up/list?title_no=3627)

## License

MIT License - See LICENSE file for details.