#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.UI;

namespace PickMeUp.EditorTools
{
    public static class SetupHubUI
    {
        [MenuItem("Tools/PickMeUp/Setup Hub UI (Auto-Wire V4)")]
        public static void SetupUI()
        {
            Debug.Log("<color=cyan>[SetupHubUI] Starting V4 (Fixing Raycasts)...</color>");

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Hub")
            {
                EditorUtility.DisplayDialog("Error", "Please open the 'Hub' scene first.", "OK");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // 1. CLEANUP
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;
                if (obj.name == "RosterPanel" || obj.name == "SynthesisPanel" || obj.name == "SynthesisBtn")
                {
                    Undo.DestroyObjectImmediate(obj);
                }
            }

            // FORCE DELETE OLD PREFAB so it rebuilds with the raycast fix
            string prefabPath = "Assets/Prefabs/HeroCardPrefab.prefab";
            if (File.Exists(prefabPath)) {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            if (!Directory.Exists("Assets/Prefabs")) Directory.CreateDirectory("Assets/Prefabs");

            // 2. PREFAB
            GameObject heroCardPrefab = CreateHeroCardPrefab();

            // 3. ROSTER
            RosterView rosterView = CreateRosterPanel(canvas.transform, heroCardPrefab);

            // 4. SYNTHESIS
            SynthesisView synthesisView = CreateSynthesisPanel(canvas.transform, rosterView);

            // 5. TRIGGER BUTTON
            CreateSynthesisButton(canvas.transform, synthesisView);

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("<color=green>[SetupHubUI V4] SUCCESS! Raycasts fixed.</color>");
        }

        #region UI Builders

        private static GameObject CreateHeroCardPrefab()
        {
            string prefabPath = "Assets/Prefabs/HeroCardPrefab.prefab";
            
            GameObject card = CreateGO("HeroCardPrefab", null, typeof(RectTransform), typeof(Image), typeof(HeroCardUI));
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
            Image cardBg = card.GetComponent<Image>();
            cardBg.color = new Color(0.2f, 0.2f, 0.2f, 1f); 

            GameObject btnObj = CreateGO("SelectButton", card.transform, typeof(RectTransform), typeof(Image), typeof(Button));
            StretchRect(btnObj.GetComponent<RectTransform>());
            btnObj.GetComponent<Image>().color = new Color(1, 1, 1, 0); 

            GameObject nameObj = CreateTextGO("NameText", card.transform, "1★ Hero", 16, TextAnchor.UpperCenter);
            nameObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.6f);
            nameObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

            GameObject statsObj = CreateTextGO("StatsText", card.transform, "Lv.1\nHP:100", 14, TextAnchor.LowerCenter);
            statsObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            statsObj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.4f);

            HeroCardUI cardUI = card.GetComponent<HeroCardUI>();
            cardUI.NameText = nameObj.GetComponent<Text>();
            cardUI.StatsText = statsObj.GetComponent<Text>();
            cardUI.SelectionHighlight = cardBg; 

