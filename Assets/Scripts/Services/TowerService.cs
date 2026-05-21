// Assets/Scripts/Services/Implementations/TowerService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of the tower generation and management service.
    /// </summary>
    public class TowerService : ITowerService
    {
        #region Fields

        private TowerRunState _activeRun;
        private TowerEnemyDatabase _enemyDatabase;

        #endregion

        #region Constructor

        public TowerService()
        {
            // Load enemy database from Resources
            _enemyDatabase = Resources.Load<TowerEnemyDatabase>("TowerEnemyDatabase");
            if (_enemyDatabase == null)
            {
                Debug.LogWarning("[TowerService] TowerEnemyDatabase not found in Resources. Enemy generation will use fallback.");
            }
        }

        #endregion

        #region ITowerService Implementation

        public TowerRunState StartNewRun(List<HeroInstance> party, int startingFloor)
        {
            int seed = Environment.TickCount; // Base seed for the run
            
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
            Random rng = new Random(seed + floorLevel);
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
            Random rng = new Random(seed);
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
                int enemyCount = rng.Next(1, 5); // 1 to 4 enemies
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
                        // 2.0x stat multiplier and +50% HP (total 3.0x HP)
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
                    hero.Morale = Math.Min(10000, hero.Morale + 2000); // +20% morale
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

            // Check if all nodes on the current floor are cleared
            if (runState.CurrentFloorData.Nodes.All(n => n.IsCleared))
            {
                runState.CurrentFloorData.IsCleared = true;
                runState.CompletedFloors.Add(runState.CurrentFloorData);
                runState.CurrentFloor++;
                
                // Generate next floor
                runState.CurrentFloorData = GenerateFloor(runState.CurrentFloor, runState.RunSeed);
                Debug.Log($"[TowerService] Floor cleared! Advancing to floor {runState.CurrentFloor}.");
            }
        }

        public List<HeroInstance> GenerateEnemies(int floorLevel, int count, int seed)
        {
            Random rng = new Random(seed);
            List<HeroInstance> enemies = new List<HeroInstance>();
            IDataService dataService = ServiceRegistry.Resolve<IDataService>();

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
                // Fallback: create dummy templates from all heroes
                var allHeroes = dataService.GetAllHeroDefinitions();
                foreach (var h in allHeroes)
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

                HeroDefinition def = dataService.GetHeroDefinition(selected.HeroDefId);
                if (def == null) continue;

                HeroInstance enemy = new HeroInstance(def);
                
                // Scale stats by template multiplier and floor level
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

        #endregion
    }
}