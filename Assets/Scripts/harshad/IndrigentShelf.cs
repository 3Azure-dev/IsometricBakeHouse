
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

    // STATIC on purpose: the Bakery scene is destroyed and loaded again after every hunt.
    // A normal field would be wiped each time. A static one keeps what is on the shelf.
    private static Dictionary<IngredientType, int> savedStock;

    private Dictionary<IngredientType, int> stock;

    // Makes sure old stock from a previous Play session never leaks in
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSavedStock()
    {
        savedStock = null;
    }

    private void Awake()
    {
        if (savedStock == null)
        {
            // First time the shelf exists in this Play session
            savedStock = new Dictionary<IngredientType, int>();

            // Initialize every ingredient to zero.
            foreach (IngredientType ingredient
                     in System.Enum.GetValues(typeof(IngredientType)))
            {
                savedStock[ingredient] = 0;
            }

            // Apply the temporary test quantities (empty this list in the Inspector for the real game).
            foreach (ShelfIngredient item in startingIngredients)
            {
                savedStock[item.ingredient] = Mathf.Max(0, item.startingAmount);
            }
        }

        stock = savedStock;
    }

    // --------------------------------------------------
    // DEPOSIT (SCRUM-142): move everything the player hunted onto the shelf
    // --------------------------------------------------

    // A short sentence about the last deposit, for the shelf window's message text
    public string LastDepositSummary { get; private set; } = "";

    // Returns how many ingredients were moved
    public int DepositHuntedIngredients()
    {
        LastDepositSummary = "";

        if (Inventory1.Instance == null)
        {
            Debug.Log("Shelf: nothing hunted yet (no hunt inventory found).");
            return 0;
        }

        int moved = 0;
        string summary = "";

        foreach (KeyValuePair<IngredientData, int> item in Inventory1.Instance.GetAllCounts())
        {
            IngredientData hunted = item.Key;
            int amount = item.Value;

            if (hunted == null || amount <= 0)
            {
                continue;
            }

            // The hunt uses IngredientData assets, the shelf uses the IngredientType list.
            // They are matched by NAME (SO_Ingredient_Flour has ingredientName "Flour").
            IngredientType type;

            if (!System.Enum.TryParse(hunted.ingredientName, true, out type))
            {
                Debug.LogWarning("Shelf: '" + hunted.ingredientName + "' is not in the IngredientType list, so it stays in your bag. Add it to IngredientType.");
                continue;
            }

            // Take it out of the player's bag first, then put it on the shelf
            if (!Inventory1.Instance.Remove(hunted, amount))
            {
                continue;
            }

            stock[type] += amount;
            moved += amount;

            summary += (summary.Length > 0 ? ", " : "") + amount + " " + type;

            Debug.Log("Shelf: moved " + amount + " " + type + ". The shelf now holds " + stock[type] + " " + type + ".");
        }

        if (moved == 0)
        {
            Debug.Log("Shelf: the player has nothing to put on the shelf.");
        }
        else
        {
            LastDepositSummary = "Put on the shelf: " + summary;
        }

        return moved;
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
