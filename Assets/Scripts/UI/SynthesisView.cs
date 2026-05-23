using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public class SynthesisView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text StatusText;
        [SerializeField] private Button SynthesizeButton;
        
        [Header("External References")]
        [SerializeField] private RosterView rosterView;
        [SerializeField] private HubView hubView;

        private ISynthesisService _synthesisService;

        private void Start()
        {
            _synthesisService = ServiceRegistry.Resolve<ISynthesisService>();
            
            if (SynthesizeButton != null) SynthesizeButton.onClick.AddListener(OnSynthesizeClicked);
            if (rosterView == null) rosterView = FindAnyObjectByType<RosterView>();
            if (hubView == null) hubView = FindAnyObjectByType<HubView>();

            Hide();
        }

        private void OnDestroy()
        {
            if (SynthesizeButton != null) SynthesizeButton.onClick.RemoveListener(OnSynthesizeClicked);
        }

                public void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            
            if (rosterView != null) 
            {
                rosterView.gameObject.SetActive(true); 
                
                // CRITICAL FIX: Brings Roster to the absolute front of the Canvas hierarchy
                // so the Synthesis panel cannot block its clicks.
                rosterView.transform.SetAsLastSibling(); 
                
                rosterView.Show(); 
            }
            
            UpdateStatusText("Select 2 (60% chance) or 3 (100% chance) heroes of the same star level.");
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (rosterView != null) rosterView.gameObject.SetActive(false);
        }

        private void OnSynthesizeClicked()
        {
            if (rosterView == null) return;

            List<HeroInstance> selected = rosterView.GetSelectedHeroes();
            
            // DEBUG: Prove how many heroes were actually passed
            Debug.Log($"<color=yellow>[SynthesisView] Attempting synthesis with {selected.Count} heroes.</color>");

            if (!_synthesisService.CanSynthesize(selected, out string error))
            {
                UpdateStatusText($"<color=red>ERROR: {error}</color>");
                return;
            }

            _synthesisService.OnSynthesisSuccess += OnSuccess;
            _synthesisService.OnSynthesisFailure += OnFailure;

            HeroInstance result = _synthesisService.Synthesize(selected, null);

            _synthesisService.OnSynthesisSuccess -= OnSuccess;
            _synthesisService.OnSynthesisFailure -= OnFailure;

            rosterView.ClearSelection();
        }

        private void OnSuccess(HeroInstance promotedHero)
        {
            UpdateStatusText($"<color=green>SUCCESS!</color> {promotedHero.HeroDefId} promoted to {promotedHero.CurrentStar}★!");
            if (hubView != null) hubView.SetHeroText($"★ SYNTHESIS SUCCESS ★\n{promotedHero.HeroDefId} is now {promotedHero.CurrentStar}★!");
        }

        private void OnFailure()
        {
            UpdateStatusText("<color=red>FAILURE!</color> Fodder burned. You received consolation trash.");
            if (hubView != null) hubView.SetHeroText("✖ SYNTHESIS FAILED ✖\nYour heroes were consumed by the Abyss...");
        }

        private void UpdateStatusText(string msg)
        {
            if (StatusText != null) StatusText.text = msg;
        }
    }
}