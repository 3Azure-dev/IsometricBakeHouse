using UnityEngine;

public class DirtyPlatePromptManager : MonoBehaviour
{
    [Header("Prompts")]
    [SerializeField] private GameObject[] platePromptObjects;  // Changed to GameObject
    [SerializeField] private Transform[] dirtyPlates;
    [SerializeField] private float promptRange = 3f;
    [SerializeField] private Transform player;

    private DirtyPlatePrompt[] platePrompts;  // Cache the components
    private int nearestPlateIndex = -1;

    private void Start()
    {
        // Convert GameObjects to DirtyPlatePrompt components
        platePrompts = new DirtyPlatePrompt[platePromptObjects.Length];
        for (int i = 0; i < platePromptObjects.Length; i++)
        {
            platePrompts[i] = platePromptObjects[i].GetComponent<DirtyPlatePrompt>();
        }
    }
    private void Update()
    {
        FindNearestPlate();
        UpdatePrompts();
    }

    private void FindNearestPlate()
    {
        // 1. Safely find the player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
            else
            {
                return; // Stop here if no player exists!
            }
        }

        // 2. Safety check for arrays
        if (dirtyPlates == null || platePrompts == null) return;
        if (dirtyPlates.Length != platePrompts.Length)
        {
            Debug.LogError("DirtyPlatePromptManager: Dirty Plates and Plate Prompts arrays must be the same size!");
            return;
        }

        float nearestDistance = float.MaxValue;
        nearestPlateIndex = -1;

        for (int i = 0; i < dirtyPlates.Length; i++)
        {
            // 3. Safety check for empty slots in the Inspector
            if (dirtyPlates[i] == null || platePrompts[i] == null) continue;

            // Only consider plates that are actually dirty
            if (!platePrompts[i].IsReady()) continue;

            float distance = Vector3.Distance(player.position, dirtyPlates[i].position);

            if (distance < nearestDistance && distance <= promptRange)
            {
                nearestDistance = distance;
                nearestPlateIndex = i;
            }
        }
    }
    private void UpdatePrompts()
    {
        for (int i = 0; i < platePrompts.Length; i++)
        {
            if (platePrompts[i] == null) continue;

            // Check if this is the nearest plate
            bool isNearest = (i == nearestPlateIndex);

            // IMPORTANT: Disable the PROMPT SCRIPT itself, not just the canvas.
            // This stops the ProximityPromptBase from running its own distance checks.
            platePrompts[i].enabled = isNearest;
        }
    }
}