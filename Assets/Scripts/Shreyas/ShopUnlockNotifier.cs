using UnityEngine;
using System.Collections.Generic;

public class ShopUnlockNotifier : MonoBehaviour
{
    [System.Serializable]
    public class UnlockRule
    {
        [Tooltip("The level required to unlock this item/tab")]
        public int requiredLevel;

        [Tooltip("The tab that should show the red dot")]
        public TabNewIndicator targetTab;
    }

    [Header("Unlock Rules")]
    [Tooltip("Set up which level unlocks which tab")]
    [SerializeField] private List<UnlockRule> rules = new List<UnlockRule>();

    private void OnEnable()
    {
        // Listen to the main 'Changed' event in BakeryProgress
        if (BakeryProgress.Instance != null)
        {
            BakeryProgress.Instance.Changed += OnProgressChanged;
        }
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        if (BakeryProgress.Instance != null)
        {
            BakeryProgress.Instance.Changed -= OnProgressChanged;
        }
    }

    private void OnProgressChanged(ProgressChange changeType)
    {
        // We only care if the Level went up or a Recipe was unlocked
        if (changeType == ProgressChange.Level || changeType == ProgressChange.Recipe)
        {
            int currentLevel = BakeryProgress.Instance.Level;

            foreach (var rule in rules)
            {
                // If the player's current level meets the requirement, show the dot!
                if (currentLevel >= rule.requiredLevel && rule.targetTab != null)
                {
                    rule.targetTab.ShowDot();
                    Debug.Log("Showing 'New' dot on tab for Level " + rule.requiredLevel);
                }
            }
        }
    }
}