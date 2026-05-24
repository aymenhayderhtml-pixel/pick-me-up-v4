using System;
using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Data
{
    public enum TowerNodeType { Combat, Elite, Boss, Rest, Treasure }

    [Serializable]
    public class TowerNode
    {
        public int NodeId;
        public TowerNodeType Type;
        public int FloorLevel;
        public string Description;
        public int GoldReward;
        public int XpReward;
        public List<HeroInstance> Enemies = new List<HeroInstance>();
        public bool IsCleared;
    }

    [Serializable]
    public class TowerFloorData
    {
        public int FloorLevel;
        public bool IsBossFloor;
        public int Seed;
        public List<TowerNode> Nodes = new List<TowerNode>();
        public bool IsCleared;
    }

    [Serializable]
    public class TowerRunState
    {
        public int CurrentFloor;
        public List<HeroInstance> ActiveParty = new List<HeroInstance>();
        public int RunSeed;
        public bool IsRunActive;
        public TowerFloorData CurrentFloorData;
        public List<TowerFloorData> CompletedFloors = new List<TowerFloorData>();
        public int TotalGoldEarned;
        public int TotalXpEarned;
    }

    [Serializable]
    public class TowerEnemyTemplate
    {
        public string HeroDefId;
        public int MinFloor;
        public int MaxFloor;
        public float SpawnWeight = 1.0f;
        public float StatMultiplier = 1.0f;
    }
}