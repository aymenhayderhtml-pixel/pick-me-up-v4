using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class RosterView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject heroEntryPrefab; // Must have HeroCardUI script
        [SerializeField] private GameObject panelRoot;

        private IHeroRosterService _rosterService;
        private readonly List<GameObject> _activeEntries = new List<GameObject>();
        private readonly List<HeroCardUI> _selectedCards = new List<HeroCardUI>();

        private void Start()
        {
            if (!ServiceRegistry.HasService<IHeroRosterService>()) return;

            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
            _rosterService.OnHeroAdded += OnHeroAddedHandler;
            _rosterService.OnHeroRemoved += OnHeroRemovedHandler;

            RefreshRoster();
            Hide(); 
        }

        private void OnDestroy()
        {
            if (_rosterService != null)
            {
                _rosterService.OnHeroAdded -= OnHeroAddedHandler;
                _rosterService.OnHeroRemoved -= OnHeroRemovedHandler;
            }
        }

        private void OnHeroAddedHandler(HeroInstance hero) => RefreshRoster();
        private void OnHeroRemovedHandler(string instanceId) => RefreshRoster();

        public void RefreshRoster()
        {
            if (_rosterService == null || contentParent == null || heroEntryPrefab == null) return;

            // Clear existing
            foreach (GameObject entry in _activeEntries) if (entry != null) Destroy(entry);
            _activeEntries.Clear();
            _selectedCards.Clear();

            // Populate
            List<HeroInstance> heroes = _rosterService.GetAllHeroes();
            foreach (HeroInstance hero in heroes)
            {
                GameObject entryObj = Instantiate(heroEntryPrefab, contentParent);
                HeroCardUI card = entryObj.GetComponent<HeroCardUI>();
                
                if (card != null)
                {
                    card.Setup(hero, OnCardToggled);
                }
                else
                {
                    Debug.LogError("[RosterView] Prefab is missing HeroCardUI script!");
                }
                _activeEntries.Add(entryObj);
            }
        }

        private void OnCardToggled(HeroCardUI card)
        {
            if (card.IsSelected)
            {
                if (_selectedCards.Count < 3)
                {
                    _selectedCards.Add(card);
                }
                else
                {
                    // Max 3 selected for synthesis
                    card.ForceDeselect();
                }
            }
            else
            {
                _selectedCards.Remove(card);
            }
        }

        public List<HeroInstance> GetSelectedHeroes()
        {
            return _selectedCards.Select(c => c.Hero).ToList();
        }

        public void ClearSelection()
        {
            foreach (var card in _selectedCards) card.ForceDeselect();
            _selectedCards.Clear();
        }

        public void Show() { if (panelRoot != null) { panelRoot.SetActive(true); RefreshRoster(); } }
        public void Hide() { if (panelRoot != null) panelRoot.SetActive(false); }
    }
}