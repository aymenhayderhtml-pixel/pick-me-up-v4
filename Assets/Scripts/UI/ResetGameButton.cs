using UnityEngine;
using UnityEngine.SceneManagement;

namespace PickMeUp.UI
{
    /// <summary>
    /// Handles the Reset button click. Used by the Auto-Wire Editor Tool.
    /// </summary>
    public class ResetGameButton : MonoBehaviour
    {
        public void ResetGame()
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene("Boot");
        }
    }
}