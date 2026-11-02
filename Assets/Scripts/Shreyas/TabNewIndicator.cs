using UnityEngine;

public class TabNewIndicator : MonoBehaviour
{
    [Header("New Item Dot")]
    [Tooltip("Drag the 'NewDot' child object here")]
    [SerializeField] private GameObject newDot;

    // Call this when the tab is clicked to hide the dot
    public void HideDot()
    {
        if (newDot != null)
        {
            newDot.SetActive(false);
        }
    }

    // Call this when a level-up unlocks something for this tab
    public void ShowDot()
    {
        if (newDot != null)
        {
            newDot.SetActive(true);
        }
    }
}