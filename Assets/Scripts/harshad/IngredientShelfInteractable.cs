
using UnityEngine;

public class IngredientShelfInteractable : MonoBehaviour
{
    [SerializeField] private IngredientShelf shelf;

    public void Interact()
    {
        if (IngredientShelfUI.Instance == null)
        {
            Debug.LogError("IngredientShelfUI was not found in the scene.");
            return;
        }

        int moved = shelf.DepositOncePerVisit();

        IngredientShelfUI.Instance.Open(shelf);

        IngredientShelfUI.Instance.ShowMessage(
            moved > 0
                ? shelf.LastDepositSummary + ". Press Take to carry what you need."
                : "Press Take to carry ingredients. Stations cook from your bag.");
    }

    private void Awake()
    {
        if (shelf == null)
        {
            shelf = GetComponent<IngredientShelf>();
        }
    }
}
