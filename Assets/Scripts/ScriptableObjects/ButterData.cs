using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_Butter", menuName = "Tiny Bake House/Ingredient - Butter")]
public class ButterData : DropperData
{
    [Header("Butter Blob")]
    [Tooltip("Seconds before a butter blob disappears")]
    public float puddleLifetime = 4f;
}