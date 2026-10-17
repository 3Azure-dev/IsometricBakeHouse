using System.Collections;
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

    // --------------------------------------------------
    // SETUP
    // --------------------------------------------------

    private void Start()
    {
        clock = DayNightClock.Instance;

        if (clock == null)
        {
            Debug.LogWarning("CustomerSpawner: no DayNightClock found. Customers will come all the time.");
        }
        else
        {
            dayIsOpen = clock.IsDay;

            clock.DayEnded += OnDayEnded;
            clock.NightEnded += OnNightEnded;
        }

        StartCoroutine(SpawnLoop());
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

        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.SendDoorQueueHome();
        }

        Debug.Log("CustomerSpawner: day ended, no new customers.");
    }

    private void OnNightEnded()
    {
        dayIsOpen = true;

        Debug.Log("CustomerSpawner: a new day began, customers are coming again.");
    }

    // --------------------------------------------------
    // SPAWNING (a coroutine: code that can wait)
    // --------------------------------------------------

    private IEnumerator SpawnLoop()
    {
        if (settings == null)
        {
            Debug.LogError("CustomerSpawner has no CustomerSettings assigned.");
            yield break;
        }

        while (true)
        {
            // 1. Wait until it is day, the shop is open, and a whole batch fits at the door
            yield return new WaitUntil(CanSpawnBatch);

            // 2. Wait the spawn delay (5 sec from the settings asset)
            yield return new WaitForSeconds(settings.doorSpawnDelay);

            // 3. Things may have changed while we waited (the day may have ended), so check again
            if (CanSpawnBatch())
            {
                for (int i = 0; i < settings.customersPerBatch; i++)
                {
                    SpawnCustomer();
                }
            }
        }
    }

    private bool CanSpawnBatch()
    {
        if (!dayIsOpen || !ShopIsOpen())
        {
            return false;
        }

        if (CustomerManager.Instance == null)
        {
            return false;
        }

        return CustomerManager.Instance.DoorFreeSpots >= settings.customersPerBatch;
    }

    // PLACEHOLDER until Shreyas's Open/Close sign (SCRUM-281) exists.
    // Then ask the sign: "is the shop open?" and return that here.
    private bool ShopIsOpen()
    {
        return true;
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