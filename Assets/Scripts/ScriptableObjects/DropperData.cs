using UnityEngine;

// Base for creatures that drop something while running (Seeds, Butter, Sauce)
public abstract class DropperData : IngredientData
{
    [Header("Dropping")]
    [Tooltip("Seconds between each drop while running away")]
    public float dropInterval = 0.8f;
}