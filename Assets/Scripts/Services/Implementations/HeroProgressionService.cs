// Assets/Scripts/Services/Implementations/HeroProgressionService.cs
using System;
using System.Linq;
using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Core;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of hero progression, handling the exponential power curves
    /// that define the gap between star tiers in the manhwa.
    /// </summary>
    public class HeroProgressionService : IHeroProgressionService
    {
        #region Events

        public event Action<HeroInstance, int> OnHeroLevelUp;

        #endregion

        #region IHeroProgressionService Implementation

        public long GetXPRequiredForLevel(int star, int currentLevel)
        {
            long baseXP = 100 * star;
            float levelMultiplier = Mathf.Pow(1.15f, currentLevel);
            return (long)(baseXP * levelMultiplier);
        }

        public void AddXP(HeroInstance hero, long xp)
        {
            if (hero == null || xp <= 0) return;

            HeroDefinition definition = GetDefinition(hero);
            if (definition == null) return;

            hero.CurrentXP += xp;
            int maxLevel = definition.MaxLevelPerStar[hero.CurrentStar - 1];

            bool leveledUp = true;
            while (leveledUp)
            {
                leveledUp = TryLevelUp(hero);
            }

            // Cap XP if at max level for the current star tier
            if (hero.CurrentLevel >= maxLevel)
            {
                long maxXP = GetXPRequiredForLevel(hero.CurrentStar, hero.CurrentLevel) - 1;
                if (maxXP < 0) maxXP = 0;
                if (hero.CurrentXP > maxXP)
                {
                    hero.CurrentXP = maxXP;
                }
            }
        }

        public bool TryLevelUp(HeroInstance hero)
        {
            if (hero == null) return false;

            HeroDefinition definition = GetDefinition(hero);
            if (definition == null) return false;

            int maxLevel = definition.MaxLevelPerStar[hero.CurrentStar - 1];
            if (hero.CurrentLevel >= maxLevel) return false;

            long requiredXP = GetXPRequiredForLevel(hero.CurrentStar, hero.CurrentLevel);
            if (hero.CurrentXP >= requiredXP)
            {
                hero.CurrentXP -= requiredXP;
                hero.CurrentLevel++;
                RecalculateStats(hero);
                OnHeroLevelUp?.Invoke(hero, hero.CurrentLevel);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Higher-star heroes have exponentially steeper growth curves, matching the manhwa's power gap between tiers.
        /// A 5★ hero will vastly outscale a 4★ hero even at lower levels due to the starMultiplier.
        /// </summary>
        public void RecalculateStats(HeroInstance hero)
        {
            if (hero == null) return;

            HeroDefinition definition = GetDefinition(hero);
            if (definition == null) return;

            // Exponential star scaling: 1★=1.0, 2★=1.4, 3★=1.96, 4★=2.74, 5★=3.84, 6★=5.38, 7★=7.53
            float starMultiplier = Mathf.Pow(1.4f, hero.CurrentStar - 1);

            int maxLevel = definition.MaxLevelPerStar[hero.CurrentStar - 1];
            float levelProgress = (maxLevel > 1) ? (float)(hero.CurrentLevel - 1) / (maxLevel - 1) : 0f;

            // Linear level scaling on top of the exponential star base (up to 1.5x at max level)
            float levelMultiplier = 1f + (levelProgress * 0.5f);

            hero.MaxHP = (int)(definition.BaseHP * starMultiplier * levelMultiplier);
            hero.ATK = (int)(definition.BaseATK * starMultiplier * levelMultiplier);
            hero.DEF = (int)(definition.BaseDEF * starMultiplier * levelMultiplier);
            hero.SPD = (int)(definition.BaseSPD * starMultiplier * levelMultiplier);

            // Crit stats remain flat from definition
            hero.CritRate = definition.BaseCritRate;
            hero.CritDmg = definition.BaseCritDmg;

            // Ensure current HP doesn't exceed new max HP
            hero.CurrentHP = Mathf.Min(hero.CurrentHP, hero.MaxHP);

            // Update EquippedSkills: unlock new skills without removing existing ones
            if (definition.Skills != null)
            {
                foreach (var skillRef in definition.Skills)
                {
                    if (skillRef.UnlockLevel <= hero.CurrentLevel)
                    {
                        if (!hero.EquippedSkills.Any(s => s.SkillDefId == skillRef.Skill.SkillId))
                        {
                            hero.EquippedSkills.Add(new SkillState(skillRef.Skill));
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private HeroDefinition GetDefinition(HeroInstance hero)
        {
            if (hero.CachedDefinition != null && hero.CachedDefinition.HeroId == hero.HeroDefId)
                return hero.CachedDefinition;

            var def = Resources.LoadAll<HeroDefinition>("").FirstOrDefault(h => h.HeroId == hero.HeroDefId);
            hero.CachedDefinition = def;
            return def;
        }

        #endregion
    }
}
