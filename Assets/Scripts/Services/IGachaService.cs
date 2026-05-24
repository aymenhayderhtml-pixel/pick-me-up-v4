using System.Collections.Generic;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the summoning mechanics, banner configurations, and pity systems.
    /// </summary>
    public interface IGachaService
    {
        /// <summary>
        /// Executes a pull on the Standard (Gold) banner.
        /// </summary>
        List<HeroInstance> PullStandard(int count);

        /// <summary>
        /// Executes a pull on the Premium (Gem) banner.
        /// </summary>
        List<HeroInstance> PullPremium(int count);

        /// <summary>
        /// Checks if the player has enough Gold for a Standard pull.
        /// </summary>
        bool CanAffordStandard(int count);

        /// <summary>
        /// Checks if the player has enough Gems for a Premium pull.
        /// </summary>
        bool CanAffordPremium(int count);
        
        /// <summary>
        /// Gets the current pity count for a specific banner (0 = Standard, 1 = Premium).
        /// </summary>
        int GetPityCount(int bannerId);
    }
}