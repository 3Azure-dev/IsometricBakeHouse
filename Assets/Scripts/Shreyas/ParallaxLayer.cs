using UnityEngine;

/// <summary>
/// Put this on every background layer (sky, far, middle, near).
/// parallaxSpeed = how fast the layer scrolls across the screen compared to the world:
///   0   = glued to the camera (sky, never scrolls)
///   0.1 = far, scrolls very slowly
///   0.6 = near, scrolls almost like the ground
///   1   = moves with the world (same as gameplay layer)
/// The layer clones its own sprite left and right so there is never a gap.
/// Use a sprite whose left and right edges match (placeholder colours always do).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class ParallaxLayer : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Leave empty to use Camera.main")]
    [SerializeField] private Camera targetCamera;

    [Range(0f, 1f)]
    [Tooltip("0 = glued to camera (sky). Far = small, near = bigger. 1 = moves with the world.")]
    [SerializeField] private float parallaxSpeed = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Same idea on Y. 1 = layer stays fixed in the world vertically, 0 = follows camera Y fully.")]
    [SerializeField] private float verticalSpeed = 0f;

    [Tooltip("Repeat the sprite side to side")]
    [SerializeField] private bool tileHorizontally = true;

    [Tooltip("Flip every second tile so left/right edges always match. Use when the art is not seamless (e.g. Mohan's far tree layer).")]
    [SerializeField] private bool mirrorAlternateTiles = false;

    private Transform cam;
    private SpriteRenderer sr;
    private float startX, startY;
    private float camStartX, camStartY;
    private float length;
    private bool initialised;

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null)
        {
            Debug.LogWarning("ParallaxLayer: no camera found on " + name, this);
            enabled = false;
            return;
        }

        cam = targetCamera.transform;
        sr = GetComponent<SpriteRenderer>();

        startX = transform.position.x;
        startY = transform.position.y;
        camStartX = cam.position.x;
        camStartY = cam.position.y;
        length = sr.bounds.size.x;

        if (tileHorizontally && length > 0.01f) SpawnClones();

        initialised = true;
    }

    private void SpawnClones()
    {
        // How many copies each side so the camera never sees past the edge.
        float halfWidth = targetCamera.orthographic
            ? targetCamera.orthographicSize * targetCamera.aspect
            : 20f;
        int n = Mathf.CeilToInt(halfWidth / length) + 2;

        for (int i = -n; i <= n; i++)
        {
            if (i == 0) continue;

            GameObject clone = new GameObject(name + "_tile_" + i);
            clone.layer = gameObject.layer;

            SpriteRenderer c = clone.AddComponent<SpriteRenderer>();
            c.sprite = sr.sprite;
            c.color = sr.color;
            bool mirror = mirrorAlternateTiles && (Mathf.Abs(i) % 2 == 1);
            c.flipX = mirror ? !sr.flipX : sr.flipX;
            c.flipY = sr.flipY;
            c.sharedMaterial = sr.sharedMaterial;
            c.sortingLayerID = sr.sortingLayerID;
            c.sortingOrder = sr.sortingOrder;

            clone.transform.position = transform.position + Vector3.right * (length * i);
            clone.transform.rotation = transform.rotation;
            clone.transform.localScale = transform.lossyScale;
            clone.transform.SetParent(transform, true);
        }
    }

    private void LateUpdate()
    {
        if (!initialised) return;

        Vector3 camPos = cam.position;

        float x = startX + (camPos.x - camStartX) * (1f - parallaxSpeed);
        float y = startY + (camPos.y - camStartY) * (1f - verticalSpeed);

        if (tileHorizontally && length > 0.01f)
        {
            // Jump the layer one tile width when it drifts a full tile from the camera.
            // Tiles are identical, so the jump is invisible.
            float rel = x - camPos.x;
            // With mirrored tiles the pattern repeats every 2 tiles, so jump 2 tiles to keep it aligned.
            float step = mirrorAlternateTiles ? length * 2f : length;
            while (rel > length) { startX -= step; x -= step; rel -= step; }
            while (rel < -length) { startX += step; x += step; rel += step; }
        }

        transform.position = new Vector3(x, y, transform.position.z);
    }
}