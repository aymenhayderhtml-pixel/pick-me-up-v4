using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    public class TowerService : ITowerService
    {
        private TowerRunState _activeRun;
        private TowerEnemyDatabase _enemyDatabase;

        public TowerService()
        {
            _enemyDatabase = Resources.Load<TowerEnemyDatabase>("TowerEnemyDatabase");
            if (_enemyDatabase == null)
            {
                Debug.LogWarning("[TowerService] TowerEnemyDatabase not found in Resources. Enemy generation will use fallback.");
            }
        }

        public TowerRunState StartNewRun(List<HeroInstance> party, int startingFloor)
        {
            int seed = Environment.TickCount; 
            
            _activeRun = new TowerRunState
            {
                CurrentFloor = startingFloor,
                ActiveParty = new List<HeroInstance>(party),
                RunSeed = seed,
                IsRunActive = true
            };

            _activeRun.CurrentFloorData = GenerateFloor(startingFloor, seed);
            Debug.Log($"[TowerService] Started new run at floor {startingFloor} with {party.Count} heroes.");
            return _activeRun;
        }

        public TowerFloorData GenerateFloor(int floorLevel, int seed)
        {
            System.Random rng = new System.Random(seed + floorLevel);
            TowerFloorData floor = new TowerFloorData
            {
                FloorLevel = floorLevel,
                IsBossFloor = (floorLevel % 5 == 0),
                Seed = seed + floorLevel,
                Nodes = new List<TowerNode>()
            };

            List<TowerNodeType> nodeTypes = new List<TowerNodeType>();
            
            if (floorLevel == 1)
            {
                nodeTypes.AddRange(new[] { TowerNodeType.Combat, TowerNodeType.Combat, TowerNodeType.Rest });
            }
            else if (floor.IsBossFloor)
            {
                nodeTypes.AddRange(new[] { TowerNodeType.Combat, TowerNodeType.Combat, TowerNodeType.Rest, TowerNodeType.Boss });
            }
            else
            {
                nodeTypes.AddRange(new[] { TowerNodeType.Combat, TowerNodeType.Combat, TowerNodeType.Combat, TowerNodeType.Rest, TowerNodeType.Treasure });
            }

            foreach (var type in nodeTypes)
            {
                int nodeSeed = rng.Next();
                floor.Nodes.Add(GenerateNode(floorLevel, type, nodeSeed));
            }

            return floor;
        }

        public TowerNode GenerateNode(int floorLevel, TowerNodeType type, int seed)
        {
            System.Random rng = new System.Random(seed);
            TowerNode node = new TowerNode
            {
                NodeId = seed,
                Type = type,
                FloorLevel = floorLevel,
                Description = $"{type} Node on Floor {floorLevel}",
                GoldReward = floorLevel * 10 + rng.Next(0, Math.Max(1, floorLevel * 5)),
                XpReward = floorLevel * 5 + rng.Next(0, Math.Max(1, floorLevel * 3)),
                Enemies = new List<HeroInstance>()
            };

            if (type == TowerNodeType.Combat || type == TowerNodeType.Elite || type == TowerNodeType.Boss)
            {
                int enemyCount = rng.Next(1, 5); 
                node.Enemies = GenerateEnemies(floorLevel, enemyCount, rng.Next());

                if (type == TowerNodeType.Elite)
                {
                    foreach (var e in node.Enemies)
                    {
                        e.MaxHP = (int)(e.MaxHP * 1.3f); e.CurrentHP = e.MaxHP;
                        e.ATK = (int)(e.ATK * 1.3f); 
                        e.DEF = (int)(e.DEF * 1.3f);
                    }
                }
                else if (type == TowerNodeType.Boss)
                {
                    foreach (var e in node.Enemies)
                    {
                        e.MaxHP = (int)(e.MaxHP * 3.0f); e.CurrentHP = e.MaxHP;
                        e.ATK = (int)(e.ATK * 2.0f); 
                        e.DEF = (int)(e.DEF * 2.0f);
                    }
                }
            }

            return node;
        }

        public CombatSnapshot ResolveNode(TowerRunState runState, TowerNode node)
        {
            ICombatEngineService combatEngine = ServiceRegistry.Resolve<ICombatEngineService>();
            int seed = runState.RunSeed + node.NodeId;

            if (node.Type == TowerNodeType.Combat || node.Type == TowerNodeType.Elite || node.Type == TowerNodeType.Boss)
            {
                return combatEngine.SimulateCombat(runState.ActiveParty, node.Enemies, node.FloorLevel, seed);
            }
            else if (node.Type == TowerNodeType.Rest)
            {
                foreach (var hero in runState.ActiveParty)
                {
                    hero.CurrentHP = Math.Min(hero.MaxHP, hero.CurrentHP + (int)(hero.MaxHP * 0.3f));
                    hero.Morale = Math.Min(10000, hero.Morale + 2000); 
                }
                return new CombatSnapshot 
                { 
                    IsComplete = true, 
                    IsVictory = true, 
                    EventLog = new List<CombatEvent> { new CombatEvent { Description = "Party rested and recovered." } } 
                };
            }
            else if (node.Type == TowerNodeType.Treasure)
            {
                return new CombatSnapshot 
                { 
                    IsComplete = true, 
                    IsVictory = true, 
                    EventLog = new List<CombatEvent> { new CombatEvent { Description = "Treasure collected successfully." } } 
                };
            }

            return null;
        }

        public void CompleteNode(TowerRunState runState, TowerNode node)
        {
            node.IsCleared = true;
            runState.TotalGoldEarned += node.GoldReward;
            runState.TotalXpEarned += node.XpReward;

            if (runState.CurrentFloorData.Nodes.All(n => n.IsCleared))
            {
                runState.CurrentFloorData.IsCleared = true;
                runState.CompletedFloors.Add(runState.CurrentFloorData);
                runState.CurrentFloor++;
                
                // SAVE PROGRESS: Update highest floor cleared
                try
                {
                    var saveService = ServiceRegistry.Resolve<ISaveLoadService>();
                    var save = saveService.Load();
                    if (runState.CurrentFloor - 1 > save.HighestFloorCleared)
                    {
                        save.HighestFloorCleared = runState.CurrentFloor - 1;
                        saveService.Save(save);
                        Debug.Log($"[TowerService] New highest floor saved: {save.HighestFloorCleared}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TowerService] Failed to save floor progress: {ex.Message}");
                }

                runState.CurrentFloorData = GenerateFloor(runState.CurrentFloor, runState.RunSeed);
                Debug.Log($"[TowerService] Floor cleared! Advancing to floor {runState.CurrentFloor}.");
            }
        }

        public List<HeroInstance> GenerateEnemies(int floorLevel, int count, int seed)
        {
            System.Random rng = new System.Random(seed);
            List<HeroInstance> enemies = new List<HeroInstance>();
            
            var allDefs = Resources.LoadAll<HeroDefinition>("");
            List<TowerEnemyTemplate> validTemplates = new List<TowerEnemyTemplate>();

            if (_enemyDatabase != null && _enemyDatabase.EnemyTemplates != null && _enemyDatabase.EnemyTemplates.Count > 0)
            {
                validTemplates = _enemyDatabase.EnemyTemplates
                    .Where(t => floorLevel >= t.MinFloor && floorLevel <= t.MaxFloor)
                    .ToList();
                    
                if (validTemplates.Count == 0) validTemplates = _enemyDatabase.EnemyTemplates;
            }
            else
            {
                foreach (var h in allDefs)
                {
                    validTemplates.Add(new TowerEnemyTemplate { HeroDefId = h.HeroId, SpawnWeight = 1.0f, StatMultiplier = 1.0f });
                }
            }

            for (int i = 0; i < count; i++)
            {
                float totalWeight = validTemplates.Sum(t => t.SpawnWeight);
                double roll = rng.NextDouble() * totalWeight;
                double current = 0;
                TowerEnemyTemplate selected = validTemplates[0];
                
                foreach (var t in validTemplates)
                {
                    current += t.SpawnWeight;
                    if (roll <= current)
                    {
                        selected = t;
                        break;
                    }
                }

                HeroDefinition def = allDefs.FirstOrDefault(h => h.HeroId == selected.HeroDefId);
                if (def == null) continue;

                HeroInstance enemy = new HeroInstance(def);
                
                float scale = selected.StatMultiplier + (floorLevel * 0.1f);
                enemy.MaxHP = (int)(enemy.MaxHP * scale);
                enemy.CurrentHP = enemy.MaxHP;
                enemy.ATK = (int)(enemy.ATK * scale);
                enemy.DEF = (int)(enemy.DEF * scale);
                enemy.SPD = (int)(enemy.SPD * scale);
                
                enemies.Add(enemy);
            }

            return enemies;
        }

        public TowerRunState GetActiveRun()
        {
            return _activeRun;
        }

        public void EndRun(TowerRunState runState)
        {
            if (runState != null)
            {
                runState.IsRunActive = false;
                Debug.Log($"[TowerService] Run ended at floor {runState.CurrentFloor}. Total Gold: {runState.TotalGoldEarned}, Total XP: {runState.TotalXpEarned}");
            }
            if (_activeRun == runState)
            {
                _activeRun = null;
            }
        }
    }
}