            Button btn = btnObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, cardUI.OnCardClicked);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, prefabPath);
            Undo.DestroyObjectImmediate(card); 
            return prefab;
        }

        private static RosterView CreateRosterPanel(Transform parent, GameObject heroPrefab)
        {
            GameObject panel = CreateGO("RosterPanel", parent, typeof(RectTransform), typeof(Image), typeof(RosterView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0.5f, 1); 
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f); 

            GameObject scrollView = CreateScrollView(panel.transform);
            
            RosterView rosterView = panel.GetComponent<RosterView>();
            SerializedObject so = new SerializedObject(rosterView);
            so.FindProperty("contentParent").objectReferenceValue = scrollView.transform.Find("Viewport/Content");
            so.FindProperty("heroEntryPrefab").objectReferenceValue = heroPrefab;
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.ApplyModifiedProperties();

            return rosterView;
        }

        private static SynthesisView CreateSynthesisPanel(Transform parent, RosterView rosterView)
        {
            GameObject panel = CreateGO("SynthesisPanel", parent, typeof(RectTransform), typeof(Image), typeof(SynthesisView));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(1, 1); 
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0.05f, 1f); 

            GameObject statusObj = CreateTextGO("StatusText", panel.transform, "Select 2 or 3 heroes...", 24, TextAnchor.MiddleCenter);
            StretchRect(statusObj.GetComponent<RectTransform>());
            statusObj.GetComponent<RectTransform>().offsetMin = new Vector2(20, 100);
            statusObj.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -80);

            GameObject btnObj = CreateButtonGO("SynthesizeBtn", panel.transform, "SYNTHESIZE");
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0.05f);
            btnRect.anchorMax = new Vector2(0.8f, 0.15f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;

            GameObject closeBtn = CreateButtonGO("CloseBtn", panel.transform, "X CLOSE");
            RectTransform closeRect = closeBtn.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.7f, 0.9f);
            closeRect.anchorMax = new Vector2(0.95f, 0.96f);
            closeRect.offsetMin = closeRect.offsetMax = Vector2.zero;
            closeBtn.GetComponent<Image>().color = new Color(0.6f, 0.1f, 0.1f, 1f); 

            SynthesisView synthView = panel.GetComponent<SynthesisView>();
            HubView hubView = Object.FindAnyObjectByType<HubView>();

            SerializedObject so = new SerializedObject(synthView);
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("StatusText").objectReferenceValue = statusObj.GetComponent<Text>();
            so.FindProperty("SynthesizeButton").objectReferenceValue = btnObj.GetComponent<Button>();
            so.FindProperty("rosterView").objectReferenceValue = rosterView;
            so.FindProperty("hubView").objectReferenceValue = hubView;
            so.ApplyModifiedProperties();

            UnityEditor.Events.UnityEventTools.AddPersistentListener(closeBtn.GetComponent<Button>().onClick, synthView.Hide);

            return synthView;
        }

        private static void CreateSynthesisButton(Transform parent, SynthesisView synthView)
        {
            GameObject btnObj = CreateButtonGO("SynthesisBtn", parent, "OPEN SYNTHESIS");
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.35f, 0.02f);
            btnRect.anchorMax = new Vector2(0.65f, 0.08f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;

            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnObj.GetComponent<Button>().onClick, synthView.Show);
        }

        #endregion

        #region Helpers

        private static GameObject CreateGO(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, components);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name); 
            if (parent != null) obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject CreateScrollView(Transform parent)
        {
            GameObject scroll = CreateGO("ScrollView", parent, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            StretchRect(scroll.GetComponent<RectTransform>());
            scroll.GetComponent<RectTransform>().offsetMin = new Vector2(10, 10);
            scroll.GetComponent<RectTransform>().offsetMax = new Vector2(-10, -10);
            scroll.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);

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
            grid.cellSize = new Vector2(150, 150);
            grid.spacing = new Vector2(10, 10);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.childAlignment = TextAnchor.UpperCenter;

            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = scroll.GetComponent<ScrollRect>();
            sr.content = contentRect;
            sr.viewport = viewport.GetComponent<RectTransform>();
            sr.horizontal = false;

            return scroll;
        }

        private static GameObject CreateTextGO(string name, Transform parent, string defaultText, int fontSize, TextAnchor alignment)
        {
            GameObject obj = CreateGO(name, parent, typeof(RectTransform), typeof(Text));
            Text txt = obj.GetComponent<Text>();
            txt.text = defaultText;
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize); 
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.color = Color.white;
            
            // CRITICAL FIX: Prevents text from blocking button clicks underneath it!
            txt.raycastTarget = false; 
            
            return obj;
        }

        private static GameObject CreateButtonGO(string name, Transform parent, string label)
        {
            GameObject btnObj = CreateGO(name, parent, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.8f, 1f); 

            GameObject txtObj = CreateTextGO("Text", btnObj.transform, label, 20, TextAnchor.MiddleCenter);
            StretchRect(txtObj.GetComponent<RectTransform>());

            return btnObj;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
#endif