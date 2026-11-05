using System.Collections;
using UnityEngine;
using UnityEngine.AI; // <-- ADDED THIS FOR NAVMESH FUNCTIONS

public class CustomerSpawner : MonoBehaviour
{
    [Header("Settings (drag SO_CustomerSettings here)")]
    [SerializeField] private CustomerSettings settings;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Shop Sign (Shreyas's Open/Close sign, SCRUM-281)")]
    [SerializeField] private ShopOpenCloseSign shopSign;

    // Customers only come while the sign says OPEN
    private bool shopIsOpen;

    private DayNightClock clock;

    // Customers only come in the Day Phase
    private bool dayIsOpen = true;

    // Set by the test button: the next customer is a VIP
    private bool nextIsVip;

    [ContextMenu("TEST: Next customer is VIP")]
    private void TestNextCustomerIsVip()
    {
        nextIsVip = true;
        Debug.Log("CustomerSpawner: the next customer will be a VIP.");
    }

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

        if (shopSign != null)
        {
            shopIsOpen = shopSign.IsOpen;
            shopSign.OnShopStateChanged += OnShopStateChanged;
        }
        else
        {
            Debug.LogWarning("CustomerSpawner: no Shop Sign assigned. The shop counts as always open.");
        }

        StartCoroutine(SpawnLoop());
    }

    private void OnShopStateChanged(bool isOpen)
    {
        shopIsOpen = isOpen;

        Debug.Log(isOpen
            ? "CustomerSpawner: shop opened. Customers can come."
            : "CustomerSpawner: shop closed. No new customers.");
    }

    private void OnDestroy()
    {
        if (shopSign != null)
        {
            shopSign.OnShopStateChanged -= OnShopStateChanged;
        }

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
    // SPAWNING
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
            yield return new WaitUntil(CanSpawnBatch);
            yield return new WaitForSeconds(settings.doorSpawnDelay);

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
        if (!dayIsOpen || !ShopIsOpen()) return false;
        if (CustomerManager.Instance == null) return false;

        return CustomerManager.Instance.DoorFreeSpots >= settings.customersPerBatch;
    }

    private bool ShopIsOpen()
    {
        if (shopSign == null) return true;
        return shopIsOpen;
    }

    private void SpawnCustomer()
    {
        CustomerType type;

        if (nextIsVip)
        {
            type = CustomerType.VIP;
            nextIsVip = false;
        }
        else
        {
            int today = DayNightClock.Instance != null ? DayNightClock.Instance.DayNumber : 1;

            if (today < settings.vipFirstDay)
            {
                type = CustomerType.Normal;
            }
            else
            {
                float randomNumber = Random.Range(0f, 100f);
                type = randomNumber <= settings.vipChancePercent ? CustomerType.VIP : CustomerType.Normal;
            }
        }

        GameObject customer = CustomerPool.Instance.GetCustomer(type);
        if (customer == null) return;

        // --- 🌟 NAVMESH SAFE SPAWN FIX 🌟 ---
        // This guarantees the customer snaps to the nearest valid walkable floor, 
        // even if the spawnPoint is floating slightly in the air!
        Vector3 safeSpawnPos = spawnPoint.position;

        if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            safeSpawnPos = hit.position; // Snap to the blue NavMesh grid
        }
        else
        {
            Debug.LogWarning("CustomerSpawner: Could not find valid NavMesh near spawn point! Lower the spawn point Y position.");
        }

        customer.transform.position = safeSpawnPos;
        customer.transform.rotation = spawnPoint.rotation;
        // ------------------------------------

        CustomerController controller = customer.GetComponent<CustomerController>();
        controller.StartCustomer(type);
    }
}