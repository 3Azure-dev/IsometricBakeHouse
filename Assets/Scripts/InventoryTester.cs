using UnityEngine;

// Temporary test script. Delete it once the real catch works.
public class InventoryTester : MonoBehaviour
{
    public IngredientData testIngredient;

    void OnGUI()
    {
        int count = Inventory.Instance.GetCount(testIngredient);
        GUI.Label(new Rect(10, 10, 300, 30), testIngredient.name + ": " + count);

        if (GUI.Button(new Rect(10, 40, 120, 30), "Add 1"))
        {
            Inventory.Instance.Add(testIngredient);
        }
    }
}