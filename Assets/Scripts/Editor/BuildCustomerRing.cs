#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Builds the SCRUM-529 "Customer Ring" visuals under the selected World Space canvas
// (the customer's TimerCanvas, inside the NormalCostomer / VIPCustomer prefab).
//
// Menu:  Tools > Bakery > Build Customer Ring
//
// Creates ONE new child "RingRoot" (100x100) with, in this order (top to bottom):
//   RingBack    Knob 100x100  #2B2B2B, 75% alpha
//   TimerRing   Knob  92x92   Filled, Radial 360, origin Top, cream   (colour set by script later)
//   CentreDisc  Knob  72x72   #2B2B2B  (turns the pie into a ring)
//   CenterIcon  Knob  58x58   Preserve Aspect                          (colour set by script later)
//   IconLetter  TMP "B", size 26, Bold, white, centred                 (placeholder, hidden when a sprite is used)
//
// It deletes nothing and does not touch any existing object or script. One Undo step (Ctrl+Z).
public static class BuildCustomerRing
{
    private const string RootName = "RingRoot";

    [MenuItem("Tools/Bakery/Build Customer Ring")]
    private static void Build()
    {
        GameObject canvasGO = Selection.activeGameObject;

        if (canvasGO == null || canvasGO.GetComponent<Canvas>() == null)
        {
            EditorUtility.DisplayDialog("Customer Ring",
                "Select the customer's timer canvas first (the object that has the Canvas component).", "OK");
            return;
        }

        if (canvasGO.transform.Find(RootName) != null)
        {
            EditorUtility.DisplayDialog("Customer Ring",
                "This canvas already has a '" + RootName + "'. Delete or rename it first.", "OK");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Customer Ring");

        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        RectTransform root = NewRect(RootName, canvasGO.transform, new Vector2(100, 100));

        Image back = NewImage("RingBack", root, knob, new Vector2(100, 100), "#2B2B2B");
        Color backColor = back.color;
        backColor.a = 0.75f;
        back.color = backColor;

        Image ring = NewImage("TimerRing", root, knob, new Vector2(92, 92), "#F5E6C4");
        ring.type = Image.Type.Filled;
        ring.fillMethod = Image.FillMethod.Radial360;
        ring.fillOrigin = (int)Image.Origin360.Top;
        ring.fillClockwise = true;
        ring.fillAmount = 1f;

        NewImage("CentreDisc", root, knob, new Vector2(72, 72), "#2B2B2B");

        Image icon = NewImage("CenterIcon", root, knob, new Vector2(58, 58), "#D9A04E");
        icon.preserveAspect = true;

        RectTransform letterRect = NewRect("IconLetter", root, new Vector2(58, 58));
        TextMeshProUGUI letter = letterRect.gameObject.AddComponent<TextMeshProUGUI>();
        letter.text = "B";
        letter.fontSize = 26;
        letter.fontStyle = FontStyles.Bold;
        letter.alignment = TextAlignmentOptions.Center;
        letter.color = Color.white;
        letter.raycastTarget = false;

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = root.gameObject;

        Debug.Log("Customer Ring built under '" + canvasGO.name + "'. Nothing was deleted or wired yet. " +
                  "Canvas scale is " + canvasGO.transform.localScale.x + " (the spec uses 0.01).", canvasGO);
    }

    // ---------------------------------------------------------------- helpers

    private static RectTransform NewRect(string name, Transform parent, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Build Customer Ring");
        go.layer = parent.gameObject.layer;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition3D = Vector3.zero;
        rt.localScale = Vector3.one;
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Vector2 size, string hex)
    {
        RectTransform rt = NewRect(name, parent, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Hex(hex);
        img.raycastTarget = false;
        return img;
    }

    private static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
#endif