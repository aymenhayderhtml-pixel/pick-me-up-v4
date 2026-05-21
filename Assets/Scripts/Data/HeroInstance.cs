// Assets/Scripts/Data/HeroInstance.cs
using System;
using System.Collections.Generic;

namespace PickMeUp.Data
{
    /// <summary>
    /// Represents a specific, owned instance of a hero in the player's roster.
    /// Contains runtime progression data (level, XP, ascension) separate from the static definition.
    /// </summary>
    [Serializable]
    public class HeroInstance
    {
        public string InstanceId;
        public string HeroDefId;
        public int Level;
        public int CurrentXP;
        public int AscensionLevel;
        public int CurrentHP;
        public int Morale;
        public int MaxHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public int CritRate;
        public int CritDmg;
        public List<SkillState> EquippedSkills;
        public List<TraitState> ActiveTraits;

        public HeroInstance()
        {
            EquippedSkills = new List<SkillState>();
            ActiveTraits = new List<TraitState>();
        }

        public HeroInstance(HeroDefinition definition) : this()
        {
            InstanceId = Guid.NewGuid().ToString();
            HeroDefId = definition.HeroId;
            Level = 1;
            CurrentXP = 0;
            AscensionLevel = 0;
            Morale = 10000;
            MaxHP = definition.BaseHP;
            CurrentHP = MaxHP;
            ATK = definition.BaseATK;
            DEF = definition.BaseDEF;
            SPD = definition.BaseSPD;
            CritRate = definition.BaseCritRate;
            CritDmg = definition.BaseCritDmg;

            foreach (var skillRef in definition.Skills)
            {
                if (skillRef.UnlockLevel <= Level)
                {
                    EquippedSkills.Add(new SkillState(skillRef.Skill));
                }
            }
        }
    }
}