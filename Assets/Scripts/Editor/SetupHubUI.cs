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
        [MenuItem("Tools/PickMeUp/Setup Hub UI (Auto-Wire V15 Mobile - Training Room)")]
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

            string[] destroyNames = { "RosterPanel", "SynthesisPanel", "TrainingPanel", "BottomDock", 
                                     "SummonBtn", "TowerBtn", "RosterBtn", "SynthesisBtn", 
                                     "ResetBtn", "TrainBtn" };
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

            GameObject heroCardPrefab = CreateHeroCardPrefab();
            RosterView rosterView = CreateRosterPanel(canvas.transform, heroCardPrefab);
            SynthesisView synthesisView = CreateSynthesisPanel(canvas.transform, rosterView);
            TrainingView trainingView = CreateTrainingPanel(canvas.transform);
            CreateResponsiveBottomDock(canvas.transform, synthesisView, rosterView, trainingView);

            var hubTextObj = GameObject.Find("DisplayText");
            if (hubTextObj != null)
            {
                Text txt = hubTextObj.GetComponent<Text>();
                if (txt != null)
                {
                    txt.resizeTextForBestFit = true;
                    txt.resizeTextMinSize = 40; txt.resizeTextMaxSize = 100;
                    txt.alignment = TextAnchor.MiddleCenter;
                    RectTransform rect = txt.rectTransform;
                    rect.anchorMin = new Vector2(0, 0.25f); rect.anchorMax = new Vector2(1, 1);
                    rect.offsetMin = new Vector2(20, 20); rect.offsetMax = new Vector2(-20, -20);
                }
            }

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("<color=green>[SetupHubUI V15] SUCCESS! Training Room added & wired.</color>");
        }

        #region UI Builders
        private static GameObject CreateHeroCardPrefab()
        {
            GameObject card = CreateGO("HeroCardPrefab", null, typeof(RectTransform), typeof(Image), typeof(HeroCardUI));
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 420); 
            Image cardBg = card.GetComponent<Image>(); cardBg.color = new Color(0.3f, 0.3f, 0.3f, 1f); 
            GameObject highlightObj = CreateGO("Highlight", card.transform, typeof(RectTransform), typeof(Image));
            StretchRect(highlightObj.GetComponent<RectTransform>());
            Image highlight = highlightObj.GetComponent<Image>();
            highlight.color = new Color(1f, 0.92f, 0.016f, 1f); highlight.enabled = false; highlight.raycastTarget = false;
            GameObject btnObj = CreateGO("SelectButton", card.transform, typeof(RectTransform), typeof(Image), typeof(Button));
            StretchRect(btnObj.GetComponent<RectTransform>()); btnObj.GetComponent<Image>().color = new Color(1, 1, 1, 0); 
            GameObject portraitObj = CreateGO("PortraitImage", card.transform, typeof(RectTransform), typeof(Image));
            RectTransform pRect = portraitObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0, 0.55f); pRect.anchorMax = new Vector2(1, 0.95f);
            pRect.sizeDelta = Vector2.zero; pRect.offsetMin = Vector2.one * 15; pRect.offsetMax = -Vector2.one * 15;
            Image portraitImg = portraitObj.GetComponent<Image>();
            portraitImg.type = Image.Type.Simple; portraitImg.preserveAspect = true;
            GameObject nameObj = CreateTextGO("NameText", card.transform, "1★ Hero", 42, TextAnchor.MiddleCenter);
            nameObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.35f); nameObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.55f);
            GameObject statsObj = CreateTextGO("StatsText", card.transform, "Lv.1\nHP:100", 36, TextAnchor.MiddleCenter);
            statsObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0); statsObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.35f);
            HeroCardUI cardUI = card.GetComponent<HeroCardUI>();
            cardUI.NameText = nameObj.GetComponent<Text>(); cardUI.StatsText = statsObj.GetComponent<Text>();
            cardUI.SelectionHighlight = highlight; cardUI.BackgroundImage = cardBg; cardUI.PortraitImage = portraitImg;
            Button btn = btnObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, cardUI.OnCardClicked);
            string path = "Assets/Prefabs/HeroCardPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, path);
            Undo.DestroyObjectImmediate(card); return prefab;
        }

        private static RosterView CreateRosterPanel(Transform parent, GameObject heroPrefab)
        {
            GameObject panel = CreateGO("RosterPanel", parent, typeof(RectTransform), typeof(Image), typeof(RosterView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.98f);
            GameObject scrollView = CreateScrollView(panel.transform);
            GameObject closeBtn = CreateButtonGO("CloseRosterBtn", panel.transform, "X CLOSE");
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.75f, 0.92f); closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.97f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 1f); 
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
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = new Vector2(1, 0.35f);
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.1f, 0.05f, 0.05f, 0.95f);
            GameObject statusObj = CreateTextGO("StatusText", panel.transform, "Select 2 or 3 heroes...", 64, TextAnchor.MiddleCenter);
            statusObj.GetComponent<Text>().resizeTextForBestFit = true; statusObj.GetComponent<Text>().resizeTextMinSize = 40; statusObj.GetComponent<Text>().resizeTextMaxSize = 80;
            StretchRect(statusObj.GetComponent<RectTransform>()); statusObj.GetComponent<RectTransform>().offsetMin = new Vector2(30, 150); statusObj.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -100);
            GameObject btnObj = CreateButtonGO("SynthesizeBtn", panel.transform, "SYNTHESIZE");
            btnObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.05f); btnObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.25f);
            btnObj.GetComponent<RectTransform>().offsetMin = btnObj.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            SynthesisView synthView = panel.GetComponent<SynthesisView>();
            HubView hubView = Object.FindAnyObjectByType<HubView>();
            SerializedObject so = new SerializedObject(synthView);
            so.FindProperty("panelRoot").objectReferenceValue = panel; so.FindProperty("StatusText").objectReferenceValue = statusObj.GetComponent<Text>();
            so.FindProperty("SynthesizeButton").objectReferenceValue = btnObj.GetComponent<Button>();
            so.FindProperty("rosterView").objectReferenceValue = rosterView; so.FindProperty("hubView").objectReferenceValue = hubView;
            so.ApplyModifiedProperties();
            GameObject closeBtn = CreateButtonGO("CloseSynthBtn", panel.transform, "▲ CLOSE");
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.35f, 0.85f); closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.65f, 0.95f);
            closeBtn.GetComponent<RectTransform>().offsetMin = closeBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f); 
            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.GetComponent<Button>().onClick, synthView.Hide);
            return synthView;
        }

        private static TrainingView CreateTrainingPanel(Transform parent)
        {
            GameObject panel = CreateGO("TrainingPanel", parent, typeof(RectTransform), typeof(Image), typeof(TrainingView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = new Vector2(1, 0.65f); // Covers top 65%
            panelRect.offsetMin = Vector2.zero; panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.98f);

            // Hero List (Scrollable)
            GameObject scroll = CreateGO("ListScroll", panel.transform, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scroll.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.55f); scroll.GetComponent<RectTransform>().anchorMax = Vector2.one;
            scroll.GetComponent<RectTransform>().offsetMin = new Vector2(20, 120); scroll.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -20);
            scroll.GetComponent<Image>().color = new Color(0,0,0,0.2f);
            GameObject vp = CreateGO("Viewport", scroll.transform, typeof(RectTransform), typeof(Image), typeof(Mask));
            StretchRect(vp.GetComponent<RectTransform>()); vp.GetComponent<Image>().color = Color.white; vp.GetComponent<Mask>().showMaskGraphic = false;
            GameObject content = CreateGO("Content", vp.transform, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1); content.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
            content.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
            var vl = content.GetComponent<VerticalLayoutGroup>(); vl.padding = new RectOffset(20,20,20,20); vl.spacing = 15;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.GetComponent<ScrollRect>().content = content.GetComponent<RectTransform>(); scroll.GetComponent<ScrollRect>().viewport = vp.GetComponent<RectTransform>(); scroll.GetComponent<ScrollRect>().horizontal = false;

            // List Entry Prefab
            GameObject entry = CreateGO("ListEntry", null, typeof(RectTransform), typeof(Image), typeof(Button));
            entry.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            entry.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1f);
            CreateTextGO("EntryText", entry.transform, "Hero Name", 52, TextAnchor.MiddleCenter).GetComponent<Text>().resizeTextForBestFit = true;
            entry.GetComponent<Text>().resizeTextMinSize = 30; entry.GetComponent<Text>().resizeTextMaxSize = 60;
            string entryPath = "Assets/Prefabs/TrainingListEntry.prefab";
            GameObject entryPrefab = PrefabUtility.SaveAsPrefabAsset(entry, entryPath);
            Undo.DestroyObjectImmediate(entry);

            // Details Area
            GameObject detailPanel = CreateGO("DetailArea", panel.transform, typeof(RectTransform));
            detailPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0); detailPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.55f);
            detailPanel.GetComponent<RectTransform>().offsetMin = new Vector2(30, 20); detailPanel.GetComponent<RectTransform>().offsetMax = new Vector2(-30, -20);

            Text nameTxt = CreateTextGO("HeroNameTxt", detailPanel.transform, "Select a Hero", 70, TextAnchor.MiddleCenter);
            nameTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.75f); nameTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.95f);
            nameTxt.GetComponent<Text>().resizeTextForBestFit = true; nameTxt.GetComponent<Text>().resizeTextMinSize = 40; nameTxt.GetComponent<Text>().resizeTextMaxSize = 80;

            Text lvlTxt = CreateTextGO("LevelTxt", detailPanel.transform, "Level 1", 60, TextAnchor.MiddleCenter);
            lvlTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.6f); lvlTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.75f);
            lvlTxt.GetComponent<Text>().resizeTextForBestFit = true;

            GameObject xpBarBg = CreateGO("XPBarBg", detailPanel.transform, typeof(RectTransform), typeof(Image));
            xpBarBg.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.45f); xpBarBg.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.6f);
            xpBarBg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
            GameObject xpBarFill = CreateGO("XPBarFill", xpBarBg.transform, typeof(RectTransform), typeof(Image));
            xpBarFill.GetComponent<RectTransform>().anchorMin = Vector2.zero; xpBarFill.GetComponent<RectTransform>().anchorMax = Vector2.one;
            xpBarFill.GetComponent<Image>().color = new Color(0.9f, 0.8f, 0.1f, 1f); xpBarFill.GetComponent<Image>().type = Image.Type.Filled; xpBarFill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;

            Text xpTxt = CreateTextGO("XPTxt", detailPanel.transform, "0/100 XP", 50, TextAnchor.MiddleCenter);
            xpTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.3f); xpTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.45f);
            xpTxt.GetComponent<Text>().resizeTextForBestFit = true;

            Text statsTxt = CreateTextGO("StatsTxt", detailPanel.transform, "HP: 100 | ATK: 20", 55, TextAnchor.MiddleCenter);
            statsTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.15f); statsTxt.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.3f);
            statsTxt.GetComponent<Text>().resizeTextForBestFit = true;

            Text costTxt = CreateTextGO("CostTxt", detailPanel.transform, "Cost: 50 Gold", 60, TextAnchor.MiddleCenter);
            costTxt.GetComponent<RectTransform>().anchorMin = new Vector2(0.25f, 0); costTxt.GetComponent<RectTransform>().anchorMax = new Vector2(0.75f, 0.15f);
            costTxt.GetComponent<Text>().resizeTextForBestFit = true;

            GameObject lvlBtn = CreateButtonGO("LevelUpBtn", detailPanel.transform, "LEVEL UP");
            lvlBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.15f, 0.02f); lvlBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.85f, 0.15f);
            lvlBtn.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.2f, 1f);
            lvlBtn.GetComponent<Text>().resizeTextForBestFit = true; lvlBtn.GetComponent<Text>().resizeTextMinSize = 30; lvlBtn.GetComponent<Text>().resizeTextMaxSize = 70;

            GameObject closeBtn = CreateButtonGO("CloseTrainBtn", panel.transform, "▲ CLOSE");
            closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.8f, 0.92f); closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.97f);
            closeBtn.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 1f);

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

        private static void CreateResponsiveBottomDock(Transform parent, SynthesisView synthView, RosterView rosterView, TrainingView trainingView)
        {
            GameObject dock = CreateGO("BottomDock", parent, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            RectTransform dockRect = dock.GetComponent<RectTransform>();
            dockRect.anchorMin = new Vector2(0, 0); dockRect.anchorMax = new Vector2(1, 0.15f);
            dockRect.offsetMin = new Vector2(15, 15); dockRect.offsetMax = new Vector2(-15, -10);
            HorizontalLayoutGroup layout = dock.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8); layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = true;
            layout.childControlWidth = true; layout.childControlHeight = true; 

            GameObject rosterBtn = CreateDockButton(dock.transform, "ROSTER", null);
            GameObject synthBtn = CreateDockButton(dock.transform, "SYNTH", null);
            GameObject trainBtn = CreateDockButton(dock.transform, "TRAIN", null);
            GameObject summonBtn = CreateDockButton(dock.transform, "SUMMON", null);
            GameObject resetBtn = CreateDockButton(dock.transform, "RESET", null);

            UnityEditor.Events.UnityEventTools.AddPersistentListener(rosterBtn.GetComponent<Button>().onClick, rosterView.Show);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(synthBtn.GetComponent<Button>().onClick, synthView.Show);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(trainBtn.GetComponent<Button>().onClick, trainingView.Show);
            summonBtn.AddComponent<SummonButton>();
            ResetGameButton resetScript = resetBtn.AddComponent<ResetGameButton>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(resetBtn.GetComponent<Button>().onClick, resetScript.ResetGame);
        }

        private static GameObject CreateDockButton(Transform parent, string label, SynthesisView synthView)
        {
            GameObject btnObj = CreateGO($"{label}Btn", parent, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.sizeDelta = Vector2.zero; rect.anchoredPosition = Vector2.zero;
            btnObj.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.9f, 1f); 
            GameObject txtObj = CreateTextGO("Text", btnObj.transform, label, 100, TextAnchor.MiddleCenter);
            Text txt = txtObj.GetComponent<Text>(); txt.resizeTextForBestFit = true; txt.resizeTextMinSize = 35; txt.resizeTextMaxSize = 120;
            RectTransform textRect = txtObj.GetComponent<RectTransform>(); textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero; textRect.offsetMax = Vector2.zero;
            return btnObj;
        }

        #endregion
        #region Helpers
        private static GameObject CreateGO(string name, Transform parent, params System.Type[] components) { GameObject obj = new GameObject(name, components); Undo.RegisterCreatedObjectUndo(obj, "Create " + name); if (parent != null) obj.transform.SetParent(parent, false); return obj; }
        private static GameObject CreateScrollView(Transform parent) { GameObject scroll = CreateGO("ScrollView", parent, typeof(RectTransform), typeof(Image), typeof(ScrollRect)); StretchRect(scroll.GetComponent<RectTransform>()); scroll.GetComponent<RectTransform>().offsetMin = new Vector2(20, 440); scroll.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -120); scroll.GetComponent<Image>().color = new Color(0,0,0,0.2f); GameObject viewport = CreateGO("Viewport", scroll.transform, typeof(RectTransform), typeof(Image), typeof(Mask)); StretchRect(viewport.GetComponent<RectTransform>()); viewport.GetComponent<Image>().color = Color.white; viewport.GetComponent<Mask>().showMaskGraphic = false; GameObject content = CreateGO("Content", viewport.transform, typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter)); RectTransform contentRect = content.GetComponent<RectTransform>(); contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = new Vector2(1, 1); contentRect.pivot = new Vector2(0.5f, 1); contentRect.sizeDelta = new Vector2(0, 0); GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(320, 420); grid.spacing = new Vector2(25, 25); grid.padding = new RectOffset(25, 25, 25, 25); grid.childAlignment = TextAnchor.UpperCenter; content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize; ScrollRect sr = scroll.GetComponent<ScrollRect>(); sr.content = contentRect; sr.viewport = viewport.GetComponent<RectTransform>(); sr.horizontal = false; return scroll; }
        private static GameObject CreateTextGO(string name, Transform parent, string defaultText, int fontSize, TextAnchor alignment) { GameObject obj = CreateGO(name, parent, typeof(RectTransform), typeof(Text)); Text txt = obj.GetComponent<Text>(); txt.text = defaultText; txt.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize); txt.fontSize = fontSize; txt.alignment = alignment; txt.color = Color.white; txt.raycastTarget = false; return obj; }
        private static GameObject CreateButtonGO(string name, Transform parent, string label) { GameObject btnObj = CreateGO(name, parent, typeof(RectTransform), typeof(Image), typeof(Button)); btnObj.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 1f); GameObject txtObj = CreateTextGO("Text", btnObj.transform, label, 48, TextAnchor.MiddleCenter); StretchRect(txtObj.GetComponent<RectTransform>()); return btnObj; }
        private static void StretchRect(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        #endregion
    }
}
#endif