using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Services;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class HubView : MonoBehaviour
    {
        private Text _displayText;
        private ISaveLoadService _saveLoadService;

        private void Awake()
        {
            _displayText = GetComponent<Text>();
            _saveLoadService = ServiceRegistry.Resolve<ISaveLoadService>();

            RefreshDisplay();
        }

        public void SetHeroText(int heroCount, HeroInstance firstHero)
        {
            if (_displayText != null)
            {
                var heroText = firstHero != null
                    ? $"Heroes: {heroCount}\nFirst Hero: {firstHero.Definition.DisplayName} (Lvl {firstHero.Level})"
                    : $"Heroes: {heroCount}";
                _displayText.text = heroText;
            }
        }

        public void RefreshDisplay()
        {
            var saveData = _saveLoadService.LoadGame();
            var heroCount = saveData.OwnedHeroIds.Count;
            SetHeroText(heroCount, null);
        }
    }
}