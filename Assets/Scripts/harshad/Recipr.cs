using System.Collections.Generic;

[System.Serializable]
public class Recipe
{
    public FoodType foodType;

    public CookingMethod cookingMethod;

    public float cookingTime;

    public List<IngredientType> ingredients =
        new List<IngredientType>();
}