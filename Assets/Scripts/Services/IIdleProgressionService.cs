using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface IIdleProgressionService
    {
        IdleReward CalculateIdleReward(GameSaveData saveData, long elapsedSeconds);
    }
}