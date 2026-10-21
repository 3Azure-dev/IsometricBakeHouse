using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [SerializeField]
    private List<Recipe> recipes = new List<Recipe>();

    [Header("Cupcake Settings")]
    [SerializeField] private FoodType cupcakeRecipeFoodType;

    private readonly HashSet<FoodType> unlockedRecipes =
        new HashSet<FoodType>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Destroy only this extra COMPONENT, never its whole GameObject.
            // (The Bakery scene has a second RecipeManager sitting on the CustomerManager object;
            // destroying the GameObject would have deleted the CustomerManager and the spawner with it.)
            Debug.LogWarning("RecipeManager: a second RecipeManager exists on '" + gameObject.name + "'. Remove the extra one in the scene.");
            Destroy(this);
            return;
        }

        Instance = this;

        // Load the recipes that are available by default.
        foreach (Recipe recipe in recipes)
        {
            if (recipe != null && recipe.unlockedByDefault)
            {
                unlockedRecipes.Add(recipe.foodType);
            }
        }
    }

    public Recipe GetRecipe(FoodType foodType)
    {
        // Don't return recipes that are still locked.
        if (!unlockedRecipes.Contains(foodType))
        {
            return null;
        }

        foreach (Recipe recipe in recipes)
        {
            if (recipe != null && recipe.foodType == foodType)
            {
                return recipe;
            }
        }

        return null;
    }

    public bool IsRecipeUnlocked(FoodType foodType)
    {
        return unlockedRecipes.Contains(foodType);
    }

    public void UnlockRecipe(FoodType foodType)
    {
        bool recipeExists = false;

        foreach (Recipe recipe in recipes)
        {
            if (recipe != null && recipe.foodType == foodType)
            {
                recipeExists = true;
                break;
            }
        }

        if (!recipeExists)
        {
            Debug.LogError(
                "RecipeManager: No recipe found for " + foodType
            );
            return;
        }

        if (unlockedRecipes.Add(foodType))
        {
            Debug.Log(foodType + " recipe unlocked!");
        }
        else
        {
            Debug.Log(foodType + " recipe is already unlocked.");
        }
    }

    // This method can be connected to BakeryShopUI's UnityEvent.
    public void UnlockCupcakeRecipe()
    {
        UnlockRecipe(cupcakeRecipeFoodType);
    }
}