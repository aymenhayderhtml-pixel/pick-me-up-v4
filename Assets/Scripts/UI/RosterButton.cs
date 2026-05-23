using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Added for FirstOrDefault

namespace PickMeUp.UI
{
    public class RosterButton : MonoBehaviour
    {
        [SerializeField] private RosterView rosterView;
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
                enabled = false;
                return;
            }

            if (rosterView == null)
            {
                // FIX: FindObjectsInactive.Include forces Unity to find the RosterView 
                // even though it hidden itself on startup!
                rosterView = FindObjectsByType<RosterView>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
                
                if (rosterView == null)
                {
                    Debug.LogError("[RosterButton] Could not find RosterView in the scene.");
                    enabled = false;
                    return;
                }
            }

            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            rosterView.Show();
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}