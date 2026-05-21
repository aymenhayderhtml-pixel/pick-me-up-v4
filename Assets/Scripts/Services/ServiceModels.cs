// Assets/Scripts/Services/ServiceModels.cs
using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    [Serializable]
    public class CombatInput
    {
        public List<HeroInstance> PlayerParty { get; set; } = new();
        public List<HeroInstance> EnemyParty { get; set; } = new();
    }

    [Serializable]
    public class CombatResult
    {
        public bool PlayerWon { get; set; }
        public List<HeroInstance> SurvivorParty { get; set; } = new();
        public List<CombatEventLog> EventLog { get; set; } = new();
    }

    [Serializable]
    public class CombatEventLog
    {
        public string Description { get; set; }
        public int TurnNumber { get; set; }
        public string ActorName { get; set; }
    }

    [Serializable]
    public class IdleReward
    {
        public long Gold { get; set; }
        public long Experience { get; set; }
        public int HoursOffline { get; set; }
    }

    [Serializable]
    public class SaveSnapshot
    {
        public GameSaveData SaveData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}