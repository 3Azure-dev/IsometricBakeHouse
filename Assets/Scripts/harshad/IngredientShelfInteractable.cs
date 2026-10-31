
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
