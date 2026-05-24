using System;
using System.Collections.Generic;
using PickMeUp.Services; // Needed for CombatSkillState if it's in the Services namespace

namespace PickMeUp.Data
{
    [Serializable]
    public class HeroInstance
    {
        public string InstanceId;
        public string HeroDefId;
        public int CurrentStar;
        public int CurrentLevel;
        public int CurrentXP;
        public bool IsLocked;
        public int PromotionCount;

        public int MaxHP;
        public int CurrentHP;
        public int ATK;
        public int DEF;
        public int SPD;
        public int CritRate;
        public int CritDmg;
        public int Morale;

        public List<CombatSkillState> EquippedSkills;

        public HeroInstance() { EquippedSkills = new List<CombatSkillState>(); }

        public HeroInstance(HeroDefinition definition)
        {
            InstanceId = Guid.NewGuid().ToString();
            HeroDefId = definition.HeroId;
            CurrentStar = definition.BaseStar;
            CurrentLevel = 1;
            CurrentXP = 0;
            MaxHP = definition.BaseHP;
            CurrentHP = MaxHP;
            ATK = definition.BaseATK;
            DEF = definition.BaseDEF;
            SPD = definition.BaseSPD;
            CritRate = definition.BaseCritRate;
            CritDmg = definition.BaseCritDmg;
            Morale = 10000;
            IsLocked = false;
            PromotionCount = 0;
            EquippedSkills = new List<CombatSkillState>();
        }

        public bool IsAlive => CurrentHP > 0;
    }
}