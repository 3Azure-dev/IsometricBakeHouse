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
}