using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PickMeUp.Core;
using PickMeUp.Data;
using PickMeUp.Services;

namespace PickMeUp.UI
{
    public enum GachaBannerType { Standard, Premium }

    public class SummonView : MonoBehaviour
    {
        [Header("Main UI")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _gemText;

        [Header("Summon Buttons & Pity")]
        [SerializeField] private Button _std1Btn;
        [SerializeField] private Button _std10Btn;
        [SerializeField] private Button _prem1Btn;
        [SerializeField] private Button _prem10Btn;
        [SerializeField] private Button _closeBtn;
        
        [SerializeField] private Text _std1PityText;
        [SerializeField] private Text _std10PityText;
        [SerializeField] private Text _prem1PityText;
        [SerializeField] private Text _prem10PityText;
        [SerializeField] private GameObject _prem10GuaranteeBadge;

        [Header("Shimmer Images")]
        [SerializeField] private Image _std1Shimmer;
        [SerializeField] private Image _std10Shimmer;
        [SerializeField] private Image _prem1Shimmer;
        [SerializeField] private Image _prem10Shimmer;

        [Header("Animation Layer")]
        [SerializeField] private GameObject _animationLayer;
        [SerializeField] private Image _flashImage;
        [SerializeField] private Image _crackImage; 
        [SerializeField] private Transform _resultContainer;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private Image _solidBackground;

        private IGachaService _gachaService;
        private IHeroRosterService _rosterService;
        private ISaveLoadService _saveService;
        private bool _isAnimating = false;

        private void Awake()
        {
            _gachaService = ServiceRegistry.Resolve<IGachaService>();
            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
            _saveService = ServiceRegistry.Resolve<ISaveLoadService>();
            
            // FIX 3.1: Force solid background to stretch 100%
            if (_solidBackground != null)
            {
                RectTransform bgRect = _solidBackground.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
        }

        private void OnEnable()
        {
            _std1Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Standard, 1));
            _std10Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Standard, 10));
            _prem1Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Premium, 1));
            _prem10Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Premium, 10));
            _closeBtn.onClick.AddListener(Hide);

            UpdateUI();
            StartCoroutine(ShimmerLoop());
        }

        private void OnDisable()
        {
            _std1Btn.onClick.RemoveAllListeners();
            _std10Btn.onClick.RemoveAllListeners();
            _prem1Btn.onClick.RemoveAllListeners();
            _prem10Btn.onClick.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
            StopAllCoroutines();
        }

        public void Show()
        {
            // FIX 3.1: Ensure this panel renders above everything
            transform.SetAsLastSibling(); 
            _panelRoot.SetActive(true);
            _animationLayer.SetActive(false);
            UpdateUI();
        }

        public void Hide()
        {
            if (_isAnimating) return;
            _panelRoot.SetActive(false);
        }

        private void UpdateUI()
        {
            var save = _saveService.Load();
            _goldText.text = $"GOLD: {save.Gold:N0}";
            _gemText.text = $"GEMS: {save.Gems:N0}";

            int stdPity = _gachaService.GetPityCount(0);
            int premPity = _gachaService.GetPityCount(1);

            // FIX 1: Safely update pity text
            if (_std1PityText != null) _std1PityText.text = $"PITY: {stdPity} / 10";
            if (_std10PityText != null) _std10PityText.text = $"PITY: {stdPity} / 10";
            if (_prem1PityText != null) _prem1PityText.text = $"PITY: {premPity} / 10";
            if (_prem10PityText != null) _prem10PityText.text = $"PITY: {premPity} / 10";

            bool isGuaranteed = _gachaService.IsPremiumGuaranteed();
            if (_prem10GuaranteeBadge != null) _prem10GuaranteeBadge.SetActive(isGuaranteed);
        }

        private void StartSummon(GachaBannerType type, int count)
        {
            if (_isAnimating) return;
            StartCoroutine(ExecuteSummonSequence(type, count));
        }

        private IEnumerator ExecuteSummonSequence(GachaBannerType type, int count)
        {
            _isAnimating = true;
            SetButtonsInteractable(false);
            ClearResults();

            List<HeroInstance> results = (type == GachaBannerType.Premium) 
                ? _gachaService.PullPremium(count) 
                : _gachaService.PullStandard(count);

            if (results == null || results.Count == 0)
            {
                _isAnimating = false;
                SetButtonsInteractable(true);
                yield break;
            }

            foreach (var hero in results) _rosterService.AddHero(hero);
            _saveService.Save(_saveService.Load());

            _animationLayer.SetActive(true);
            
            // FIX 3.1: Fully opaque background
            _solidBackground.color = new Color(13f/255f, 17f/255f, 23f/255f, 1f); 
            _solidBackground.enabled = true;
            _solidBackground.transform.SetAsLastSibling(); // Ensure it covers the buttons
            
            yield return FadeCanvasGroup(_canvasGroup, 0.2f, 0.3f);

            int highestRarity = results.Max(h => h.CurrentStar);

            yield return StartCoroutine(PlayCrackAnimation(highestRarity));
            yield return StartCoroutine(DropAndRevealCards(results, highestRarity));

            _isAnimating = false;
            SetButtonsInteractable(true);
            UpdateUI();
        }

        #region Phase 1: The Crack
        private IEnumerator PlayCrackAnimation(int highestRarity)
        {
            _crackImage.gameObject.SetActive(true);
            _crackImage.transform.localScale = Vector3.zero;
            
            Color crackColor = highestRarity switch
            {
                5 => Hex("#FFD700"),
                4 => Hex("#9C27B0"),
                3 => Hex("#2196F3"),
                _ => Color.white
            };
            _crackImage.color = crackColor;

            float duration = highestRarity >= 5 ? 2.5f : (highestRarity == 4 ? 1.5f : 0.8f);
            float t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float scale = Mathf.Lerp(0, 1.5f, t / duration);
                _crackImage.transform.localScale = Vector3.one * scale;
                
                float pulse = Mathf.PingPong(t * 4f, 1f);
                _crackImage.color = Color.Lerp(crackColor, Color.white, pulse * 0.5f);
                yield return null;
            }
        }
        #endregion

        #region Phase 2 & 3: Drop and Reveal
        private IEnumerator DropAndRevealCards(List<HeroInstance> results, int highestRarity)
        {
            bool isSingle = results.Count == 1;
            var grid = _resultContainer.GetComponent<GridLayoutGroup>();
            var containerRect = _resultContainer.GetComponent<RectTransform>();

            if (isSingle)
            {
                grid.cellSize = new Vector2(800, 1200);
                grid.constraintCount = 1;
                grid.spacing = Vector2.zero;
                grid.padding = new RectOffset(0,0,0,0);
                containerRect.anchorMin = new Vector2(0.1f, 0.1f);
                containerRect.anchorMax = new Vector2(0.9f, 0.9f);
            }
            else
            {
                // FIX 3.2: 10-pull layout adjustments
                grid.cellSize = new Vector2(180, 280); // Taller cells
                grid.spacing = new Vector2(10, 10);
                grid.constraintCount = 5;
                grid.padding = new RectOffset(20, 20, 0, 0); // Left/Right padding
                
                containerRect.anchorMin = new Vector2(0f, 0.1f); // Use most of screen height
                containerRect.anchorMax = new Vector2(1f, 0.9f);
            }
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.childAlignment = TextAnchor.MiddleCenter;
            containerRect.sizeDelta = Vector2.zero;

            // Fade out crack and solid bg
            float fadeT = 0;
            while (fadeT < 0.3f)
            {
                fadeT += Time.deltaTime;
                float a = 1f - (fadeT / 0.3f);
                _crackImage.color = new Color(_crackImage.color.r, _crackImage.color.g, _crackImage.color.b, a);
                _solidBackground.color = new Color(_solidBackground.color.r, _solidBackground.color.g, _solidBackground.color.b, a);
                yield return null;
            }
            _crackImage.gameObject.SetActive(false);
            _solidBackground.enabled = false;

            // Drop Cards
            for (int i = 0; i < results.Count; i++)
            {
                var hero = results[i];
                GameObject cardObj = Instantiate(_cardPrefab, _resultContainer);
                GachaResultCardUI cardUI = cardObj.GetComponent<GachaResultCardUI>();
                
                // Load art
                HeroDefinition def = Resources.Load<HeroDefinition>($"Heroes/{hero.HeroDefId}");
                Sprite portrait = def != null ? def.Portrait : null;
                Sprite crest = def != null ? def.Crest : null;
                
                cardUI.SetupCard(hero, portrait, crest);
                cardUI.ApplyRarityTheme(hero.CurrentStar);

                RectTransform cardRect = cardObj.GetComponent<RectTransform>();
                // FIX 3.2: Ensure no explicit sizeDelta overrides the grid cell size
                cardRect.sizeDelta = Vector2.zero; 
                
                Vector3 targetPos = cardRect.localPosition;
                
                // Start above screen
                cardRect.localPosition = targetPos + new Vector3(0, 1500, 0);
                cardRect.localEulerAngles = new Vector3(0, 0, Random.Range(-15f, 15f));

                if (hero.CurrentStar >= 5 && isSingle)
                {
                    yield return new WaitForSeconds(1.0f); 
                }

                float dropTime = (hero.CurrentStar >= 5 && isSingle) ? 0.8f : 0.3f;
                float t = 0;
                while (t < dropTime)
                {
                    t += Time.deltaTime;
                    float progress = t / dropTime;
                    cardRect.localPosition = Vector3.Lerp(targetPos + new Vector3(0, 1500, 0), targetPos, progress);
                    cardRect.localEulerAngles = Vector3.Lerp(cardRect.localEulerAngles, Vector3.zero, progress);
                    yield return null;
                }
                cardRect.localPosition = targetPos;
                cardRect.localEulerAngles = Vector3.zero;

                if (hero.CurrentStar >= 4 || isSingle)
                {
                    yield return ShakeScreen(0.2f, hero.CurrentStar >= 5 ? 20f : 8f);
                }

                if (hero.CurrentStar >= 5)
                {
                    yield return Flash(Color.yellow, 0.4f);
                }

                yield return StartCoroutine(cardUI.PlayRevealAnimation(hero.CurrentStar));

                if (!isSingle) yield return new WaitForSeconds(0.05f); 
            }

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);
            ClearResults();
            _animationLayer.SetActive(false);
            yield return FadeCanvasGroup(_canvasGroup, 1f, 0.3f);
        }
        #endregion

        #region Shimmer & Animation Helpers
        private IEnumerator ShimmerLoop()
        {
            Image[] shimmers = { _std1Shimmer, _std10Shimmer, _prem1Shimmer, _prem10Shimmer };
            while (true)
            {
                foreach (var shimmer in shimmers)
                {
                    if (shimmer != null)
                    {
                        RectTransform rect = shimmer.GetComponent<RectTransform>();
                        float t = 0;
                        while (t < 0.8f)
                        {
                            t += Time.deltaTime;
                            rect.anchorMin = new Vector2(Mathf.Lerp(-0.5f, 1.5f, t / 0.8f), 0);
                            rect.anchorMax = new Vector2(Mathf.Lerp(0f, 2f, t / 0.8f), 1);
                            yield return null;
                        }
                    }
                }
                yield return new WaitForSeconds(3f);
            }
        }

        private IEnumerator Flash(Color color, float duration)
        {
            _flashImage.color = color;
            _flashImage.gameObject.SetActive(true);
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                Color c = color;
                c.a = 1 - (t / duration);
                _flashImage.color = c;
                yield return null;
            }
            _flashImage.gameObject.SetActive(false);
        }

        private IEnumerator ShakeScreen(float duration, float magnitude)
        {
            RectTransform rect = _panelRoot.GetComponent<RectTransform>();
            Vector2 originalPos = rect.anchoredPosition;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * magnitude * (1 - t / duration);
                float y = Random.Range(-1f, 1f) * magnitude * (1 - t / duration);
                rect.anchoredPosition = originalPos + new Vector2(x, y);
                yield return null;
            }
            rect.anchoredPosition = originalPos;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
        {
            float startAlpha = cg.alpha;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }
        #endregion

        private void SetButtonsInteractable(bool state)
        {
            _std1Btn.interactable = state;
            _std10Btn.interactable = state;
            _prem1Btn.interactable = state;
            _prem10Btn.interactable = state;
            _closeBtn.interactable = state;
        }

        private void ClearResults()
        {
            foreach (Transform child in _resultContainer)
                Destroy(child.gameObject);
        }

        private Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}