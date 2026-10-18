
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

        // Open() clears the message, so show what was put on the shelf AFTER opening
        if (!string.IsNullOrEmpty(shelf.LastDepositSummary))
        {
            IngredientShelfUI.Instance.ShowMessage(shelf.LastDepositSummary);
        }
    }

    private void Awake()
    {
        if (shelf == null)
        {
            shelf = GetComponent<IngredientShelf>();
        }
    }
}
