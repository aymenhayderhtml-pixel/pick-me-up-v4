using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IHeroProgressionService
    {
        bool CanLevelUp(HeroInstance hero, out int goldCost);
        bool LevelUp(HeroInstance hero);
        void AddXP(HeroInstance hero, int amount);
        int GetMaxLevelForStar(int star, HeroDefinition def);
        void UpdateStats(HeroInstance hero, HeroDefinition def);
        void RecalculateStats(HeroInstance hero, HeroDefinition def); // ✅ Added
    }
}