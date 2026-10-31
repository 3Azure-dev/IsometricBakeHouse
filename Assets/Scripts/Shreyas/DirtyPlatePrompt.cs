using UnityEngine;

/// Shows a plate icon + "E" prompt above dirty tables.
/// Automatically hides when plates are cleared.
public class DirtyPlatePrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BakeryTable table;       // Table this prompt hangs over
    [SerializeField] private GameObject promptRoot;   // The visual prompt (has WorldSpaceBillboard)

    private void Update()
    {
        // Check if the table has dirty plates
        bool hasDirtyPlates = table != null && table.HasDirtyPlates;

        // Show/hide the prompt based on dirty plates status
        if (promptRoot != null)
        {
            promptRoot.SetActive(hasDirtyPlates);
        }
    }
}