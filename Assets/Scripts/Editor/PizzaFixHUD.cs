using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PizzaHudFix
{
    static Vector2 V(float x, float y) => new Vector2(x, y);

    static RectTransform FindRT(string name, string requiredParent)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name != name) continue;
                string parentName = rt.parent ? rt.parent.name : "null";
                if (parentName != requiredParent)
                {
                    Debug.LogWarning($"[PizzaHudFix] SKIPPED {name}: parent is '{parentName}', expected '{requiredParent}'.");
                    return null;
                }
                return rt;
            }
        Debug.LogWarning($"[PizzaHudFix] NOT FOUND: {name}");
        return null;
    }

    static void Set(string name, string parent, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos)
    {
        var rt = FindRT(name, parent);
        if (rt == null) return;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot; rt.anchoredPosition = pos;
        EditorUtility.SetDirty(rt);
        Debug.Log($"[PizzaHudFix] set {name} under {parent}");
    }

    static void SetWithSize(string name, string parent, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var rt = FindRT(name, parent);
        if (rt == null) return;
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot; rt.anchoredPosition = pos; rt.sizeDelta = size;
        EditorUtility.SetDirty(rt);
        Debug.Log($"[PizzaHudFix] set {name} under {parent} (size {size})");
    }

    [MenuItem("Tools/UI Scaling/Fix Pizza HUD Rects")]
    static void Fix()
    {
        SetWithSize("DesignRoot", "HUD", V(0, 0), V(1, 1), V(.5f, .5f), V(0, 0), V(0, 0));
        SetWithSize("IngredidentCountsRow", "DesignRoot", V(0, 1), V(0, 1), V(0, .5f), V(40, -60), V(500, 90));
        Set("ZonesText", "DesignRoot", V(.5f, 1), V(.5f, 1), V(.5f, .5f), V(0, -60));
        Set("IngredidentSackCount", "DesignRoot", V(.5f, 1), V(.5f, 1), V(0, .5f), V(60, -60));
        Set("ExitStatusText", "DesignRoot", V(1, 1), V(1, 1), V(.5f, .5f), V(-80, -60));
        Set("CreaturesText", "DesignRoot", V(0, 1), V(0, 1), V(0, .5f), V(40, -220));
        SetWithSize("ZonesBackground", "DesignRoot", V(0, 1), V(1, 1), V(.5f, .5f), V(0, -160), V(-600, 36));
        SetWithSize("ZonesBarFill", "ZonesBackground", V(0, 0), V(1, 1), V(.5f, .5f), V(0, 0), V(0, 0));
        Set("DunkSlider", "DunkAimPanel", V(1, .5f), V(1, .5f), V(1, .5f), V(-60, 0));
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[PizzaHudFix] Done. Check warnings above, then Ctrl+S.");
    }
}