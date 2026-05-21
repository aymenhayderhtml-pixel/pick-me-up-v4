// Assets/Scripts/UI/RosterView.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    /// <summary>
    /// Displays the player's hero roster in a scrollable list.
    /// 
    /// === SETUP INSTRUCTIONS ===
    /// 1. In the Hub scene, create a UI Panel to act as the Roster window (name it "RosterPanel").
    /// 2. Inside RosterPanel, create a ScrollView (Right-click > UI > ScrollView).
    /// 3. Inside the ScrollView's Viewport > Content, ensure there is a Vertical Layout Group and Content Size Fitter.
    /// 4. Create a prefab for the hero entry:
    ///    - Create a UI Panel or Image, add a Horizontal Layout Group.
    ///    - Add a Text (Legacy) child for the hero name/stats.
    ///    - Save this GameObject as a Prefab (e.g., "HeroEntryPrefab") and delete it from the scene.
    /// 5. Attach this RosterView.cs script to the "RosterPanel" GameObject.
    /// 6. Assign the "Content" transform from the ScrollView to the 'contentParent' field.
    /// 7. Assign the "HeroEntryPrefab" to the 'heroEntryPrefab' field.
    /// 8. Assign the "RosterPanel" itself to the 'panelRoot' field (for Show/Hide toggling).
    /// ========================
    /// </summary>
    public class RosterView : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI References")]
        [Tooltip("The Content transform inside the ScrollView where entries will be instantiated.")]
        [SerializeField] private Transform contentParent;
        
        [Tooltip("Prefab for a single hero entry. Must contain a Text component for displaying info.")]
        [SerializeField] private GameObject heroEntryPrefab;
        
        [Tooltip("The root panel GameObject to toggle visibility.")]
        [SerializeField] private GameObject panelRoot;

        #endregion

        #region Private Fields

        private IHeroRosterService _rosterService;
        private readonly List<GameObject> _activeEntries = new List<GameObject>();

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (!ServiceRegistry.HasService<IHeroRosterService>())
            {
                Debug.LogError("[RosterView] IHeroRosterService not found in ServiceRegistry.");
                enabled = false;
                return;
            }

            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
            _rosterService.OnHeroAdded += OnHeroAddedHandler;
            _rosterService.OnHeroRemoved += OnHeroRemovedHandler;

            RefreshRoster();
            Hide(); // Start hidden by default
        }

        private void OnDestroy()
        {
            if (_rosterService != null)
            {
                _rosterService.OnHeroAdded -= OnHeroAddedHandler;
                _rosterService.OnHeroRemoved -= OnHeroRemovedHandler;
            }
        }

        #endregion

        #region Event Handlers

        private void OnHeroAddedHandler(HeroInstance hero)
        {
            RefreshRoster();
        }

        private void OnHeroRemovedHandler(string instanceId)
        {
            RefreshRoster();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears and rebuilds the roster UI based on the current service data.
        /// </summary>
        public void RefreshRoster()
        {
            if (_rosterService == null || contentParent == null || heroEntryPrefab == null)
            {
                Debug.LogWarning("[RosterView] Cannot refresh roster: missing service or UI references.");
                return;
            }

            // Clear existing entries
            foreach (GameObject entry in _activeEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            _activeEntries.Clear();

            // Populate new entries
            List<HeroInstance> heroes = _rosterService.GetAllHeroes();
            foreach (HeroInstance hero in heroes)
            {
                GameObject entryObj = Instantiate(heroEntryPrefab, contentParent);
                
                // Find the Text component in the prefab to set the name
                Text entryText = entryObj.GetComponentInChildren<Text>();
                if (entryText != null)
                {
                    entryText.text = $"{hero.HeroDefId} (Lv. {hero.Level})\nHP: {hero.MaxHP} | ATK: {hero.ATK}";
                }
                else
                {
                    Debug.LogWarning("[RosterView] HeroEntryPrefab is missing a Text component in its children.");
                }

                _activeEntries.Add(entryObj);
            }
        }

        /// <summary>
        /// Shows the roster panel.
        /// </summary>
        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                RefreshRoster(); // Ensure data is fresh when opened
            }
        }

        /// <summary>
        /// Hides the roster panel.
        /// </summary>
        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        #endregion
    }
}