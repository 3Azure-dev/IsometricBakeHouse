using UnityEngine;
using TMPro;

public class SackCounterUI1 : MonoBehaviour
{
    [SerializeField] private TMP_Text sackCountText;

    private void Start()
    {
        if (Inventory1.Instance == null)
        {
            Debug.LogError("Inventory1 not found!");
            return;
        }

        // Show the current inventory total
        UpdateTotalCount();

        // Subscribe to the Inventory event
        Inventory1.Instance.OnChanged += OnInventoryChanged;
    }

    private void OnDestroy()
    {
        if (Inventory1.Instance != null)
        {
            Inventory1.Instance.OnChanged -= OnInventoryChanged;
        }
    }

    private void OnInventoryChanged(IngredientData ingredient, int newCount)
    {
        UpdateTotalCount();
    }

    private void UpdateTotalCount()
    {
        int total = Inventory1.Instance.GetTotalIngredientCount();

        sackCountText.text = total.ToString();
    }
}