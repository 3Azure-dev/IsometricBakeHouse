using UnityEngine;

public class StoveController : MonoBehaviour
{
    private bool isCooking;

    private bool foodReady;

    private FoodType currentFood;

    private float cookingTimer;

    private Recipe currentRecipe;


    // --------------------------------------------------
    // PLAYER INTERACTION
    // --------------------------------------------------

    public void Interact()
    {
        if (isCooking)
        {
            Debug.Log(
                "Stove is already cooking."
            );

            return;
        }

        if (foodReady)
        {
            TakeFood();

            return;
        }

        if (PlayerFoodInventory.Instance == null)
        {
            Debug.LogError(
                "PlayerFoodInventory was not found."
            );

            return;
        }

        if (StoveUI.Instance == null)
        {
            Debug.LogError(
                "StoveUI was not found."
            );

            return;
        }

        StoveUI.Instance.Open(this);
    }

    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (!isCooking)
        {
            return;
        }

        cookingTimer += Time.deltaTime;

        if (cookingTimer >= currentRecipe.cookingTime)
        {
            FinishCooking();
        }
    }


    // --------------------------------------------------
    // START COOKING
    // --------------------------------------------------

    public void StartCooking(FoodType food)
    {
        if (isCooking)
        {
            return;
        }

        if (foodReady)
        {
            return;
        }

        if (RecipeManager.Instance == null)
        {
            Debug.LogError(
                "RecipeManager was not found."
            );

            return;
        }

        Recipe recipe =
            RecipeManager.Instance.GetRecipe(
                food
            );

        if (recipe == null)
        {
            Debug.LogWarning(
                "No recipe found for " +
                food
            );

            return;
        }

        if (recipe.cookingMethod != CookingMethod.Stove)
        {
            Debug.LogWarning(
                food +
                " cannot be cooked on the stove."
            );

            return;
        }

        currentRecipe = recipe;

        currentFood = food;

        cookingTimer = 0f;

        isCooking = true;

        Debug.Log(
            "Stove started cooking " +
            currentFood
        );
    }


    // --------------------------------------------------
    // FINISH COOKING
    // --------------------------------------------------

    private void FinishCooking()
    {
        isCooking = false;

        cookingTimer = 0f;

        foodReady = true;

        Debug.Log(
            currentFood +
            " is ready on the stove."
        );
    }


    // --------------------------------------------------
    // TAKE FOOD
    // --------------------------------------------------

    private void TakeFood()
    {
        if (!foodReady)
        {
            return;
        }

        if (PlayerFoodInventory.Instance == null)
        {
            return;
        }

        PlayerFoodInventory.Instance.TakeFood(
            currentFood
        );

        foodReady = false;

        currentRecipe = null;

        Debug.Log(
            "Player took " +
            currentFood +
            " from the stove."
        );
    }
}