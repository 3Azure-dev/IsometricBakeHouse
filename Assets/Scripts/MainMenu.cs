using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject howToPlayBackdrop;
    public GameObject howToPlayPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("Bakery");
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