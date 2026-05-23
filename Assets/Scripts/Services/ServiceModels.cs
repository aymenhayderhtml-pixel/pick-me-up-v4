using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    [Serializable]
    public class CombatInput
    {
        public List<HeroInstance> Party;
        public int FloorLevel;
        public int NodeSeed;
    }

    [Serializable]
    public class CombatResult
    {
        public bool IsVictory;
        public int TurnsTaken;
        public List<HeroInstance> UpdatedPartyState;
    }

    [Serializable]
    public class CombatEventLog
    {
        public List<string> Events;
    }

    /// <summary>
    /// Contains the calculated rewards from offline progression.
    /// </summary>
    [Serializable]
    public class IdleReward
    {
        public int GoldEarned;
        public int XpEarned;
        public int FloorsCleared;
        public int FinalFloorReached;
        public TimeSpan TimeSimulated;
    }

    [Serializable]
    public class SaveSnapshot
    {
        public int LastClearedFloor;
        public List<HeroInstance> ActiveParty;
    }
}