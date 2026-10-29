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

    [Tooltip("Order timer (seconds) for a NORMAL customer. Starts when the player takes the order.")]
    public float orderTimer = 180f;

    [Header("Reactions (SCRUM-274)")]
    [Tooltip("Ticked: serving a dish that was NOT ordered makes the customer disappointed (0 coins) and the dish is used up. " +
             "Unticked: the customer just ignores the wrong dish.")]
    public bool wrongDishDisappoints = true;

    [Header("Eating")]
    [Tooltip("How long (seconds) a customer eats before leaving.")]
    public float eatingTime = 12f;

    [Header("Door and Spawning")]
    [Tooltip("Maximum customers allowed waiting at the door.")]
    public int maxCustomersAtDoor = 4;

    [Tooltip("How many customers come at a time (GDD: 2).")]
    public int customersPerBatch = 2;

    [Tooltip("Seconds between each customer spawning at the door.")]
    public float doorSpawnDelay = 5f;

    [Header("Seating")]
    [Tooltip("How many seats each table has.")]
    public int seatsPerTable = 2;

    [Header("Customer Names")]
    [Tooltip("Each customer gets a random name from this list. Add as many as you like.")]
    public string[] customerNames =
{
        "Harshad", "Pravin", "Shreyas", "Aditya", "Vijay", "Mohan",
        "Rahul", "Priya", "Neha", "Karan", "Anjali", "Rohan"
    }; 


    [Header("VIP Customers")]
    [Tooltip("Order timer (seconds) for a VIP customer.")]
    public float vipOrderTimer = 240f;

    [Tooltip("VIP pays the dish price times this number.")]
    public int vipCoinMultiplier = 2;

    [Tooltip("Diamonds a VIP gives when served correctly. (Not paid out yet: the Wallet has no diamonds.)")]
    public int vipDiamondReward = 20;

    [Range(0f, 100f)]
    [Tooltip("Chance (percent) that a new customer is a VIP. The GDD says about 1 VIP per 3 in-game days, " +
             "which is about 4% with 8 customers a day. Replace this once the day counter exists.")]
    public float vipChancePercent = 4f;

    [Tooltip("Body colour of a VIP customer (gold)")]
    public Color vipColor = new Color(1f, 0.84f, 0f);
}