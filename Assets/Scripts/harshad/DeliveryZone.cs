using System.Collections.Generic;
using UnityEngine;

// The Delivery Zone at the bakery entrance. Put this on an empty object on the floor.
// It spawns a box for every item that has arrived, including ones that arrived
// while the player was away hunting (the list lives in DeliverySystem, not in this scene).
public class DeliveryZone : MonoBehaviour
{
    [Header("Box (leave empty to use a cube until Pravin's model is in, SCRUM-357)")]
    [SerializeField] private GameObject boxPrefab;
    [SerializeField] private Vector3 cubeSize = new Vector3(0.6f, 0.6f, 0.6f);

    [Header("Layout (boxes are placed in rows along this object's right/forward)")]
    [SerializeField] private float spacing = 0.9f;
    [SerializeField] private int boxesPerRow = 4;

    // Which box sits in which slot, so a freed slot gets reused
    private readonly Dictionary<int, GameObject> slots = new Dictionary<int, GameObject>();

    private DeliverySystem subscribedSystem;

    private void Start()
    {
        subscribedSystem = DeliverySystem.Instance;

        if (subscribedSystem == null)
        {
            Debug.LogError("DeliveryZone: DeliverySystem was not found.");
            return;
        }

        subscribedSystem.ItemsArrived += OnItemsArrived;

        // Boxes that were already waiting (for example after coming back from a hunt)
        foreach (DeliveryItem item in subscribedSystem.GetArrived())
        {
            SpawnBox(item);
        }
    }

    private void OnDestroy()
    {
        if (subscribedSystem != null)
        {
            subscribedSystem.ItemsArrived -= OnItemsArrived;
        }
    }

    private void OnItemsArrived(List<DeliveryItem> items)
    {
        foreach (DeliveryItem item in items)
        {
            SpawnBox(item);
        }
    }

    private void SpawnBox(DeliveryItem item)
    {
        int slot = FindFreeSlot();

        int column = slot % Mathf.Max(1, boxesPerRow);
        int row = slot / Mathf.Max(1, boxesPerRow);

        Vector3 position = transform.position
                         + transform.right * (column * spacing)
                         + transform.forward * (row * spacing)
                         + Vector3.up * (cubeSize.y * 0.5f);

        GameObject box;

        if (boxPrefab != null)
        {
            box = Instantiate(boxPrefab, position, transform.rotation);
        }
        else
        {
            box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetPositionAndRotation(position, transform.rotation);
            box.transform.localScale = cubeSize;
        }

        // The player's E key only looks at objects with this tag (and a collider)
        box.tag = "Interactable";

        if (box.GetComponentInChildren<Collider>() == null)
        {
            box.AddComponent<BoxCollider>();
        }

        DeliveryBox deliveryBox = box.GetComponent<DeliveryBox>();

        if (deliveryBox == null)
        {
            deliveryBox = box.AddComponent<DeliveryBox>();
        }

        deliveryBox.Setup(item);

        slots[slot] = box;
    }

    private int FindFreeSlot()
    {
        int slot = 0;

        GameObject occupant;

        while (slots.TryGetValue(slot, out occupant) && occupant != null)
        {
            slot++;
        }

        return slot;
    }

    // Shows where the boxes will appear (only in the Scene view)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.3f, new Vector3(0.7f, 0.6f, 0.7f));
    }

    // ---------------- TEST BUTTONS ----------------
    // Select the DeliveryZone while playing, click the three dots on the component

    [ContextMenu("TEST: Order a blueprint and 5 wood")]
    private void TestOrder()
    {
        if (DeliverySystem.Instance == null) return;

        DeliverySystem.Instance.Order(DeliveryItemType.Blueprint, "Table_Basic");
        DeliverySystem.Instance.Order(DeliveryItemType.Material, "Wood", 5);
    }

    [ContextMenu("TEST: Deliver now (skip the wait)")]
    private void TestDeliverNow()
    {
        if (DeliverySystem.Instance != null)
        {
            DeliverySystem.Instance.DeliverNow();
        }
    }
}