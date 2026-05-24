// Assets/Scripts/UI/TowerButton.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    /// <summary>
    /// UI component to initiate a tower run and display initial floor info.
    /// 
    /// === SETUP INSTRUCTIONS ===
    /// 1. In the Hub scene, create a UI Button (Right-click Canvas > UI > Button - Legacy).
    /// 2. Name it "TowerButton" and position it appropriately.
    /// 3. Attach this TowerButton.cs script to the Button GameObject.
    /// 4. (Optional) Drag the "HubUI" GameObject (with HubView.cs) into the 'hubView' field.
    /// ========================
    /// </summary>
    public class TowerButton : MonoBehaviour
    {
        #region Inspector Fields

        [Tooltip("Optional reference to HubView for direct UI updates.")]
        [SerializeField] private HubView hubView;

        #endregion

        #region Private Fields

        private Button _button;
        private IHeroRosterService _rosterService;
        private ITowerService _towerService;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("[TowerButton] Missing Button component.");
                enabled = false;
                return;
            }

            if (!ServiceRegistry.HasService<IHeroRosterService>() || !ServiceRegistry.HasService<ITowerService>())
            {
                Debug.LogError("[TowerButton] Required services not registered.");
                enabled = false;
                return;
            }

            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
            _towerService = ServiceRegistry.Resolve<ITowerService>();

            if (hubView == null) hubView = FindAnyObjectByType<HubView>();

            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnClicked);
        }

        #endregion

        #region Event Handlers

        private void OnClicked()
        {
            List<HeroInstance> roster = _rosterService.GetAllHeroes();
            if (roster.Count == 0)
            {
                Debug.LogWarning("[TowerButton] Cannot start run: Roster is empty.");
                return;
            }

            // Take up to 4 heroes for the party
            List<HeroInstance> party = roster.Take(4).ToList();
            
            TowerRunState run = _towerService.StartNewRun(party, 1);
            
            string floorInfo = $"--- TOWER RUN STARTED ---\n" +
                               $"Floor: {run.CurrentFloorData.FloorLevel}\n" +
                               $"Nodes: {string.Join(", ", run.CurrentFloorData.Nodes.Select(n => n.Type.ToString()))}\n\n" +
                               $"First Node: {run.CurrentFloorData.Nodes[0].Type}\n" +
                               $"(Check console for combat simulation)";

            if (hubView != null)
            {
                hubView.SetHeroText(floorInfo);
            }

            // Auto-resolve first node for MVP demonstration
            TowerNode firstNode = run.CurrentFloorData.Nodes[0];
            CombatSnapshot result = _towerService.ResolveNode(run, firstNode);
            
            if (result != null)
            {
                Debug.Log($"[TowerButton] First node resolved. Victory: {result.IsVictory}");
                if (result.IsVictory)
                {
                    _towerService.CompleteNode(run, firstNode);
                }
                else
                {
                    _towerService.EndRun(run);
                }
            }
        }

        #endregion
    }
}