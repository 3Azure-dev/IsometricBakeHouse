using UnityEngine;

[CreateAssetMenu(fileName = "SO_MoltenCheese", menuName = "Tiny Bake House/Molten Cheese Data")]
public class MoltenCheeseData : ScriptableObject
{
    [Tooltip("Player speed in the pool. 0.75 = a bit slow (jam is 0.5)")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.75f;
    public float slowDuration = 0.2f;

    [Tooltip("Seconds on the ground in the pool before burning")]
    public float burnTime = 1.5f;

    [Tooltip("How fast the heat cools while jumping in the pool")]
    public float coolSpeed = 0.5f;

    [Tooltip("Ingredients lost per burn (only ones caught in this level)")]
    [Min(0)] public int ingredientsLost = 1;
}