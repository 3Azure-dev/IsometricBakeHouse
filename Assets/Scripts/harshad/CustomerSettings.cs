using UnityEngine;

// A ScriptableObject is a data file that lives in your Assets folder.
// Scripts read numbers from it, so designers can change values in the Inspector.
[CreateAssetMenu(fileName = "SO_CustomerSettings", menuName = "Bakery/Customer Settings")]
public class CustomerSettings : ScriptableObject
{
    [Header("Thinking")]
    [Tooltip("Shortest time (seconds) a customer thinks before they are ready to order.")]
    public float thinkingTimeMin = 3f;

    [Tooltip("Longest time (seconds) a customer thinks before they are ready to order.")]
    public float thinkingTimeMax = 5f;

    [Header("Patience and Timers")]
    [Tooltip("How long (seconds) a customer waits for the player to take their order.")]
    public float orderPatience = 20f;

    [Tooltip("Total time (seconds) the order timer runs.")]
    public float orderTimer = 180f;

    [Header("Eating")]
    [Tooltip("How long (seconds) a customer eats before leaving.")]
    public float eatingTime = 12f;

    [Header("Door and Spawning")]
    [Tooltip("Maximum customers allowed waiting at the door.")]
    public int maxCustomersAtDoor = 4;

    [Tooltip("Seconds between each customer spawning at the door.")]
    public float doorSpawnDelay = 5f;

    [Header("Seating")]
    [Tooltip("How many seats each table has.")]
    public int seatsPerTable = 2;
}