using PickMeUp.Data;

namespace PickMeUp.Services
{
    public interface ISaveLoadService
    {
        void SaveGame(GameSaveData data);
        GameSaveData LoadGame();
    }
}