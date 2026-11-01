using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Recipe
{
    public FoodType foodType;

    public CookingMethod cookingMethod;

    public float cookingTime;

    public List<IngredientType> ingredients =
        new List<IngredientType>();

    [Header("Recipe Unlock")]
    public bool unlockedByDefault = true;
}