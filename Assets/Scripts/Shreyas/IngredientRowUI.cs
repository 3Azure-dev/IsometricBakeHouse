using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngredientRowUI : MonoBehaviour
{
    [Header("Row UI")]
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TMP_Text ingredientNameText;
    [SerializeField] private TMP_Text countText;

    private IngredientData ingredientData;
    private Inventory1 subscribedInventory;

    [Header("Pop when the count goes up")]
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popTime = 0.1f;

    private int lastCount = -1;
    private Coroutine popRoutine;

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

            if (lastCount >= 0 && amount > lastCount) Pop();
            lastCount = amount;

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

    private void Pop()
    {
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Transform t = countText.transform;
        Vector3 big = Vector3.one * popScale;

        for (float e = 0f; e < popTime; e += Time.unscaledDeltaTime)
        {
            t.localScale = Vector3.Lerp(Vector3.one, big, e / popTime);
            yield return null;
        }
        for (float e = 0f; e < popTime; e += Time.unscaledDeltaTime)
        {
            t.localScale = Vector3.Lerp(big, Vector3.one, e / popTime);
            yield return null;
        }

        t.localScale = Vector3.one;
        popRoutine = null;
    }
}