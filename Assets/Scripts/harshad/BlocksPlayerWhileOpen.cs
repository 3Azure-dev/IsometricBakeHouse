using UnityEngine;

// Put this on any panel that must freeze the player while it is open.
// It counts itself in when the panel turns on and out when it turns off.
public class BlocksPlayerWhileOpen : MonoBehaviour
{
    private static int openCount;

    public static bool IsBlocked { get { return openCount > 0; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCount()
    {
        openCount = 0;
    }

    private void OnEnable()
    {
        openCount++;
    }

    private void OnDisable()
    {
        openCount = Mathf.Max(0, openCount - 1);
    }
}