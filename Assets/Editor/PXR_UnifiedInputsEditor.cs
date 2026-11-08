#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(PXR_UnifiedInputs))]
public class PXR_UnifiedInputsEditor : Editor
{
    // All foldouts default to closed.
    private bool showPrimary = false;
    private bool showDrop = false;

    private SerializedProperty Find(string name)
    {
        return serializedObject.FindProperty(name);
    }

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        serializedObject.Update();

        SerializedProperty inputType = Find("interactionType");
        SerializedProperty dropBehavior = Find("dropBehavior");

        PXR_InspectorGuidance.PropertyField(
            inputType,
            new GUIContent("Interaction Type")
        );

        PXR_InputType type =
            (PXR_InputType)inputType.intValue;

        PXR_InspectorGuidance.InputRequirements(type, ((PXR_UnifiedInputs)target).gameObject);

        if (type == PXR_InputType.Pickup)
        {
            PXR_InspectorGuidance.PropertyField(
                dropBehavior,
                new GUIContent("On Drop Behavior")
            );
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Networking",
            EditorStyles.boldLabel
        );

        PXR_InspectorGuidance.PropertyField(
            Find("synced"),
            new GUIContent("Synced")
        );

        EditorGUILayout.HelpBox(
            "Ordinary events use the selected Local/All " +
            "Players scope. SpawnObject uses the input's " +
            "Synced setting to choose local instantiation " +
            "or pooled spawning. Teleport and synchronized " +
            "AVPro use their existing special routes.",
            MessageType.Info
        );

        PXR_InspectorGuidance.PropertyField(
            Find("useDelay"),
            new GUIContent("Use Delay")
        );

        if (Find("useDelay").boolValue)
        {
            SerializedProperty delay = Find("delaySeconds");

            PXR_InspectorGuidance.PropertyField(
                delay,
                new GUIContent("Delay (seconds)")
            );

            delay.floatValue = Mathf.Max(
                0f,
                delay.floatValue
            );
        }

        if (type == PXR_InputType.Key)
        {
            DrawKeySettings();
        }

        EditorGUILayout.Space();

        DrawList(
            "Primary Events",
            ref showPrimary,
            "primaryEventTargets",
            "primaryEventNames",
            "primaryEventScopes",
            "primaryTeleportTargets",
            "primaryVideoTargets",
            "eventTarget",
            "eventName"
        );

        if (type == PXR_InputType.Pickup &&
            (PXR_DropBehavior)dropBehavior.intValue ==
                PXR_DropBehavior.SeparateFunction)
        {
            DrawList(
                "On Drop Events",
                ref showDrop,
                "dropEventTargets",
                "dropEventNames",
                "dropEventScopes",
                "dropTeleportTargets",
                "dropVideoTargets",
                "secondEventTarget",
                "secondEventName"
            );
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawKeySettings()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(
            "Key Lock", EditorStyles.boldLabel);

        PXR_InspectorGuidance.PropertyField(
            Find("destroyKeyAfterUse"));

        SerializedProperty required = Find("requiredKeys");
        PXR_InspectorGuidance.PropertyField(required);
        required.intValue = Mathf.Max(1, required.intValue);

        PXR_InspectorGuidance.PropertyField(Find("checkKeyTag"));
        if (Find("checkKeyTag").boolValue)
            PXR_InspectorGuidance.PropertyField(Find("requiredKeyTag"));

        PXR_InspectorGuidance.PropertyField(
            Find("pooledKeyPool"),
            new GUIContent("Pooled Key Manager"));
        PXR_InspectorGuidance.PropertyField(
            Find("keyLockState"),
            new GUIContent("Key Lock State"));

        PXR_KeyLockState state =
            Find("keyLockState").objectReferenceValue
                as PXR_KeyLockState;
        PXR_UnifiedInputs input = (PXR_UnifiedInputs)target;

        if (state == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a PXR_KeyLockState (Manual) to this " +
                "lock. Ordinary pickups do not need one.",
                MessageType.Error);
        } else
        {
            if (state.input != input)
            {
                EditorGUILayout.HelpBox(
                    "Set Key Lock State > Input to this " +
                    "PXR_UnifiedInputs component.",
                    MessageType.Warning);
            }
            if (state.GetComponent<VRC.SDK3.Components.VRCObjectSync>() != null)
            {
                EditorGUILayout.HelpBox(
                    "Move the Manual lock state onto a " +
                    "GameObject without VRCObjectSync.",
                    MessageType.Error);
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField(new GUIContent("Key Count", PXR_InspectorGuidance.Tooltip("PXR_KeyLockState", "keyCount")), state.keyCount);
            EditorGUILayout.Toggle(new GUIContent("Keys Complete", PXR_InspectorGuidance.Tooltip("PXR_KeyLockState", "keysComplete")), state.keysComplete);
            EditorGUI.EndDisabledGroup();
        }

        PXR_PoolManager manager =
            Find("pooledKeyPool").objectReferenceValue
                as PXR_PoolManager;

        if (manager != null && state != null &&
            manager.FindLockIndex(state) < 0)
        {
            EditorGUILayout.HelpBox(
                "Register the PXR_KeyLockState in " +
                "Pool Manager > Key Locks.",
                MessageType.Warning);
        }
    }

