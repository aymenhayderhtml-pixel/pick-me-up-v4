using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class HubView : MonoBehaviour
    {
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
            if (!ServiceRegistry.HasService<IDataService>()) return;
            _dataService = ServiceRegistry.Resolve<IDataService>();
        }

        private void LoadHeroData()
        {
            if (_dataService == null) return;

            IReadOnlyList<HeroDefinition> heroes = _dataService.GetAllHeroDefinitions();
            if (heroes == null) return;

            _cachedHeroes.Clear();
            _cachedHeroes.AddRange(heroes);
        }

        private void UpdateUI()
        {
            if (displayText == null) return;

            // FIX: Get the actual owned hero count from the Roster Service
            int ownedHeroCount = 0;
            if (ServiceRegistry.HasService<IHeroRosterService>())
            {
                ownedHeroCount = ServiceRegistry.Resolve<IHeroRosterService>().GetHeroCount();
            }

            if (_cachedHeroes.Count == 0)
            {
                displayText.text = "No hero definitions loaded.\nRun Tools > PickMeUp > Create Sample Data";
                return;
            }

            HeroDefinition firstHero = _cachedHeroes[0];
            
            // FIX: Removed the overwrite that was forcing it to say 1
            string heroInfo = $"Total Heroes Owned: {ownedHeroCount}\n" +
                              $"(Templates Loaded: {_cachedHeroes.Count})\n\n" +
                              $"Featured Template:\n" +
                              $"Name: {firstHero.HeroName}\n" +
                              $"Element: {firstHero.Element} | Class: {firstHero.ClassType}\n" +
                              $"Base HP: {firstHero.BaseHP} | Base ATK: {firstHero.BaseATK}";
            
            displayText.text = heroInfo;
        }

        public void SetHeroText(string text)
        {
            if (displayText != null) displayText.text = text;
        }

        public void RefreshHeroDisplay()
        {
            LoadHeroData();
            UpdateUI();
        }
    }
}