using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CanvasScalerBatch
{
    // Paste your real scene paths here (right-click scene > Copy Path)
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/IngredientWorld_Pizza.unity",
        "Assets/Scenes/IngredientWorld_Bread.unity",
        "Assets/Scenes/IngredientWorld_Cupcake.unity",
        "Assets/Scenes/MainMenu.unity",
        // Bakery goes here ONLY after Harshad's clean-up is in beta
    };

    private static void Apply(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(scaler);
    }

    private static bool IsTarget(CanvasScaler scaler)
    {
        Canvas canvas = scaler.GetComponent<Canvas>();
        return canvas != null && canvas.isRootCanvas && canvas.renderMode != RenderMode.WorldSpace;
    }

    // Version-safe scene walk: no FindObjectsByType at all
    private static int FixScene(Scene scene)
    {
        int count = 0;
        foreach (GameObject rootGo in scene.GetRootGameObjects())
        {
            foreach (CanvasScaler s in rootGo.GetComponentsInChildren<CanvasScaler>(true))
            {
                if (IsTarget(s)) { Apply(s); count++; }
            }
        }
        return count;
    }

    [MenuItem("Tools/UI Scaling/Fix Scalers in Open Scene")]
    private static void FixOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        int count = FixScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[UI Scaling] {scene.name}: fixed {count} screen canvases. Now Ctrl+S.");
    }

    [MenuItem("Tools/UI Scaling/Fix Scalers in Selected Prefabs")]
    private static void FixSelectedPrefabs()
    {
        int count = 0;
        foreach (string guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;
            bool changed = false;
            foreach (CanvasScaler s in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                if (IsTarget(s)) { Apply(s); changed = true; count++; }
            }
            if (changed) PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"[UI Scaling] Prefab fixed: {path}");
        }
        Debug.Log($"[UI Scaling] Prefab scalers fixed: {count}");
    }

    [MenuItem("Tools/UI Scaling/Fix All Hunt Levels + Main Menu")]
    private static void FixAllScenes()
    {
        foreach (string path in ScenePaths)
        {
            if (!System.IO.File.Exists(path)) { Debug.LogWarning($"[UI Scaling] Skipped, not found: {path}"); continue; }
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int count = FixScene(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[UI Scaling] {scene.name}: fixed {count} canvases and saved.");
        }
    }
}