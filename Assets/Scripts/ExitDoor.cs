using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ExitDoor1 : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Bakery";
    [SerializeField] private GameObject ePrompt;

    private bool playerInRange;

    private void Start()
    {
        if (ePrompt != null)
            ePrompt.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (ePrompt != null)
                ePrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (ePrompt != null)
                ePrompt.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}