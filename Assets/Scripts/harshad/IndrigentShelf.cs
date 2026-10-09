
using System.Collections.Generic;
using UnityEngine;

public class IngredientShelf : MonoBehaviour
{
    [System.Serializable]
    public class ShelfIngredient
    {
        public IngredientType ingredient;
        [Min(0)] public int startingAmount = 5;
    }

    [Header("Temporary Test Stock")]
    [SerializeField]
    private List<ShelfIngredient> startingIngredients =
        new List<ShelfIngredient>();

    private Dictionary<IngredientType, int> stock =
        new Dictionary<IngredientType, int>();

    private void Awake()
    {
        // Initialize every ingredient to zero.
        foreach (IngredientType ingredient
                 in System.Enum.GetValues(typeof(IngredientType)))
        {
            stock[ingredient] = 0;
        }

        // Apply the temporary test quantities.
        foreach (ShelfIngredient item in startingIngredients)
        {
            stock[item.ingredient] = Mathf.Max(0, item.startingAmount);
        }
    }

    public int GetAmount(IngredientType ingredient)
    {
        return stock.TryGetValue(ingredient, out int amount)
            ? amount
            : 0;
    }

    public bool TakeIngredient(
        IngredientType ingredient,
        int amount = 1)
    {
        if (amount <= 0)
            return false;

        if (GetAmount(ingredient) < amount)
        {
            Debug.Log("Not enough " + ingredient + " on the shelf.");
            return false;
        }

        if (IngredientInventory.Instance == null)
        {
            Debug.LogError("IngredientInventory was not found.");
            return false;
        }

        // Transfer only if the player's inventory can receive it.
        stock[ingredient] -= amount;

        IngredientInventory.Instance.AddIngredient(
            ingredient,
            amount
        );

        Debug.Log(
            "Took " + amount + " " + ingredient +
            ". Shelf remaining: " + stock[ingredient]
        );

        return true;
    }
}
