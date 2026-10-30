using UnityEngine;
using UnityEngine.InputSystem;

// SCRUM-393: move a furniture ghost on the floor, click to place the blueprint.
public class BuildPlacer : MonoBehaviour
{
    [Header("Test (until the shop sends items)")]
    [SerializeField] private FurnitureData testItem;

    [Header("Floor and blocking")]
    [SerializeField] private LayerMask floorLayer;
    [Tooltip("Things a blueprint can't overlap: walls, tables, counter. NOT the floor.")]
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField, Min(0.1f)] private float gridSize = 1f;

    [Header("Ghost look")]
    [SerializeField] private Material ghostOk;
    [SerializeField] private Material ghostBlocked;

    private Camera cam;
    private FurnitureData item;
    private GameObject ghost;
    private Renderer[] ghostRenderers;
    private Material currentGhostMat;
    private Bounds ghostBounds;
    private bool canPlace;

    // State: Placing while a ghost exists, Idle otherwise
    public bool IsPlacing => ghost != null;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (!IsPlacing)
        {
            if (Keyboard.current.bKey.wasPressedThisFrame) StartPlacing(testItem);
            return;
        }

        MoveGhost();

        if (Keyboard.current.rKey.wasPressedThisFrame)
            ghost.transform.Rotate(0f, 90f, 0f);

        if (Mouse.current.leftButton.wasPressedThisFrame && canPlace)
            Place();
        else if (Mouse.current.rightButton.wasPressedThisFrame)
            Cancel();
    }

    public void StartPlacing(FurnitureData data)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning("BuildPlacer: item has no prefab");
            return;
        }

        Cancel();
        item = data;
        currentGhostMat = null;

        ghost = Instantiate(data.prefab);
        ghost.name = "Ghost_" + data.furnitureName;

        // The ghost is only a picture: no collisions, no scripts running
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (Behaviour b in ghost.GetComponentsInChildren<Behaviour>()) b.enabled = false;

        ghostRenderers = ghost.GetComponentsInChildren<Renderer>();
    }

    private void MoveGhost()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, floorLayer)) return;

        Vector3 p = hit.point;
        p.x = Mathf.Round(p.x / gridSize) * gridSize;
        p.z = Mathf.Round(p.z / gridSize) * gridSize;
        ghost.transform.position = p;

        canPlace = !IsBlocked();
        SetGhostMaterial(canPlace ? ghostOk : ghostBlocked);
    }

    private bool IsBlocked()
    {
        if (ghostRenderers.Length == 0) return false;

        ghostBounds = ghostRenderers[0].bounds;
        foreach (Renderer r in ghostRenderers) ghostBounds.Encapsulate(r.bounds);

        return Physics.CheckBox(ghostBounds.center, ghostBounds.extents * 0.9f,
            Quaternion.identity, blockingLayers, QueryTriggerInteraction.Ignore);
    }

    private void SetGhostMaterial(Material mat)
    {
        if (mat == currentGhostMat) return;
        currentGhostMat = mat;

        foreach (Renderer r in ghostRenderers)
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }

    private void Place()
    {
        // Colliders back on, so the next ghost can't overlap this blueprint
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>()) c.enabled = true;

        ghost.AddComponent<Blueprint>().Init(item);
        Debug.Log($"BuildPlacer: placed {item.furnitureName} blueprint at {ghost.transform.position}");

        ghost = null;
        item = null;
    }

    private void Cancel()
    {
        if (ghost != null) Destroy(ghost);
        ghost = null;
        item = null;
    }

    [ContextMenu("Test: place test item")]
    private void TestPlace() => StartPlacing(testItem);

    private void OnDrawGizmos()
    {
        if (!IsPlacing) return;
        Gizmos.color = canPlace ? Color.green : Color.red;
        Gizmos.DrawWireCube(ghostBounds.center, ghostBounds.size);
    }
}