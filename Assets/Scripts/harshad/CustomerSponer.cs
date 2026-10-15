using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    private DayNightClock clock;

    // Customers only come in the Day Phase
    private bool dayIsOpen = true;

    private float spawnTimer;

    // --------------------------------------------------
    // LISTEN TO THE CLOCK
    // --------------------------------------------------

    private void Start()
    {
        clock = DayNightClock.Instance;

        if (clock == null)
        {
            Debug.LogWarning("CustomerSpawner: no DayNightClock found. Customers will come all the time.");
            return;
        }

        // If we arrive in the middle of a night (back from a hunt), do not spawn
        dayIsOpen = clock.IsDay;

        clock.DayEnded += OnDayEnded;
        clock.NightEnded += OnNightEnded;
    }

    // The clock outlives this scene, so unsubscribe when the spawner is destroyed
    private void OnDestroy()
    {
        if (clock != null)
        {
            clock.DayEnded -= OnDayEnded;
            clock.NightEnded -= OnNightEnded;
        }
    }

    private void OnDayEnded()
    {
        dayIsOpen = false;

        SendDoorCustomersHome();

        Debug.Log("CustomerSpawner: day ended, no new customers.");
    }

    private void OnNightEnded()
    {
        dayIsOpen = true;

        spawnTimer = 0f;

        Debug.Log("CustomerSpawner: a new day began, customers are coming again.");
    }

    // Customers who have not sat down yet (the door queue) walk away.
    // Customers who are already eating are left alone: they finish and pay as normal.
    private void SendDoorCustomersHome()
    {
        CustomerController[] customers =
            FindObjectsByType<CustomerController>(FindObjectsSortMode.None);

        foreach (CustomerController customer in customers)
        {
            customer.LeaveBecauseDayEnded();
        }
    }

    // --------------------------------------------------
    // SPAWNING
    // --------------------------------------------------

    private void Update()
    {
        if (settings == null)
        {
            return;
        }

        // Night: nobody comes
        if (!dayIsOpen)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        // Wait for the spawn delay from the settings asset
        if (spawnTimer >= settings.doorSpawnDelay)
        {
            // Only spawn if the door is not already full
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

        // VIP chance comes from SO_CustomerSettings (percent)
        float randomNumber = Random.Range(0f, 100f);

        if (randomNumber <= settings.vipChancePercent)
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