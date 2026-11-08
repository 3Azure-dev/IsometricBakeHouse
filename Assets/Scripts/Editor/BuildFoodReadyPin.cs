#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Builds the SCRUM-519 "Food Ready Pin" (green pin, hot loaf on a plate, steam, E key cap)
// under the selected World Space canvas.
//
// Menu:  Tools > Bakery > Build Food Ready Pin
//
// What it does:
//   - Creates ONE new child called "FoodReadyPin" using only Unity's built-in sprites.
//   - Points PromptPopper.bubble at the new pin (if the canvas has a PromptPopper).
// What it does NOT do:
//   - It deletes nothing and changes nothing else. The old bubble stays, so you can switch it
//     off and back on. Everything is one Undo step (Ctrl+Z).
// Same sizes and key cap as the Dirty Plate Pin, so use the same canvas scale as that one.
public static class BuildFoodReadyPin
{
    private const string PinName = "FoodReadyPin";
    private const string Green = "#57B65F";
    private const string Outline = "#1F5B2B";

    [MenuItem("Tools/Bakery/Build Food Ready Pin")]
    private static void Build()
    {
        GameObject canvasGO = Selection.activeGameObject;

        if (canvasGO == null || canvasGO.GetComponent<Canvas>() == null)
        {
            EditorUtility.DisplayDialog("Food Ready Pin",
                "Select the prompt canvas first (the FoodReadyPrompt object that has the Canvas component).", "OK");
            return;
        }

        if (canvasGO.transform.Find(PinName) != null)
        {
            EditorUtility.DisplayDialog("Food Ready Pin",
                "This canvas already has a '" + PinName + "'. Delete or rename it first.", "OK");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Build Food Ready Pin");

        Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        RectTransform pin = NewRect(PinName, canvasGO.transform, new Vector2(140, 160), Vector2.zero);

        // Order matters: tail behind body, key cap on top.
        Image tail = NewImage("PinTail", pin, null, new Vector2(30, 30), new Vector2(0, -55), Green, false);
        tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        AddOutline(tail.gameObject);

        Image body = NewImage("PinBody", pin, rounded, new Vector2(110, 110), Vector2.zero, Green, true);
        AddOutline(body.gameObject);

        // Steam (UISprite, 6x18, white, alpha 220). Steam1 is from the spec; 2 and 3 are read from the picture.
        NewSteam("Steam1", pin, rounded, new Vector2(-10, 32));
        NewSteam("Steam2", pin, rounded, new Vector2(0, 35));
        NewSteam("Steam3", pin, rounded, new Vector2(10, 32));

        // Plate with the hot loaf
        NewImage("PlateRim", pin, knob, new Vector2(66, 66), new Vector2(0, -10), "#FFFFFF", false);
        NewImage("PlateCentre", pin, knob, new Vector2(46, 46), new Vector2(0, -10), "#EEEBE4", false);
        NewImage("Loaf", pin, knob, new Vector2(46, 28), new Vector2(0, -10), "#E0A33A", false);
        NewImage("LoafTop", pin, knob, new Vector2(32, 10), new Vector2(0, -5), "#F2C56B", false);

        // Key cap: same as the Dirty Plate Pin
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

        Debug.Log("Food Ready Pin built under '" + canvasGO.name + "'. Canvas scale is " +
                  canvasGO.transform.localScale.x + " (use the same scale as the Dirty Plate canvas). " +
                  "Now switch off the old bubble.", canvasGO);
    }

    // ---------------------------------------------------------------- helpers

    private static void NewSteam(string name, Transform parent, Sprite rounded, Vector2 pos)
    {
        Image steam = NewImage(name, parent, rounded, new Vector2(6, 18), pos, "#FFFFFF", true);
        Color c = steam.color;
        c.a = 220f / 255f;
        steam.color = c;
    }

    private static RectTransform NewRect(string name, Transform parent, Vector2 size, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Build Food Ready Pin");
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