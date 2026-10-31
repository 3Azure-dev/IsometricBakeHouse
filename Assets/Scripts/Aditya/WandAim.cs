using UnityEngine;
using UnityEngine.InputSystem;

public class WandAim : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    // other scripts (the fire script) read this to know where to shoot
    public Vector2 AimDirection { get; private set; } = Vector2.right;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        // mouse is in screen pixels -> convert to world position
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = _camera.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2)transform.position;
        if (direction.sqrMagnitude < 0.0001f) return; // mouse exactly on player, skip

        AimDirection = direction.normalized;

        // turn the direction into an angle and rotate the pivot
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}