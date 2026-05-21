// Assets/Scripts/UI/SummonButton.cs
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    /// <summary>
    /// UI component attached to a Button that triggers a gacha pull and displays the result.
    /// 
    /// === SETUP INSTRUCTIONS ===
    /// 1. In the Hub scene, create a Button: Right-click Canvas > UI > Button - Legacy
    /// 2. Position the Button where you want the summon control
    /// 3. Attach this SummonButton.cs script to the Button GameObject
    /// 4. (Optional) In the Inspector, drag the HubUI GameObject (with HubView.cs) into the "hubView" field
    ///    - If left unassigned, the script will automatically find HubView via FindObjectOfType
    /// 5. Ensure the Button's OnClick() list is empty; this script adds its listener programmatically
    /// ========================
    /// </summary>
    public class SummonButton : MonoBehaviour
    {
        [Tooltip("Optional reference to HubView for direct UI updates. If null, FindObjectOfType will be used.")]
        [SerializeField] private HubView hubView;

        private Button _button;
        private IGachaService _gachaService;
        private bool _isInitialized = false;

        private void Start()
        {
            InitializeComponents();
            SetupButtonListener();
        }

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
        }

        private void SetupButtonListener()
        {
            if (_button == null) return;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnSummonPressed);
        }

        private void OnSummonPressed()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[SummonButton] Not initialized; cannot perform summon");
                return;
            }

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
        }

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
                foundView.SetHeroText(text);
                return;
            }

            Debug.LogWarning($"[SummonButton] Could not find HubView to display: {text}");
        }

        public void SetHubViewReference(HubView view)
        {
            hubView = view;
        }
    }
}