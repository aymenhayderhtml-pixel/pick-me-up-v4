// Assets/Scripts/Data/TowerModels.cs
using System;
using System.Collections.Generic;

namespace PickMeUp.Data
{
    /// <summary>
    /// Defines the type of encounter or event on a tower floor.
    /// </summary>
    public enum TowerNodeType
    {
        Combat,
        Elite,
        Rest,
        Treasure,
        Boss
    }

    /// <summary>
    /// Represents a single node (encounter/event) on a tower floor.
    /// </summary>
    [Serializable]
    public class TowerNode
    {
        public int NodeId;
        public TowerNodeType Type;
        public int FloorLevel;
        public string Description;
        public bool IsCleared;
        public List<HeroInstance> Enemies;
        public int GoldReward;
        public int XpReward;

        public TowerNode()
        {
            Enemies = new List<HeroInstance>();
        }
    }

    /// <summary>
    /// Represents a single floor in the tower, containing multiple nodes.
    /// </summary>
    [Serializable]
    public class TowerFloorData
    {
        public int FloorLevel;
        public List<TowerNode> Nodes;
        public bool IsBossFloor;
        public bool IsCleared;
        public int Seed;

        public TowerFloorData()
        {
            Nodes = new List<TowerNode>();
        }
    }

    /// <summary>
    /// Tracks the complete state of an active tower run.
    /// </summary>
    [Serializable]
    public class TowerRunState
    {
        public int CurrentFloor;
        public List<TowerFloorData> CompletedFloors;
        public TowerFloorData CurrentFloorData;
        public List<HeroInstance> ActiveParty;
        public int TotalGoldEarned;
        public int TotalXpEarned;
        public int RunSeed;
        public bool IsRunActive;

        public TowerRunState()
        {
            CompletedFloors = new List<TowerFloorData>();
            ActiveParty = new List<HeroInstance>();
        }
    }

    /// <summary>
    /// Template definition for generating enemies in the tower.
    /// </summary>
    [Serializable]
    public class TowerEnemyTemplate
    {
        public string HeroDefId;
        public int MinFloor;
        public int MaxFloor;
        public float SpawnWeight;
        public float StatMultiplier;
    }
}