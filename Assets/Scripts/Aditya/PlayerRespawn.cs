using UnityEngine;

// Remembers where the player entered the latest mould zone.
// When he falls in an escape hole, he is sent back to that spot.
// Observer pattern: listens to MouldZone.PlayerEntered.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRespawn : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector3 _checkpoint;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _checkpoint = transform.position; // level start, until the first zone
    }

    private void OnEnable()  => MouldZone.PlayerEntered += OnZoneEntered;
    private void OnDisable() => MouldZone.PlayerEntered -= OnZoneEntered;

    private void OnZoneEntered(MouldZone zone)
    {
        _checkpoint = transform.position; // the spot just before the zone
    }

    public void Respawn()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = _checkpoint;
        Debug.Log("Player fell in a hole, back to the latest zone");
    }
}