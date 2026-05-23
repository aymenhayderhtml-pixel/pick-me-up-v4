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
        public Image BackgroundImage;
        public Image PortraitImage; // NEW: For displaying the hero portrait

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

            // Clean name for display
            string displayName = hero.HeroDefId.Replace("hero_", "").Replace("_", " ");
            if (NameText != null) NameText.text = $"{hero.CurrentStar}★ {displayName}";
            if (StatsText != null) StatsText.text = $"Lv.{hero.CurrentLevel}\nHP:{hero.MaxHP} ATK:{hero.ATK}";

            // LOAD PORTRAIT FROM RESOURCES
            string portraitName = hero.HeroDefId.Replace("hero_", "");
            Sprite portrait = Resources.Load<Sprite>($"Portraits/{portraitName}");
            if (PortraitImage != null && portrait != null)
            {
                PortraitImage.sprite = portrait;
            }

            // COLOR CODE BY STAR RATING
            if (BackgroundImage != null)
            {
                Color bgColor = Color.gray;
                switch (hero.CurrentStar)
                {
                    case 1: bgColor = new Color(0.4f, 0.4f, 0.4f); break;
                    case 2: bgColor = new Color(0.2f, 0.6f, 0.2f); break;
                    case 3: bgColor = new Color(0.2f, 0.4f, 0.8f); break;
                    case 4: bgColor = new Color(0.6f, 0.2f, 0.8f); break;
                    case 5: bgColor = new Color(0.9f, 0.7f, 0.1f); break;
                    default: bgColor = new Color(0.8f, 0.1f, 0.1f); break;
                }
                BackgroundImage.color = bgColor;
            }
        }

        public void OnCardClicked()
        {
            _isSelected = !_isSelected;
            if (SelectionHighlight != null) SelectionHighlight.enabled = _isSelected;
            _onToggleCallback?.Invoke(this);
        }
        
        public void ForceDeselect()
        {
            _isSelected = false;
            if (SelectionHighlight != null) SelectionHighlight.enabled = false;
        }
    }
}