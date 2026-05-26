#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using PickMeUp.UI;

namespace PickMeUp.EditorTools
{
    public static class SetupHubUI
    {
        // --- PREMIUM PALETTE ---
        private static readonly Color C_BG = Hex("#0D1117");       
        private static readonly Color C_SURFACE = Hex("#161B22");  
        private static readonly Color C_BORDER = Hex("#30363D");   
        private static readonly Color C_PRIMARY = Hex("#5B4FBF");  
        private static readonly Color C_SUCCESS = Hex("#1D9E75");  
        private static readonly Color C_DANGER = Hex("#E24B4A");   
        private static readonly Color C_TEXT = Color.white;
        private static readonly Color C_MUTED = Hex("#8B949E");    

        [MenuItem("Tools/PickMeUp/Setup Hub UI (Auto-Wire V22 - Emergency Text Fix)")]
        public static void SetupUI()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Hub")
            {
                EditorUtility.DisplayDialog("Error", "Please open the 'Hub' scene first.", "OK");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2340);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1.0f;
            scaler.referencePixelsPerUnit = 100;

            Image bgImg = canvas.gameObject.GetComponent<Image>();
            if (bgImg == null) bgImg = canvas.gameObject.AddComponent<Image>();
            bgImg.color = C_BG;

            string[] destroyNames = { "RosterPanel", "SynthesisPanel", "TrainingPanel", "TowerMapPanel", "SummonPanel", "BottomDock", 
                                     "SummonBtn", "TowerBtn", "RosterBtn", "SynthesisBtn", "ResetBtn", "TrainBtn", "CurrencyHUD" };
            foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj == null) continue;
                foreach (var name in destroyNames) {
                    if (obj.name.StartsWith(name)) { Undo.DestroyObjectImmediate(obj); break; }
                }
            }

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            string prefabPath = "Assets/Prefabs/HeroCardPrefab.prefab";
            if (File.Exists(prefabPath)) AssetDatabase.DeleteAsset(prefabPath);
            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");

