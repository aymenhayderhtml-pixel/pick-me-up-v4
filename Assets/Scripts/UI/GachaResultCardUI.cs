// Assets/Scripts/UI/GachaResultCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using PickMeUp.Data; // <-- ADD THIS LINE


namespace PickMeUp.UI
{
    public class GachaResultCardUI : MonoBehaviour
    {
        [Header("Visual Components")]
        public Image FrameImage;
        public Image PortraitImage;
        public Image CrestImage;
        public Image BackgroundImage;
        public Image NameBanner;
        public Text NameText;
        public Image[] StarIcons;
        public ParticleSystem RarityParticles;

        private RectTransform _bannerRect;
        private Vector2 _bannerOriginalPos;

        private void Awake()
        {
            _bannerRect = NameBanner.GetComponent<RectTransform>();
            _bannerOriginalPos = _bannerRect.anchoredPosition;
        }

        public void SetupCard(HeroInstance hero, Sprite portrait, Sprite crest)
        {
            NameText.text = $"◄ {hero.HeroDefId.ToUpper()} ►";
            if (portrait != null) PortraitImage.sprite = portrait;
            if (crest != null) CrestImage.sprite = crest;

            // Hide elements for reveal animation
            NameBanner.gameObject.SetActive(false);
            foreach (var star in StarIcons) star.gameObject.SetActive(false);
            if (RarityParticles != null) RarityParticles.gameObject.SetActive(false);
        }

        public void ApplyRarityTheme(int stars)
        {
            Color frameColor = stars switch
            {
                1 => Hex("#607D8B"),
                2 => Hex("#4CAF50"),
                3 => Hex("#2196F3"),
                4 => Hex("#9C27B0"),
                5 => Hex("#FFD700"),
                _ => Color.gray
            };
            FrameImage.color = frameColor;
        }

        // Phase 3: The Reveal
        public IEnumerator PlayRevealAnimation(int stars)
        {
            // 1. Y-Axis Flip (0.3s)
            transform.localScale = new Vector3(-1f, 1f, 1f);
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                float x = Mathf.Lerp(-1f, 1f, t / 0.3f);
                transform.localScale = new Vector3(x, 1f, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;

            // 2. Name Banner Slide Up
            NameBanner.gameObject.SetActive(true);
            _bannerRect.anchoredPosition = _bannerOriginalPos + new Vector2(0, -150);
            t = 0;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                float y = Mathf.Lerp(-150, 0, EaseOutBack(t / 0.25f));
                _bannerRect.anchoredPosition = _bannerOriginalPos + new Vector2(0, y);
                yield return null;
            }
            _bannerRect.anchoredPosition = _bannerOriginalPos;

            // 3. Star Icons Pop In
            for (int i = 0; i < stars && i < StarIcons.Length; i++)
            {
                StarIcons[i].gameObject.SetActive(true);
                StarIcons[i].transform.localScale = Vector3.zero;
                
                float starT = 0;
                while (starT < 0.2f)
                {
                    starT += Time.deltaTime;
                    float s = Mathf.Lerp(0, 1.2f, starT / 0.2f);
                    StarIcons[i].transform.localScale = Vector3.one * s;
                    yield return null;
                }
                StarIcons[i].transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.05f);
            }

            // 4. Particles for 4★/5★
            if (stars >= 4 && RarityParticles != null)
            {
                RarityParticles.gameObject.SetActive(true);
                RarityParticles.Play();
            }
        }

        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        private Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}