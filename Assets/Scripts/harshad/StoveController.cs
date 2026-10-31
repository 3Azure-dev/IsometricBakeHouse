using UnityEngine;
using UnityEngine.UI;

public class StoveController : MonoBehaviour
{
    [Header("Recipes (drag SO_RecipeDatabase here)")]
    [SerializeField] private RecipeDatabase recipeDatabase;

    [Header("Which equipment is this? (drag SO_Equipment_Stove)")]
    [SerializeField] private EquipmentData equipment;

    [Header("Cooking Progress UI")]
    [SerializeField] private GameObject cookingProgressCanvas;
    [SerializeField] private Slider cookingProgressSlider;

    private bool isCooking;
    private bool foodReady;

    private FoodType currentFood;
    private float cookingTimer;

    // The recipe asset for the dish being cooked (cook time comes from here)
    private RecipeData currentRecipe;

    // --------------------------------------------------
    // START
    // --------------------------------------------------

    private void Start()
    {
        if (cookingProgressCanvas != null)
        {
            cookingProgressCanvas.SetActive(false);
        }

        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.minValue = 0f;
            cookingProgressSlider.maxValue = 1f;
            cookingProgressSlider.value = 0f;
        }
    }

    // --------------------------------------------------
    // PLAYER INTERACTION
    // --------------------------------------------------

    public void Interact()
    {
        if (isCooking)
        {
            Debug.Log("Stove is already cooking.");
            return;
        }

        if (foodReady)
        {
            TakeFood();
            return;
        }

        if (PlayerFoodInventory.Instance == null)
        {
            Debug.LogError("PlayerFoodInventory was not found.");
            return;
        }

        if (StoveUI.Instance == null)
        {
            Debug.LogError("StoveUI was not found.");
            return;
        }

        StoveUI.Instance.Open(this);
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!isCooking || currentRecipe == null)
        {
            return;
        }

        cookingTimer += Time.deltaTime;

        float cookTime = currentRecipe.cookTime;

        if (cookingProgressSlider != null && cookTime > 0f)
        {
            cookingProgressSlider.value = Mathf.Clamp01(cookingTimer / cookTime);
        }

        if (cookingTimer >= cookTime)
        {
            FinishCooking();
        }
    }

    // --------------------------------------------------
    // START COOKING
    // --------------------------------------------------

    public void StartCooking(FoodType food)
    {
        if (isCooking || foodReady)
        {
            return;
        }

        if (recipeDatabase == null)
        {
            Debug.LogError("StoveController has no RecipeDatabase assigned.");
            return;
        }

        RecipeData recipe = recipeDatabase.GetRecipe(food);

        if (recipe == null)
        {
            Debug.LogWarning("No recipe found for " + food + ". Check SO_RecipeDatabase and the Food Type on the recipe asset.");
            return;
        }

        // Cupcake needs the Oven, Pizza needs the Pizza Stone
        if (equipment != null && recipe.requiredEquipment != equipment)
        {
            Debug.Log(food + " cannot be cooked on the " + equipment.equipmentName + ".");
            return;
        }

        // Uses 1 of each ingredient from Inventory1 (Bread = Flour + Yeast + Seeds)
        if (Inventory1.Instance != null && !Inventory1.Instance.UseIngredients(recipe))
        {
            Debug.Log("Not enough ingredients for " + food + ".");
            return;
        }

        currentRecipe = recipe;
        currentFood = food;
        cookingTimer = 0f;
        isCooking = true;

        if (BakeryAudioManager.Instance != null)
        {
            BakeryAudioManager.Instance.PlayCookingStart();
        }

        if (cookingProgressCanvas != null)
        {
            cookingProgressCanvas.SetActive(true);
        }

        if (cookingProgressSlider != null)
        {
            cookingProgressSlider.value = 0f;
        }

        Debug.Log("Stove started cooking " + currentFood + " (" + recipe.cookTime + " sec)");
    }

    // --------------------------------------------------
    // FINISH COOKING
    // --------------------------------------------------

    private void FinishCooking()
    {
        isCooking = false;
        cookingTimer = 0f;
        foodReady = true;

        if (BakeryAudioManager.Instance != null)
        {
            BakeryAudioManager.Instance.PlayCookingDone();
        }

        if (cookingProgressCanvas != null)
        {
            cookingProgressCanvas.SetActive(false);
        }

        Debug.Log(currentFood + " is ready on the stove.");
    }

    // --------------------------------------------------
    // TAKE FOOD
    // --------------------------------------------------

    private void TakeFood()
    {
        if (!foodReady || PlayerFoodInventory.Instance == null)
        {
            return;
        }

        // FIX: only clear the stove if the player really got the dish.
        // Before, a full inventory made the dish disappear from the stove.
        bool taken = PlayerFoodInventory.Instance.TakeFood(currentFood);

        if (!taken)
        {
            Debug.Log("Inventory full. " + currentFood + " stays on the stove.");
            return;
        }

        foodReady = false;
        currentRecipe = null;

        Debug.Log("Player took " + currentFood + " from the stove.");
    }
}