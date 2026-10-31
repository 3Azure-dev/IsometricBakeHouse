using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A cooking station that reads its recipes from the RecipeData assets (SO_Recipe_*).
// ONE script for the Oven and the Pizza Stone: set "Equipment" to the right asset.
// Ingredients come from Inventory1, the same bag the hunts fill and the counters show.
public class OvenController : MonoBehaviour
{
    [Header("Data (drag the assets in)")]
    [SerializeField] private RecipeDatabase recipeDatabase;   // SO_RecipeDatabase
    [SerializeField] private EquipmentData equipment;         // SO_Equipment_Oven or SO_Equipment_PizzaStone

    public bool IsCooking { get; private set; }
    public FoodType CurrentFood { get; private set; }

    // True when the dish is finished and waiting for the player to press E
    public bool FoodReady { get; private set; }

    public string StationName => equipment != null ? equipment.equipmentName : name;

    public event Action<string> OnStatusChanged;
    public event Action OnCookingStateChanged;

    private Coroutine cookingCoroutine;

    public void Interact()
    {
        // Finished dish waiting? E picks it up (same as the Stove)
        if (FoodReady)
        {
            TakeFood();
            return;
        }

        if (OvenUI.Instance == null)
        {
            Debug.LogError("OvenUI was not found in the scene.");
            return;
        }

        OvenUI.Instance.Open(this);
    }

    // ---------------- WHICH RECIPES THIS STATION COOKS ----------------

    // Recipes whose Required Equipment is THIS station and that the player has unlocked
    public List<RecipeData> GetAvailableRecipes()
    {
        List<RecipeData> result = new List<RecipeData>();

        if (recipeDatabase == null)
        {
            Debug.LogError(name + ": no RecipeDatabase assigned (drag SO_RecipeDatabase in).");
            return result;
        }

        if (equipment == null)
        {
            Debug.LogError(name + ": no Equipment assigned (drag SO_Equipment_Oven or SO_Equipment_PizzaStone in).");
            return result;
        }

        foreach (RecipeData recipe in recipeDatabase.Recipes)
        {
            if (recipe != null &&
                recipe.requiredEquipment == equipment &&
                IsUnlocked(recipe.foodType))
            {
                result.Add(recipe);
            }
        }

        return result;
    }

    public bool CanCook(FoodType food)
    {
        return FindRecipe(food) != null;
    }

    private RecipeData FindRecipe(FoodType food)
    {
        foreach (RecipeData recipe in GetAvailableRecipes())
        {
            if (recipe.foodType == food)
            {
                return recipe;
            }
        }

        return null;
    }

    private bool IsUnlocked(FoodType food)
    {
        return BakeryProgress.Instance == null ||
               BakeryProgress.Instance.IsRecipeUnlocked(food);
    }

    // ---------------- TAKE THE FINISHED DISH ----------------

    private void TakeFood()
    {
        if (!FoodReady || PlayerFoodInventory.Instance == null)
        {
            return;
        }

        // Only clear the station if the player really got the dish
        bool taken = PlayerFoodInventory.Instance.TakeFood(CurrentFood);

        if (!taken)
        {
            SetStatus("Your food inventory is full. " + CurrentFood + " stays on the " + StationName + ".");
            return;
        }

        FoodReady = false;

        SetStatus("You took the " + CurrentFood + ".");

        OnCookingStateChanged?.Invoke();
    }

    // ---------------- START COOKING ----------------

    public bool TryStartCooking(FoodType food)
    {
        if (IsCooking)
        {
            SetStatus("The " + StationName + " is already cooking.");
            return false;
        }

        if (FoodReady)
        {
            SetStatus("Take the finished " + CurrentFood + " first (press E).");
            return false;
        }

        RecipeData recipe = FindRecipe(food);

        if (recipe == null)
        {
            SetStatus("The " + StationName + " cannot cook " + food +
                      " (wrong equipment, or the recipe is not unlocked).");
            return false;
        }

        if (Inventory1.Instance == null)
        {
            SetStatus("Ingredient inventory is missing.");
            return false;
        }

        string missing = GetMissingIngredients(recipe);

        if (missing.Length > 0)
        {
            SetStatus("Not enough ingredients: " + missing);
            return false;
        }

        if (!Inventory1.Instance.UseIngredients(recipe))
        {
            SetStatus("Could not use the ingredients.");
            return false;
        }

        CurrentFood = food;
        IsCooking = true;

        if (BakeryAudioManager.Instance != null)
        {
            BakeryAudioManager.Instance.PlayCookingStart();
        }

        OnCookingStateChanged?.Invoke();
        SetStatus("Cooking " + food + " (" + recipe.cookTime.ToString("0.#") + " sec)...");

        cookingCoroutine = StartCoroutine(CookRecipe(recipe));

        return true;
    }

    // "Dough (have 0, need 1), Cheese (have 0, need 1)", or "" if nothing is missing
    private string GetMissingIngredients(RecipeData recipe)
    {
        Dictionary<IngredientData, int> needed = new Dictionary<IngredientData, int>();

        foreach (IngredientData ingredient in recipe.huntedIngredients)
        {
            if (ingredient == null) continue;

            needed[ingredient] = needed.ContainsKey(ingredient) ? needed[ingredient] + 1 : 1;
        }

        string missing = "";

        foreach (KeyValuePair<IngredientData, int> item in needed)
        {
            int have = Inventory1.Instance.GetCount(item.Key);

            if (have < item.Value)
            {
                if (missing.Length > 0) missing += ", ";

                missing += item.Key.ingredientName + " (have " + have + ", need " + item.Value + ")";
            }
        }

        return missing;
    }

    private IEnumerator CookRecipe(RecipeData recipe)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, recipe.cookTime));

        // The dish waits on the station until the player presses E. It never burns.
        IsCooking = false;
        FoodReady = true;
        cookingCoroutine = null;

        if (BakeryAudioManager.Instance != null)
        {
            BakeryAudioManager.Instance.PlayCookingDone();
        }

        SetStatus(CurrentFood + " is ready! Press E at the " + StationName + " to take it.");

        OnCookingStateChanged?.Invoke();
    }

    private void SetStatus(string message)
    {
        Debug.Log("[" + StationName + "] " + message);
        OnStatusChanged?.Invoke(message);
    }

    private void OnDisable()
    {
        if (cookingCoroutine != null)
        {
            StopCoroutine(cookingCoroutine);
            cookingCoroutine = null;
        }
    }

    // ---------------- TEST BUTTON ----------------
    // Gives 1 of every ingredient this station's recipes need (into Inventory1)

    [ContextMenu("TEST: Give ingredients for this station")]
    private void TestGiveIngredients()
    {
        if (Inventory1.Instance == null || recipeDatabase == null || equipment == null)
        {
            Debug.LogWarning("TEST: needs Inventory1, a RecipeDatabase and an Equipment.");
            return;
        }

        foreach (RecipeData recipe in recipeDatabase.Recipes)
        {
            if (recipe == null || recipe.requiredEquipment != equipment) continue;

            foreach (IngredientData ingredient in recipe.huntedIngredients)
            {
                if (ingredient != null) Inventory1.Instance.Add(ingredient, 1);
            }
        }

        Debug.Log("TEST: gave ingredients for the " + StationName);
    }
}