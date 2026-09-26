using UnityEngine;

public class BuyItemDevice : MonoBehaviour
{
    [SerializeField] private GameObject buyItemPanel;

    public void Interact()
    {
        Debug.Log("yes");

        if (buyItemPanel != null)
        {
            buyItemPanel.SetActive(true);
        }
    }
}