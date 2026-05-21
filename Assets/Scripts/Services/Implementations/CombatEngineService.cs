// Assets/Scripts/Services/Implementations/CombatEngineService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Combat;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of the deterministic combat engine.
    /// Completely headless and decoupled from Unity's update loop.
    /// </summary>
    public class CombatEngineService : ICombatEngineService
    {
        #region Constants

        private const int MAX_TURNS = 100;
        private const float BASIC_ATTACK_MULTIPLIER = 1.0f;
        private const int BASIC_ATTACK_ENERGY_GAIN = 20;

        #endregion

        #region ICombatEngineService Implementation

        public CombatSnapshot SimulateCombat(List<HeroInstance> heroParty, List<HeroInstance> enemyParty, int floorLevel, int seed)
        {
            IDataService dataService = ServiceRegistry.Resolve<IDataService>();
            
            CombatSnapshot snapshot = new CombatSnapshot
            {
                FloorLevel = floorLevel,
                Seed = seed,
                Heroes = ConvertToCombatUnits(heroParty, dataService, true),
                Enemies = ConvertToCombatUnits(enemyParty, dataService, false)
            };

            RunCombatLoop(snapshot, new Random(seed));
            return snapshot;
        }

        public CombatSnapshot SimulateHeadless(CombatSnapshot snapshot)
        {
            if (snapshot.IsComplete) return snapshot;
            
            // Recreate the RNG state. 
            // Note: For true mid-battle resumption, RNG state would need to be serialized.
            // For MVP, we re-seed. A production system would use a serializable PRNG state.
            Random random = new Random(snapshot.Seed); 
            
            // Fast-forward RNG to current turn to maintain determinism if resuming
            for (int i = 0; i < snapshot.TurnCount; i++) random.Next();

            RunCombatLoop(snapshot, random);
            return snapshot;
        }

        #endregion

        #region Combat Loop

        private void RunCombatLoop(CombatSnapshot snapshot, Random random)
        {
            while (!snapshot.IsComplete && snapshot.TurnCount < MAX_TURNS)
            {
                snapshot.TurnCount++;
                List<CombatUnit> turnOrder = BuildTurnOrder(snapshot, random);

                foreach (CombatUnit unit in turnOrder)
                {
                    if (!unit.IsAlive || snapshot.IsComplete) continue;

                    // Reduce cooldowns
                    foreach (var skill in unit.Skills)
                    {
                        if (skill.CooldownCurrent > 0) skill.CooldownCurrent--;
                    }

                    // Select Target (Prioritize front row)
                    List<CombatUnit> enemies = unit.IsHero ? snapshot.Enemies : snapshot.Heroes;
                    CombatUnit target = SelectTarget(enemies);
                    if (target == null) continue;

                    // Select Action
                    CombatSkillState usedSkill = null;
                    float multiplier = BASIC_ATTACK_MULTIPLIER;

                    if (unit.Skills != null)
                    {
                        var availableSkill = unit.Skills.FirstOrDefault(s => 
                            s.Type == SkillType.Active && 
                            s.CooldownCurrent == 0 && 
                            s.EnergyCurrent >= s.EnergyMax);

                        if (availableSkill != null)
                        {
                            usedSkill = availableSkill;
                            multiplier = availableSkill.PowerMultiplier;
                            availableSkill.CooldownCurrent = availableSkill.CooldownMax;
                            availableSkill.EnergyCurrent -= availableSkill.EnergyMax;
                            
                            LogEvent(snapshot, $"{unit.Name} uses {usedSkill.SkillId}!", CombatEventType.SkillUsed);
                        }
                    }

                    // If no skill used, basic attack generates energy
                    if (usedSkill == null)
                    {
                        var energySkill = unit.Skills?.FirstOrDefault(s => s.Type == SkillType.Active);
                        if (energySkill != null)
                        {
                            energySkill.EnergyCurrent = Math.Min(energySkill.EnergyMax, energySkill.EnergyCurrent + BASIC_ATTACK_ENERGY_GAIN);
                        }
                    }

                    // Resolve Damage
                    int damage = CombatFormulas.CalculateDamage(unit, target, multiplier, random);
                    target.CurrentHP = Math.Max(0, target.CurrentHP - damage);

                    string actionName = usedSkill != null ? usedSkill.SkillId : "Basic Attack";
                    LogEvent(snapshot, $"{unit.Name} hits {target.Name} with {actionName} for {damage} damage.", CombatEventType.Damage);

                    // Check Death
                    if (!target.IsAlive)
                    {
                        LogEvent(snapshot, $"{target.Name} has been defeated.", CombatEventType.Death);
                    }

                    // Check Win/Loss
                    if (CheckVictory(snapshot))
                    {
                        snapshot.IsComplete = true;
                        snapshot.IsVictory = true;
                        LogEvent(snapshot, "Heroes are victorious!", CombatEventType.Victory);
                        break;
                    }
                    
                    if (CheckDefeat(snapshot))
                    {
                        snapshot.IsComplete = true;
                        snapshot.IsVictory = false;
                        LogEvent(snapshot, "Heroes have been defeated.", CombatEventType.Defeat);
                        break;
                    }
                }
            }

            // Failsafe for max turns reached
            if (!snapshot.IsComplete)
            {
                snapshot.IsComplete = true;
                snapshot.IsVictory = false; // Timeout counts as defeat
                LogEvent(snapshot, "Battle timed out. Defeat.", CombatEventType.Defeat);
            }
        }

        #endregion

        #region Helper Methods

        private List<CombatUnit> BuildTurnOrder(CombatSnapshot snapshot, Random random)
        {
            List<CombatUnit> allAliveUnits = new List<CombatUnit>();
            allAliveUnits.AddRange(snapshot.Heroes.Where(h => h.IsAlive));
            allAliveUnits.AddRange(snapshot.Enemies.Where(e => e.IsAlive));
            
            return CombatFormulas.CalculateTurnOrder(allAliveUnits, random);
        }

        private CombatUnit SelectTarget(List<CombatUnit> enemies)
        {
            var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();
            if (!aliveEnemies.Any()) return null;

            // Target front row (Position 0) first
            var frontRow = aliveEnemies.Where(e => e.Position == 0).ToList();
            return frontRow.Any() ? frontRow.First() : aliveEnemies.First();
        }

        private bool CheckVictory(CombatSnapshot snapshot)
        {
            return snapshot.Enemies.All(e => !e.IsAlive);
        }

        private bool CheckDefeat(CombatSnapshot snapshot)
        {
            return snapshot.Heroes.All(h => !h.IsAlive);
        }

        private void LogEvent(CombatSnapshot snapshot, string description, CombatEventType type)
        {
            snapshot.EventLog.Add(new CombatEvent
            {
                TurnNumber = snapshot.TurnCount,
                Description = description,
                Type = type
            });
        }

        private List<CombatUnit> ConvertToCombatUnits(List<HeroInstance> instances, IDataService dataService, bool isHero)
        {
            List<CombatUnit> units = new List<CombatUnit>();
            if (instances == null) return units;

            for (int i = 0; i < instances.Count; i++)
            {
                HeroInstance inst = instances[i];
                HeroDefinition def = dataService.GetHeroDefinition(inst.HeroDefId);

                CombatUnit unit = new CombatUnit
                {
                    Name = def != null ? def.HeroName : inst.HeroDefId,
                    InstanceId = inst.InstanceId,
                    MaxHP = inst.MaxHP,
                    CurrentHP = inst.CurrentHP > 0 ? inst.CurrentHP : inst.MaxHP,
                    ATK = inst.ATK,
                    DEF = inst.DEF,
                    SPD = inst.SPD,
                    CritRate = inst.CritRate,
                    CritDmg = inst.CritDmg,
                    Element = def != null ? def.Element : ElementType.None,
                    ClassType = def != null ? def.ClassType : ClassType.Striker,
                    Position = i < 2 ? 0 : 1, // First 2 are front row, rest back row
                    IsHero = isHero,
                    Skills = new List<CombatSkillState>(),
                    Traits = new List<CombatTraitState>()
                };

                // Map Skills
                if (inst.EquippedSkills != null)
                {
                    foreach (var skillState in inst.EquippedSkills)
                    {
                        SkillDefinition skillDef = dataService.GetSkillDefinition(skillState.SkillDefId);
                        if (skillDef != null)
                        {
                            unit.Skills.Add(new CombatSkillState
                            {
                                SkillId = skillDef.SkillId,
                                Type = skillDef.Type,
                                CooldownMax = skillDef.CooldownTurns,
                                CooldownCurrent = 0,
                                EnergyMax = skillDef.EnergyCost,
                                EnergyCurrent = 0, // Start with 0 energy, basic attacks generate it
                                PowerMultiplier = 1.5f // Default MVP multiplier
                            });
                        }
                    }
                }

                units.Add(unit);
            }

            return units;
        }

        #endregion
    }
}