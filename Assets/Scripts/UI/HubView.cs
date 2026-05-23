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

            int ownedHeroCount = 0;
            int highestFloor = 0;
            int gold = 0;

            if (ServiceRegistry.HasService<IHeroRosterService>())
                ownedHeroCount = ServiceRegistry.Resolve<IHeroRosterService>().GetHeroCount();

            if (ServiceRegistry.HasService<ISaveLoadService>())
            {
                var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
                highestFloor = save.HighestFloorCleared;
                gold = save.Gold;
            }

            string heroInfo = $"=== TOWNIA HUB ===\n" +
                              $"Heroes Owned: {ownedHeroCount}\n" +
                              $"Highest Floor: {highestFloor}\n" +
                              $"Gold: {gold}\n\n";

            if (_cachedHeroes.Count > 0)
            {
                HeroDefinition firstHero = _cachedHeroes[0];
                heroInfo += $"Featured Template:\n" +
                            $"{firstHero.HeroName} ({firstHero.Element} {firstHero.ClassType})\n" +
                            $"Base HP: {firstHero.BaseHP} | ATK: {firstHero.BaseATK}";
            }
            
            displayText.text = heroInfo;
        }

        public void SetHeroText(string text)
        {
            if (displayText != null) displayText.text = text;
        }
        public void ShowSummonResult(HeroInstance hero)
        {
            if (displayText == null || hero == null) return;

            // Fetch the actual name from the database
            string heroName = hero.HeroDefId;
            string element = "None";
            string cls = "None";

            if (_dataService != null)
            {
                var def = _dataService.GetHeroDefinition(hero.HeroDefId);
                if (def != null)
                {
                    heroName = def.HeroName;
                    element = def.Element.ToString();
                    cls = def.ClassType.ToString();
                }
            }

            int ownedCount = ServiceRegistry.HasService<IHeroRosterService>() 
                ? ServiceRegistry.Resolve<IHeroRosterService>().GetHeroCount() 
                : 0;

            string resultText = $"★ NEW HERO ACQUIRED ★\n\n" +
                                $"{hero.CurrentStar}★ {heroName}\n" +
                                $"Element: {element} | Class: {cls}\n" +
                                $"HP: {hero.MaxHP} | ATK: {hero.ATK}\n\n" +
                                $"Total Heroes Owned: {ownedCount}";

            displayText.text = resultText;
        }
        public void RefreshHeroDisplay()
        {
            LoadHeroData();
            UpdateUI();
        }
    }
}