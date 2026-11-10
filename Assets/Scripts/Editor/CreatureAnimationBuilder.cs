using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CreatureAnimationBuilder
{
    const string Folder = "Assets/Art/Animations";

    
    // The other 8 creatures (Flour is built separately)
    static readonly string[] OtherCreatures =
        { "Yeast", "Seed", "TomatoSauce", "Egg",
      "Cheese", "Dough", "Butter", "Sugar" };

    // ---------- Step 1: AC_Flour + 4 Flour clips ----------
    [MenuItem("Tools/Creatures/1 - Build AC_Flour")]
    public static void BuildFlour()
    {
        var idle = MakeIdle("Flour", 0.1f, 1f);
        var run = MakeRun("Flour", 1.2f, 0.4f);
        var stunned = MakeStunned("Flour", 0.05f, 0.3f);
        var caught = MakeCaught("Flour", 1.3f, 0.4f);

        string path = $"{Folder}/AC_Flour.controller";
        AssetDatabase.DeleteAsset(path); // rebuild cleanly
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        ctrl.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Stunned", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Caught", AnimatorControllerParameterType.Trigger);

        var sm = ctrl.layers[0].stateMachine;
        var sIdle = sm.AddState("Idle");
        var sRun = sm.AddState("Run");
        var sStunned = sm.AddState("Stunned");
        var sCaught = sm.AddState("Caught");
        sIdle.motion = idle; sRun.motion = run;
        sStunned.motion = stunned; sCaught.motion = caught;
        sm.defaultState = sIdle;

        Cond(sIdle.AddTransition(sRun), AnimatorConditionMode.If, "IsRunning");
        Cond(sRun.AddTransition(sIdle), AnimatorConditionMode.IfNot, "IsRunning");
        Cond(sStunned.AddTransition(sIdle), AnimatorConditionMode.IfNot, "Stunned");

        var toStun = sm.AddAnyStateTransition(sStunned);
        Cond(toStun, AnimatorConditionMode.If, "Stunned");
        toStun.canTransitionToSelf = false;

        var toCaught = sm.AddAnyStateTransition(sCaught);
        Cond(toCaught, AnimatorConditionMode.If, "Caught");
        toCaught.canTransitionToSelf = false;

        AssetDatabase.SaveAssets();
        Debug.Log("AC_Flour built.");
    }

    // ---------- Step 2: Override controllers for the other 8 ----------
    [MenuItem("Tools/Creatures/2 - Build Override Controllers")]
    public static void BuildOverrides()
    {
        var baseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{Folder}/AC_Flour.controller");
        if (baseCtrl == null) { Debug.LogError("Build AC_Flour first."); return; }

        for (int i = 0; i < OtherCreatures.Length; i++)
        {
            string n = OtherCreatures[i];
            float bob = 0.08f + 0.02f * (i % 4);   // different bob height
            float bobT = 0.8f + 0.1f * (i % 3);   // different bob speed
            float squash = 1.15f + 0.05f * (i % 3);   // different squash
            float runT = 0.3f + 0.05f * (i % 3);
            float shake = 0.04f + 0.01f * (i % 3);
            float pop = 1.2f + 0.1f * (i % 3);

            var map = new Dictionary<string, AnimationClip>
            {
                { "Flour_Idle",    MakeIdle(n, bob, bobT) },
                { "Flour_Run",     MakeRun(n, squash, runT) },
                { "Flour_Stunned", MakeStunned(n, shake, 0.3f) },
                { "Flour_Caught",  MakeCaught(n, pop, 0.4f) },
            };

            string path = $"{Folder}/AOC_{n}.overrideController";
            AssetDatabase.DeleteAsset(path);
            var aoc = new AnimatorOverrideController(baseCtrl);
            AssetDatabase.CreateAsset(aoc, path);

            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            aoc.GetOverrides(pairs);
            for (int k = 0; k < pairs.Count; k++)
                pairs[k] = new KeyValuePair<AnimationClip, AnimationClip>(
                    pairs[k].Key, map[pairs[k].Key.name]);
            aoc.ApplyOverrides(pairs);
            EditorUtility.SetDirty(aoc);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Override controllers built.");
    }

    // ---------- Clip makers (Position / Scale / Color only, never Sprite) ----------
    static AnimationClip MakeIdle(string n, float height, float len)
    {
        var c = NewClip(n, "Idle", true);
        Set(c, typeof(Transform), "localPosition.y",
            Curve((0, 0), (len * 0.5f, height), (len, 0)));
        return Save(c, n, "Idle");
    }

    static AnimationClip MakeRun(string n, float squash, float len)
    {
        var c = NewClip(n, "Run", true);
        float stretch = 2f - squash; // squash wide+short, stretch narrow+tall
        Set(c, typeof(Transform), "localScale.x",
            Curve((0, 1), (len * 0.33f, squash), (len * 0.66f, stretch), (len, 1)));
        Set(c, typeof(Transform), "localScale.y",
            Curve((0, 1), (len * 0.33f, stretch), (len * 0.66f, squash), (len, 1)));
        return Save(c, n, "Run");
    }

    static AnimationClip MakeStunned(string n, float shake, float len)
    {
        var c = NewClip(n, "Stunned", true);
        Set(c, typeof(Transform), "localPosition.x",
            Curve((0, 0), (len * 0.25f, shake), (len * 0.5f, -shake),
                  (len * 0.75f, shake), (len, 0)));
        // flash yellow: white -> yellow -> white
        Set(c, typeof(SpriteRenderer), "m_Color.r", Curve((0, 1), (len, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.g", Curve((0, 1), (len, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.b",
            Curve((0, 1), (len * 0.25f, 0), (len * 0.75f, 0), (len, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.a", Curve((0, 1), (len, 1)));
        return Save(c, n, "Stunned");
    }

    static AnimationClip MakeCaught(string n, float pop, float len)
    {
        var c = NewClip(n, "Caught", false);
        var k = Curve((0, 1), (len * 0.3f, pop), (len, 0));
        Set(c, typeof(Transform), "localScale.x", k);
        Set(c, typeof(Transform), "localScale.y", k);
        return Save(c, n, "Caught");
    }

    // ---------- helpers ----------
    static AnimationClip NewClip(string creature, string state, bool loop)
    {
        var c = new AnimationClip { name = $"{creature}_{state}", frameRate = 60 };
        var s = AnimationUtility.GetAnimationClipSettings(c);
        s.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(c, s);
        return c;
    }

    // Name must be "Flour_Idle" etc. for the override mapping, so we map by state name
    static AnimationClip Save(AnimationClip c, string creature, string state)
    {
        string path = $"{Folder}/{creature}_{state}.anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(c, path);
        return c;
    }

    static void Set(AnimationClip c, System.Type t, string prop, AnimationCurve curve)
        => AnimationUtility.SetEditorCurve(c, EditorCurveBinding.FloatCurve("", t, prop), curve);

    static AnimationCurve Curve(params (float t, float v)[] keys)
    {
        var kf = new Keyframe[keys.Length];
        for (int i = 0; i < keys.Length; i++) kf[i] = new Keyframe(keys[i].t, keys[i].v);
        var curve = new AnimationCurve(kf);
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    static void Cond(AnimatorStateTransition t, AnimatorConditionMode mode, string param)
    {
        t.hasExitTime = false;
        t.duration = 0f;
        t.AddCondition(mode, 0, param);
    }
}