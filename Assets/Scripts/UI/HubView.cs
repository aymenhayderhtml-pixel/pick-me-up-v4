// Assets/Scripts/UI/HubView.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    /// <summary>
    /// Minimal UI view for the Hub scene that displays loaded hero data.
    /// 
    /// === SETUP INSTRUCTIONS ===
    /// 1. Create a new scene: File > New Scene, save as "Hub.unity"
    /// 2. Create a Canvas: Right-click in Hierarchy > UI > Canvas
    /// 3. Create a Text element: Right-click Canvas > UI > Text - Legacy
    /// 4. Position the Text element where you want hero info displayed
    /// 5. Create an empty GameObject named "HubUI" in the scene
    /// 6. Attach this HubView.cs script to the "HubUI" GameObject
    /// 7. Drag the Text component from step 3 into the "displayText" field in the Inspector
    /// 8. Add "Hub.unity" to Build Settings at index 1 (Boot.unity should be index 0)
    /// 9. Ensure BootInstaller is in Boot.unity (scene index 0) as the initial loader
    /// ========================
    /// </summary>
    public class HubView : MonoBehaviour
    {
        [Tooltip("Reference to the UI Text component that displays hero information.")]
        [SerializeField] private Text displayText;

        private IDataService _dataService;
        private readonly List<HeroDefinition> _cachedHeroes = new List<HeroDefinition>();

        private void Start()
        {
            InitializeServices();
            LoadHeroData();
            UpdateUI();
        }

        private void InitializeServices()
        {
            if (!ServiceRegistry.HasService<IDataService>())
            {
                Debug.LogError("[HubView] IDataService not registered in ServiceRegistry");
                return;
            }

            _dataService = ServiceRegistry.Resolve<IDataService>();
        }

        private void LoadHeroData()
        {
            if (_dataService == null)
            {
                Debug.LogWarning("[HubView] Cannot load heroes: IDataService is null");
                return;
            }

            IReadOnlyList<HeroDefinition> heroes = _dataService.GetAllHeroDefinitions();

            if (heroes == null)
            {
                Debug.LogWarning("[HubView] GetAllHeroDefinitions returned null");
                return;
            }

            _cachedHeroes.Clear();
            _cachedHeroes.AddRange(heroes);

            Debug.Log($"[HubView] Loaded {_cachedHeroes.Count} hero definitions");

            if (_cachedHeroes.Count > 0)
            {
                HeroDefinition first = _cachedHeroes[0];
                Debug.Log($"[HubView] First hero: {first.HeroName} (ID: {first.HeroId}, Element: {first.Element}, Class: {first.ClassType})");
            }
        }

        private void UpdateUI()
        {
            if (displayText == null)
            {
                Debug.LogWarning("[HubView] displayText reference is not assigned.");
                return;
            }

            if (_cachedHeroes.Count == 0)
            {
                displayText.text = "No heroes loaded.\nRun Tools > PickMeUp > Create Sample Data";
                return;
            }

            HeroDefinition firstHero = _cachedHeroes[0];
            string heroInfo = $"Heroes Loaded: {_cachedHeroes.Count}\n\n" +
                             $"Featured Hero:\n" +
                             $"Name: {firstHero.HeroName}\n" +
                             $"ID: {firstHero.HeroId}\n" +
                             $"Element: {firstHero.Element}\n" +
                             $"Class: {firstHero.ClassType}\n" +
                             $"HP: {firstHero.BaseHP} | ATK: {firstHero.BaseATK}";

            displayText.text = heroInfo;
        }

        public void SetHeroText(string text)
        {
            if (displayText == null)
            {
                Debug.LogWarning("[HubView] Cannot set text: displayText reference is null");
                return;
            }
            displayText.text = text;
        }

        public void RefreshHeroDisplay()
        {
            LoadHeroData();
            UpdateUI();
        }
    }
}