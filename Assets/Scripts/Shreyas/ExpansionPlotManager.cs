using System.Collections.Generic;
using UnityEngine;

public class ExpansionPlotManager : MonoBehaviour
{
    [System.Serializable]
    public class PlotData
    {
        public int plotNumber;          // 1, 2, or 3
        public int costInDiamonds;      // 200, 250, 300
        public GameObject blocker;      // The wall/fence in the scene
        [HideInInspector] public bool isUnlocked = false;
    }

    [Header("Plot Configuration")]
    [Tooltip("Drag in the 3 blockers and set their costs")]
    [SerializeField] private List<PlotData> plots = new List<PlotData>();

    private void Awake()
    {
        // Sort plots just in case they get mixed up in the inspector
        plots.Sort((x, y) => x.plotNumber.CompareTo(y.plotNumber));
    }

    // Call this when the player buys a plot
    public bool BuyPlot(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;

        PlotData targetPlot = plots[plotNumber - 1];

        // 1. Check if the previous plot is unlocked (Order enforcement)
        if (plotNumber > 1 && !plots[plotNumber - 2].isUnlocked)
        {
            Debug.Log($"You must buy Plot {plotNumber - 1} first!");
            return false;
        }

        // 2. Check if already unlocked
        if (targetPlot.isUnlocked)
        {
            Debug.Log("Plot already unlocked!");
            return false;
        }

        // 3. Spend Diamonds
        if (Wallet.Instance == null || !Wallet.Instance.SpendDiamonds(targetPlot.costInDiamonds))
        {
            Debug.Log("Not enough diamonds!");
            return false;
        }

        // 4. Unlock the plot!
        UnlockPlot(targetPlot);
        return true;
    }

    private void UnlockPlot(PlotData plot)
    {
        plot.isUnlocked = true;
        Debug.Log("DEBUG: Unlocking plot " + plot.plotNumber);

        if (plot.blocker != null)
        {
            Debug.Log("DEBUG: Blocker found, disabling it...");
            plot.blocker.SetActive(false);
        }
        else
        {
            Debug.LogError("DEBUG: Blocker is NULL! Did you assign it in the Inspector?");
        }

        Debug.Log($"Plot {plot.plotNumber} unlocked! Floor space expanded.");
    }

    // --------------------------------------------------
    // UI BUTTON WRAPPERS (These show up in the On Click list!)
    // --------------------------------------------------
    public void BuyPlot1() => BuyPlot(1);
    public void BuyPlot2() => BuyPlot(2);
    public void BuyPlot3() => BuyPlot(3);

    // This method lets the Shop UI check if a plot is already bought
    public bool IsPlotUnlocked(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;

        return plots[plotNumber - 1].isUnlocked;
    }

    // --------------------------------------------------
    // TEST BUTTONS (For Demos)
    // --------------------------------------------------
    [ContextMenu("TEST: Buy Plot 1 (200 Diamonds)")]
    private void TestBuyPlot1() => BuyPlot(1);

    [ContextMenu("TEST: Buy Plot 2 (250 Diamonds)")]
    private void TestBuyPlot2() => BuyPlot(2);

    [ContextMenu("TEST: Buy Plot 3 (300 Diamonds)")]
    private void TestBuyPlot3() => BuyPlot(3);
}