GameObject heroCardPrefab = CreateGachaResultCardPrefab(); 
            GameObject gachaResultCardPrefab = CreateGachaResultCardPrefab();
            
            CreateGlobalHUD(canvas.transform);

            RosterView rosterView = CreateRosterPanel(canvas.transform, heroCardPrefab);
            SynthesisView synthesisView = CreateSynthesisPanel(canvas.transform, rosterView);
            TrainingView trainingView = CreateTrainingPanel(canvas.transform);
            TowerMapView towerView = CreateTowerMapPanel(canvas.transform);
            SummonView summonView = CreateSummonView(canvas.transform, gachaResultCardPrefab);
            
            CreateResponsiveBottomDock(canvas.transform, synthesisView, rosterView, trainingView, towerView, summonView);

            var hubTextObj = GameObject.Find("DisplayText");
            if (hubTextObj != null)
            {
                Text txt = hubTextObj.GetComponent<Text>();
                if (txt != null)
                {
                    txt.color = C_MUTED;
                    txt.fontStyle = FontStyle.Bold;
                    txt.alignment = TextAnchor.MiddleCenter;
                    RectTransform rect = txt.rectTransform;
                    rect.anchorMin = new Vector2(0, 0.25f); rect.anchorMax = new Vector2(1, 1);
                }
            }

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("<color=green>[SetupHubUI V22] SUCCESS! Emergency Text Fix applied.</color>");
        }

        #region UI Builders
        
        private static void CreateGlobalHUD(Transform parent)
        {
            GameObject hud = CreateGO("CurrencyHUD", parent, typeof(RectTransform));
            RectTransform rect = hud.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.92f); rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(8, 0); 
            rect.offsetMax = new Vector2(-8, 0);

            CreateCurrencyChip(hud.transform, "GoldChip", "GOLD:", "100,000", new Vector2(0, 0), new Vector2(0.48f, 1));
            CreateCurrencyChip(hud.transform, "GemChip", "GEMS:", "10,000", new Vector2(0.52f, 0), new Vector2(1f, 1));
        }

        private static void CreateCurrencyChip(Transform parent, string name, string label, string val, Vector2 min, Vector2 max)
        {
            GameObject chip = CreateGO(name, parent, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            RectTransform rect = chip.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            
            Image bg = chip.GetComponent<Image>();
            bg.color = C_SURFACE; 
            
            var hl = chip.GetComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(12, 12, 4, 4); 
            hl.spacing = 8;
            hl.childAlignment = TextAnchor.MiddleCenter;

            GameObject lblObj = CreateTextGO("Label", chip.transform, label, 28, TextAnchor.MiddleRight);
            lblObj.GetComponent<Text>().color = Hex("#8D96A0");
            lblObj.GetComponent<Text>().fontStyle = FontStyle.Normal;
            lblObj.GetComponent<Text>().resizeTextForBestFit = false;
            
            GameObject valObj = CreateTextGO("Value", chip.transform, val, 55, TextAnchor.MiddleLeft);
            Text valText = valObj.GetComponent<Text>();
            valText.color = Color.white;
            valText.fontStyle = FontStyle.Bold;
            valText.resizeTextForBestFit = false;
            valText.horizontalOverflow = HorizontalWrapMode.Overflow;
            valText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        

        private static GameObject CreateGachaResultCardPrefab()
        {
            // Root (Pure Black Background)
            GameObject card = CreateGO("GachaResultCard", null, typeof(RectTransform), typeof(Image), typeof(GachaResultCardUI));
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 280);
            Image bgImg = card.GetComponent<Image>();
            bgImg.color = Color.black; // FIX: Pure black #000000
            
            // 1. Portrait (Rendered BEHIND the frame)
            GameObject portrait = CreateGO("Portrait", card.transform, typeof(RectTransform), typeof(Image));
            RectTransform pRect = portrait.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.05f, 0.15f); 
            pRect.anchorMax = new Vector2(0.95f, 0.95f);
            pRect.offsetMin = pRect.offsetMax = Vector2.zero;
            Image pImg = portrait.GetComponent<Image>();
            pImg.color = Color.white; // FIX: Force bright white, no tint
            pImg.preserveAspect = true; // FIX: Don't stretch the art
            pImg.raycastTarget = false;

            // 2. Frame Overlay (Rendered ON TOP of the portrait)
            GameObject frame = CreateGO("FrameOverlay", card.transform, typeof(RectTransform), typeof(Image));
            StretchRect(frame.GetComponent<RectTransform>());
            Image frameImg = frame.GetComponent<Image>();
            frameImg.color = Color.gray; 
            frameImg.raycastTarget = false;

            // 3. Crest Icon
            GameObject crest = CreateGO("CrestIcon", card.transform, typeof(RectTransform), typeof(Image));
            RectTransform cRect = crest.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.35f, 0.85f); 
            cRect.anchorMax = new Vector2(0.65f, 0.98f);
            cRect.offsetMin = cRect.offsetMax = Vector2.zero;
            crest.GetComponent<Image>().color = Color.white;
            crest.GetComponent<Image>().raycastTarget = false;

            // 4. Star Container
            GameObject stars = CreateGO("StarContainer", card.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform sRect = stars.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0.2f, 0.18f); 
            sRect.anchorMax = new Vector2(0.8f, 0.28f);
            sRect.offsetMin = sRect.offsetMax = Vector2.zero;
            var hl = stars.GetComponent<HorizontalLayoutGroup>();
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.spacing = 5;

            Image[] starIcons = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                GameObject star = CreateGO($"Star_{i}", stars.transform, typeof(RectTransform), typeof(Image));
                star.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                Image sImg = star.GetComponent<Image>();
                sImg.color = Hex("#FFD700");
                sImg.raycastTarget = false;
                starIcons[i] = sImg;
            }

            // 5. Name Banner
            GameObject banner = CreateGO("NameBanner", card.transform, typeof(RectTransform), typeof(Image));
            RectTransform bRect = banner.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0, 0); 
            bRect.anchorMax = new Vector2(1, 0.15f);
            bRect.offsetMin = bRect.offsetMax = Vector2.zero;
            banner.GetComponent<Image>().color = Color.black;

            GameObject nameTxt = CreateTextGO("NameText", banner.transform, "◄ HERO ►", 28, TextAnchor.MiddleCenter);
            StretchRect(nameTxt.GetComponent<RectTransform>());
            nameTxt.GetComponent<Text>().color = Color.white;
            nameTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
            nameTxt.GetComponent<Text>().resizeTextForBestFit = true;

            // Wire up GachaResultCardUI
            GachaResultCardUI ui = card.GetComponent<GachaResultCardUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("FrameImage").objectReferenceValue = frameImg;
            so.FindProperty("PortraitImage").objectReferenceValue = pImg;
            so.FindProperty("CrestImage").objectReferenceValue = crest.GetComponent<Image>();
            so.FindProperty("BackgroundImage").objectReferenceValue = bgImg;
            so.FindProperty("NameBanner").objectReferenceValue = banner.GetComponent<Image>();
            so.FindProperty("NameText").objectReferenceValue = nameTxt.GetComponent<Text>();
            
            SerializedProperty starProp = so.FindProperty("StarIcons");
            starProp.arraySize = 5;
            for (int i = 0; i < 5; i++) 
                starProp.GetArrayElementAtIndex(i).objectReferenceValue = starIcons[i];
            
            so.ApplyModifiedProperties();

            string path = "Assets/Prefabs/GachaResultCard.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Undo.DestroyObjectImmediate(card); 
            return prefab;
        }

        private static RosterView CreateRosterPanel(Transform parent, GameObject heroPrefab)
        {
            GameObject panel = CreateGO("RosterPanel", parent, typeof(RectTransform), typeof(Image), typeof(RosterView));
            StretchRect(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = C_BG; 
            
            GameObject scrollView = CreateScrollView(panel.transform);
            GameObject closeBtn = CreateButtonGO("CloseRosterBtn", panel.transform, "✕", ButtonStyle.Ghost);
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.85f, 0.92f); closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.97f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            
            RosterView rosterView = panel.GetComponent<RosterView>();
            SerializedObject so = new SerializedObject(rosterView);
            so.FindProperty("contentParent").objectReferenceValue = scrollView.transform.Find("Viewport/Content");
            so.FindProperty("heroEntryPrefab").objectReferenceValue = heroPrefab;
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.ApplyModifiedProperties();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.GetComponent<Button>().onClick, rosterView.Hide);
            return rosterView;
        }

        private static SynthesisView CreateSynthesisPanel(Transform parent, RosterView rosterView)
        {
            GameObject panel = CreateGO("SynthesisPanel", parent, typeof(RectTransform), typeof(Image), typeof(SynthesisView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = new Vector2(1, 0.4f);
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = C_SURFACE;
            
            GameObject statusObj = CreateTextGO("StatusText", panel.transform, "SELECT MATERIALS", 48, TextAnchor.MiddleCenter);
            statusObj.GetComponent<Text>().color = C_MUTED;
            statusObj.GetComponent<Text>().fontStyle = FontStyle.Bold;
            StretchRect(statusObj.GetComponent<RectTransform>()); 
            statusObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 150); 
            statusObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -100);
            
            GameObject btnObj = CreateButtonGO("SynthesizeBtn", panel.transform, "FUSE", ButtonStyle.Primary);
            btnObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.05f); 
            btnObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.25f);
            btnObj.GetComponent<RectTransform>().offsetMin = btnObj.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            
            SynthesisView synthView = panel.GetComponent<SynthesisView>();
            HubView hubView = Object.FindAnyObjectByType<HubView>();
            SerializedObject so = new SerializedObject(synthView);
            so.FindProperty("panelRoot").objectReferenceValue = panel; 
            so.FindProperty("StatusText").objectReferenceValue = statusObj.GetComponent<Text>();
            so.FindProperty("SynthesizeButton").objectReferenceValue = btnObj.GetComponent<Button>();
            so.FindProperty("rosterView").objectReferenceValue = rosterView; 
            so.FindProperty("hubView").objectReferenceValue = hubView;
            so.ApplyModifiedProperties();
            
            GameObject closeBtn = CreateButtonGO("CloseSynthBtn", panel.transform, "CLOSE", ButtonStyle.Ghost);
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.85f); 
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.95f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.GetComponent<Button>().onClick, synthView.Hide);
            return synthView;
        }

        private static TrainingView CreateTrainingPanel(Transform parent)
        {
            GameObject panel = CreateGO("TrainingPanel", parent, typeof(RectTransform), typeof(Image), typeof(TrainingView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = new Vector2(1, 0.7f); 
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = C_SURFACE;

            GameObject scroll = CreateGO("ListScroll", panel.transform, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.55f); 
            scroll.GetComponent<RectTransform>().anchorMax = Vector2.one;
            scroll.GetComponent<RectTransform>().offsetMin = new Vector2(20, 120); 
            scroll.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);
            scroll.GetComponent<Image>().color = C_BG;
            
            GameObject vp = CreateGO("Viewport", scroll.transform, typeof(RectTransform), typeof(Image), typeof(Mask));
            StretchRect(vp.GetComponent<RectTransform>()); 
            vp.GetComponent<Image>().color = Color.white; 
            vp.GetComponent<Mask>().showMaskGraphic = false;
            GameObject content = CreateGO("Content", vp.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); 
            content.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            content.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            var vl = content.GetComponent<VerticalLayoutGroup>(); 
            vl.padding = new RectOffset(20,20,20,20); 
            vl.spacing = 15;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.GetComponent<ScrollRect>().content = content.GetComponent<RectTransform>(); 
            scroll.GetComponent<ScrollRect>().viewport = vp.GetComponent<RectTransform>(); 
            scroll.GetComponent<ScrollRect>().horizontal = false;

            GameObject entry = CreateGO("ListEntry", null, typeof(RectTransform), typeof(Image), typeof(Button));
            entry.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            Image entryImg = entry.GetComponent<Image>();
            entryImg.color = C_SURFACE;
            CreateTextGO("EntryText", entry.transform, "HERO NAME", 40, TextAnchor.MiddleCenter);
            string entryPath = "Assets/Prefabs/TrainingListEntry.prefab";
            GameObject entryPrefab = PrefabUtility.SaveAsPrefabAsset(entry, entryPath);
            Undo.DestroyObjectImmediate(entry);

            GameObject detailPanel = CreateGO("DetailArea", panel.transform, typeof(RectTransform));
            detailPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0); 
            detailPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.55f);
            detailPanel.GetComponent<RectTransform>().offsetMin = new Vector2(30, 20); 
            detailPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -20);

            GameObject nameTxt = CreateTextGO("HeroNameTxt", detailPanel.transform, "SELECT UNIT", 60, TextAnchor.MiddleCenter);
            nameTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.75f); 
            nameTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.95f);
            nameTxt.GetComponent<RectTransform>().offsetMin = nameTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            nameTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;

            GameObject lvlTxt = CreateTextGO("LevelTxt", detailPanel.transform, "LEVEL 1", 40, TextAnchor.MiddleCenter);
            lvlTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.6f); 
            lvlTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.75f);
            lvlTxt.GetComponent<RectTransform>().offsetMin = lvlTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            lvlTxt.GetComponent<Text>().color = C_MUTED;

            GameObject xpBarBg = CreateGO("XPBarBg", detailPanel.transform, typeof(RectTransform), typeof(Image));
            xpBarBg.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.45f); 
            xpBarBg.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.6f);
            xpBarBg.GetComponent<RectTransform>().offsetMin = xpBarBg.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            xpBarBg.GetComponent<Image>().color = C_BG;
            
            GameObject xpBarFill = CreateGO("XPBarFill", xpBarBg.transform, typeof(RectTransform), typeof(Image));
            StretchRect(xpBarFill.GetComponent<RectTransform>());
            xpBarFill.GetComponent<Image>().color = C_PRIMARY; 
            xpBarFill.GetComponent<Image>().type = Image.Type.Filled; 
            xpBarFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;

            GameObject xpTxt = CreateTextGO("XPTxt", detailPanel.transform, "0/100 XP", 30, TextAnchor.MiddleCenter);
            xpTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.3f); 
            xpTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.45f);
            xpTxt.GetComponent<RectTransform>().offsetMin = xpTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            xpTxt.GetComponent<Text>().color = C_MUTED;

            GameObject statsTxt = CreateTextGO("StatsTxt", detailPanel.transform, "HP: 100 | ATK: 20", 40, TextAnchor.MiddleCenter);
            statsTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.15f); 
            statsTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.3f);
            statsTxt.GetComponent<RectTransform>().offsetMin = statsTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            GameObject costTxt = CreateTextGO("CostTxt", detailPanel.transform, "COST: 50 GOLD", 40, TextAnchor.MiddleCenter);
            costTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0); 
            costTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 0.15f);
            costTxt.GetComponent<RectTransform>().offsetMin = costTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            costTxt.GetComponent<Text>().color = C_MUTED;

            GameObject lvlBtn = CreateButtonGO("LevelUpBtn", detailPanel.transform, "UPGRADE", ButtonStyle.Success);
            lvlBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0.02f); 
            lvlBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 0.15f);
            lvlBtn.GetComponent<RectTransform>().offsetMin = lvlBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            GameObject closeBtn = CreateButtonGO("CloseTrainBtn", panel.transform, "✕", ButtonStyle.Ghost);
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.8f, 0.92f); 
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.97f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            TrainingView tv = panel.GetComponent<TrainingView>();
            SerializedObject so = new SerializedObject(tv);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("HeroListContainer").objectReferenceValue = content.transform;
            so.FindProperty("HeroListEntryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("HeroNameText").objectReferenceValue = nameTxt.GetComponent<Text>();
            so.FindProperty("LevelText").objectReferenceValue = lvlTxt.GetComponent<Text>();
            so.FindProperty("XPText").objectReferenceValue = xpTxt.GetComponent<Text>();
            so.FindProperty("XPBarImage").objectReferenceValue = xpBarFill.GetComponent<Image>();
            so.FindProperty("StatsText").objectReferenceValue = statsTxt.GetComponent<Text>();
            so.FindProperty("GoldCostText").objectReferenceValue = costTxt.GetComponent<Text>();
            so.FindProperty("LevelUpButton").objectReferenceValue = lvlBtn.GetComponent<Button>();
            so.FindProperty("CloseButton").objectReferenceValue = closeBtn.GetComponent<Button>();
            so.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.GetComponent<Button>().onClick, tv.Hide);
            return tv;
        }

        private static TowerMapView CreateTowerMapPanel(Transform parent)
        {
            GameObject panel = CreateGO("TowerMapPanel", parent, typeof(RectTransform), typeof(Image), typeof(TowerMapView));
            StretchRect(panel.GetComponent<RectTransform>());
            panel.GetComponent<Image>().color = C_BG;

            GameObject header = CreateGO("Header", panel.transform, typeof(RectTransform));
            header.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.88f); 
            header.GetComponent<RectTransform>().anchorMax = Vector2.one;
            header.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20); 
            header.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);

            GameObject floorTxt = CreateTextGO("FloorText", header.transform, "TOWER", 60, TextAnchor.MiddleLeft);
            floorTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0); 
            floorTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
            floorTxt.GetComponent<RectTransform>().offsetMin = floorTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            floorTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;

            GameObject goldTxt = CreateTextGO("GoldText", header.transform, "0", 50, TextAnchor.MiddleRight);
            goldTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f); 
            goldTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 1);
            goldTxt.GetComponent<RectTransform>().offsetMin = goldTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            goldTxt.GetComponent<Text>().color = C_MUTED;

            GameObject retreatBtn = CreateButtonGO("RetreatButton", header.transform, "RETREAT", ButtonStyle.Danger);
            retreatBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.85f, 0.2f); 
            retreatBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.8f);
            retreatBtn.GetComponent<RectTransform>().offsetMin = retreatBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            GameObject nodeScroll = CreateGO("NodeScrollRect", panel.transform, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            nodeScroll.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.35f); 
            nodeScroll.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.88f);
            nodeScroll.GetComponent<RectTransform>().offsetMin = new Vector2(40, 20); 
            nodeScroll.GetComponent<RectTransform>().offsetMax = new Vector2(-40, -20);
            nodeScroll.GetComponent<Image>().color = C_SURFACE;

            GameObject nodeVp = CreateGO("Viewport", nodeScroll.transform, typeof(RectTransform), typeof(Image), typeof(Mask));
            StretchRect(nodeVp.GetComponent<RectTransform>()); 
            nodeVp.GetComponent<Image>().color = Color.white; 
            nodeVp.GetComponent<Mask>().showMaskGraphic = false;
            
            GameObject nodeContent = CreateGO("NodeContainer", nodeVp.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            nodeContent.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); 
            nodeContent.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            nodeContent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            var nodeVl = nodeContent.GetComponent<VerticalLayoutGroup>(); 
            nodeVl.padding = new RectOffset(20,20,20,20); 
            nodeVl.spacing = 20;
            nodeContent.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            nodeScroll.GetComponent<ScrollRect>().content = nodeContent.GetComponent<RectTransform>(); 
            nodeScroll.GetComponent<ScrollRect>().viewport = nodeVp.GetComponent<RectTransform>(); 
            nodeScroll.GetComponent<ScrollRect>().horizontal = false;

            GameObject logPanel = CreateGO("LogPanel", panel.transform, typeof(RectTransform), typeof(Image));
            logPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.15f); 
            logPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.35f);
            logPanel.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20); 
            logPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);
            logPanel.GetComponent<Image>().color = C_SURFACE;

            GameObject logScroll = CreateGO("LogScrollRect", logPanel.transform, typeof(RectTransform), typeof(ScrollRect));
            StretchRect(logScroll.GetComponent<RectTransform>());
            GameObject logVp = CreateGO("Viewport", logScroll.transform, typeof(RectTransform), typeof(Mask));
            StretchRect(logVp.GetComponent<RectTransform>()); 
            logVp.GetComponent<Mask>().showMaskGraphic = false;
            GameObject logContent = CreateGO("Content", logVp.transform, typeof(RectTransform));
            StretchRect(logContent.GetComponent<RectTransform>()); 
            logContent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
            logScroll.GetComponent<ScrollRect>().content = logContent.GetComponent<RectTransform>();
            logScroll.GetComponent<ScrollRect>().viewport = logVp.GetComponent<RectTransform>();

            GameObject logTxt = CreateTextGO("CombatLogText", logContent.transform, "AWAITING ORDERS...", 36, TextAnchor.LowerLeft);
            StretchRect(logTxt.GetComponent<RectTransform>()); 
            logTxt.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20);
            logTxt.GetComponent<Text>().color = C_MUTED;

            GameObject startBtn = CreateButtonGO("StartRunButton", panel.transform, "DEPLOY", ButtonStyle.Primary);
            startBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.03f); 
            startBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.12f);
            startBtn.GetComponent<RectTransform>().offsetMin = startBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            TowerMapView tView = panel.GetComponent<TowerMapView>();
            SerializedObject so = new SerializedObject(tView);
            so.FindProperty("_floorText").objectReferenceValue = floorTxt.GetComponent<Text>();
            so.FindProperty("_goldText").objectReferenceValue = goldTxt.GetComponent<Text>();
            so.FindProperty("_retreatButton").objectReferenceValue = retreatBtn.GetComponent<Button>();
            so.FindProperty("_nodeScrollRect").objectReferenceValue = nodeScroll.GetComponent<ScrollRect>();
            so.FindProperty("_nodeContainer").objectReferenceValue = nodeContent.transform;
            so.FindProperty("_startRunButton").objectReferenceValue = startBtn.GetComponent<Button>();
            so.FindProperty("_combatLogText").objectReferenceValue = logTxt.GetComponent<Text>();
            so.FindProperty("_logScrollRect").objectReferenceValue = logScroll.GetComponent<ScrollRect>();
            so.FindProperty("_logPanel").objectReferenceValue = logPanel;
            so.ApplyModifiedProperties();

            panel.SetActive(false);
            return tView;
        }

        private static SummonView CreateSummonView(Transform parent, GameObject gachaResultCardPrefab)
        {
            GameObject panel = CreateGO("SummonPanel", parent, typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(SummonView));
            StretchRect(panel.GetComponent<RectTransform>());
            
            Image panelBg = panel.GetComponent<Image>();
            panelBg.color = new Color(13f/255f, 17f/255f, 23f/255f, 1f); 
            
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            cg.alpha = 1f; 

            GameObject header = CreateGO("Header", panel.transform, typeof(RectTransform));
            header.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.9f); 
            header.GetComponent<RectTransform>().anchorMax = Vector2.one;
            header.GetComponent<RectTransform>().offsetMin = new Vector2(20, 20); 
            header.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);

            GameObject goldTxt = CreateTextGO("GoldText", header.transform, "GOLD: 0", 55, TextAnchor.MiddleLeft);
            goldTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0); 
            goldTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.4f, 1);
            goldTxt.GetComponent<RectTransform>().offsetMin = goldTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            goldTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 0);
            Text goldTextComp = goldTxt.GetComponent<Text>();
            goldTextComp.color = Color.white;
            goldTextComp.resizeTextForBestFit = false;
            goldTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            goldTextComp.verticalOverflow = VerticalWrapMode.Overflow;
            
            GameObject gemTxt = CreateTextGO("GemText", header.transform, "GEMS: 0", 55, TextAnchor.MiddleCenter);
            gemTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.4f, 0); 
            gemTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 1);
            gemTxt.GetComponent<RectTransform>().offsetMin = gemTxt.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            gemTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 0);
            Text gemTextComp = gemTxt.GetComponent<Text>();
            gemTextComp.color = Color.white;
            gemTextComp.resizeTextForBestFit = false;
            gemTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            gemTextComp.verticalOverflow = VerticalWrapMode.Overflow;

            GameObject closeBtn = CreateButtonGO("CloseBtn", header.transform, "✕", ButtonStyle.Ghost);
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.85f, 0.2f); 
            closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.8f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            GameObject bottomRow = CreateGO("SummonBottomRow", panel.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform rowRect = bottomRow.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 0); 
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0.5f, 0);
            rowRect.sizeDelta = new Vector2(0, 296);

            var hl = bottomRow.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 8; 
            hl.padding = new RectOffset(16, 16, 0, 0); 
            hl.childAlignment = TextAnchor.LowerCenter;
            hl.childForceExpandWidth = true; 
            hl.childForceExpandHeight = true;
            hl.childControlWidth = true;
            hl.childControlHeight = true;

            // Updated CreateSummonCard calls with new signature
            GameObject std1 = CreateSummonCard(bottomRow.transform, "STANDARD", "x1", "1,000 GOLD", Hex("#D4A017"), Hex("#1C2A3A"), Hex("#378ADD"), "PITY: 0 / 10", false);
            GameObject std10 = CreateSummonCard(bottomRow.transform, "STANDARD", "x10", "9,000 GOLD", Hex("#D4A017"), Hex("#1C2A3A"), Hex("#378ADD"), "PITY: 0 / 10", false);
            GameObject prem1 = CreateSummonCard(bottomRow.transform, "PREMIUM", "x1", "300 GEMS", Hex("#7B5FD4"), Hex("#2A1C3A"), Hex("#7B5FD4"), "PITY: 0 / 10", false);
            GameObject prem10 = CreateSummonCard(bottomRow.transform, "PREMIUM", "x10", "2,700 GEMS", Hex("#7B5FD4"), Hex("#2A1A1A"), Hex("#D4A017"), "PITY: 0 / 10", true);

            // Get references to PityText components for each card
            Text std1Pity = std1.transform.Find("PityText")?.GetComponent<Text>();
            Text std10Pity = std10.transform.Find("PityText")?.GetComponent<Text>();
            Text prem1Pity = prem1.transform.Find("PityText")?.GetComponent<Text>();
            Text prem10Pity = prem10.transform.Find("PityText")?.GetComponent<Text>();
            
            // Get GuaranteeBadge for premium 10-pull
            GameObject guaranteeBadge = prem10.transform.Find("GuaranteeBadge")?.gameObject;
            
            // Get Shimmer images
            Image std1Shimmer = std1.transform.Find("Shimmer")?.GetComponent<Image>();
            Image std10Shimmer = std10.transform.Find("Shimmer")?.GetComponent<Image>();
            Image prem1Shimmer = prem1.transform.Find("Shimmer")?.GetComponent<Image>();
            Image prem10Shimmer = prem10.transform.Find("Shimmer")?.GetComponent<Image>();

            GameObject animLayer = CreateGO("AnimationLayer", panel.transform, typeof(RectTransform));
            StretchRect(animLayer.GetComponent<RectTransform>());
            animLayer.transform.SetAsLastSibling();

            GameObject crackImg = CreateGO("CrackImage", animLayer.transform, typeof(RectTransform), typeof(Image));
            StretchRect(crackImg.GetComponent<RectTransform>());
            crackImg.GetComponent<Image>().color = Color.white;
            crackImg.gameObject.SetActive(false);

            GameObject flashImg = CreateGO("FlashImage", animLayer.transform, typeof(RectTransform), typeof(Image));
            StretchRect(flashImg.GetComponent<RectTransform>());
            flashImg.GetComponent<Image>().color = Color.white;
            flashImg.gameObject.SetActive(false);

            GameObject resultContainer = CreateGO("ResultContainer", animLayer.transform, typeof(RectTransform), typeof(GridLayoutGroup));
            resultContainer.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0.2f); 
            resultContainer.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.85f);
            resultContainer.GetComponent<RectTransform>().offsetMin = resultContainer.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            var resGrid = resultContainer.GetComponent<GridLayoutGroup>(); 
            resGrid.cellSize = new Vector2(180, 250); 
            resGrid.spacing = new Vector2(15, 15); 
            resGrid.childAlignment = TextAnchor.MiddleCenter;
            resGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            resGrid.constraintCount = 5; 

            GameObject solidBg = CreateGO("SolidBackground", animLayer.transform, typeof(RectTransform), typeof(Image));
            StretchRect(solidBg.GetComponent<RectTransform>());
            solidBg.GetComponent<Image>().color = Color.black;
            solidBg.SetActive(false);

            SummonView sv = panel.GetComponent<SummonView>();
            SerializedObject so = new SerializedObject(sv);
            so.FindProperty("_panelRoot").objectReferenceValue = panel;
            so.FindProperty("_canvasGroup").objectReferenceValue = cg;
            so.FindProperty("_goldText").objectReferenceValue = goldTxt.GetComponent<Text>();
            so.FindProperty("_gemText").objectReferenceValue = gemTxt.GetComponent<Text>();
            so.FindProperty("_std1Btn").objectReferenceValue = std1.GetComponent<Button>();
            so.FindProperty("_std10Btn").objectReferenceValue = std10.GetComponent<Button>();
            so.FindProperty("_prem1Btn").objectReferenceValue = prem1.GetComponent<Button>();
            so.FindProperty("_prem10Btn").objectReferenceValue = prem10.GetComponent<Button>();
            so.FindProperty("_closeBtn").objectReferenceValue = closeBtn.GetComponent<Button>();
            so.FindProperty("_std1PityText").objectReferenceValue = std1Pity;
            so.FindProperty("_std10PityText").objectReferenceValue = std10Pity;
            so.FindProperty("_prem1PityText").objectReferenceValue = prem1Pity;
            so.FindProperty("_prem10PityText").objectReferenceValue = prem10Pity;
            so.FindProperty("_prem10GuaranteeBadge").objectReferenceValue = guaranteeBadge;
            so.FindProperty("_std1Shimmer").objectReferenceValue = std1Shimmer;
            so.FindProperty("_std10Shimmer").objectReferenceValue = std10Shimmer;
            so.FindProperty("_prem1Shimmer").objectReferenceValue = prem1Shimmer;
            so.FindProperty("_prem10Shimmer").objectReferenceValue = prem10Shimmer;
            so.FindProperty("_animationLayer").objectReferenceValue = animLayer;
            so.FindProperty("_crackImage").objectReferenceValue = crackImg.GetComponent<Image>();
            so.FindProperty("_flashImage").objectReferenceValue = flashImg.GetComponent<Image>();
            so.FindProperty("_resultContainer").objectReferenceValue = resultContainer.transform;
            so.FindProperty("_cardPrefab").objectReferenceValue = gachaResultCardPrefab;
            so.FindProperty("_solidBackground").objectReferenceValue = solidBg.GetComponent<Image>();
            so.ApplyModifiedProperties();

            panel.SetActive(false);
            return sv;
        }

        private static GameObject CreateSummonCard(Transform parent, string title, string subtitle, string cost, Color costColor, Color bgColor, Color borderColor, string pityText, bool showGuarantee)
        {
            GameObject card = CreateGO("Card", parent, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(VerticalLayoutGroup), typeof(Mask));
            
            Image bg = card.GetComponent<Image>();
            bg.color = bgColor;
            bg.type = Image.Type.Sliced;

            Outline border = card.GetComponent<Outline>();
            border.effectColor = borderColor;
            border.effectDistance = new Vector2(2f, -2f); 

            var vl = card.GetComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(0, 0, 12, 12);
            vl.spacing = 4;
            vl.childAlignment = TextAnchor.MiddleCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = false;

            // Title
            GameObject titleObj = CreateTextGO("Title", card.transform, title, 90, TextAnchor.MiddleCenter);
            Text titleText = titleObj.GetComponent<Text>();
            titleText.color = Color.white;
            titleText.fontStyle = FontStyle.Bold;
            titleText.resizeTextForBestFit = false;
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            titleObj.AddComponent<LayoutElement>().preferredHeight = 100;

            // Subtitle
            GameObject subObj = CreateTextGO("Subtitle", card.transform, subtitle, 70, TextAnchor.MiddleCenter);
            Text subText = subObj.GetComponent<Text>();
            subText.color = Color.white;
            subText.resizeTextForBestFit = false;
            subText.horizontalOverflow = HorizontalWrapMode.Overflow;
            subText.verticalOverflow = VerticalWrapMode.Overflow;
            subObj.AddComponent<LayoutElement>().preferredHeight = 80;

            // Cost
            GameObject costObj = CreateTextGO("Cost", card.transform, cost, 60, TextAnchor.MiddleCenter);
            Text costText = costObj.GetComponent<Text>();
            costText.color = costColor;
            costText.fontStyle = FontStyle.Bold;
            costText.resizeTextForBestFit = false;
            costText.horizontalOverflow = HorizontalWrapMode.Overflow;
            costText.verticalOverflow = VerticalWrapMode.Overflow;
            costObj.AddComponent<LayoutElement>().preferredHeight = 70;

            // Pity Text
            GameObject pityObj = CreateTextGO("PityText", card.transform, pityText, 28, TextAnchor.MiddleCenter);
            Text pityTextComp = pityObj.GetComponent<Text>();
            pityTextComp.color = Hex("#8B949E"); 
            pityTextComp.resizeTextForBestFit = false; // Explicitly disabled
            pityTextComp.fontSize = 28;                // Explicit size
            pityTextComp.horizontalOverflow = HorizontalWrapMode.Overflow;
            pityTextComp.verticalOverflow = VerticalWrapMode.Overflow;
            
            // Force layout size so it doesn't collapse to 0
            LayoutElement le = pityObj.GetComponent<LayoutElement>();
            if (le == null) le = pityObj.AddComponent<LayoutElement>();
            le.preferredHeight = 35;
            le.minWidth = 100;

            // Guarantee Badge (Only for Premium x10)
            if (showGuarantee)
            {
                GameObject badge = CreateGO("GuaranteeBadge", card.transform, typeof(RectTransform), typeof(Image));
                badge.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.9f);
                badge.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.9f);
                badge.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
                badge.GetComponent<Image>().color = Hex("#FFD700");
                GameObject badgeTxt = CreateTextGO("BadgeText", badge.transform, "GUARANTEED 4★+", 24, TextAnchor.MiddleCenter);
                badgeTxt.GetComponent<Text>().color = Color.black;
                badgeTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
                badge.SetActive(false); // Hidden by default, toggled by SummonView
            }

            // Shimmer Effect
            GameObject shimmerObj = CreateGO("Shimmer", card.transform, typeof(RectTransform), typeof(Image));
            shimmerObj.GetComponent<RectTransform>().anchorMin = new Vector2(-0.5f, 0);
            shimmerObj.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1);
            shimmerObj.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
            shimmerObj.GetComponent<Image>().raycastTarget = false;

            return card;
        }

        private static void CreateResponsiveBottomDock(Transform parent, SynthesisView synthView, RosterView rosterView, TrainingView trainingView, TowerMapView towerView, SummonView summonView)
        {
            GameObject dock = CreateGO("BottomDock", parent, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform dockRect = dock.GetComponent<RectTransform>();
            dockRect.anchorMin = new Vector2(0, 0); 
            dockRect.anchorMax = new Vector2(1, 0.12f); 
            dockRect.offsetMin = new Vector2(15, 15); 
            dockRect.offsetMax = new Vector2(-15, -10);
            HorizontalLayoutGroup layout = dock.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8); 
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true; 
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true; 
            layout.childControlHeight = true; 

            GameObject rosterBtn = CreateDockButton(dock.transform, "ROSTER");
            GameObject synthBtn = CreateDockButton(dock.transform, "SYNTH");
            GameObject trainBtn = CreateDockButton(dock.transform, "TRAIN");
            GameObject towerBtn = CreateDockButton(dock.transform, "TOWER"); 
            GameObject summonBtn = CreateDockButton(dock.transform, "SUMMON");
            GameObject resetBtn = CreateDockButton(dock.transform, "RESET");

            UnityEditor.Events.UnityEventTools.AddPersistentListener(rosterBtn.GetComponent<Button>().onClick, rosterView.Show);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(synthBtn.GetComponent<Button>().onClick, synthView.Show);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(trainBtn.GetComponent<Button>().onClick, trainingView.Show);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(towerBtn.GetComponent<Button>().onClick, towerView.Show); 
            UnityEditor.Events.UnityEventTools.AddPersistentListener(summonBtn.GetComponent<Button>().onClick, summonView.Show);
            
            ResetGameButton resetScript = resetBtn.AddComponent<ResetGameButton>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, resetScript.ResetGame);
        }

        private static GameObject CreateDockButton(Transform parent, string label)
        {
            GameObject btnObj = CreateGO($"{label}Btn", parent, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; 
            rect.anchorMax = Vector2.one; 
            rect.sizeDelta = Vector2.zero; 
            rect.anchoredPosition = Vector2.zero;
            Image img = btnObj.GetComponent<Image>();
            img.color = C_SURFACE; 
            
            GameObject txtObj = CreateTextGO("Text", btnObj.transform, label, 100, TextAnchor.MiddleCenter);
            Text txt = txtObj.GetComponent<Text>(); 
            txt.resizeTextForBestFit = true; 
            txt.resizeTextMinSize = 24; 
            txt.resizeTextMaxSize = 60;
            txt.fontStyle = FontStyle.Bold;
            RectTransform textRect = txtObj.GetComponent<RectTransform>(); 
            textRect.anchorMin = Vector2.zero; 
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5); 
            textRect.offsetMax = new Vector2(-5, -5); 
            return btnObj;
        }

        #endregion
        
        #region Helpers
        
        private enum ButtonStyle { Primary, Ghost, Success, Danger }

        private static GameObject CreateButtonGO(string name, Transform parent, string label, ButtonStyle style) { 
            GameObject btnObj = CreateGO(name, parent, typeof(RectTransform), typeof(Image), typeof(Button)); 
            Image img = btnObj.GetComponent<Image>();
            Text txt = null;

            switch (style)
            {
                case ButtonStyle.Primary:
                    img.color = C_PRIMARY;
                    txt = CreateTextGO("Text", btnObj.transform, label, 48, TextAnchor.MiddleCenter).GetComponent<Text>();
                    txt.color = Color.white;
                    break;
                case ButtonStyle.Ghost:
                    img.color = C_SURFACE;
                    txt = CreateTextGO("Text", btnObj.transform, label, 48, TextAnchor.MiddleCenter).GetComponent<Text>();
                    txt.color = C_TEXT;
                    break;
                case ButtonStyle.Success:
                    img.color = C_SUCCESS;
                    txt = CreateTextGO("Text", btnObj.transform, label, 48, TextAnchor.MiddleCenter).GetComponent<Text>();
                    txt.color = Color.white;
                    break;
                case ButtonStyle.Danger:
                    img.color = C_DANGER;
                    txt = CreateTextGO("Text", btnObj.transform, label, 48, TextAnchor.MiddleCenter).GetComponent<Text>();
                    txt.color = Color.white;
                    break;
            }
            
            txt.fontStyle = FontStyle.Bold;
            StretchRect(txt.GetComponent<RectTransform>()); 
            txt.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            txt.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            return btnObj; 
        }

        private static GameObject CreateTextGO(string name, Transform parent, string defaultText, int fontSize, TextAnchor alignment) { 
            GameObject obj = CreateGO(name, parent, typeof(RectTransform), typeof(Text)); 
            Text txt = obj.GetComponent<Text>(); 
            txt.text = defaultText; 
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize); 
            txt.fontSize = fontSize; 
            txt.alignment = alignment; 
            txt.color = C_TEXT; 
            txt.raycastTarget = false; 
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 10;
            txt.resizeTextMaxSize = fontSize; 
            return obj; 
        }

        private static GameObject CreateGO(string name, Transform parent, params System.Type[] components) { 
            GameObject obj = new GameObject(name, components); 
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name); 
            if (parent != null) obj.transform.SetParent(parent, false); 
            return obj; 
        }
        
        private static GameObject CreateScrollView(Transform parent) { 
            GameObject scroll = CreateGO("ScrollView", parent, typeof(RectTransform), typeof(Image), typeof(ScrollRect)); 
            StretchRect(scroll.GetComponent<RectTransform>()); 
            scroll.GetComponent<RectTransform>().offsetMin = new Vector2(20, 440); 
            scroll.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -120); 
            scroll.GetComponent<Image>().color = C_BG; 
            GameObject viewport = CreateGO("Viewport", scroll.transform, typeof(RectTransform), typeof(Image), typeof(Mask)); 
            StretchRect(viewport.GetComponent<RectTransform>()); 
            viewport.GetComponent<Image>().color = Color.white; 
            viewport.GetComponent<Mask>().showMaskGraphic = false; 
            GameObject content = CreateGO("Content", viewport.transform, typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)); 
            RectTransform contentRect = content.GetComponent<RectTransform>(); 
            contentRect.anchorMin = new Vector2(0, 1); 
            contentRect.anchorMax = new Vector2(1, 1); 
            contentRect.pivot = new Vector2(0.5f, 1); 
            contentRect.sizeDelta = new Vector2(0, 0); 
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>(); 
            grid.cellSize = new Vector2(320, 420); 
            grid.spacing = new Vector2(25, 25); 
            grid.padding = new RectOffset(25, 25, 25, 25); 
            grid.childAlignment = TextAnchor.UpperCenter; 
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize; 
            ScrollRect sr = scroll.GetComponent<ScrollRect>(); 
            sr.content = contentRect; 
            sr.viewport = viewport.GetComponent<RectTransform>(); 
            sr.horizontal = false; 
            return scroll; 
        }
        
        private static void StretchRect(RectTransform rect) { 
            rect.anchorMin = Vector2.zero; 
            rect.anchorMax = Vector2.one; 
            rect.offsetMin = Vector2.zero; 
            rect.offsetMax = Vector2.zero; 
        }
        
        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
        #endregion
    }
}
#endif