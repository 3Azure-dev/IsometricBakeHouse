using System.Collections.Generic;
using UnityEngine;

public class PlayerRangeInteraction : MonoBehaviour
{
    [SerializeField] private string interactableTag = "Interactable";

    private readonly List<GameObject> nearbyObjects =
        new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithNearestObject();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(interactableTag))
        {
            return;
        }

        GameObject target =
            other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        if (!nearbyObjects.Contains(target))
        {
            nearbyObjects.Add(target);
        }

        RangeHighlight highlight =
            other.GetComponentInParent<RangeHighlight>();

        if (highlight != null)
        {
            highlight.SetPlayerInRange(other, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject target =
            other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        nearbyObjects.Remove(target);

        RangeHighlight highlight =
            other.GetComponentInParent<RangeHighlight>();

        if (highlight != null)
        {
            highlight.SetPlayerInRange(other, false);
        }
    }

    // Goes through the objects in range, closest first.
    // The first one that can actually use the E press gets it.
    private void InteractWithNearestObject()
    {
        nearbyObjects.RemoveAll(target => target == null);

        if (nearbyObjects.Count == 0)
        {
            return;
        }

        // Work on a copy, sorted closest first
        List<GameObject> candidates = new List<GameObject>(nearbyObjects);
        candidates.Sort(CompareByDistance);

        foreach (GameObject target in candidates)
        {
            if (target != null && TryInteract(target))
            {
                return;
            }
        }

        Debug.Log("Nothing to interact with right now.");
    }

    private int CompareByDistance(GameObject a, GameObject b)
    {
        float distanceA = Vector3.Distance(transform.position, a.transform.position);
        float distanceB = Vector3.Distance(transform.position, b.transform.position);

        return distanceA.CompareTo(distanceB);
    }

    // Returns true if the object used the E press, false if it is not ready
    private bool TryInteract(GameObject target)
    {
        // CUSTOMER: only if they are ready (waiting to order, or waiting for food)
        CustomerController customer =
            target.GetComponentInParent<CustomerController>();

        if (customer != null)
        {
            if (!customer.CanInteractWithPlayer)
            {
                return false;
            }

            customer.InteractWithPlayer();
            return true;
        }

        // TABLE: only if it has dirty plates to clear
        BakeryTable table =
            target.GetComponentInParent<BakeryTable>();

        if (table != null)
        {
            if (!table.HasDirtyPlates)
            {
                return false;
            }

            table.Interact();
            return true;
        }

        // STOVE
        StoveController stove =
            target.GetComponentInParent<StoveController>();

        if (stove != null)
        {
            stove.Interact();
            return true;
        }

        // TRANSPORT
        Transport transport =
            target.GetComponentInParent<Transport>();

        if (transport != null)
        {
            transport.Interact();
            return true;
        }

        // BUY ITEM DEVICE
        BuyItemDevice buyDevice =
            target.GetComponentInParent<BuyItemDevice>();

        if (buyDevice != null)
        {
            buyDevice.Interact();
            return true;
        }

        // INGREDIENT SHELF
        IngredientShelfInteractable shelf =
            target.GetComponentInParent<IngredientShelfInteractable>();

        if (shelf != null)
        {
            shelf.Interact();
            return true;
        }

        // OVEN
        OvenController oven =
            target.GetComponentInParent<OvenController>();

        if (oven != null)
        {
            oven.Interact();
            return true;
        }

        // DOOR
        Door door =
            target.GetComponentInParent<Door>();

        if (door != null)
        {
            door.ToggleDoor();
            return true;
        }

        return false;
    }
}