using UnityEngine;

[CreateAssetMenu(menuName = "Tiny Bake House/Mould Settings")]
public class MouldSettings : ScriptableObject
{
    [Tooltip("Seconds before mould can be placed again")]
    public float cooldown = 3f;

    [Tooltip("Size of every mould zone")]
    public float zoneRadius = 4f;
}