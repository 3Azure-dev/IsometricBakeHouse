using System.Collections.Generic;
using UnityEngine;

public class IngredientInventory : MonoBehaviour
{
    public static IngredientInventory Instance { get; private set; }

    // STATIC on purpose: the Bakery scene loads again after every hunt, and the ingredients
    // the player took from the shelf must not disappear when that happens.
    private static Dictionary<IngredientType, int> ingredients =
        new Dictionary<IngredientType, int>();

    // Makes sure old stock from a previous Play session never leaks in
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStock()
    {
        ingredients = new Dictionary<IngredientType, int>();
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public int GetAmount(IngredientType ingredient)
    {
        if (ingredients.ContainsKey(ingredient))
        {
            return ingredients[ingredient];
        }

        return 0;
    }


    public void AddIngredient(
        IngredientType ingredient,
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!ingredients.ContainsKey(ingredient))
        {
            ingredients.Add(
                ingredient,
                0
            );
        }

        ingredients[ingredient] += amount;

        Debug.Log(
            ingredient +
            " added. Amount: " +
            ingredients[ingredient]
        );
    }


    public bool HasIngredient(
        IngredientType ingredient,
        int amount)
    {
        return GetAmount(ingredient) >= amount;
    }


    public bool RemoveIngredient(
        IngredientType ingredient,
        int amount)
    {
        if (!HasIngredient(ingredient, amount))
        {
            return false;
        }

        ingredients[ingredient] -= amount;

        return true;
    }
}