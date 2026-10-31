using UnityEngine;

// Recipe NUMBERS (cook time, ingredients, coins) live in the RecipeData assets (SO_Recipe_*).
// This script only keeps the unlock shortcuts the shop buttons call.
// The unlocks themselves are stored in BakeryProgress so they survive hunts.
public class RecipeManager : MonoBehaviour
{
    public static RecipeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Destroy only this extra COMPONENT, never the whole GameObject
            Debug.LogWarning("RecipeManager: a second RecipeManager exists on '" + gameObject.name + "'. Remove the extra one in the scene.");
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public bool IsRecipeUnlocked(FoodType foodType)
    {
        return BakeryProgress.Instance != null &&
               BakeryProgress.Instance.IsRecipeUnlocked(foodType);
    }

    public void UnlockRecipe(FoodType foodType)
    {
        if (BakeryProgress.Instance != null)
        {
            BakeryProgress.Instance.UnlockRecipe(foodType);
        }
    }

    // The shop's "On Cupcake Recipe Purchased" event calls this one (already wired in the scene)
    public void UnlockCupcakeRecipe()
    {
        UnlockRecipe(FoodType.Cupcake);
    }

    public void UnlockPizzaRecipe()
    {
        UnlockRecipe(FoodType.Pizza);
    }
}