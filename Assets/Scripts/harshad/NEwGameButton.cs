using UnityEngine;

// On the New Game button. Always reaches the SaveSystem that is still alive,
// even after coming back from the Bakery.
public class NewGameButton : MonoBehaviour
{
    public void Press()
    {
        if (SaveSystem.Instance != null)
            SaveSystem.Instance.NewGame();
        else
            Debug.LogWarning("NewGameButton: no SaveSystem found.");
    }
}