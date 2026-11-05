using UnityEngine;

// Stove prompt:
//   idle     -> "Cook"       (E opens the cooking menu)
//   cooking  -> hidden
//   ready    -> "Take food"  (E takes the dish)
// StoveController.Interact() already handles each state, so wire it once in On Interact ().
public class FoodReadyPrompt : ProximityPromptBase
{
    [Header("References")]
    [SerializeField] private StoveController stoveController;

    [Header("Labels")]
    [SerializeField] private string cookLabel = "Cook";
    [SerializeField] private string takeFoodLabel = "Take food";

    protected override bool ConditionMet =>
        stoveController != null && !stoveController.IsCooking;

    protected override string CurrentLabel =>
        stoveController != null && stoveController.IsFoodReady ? takeFoodLabel : cookLabel;

    protected override void Awake()
    {
        base.Awake();

        if (stoveController == null)
            Debug.LogError("FoodReadyPrompt: Stove Controller is NOT assigned in Inspector!", this);
    }
}