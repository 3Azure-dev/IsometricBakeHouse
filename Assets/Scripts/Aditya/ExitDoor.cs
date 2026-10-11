using UnityEngine;
using UnityEngine.InputSystem;

public class ExitDoor1 : MonoBehaviour
{
    [SerializeField] private GameObject ePrompt;
    [SerializeField] private LevelCompleteUI levelCompleteUI;

    private bool playerInRange;
    private bool levelCompleted;

    private bool unlocked;

    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.green;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = lockedColor;
    }

    private void OnEnable()  => LevelTracker.LevelEmpty += Unlock;
    private void OnDisable() => LevelTracker.LevelEmpty -= Unlock;

    private void Unlock(int caught)
    {
        unlocked = true;
        if (sr != null) sr.color = unlockedColor;
        Debug.Log("Exit door unlocked");

        if (playerInRange && ePrompt != null)
            ePrompt.SetActive(true);
    }
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

            if (unlocked && ePrompt != null)
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
        if (unlocked && playerInRange &&
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