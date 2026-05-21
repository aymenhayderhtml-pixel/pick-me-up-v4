// Assets/Scripts/Services/ServiceModels.cs
using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Placeholder models used as inputs/outputs for service interfaces.
    /// These will be fully fleshed out during the implementation phase.
    /// </summary>

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

    [Serializable]
    public class IdleReward
    {
        public int GoldEarned;
        public int XpEarned;
        public TimeSpan TimeSimulated;
    }

    [Serializable]
    public class SaveSnapshot
    {
        public int LastClearedFloor;
        public List<HeroInstance> ActiveParty;
    }
}