    private void DrawList(
        string title,
        ref bool expanded,
        string targetPath,
        string namePath,
        string scopePath,
        string teleportPath,
        string videoPath,
        string legacyTargetPath,
        string legacyNamePath)
    {
        SerializedProperty targets = Find(targetPath);
        SerializedProperty names = Find(namePath);
        SerializedProperty scopes = Find(scopePath);
        SerializedProperty teleports = Find(teleportPath);
        SerializedProperty videos = Find(videoPath);

        int count = targets.arraySize;

        names.arraySize = count;
        scopes.arraySize = count;
        teleports.arraySize = count;
        videos.arraySize = count;

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        expanded = EditorGUILayout.Foldout(
            expanded,
            title + " (" + count + ")",
            true
        );

        if (expanded)
        {
            EditorGUI.indentLevel++;

            SerializedProperty legacyTarget =
                Find(legacyTargetPath);

            SerializedProperty legacyName =
                Find(legacyNamePath);

            if (count == 0 &&
                legacyTarget.objectReferenceValue != null)
            {
                if (GUILayout.Button("Import Legacy Event"))
                {
                    targets.arraySize = 1;
                    names.arraySize = 1;
                    scopes.arraySize = 1;
                    teleports.arraySize = 1;
                    videos.arraySize = 1;

                    targets.GetArrayElementAtIndex(0)
                        .objectReferenceValue =
                        legacyTarget.objectReferenceValue;

                    names.GetArrayElementAtIndex(0)
                        .stringValue =
                        legacyName.stringValue;

                    scopes.GetArrayElementAtIndex(0)
                        .intValue = 0;

                    legacyTarget.objectReferenceValue = null;
                    legacyName.stringValue = "";

                    count = 1;
                }
            }

            for (int i = 0; i < count; i++)
            {
                EditorGUILayout.BeginVertical(
                    EditorStyles.helpBox
                );

                EditorGUILayout.LabelField(
                    "Event " + (i + 1),
                    EditorStyles.boldLabel
                );

                SerializedProperty targetProp =
                    targets.GetArrayElementAtIndex(i);

                SerializedProperty nameProp =
                    names.GetArrayElementAtIndex(i);

                DrawMethodSelector(targetProp, nameProp);

                SerializedProperty teleportProp =
                    teleports.GetArrayElementAtIndex(i);

                SerializedProperty videoProp =
                    videos.GetArrayElementAtIndex(i);

                SetSpecialReferences(
                    targetProp,
                    nameProp,
                    teleportProp,
                    videoProp
                );

                PXR_UnifiedOutputs output =
                    targetProp.objectReferenceValue
                    as PXR_UnifiedOutputs;

                PXR_UnifiedPhysics physics =
                    targetProp.objectReferenceValue
                    as PXR_UnifiedPhysics;

                bool spawn =
                    nameProp.stringValue == "SpawnObject" &&
                    (output != null || physics != null);

                bool teleport =
                    output != null &&
                    nameProp.stringValue == "TeleportPlayer";

                bool syncedVideo =
                    output != null &&
                    nameProp.stringValue == "PlayAVProVideo" &&
                    output.avProUseSyncedPrefab;

                SerializedProperty scope =
                    scopes.GetArrayElementAtIndex(i);

                EditorGUI.BeginDisabledGroup(
                    spawn || teleport || syncedVideo
                );

                scope.intValue = EditorGUILayout.Popup(
                    new GUIContent("Execution Scope", "Choose whether this output runs locally, for all players, or uses the input's Synced setting."),
                    Mathf.Clamp(scope.intValue, 0, 2),
                    new string[]
                    {
                        "Input Default",
                        "Local Only",
                        "All Players"
                    }
                );

                EditorGUI.EndDisabledGroup();

                if (spawn)
                {
                    EditorGUILayout.HelpBox(
                        "SpawnObject uses the input's " +
                        "Synced checkbox, not the per-event scope.",
                        MessageType.Info
                    );

                    PXR_PoolManager pool =
                        output != null
                            ? output.spawnPoolManager
                            : physics.spawnPoolManager;

                    if (Find("synced").boolValue &&
                        pool == null)
                    {
                        EditorGUILayout.HelpBox(
                            "Synced spawning needs a Pool Manager " +
                            "on the target output behavior.",
                            MessageType.Warning
                        );
                    }
                }

                if (teleport)
                {
                    EditorGUILayout.HelpBox(
                        "TeleportPlayer uses its own " +
                        "targeting and network routing.",
                        MessageType.Info
                    );
                }

                if (syncedVideo)
                {
                    EditorGUILayout.HelpBox(
                        "Synchronized AVPro uses the " +
                        "video owner's network route.",
                        MessageType.Info
                    );
                }

                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginDisabledGroup(i == 0);
                bool up = GUILayout.Button("Up");
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(i >= count - 1);
                bool down = GUILayout.Button("Down");
                EditorGUI.EndDisabledGroup();

                bool remove = GUILayout.Button("Remove");

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                if (up || down)
                {
                    int destination = up ? i - 1 : i + 1;

                    MoveEntry(
                        targets,
                        names,
                        scopes,
                        teleports,
                        videos,
                        i,
                        destination
                    );

                    break;
                }

                if (remove)
                {
                    RemoveEntry(
                        targets,
                        names,
                        scopes,
                        teleports,
                        videos,
                        i
                    );

                    break;
                }
            }

            if (GUILayout.Button("+ Add Event"))
            {
                int index = targets.arraySize;

                targets.arraySize = index + 1;
                names.arraySize = index + 1;
                scopes.arraySize = index + 1;
                teleports.arraySize = index + 1;
                videos.arraySize = index + 1;

                targets.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;

                names.GetArrayElementAtIndex(index)
                    .stringValue = "";

                scopes.GetArrayElementAtIndex(index)
                    .intValue = 0;

                teleports.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;

                videos.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;
            }

            EditorGUI.indentLevel--;
        }

        // Keep special references consistent
        // even when the foldout is collapsed.

        for (int i = 0; i < targets.arraySize; i++)
        {
            SetSpecialReferences(
                targets.GetArrayElementAtIndex(i),
                names.GetArrayElementAtIndex(i),
                teleports.GetArrayElementAtIndex(i),
                videos.GetArrayElementAtIndex(i)
            );
        }

        EditorGUILayout.EndVertical();
    }

