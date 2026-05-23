// Assets/Scripts/Data/HeroInstance.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PickMeUp.Data
{
    /// <summary>
    /// Represents a specific, owned instance of a hero in the player's roster.
    /// Contains runtime progression data separate from the static definition.
    /// </summary>
    [Serializable]
    public class HeroInstance
    {
        #region Fields

        public string InstanceId;
        public string HeroDefId;
        
        public int CurrentStar;
        public int CurrentLevel;
        public long CurrentXP;
        public bool IsLocked;
        public int PromotionCount;

        public int AscensionLevel;
        
        public int CurrentHP;
        public int Morale; // 0 to 10000 (basis points)

        // Calculated runtime stats
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public int CritRate;
        public int CritDmg;

        public List<SkillState> EquippedSkills;
        public List<TraitState> ActiveTraits;

        /// <summary>
        /// Runtime cache for the ScriptableObject definition. Not serialized to JSON.
        /// </summary>
        [NonSerialized] public HeroDefinition CachedDefinition;

        #endregion

        #region Constructors

        /// <summary>
        /// Parameterless constructor for JSON serialization.
        /// </summary>
        public HeroInstance() 
        {
            EquippedSkills = new List<SkillState>();
            ActiveTraits = new List<TraitState>();
        }

        /// <summary>
        /// Creates a new HeroInstance based on a static HeroDefinition.
        /// </summary>
        public HeroInstance(HeroDefinition definition) : this()
        {
            InstanceId = Guid.NewGuid().ToString();
            HeroDefId = definition.HeroId;
            
            CurrentStar = definition.BaseStar;
            CurrentLevel = 1;
            CurrentXP = 0;
            IsLocked = false;
            PromotionCount = 0;
            
            AscensionLevel = 0;
            Morale = 10000; // 100%

            // Copy base stats
            MaxHP = definition.BaseHP;
            CurrentHP = MaxHP;
            ATK = definition.BaseATK;
            DEF = definition.BaseDEF;
            SPD = definition.BaseSPD;
            CritRate = definition.BaseCritRate;
            CritDmg = definition.BaseCritDmg;

            CachedDefinition = definition;

            // Initialize skills based on definition
            foreach (var skillRef in definition.Skills)
            {
                if (skillRef.UnlockLevel <= CurrentLevel)
                {
                    EquippedSkills.Add(new SkillState(skillRef.Skill));
                }
            }
        }

        #endregion
    }
}