using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixOrderUI
{
    [MenuItem("Tools/Fix Customer Order UI (selected root)")]
    private static void Fix()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null || FindChild(root.transform, "OrderBubbwidget") == null)
        {
            EditorUtility.DisplayDialog("Fix Order UI",
                "Select the customer PREFAB asset (Capsule) in the Project window first.", "OK");
            return;
        }

        Transform widgetT = FindChild(root.transform, "OrderBubbwidget");
        RectTransform widget = widgetT.GetComponent<RectTransform>();

        Canvas canvas = widgetT.GetComponent<Canvas>() ?? widgetT.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        if (widgetT.GetComponent<GraphicRaycaster>() == null) widgetT.gameObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = widgetT.GetComponent<CanvasScaler>() ?? widgetT.gameObject.AddComponent<CanvasScaler>();
        scaler.referencePixelsPerUnit = 100f;

        CenterRect(widget, new Vector2(100f, 100f));
        widgetT.localPosition = new Vector3(0f, 1.5f, 0f);
        widgetT.localRotation = Quaternion.identity;
        widgetT.localScale = Vector3.one;

        Transform bgT = FindChild(widgetT, "BubbleBackground");
        if (bgT != null) CenterRect(bgT.GetComponent<RectTransform>(), new Vector2(90f, 90f));

        Image ring = null;
        Transform ringT = FindChild(widgetT, "TimerRing");
        if (ringT != null)
        {
            ring = ringT.GetComponent<Image>();
            if (ring != null)
            {
                ring.type = Image.Type.Filled;
                ring.fillMethod = Image.FillMethod.Radial360;
            }
            CenterRect(ringT.GetComponent<RectTransform>(), new Vector2(70f, 70f));
        }

        TMP_Text text = null;
        Transform textT = FindChild(widgetT, "TimerText");
        if (textT != null)
        {
            text = textT.GetComponent<TMP_Text>();
            CenterRect(textT.GetComponent<RectTransform>(), new Vector2(70f, 70f)); // kills Pos Z = 20.29
            if (text != null)
            {
                text.enableAutoSizing = false;
                text.fontSize = 34f;               // kills Font Size = 0.5
                text.alignment = TextAlignmentOptions.Center;
                text.margin = Vector4.zero;
            }
        }

        if (widgetT.GetComponent<WorldSpaceBillboard>() == null)
            widgetT.gameObject.AddComponent<WorldSpaceBillboard>();

        OrderTimer timer = root.GetComponentInChildren<OrderTimer>(true);
        if (timer != null)
        {
            SerializedObject so = new SerializedObject(timer);
            so.FindProperty("timerVisualRoot").objectReferenceValue = widgetT.gameObject;
            if (ring != null) so.FindProperty("timerRing").objectReferenceValue = ring;
            if (text != null) so.FindProperty("timerText").objectReferenceValue = text;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(timer);
        }

        Transform thinkT = FindChild(root.transform, "thinking");
        if (thinkT != null)
        {
            Canvas tc = thinkT.GetComponent<Canvas>() ?? thinkT.GetComponentInChildren<Canvas>(true);
            if (tc != null)
            {
                tc.renderMode = RenderMode.WorldSpace;
                tc.worldCamera = Camera.main;
                if (tc.transform.GetComponent<WorldSpaceBillboard>() == null)
                    tc.transform.gameObject.AddComponent<WorldSpaceBillboard>();
                EditorUtility.SetDirty(tc);
            }
        }

        EditorUtility.SetDirty(root);
        AssetDatabase.SaveAssets();
        Debug.Log("FixOrderUI: done on " + root.name + ".");
    }

    private static void CenterRect(RectTransform rt, Vector2 size)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        rt.localPosition = new Vector3(0f, rt.localPosition.y, 0f); // kill stray Z (your 20.29)
        rt.localRotation = Quaternion.identity;
    }

    private static Transform FindChild(Transform parent, string n)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == n) return t;
        return null;
    }
}