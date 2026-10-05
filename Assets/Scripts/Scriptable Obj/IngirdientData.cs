using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_New", menuName = "Tiny Bake House/Ingredient")]
public class IngredientData : ScriptableObject
{
    [Header("Info")]
    public string ingredientName;
    public Sprite icon;

    [Header("Hunting - Movement")]
    [Tooltip("How fast this creature runs from the player")]
    public float fleeSpeed = 4f;

    [Tooltip("How high this creature jumps over walls and gaps")]
    public float jumpForce = 8f;

    [Tooltip("Creature starts running when the player is closer than this")]
    public float detectRange = 4f;

    [Tooltip("Creature calms down when the player is farther than this. Keep it bigger than Detect Range")]
    public float safeRange = 7f;

    [Header("Hunting - Catching")]
    [Tooltip("Seconds the creature stays frozen after a bolt hits it")]
    public float stunDuration = 2f;

    [Tooltip("How close the player must be to press E and catch it")]
    public float catchRange = 1.5f;

    [Tooltip("How many ingredients the player gets for one catch")]
    [Min(1)] public int ingredientsPerCatch = 1;
}