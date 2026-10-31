
using UnityEngine;

public class IngredientShelfInteractable : MonoBehaviour
{
    [SerializeField] private IngredientShelf shelf;

    public void Interact()
    {
        // Ingredients now stay in Inventory1 (one store for hunts, the starting state and cooking).
        // The Starting State script on the player toggles the counter panel on the same E press,
        // so nothing else is needed here.
        Debug.Log("Shelf: ingredients are kept in the ingredient bag. The counter panel shows the amounts.");
    }

    private void Awake()
    {
        if (shelf == null)
        {
            shelf = GetComponent<IngredientShelf>();
        }
    }
}