    // ========================================
    // TARGET-DECLARED WHITELIST DISCOVERY
    // ========================================

    private static void DrawMethodSelector(
        SerializedProperty targetProp,
        SerializedProperty nameProp)
    {
        EditorGUI.BeginChangeCheck();

        PXR_InspectorGuidance.PropertyField(
            targetProp,
            new GUIContent("Event Target")
        );

        if (EditorGUI.EndChangeCheck())
        {
            nameProp.stringValue = "";
        }

        UdonSharpBehaviour receiver =
            targetProp.objectReferenceValue
            as UdonSharpBehaviour;

        if (receiver == null)
        {
            return;
        }

        MethodInfo[] methods =
            receiver.GetType().GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );

        List<string> available =
            new List<string>();

        foreach (MethodInfo method in methods)
        {
            if (method.IsStatic ||
                method.IsSpecialName ||
                method.ReturnType != typeof(void) ||
                method.GetParameters().Length != 0 ||
                method.Name.StartsWith("_"))
            {
                continue;
            }

            // The target method must explicitly
            // declare its own PXR exposure.
            //
            // No target-type whitelist exists here.

            if (!method.IsDefined(
                typeof(PXR_InputCallableAttribute),
                false))
            {
                continue;
            }

            if (!available.Contains(method.Name))
            {
                available.Add(method.Name);
            }
        }

