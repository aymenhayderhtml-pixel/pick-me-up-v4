// Assets/Scripts/Services/IGachaService.cs
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Manages the summoning mechanics, banner configurations, and pity systems.
    /// </summary>
    public interface IGachaService
    {
        /// <summary>
        /// Executes a single pull on the specified banner.
        /// </summary>
        HeroInstance Pull(int bannerId);

        /// <summary>
        /// Executes a multi-pull (usually 10x) on the specified banner.
        /// </summary>
        HeroInstance[] PullMultiple(int bannerId, int count);

        /// <summary>
        /// Increments and evaluates the pity counter for a specific banner.
        /// </summary>
        void TrackPity(int bannerId);

        /// <summary>
        /// Retrieves the current pity count for a specific banner.
        /// </summary>
        int GetPityCount(int bannerId);
    }
}