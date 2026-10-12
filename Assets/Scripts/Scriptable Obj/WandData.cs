using UnityEngine;

[CreateAssetMenu(fileName = "SO_Wand", menuName = "Tiny Bake House/Wand Data")]
public class WandData : ScriptableObject
{
    [Tooltip("Seconds between shots. 0 = no cooldown.")]
    [Min(0f)] public float cooldown = 0.3f;
}