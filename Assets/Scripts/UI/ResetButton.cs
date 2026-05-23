using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PickMeUp.Core;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class ResetButton : MonoBehaviour
    {
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
        }

        private void OnClicked()
        {
            Debug.LogWarning("[ResetButton] Wiping save data and restarting game...");
            
            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                ServiceRegistry.Resolve<ISaveLoadService>().DeleteSave();
            }
            
            // Clear the registry to force a fresh boot
            ServiceRegistry.Clear();
            
            // Reload the Boot scene to start fresh
            SceneManager.LoadScene("Boot");
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnClicked);
        }
    }
}