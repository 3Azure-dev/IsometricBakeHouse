using UnityEngine;
using UnityEngine.SceneManagement;

public class Transport : MonoBehaviour
{
    [SerializeField] private GameObject Worlds;

    public void Interact()
    {
        Debug.Log("open");

        if (Worlds != null)
        {
            Worlds.SetActive(true);
        }
    }

    public void Close()
    {
        Worlds.SetActive(false);

    }

    public void ChangeScene(string IngredientWorld_Bread)
    {
        SceneManager.LoadScene(IngredientWorld_Bread);
    }
}