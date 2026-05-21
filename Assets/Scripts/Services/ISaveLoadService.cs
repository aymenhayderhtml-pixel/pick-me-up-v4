// Assets/Scripts/Services/ISaveLoadService.cs
using System;

namespace PickMeUp.Services
{
    public interface ISaveLoadService
    {
        event Action OnSaveCompleted;
        void Save(GameSaveData data);
        GameSaveData Load();
        bool HasSave();
        void DeleteSave();
    }
}