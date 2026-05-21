// Assets/Scripts/UI/RosterButton.cs
using UnityEngine;
using UnityEngine.UI;

namespace PickMeUp.UI
{
    /// <summary>
    /// Simple button controller to open the Roster UI.
    /// 
    /// === SETUP INSTRUCTIONS ===
    /// 1. In the Hub scene, create a UI Button (Right-click Canvas > UI > Button - Legacy).
    /// 2. Name it "RosterButton" and position it appropriately.
    /// 3. Attach this RosterButton.cs script to the Button GameObject.
    /// 4. Drag the "RosterPanel" GameObject (which has RosterView.cs) into the 'rosterView' field in the Inspector.
    /// ========================
    /// </summary>
    public class RosterButton : MonoBehaviour
    {
        #region Inspector Fields

        [Tooltip("Reference to the RosterView component to control.")]
        [SerializeField] private RosterView rosterView;

        #endregion

        #region Private Fields

        private Button _button;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("[RosterButton] Missing Button component on this GameObject.");
                enabled = false;
                return;
            }

            if (rosterView == null)
            {
                rosterView = FindObjectOfType<RosterView>();
                if (rosterView == null)
                {
                    Debug.LogError("[RosterButton] Could not find RosterView in the scene.");
                    enabled = false;
                    return;
                }
            }

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        #endregion

        #region Event Handlers

        private void OnButtonClicked()
        {
            rosterView.Show();
        }

        #endregion
    }
}