using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    [SerializeField]
    private List<Recipe> recipes =
        new List<Recipe>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    public Recipe GetRecipe(FoodType foodType)
    {
        foreach (Recipe recipe in recipes)
        {
            if (recipe.foodType == foodType)
            {
                return recipe;
            }
        }

        return null;
    }
}