using UnityEngine;
using UnityEngine.Serialization;

// All dunk numbers live here so Pravin can tune them without code.
[CreateAssetMenu(fileName = "DunkData", menuName = "Tiny Bake House/Dunk Data")]
public class DunkData : ScriptableObject
{
    [Header("Aim swing")]
    [Tooltip("Seconds for one full swing, bottom to top")]
    public float swingTime = 1.5f;

    [Tooltip("Normal speed for this many seconds, then it starts speeding up")]
    [FormerlySerializedAs("speedUpEvery")]
    public float speedUpDelay = 2f;

    [Tooltip("How much faster per second, smoothly. 0.075 = +7.5% per second")]
    public float speedUpPerSecond = 0.075f;

    [Tooltip("The aim never goes faster than this. 3 = 3x")]
    public float maxSpeed = 3f;

    [Header("Hit zone")]
    [Tooltip("Where the pot is on the swing. 0 = bottom, 1 = top")]
    [Range(0f, 1f)] public float hitZoneCenter = 0.5f;

    [Tooltip("Size of the hit zone. 0.2 = 20% of the swing")]
    [Range(0.05f, 1f)] public float hitZoneSize = 0.2f;

    [Header("Reward")]
    public int bonusMultiplier = 2;

    [Header("Grandfather")]
    [Tooltip("Chance he comes after a hunt. 0.33 = 1 in 3")]
    [Range(0f, 1f)] public float grandfatherChance = 0.33f;
}