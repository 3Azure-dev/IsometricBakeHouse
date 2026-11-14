using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;


public class ExpansionPlotManager : MonoBehaviour
{
    [System.Serializable]
    public class PlotSceneData
    {
        public PlotDataSO plotData;
        public GameObject blocker;
        public GameObject expansionFloor;
        [Tooltip("Drag the parent group of your new walls here (e.g., Walls_Plot1)")]
        public GameObject newWalls;
        [HideInInspector] public bool isUnlocked = false;
    }

    [Header("Plot Configuration")]
    [Tooltip("Drag in the 3 ScriptableObjects, Blockers, Floors, and New Wall Groups")]
    [SerializeField] private List<PlotSceneData> plots = new List<PlotSceneData>();

    private void Awake()
    {
        // SAFETY CHECK: Prevents crashes if Unity wipes the inspector slots
        if (plots.Count == 0 || plots[0].plotData == null)
        {
            Debug.LogWarning("ExpansionPlotManager: Inspector slots are empty! Please re-assign them.");
            return;
        }

        // Sort plots by number just in case
        plots.Sort((x, y) => x.plotData.plotNumber.CompareTo(y.plotData.plotNumber));

        // INITIAL SETUP: Hide floors and new walls, ensure blockers are visible
        foreach (var plot in plots)
        {
            if (!plot.isUnlocked)
            {
                if (plot.expansionFloor != null) plot.expansionFloor.SetActive(false);
                if (plot.newWalls != null) plot.newWalls.SetActive(false); // Hides the whole group of walls!
                if (plot.blocker != null) plot.blocker.SetActive(true);
            }
        }
    }

    public bool BuyPlot(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;

        PlotSceneData targetPlot = plots[plotNumber - 1];

        // 1. Check order enforcement
        if (plotNumber > 1 && !plots[plotNumber - 2].isUnlocked)
        {
            Debug.Log($"You must buy Plot {plotNumber - 1} first!");
            return false;
        }

        // 2. Check if already unlocked
        if (targetPlot.isUnlocked) return false;

        // 3. Spend Diamonds
        if (Wallet.Instance == null || !Wallet.Instance.SpendDiamonds(targetPlot.plotData.costInDiamonds))
        {
            Debug.Log("Not enough diamonds!");
            return false;
        }

        // 4. Unlock and Animate!
        UnlockPlot(targetPlot);
        return true;
    }

    private void UnlockPlot(PlotSceneData plot)
    {
        plot.isUnlocked = true;
        StartCoroutine(AnimateExpansion(plot));
    }

    private IEnumerator AnimateExpansion(PlotSceneData plot)
    {
        if (plot.expansionFloor == null || plot.blocker == null) yield break;

        Transform floorT = plot.expansionFloor.transform;
        Transform blockerT = plot.blocker.transform;

        // 1. ENABLE the floor and the new walls group
        plot.expansionFloor.SetActive(true);
        if (plot.newWalls != null) plot.newWalls.SetActive(true); // Turns on all walls in the group!

        Vector3 targetFloorScale = floorT.localScale;
        Vector3 targetBlockerScale = blockerT.localScale;

        // 2. Start floor at ZERO size
        floorT.localScale = Vector3.zero;

        float duration = 0.6f;
        float elapsed = 0f;

        // 3. LERP ANIMATION
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Floor smoothly grows
            floorT.localScale = Vector3.Lerp(Vector3.zero, targetFloorScale, t);
            // Old wall smoothly shrinks
            blockerT.localScale = Vector3.Lerp(targetBlockerScale, Vector3.zero, t);

            yield return null;
        }

        // ... existing code ...

        // 4. Clean up
        floorT.localScale = targetFloorScale;
        plot.blocker.SetActive(false);

        // --- ADD THIS: Rebuild NavMesh so customers can walk into the new area ---
        NavMeshSurface surface = FindAnyObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }

        Debug.Log($"Plot {plot.plotData.plotNumber} unlocked! Floor and walls expanded.");
    }

    // Helper for the Shop UI
    public bool IsPlotUnlocked(int plotNumber)
    {
        if (plotNumber < 1 || plotNumber > plots.Count) return false;
        return plots[plotNumber - 1].isUnlocked;
    }

    public void BuyPlot1() => BuyPlot(1);
    public void BuyPlot2() => BuyPlot(2);
    public void BuyPlot3() => BuyPlot(3);
}