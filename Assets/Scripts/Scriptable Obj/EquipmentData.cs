using UnityEngine;

[CreateAssetMenu(fileName = "SO_Equipment_New", menuName = "Tiny Bake House/Equipment")]
public class EquipmentData : ScriptableObject
{
    [Header("Info")]
    public string equipmentName;
    public Sprite icon;

    [Header("Shop")]
    public int cost;
    public int unlockLevel = 1;
}