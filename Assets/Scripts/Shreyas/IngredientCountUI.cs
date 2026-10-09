using TMPro;
using UnityEngine;

public class IngredientCountUI : MonoBehaviour
{
    [Header("Ingredient")]
    [SerializeField] private IngredientData ingredientData;

    [Header("UI")]
    [SerializeField] private TMP_Text countText;

    private Inventory1 subscribedInventory;

    private void Start()
    {
        subscribedInventory = Inventory1.Instance;

        if (subscribedInventory == null)
        {
            Debug.LogError(
                "IngredientCountUI: Inventory1.Instance was not found.",
                this
            );
            return;
        }

        if (ingredientData == null)
        {
            Debug.LogError(
                "IngredientCountUI: IngredientData is not assigned.",
                this
            );
            return;
        }

        if (countText == null)
        {
            Debug.LogError(
                "IngredientCountUI: Count Text is not assigned.",
                this
            );
            return;
        }

        // Listen for changes to the real inventory.
        subscribedInventory.OnChanged += HandleInventoryChanged;

        // Display the current count immediately.
        UpdateCount(
            subscribedInventory.GetCount(ingredientData)
        );
    }

    private void HandleInventoryChanged(
        IngredientData changedIngredient,
        int newCount)
    {
        // Ignore changes to other ingredient types.
        if (changedIngredient != ingredientData)
            return;

        UpdateCount(newCount);
    }

    private void UpdateCount(int amount)
    {
        if (countText != null)
        {
            countText.text = amount.ToString();
        }
    }

    private void OnDestroy()
    {
        // Stop listening when this UI object is destroyed.
        if (subscribedInventory != null)
        {
            subscribedInventory.OnChanged -= HandleInventoryChanged;
        }
    }
}