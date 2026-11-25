using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject howToPlayBackdrop;
    public GameObject howToPlayPanel;

    [Header("Play / Continue / New Game")]
    public GameObject continueButton;        // the main Play button object
    public TMP_Text playButtonLabel;         // the text on that button ("Play" / "Continue")
    public GameObject newGameButton;         // the New Game button (only with a save)
    public GameObject newGameConfirmPanel;   // the "are you sure" popup (starts inactive)
    private static bool hasPlayedThisSession;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionFlag()
    {
        hasPlayedThisSession = false;   // keeps the Editor correct even without a domain reload
    }

    private void Start()
    {
        RefreshButtons();
        if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshButtons();
    }

    // First time: "Play" only. With a save: "Continue" + "New Game".
    // "Play" every time the game starts. "Continue" only after coming back from the game.
    // New Game shows whenever a save exists.
    private void RefreshButtons()
    {
        if (continueButton != null)
            continueButton.SetActive(true);

        if (playButtonLabel != null)
            playButtonLabel.text = hasPlayedThisSession ? "Continue" : "Play";

        if (newGameButton != null)
            newGameButton.SetActive(SaveSystem.HasSave);
    }

    public void PlayGame()
    {
        // Continue (back from the game): keep the bag exactly as it is
        if (hasPlayedThisSession)
            IngredientShelf.SkipNextDeposit();

        EnterBakery();
    }

    private void EnterBakery()
    {
        hasPlayedThisSession = true;
        SceneManager.LoadScene("Bakery");
    }

    // New Game button: ask first (it only shows when a save exists)
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

        EnterBakery();
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