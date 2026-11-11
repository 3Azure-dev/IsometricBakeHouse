using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CreatureAnimationBuilder
{
    const string Folder = "Assets/Art/Animations";

    class Style
    {
        public string name;
        // Idle
        public float idleBob = 0.1f, idleTime = 1f, idleTilt = 0f, idleBreath = 0f;
        // Run
        public float runSquash = 1.2f, runTime = 0.4f, runTilt = 0f, runHop = 0f;
        // Stunned
        public float stunShake = 0.05f, stunTilt = 0f, stunTime = 0.3f;
        public Color stunColor = Color.yellow;
        // Caught
        public float caughtPop = 1.3f, caughtSpin = 0f, caughtTime = 0.4f;
        // Special (custom behaviour)
        public float specialX = 1.2f, specialY = 0.8f, specialTilt = 0f, specialTime = 0.5f;
    }

    // Flour is the base. The other 8 each have their own look. Change numbers here and rerun.
    static readonly Style Flour = new Style { name = "Flour", specialTilt = 10f };

    static readonly Style[] Others =
    {
        new Style { name="Yeast", idleBreath=0.08f, idleBob=0.04f, idleTime=1.4f,
                    runHop=0.18f, runSquash=1.1f, runTime=0.35f,
                    stunTilt=15f, caughtSpin=360f, caughtPop=1.4f },
        new Style { name="Seeds", idleTilt=6f, idleBob=0.03f, idleTime=0.8f,
                    runTilt=-12f, runSquash=1.05f, runTime=0.25f, runHop=0.1f,
                    stunShake=0.08f, caughtSpin=720f, caughtPop=1.2f, caughtTime=0.3f },
        new Style { name="TomatoSauce", idleBreath=0.12f, idleBob=0.02f, idleTime=1.2f,
                    runSquash=1.35f, runTime=0.5f, runTilt=-8f,
                    stunShake=0.04f, stunTilt=8f, stunColor=new Color(1f,0.6f,0.2f),
                    caughtPop=1.6f, caughtTime=0.5f, specialX=1.6f, specialY=0.5f },
        new Style { name="Egg", idleTilt=5f, idleBob=0.05f, idleTime=1.1f,
                    runTilt=14f, runSquash=1.1f, runTime=0.4f, runHop=0.08f,
                    stunTilt=30f, stunShake=0.03f, stunTime=0.4f,
                    caughtPop=1.25f, caughtSpin=-180f },
        new Style { name="Cheese", idleBreath=0.05f, idleBob=0.06f, idleTime=1.6f,
                    runSquash=1.3f, runTime=0.55f, runTilt=6f,
                    stunShake=0.06f, stunColor=new Color(1f,0.8f,0f),
                    caughtPop=1.5f, caughtTime=0.5f, specialX=1.5f, specialY=1.3f },
        new Style { name="Dough", idleBreath=0.1f, idleBob=0.03f, idleTime=1.8f,
                    runSquash=1.25f, runTime=0.6f, runHop=0.05f,
                    stunShake=0.07f, stunTilt=6f, stunTime=0.35f,
                    caughtPop=1.1f, caughtTime=0.5f, specialX=1.5f, specialY=1.3f, specialTime=0.5f },
        new Style { name="Butter", idleBob=0.03f, idleTime=1.3f, idleTilt=2f,
                    runSquash=1.3f, runTime=0.45f, runTilt=-20f,
                    stunShake=0.05f, stunTilt=10f,
                    caughtPop=1.3f, caughtTime=0.4f, specialX=1.4f, specialY=0.7f, specialTilt=-20f },
        new Style { name="Sugar", idleBob=0.05f, idleTime=0.6f, idleTilt=4f,
                    runSquash=1.1f, runTime=0.2f, runHop=0.12f, runTilt=-10f,
                    stunShake=0.1f, stunTime=0.25f, stunColor=Color.white,
                    caughtPop=1.5f, caughtSpin=540f, caughtTime=0.3f,
                    specialX=1.6f, specialY=0.7f, specialTilt=-10f, specialTime=0.3f },
    };

    // ---------- Menu 1: AC_Flour + Flour clips ----------
    [MenuItem("Tools/Creatures/1 - Build AC_Flour")]
    public static void BuildFlour()
    {
        var idle = MakeIdle(Flour);
        var run = MakeRun(Flour);
        var stun = MakeStunned(Flour);
        var caught = MakeCaught(Flour);
        var special = MakeSpecial(Flour);

        string path = $"{Folder}/AC_Flour.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path)
                   ?? AnimatorController.CreateAnimatorControllerAtPath(path);

        // clear old content but keep the asset (so prefab links survive)
        var sm = ctrl.layers[0].stateMachine;
        foreach (var s in sm.states) sm.RemoveState(s.state);
        foreach (var t in sm.anyStateTransitions) sm.RemoveAnyStateTransition(t);
        for (int i = ctrl.parameters.Length - 1; i >= 0; i--) ctrl.RemoveParameter(i);

        ctrl.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Stunned", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Caught", AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Special", AnimatorControllerParameterType.Trigger);

        var sIdle = sm.AddState("Idle");
        var sRun = sm.AddState("Run");
        var sStun = sm.AddState("Stunned");
        var sCaught = sm.AddState("Caught");
        var sSpecial = sm.AddState("Special");
        sIdle.motion = idle; sRun.motion = run; sStun.motion = stun;
        sCaught.motion = caught; sSpecial.motion = special;
        sm.defaultState = sIdle;

        Cond(sIdle.AddTransition(sRun), AnimatorConditionMode.If, "IsRunning");
        Cond(sRun.AddTransition(sIdle), AnimatorConditionMode.IfNot, "IsRunning");
        Cond(sStun.AddTransition(sIdle), AnimatorConditionMode.IfNot, "Stunned");

        var toStun = sm.AddAnyStateTransition(sStun);
        Cond(toStun, AnimatorConditionMode.If, "Stunned");
        toStun.canTransitionToSelf = false;

        var toCaught = sm.AddAnyStateTransition(sCaught);
        Cond(toCaught, AnimatorConditionMode.If, "Caught");
        toCaught.canTransitionToSelf = false;

        var toSpecial = sm.AddAnyStateTransition(sSpecial);
        Cond(toSpecial, AnimatorConditionMode.If, "Special");
        toSpecial.canTransitionToSelf = false;

        // Special plays once, then returns to Idle
        var back = sSpecial.AddTransition(sIdle);
        back.hasExitTime = true; back.exitTime = 1f; back.duration = 0f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        Debug.Log("AC_Flour built.");
    }

    // ---------- Menu 2: one override controller per creature ----------
    [MenuItem("Tools/Creatures/2 - Build Override Controllers")]
    public static void BuildOverrides()
    {
        var baseCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{Folder}/AC_Flour.controller");
        if (baseCtrl == null) { Debug.LogError("Build AC_Flour first."); return; }

        foreach (var st in Others)
        {
            var map = new Dictionary<string, AnimationClip>
            {
                { "Flour_Idle",    MakeIdle(st) },
                { "Flour_Run",     MakeRun(st) },
                { "Flour_Stunned", MakeStunned(st) },
                { "Flour_Caught",  MakeCaught(st) },
                { "Flour_Special", MakeSpecial(st) },
            };

            string path = $"{Folder}/AOC_{st.name}.overrideController";
            var aoc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
            if (aoc == null)
            {
                aoc = new AnimatorOverrideController(baseCtrl);
                AssetDatabase.CreateAsset(aoc, path);
            }
            aoc.runtimeAnimatorController = baseCtrl;

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

    // ---------- Clips (Position / Scale / Rotation / Color only, never Sprite) ----------
    static AnimationClip MakeIdle(Style s)
    {
        var c = NewClip(s.name, "Idle", true);
        float L = s.idleTime;
        Set(c, typeof(Transform), "localPosition.y", Curve((0, 0), (L * .5f, s.idleBob), (L, 0)));
        if (s.idleTilt != 0)
            Set(c, typeof(Transform), "localEulerAnglesRaw.z",
                Curve((0, 0), (L * .25f, s.idleTilt), (L * .5f, 0), (L * .75f, -s.idleTilt), (L, 0)));
        if (s.idleBreath != 0)
        {
            var k = Curve((0, 1), (L * .5f, 1 + s.idleBreath), (L, 1));
            Set(c, typeof(Transform), "localScale.x", k);
            Set(c, typeof(Transform), "localScale.y", k);
        }
        return Save(c, s.name, "Idle");
    }

    static AnimationClip MakeRun(Style s)
    {
        var c = NewClip(s.name, "Run", true);
        float L = s.runTime, sq = s.runSquash, stretch = 2f - sq;
        Set(c, typeof(Transform), "localScale.x", Curve((0, 1), (L * .33f, sq), (L * .66f, stretch), (L, 1)));
        Set(c, typeof(Transform), "localScale.y", Curve((0, 1), (L * .33f, stretch), (L * .66f, sq), (L, 1)));
        if (s.runTilt != 0)
            Set(c, typeof(Transform), "localEulerAnglesRaw.z", Curve((0, s.runTilt), (L, s.runTilt)));
        if (s.runHop > 0)
            Set(c, typeof(Transform), "localPosition.y", Curve((0, 0), (L * .5f, s.runHop), (L, 0)));
        return Save(c, s.name, "Run");
    }

    static AnimationClip MakeStunned(Style s)
    {
        var c = NewClip(s.name, "Stunned", true);
        float L = s.stunTime, a = s.stunShake;
        Set(c, typeof(Transform), "localPosition.x",
            Curve((0, 0), (L * .25f, a), (L * .5f, -a), (L * .75f, a), (L, 0)));
        if (s.stunTilt != 0)
            Set(c, typeof(Transform), "localEulerAnglesRaw.z",
                Curve((0, 0), (L * .25f, s.stunTilt), (L * .5f, -s.stunTilt), (L * .75f, s.stunTilt), (L, 0)));
        Color f = s.stunColor;
        Set(c, typeof(SpriteRenderer), "m_Color.r", Curve((0, 1), (L * .25f, f.r), (L * .75f, f.r), (L, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.g", Curve((0, 1), (L * .25f, f.g), (L * .75f, f.g), (L, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.b", Curve((0, 1), (L * .25f, f.b), (L * .75f, f.b), (L, 1)));
        Set(c, typeof(SpriteRenderer), "m_Color.a", Curve((0, 1), (L, 1)));
        return Save(c, s.name, "Stunned");
    }

    static AnimationClip MakeCaught(Style s)
    {
        var c = NewClip(s.name, "Caught", false);
        float L = s.caughtTime;
        var k = Curve((0, 1), (L * .3f, s.caughtPop), (L, 0));
        Set(c, typeof(Transform), "localScale.x", k);
        Set(c, typeof(Transform), "localScale.y", k);
        if (s.caughtSpin != 0)
            Set(c, typeof(Transform), "localEulerAnglesRaw.z", Curve((0, 0), (L, s.caughtSpin)));
        return Save(c, s.name, "Caught");
    }

    static AnimationClip MakeSpecial(Style s)
    {
        var c = NewClip(s.name, "Special", false);
        float L = s.specialTime;
        Set(c, typeof(Transform), "localScale.x", Curve((0, 1), (L * .4f, s.specialX), (L, 1)));
        Set(c, typeof(Transform), "localScale.y", Curve((0, 1), (L * .4f, s.specialY), (L, 1)));
        if (s.specialTilt != 0)
            Set(c, typeof(Transform), "localEulerAnglesRaw.z", Curve((0, 0), (L * .4f, s.specialTilt), (L, 0)));
        return Save(c, s.name, "Special");
    }

    // ---------- helpers ----------
    static AnimationClip NewClip(string creature, string state, bool loop)
    {
        var c = new AnimationClip { name = $"{creature}_{state}", frameRate = 60 };
        var set = AnimationUtility.GetAnimationClipSettings(c);
        set.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(c, set);
        return c;
    }

    // Keeps the same asset (and GUID) if it already exists
    static AnimationClip Save(AnimationClip c, string creature, string state)
    {
        string path = $"{Folder}/{creature}_{state}.anim";
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(c, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }
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