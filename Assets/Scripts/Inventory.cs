using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory1 : MonoBehaviour
{
    // Singleton: one Inventory, reachable from anywhere
    public static Inventory1 Instance { get; private set; }

    // Observer: UI and shelf listen to this to know when a count changes
    public event Action<IngredientData, int> OnChanged;

    private readonly Dictionary<IngredientData, int> counts = new();

    void Awake()
    {
        // If one already exists, delete this duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // survive scene changes
    }

    public void Add(IngredientData ingredient, int amount = 1)
    {
        counts[ingredient] = GetCount(ingredient) + amount;
        OnChanged?.Invoke(ingredient, counts[ingredient]);
    }

    // Returns false if there is not enough
    public bool Remove(IngredientData ingredient, int amount = 1)
    {
        if (GetCount(ingredient) < amount) return false;

        counts[ingredient] -= amount;
        OnChanged?.Invoke(ingredient, counts[ingredient]);
        return true;
    }

    public int GetCount(IngredientData ingredient)
    {
        return counts.TryGetValue(ingredient, out int count) ? count : 0;
    }


    // True if the player has 1 of every ingredient the recipe needs
    public bool CanCook(RecipeData recipe)
    {
        foreach (IngredientData ingredient in recipe.huntedIngredients)
        {
            if (GetCount(ingredient) < 1) return false;
        }
        return true;
    }

    // Removes the ingredients. Returns false and removes nothing if something is missing
    public bool UseIngredients(RecipeData recipe)
    {
        if (!CanCook(recipe)) return false;

        foreach (IngredientData ingredient in recipe.huntedIngredients)
        {
            Remove(ingredient);
        }
        return true;
    }
    public int GetTotalIngredientCount()
    {
        int total = 0;

        foreach (int count in counts.Values)
        {
            total += count;
        }

        return total;
    }
}