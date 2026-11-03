using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpansionPlotManager : MonoBehaviour
{
    // This holds the ScriptableObject (for costs) AND the Scene objects (walls/floors)
    [System.Serializable]
    public class PlotSceneData
    {
        public PlotDataSO plotData;       // Drag ScriptableObject here
        public GameObject blocker;        // Drag PlotBlocker here
        public GameObject expansionFloor; // Drag ExpansionFloor here
        [HideInInspector] public bool isUnlocked = false;
    }

    [Header("Plot Configuration")]
    [Tooltip("Drag in the 3 ScriptableObjects, Blockers, and Floors")]
    [SerializeField] private List<PlotSceneData> plots = new List<PlotSceneData>();

    private void Awake()
    {
        // Sort plots by their plot number just in case
        plots.Sort((x, y) => x.plotData.plotNumber.CompareTo(y.plotData.plotNumber));
    }

    // Call this when the player buys a plot
    public bool BuyPlot(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;

        PlotSceneData targetPlot = plots[plotNumber - 1];

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

        // 3. Spend Diamonds (Notice we use .plotData.costInDiamonds now)
        if (Wallet.Instance == null || !Wallet.Instance.SpendDiamonds(targetPlot.plotData.costInDiamonds))
        {
            Debug.Log("Not enough diamonds!");
            return false;
        }

        // 4. Unlock the plot!
        UnlockPlot(targetPlot);
        return true;
    }

    private void UnlockPlot(PlotSceneData plot)
    {
        plot.isUnlocked = true;
        StartCoroutine(AnimateExpansion(plot));
    }

    // The cool shrinking/growing animation your designer asked for!
    private IEnumerator AnimateExpansion(PlotSceneData plot)
    {
        if (plot.expansionFloor == null || plot.blocker == null) yield break;

        Transform floorT = plot.expansionFloor.transform;
        Transform blockerT = plot.blocker.transform;

        Vector3 targetFloorScale = floorT.localScale;
        Vector3 targetBlockerScale = blockerT.localScale;

        // Start floor invisible, wall fully visible
        floorT.localScale = Vector3.zero;

        float duration = 0.6f; // Animation takes 0.6 seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Floor grows from 0 to full size
            floorT.localScale = Vector3.Lerp(Vector3.zero, targetFloorScale, t);

            // Wall shrinks from full size to 0
            blockerT.localScale = Vector3.Lerp(targetBlockerScale, Vector3.zero, t);

            yield return null;
        }

        // Clean up
        floorT.localScale = targetFloorScale;
        plot.blocker.SetActive(false);

        Debug.Log($"Plot {plot.plotData.plotNumber} unlocked! Floor space expanded.");
    }

    // Helper for the Shop UI to check if a plot is bought
    public bool IsPlotUnlocked(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;
        return plots[plotNumber - 1].isUnlocked;
    }

    // UI Button Wrappers
    public void BuyPlot1() => BuyPlot(1);
    public void BuyPlot2() => BuyPlot(2);
    public void BuyPlot3() => BuyPlot(3);
}