using System.Collections.Generic;
using UnityEngine;

// The shelf is real storage.
//  - First E at the shelf each visit: everything in the player's bag (Inventory1) goes onto the shelf.
//  - Take buttons in the shelf window move ingredients from the shelf back into the bag.
//  - Stations (Stove, Oven, Pizza Stone) cook from the bag.
public class IngredientShelf : MonoBehaviour
{
    [System.Serializable]
    public class ShelfIngredient
    {
        public IngredientType ingredient;
        [Min(0)] public int startingAmount = 0;
    }

    [Header("Optional test stock (leave at 0 for the real game)")]
    [SerializeField]
    private List<ShelfIngredient> startingIngredients =
        new List<ShelfIngredient>();

    [Header("Drag every SO_Ingredient_* asset here (so Take knows what to give)")]
    [SerializeField] private IngredientData[] knownIngredients;

    // STATIC on purpose: the Bakery scene is destroyed and loaded again after every hunt.
    private static Dictionary<IngredientType, int> savedStock;
    private static Dictionary<IngredientType, IngredientData> dataByType =
        new Dictionary<IngredientType, IngredientData>();

    private Dictionary<IngredientType, int> stock;

    // New scene load = new visit, so this resets after every hunt
    private bool depositedThisVisit;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSavedStock()
    {
        savedStock = null;
        dataByType = new Dictionary<IngredientType, IngredientData>();
    }

    private void Awake()
    {
        if (savedStock == null)
        {
            savedStock = new Dictionary<IngredientType, int>();

            foreach (IngredientType ingredient
                     in System.Enum.GetValues(typeof(IngredientType)))
            {
                savedStock[ingredient] = 0;
            }

            foreach (ShelfIngredient item in startingIngredients)
            {
                savedStock[item.ingredient] = Mathf.Max(0, item.startingAmount);
            }
        }

        stock = savedStock;

        // Remember which asset belongs to which type
        if (knownIngredients != null)
        {
            foreach (IngredientData data in knownIngredients)
            {
                if (data != null && TryGetType(data.ingredientName, out IngredientType type))
                {
                    dataByType[type] = data;
                }
            }
        }
    }

    // "Tomato Sauce" = TomatoSauce, "Seed" = Seeds
    private static string Normalize(string text)
    {
        string result = text.Replace(" ", "").ToLowerInvariant();

        if (result.EndsWith("s"))
        {
            result = result.Substring(0, result.Length - 1);
        }

        return result;
    }

    private static bool TryGetType(string ingredientName, out IngredientType type)
    {
        string wanted = Normalize(ingredientName);

        foreach (IngredientType candidate in System.Enum.GetValues(typeof(IngredientType)))
        {
            if (Normalize(candidate.ToString()) == wanted)
            {
                type = candidate;
                return true;
            }
        }

        type = default;
        return false;
    }

    // --------------------------------------------------
    // PUT ON THE SHELF
    // --------------------------------------------------

    public string LastDepositSummary { get; private set; } = "";

    // Called on every E at the shelf. Only the first E of each visit really deposits,
    // so the things you just took are not put straight back.
    public int DepositOncePerVisit()
    {
        if (depositedThisVisit)
        {
            LastDepositSummary = "";
            return 0;
        }

        depositedThisVisit = true;
        return DepositHuntedIngredients();
    }

    public int DepositHuntedIngredients()
    {
        LastDepositSummary = "";

        if (Inventory1.Instance == null)
        {
            return 0;
        }

        int moved = 0;
        string summary = "";

        foreach (KeyValuePair<IngredientData, int> item in Inventory1.Instance.GetAllCounts())
        {
            IngredientData data = item.Key;
            int amount = item.Value;

            if (data == null || amount <= 0)
            {
                continue;
            }

            if (!TryGetType(data.ingredientName, out IngredientType type))
            {
                Debug.LogWarning("Shelf: '" + data.ingredientName + "' is not in the IngredientType list, so it stays in your bag.");
                continue;
            }

            if (!Inventory1.Instance.Remove(data, amount))
            {
                continue;
            }

            stock[type] += amount;
            dataByType[type] = data;
            moved += amount;

            summary += (summary.Length > 0 ? ", " : "") + amount + " " + type;
        }

        if (moved > 0)
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

    // --------------------------------------------------
    // TAKE FROM THE SHELF INTO THE BAG
    // --------------------------------------------------

    public bool TakeIngredient(IngredientType ingredient, int amount = 1)
    {
        if (amount <= 0)
            return false;

        if (GetAmount(ingredient) < amount)
        {
            Debug.Log("Not enough " + ingredient + " on the shelf.");
            return false;
        }

        if (Inventory1.Instance == null)
        {
            Debug.LogError("Inventory1 (the ingredient bag) was not found.");
            return false;
        }

        if (!dataByType.TryGetValue(ingredient, out IngredientData data) || data == null)
        {
            Debug.LogError("Shelf: no IngredientData asset known for " + ingredient +
                           ". Drag the SO_Ingredient_" + ingredient + " asset into 'Known Ingredients' on the shelf.");
            return false;
        }

        stock[ingredient] -= amount;
        Inventory1.Instance.Add(data, amount);

        Debug.Log("Took " + amount + " " + ingredient + " into your bag. Shelf remaining: " + stock[ingredient]);

        return true;
    }
}