using System.Collections.Generic;
using UnityEngine;

public class PlayerRangeInteraction : MonoBehaviour
{
    [SerializeField] private string interactableTag = "Interactable";

    private readonly List<GameObject> nearbyObjects = new List<GameObject>();

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

        GameObject target = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        if (!nearbyObjects.Contains(target))
        {
            nearbyObjects.Add(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject target = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        nearbyObjects.Remove(target);
    }

    private void InteractWithNearestObject()
    {
        nearbyObjects.RemoveAll(target => target == null);

        if (nearbyObjects.Count == 0)
        {
            return;
        }

        GameObject target = nearbyObjects[0];

        Door door = target.GetComponentInParent<Door>();

        if (door != null)
        {
            door.ToggleDoor();
        }
        else
        {
            target.SetActive(true);
        }
    }
}