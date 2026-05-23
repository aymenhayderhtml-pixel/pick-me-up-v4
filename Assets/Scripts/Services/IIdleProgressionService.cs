using System;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IIdleProgressionService
    {
        /// <summary>
        /// Calculates the resources earned while the player was offline.
        /// </summary>
        IdleReward CalculateOfflineGains(TimeSpan timeAway, GameSaveData saveData);

        /// <summary>
        /// Gets the maximum allowed duration for offline progression calculations.
        /// </summary>
        TimeSpan GetMaxOfflineDuration();
    }
}