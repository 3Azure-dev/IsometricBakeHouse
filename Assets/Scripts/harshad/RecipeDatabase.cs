using System.Collections.Generic;
using UnityEngine;

// One asset that holds all recipes, so any script can ask:
// "give me the recipe for this FoodType".
[CreateAssetMenu(fileName = "SO_RecipeDatabase", menuName = "Tiny Bake House/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [SerializeField] private RecipeData[] recipes;

    public IReadOnlyList<RecipeData> Recipes => recipes;

    public RecipeData GetRecipe(FoodType food)
    {
        foreach (RecipeData recipe in recipes)
        {
            if (recipe != null && recipe.foodType == food)
            {
                return recipe;
            }
        }

        return null;
    }

    // Picks a random dish that actually has a recipe.
    // Returns false if the database is empty.
    // Picks a random dish for a customer to order.
    // GDD 1.6: each customer orders one dish, chosen randomly (equal chance)
    // from the recipes the player has UNLOCKED. A recipe counts only if it
    // has a recipe asset here AND the player has unlocked it (BakeryProgress).
    // Returns false if nothing is available.
    public bool TryGetRandomFood(out FoodType food)
    {
        List<FoodType> available = new List<FoodType>();

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
            {
                continue;
            }

            if (available.Contains(recipe.foodType))
            {
                continue;
            }

            if (IsUnlocked(recipe.foodType))
            {
                available.Add(recipe.foodType);
            }
        }

        if (available.Count == 0)
        {
            Debug.LogWarning("RecipeDatabase: no unlocked recipe has a recipe asset. Check the recipes list.");

            food = FoodType.Bread;
            return false;
        }

        food = available[Random.Range(0, available.Count)];

        Debug.Log("RecipeDatabase: " + available.Count + " unlocked dish(es), picked " + food);

        return true;
    }

    // Bread is unlocked from the start (BakeryProgress adds it).
    // If BakeryProgress is missing for some reason, do not lock anything.
    private bool IsUnlocked(FoodType food)
    {
        if (BakeryProgress.Instance == null)
        {
            return true;
        }

        return BakeryProgress.Instance.IsRecipeUnlocked(food);
    }
}