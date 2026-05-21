# Pick Me Up: Infinite Gacha (V4)

A mobile idle gacha RPG inspired by the Korean webtoon **"Pick Me Up: Infinite Gacha"**. Summon heroes, train them, form a party, and climb the infinite spire. Built with Unity 6 LTS.

## Current State
- ✅ Core architecture: service locator, pure C# data models, ScriptableObject definitions
- ✅ Boot sequence & service wiring
- ✅ Hub UI with summon button (MVP)
- ✅ Editor tool to generate sample hero/skill/trait data
- 🟡 Stub combat, idle, full gacha rates (coming next)

## Quick Start
1. Open project in Unity 6.
2. Run **Tools > PickMeUp > Create Sample Data** to generate sample assets.
3. Create `Boot.unity` and `Hub.unity` as described in the code comments.
4. Add both scenes to Build Settings (Boot as index 0).
5. Press Play.

## Inspiration
Based on the webtoon [Pick Me Up: Infinite Gacha](https://www.webtoons.com/en/fantasy/pick-me-up/list?title_no=3627). The game adapts the "train underdogs to conquer an impossible tower" concept into a mobile idle/RPG loop.

## License
MIT