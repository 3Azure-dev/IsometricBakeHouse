using UnityEngine;

[CreateAssetMenu(fileName = "SO_Furniture_New", menuName = "Tiny Bake House/Furniture")]
public class FurnitureData : ScriptableObject
{
    [Header("Info")]
    public string furnitureName;
    public Sprite icon;

    [Header("Shop")]
    public int cost;

    [Header("Effect")]
    [Tooltip("How much this raises bakery appeal")]
    public int appeal;
    [Tooltip("How many customers can sit here. 0 for decorations")]
    public int seats;

    [Header("Building")]
    [Tooltip("Diamonds to buy this item's blueprint")]
    [Min(0)] public int blueprintDiamonds = 0;

    [Tooltip("The real object that gets built")]
    public GameObject prefab;
    [Tooltip("Material boxes needed to finish building")]
    [Min(1)] public int materialBoxes = 1;

    [Header("IDs (shop, delivery and save use these)")]
    [Tooltip("Must match what the shop orders, e.g. Table_Basic")]
    public string itemId;
    [Tooltip("Material this blueprint needs, e.g. Wood")]
    public string materialId = "Wood";
}