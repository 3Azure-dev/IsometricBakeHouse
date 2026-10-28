using UnityEngine;

// Camera follows the player in the 3D isometric Bakery.
// It keeps the angle and distance you set up in the scene, and only moves along with the player.
public class CameraFollow3D : MonoBehaviour
{
    [Header("Target (drag the Player here, or leave empty to find it by name)")]
    [SerializeField] private Transform target;
    [SerializeField] private string playerName = "Player";

    [Header("Offset")]
    [Tooltip("ON: use the distance between camera and player when the game starts. OFF: use the offset below.")]
    [SerializeField] private bool useStartingOffset = true;
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -10f);

    [Header("Smoothing")]
    [Tooltip("Higher = the camera lags behind more. 0 = stuck to the player.")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Limits (stop the camera showing outside the bakery)")]
    [SerializeField] private bool useLimits = false;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minZ = -30f;
    [SerializeField] private float maxZ = 10f;

    private Vector3 velocity;

    private void Start()
    {
        FindTarget();

        if (target != null && useStartingOffset)
        {
            offset = transform.position - target.position;
        }
    }

    // LateUpdate runs after the player has moved, so the camera never shakes
    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        Vector3 goal = target.position + offset;

        if (useLimits)
        {
            goal.x = Mathf.Clamp(goal.x, minX, maxX);
            goal.z = Mathf.Clamp(goal.z, minZ, maxZ);
        }

        transform.position = Vector3.SmoothDamp(
            transform.position, goal, ref velocity, smoothTime);
    }

    private void FindTarget()
    {
        if (target != null) return;

        GameObject player = GameObject.Find(playerName);

        if (player != null)
        {
            target = player.transform;
        }
    }

    // Snap the camera to the player at once (for example after a teleport)
    public void SnapToTarget()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        velocity = Vector3.zero;
    }
}