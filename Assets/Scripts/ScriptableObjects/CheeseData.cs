using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_Cheese", menuName = "Tiny Bake House/Ingredient - Cheese")]
public class CheeseData : IngredientData
{
    [Header("Cheese Holes")]
    [Tooltip("Seconds Cheese stays out of a hole")]
    public float popUpTime = 0.8f;

    [Tooltip("Seconds the hole shakes before Cheese pops out")]
    public float rumbleTime = 0.5f;

    [Tooltip("Seconds hidden between pop-ups")]
    public float hideTime = 0.6f;

    [Tooltip("Pop-ups before it escapes for good")]
    [Min(1)] public int popUpCount = 3;
}