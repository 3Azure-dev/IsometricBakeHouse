using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    [SerializeField] private AudioMixer _mixer; // this scene's mixer
    [SerializeField] private GameObject optionsRoot; // the options canvas inside this prefab

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        if (optionsRoot != null) optionsRoot.SetActive(false);
        OptionsMenu.ApplySaved(_mixer);
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Esc closes Options first, then the pause menu
        if (optionsRoot != null && optionsRoot.activeSelf)
        {
            optionsRoot.SetActive(false);
            return;
        }

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (optionsRoot != null) optionsRoot.SetActive(false);
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenOptions()
    {
        if (optionsRoot != null) optionsRoot.SetActive(true);
    }

    public void MainMenu()
    {
      
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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