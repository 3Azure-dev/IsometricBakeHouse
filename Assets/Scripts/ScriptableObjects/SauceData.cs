using UnityEngine;

[CreateAssetMenu(fileName = "SO_Ingredient_Sauce", menuName = "Tiny Bake House/Ingredient - Sauce")]
public class SauceData : DropperData
{
    [Header("Sauce Droplets")]
    [Min(1)] public int dropletsPerDrop = 2;
    [Tooltip("How fast droplets fly back at the player")]
    public float dropletBackSpeed = 4f;
    public float dropletUpSpeed = 5f;
    public float dropletLifetime = 4f;

    [Tooltip("Seconds added to the wand cooldown when a droplet hits")]
    public float extraCooldown = 3f;

    [Tooltip("How long the sticky wand lasts. A new hit restarts it")]
    public float debuffTime = 8f;
}