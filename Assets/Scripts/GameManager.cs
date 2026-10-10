using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Bakery, Hunting, Paused }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // other scripts listen to this. It sends the new state.
    public event Action<GameState> StateChanged;

    public GameState State { get; private set; } = GameState.Menu;

    private GameState _stateBeforePause;

    // makes itself when the game starts, same as the Wallet
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateOnStart()
    {
        if (Instance != null) return;
        new GameObject("GameManager").AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // picks the state from the scene name
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.StartsWith("IngredientWorld")) SetState(GameState.Hunting);
        else if (scene.name.StartsWith("Bakery"))     SetState(GameState.Bakery);
        else                                          SetState(GameState.Menu);
    }

    // the pause menu calls this
    public void SetPaused(bool paused)
    {
        if (paused && State != GameState.Paused)
        {
            _stateBeforePause = State;
            SetState(GameState.Paused);
        }
        else if (!paused && State == GameState.Paused)
        {
            SetState(_stateBeforePause);
        }
    }

    private void SetState(GameState newState)
    {
        if (newState == State) return;

        State = newState;
        StateChanged?.Invoke(State);
        Debug.Log($"GameManager: state is now {State}");
    }
}