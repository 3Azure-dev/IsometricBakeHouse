#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Builds the SCRUM-518 "Dirty Plate Pin" under the selected World Space canvas.
//
// Menu:  Tools > Bakery > Build Dirty Plate Pin
//
// What it does:
//   - Creates ONE new child called "DirtyPlatePin" with the parts from the spec
//     (tail, body, plate, smear, crumbs, key cap + "E"), using only Unity's built-in sprites.
//   - Points PromptPopper.bubble at the new pin (if the canvas has a PromptPopper).
// What it does NOT do:
//   - It deletes nothing and changes nothing else. The old SquareBubble stays, so you can
//     switch it off and back on. Everything is one Undo step (Ctrl+Z).
public static class BuildDirtyPlatePin
{
    private const string PinName = "DirtyPlatePin";
    private const string Amber = "#F5B83D";
    private const string Outline = "#6B4210";

    [MenuItem("Tools/Bakery/Build Dirty Plate Pin")]
    private static void Build()
    {
        GameObject canvasGO = Selection.activeGameObject;

        if (canvasGO == null || canvasGO.GetComponent<Canvas>() == null)
        {
            EditorUtility.DisplayDialog("Dirty Plate Pin",
                "Select the TablePromptCanvas object first (the one that has the Canvas component).", "OK");
            return;
        }

        if (canvasGO.transform.Find(PinName) != null)
        {
            EditorUtility.DisplayDialog("Dirty Plate Pin",
                "This canvas already has a '" + PinName + "'. Delete or rename it first.", "OK");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Dirty Plate Pin");

        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        RectTransform pin = NewRect(PinName, canvasGO.transform, new Vector2(140, 160), Vector2.zero);

        // Order matters: Unity draws top to bottom, so the tail is behind the body and the key is on top.
        Image tail = NewImage("PinTail", pin, null, new Vector2(30, 30), new Vector2(0, -55), Amber, false);
        tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        AddOutline(tail.gameObject);

        Image body = NewImage("PinBody", pin, rounded, new Vector2(110, 110), Vector2.zero, Amber, true);
        AddOutline(body.gameObject);

        NewImage("PlateRim", pin, knob, new Vector2(78, 78), Vector2.zero, "#FFFFFF", false);
        NewImage("PlateCentre", pin, knob, new Vector2(54, 54), Vector2.zero, "#EEEBE4", false);

        Image smear = NewImage("Smear", pin, knob, new Vector2(18, 10), new Vector2(8, 10), "#D9783A", false);
        Color smearColor = smear.color;
        smearColor.a = 190f / 255f;
        smear.color = smearColor;

        NewImage("Crumb1", pin, knob, new Vector2(10, 10), new Vector2(-10, 4), "#9A5B2A", false);
        NewImage("Crumb2", pin, knob, new Vector2(8, 8), new Vector2(12, -6), "#9A5B2A", false);
        NewImage("Crumb3", pin, knob, new Vector2(12, 12), new Vector2(-1, -13), "#9A5B2A", false);

        Image key = NewImage("KeyCap", pin, rounded, new Vector2(38, 38), new Vector2(48, -48), "#2B2B2B", true);
        Shadow shadow = key.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(0f, -4f);

        RectTransform labelRect = NewRect("KeyLabel", key.transform, new Vector2(38, 38), Vector2.zero);
        TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = "E";
        label.fontSize = 24;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;

        // Make the popper animate the new pin instead of the old bubble.
        PromptPopper popper = canvasGO.GetComponent<PromptPopper>();
        if (popper != null)
        {
            SerializedObject so = new SerializedObject(popper);
            SerializedProperty bubbleProp = so.FindProperty("bubble");
            if (bubbleProp != null)
            {
                bubbleProp.objectReferenceValue = pin;
                so.ApplyModifiedProperties();
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = pin.gameObject;

        Debug.Log("Dirty Plate Pin built under '" + canvasGO.name + "'. Canvas scale is " +
                  canvasGO.transform.localScale.x + " (the spec uses 0.01). Now switch off the old SquareBubble.", canvasGO);
    }

    // ---------------------------------------------------------------- helpers

    private static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Build Dirty Plate Pin");
        go.layer = parent.gameObject.layer;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition3D = new Vector3(pos.x, pos.y, 0f);
        rt.localScale = Vector3.one;
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Vector2 size, Vector2 pos, string hex, bool sliced)
    {
        RectTransform rt = NewRect(name, parent, size, pos);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Hex(hex);
        img.raycastTarget = false;

        if (sliced)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.3f;   // lower = rounder corners. Tune by eye.
        }

        return img;
    }

    private static void AddOutline(GameObject go)
    {
        UnityEngine.UI.Outline o = go.AddComponent<UnityEngine.UI.Outline>();
        o.effectColor = Hex(Outline);
        o.effectDistance = new Vector2(4f, -4f);
    }

    private static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
#endif