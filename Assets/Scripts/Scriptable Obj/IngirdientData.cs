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

    [Tooltip("1 = normal fall. Lower = floats down slowly")]
    public float gravityScale = 1f;

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

    [Header("Flying - only used by flying creatures")]
    [Tooltip("How high above the ground it floats")]
    public float hoverHeight = 3f;

    [Tooltip("How far it bobs up and down while floating")]
    public float bobAmount = 0.3f;

    [Tooltip("How fast it bobs up and down")]
    public float bobSpeed = 2f;

    [Header("Slow Trail - only used by creatures that drop puddles")]
    [Tooltip("Seconds between each puddle while running away")]
    public float dropInterval = 0.8f;

    [Tooltip("Player speed inside a puddle. 0.5 = half speed")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;

    [Tooltip("Seconds the player stays slow after leaving a puddle")]
    public float slowDuration = 1f;

    [Tooltip("Seconds before a puddle disappears")]
    public float puddleLifetime = 4f;
}