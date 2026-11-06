using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientRowUI : MonoBehaviour
{
    [Header("Row UI")]
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TMP_Text ingredientNameText;
    [SerializeField] private TMP_Text countText;

    private IngredientData ingredientData;
    private Inventory1 subscribedInventory;

    public void Initialize(IngredientData data)
    {
        // Remove an earlier subscription if the row is reused.
        Unsubscribe();

        ingredientData = data;
        Debug.Log(
    $"DYNAMIC ROW INITIALIZED | " +
    $"Row: {name} | " +
    $"Asset: {ingredientData.name} | " +
    $"Ingredient: {ingredientData.ingredientName}"
);

        if (ingredientData == null)
        {
            Debug.LogError(
                "IngredientRowUI: IngredientData is missing.",
                this
            );
            return;
        }

        // Set the ingredient name.
        if (ingredientNameText != null)
        {
            ingredientNameText.text =
                ingredientData.ingredientName;
        }

        // Set the ingredient icon.
        if (ingredientIcon != null)
        {
            ingredientIcon.sprite = ingredientData.icon;
            ingredientIcon.enabled = ingredientData.icon != null;
        }

        // Connect to the existing inventory.
        subscribedInventory = Inventory1.Instance;

        if (subscribedInventory == null)
        {
            Debug.LogError(
                "IngredientRowUI: Inventory1.Instance was not found.",
                this
            );
            return;
        }

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
        Debug.Log(
            $"DYNAMIC ROW EVENT | " +
            $"Row ingredient: {ingredientData.ingredientName} | " +
            $"Changed ingredient: {changedIngredient.ingredientName} | " +
            $"New count: {newCount}"
        );

        if (changedIngredient != ingredientData)
            return;

        UpdateCount(newCount);
    }
    private void UpdateCount(int amount)
    {
        if (countText != null)
        {
            countText.text = amount.ToString();

            Debug.Log(
                $"COUNT DISPLAY UPDATED | " +
                $"Row: {ingredientData.ingredientName} | " +
                $"Text Object: {countText.name} | " +
                $"Displayed Value: {countText.text}",
                this
            );
        }
        else
        {
            Debug.LogError(
                $"Count Text is missing for {ingredientData.ingredientName}",
                this
            );
        }
    }

    private void Unsubscribe()
    {
        if (subscribedInventory != null)
        {
            subscribedInventory.OnChanged -= HandleInventoryChanged;
            subscribedInventory = null;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}