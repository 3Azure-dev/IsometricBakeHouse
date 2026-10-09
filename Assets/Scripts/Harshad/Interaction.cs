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


    private void InteractWithNearestObject()
    {
        nearbyObjects.RemoveAll(
            target => target == null
        );

        if (nearbyObjects.Count == 0)
        {
            return;
        }

        GameObject target =
            nearbyObjects[0];


        // CUSTOMER
        CustomerController customer =
            target.GetComponentInParent<CustomerController>();

        if (customer != null)
        {
            customer.InteractWithPlayer();
            return;
        }

        StoveController stove =
    target.GetComponentInParent<StoveController>();

        if (stove != null)
        {
            stove.Interact();
            return;
        }


        // TRANSPORT
        Transport transport =
            target.GetComponentInParent<Transport>();

        if (transport != null)
        {
            transport.Interact();
            return;
        }


        // BUY ITEM DEVICE
        BuyItemDevice buyDevice =
            target.GetComponentInParent<BuyItemDevice>();

        if (buyDevice != null)
        {
            buyDevice.Interact();
            return;
        }


        // DOOR
        Door door =
            target.GetComponentInParent<Door>();

        if (door != null)
        {
            door.ToggleDoor();
        }
    }
}