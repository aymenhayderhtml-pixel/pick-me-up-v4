using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Data
{
    [System.Serializable]
    public class GameSaveData
    {
        public long TotalGold;
        public long TotalExperience;
        public int CurrentFloor;
        public long LastIdleTimestamp;
        public List<string> OwnedHeroIds = new();

        // Pity tracking (using lists instead of dictionaries for serialization)
        public List<string> PityHeroIds = new();
        public List<int> PityPullCounts = new();
    }

    [System.Serializable]
    public class SerializableHeroInstance
    {
        public string HeroInstanceId;
        public string HeroDefinitionId;
        public int Level;
        public long Experience;
        public int CurrentHealth;
        public int AscensionLevel;
    }
}