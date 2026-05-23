#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using PickMeUp.UI;

namespace PickMeUp.EditorTools
{
    public static class SetupOfflineUI
    {
        [MenuItem("Tools/PickMeUp/Setup Offline Rewards UI (Auto-Wire)")]
        public static void SetupUI()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Hub")
            {
                EditorUtility.DisplayDialog("Error", "Please open the 'Hub' scene first.", "OK");
                return;
            }

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Cleanup old
            foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (obj == null) continue;
                if (obj.name == "OfflineRewardsPanel") Undo.DestroyObjectImmediate(obj);
            }

            // 1. Dark Overlay
            GameObject overlay = CreateGO("OfflineRewardsPanel", canvas.transform, typeof(RectTransform), typeof(Image), typeof(OfflineRewardsView));
            StretchRect(overlay.GetComponent<RectTransform>());
            overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            // 2. Modal Box
            GameObject modal = CreateGO("ModalBox", overlay.transform, typeof(RectTransform), typeof(Image));
            RectTransform modalRect = modal.GetComponent<RectTransform>();
            modalRect.anchorMin = new Vector2(0.2f, 0.2f);
            modalRect.anchorMax = new Vector2(0.8f, 0.8f);
            modalRect.offsetMin = modalRect.offsetMax = Vector2.zero;
            modal.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // 3. Title
            GameObject title = CreateTextGO("TitleText", modal.transform, "WELCOME BACK!", 36, TextAnchor.MiddleCenter);
            title.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.8f);
            title.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1f);

            // 4. Time Away Text
            GameObject timeText = CreateTextGO("TimeAwayText", modal.transform, "You were away for...", 24, TextAnchor.MiddleCenter);
            timeText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.65f);
            timeText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.8f);

            // 5. Rewards Text
            GameObject rewardsText = CreateTextGO("RewardsText", modal.transform, "Floors: 0\nGold: 0\nXP: 0", 28, TextAnchor.MiddleCenter);
            rewardsText.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f, 0.25f);
            rewardsText.GetComponent<RectTransform>().anchorMax = new Vector2(0.9f, 0.65f);

            // 6. Collect Button
            GameObject btn = CreateButtonGO("CollectBtn", modal.transform, "COLLECT REWARDS");
            RectTransform btnRect = btn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.25f, 0.05f);
            btnRect.anchorMax = new Vector2(0.75f, 0.2f);
            btnRect.offsetMin = btnRect.offsetMax = Vector2.zero;
            btn.GetComponent<Image>().color = new Color(0.1f, 0.6f, 0.1f, 1f); // Green

            // Wire it up
            OfflineRewardsView view = overlay.GetComponent<OfflineRewardsView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("panelRoot").objectReferenceValue = overlay;
            so.FindProperty("TimeAwayText").objectReferenceValue = timeText.GetComponent<Text>();
            so.FindProperty("RewardsText").objectReferenceValue = rewardsText.GetComponent<Text>();
            so.FindProperty("CollectButton").objectReferenceValue = btn.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(canvas.gameObject);
            Debug.Log("<color=green>[SetupOfflineUI] Welcome Back popup created and wired!</color>");
        }

        #region Helpers
        private static GameObject CreateGO(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, components);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
            if (parent != null) obj.transform.SetParent(parent, false);
            return obj;
        }
        private static GameObject CreateTextGO(string name, Transform parent, string text, int size, TextAnchor align)
        {
            GameObject obj = CreateGO(name, parent, typeof(RectTransform), typeof(Text));
            Text t = obj.GetComponent<Text>();
            t.text = text; t.font = Font.CreateDynamicFontFromOSFont("Arial", size); t.fontSize = size; t.alignment = align; t.color = Color.white;
            t.raycastTarget = false;
            return obj;
        }
        private static GameObject CreateButtonGO(string name, Transform parent, string label)
        {
            GameObject btn = CreateGO(name, parent, typeof(RectTransform), typeof(Image), typeof(Button));
            GameObject txt = CreateTextGO("Text", btn.transform, label, 24, TextAnchor.MiddleCenter);
            StretchRect(txt.GetComponent<RectTransform>());
            return btn;
        }
        private static void StretchRect(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        #endregion
    }
}
#endif