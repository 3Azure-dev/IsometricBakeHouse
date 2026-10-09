
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
