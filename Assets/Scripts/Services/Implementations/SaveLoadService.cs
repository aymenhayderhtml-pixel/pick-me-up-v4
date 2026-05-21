using System.Text;
using UnityEngine;
using PickMeUp.Data;

namespace PickMeUp.Services.Implementations
{
    public class SaveLoadService : ISaveLoadService
    {
        private const string SaveKey = "PickMeUpSave";
        private const byte XorKey = 0xAB;

        public void SaveGame(GameSaveData data)
        {
            var json = JsonUtility.ToJson(data);
            var encrypted = EncryptString(json);
            PlayerPrefs.SetString(SaveKey, encrypted);
            PlayerPrefs.Save();
        }

        public GameSaveData LoadGame()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return new GameSaveData();

            var encrypted = PlayerPrefs.GetString(SaveKey);
            var json = DecryptString(encrypted);
            return JsonUtility.FromJson<GameSaveData>(json) ?? new GameSaveData();
        }

        private string EncryptString(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= XorKey;
            return System.Convert.ToBase64String(bytes);
        }

        private string DecryptString(string input)
        {
            try
            {
                var bytes = System.Convert.FromBase64String(input);
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] ^= XorKey;
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return "";
            }
        }
    }
}