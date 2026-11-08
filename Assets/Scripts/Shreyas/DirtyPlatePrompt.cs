using UnityEngine;

// Amber pin for dirty plates.
//   Plate Index = -1     -> any plate (one pin for the whole table)
//   Plate Index = 0, 1.. -> only that plate: one prompt + canvas per plate, same order as the
//                           Bakery Table's "Dirty Plates" list.
// "Only Nearest Plate": the pin shows only while THIS plate is the one the player is "at".
// "Use Facing": when the player stands in the middle (two plates almost equally far), the plate
// he is facing wins, so facing left shows the left plate's pin and facing right shows the right one.
public class DirtyPlatePrompt : ProximityPromptBase
{
    [Header("References")]
    [SerializeField] private BakeryTable table;

    [Header("Which plate? (-1 = any plate)")]
    [SerializeField] private int plateIndex = -1;

    [Header("Nearest plate wins")]
    [SerializeField] private bool onlyNearestPlate = true;

    [Header("Facing (when standing between plates)")]
    [Tooltip("In the middle of the table, the plate the player is facing gets the prompt.")]
    [SerializeField] private bool useFacing = true;

    [Tooltip("How close in distance two plates must be to count as 'the middle'. Raise it so facing decides more often.")]
    [SerializeField] private float middleZone = 0.6f;

    protected override bool ConditionMet
    {
        get
        {
            if (table == null) return false;
            if (plateIndex < 0) return table.HasDirtyPlates;
            if (!table.IsPlateDirty(plateIndex)) return false;
            if (!onlyNearestPlate) return true;

            Transform p = PlayerTransform;
            if (p == null) return false;

            int chosen = useFacing
                ? table.PreferredDirtyPlateIndex(p.position, p.forward, middleZone)
                : table.NearestDirtyPlateIndex(p.position);

            return chosen == plateIndex;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (table == null)
            Debug.LogError("DirtyPlatePrompt: Table is NOT assigned in Inspector!", this);
    }

    [ContextMenu("Debug: which plate wins?")]
    private void DebugWhichPlate()
    {
        Transform p = PlayerTransform;
        if (table == null || p == null) { Debug.Log("table or player missing", this); return; }

        int preferred = table.PreferredDirtyPlateIndex(p.position, p.forward, middleZone);
        int nearest = table.NearestDirtyPlateIndex(p.position);
        Debug.Log($"{name} (plate {plateIndex}) | dirty={table.IsPlateDirty(plateIndex)} | winner by facing={preferred} | nearest={nearest} | player forward={p.forward}", this);
    }
}