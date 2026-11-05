using UnityEngine;

// Shows the [E] prompt above the table while it has dirty plates and the player is close enough.
public class DirtyPlatePrompt : ProximityPromptBase
{
    [Header("References")]
    [SerializeField] private BakeryTable table;

    protected override bool ConditionMet => table != null && table.HasDirtyPlates;

    protected override void Awake()
    {
        base.Awake();

        if (table == null)
            Debug.LogError("DirtyPlatePrompt: Table is NOT assigned in Inspector!", this);
    }
}