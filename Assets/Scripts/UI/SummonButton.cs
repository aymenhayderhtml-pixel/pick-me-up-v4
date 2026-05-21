// Assets/Scripts/UI/SummonButton.cs
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;
using PickMeUp.UI;

namespace PickMeUp.UI
{
    /// <summary>
    /// UI component attached to trigger a gacha pull and display the result.
    /// </summary>
    public class SummonButton : MonoBehaviour
    {
        #region Inspector Fields

        [Tooltip("Optional reference to HubView for direct UI updates. If null, FindObjectOfType will be used.")]
        [SerializeField] private HubView hubView;

        #endregion

        #region Private Fields

        private Button _button;
        private IGachaService _gachaService;
        private bool _isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            InitializeComponents();
            SetupButtonListener();
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("[SummonButton] Button component not found on this GameObject");
                enabled = false;
                return;
            }

            if (!ServiceRegistry.HasService<IGachaService>())
            {
                Debug.LogError("[SummonButton] IGachaService not registered in ServiceRegistry");
                enabled = false;
                return;
            }

            _gachaService = ServiceRegistry.Resolve<IGachaService>();
            _isInitialized = true;
            
            if (hubView == null)
            {
                hubView = FindObjectOfType<HubView>();
            }
        }

        private void SetupButtonListener()
        {
            if (_button == null) return;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnSummonPressed);
        }

        #endregion

        #region Button Callback

        private void OnSummonPressed()
        {
            if (!_isInitialized) return;

            HeroInstance pulledHero = _gachaService.Pull(0);
            
            if (pulledHero == null)
            {
                string errorMsg = "Summon Failed: No hero received";
                Debug.LogError($"[SummonButton] {errorMsg}");
                UpdateDisplayText(errorMsg);
                return;
            }

            Debug.Log($"[SummonButton] Pulled hero: {pulledHero.HeroDefId} (Instance: {pulledHero.InstanceId})");
            
            string resultText = $"★ New Hero Acquired! ★\n\n" +
                               $"Name: {pulledHero.HeroDefId}\n" +
                               $"Level: {pulledHero.Level}\n" +
                               $"HP: {pulledHero.MaxHP} | ATK: {pulledHero.ATK}\n" +
                               $"Morale: {pulledHero.Morale / 100}%";
            
            UpdateDisplayText(resultText);
            
            // Refresh the main hub view to update the total hero count
            if (hubView != null)
            {
                hubView.RefreshHeroDisplay();
            }
        }

        #endregion

        #region UI Update Helpers

        private void UpdateDisplayText(string text)
        {
            if (hubView != null)
            {
                hubView.SetHeroText(text);
                return;
            }
            
            HubView foundView = FindObjectOfType<HubView>();
            if (foundView != null)
            {
                hubView = foundView; // Cache for next time
                foundView.SetHeroText(text);
                return;
            }
            
            Debug.LogWarning($"[SummonButton] Could not find HubView to display: {text}");
        }

        #endregion
    }
}