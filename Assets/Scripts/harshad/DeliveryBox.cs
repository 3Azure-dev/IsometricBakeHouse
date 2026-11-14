using UnityEngine;

// One box at the entrance. Press E to open it.
public class DeliveryBox : MonoBehaviour, IInteractable
{
    private DeliveryItem item;

    public void Setup(DeliveryItem newItem)
    {
        item = newItem;
        name = "DeliveryBox (" + item + ")";
    }

    public void Interact()
    {
        if (DeliverySystem.Instance == null) return;

        if (DeliverySystem.Instance.Collect(item))
        {
            Destroy(gameObject);
        }
    }
}