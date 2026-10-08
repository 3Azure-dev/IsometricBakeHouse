using UnityEngine;
using UnityEngine.InputSystem;

public class RecipeCheckTest : MonoBehaviour
{
    public RecipeData recipe;

    void Update()
    {
        // Press C to try cooking
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            // print how many of each ingredient we have
            foreach (IngredientData ingredient in recipe.huntedIngredients)
                Debug.Log(ingredient.ingredientName + ": " + Inventory1.Instance.GetCount(ingredient));

            bool cooked = Inventory1.Instance.UseIngredients(recipe);
            Debug.Log(cooked ? "Cooked " + recipe.recipeName : "Not enough ingredients");
        }
    }
}