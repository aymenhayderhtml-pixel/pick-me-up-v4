using UnityEngine;
using UnityEngine.UI;
using PickMeUp.Core;
using PickMeUp.Services; // ADD THIS LINE

namespace PickMeUp.UI
{
    public class OfflineRewardsView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text TimeAwayText;
        [SerializeField] private Text RewardsText;
        [SerializeField] private Button CollectButton;

        private void Start()
        {
            if (CollectButton != null) CollectButton.onClick.AddListener(OnCollectClicked);
            
            // Check if the BootInstaller has pending rewards for us
            if (BootInstaller.PendingOfflineReward != null && BootInstaller.PendingOfflineReward.TimeSimulated.TotalMinutes >= 1)
            {
                Show(BootInstaller.PendingOfflineReward);
            }
            else
            {
                Hide();
            }
        }

        private void Show(IdleReward reward)
        {
            if (panelRoot != null) panelRoot.SetActive(true);

            if (TimeAwayText != null)
            {
                string timeStr = reward.TimeSimulated.TotalHours >= 1 
                    ? $"{reward.TimeSimulated.TotalHours:F1} Hours" 
                    : $"{reward.TimeSimulated.TotalMinutes:F0} Minutes";
                TimeAwayText.text = $"You were away for {timeStr}";
            }

            if (RewardsText != null)
            {
                RewardsText.text = $"Floors Cleared: {reward.FloorsCleared}\n" +
                                   $"Gold Earned: {reward.GoldEarned}\n" +
                                   $"XP Earned: {reward.XpEarned}";
            }
        }

        private void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnCollectClicked()
        {
            BootInstaller.PendingOfflineReward = null; // Clear it so it doesn't show again
            Hide();
        }

        private void OnDestroy()
        {
            if (CollectButton != null) CollectButton.onClick.RemoveListener(OnCollectClicked);
        }
    }
}