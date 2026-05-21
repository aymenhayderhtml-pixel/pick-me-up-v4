// Assets/Scripts/Services/Implementations/IdleProgressionService.cs
using System;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Stub implementation of IIdleProgressionService for MVP.
    /// Returns zero rewards; actual calculation logic to be added later.
    /// </summary>
    public class IdleProgressionService : IIdleProgressionService
    {
        public IdleReward CalculateOfflineGains(TimeSpan timeAway, SaveSnapshot snapshot)
        {
            return new IdleReward
            {
                GoldEarned = 0,
                XpEarned = 0,
                TimeSimulated = timeAway
            };
        }

        public TimeSpan GetMaxOfflineDuration()
        {
            return TimeSpan.FromHours(12);
        }
    }
}