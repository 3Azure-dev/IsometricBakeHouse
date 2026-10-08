using UnityEngine;
using UnityEngine.InputSystem;

public class ExitDoor1 : MonoBehaviour
{
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    private bool playerInRange;
    private bool levelCompleted;

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
            !levelCompleted &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            levelCompleted = true;

            if (ePrompt != null)
                ePrompt.SetActive(false);

            if (levelCompleteUI != null)
                levelCompleteUI.ShowLevelComplete();
        }
    }
}