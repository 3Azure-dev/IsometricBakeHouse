using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_Seeds", menuName = "Tiny Bake House/Ingredient - Seeds")]
public class SeedsData : DropperData
{
    [Header("Slow Puddle")]
    [Tooltip("Player speed inside a puddle. 0.5 = half speed")]
    [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;

    [Tooltip("Seconds the player stays slow after leaving a puddle")]
    public float slowDuration = 1f;

    [Tooltip("Seconds before a puddle disappears")]
    public float puddleLifetime = 4f;
}