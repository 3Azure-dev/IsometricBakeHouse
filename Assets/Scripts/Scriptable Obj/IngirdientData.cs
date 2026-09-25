using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_New", menuName = "Tiny Bake House/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Info")]
    public string ingredientName;
    public Sprite icon;

    [Header("Hunting")]
    [Tooltip("How fast this creature runs from the player")]
    public float fleeSpeed = 4f;
}