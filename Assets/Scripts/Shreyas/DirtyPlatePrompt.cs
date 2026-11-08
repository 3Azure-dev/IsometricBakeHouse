using UnityEngine;

// Shows the amber pin while a dirty plate is on the table and the player is close.
//   Plate Index = -1     -> any plate (one pin for the whole table, the old behaviour)
//   Plate Index = 0, 1.. -> only that plate: use one prompt + canvas per plate, in the same
//                           order as the Bakery Table's "Dirty Plates" list.
public class DirtyPlatePrompt : ProximityPromptBase
{
    [Header("References")]
    [SerializeField] private BakeryTable table;

    [Header("Which plate? (-1 = any plate)")]
    [SerializeField] private int plateIndex = -1;

    public override bool ConditionMet
    {
        get
        {
            if (table == null) return false;
            return plateIndex < 0 ? table.HasDirtyPlates : table.IsPlateDirty(plateIndex);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        if (table == null)
            Debug.LogError("DirtyPlatePrompt: Table is NOT assigned in Inspector!", this);
    }

    // Add this public method so the Manager can check it
    public bool IsReady()
    {
        return ConditionMet;
    }
}