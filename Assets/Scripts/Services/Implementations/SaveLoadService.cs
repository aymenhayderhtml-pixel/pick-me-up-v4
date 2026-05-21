// Assets/Scripts/Services/Implementations/SaveLoadService.cs
using System;
using System.Text;
using UnityEngine;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.Services.Implementations
{
    /// <summary>
    /// Concrete implementation of ISaveLoadService using PlayerPrefs for storage
    /// with simple XOR encryption and Base64 encoding.
    /// </summary>
    public class SaveLoadService : ISaveLoadService
    {
        private const string SAVE_KEY = "GameSave";
        private const string XOR_KEY = "PickMeUpV4";

        public event Action OnSaveCompleted;

        public void Save(GameSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                string encrypted = Encrypt(json);
                PlayerPrefs.SetString(SAVE_KEY, encrypted);
                PlayerPrefs.Save();

                Debug.Log("[SaveLoadService] Save completed successfully");
                OnSaveCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadService] Save failed: {ex.Message}");
            }
        }

        public GameSaveData Load()
        {
            try
            {
                if (!HasSave())
                {
                    Debug.Log("[SaveLoadService] No save file found, returning new default data");
                    return new GameSaveData();
                }

                string encrypted = PlayerPrefs.GetString(SAVE_KEY);
                string json = Decrypt(encrypted);
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                if (data == null)
                {
                    Debug.LogWarning("[SaveLoadService] Deserialization returned null, returning new default data");
                    return new GameSaveData();
                }

                Debug.Log("[SaveLoadService] Load completed successfully");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveLoadService] Load failed: {ex.Message}");
                return new GameSaveData();
            }
        }

        public bool HasSave()
        {
            return PlayerPrefs.HasKey(SAVE_KEY);
        }

        public void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            Debug.Log("[SaveLoadService] Save deleted");
        }

        private string Encrypt(string plaintext)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] keyBytes = Encoding.UTF8.GetBytes(XOR_KEY);

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Convert.ToBase64String(bytes);
        }

        private string Decrypt(string ciphertext)
        {
            byte[] bytes = Convert.FromBase64String(ciphertext);
            byte[] keyBytes = Encoding.UTF8.GetBytes(XOR_KEY);

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}