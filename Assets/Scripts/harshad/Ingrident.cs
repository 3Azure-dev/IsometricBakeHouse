public enum IngredientType
{
    Flour,
    Egg,
    Milk,
    Sugar,
    Butter,
    Bread,
    Meat,
    Cheese,
    Tomato,

    // Added at the END on purpose: the numbers of the old ingredients must not change,
    // because the scene already saved them. These two are hunted in the Bread world.
    Seeds,
    Yeast
}