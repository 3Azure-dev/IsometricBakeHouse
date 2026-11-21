using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject howToPlayBackdrop;
    public GameObject howToPlayPanel;

    [Header("Continue / New Game")]
    public GameObject continueButton;        // the existing Play button object
    public GameObject newGameConfirmPanel;   // the "are you sure" popup (starts inactive)

    private void Start()
    {
        RefreshButtons();
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshButtons();
    }

    // Continue is only shown when a save exists
    private void RefreshButtons()
    {
        if (continueButton != null)
            continueButton.SetActive(SaveSystem.HasSave);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Bakery");
    }

    // New Game button: ask first if a save exists
    public void OnNewGamePressed()
    {
        if (SaveSystem.HasSave && newGameConfirmPanel != null)
            newGameConfirmPanel.SetActive(true);
        else
            StartNewGame();
    }

    // "Yes" on the popup
    public void StartNewGame()
    {
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);

        if (SaveSystem.Instance != null)
            SaveSystem.Instance.NewGame();

        PlayGame();
    }

    // "No" on the popup
    public void CancelNewGame()
    {
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    public void OpenHowToPlay()
    {
        mainMenuUI.SetActive(false);
        howToPlayBackdrop.SetActive(true);
        howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        mainMenuUI.SetActive(true);
        howToPlayBackdrop.SetActive(false);
        howToPlayPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}