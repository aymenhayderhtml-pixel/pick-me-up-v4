using System;
using PickMeUp.Core;
using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class HeroProgressionService : IHeroProgressionService
    {
        public bool CanLevelUp(HeroInstance hero, out int goldCost)
        {
            goldCost = 0;
            var def = ServiceRegistry.Resolve<IDataService>().GetHeroDefinition(hero.HeroDefId);
            if (def == null) return false;

            int maxLvl = GetMaxLevelForStar(hero.CurrentStar, def);
            if (hero.CurrentLevel >= maxLvl) return false;

            int xpNeeded = hero.CurrentLevel * 100 * hero.CurrentStar;
            if (hero.CurrentXP < xpNeeded) return false;

            goldCost = hero.CurrentLevel * 50 * hero.CurrentStar;
            var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
            if (save.Gold < goldCost) return false;

            return true;
        }

        public bool LevelUp(HeroInstance hero)
        {
            var def = ServiceRegistry.Resolve<IDataService>().GetHeroDefinition(hero.HeroDefId);
            if (def == null || !CanLevelUp(hero, out int cost)) return false;

            int xpNeeded = hero.CurrentLevel * 100 * hero.CurrentStar;
            hero.CurrentXP -= xpNeeded;
            hero.CurrentLevel++;

            var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
            save.Gold -= cost;
            ServiceRegistry.Resolve<ISaveLoadService>().Save(save);

            UpdateStats(hero, def);
            return true;
        }

        public void AddXP(HeroInstance hero, int amount)
        {
            hero.CurrentXP += amount;
            // XP just accumulates. Player spends Gold to convert XP into Levels.
        }

        public int GetMaxLevelForStar(int star, HeroDefinition def)
        {
            if (def != null && def.MaxLevelPerStar != null && star > 0 && star <= def.MaxLevelPerStar.Length)
                return def.MaxLevelPerStar[star - 1];
            return star * 10; // Fallback
            
        }

        public void UpdateStats(HeroInstance hero, HeroDefinition def)
        {
            float levelMult = 1f + (hero.CurrentLevel - 1) * 0.1f;
            float starMult = 1f + (hero.CurrentStar - 1) * 0.25f;
            
            hero.MaxHP = (int)(def.BaseHP * levelMult * starMult);
            hero.ATK = (int)(def.BaseATK * levelMult * starMult);
            hero.DEF = (int)(def.BaseDEF * levelMult * starMult);
            hero.SPD = (int)(def.BaseSPD * (1 + (hero.CurrentLevel - 1) * 0.02f));
            hero.CurrentHP = hero.MaxHP; // Full heal on level up
        }
                public void RecalculateStats(HeroInstance hero, HeroDefinition def)
        {
            UpdateStats(hero, def);
        }
    }
}