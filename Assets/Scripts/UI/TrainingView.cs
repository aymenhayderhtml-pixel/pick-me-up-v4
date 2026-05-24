using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class TrainingView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform HeroListContainer;
        [SerializeField] private GameObject HeroListEntryPrefab;
        [SerializeField] private Text HeroNameText;
        [SerializeField] private Text LevelText;
        [SerializeField] private Text XPText;
        [SerializeField] private Image XPBarImage;
        [SerializeField] private Text StatsText;
        [SerializeField] private Text GoldCostText;
        [SerializeField] private Button LevelUpButton;
        [SerializeField] private Button CloseButton;

        private IHeroProgressionService _progression;
        private IHeroRosterService _roster;
        private HeroInstance _selectedHero;
        private readonly List<GameObject> _listEntries = new List<GameObject>();

        private void Start()
        {
            _progression = ServiceRegistry.Resolve<IHeroProgressionService>();
            _roster = ServiceRegistry.Resolve<IHeroRosterService>();

            if (LevelUpButton != null) LevelUpButton.onClick.AddListener(OnLevelUpClicked);
            if (CloseButton != null) CloseButton.onClick.AddListener(Hide);

            Hide();
        }

        public void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshList();
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void RefreshList()
        {
            foreach (var obj in _listEntries) if (obj != null) Destroy(obj);
            _listEntries.Clear();

            var heroes = _roster.GetAllHeroes();
            foreach (var hero in heroes)
            {
                GameObject entry = Instantiate(HeroListEntryPrefab, HeroListContainer);
                Text entryTxt = entry.GetComponentInChildren<Text>();
                if (entryTxt != null) entryTxt.text = $"{hero.CurrentStar}★ {hero.HeroDefId.Replace("hero_", "")}";
                
                Button btn = entry.GetComponent<Button>();
                btn.onClick.AddListener(() => SelectHero(hero));
                _listEntries.Add(entry);
            }
        }

        private void SelectHero(HeroInstance hero)
        {
            _selectedHero = hero;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_selectedHero == null || HeroNameText == null) return;

            var def = ServiceRegistry.Resolve<IDataService>().GetHeroDefinition(_selectedHero.HeroDefId);
HeroNameText.text = $"{_selectedHero.CurrentStar}★ {(def != null ? def.HeroName : _selectedHero.HeroDefId)}";            LevelText.text = $"Level {_selectedHero.CurrentLevel}";

            int xpNeeded = _selectedHero.CurrentLevel * 100 * _selectedHero.CurrentStar;
            XPText.text = $"{_selectedHero.CurrentXP}/{xpNeeded} XP";
            
            if (XPBarImage != null)
                XPBarImage.fillAmount = Mathf.Clamp01((float)_selectedHero.CurrentXP / xpNeeded);

            if (StatsText != null)
                StatsText.text = $"HP: {_selectedHero.MaxHP} | ATK: {_selectedHero.ATK}\nDEF: {_selectedHero.DEF} | SPD: {_selectedHero.SPD}";

            if (_progression.CanLevelUp(_selectedHero, out int cost))
            {
                GoldCostText.text = $"Cost: {cost} Gold";
                GoldCostText.color = Color.white;
                if (LevelUpButton != null) LevelUpButton.interactable = true;
            }
            else
            {
                int maxLvl = def != null ? _progression.GetMaxLevelForStar(_selectedHero.CurrentStar, def) : 99;
                bool atMax = _selectedHero.CurrentLevel >= maxLvl;
                GoldCostText.text = atMax ? "MAX LEVEL REACHED" : "Insufficient XP or Gold";
                GoldCostText.color = Color.red;
                if (LevelUpButton != null) LevelUpButton.interactable = false;
            }
        }

        private void OnLevelUpClicked()
        {
            if (_selectedHero != null && _progression.LevelUp(_selectedHero))
            {
                UpdateUI();
                RefreshList(); // Update list in case max level reached
            }
        }

        private void OnDestroy()
        {
            if (LevelUpButton != null) LevelUpButton.onClick.RemoveListener(OnLevelUpClicked);
            if (CloseButton != null) CloseButton.onClick.RemoveListener(Hide);
        }
    }
}