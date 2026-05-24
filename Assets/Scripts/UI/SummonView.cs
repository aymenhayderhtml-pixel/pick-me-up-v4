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

        [Header("4 Summon Cards")]
        [SerializeField] private Button _std1Btn;
        [SerializeField] private Button _std10Btn;
        [SerializeField] private Button _prem1Btn;
        [SerializeField] private Button _prem10Btn;
        [SerializeField] private Button _closeBtn;

        [Header("Animation & Reveal Layer")]
        [SerializeField] private GameObject _animationLayer; 
        [SerializeField] private Image _flashImage; 
        [SerializeField] private Transform _resultContainer; 
        [SerializeField] private GameObject _cardPrefab; 

        private IGachaService _gachaService;
        private IHeroRosterService _rosterService;
        private ISaveLoadService _saveService;
        
        private bool _isAnimating = false;

        private void Awake()
        {
            _gachaService = ServiceRegistry.Resolve<IGachaService>();
            _rosterService = ServiceRegistry.Resolve<IHeroRosterService>();
            _saveService = ServiceRegistry.Resolve<ISaveLoadService>();
        }

        private void OnEnable()
        {
            // Wire up the 4 distinct cards
            _std1Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Standard, 1));
            _std10Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Standard, 10));
            _prem1Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Premium, 1));
            _prem10Btn.onClick.AddListener(() => StartSummon(GachaBannerType.Premium, 10));
            _closeBtn.onClick.AddListener(Hide);
            
            UpdateCurrencyText();
        }

        private void OnDisable()
        {
            _std1Btn.onClick.RemoveAllListeners();
            _std10Btn.onClick.RemoveAllListeners();
            _prem1Btn.onClick.RemoveAllListeners();
            _prem10Btn.onClick.RemoveAllListeners();
            _closeBtn.onClick.RemoveAllListeners();
        }

        public void Show()
        {
            transform.SetAsLastSibling();
            _panelRoot.SetActive(true);
            _animationLayer.SetActive(false);
            UpdateCurrencyText();
        }

        public void Hide()
        {
            if (_isAnimating) return; 
            _panelRoot.SetActive(false);
        }

        private void UpdateCurrencyText()
        {
            var save = _saveService.Load();
            _goldText.text = $"GOLD: {save.Gold:N0}";
            _gemText.text = $"GEMS: {save.Gems:N0}";
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

            List<HeroInstance> results = new List<HeroInstance>();
            if (type == GachaBannerType.Premium)
                results = _gachaService.PullPremium(count);
            else
                results = _gachaService.PullStandard(count);

            if (results == null || results.Count == 0)
            {
                _isAnimating = false;
                SetButtonsInteractable(true);
                yield break;
            }

            foreach (var hero in results) _rosterService.AddHero(hero);
            _saveService.Save(_saveService.Load()); 

            _animationLayer.SetActive(true);
            yield return StartCoroutine(PlayRevealAnimation(results));
            yield return StartCoroutine(ShowResultCards(results));

            _isAnimating = false;
            SetButtonsInteractable(true);
            UpdateCurrencyText();
        }

        #region Cinematic Animations

        private IEnumerator PlayRevealAnimation(List<HeroInstance> results)
        {
            int highestRarity = results.Max(h => h.CurrentStar);

            _flashImage.color = Color.black;
            _flashImage.gameObject.SetActive(true);
            yield return FadeCanvasGroup(_canvasGroup, 0.3f, 0.2f); 

            if (highestRarity >= 5)
            {
                yield return ShakeScreen(0.5f, 15f);
                yield return Flash(Color.yellow, 0.5f);
            }
            else if (highestRarity == 4)
            {
                yield return ShakeScreen(0.3f, 8f);
                yield return Flash(new Color(0.6f, 0.2f, 0.8f), 0.3f); 
            }
            else if (highestRarity == 3)
            {
                yield return Flash(new Color(0.2f, 0.5f, 1f), 0.2f); 
            }
            else
            {
                yield return new WaitForSeconds(0.3f); 
            }
        }

        private IEnumerator ShowResultCards(List<HeroInstance> results)
        {
            bool isMulti = results.Count > 1;
            
            foreach (var hero in results)
            {
                GameObject card = Instantiate(_cardPrefab, _resultContainer);
                Text cardText = card.GetComponentInChildren<Text>();
                Image cardBg = card.GetComponent<Image>();
                
                cardText.text = $"{hero.CurrentStar}★\n{hero.HeroDefId}";
                cardBg.color = GetRarityColor(hero.CurrentStar);

                if (!isMulti)
                {
                    card.transform.localScale = Vector3.zero;
                    yield return ScaleBounce(card.transform, 1.2f, 0.3f);
                    yield return ScaleBounce(card.transform, 1.0f, 0.1f);
                }
                else
                {
                    card.transform.localScale = Vector3.one; 
                }
            }

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0);
            
            ClearResults();
            _animationLayer.SetActive(false);
            yield return FadeCanvasGroup(_canvasGroup, 1f, 0.3f); 
        }

        private IEnumerator Flash(Color color, float duration)
        {
            _flashImage.color = color;
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
                float x = Random.Range(-1f, 1f) * magnitude * (1 - t/duration);
                float y = Random.Range(-1f, 1f) * magnitude * (1 - t/duration);
                rect.anchoredPosition = originalPos + new Vector2(x, y);
                yield return null;
            }
            rect.anchoredPosition = originalPos;
        }

        private IEnumerator ScaleBounce(Transform target, float endScale, float duration)
        {
            float startScale = target.localScale.x;
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(startScale, endScale, t / duration);
                target.localScale = Vector3.one * s;
                yield return null;
            }
            target.localScale = Vector3.one * endScale;
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

        private Color GetRarityColor(int stars)
        {
            return stars switch
            {
                1 => new Color(0.5f, 0.5f, 0.5f), 
                2 => new Color(0.3f, 0.6f, 0.3f), 
                3 => new Color(0.2f, 0.5f, 1f),   
                4 => new Color(0.6f, 0.2f, 0.8f), 
                5 => new Color(1f, 0.8f, 0.2f),   
                _ => Color.white
            };
        }
    }
}