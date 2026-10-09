using System.Collections.Generic;
using UnityEngine;

public class IngredientInventory : MonoBehaviour
{
    public static IngredientInventory Instance { get; private set; }

    private Dictionary<IngredientType, int> ingredients =
        new Dictionary<IngredientType, int>();


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