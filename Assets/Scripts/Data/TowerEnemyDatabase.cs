// Assets/Scripts/Data/TowerEnemyDatabase.cs
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Data
{
    /// <summary>
    /// ScriptableObject database containing all possible enemy templates for tower generation.
    /// </summary>
    [CreateAssetMenu(fileName = "TowerEnemyDatabase", menuName = "PickMeUp/Data/Tower Enemy Database")]
    public class TowerEnemyDatabase : ScriptableObject
    {
        [Tooltip("List of hero definitions that can spawn as enemies, with floor ranges and scaling.")]
        public List<TowerEnemyTemplate> EnemyTemplates;
    }
}