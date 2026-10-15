using System.Collections.Generic;
using UnityEngine;

// One asset that holds all recipes, so any script can ask:
// "give me the recipe for this FoodType".
[CreateAssetMenu(fileName = "SO_RecipeDatabase", menuName = "Tiny Bake House/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    [SerializeField] private RecipeData[] recipes;

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

    // NEW: picks a random dish that actually has a recipe.
    // Returns false if the database is empty.
    public bool TryGetRandomFood(out FoodType food)
    {
        List<FoodType> available = new List<FoodType>();

        foreach (RecipeData recipe in recipes)
        {
            if (recipe != null && !available.Contains(recipe.foodType))
            {
                available.Add(recipe.foodType);
            }
        }

        Debug.Log("RecipeDatabase: " + available.Count + " dishes available: " + string.Join(", ", available));

        if (available.Count == 0)
        {
            food = FoodType.Bread;
            return false;
        }

        food = available[Random.Range(0, available.Count)];
        return true;
    }
}