
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenController : MonoBehaviour
{
    public bool IsCooking { get; private set; }
    public FoodType CurrentFood { get; private set; }

    public event Action<string> OnStatusChanged;
    public event Action OnCookingStateChanged;

    private Coroutine cookingCoroutine;

    public void Interact()
    {
        if (OvenUI.Instance == null)
        {
            Debug.LogError("OvenUI was not found in the scene.");
            return;
        }

        OvenUI.Instance.Open(this);
    }

    public bool CanCook(FoodType food)
    {
        Recipe recipe = RecipeManager.Instance.GetRecipe(food);

        return recipe != null &&
               recipe.cookingMethod == CookingMethod.Oven;
    }

    public bool TryStartCooking(FoodType food)
    {
        if (IsCooking)
        {
            SetStatus("The oven is already cooking.");
            return false;
        }

        if (RecipeManager.Instance == null)
        {
            SetStatus("RecipeManager is missing.");
            return false;
        }

        Recipe recipe = RecipeManager.Instance.GetRecipe(food);

        if (recipe == null)
        {
            SetStatus("No recipe found for " + food + ".");
            return false;
        }

        if (recipe.cookingMethod != CookingMethod.Oven)
        {
            SetStatus(food + " must be cooked using another appliance.");
            return false;
        }

        if (recipe.ingredients == null ||
            recipe.ingredients.Count == 0)
        {
            SetStatus("This recipe has no ingredients configured.");
            return false;
        }

        if (IngredientInventory.Instance == null)
        {
            SetStatus("Ingredient inventory is missing.");
            return false;
        }

        // Check every ingredient before removing anything.
        Dictionary<IngredientType, int> required =
            new Dictionary<IngredientType, int>();

        foreach (IngredientType ingredient in recipe.ingredients)
        {
            if (!required.ContainsKey(ingredient))
                required[ingredient] = 0;

            required[ingredient]++;
        }

        foreach (KeyValuePair<IngredientType, int> item in required)
        {
            if (!IngredientInventory.Instance.HasIngredient(
                    item.Key, item.Value))
            {
                SetStatus(
                    "Not enough " + item.Key +
                    ". Required: " + item.Value + "."
                );

                return false;
            }
        }

        // All ingredients are available. Consume them.
        foreach (KeyValuePair<IngredientType, int> item in required)
        {
            bool removed = IngredientInventory.Instance.RemoveIngredient(
                item.Key, item.Value
            );

            if (!removed)
            {
                Debug.LogError(
                    "Could not remove ingredient: " + item.Key
                );

                SetStatus("Could not start cooking.");
                return false;
            }
        }

        CurrentFood = food;
        IsCooking = true;

        OnCookingStateChanged?.Invoke();
        SetStatus("Cooking " + food + "...");

        cookingCoroutine = StartCoroutine(CookRecipe(recipe));

        return true;
    }

    private IEnumerator CookRecipe(Recipe recipe)
    {
        float cookingTime = Mathf.Max(0f, recipe.cookingTime);

        yield return new WaitForSeconds(cookingTime);

        if (PlayerFoodInventory.Instance == null)
        {
            IsCooking = false;
            cookingCoroutine = null;

            SetStatus(
                "Cooking finished, but PlayerFoodInventory is missing."
            );

            OnCookingStateChanged?.Invoke();
            yield break;
        }

        // Uses the same finished-food inventory as the stove.
        PlayerFoodInventory.Instance.TakeFood(CurrentFood);

        FoodType finishedFood = CurrentFood;

        IsCooking = false;
        cookingCoroutine = null;

        SetStatus(finishedFood + " is ready! Collect your food.");

        OnCookingStateChanged?.Invoke();
    }

    private void SetStatus(string message)
    {
        Debug.Log("[Oven] " + message);
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
}
