using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    public class IdleProgressionService : IIdleProgressionService
    {
        private const int MAX_SIMULATED_FLOORS = 50; // Prevent startup lag
        private const int MORALE_DRAIN_PER_NODE = 500; // 5% morale drain per combat

        public IdleReward CalculateOfflineGains(TimeSpan timeAway, GameSaveData saveData)
        {
            IdleReward reward = new IdleReward { TimeSimulated = timeAway };

            if (saveData.OfflinePartySnapshot == null || saveData.OfflinePartySnapshot.Count == 0)
            {
                Debug.Log("[IdleProgression] No offline party found. Skipping simulation.");
                return reward;
            }

            TimeSpan maxTime = GetMaxOfflineDuration();
            if (timeAway > maxTime) timeAway = maxTime;

            double totalSecondsAvailable = timeAway.TotalSeconds;
            double secondsUsed = 0;
            int floorsCleared = 0;
            int currentFloor = saveData.OfflineFloorLevel > 0 ? saveData.OfflineFloorLevel : 1;

            // Clone party so we don't mutate the actual save during calculation
            List<HeroInstance> activeParty = CloneParty(saveData.OfflinePartySnapshot);
            
            ICombatEngineService combatEngine = ServiceRegistry.Resolve<ICombatEngineService>();
            ITowerService towerService = ServiceRegistry.Resolve<ITowerService>();

            bool partyWiped = false;
            bool outOfMorale = false;

            // 1. Headless Simulation Loop (Capped to prevent freezing)
            while (floorsCleared < MAX_SIMULATED_FLOORS && secondsUsed < totalSecondsAvailable && !partyWiped && !outOfMorale)
            {
                // Check Morale
                if (activeParty.All(h => h.Morale <= 0)) 
                {
                    outOfMorale = true;
                    break;
                }

                // Generate a floor and simulate its combat nodes
                TowerFloorData floor = towerService.GenerateFloor(currentFloor, Environment.TickCount + currentFloor);
                bool floorCleared = true;

                foreach (var node in floor.Nodes)
                {
                    if (node.Type == TowerNodeType.Combat || node.Type == TowerNodeType.Elite || node.Type == TowerNodeType.Boss)
                    {
                        CombatSnapshot result = combatEngine.SimulateCombat(activeParty, node.Enemies, currentFloor, Environment.TickCount);
                        
                        // Estimate 3 seconds per combat node
                        secondsUsed += 3; 

                        if (!result.IsVictory)
                        {
                            partyWiped = true;
                            floorCleared = false;
                            break;
                        }

                        // Apply Morale Drain
                        foreach (var hero in activeParty)
                        {
                            hero.Morale = Math.Max(0, hero.Morale - MORALE_DRAIN_PER_NODE);
                        }

                        // Sync HP back to active party for next node
                        for (int i = 0; i < activeParty.Count; i++)
                        {
                            activeParty[i].CurrentHP = result.Heroes[i].CurrentHP;
                        }
                    }
                    else if (node.Type == TowerNodeType.Rest)
                    {
                        foreach (var hero in activeParty) hero.Morale = Math.Min(10000, hero.Morale + 2000);
                        secondsUsed += 1; // 1 second to rest
                    }
                    else
                    {
                        secondsUsed += 1; // 1 second for treasure
                    }
                }

                if (floorCleared)
                {
                    floorsCleared++;
                    reward.GoldEarned += floor.Nodes.Sum(n => n.GoldReward);
                    reward.XpEarned += floor.Nodes.Sum(n => n.XpReward);
                    currentFloor++;
                }
            }

            // 2. Mathematical Extrapolation (If they are strong and time remains)
            if (floorsCleared >= MAX_SIMULATED_FLOORS && secondsUsed < totalSecondsAvailable && !partyWiped && !outOfMorale)
            {
                double avgSecondsPerFloor = secondsUsed / floorsCleared;
                double remainingSeconds = totalSecondsAvailable - secondsUsed;
                int extrapolatedFloors = (int)(remainingSeconds / avgSecondsPerFloor);

                // Cap extrapolation to 1000 floors to prevent economy breaking
                extrapolatedFloors = Math.Min(extrapolatedFloors, 1000); 

                int avgGoldPerFloor = reward.GoldEarned / Math.Max(1, floorsCleared);
                int avgXpPerFloor = reward.XpEarned / Math.Max(1, floorsCleared);

                reward.GoldEarned += extrapolatedFloors * avgGoldPerFloor;
                reward.XpEarned += extrapolatedFloors * avgXpPerFloor;
                floorsCleared += extrapolatedFloors;
                currentFloor += extrapolatedFloors;
                
                // Assume they ran out of morale at the end of extrapolation
                foreach (var hero in activeParty) hero.Morale = 0; 
            }

            reward.FloorsCleared = floorsCleared;
            reward.FinalFloorReached = currentFloor;

            Debug.Log($"[IdleProgression] Simulated {timeAway.TotalMinutes:F1} mins. Cleared {floorsCleared} floors. Gold: {reward.GoldEarned}, XP: {reward.XpEarned}");
            return reward;
        }

        public TimeSpan GetMaxOfflineDuration()
        {
            return TimeSpan.FromHours(24); // 24 hours max offline cap
        }

        private List<HeroInstance> CloneParty(List<HeroInstance> original)
        {
            List<HeroInstance> clone = new List<HeroInstance>();
            foreach (var h in original)
            {
                // Shallow copy is fine for our read-only simulation needs, but we need fresh HP/Morale
                var copy = new HeroInstance 
                { 
                    InstanceId = h.InstanceId, HeroDefId = h.HeroDefId, CurrentStar = h.CurrentStar, 
                    CurrentLevel = h.CurrentLevel, MaxHP = h.MaxHP, CurrentHP = h.MaxHP, 
                    ATK = h.ATK, DEF = h.DEF, SPD = h.SPD, CritRate = h.CritRate, CritDmg = h.CritDmg,
                    Morale = h.Morale, EquippedSkills = h.EquippedSkills
                };
                clone.Add(copy);
            }
            return clone;
        }
    }
}