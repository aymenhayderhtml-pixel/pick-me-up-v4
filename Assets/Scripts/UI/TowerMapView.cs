using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class TowerMapView : MonoBehaviour
    {
        [Header("Header UI")]
        [SerializeField] private Text _floorText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Button _retreatButton;

        [Header("Node Map UI")]
        [SerializeField] private ScrollRect _nodeScrollRect;
        [SerializeField] private Transform _nodeContainer; // Should have VerticalLayoutGroup
        [SerializeField] private GameObject _nodeButtonPrefab; // A simple Button with a Text child

        [Header("Action & Log UI")]
        [SerializeField] private Button _startRunButton;
        [SerializeField] private Text _combatLogText;
        [SerializeField] private ScrollRect _logScrollRect;
        [SerializeField] private GameObject _logPanel;

        private ITowerService _towerService;
        private IHeroRosterService _rosterService;
        private TowerRunState _activeRun;

        private void Awake()
        {
            _towerService = ServiceRegistry.Resolve<ITowerService>();
            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
        }

        private void OnEnable()
        {
            _startRunButton.onClick.AddListener(StartNewRun);
            _retreatButton.onClick.AddListener(Retreat);
            
            // Check if we are resuming an existing run
            _activeRun = _towerService.GetActiveRun();
            RefreshUI();
        }

        private void OnDisable()
        {
            _startRunButton.onClick.RemoveListener(StartNewRun);
            _retreatButton.onClick.RemoveListener(Retreat);
        }

        private void StartNewRun()
        {
            // Grab the first 4 heroes from the roster for the tower party
            var allHeroes = _rosterService.GetAllHeroes();
            var party = allHeroes.Take(4).ToList();

            if (party.Count == 0)
            {
                Debug.LogWarning("[TowerMapView] No heroes in roster to start a run!");
                return;
            }

            var save = ServiceRegistry.Resolve<ISaveLoadService>().Load();
            int startingFloor = Mathf.Max(1, save.HighestFloorCleared + 1);

            _activeRun = _towerService.StartNewRun(party, startingFloor);
            ClearCombatLog();
            RefreshUI();
        }

        private void Retreat()
        {
            if (_activeRun != null)
            {
                _towerService.EndRun(_activeRun);
                _activeRun = null;
            }
            RefreshUI();
        }

        private void RefreshUI()
        {
            // Clear existing node buttons
            foreach (Transform child in _nodeContainer)
                Destroy(child.gameObject);

            if (_activeRun == null || !_activeRun.IsRunActive)
            {
                // Show Start Screen
                _floorText.text = "Tower of Trials";
                _goldText.text = "Gold: 0";
                _startRunButton.gameObject.SetActive(true);
                _retreatButton.gameObject.SetActive(false);
                _nodeScrollRect.gameObject.SetActive(false);
                _logPanel.SetActive(false);
            }
            else
            {
                // Show Active Run
                _floorText.text = $"Floor {_activeRun.CurrentFloor}";
                _goldText.text = $"Gold: {_activeRun.TotalGoldEarned}";
                _startRunButton.gameObject.SetActive(false);
                _retreatButton.gameObject.SetActive(true);
                _nodeScrollRect.gameObject.SetActive(true);
                _logPanel.SetActive(true);

                GenerateNodeButtons();
            }
        }

        private void GenerateNodeButtons()
        {
            if (_activeRun?.CurrentFloorData == null) return;

            // Reverse the list so Node 0 is at the bottom of the scroll view (standard mobile UX)
            var nodes = _activeRun.CurrentFloorData.Nodes.AsEnumerable().Reverse();

            foreach (var node in nodes)
            {
                // If you don't have a prefab assigned, create a basic one via code
                GameObject btnObj = _nodeButtonPrefab != null 
                    ? Instantiate(_nodeButtonPrefab, _nodeContainer) 
                    : CreateDefaultNodeButton();

                Text btnText = btnObj.GetComponentInChildren<Text>();
                Button btn = btnObj.GetComponent<Button>();

                string icon = node.Type switch
                {
                    TowerNodeType.Combat => "⚔️",
                    TowerNodeType.Elite => "💀",
                    TowerNodeType.Boss => "👑",
                    TowerNodeType.Rest => "🏕️",
                    TowerNodeType.Treasure => "💰",
                    _ => "❓"
                };

                btnText.text = $"{icon} {node.Type} (Floor {node.FloorLevel})";
                
                // Visual state for cleared nodes
                if (node.IsCleared)
                {
                    btnText.color = Color.gray;
                    btn.interactable = false;
                    btnText.text += " [CLEARED]";
                }
                else
                {
                    btnText.color = Color.white;
                    btn.interactable = true;
                }

                // Capture variable for lambda
                var capturedNode = node; 
                btn.onClick.AddListener(() => OnNodeClicked(capturedNode));
            }
        }

        private void OnNodeClicked(TowerNode node)
        {
            if (_activeRun == null || node.IsCleared) return;

            ClearCombatLog();
            AppendToLog($"--- Engaging {node.Type} Node ---");

            // Resolve the node (Combat, Rest, or Treasure)
            CombatSnapshot result = _towerService.ResolveNode(_activeRun, node);

            if (result != null)
            {
                // Print the turn-by-turn log
                foreach (var evt in result.EventLog)
                {
                    AppendToLog(evt.Description);
                }

                if (result.IsVictory)
                {
                    AppendToLog("\n🎉 VICTORY! Rewards collected.");
                    _towerService.CompleteNode(_activeRun, node);
                }
                else
                {
                    AppendToLog("\n💀 DEFEAT! The run has ended.");
                    _towerService.EndRun(_activeRun);
                    _activeRun = null;
                }
            }

            // Scroll log to bottom
            Canvas.ForceUpdateCanvases();
            _logScrollRect.verticalNormalizedPosition = 0f;

            RefreshUI();
        }

        private void AppendToLog(string message)
        {
            _combatLogText.text += message + "\n";
        }

        private void ClearCombatLog()
        {
            _combatLogText.text = "";
        }

        // Fallback if no prefab is assigned in the inspector
        private GameObject CreateDefaultNodeButton()
        {
            GameObject btnObj = new GameObject("NodeButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(_nodeContainer, false);
            
            var rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 120); // Height 120, width controlled by LayoutGroup

            var img = btnObj.GetComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(btnObj.transform, false);
            
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textObj.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 40;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            text.resizeTextMaxSize = 40;

            return btnObj;
        }
    }
}