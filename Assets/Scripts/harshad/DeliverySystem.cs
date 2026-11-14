using System;
using System.Collections.Generic;
using UnityEngine;

// The delivery list. Singleton + DontDestroyOnLoad, created on start (same idea as Wallet).
// The shop calls Order(...). When the next in-game day starts, the items arrive.
public class DeliverySystem : MonoBehaviour
{
    public static DeliverySystem Instance { get; private set; }

    // Something was bought and now waits for the next day
    public event Action<DeliveryItem> ItemOrdered;

    // A new day started: these items arrived, so boxes should appear
    public event Action<List<DeliveryItem>> ItemsArrived;

    // The player picked up a box
    public event Action<DeliveryItem> ItemCollected;

    private readonly List<DeliveryItem> pending = new List<DeliveryItem>();
    private readonly List<DeliveryItem> arrived = new List<DeliveryItem>();

    private DayNightClock subscribedClock;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        if (Instance != null) return;
        new GameObject("DeliverySystem").AddComponent<DeliverySystem>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // The clock may be created after us, so keep looking until we find it
    private void Update()
    {
        if (subscribedClock == null && DayNightClock.Instance != null)
        {
            subscribedClock = DayNightClock.Instance;
            subscribedClock.NightEnded += DeliverNow;
        }
    }

    private void OnDestroy()
    {
        if (subscribedClock != null)
        {
            subscribedClock.NightEnded -= DeliverNow;
        }
    }

    // ---------------- THE SHOP USES THESE ----------------

    public void Order(DeliveryItem item)
    {
        pending.Add(item);

        Debug.Log("DeliverySystem: ordered " + item + ". It arrives when the next day starts.");
        ItemOrdered?.Invoke(item);
    }

    public void Order(DeliveryItemType type, string itemId, int amount = 1)
    {
        Order(new DeliveryItem(type, itemId, amount));
    }

    // ---------------- ASK QUESTIONS ----------------

    public int PendingCount => pending.Count;

    public List<DeliveryItem> GetPending() => new List<DeliveryItem>(pending);

    public List<DeliveryItem> GetArrived() => new List<DeliveryItem>(arrived);

    // ---------------- DELIVERY ----------------

    // Called by the clock when a new day starts. Also used by the test switch.
    public void DeliverNow()
    {
        if (pending.Count == 0) return;

        List<DeliveryItem> batch = new List<DeliveryItem>(pending);

        pending.Clear();
        arrived.AddRange(batch);

        Debug.Log("DeliverySystem: " + batch.Count + " item(s) arrived at the bakery entrance.");
        ItemsArrived?.Invoke(batch);
    }

    // The player pressed E on a box. Returns true if the item was handed over.
    public bool Collect(DeliveryItem item)
    {
        if (BakeryProgress.Instance == null)
        {
            Debug.LogError("DeliverySystem: BakeryProgress was not found.");
            return false;
        }

        int index = arrived.IndexOf(item);

        if (index < 0) return false;

        arrived.RemoveAt(index);

        switch (item.type)
        {
            case DeliveryItemType.Blueprint:
                BakeryProgress.Instance.AddBlueprint(item.itemId);
                break;

            case DeliveryItemType.Material:
                BakeryProgress.Instance.AddMaterial(item.itemId, item.amount);
                break;
        }

        Debug.Log("DeliverySystem: collected " + item);
        ItemCollected?.Invoke(item);

        return true;
    }
}