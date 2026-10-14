using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("VIP")]
    [Range(0f, 100f)]
    [SerializeField] private float vipChance = 10f;

    private float spawnTimer;

    private void Update()
    {
        if (settings == null)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        // Wait for the spawn delay from the settings asset
        if (spawnTimer >= settings.doorSpawnDelay)
        {
            // Only spawn if the door is not already full.
            // If it is full, the timer keeps running, so the next customer
            // arrives as soon as there is room.
            if (CountCustomersAtDoor() < settings.maxCustomersAtDoor)
            {
                SpawnCustomer();

                spawnTimer = 0f;
            }
        }
    }

    // Counts customers who are currently walking in toward a seat
    private int CountCustomersAtDoor()
    {
        CustomerController[] customers =
            FindObjectsByType<CustomerController>(FindObjectsSortMode.None);

        int count = 0;

        foreach (CustomerController customer in customers)
        {
            if (customer.IsEnteringBakery)
            {
                count++;
            }
        }

        return count;
    }

    private void SpawnCustomer()
    {
        CustomerType type;

        float randomNumber = Random.Range(0f, 100f);

        if (randomNumber <= vipChance)
        {
            type = CustomerType.VIP;
        }
        else
        {
            type = CustomerType.Normal;
        }

        GameObject customer = CustomerPool.Instance.GetCustomer(type);

        if (customer == null)
            return;

        customer.transform.position = spawnPoint.position;

        customer.transform.rotation = spawnPoint.rotation;

        CustomerController controller = customer.GetComponent<CustomerController>();

        controller.StartCustomer(type);
    }
}