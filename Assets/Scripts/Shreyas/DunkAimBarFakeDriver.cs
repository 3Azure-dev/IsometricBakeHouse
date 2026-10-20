using UnityEngine;
using UnityEngine.InputSystem;

public class DunkAimBarFakeDriver : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DunkAimBarUI aimBarUI;

    [Header("Fake Dunk Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float fakeHitZoneCenter = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float fakeHitZoneSize = 0.2f;

    [Header("Fake Aim Movement")]
    [SerializeField] private float startingSpeed = 0.45f;
    [SerializeField] private float speedIncrease = 0.25f;
    [SerializeField] private float speedIncreaseInterval = 2f;

    private float aimValue;
    private float direction = 1f;
    private float currentSpeed;

    private float elapsed;
    private float nextSpeedIncrease;

    private bool fakeDunkActive;

    private void Update()
    {
        if (aimBarUI == null)
            return;

        // F begins a fake dunk.
        if (!fakeDunkActive)
        {
            if (Keyboard.current != null &&
                Keyboard.current.fKey.wasPressedThisFrame)
            {
                BeginFakeDunk();
            }

            return;
        }

        // Increase speed every two seconds.
        elapsed += Time.unscaledDeltaTime;

        if (speedIncreaseInterval > 0f)
        {
            while (elapsed >= nextSpeedIncrease)
            {
                currentSpeed += speedIncrease;
                nextSpeedIncrease += speedIncreaseInterval;
            }
        }

        // Generate a normalized value between 0 and 1.
        aimValue += direction *
                    currentSpeed *
                    Time.unscaledDeltaTime;

        if (aimValue >= 1f)
        {
            aimValue = 1f;
            direction = -1f;
        }
        else if (aimValue <= 0f)
        {
            aimValue = 0f;
            direction = 1f;
        }

        // The UI only follows the value it receives.
        aimBarUI.SetAimValue(aimValue);

        // A click represents the fake throw.
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            aimBarUI.RegisterThrow();
            fakeDunkActive = false;
        }
    }

    private void BeginFakeDunk()
    {
        aimValue = 0f;
        direction = 1f;

        currentSpeed = startingSpeed;
        elapsed = 0f;
        nextSpeedIncrease = speedIncreaseInterval;

        fakeDunkActive = true;

        aimBarUI.BeginDunk(
            fakeHitZoneCenter,
            fakeHitZoneSize
        );

        aimBarUI.SetAimValue(aimValue);
    }
}