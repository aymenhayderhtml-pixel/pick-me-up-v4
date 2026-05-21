// Assets/Scripts/Services/IIdleProgressionService.cs
namespace PickMeUp.Services
{
    public interface IIdleProgressionService
    {
        IdleReward CalculateOfflineGains(long offlineSeconds);
        long GetMaxOfflineDuration();
    }
}