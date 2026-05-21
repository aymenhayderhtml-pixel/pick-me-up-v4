// Assets/Scripts/Services/IIdleProgressionService.cs
using System;

namespace PickMeUp.Services
{
    /// <summary>
    /// Calculates and manages resource generation while the application is closed or idle.
    /// </summary>
    public interface IIdleProgressionService
    {
        /// <summary>
        /// Calculates the resources earned while the player was offline.
        /// </summary>
        IdleReward CalculateOfflineGains(TimeSpan timeAway, SaveSnapshot snapshot);

        /// <summary>
        /// Gets the maximum allowed duration for offline progression calculations.
        /// </summary>
        TimeSpan GetMaxOfflineDuration();
    }
}