
using UnityEngine;

public class IngredientShelfInteractable : MonoBehaviour
{
    [SerializeField] private IngredientShelf shelf;

    public void Interact()
    {
        if (shelf == null)
        {
            Debug.LogError("IngredientShelf is not assigned.");
            return;
        }

        if (IngredientShelfUI.Instance == null)
        {
            Debug.LogError("IngredientShelfUI was not found.");
            return;
        }

        // E at the shelf: first put everything the player hunted onto the shelf (SCRUM-142),
        // then open the shelf window so ingredients can be taken for cooking.
        shelf.DepositHuntedIngredients();

        IngredientShelfUI.Instance.Open(shelf);
    }

    private void Awake()
    {
        if (shelf == null)
        {
            shelf = GetComponent<IngredientShelf>();
        }
    }
}
