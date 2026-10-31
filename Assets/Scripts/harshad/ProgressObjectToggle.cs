using UnityEngine;

// Shows the target only if the player owns the equipment.
// Put this on an always-active parent, NOT on the station itself:
// a script on a hidden object stops running.
public class ProgressObjectToggle : MonoBehaviour
{
    [SerializeField] private EquipmentType equipment = EquipmentType.PizzaStone;
    [SerializeField] private GameObject target;

    private BakeryProgress subscribed;

    private void OnEnable()
    {
        subscribed = BakeryProgress.Instance;

        if (subscribed != null)
        {
            subscribed.Changed += OnChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (subscribed != null)
        {
            subscribed.Changed -= OnChanged;
            subscribed = null;
        }
    }

    private void OnChanged(ProgressChange change)
    {
        if (change == ProgressChange.Equipment) Refresh();
    }

    private void Refresh()
    {
        if (target == null) return;

        target.SetActive(BakeryProgress.Instance != null &&
                         BakeryProgress.Instance.HasEquipment(equipment));
    }
}