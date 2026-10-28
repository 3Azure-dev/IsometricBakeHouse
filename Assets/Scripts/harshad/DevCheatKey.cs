using UnityEngine;
using UnityEngine.InputSystem;

public class DevCheatKey : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] private int coinsToAdd = 1000;
    [SerializeField] private int diamondsToAdd = 100;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !keyboard.f1Key.wasPressedThisFrame)
            return;

        if (Wallet.Instance == null)
        {
            Debug.LogWarning("DevCheatKey: Wallet was not found.");
            return;
        }

        Wallet.Instance.AddCoins(coinsToAdd);
        Wallet.Instance.AddDiamonds(diamondsToAdd);

        Debug.Log("DevCheatKey: +" + coinsToAdd + " coins, +" + diamondsToAdd + " diamonds");
    }
#endif
}