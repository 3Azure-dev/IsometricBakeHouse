using UnityEngine;

[CreateAssetMenu(fileName = "SO_Recipe_New", menuName = "Tiny Bake House/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("Info")]
    public string recipeName;
    public Sprite icon;

    [Header("Unlock")]
    public int unlockLevel = 1;
    public int blueprintCost;

    [Header("Cooking")]
    public IngredientData[] huntedIngredients;
    public string autoAppliedIngredient;
    public EquipmentData requiredEquipment;
    public float cookTime = 10f;

    [Header("Reward")]
    public int coinReward = 45;
}