        available.Sort(StringComparer.Ordinal);

        string[] labels =
            new string[available.Count + 1];

        labels[0] = "<Select Function>";

        int selectedIndex = 0;

        for (int i = 0; i < available.Count; i++)
        {
            labels[i + 1] = available[i] + "()";

            if (nameProp.stringValue == available[i])
            {
                selectedIndex = i + 1;
            }
        }

        EditorGUI.BeginChangeCheck();

        int choice = EditorGUILayout.Popup(
            new GUIContent("Event Name", "Select a public, parameterless method explicitly exposed by the target using PXR_InputCallable."),
            selectedIndex,
            labels
        );

        if (EditorGUI.EndChangeCheck())
        {
            nameProp.stringValue =
                choice == 0
                    ? ""
                    : available[choice - 1];
        }

        if (available.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "This script does not declare any " +
                "[PXR_InputCallable] public, " +
                "parameterless void methods.",
                MessageType.Info
            );
        }

        // Preserve existing serialized event names.
        // Do not silently erase them if a method
        // is renamed or its exposure is removed.

        if (!string.IsNullOrEmpty(nameProp.stringValue) &&
            !available.Contains(nameProp.stringValue))
        {
            EditorGUILayout.HelpBox(
                "The saved function '" +
                nameProp.stringValue +
                "' is not currently whitelisted " +
                "on this target. The saved event " +
                "reference has been preserved.",
                MessageType.Warning
            );
        }
    }

    // ========================================
    // SPECIAL ROUTING REFERENCES
    // ========================================

    private static void SetSpecialReferences(
        SerializedProperty targetProp,
        SerializedProperty nameProp,
        SerializedProperty teleportProp,
        SerializedProperty videoProp)
    {
        PXR_UnifiedOutputs output =
            targetProp.objectReferenceValue
            as PXR_UnifiedOutputs;

        teleportProp.objectReferenceValue =
            output != null &&
            nameProp.stringValue == "TeleportPlayer"
                ? output
                : null;

        videoProp.objectReferenceValue =
            output != null &&
            nameProp.stringValue == "PlayAVProVideo"
                ? output
                : null;
    }

    // ========================================
    // ARRAY HELPERS
    // ========================================

    private static void MoveEntry(
        SerializedProperty targets,
        SerializedProperty names,
        SerializedProperty scopes,
        SerializedProperty teleports,
        SerializedProperty videos,
        int from,
        int to)
    {
        targets.MoveArrayElement(from, to);
        names.MoveArrayElement(from, to);
        scopes.MoveArrayElement(from, to);
        teleports.MoveArrayElement(from, to);
        videos.MoveArrayElement(from, to);
    }

    private static void RemoveEntry(
        SerializedProperty targets,
        SerializedProperty names,
        SerializedProperty scopes,
        SerializedProperty teleports,
        SerializedProperty videos,
        int index)
    {
        int count = targets.arraySize;

        for (int i = index; i < count - 1; i++)
        {
            MoveEntry(
                targets,
                names,
                scopes,
                teleports,
                videos,
                i + 1,
                i
            );
        }

        targets.arraySize = count - 1;
        names.arraySize = count - 1;
        scopes.arraySize = count - 1;
        teleports.arraySize = count - 1;
        videos.arraySize = count - 1;
    }

}

#endif
