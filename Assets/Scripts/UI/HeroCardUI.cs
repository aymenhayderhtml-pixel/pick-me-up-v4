using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Data;

namespace PickMeUp.UI
{
    public class HeroCardUI : MonoBehaviour
    {
        [Header("UI References")]
        public Text NameText;
        public Text StatsText;
        public Image SelectionHighlight; 

        private HeroInstance _hero;
        private bool _isSelected;
        private System.Action<HeroCardUI> _onToggleCallback;

        public HeroInstance Hero => _hero;
        public bool IsSelected => _isSelected;

        public void Setup(HeroInstance hero, System.Action<HeroCardUI> onToggle)
        {
            _hero = hero;
            _onToggleCallback = onToggle;
            _isSelected = false;
            
            if (SelectionHighlight != null) SelectionHighlight.enabled = false;

            if (NameText != null)
                NameText.text = $"{hero.CurrentStar}★ {hero.HeroDefId}";
                
            if (StatsText != null)
                StatsText.text = $"Lv.{hero.CurrentLevel}\nHP:{hero.MaxHP} ATK:{hero.ATK}";
        }

        public void OnCardClicked()
        {
            _isSelected = !_isSelected;
            
            // DEBUG: Prove the click registered
            Debug.Log($"<color=cyan>[HeroCardUI] Clicked card: {_hero.HeroDefId}. Selected: {_isSelected}</color>");
            
            if (SelectionHighlight != null) 
                SelectionHighlight.enabled = _isSelected;
            else
                Debug.LogWarning("[HeroCardUI] SelectionHighlight is null! Cannot show yellow border.");
                
            _onToggleCallback?.Invoke(this);
        }
        
        public void ForceDeselect()
        {
            _isSelected = false;
            if (SelectionHighlight != null) SelectionHighlight.enabled = false;
        }
    }
}