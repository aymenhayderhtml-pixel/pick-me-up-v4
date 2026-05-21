using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class IdleProgressionService : IIdleProgressionService
    {
        public IdleReward CalculateIdleReward(GameSaveData saveData, long elapsedSeconds)
        {
            // Stub: return zero for now
            return new IdleReward { Gold = 0, Experience = 0 };
        }
    }
}