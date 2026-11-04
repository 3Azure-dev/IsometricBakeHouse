using UnityEngine;

// Shows the [E] prompt above the stove while food is ready and the player is close enough.
public class FoodReadyPrompt : ProximityPromptBase
{
    [Header("References")]
    [SerializeField] private StoveController stoveController;

    protected override bool ConditionMet => stoveController != null && stoveController.IsFoodReady;

    protected override void Awake()
    {
        base.Awake();

        if (stoveController == null)
            Debug.LogError("FoodReadyPrompt: Stove Controller is NOT assigned in Inspector!", this);
    }
}
