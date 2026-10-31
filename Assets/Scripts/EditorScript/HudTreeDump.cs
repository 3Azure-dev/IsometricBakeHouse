using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HudTreeDump
{
    [MenuItem("Tools/UI Scaling/Dump HUD Tree")]
    static void Dump()
    {
        foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (go.name != "HUD" && go.name != "DunkAimCanvas") continue;
            foreach (var rt in go.GetComponentsInChildren<RectTransform>(true))
            {
                int depth = 0;
                var p = rt.parent;
                while (p != null) { depth++; p = p.parent; }
                Debug.Log($"{new string(' ', depth * 2)}{rt.name} | parent={(rt.parent ? rt.parent.name : "null")}");
            }
        }
        Debug.Log("[Dump] done — copy all lines above this one.");
    }
}