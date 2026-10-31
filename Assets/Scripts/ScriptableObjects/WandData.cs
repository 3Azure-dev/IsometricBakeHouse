using UnityEngine;

[CreateAssetMenu(fileName = "SO_Wand", menuName = "Tiny Bake House/Wand Data")]
public class WandData : ScriptableObject
{
    [Tooltip("Seconds between shots. 0 = no cooldown.")]
    [Min(0f)] public float cooldown = 0.3f;

    [Tooltip("How far a bolt flies before it disappears, in world units. It can only stun within this distance.")]
    [Min(0.5f)] public float range = 10f;
}