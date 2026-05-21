// Assets/Scripts/Services/ISaveLoadService.cs
using System;
using PickMeUp.Data;

namespace PickMeUp.Services
{
    /// <summary>
    /// Handles serialization, encryption, and persistence of the player's progress.
    /// </summary>
    public interface ISaveLoadService
    {
        /// <summary>
        /// Event triggered when a save operation successfully completes.
        /// </summary>
        event Action OnSaveCompleted;

        /// <summary>
        /// Serializes and writes the game data to persistent storage.
        /// </summary>
        void Save(GameSaveData data);

        /// <summary>
        /// Reads, decrypts, and deserializes game data from persistent storage.
        /// </summary>
        GameSaveData Load();

        /// <summary>
        /// Checks if a valid save file exists on the disk.
        /// </summary>
        bool HasSave();

        /// <summary>
        /// Permanently deletes the current save file.
        /// </summary>
        void DeleteSave();
    }
}