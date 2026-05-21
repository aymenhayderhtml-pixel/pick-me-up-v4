// Assets/Scripts/Services/IHeroProgressionService.cs
using System;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages hero leveling, XP calculation, and stat progression.
    /// </summary>
    public interface IHeroProgressionService
    {
        /// <summary>
        /// Calculates the XP required to reach the next level.
        /// </summary>
        long GetXPRequiredForLevel(int star, int currentLevel);

        /// <summary>
        /// Adds XP to a hero, handling multiple level-ups and XP capping.
        /// </summary>
        void AddXP(HeroInstance hero, long xp);

        /// <summary>
        /// Attempts to level up the hero once if they have sufficient XP.
        /// </summary>
        /// <returns>True if the hero leveled up, false otherwise.</returns>
        bool TryLevelUp(HeroInstance hero);

        /// <summary>
        /// Recalculates all combat stats based on the hero's current star and level.
        /// </summary>
        void RecalculateStats(HeroInstance hero);

        /// <summary>
        /// Triggered when a hero successfully levels up.
        /// </summary>
        event Action<HeroInstance, int> OnHeroLevelUp;
    }
}
