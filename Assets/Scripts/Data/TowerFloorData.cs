using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TowerNode
{
    public string NodeId;
    public int EnemyLevel;
    public int EnemyPower; // Used for quick power comparison UI
    public List<string> EnemyHeroDefIds; // Which heroes to spawn
    public bool IsBossNode;
}

[CreateAssetMenu(fileName = "NewTowerFloor", menuName = "PickMeUp/Tower Floor Data")]
public class TowerFloorData : ScriptableObject
{
    public int FloorNumber;
    public List<TowerNode> Nodes = new List<TowerNode>();
    public int GoldReward;
    public int ExpReward;
    public int GemReward; // Bonus for clearing the whole floor
}