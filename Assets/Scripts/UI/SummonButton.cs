using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Services;
using PickMeUp.UI;

namespace PickMeUp.UI
{
    public class SummonButton : MonoBehaviour
    {
        private Button _button;
        private IGachaService _gachaService;
        private ISaveLoadService _saveLoadService;
        private HubView _hubView;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _gachaService = ServiceRegistry.Resolve<IGachaService>();
            _saveLoadService = ServiceRegistry.Resolve<ISaveLoadService>();
            _hubView = FindObjectOfType<HubView>();

            _button.onClick.AddListener(OnSummonButtonClicked);
        }

        private void OnSummonButtonClicked()
        {
            var saveData = _saveLoadService.LoadGame();
            var result = _gachaService.Pull(saveData);

            if (result?.PulledHero != null)
            {
                saveData.OwnedHeroIds.Add(result.PulledHero.HeroInstanceId);
                _saveLoadService.SaveGame(saveData);
                _hubView.RefreshDisplay();

                Debug.Log($"Summoned: {result.PulledHero.Definition.DisplayName}");
            }
        }
    }
}