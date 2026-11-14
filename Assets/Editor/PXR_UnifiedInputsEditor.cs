
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
        MigrateLegacyDelays();

        SerializedProperty inputType = Find("interactionType");
        SerializedProperty dropBehavior = Find("dropBehavior");

        PXR_InspectorGuidance.PropertyField(
            inputType,
            new GUIContent("Interaction Type")
        );

        PXR_InputType type =
            (PXR_InputType)inputType.intValue;

        PXR_InspectorGuidance.InputRequirements(
            type,
            ((PXR_UnifiedInputs)target).gameObject
        );

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
            "Ordinary events use their Local/All Players scope. " +
            "SpawnObject uses Synced to select local instantiation " +
            "or pooled spawning. Teleport and synchronized AVPro " +
            "retain their dedicated routing.",
            MessageType.Info
        );

        EditorGUILayout.HelpBox(
            "Timing is configured separately for each event. " +
            "Every delay begins when the input is triggered, " +
            "not when the preceding event finishes.",
            MessageType.Info
        );

        if (type == PXR_InputType.Key)
            DrawKeySettings();

        EditorGUILayout.Space();

        DrawList(
            "Primary Events",
            ref showPrimary,
            "primaryEventTargets",
            "primaryEventNames",
            "primaryEventScopes",
            "primaryEventDelays",
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
                "dropEventDelays",
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
            "Key Lock",
            EditorStyles.boldLabel
        );

        PXR_InspectorGuidance.PropertyField(
            Find("destroyKeyAfterUse")
        );

        SerializedProperty required = Find("requiredKeys");
        PXR_InspectorGuidance.PropertyField(required);
        required.intValue = Mathf.Max(1, required.intValue);

        PXR_InspectorGuidance.PropertyField(
            Find("repeatable"),
            new GUIContent(
                "Repeatable",
                "After the required number of accepted keys, " +
                "run the outputs and reset the count to zero. " +
                "Ordinary keys must leave and re-enter; each new " +
                "pooled projectile generation may count."
            )
        );

        if (Find("repeatable").boolValue)
        {
            EditorGUILayout.HelpBox(
                "The lock resets after completing a cycle. " +
                "New pooled spawn generations can count again. " +
                "An ordinary key must exit the lock and re-enter.",
                MessageType.Info
            );
        }

        PXR_InspectorGuidance.PropertyField(
            Find("checkKeyTag")
        );

        if (Find("checkKeyTag").boolValue)
        {
            PXR_InspectorGuidance.PropertyField(
                Find("requiredKeyTag")
            );
        }

        PXR_InspectorGuidance.PropertyField(
            Find("pooledKeyPool"),
            new GUIContent("Pooled Key Manager")
        );

        PXR_InspectorGuidance.PropertyField(
            Find("keyLockState"),
            new GUIContent("Key Lock State")
        );

        PXR_KeyLockState state =
            Find("keyLockState").objectReferenceValue
                as PXR_KeyLockState;

        PXR_UnifiedInputs input = (PXR_UnifiedInputs)target;

        if (state == null)
        {
            EditorGUILayout.HelpBox(
                "Assign PXR_KeyLockState (Manual) to this lock. " +
                "Ordinary pickup objects do not need one.",
                MessageType.Error
            );
        } else
        {
            if (state.input != input)
            {
                EditorGUILayout.HelpBox(
                    "Set Key Lock State > Input to this " +
                    "PXR_UnifiedInputs component.",
                    MessageType.Warning
                );
            }

            if (state.GetComponent<
                VRC.SDK3.Components.VRCObjectSync>() != null)
            {
                EditorGUILayout.HelpBox(
                    "Move the Manual lock state onto a " +
                    "GameObject without VRCObjectSync.",
                    MessageType.Error
                );
            }

            if (AnyLocalRows(
                "primaryEventTargets",
                "primaryEventNames",
                "primaryEventScopes"))
            {
                EditorGUILayout.HelpBox(
                    "Key locks run their events on the lock owner's " +
                    "client. Local Only events run for the lock owner, " +
                    "not for the player who used the key.",
                    MessageType.Info
                );
            }

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField(
                new GUIContent(
                    "Key Count",
                    PXR_InspectorGuidance.Tooltip(
                        "PXR_KeyLockState", "keyCount")
                ),
                state.keyCount
            );

            EditorGUILayout.Toggle(
                new GUIContent(
                    "Keys Complete",
                    PXR_InspectorGuidance.Tooltip(
                        "PXR_KeyLockState", "keysComplete")
                ),
                state.keysComplete
            );

            EditorGUI.EndDisabledGroup();
        }

        PXR_PoolManager manager =
            Find("pooledKeyPool").objectReferenceValue
                as PXR_PoolManager;

        if (manager != null &&
            state != null &&
            manager.FindLockIndex(state) < 0)
        {
            EditorGUILayout.HelpBox(
                "Register the PXR_KeyLockState in " +
                "Pool Manager > Key Locks.",
                MessageType.Warning
            );
        }
    }

    private void DrawList(
        string title,
        ref bool expanded,
        string targetPath,
        string namePath,
        string scopePath,
        string delayPath,
        string teleportPath,
        string videoPath,
        string legacyTargetPath,
        string legacyNamePath)
    {
        SerializedProperty targets = Find(targetPath);
        SerializedProperty names = Find(namePath);
        SerializedProperty scopes = Find(scopePath);
        SerializedProperty delays = Find(delayPath);
        SerializedProperty teleports = Find(teleportPath);
        SerializedProperty videos = Find(videoPath);

        int count = targets.arraySize;

        // The parallel arrays must always remain aligned.
        names.arraySize = count;
        scopes.arraySize = count;
        delays.arraySize = count;
        teleports.arraySize = count;
        videos.arraySize = count;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

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
                    delays.arraySize = 1;
                    teleports.arraySize = 1;
                    videos.arraySize = 1;

                    targets.GetArrayElementAtIndex(0)
                        .objectReferenceValue =
                        legacyTarget.objectReferenceValue;

                    names.GetArrayElementAtIndex(0)
                        .stringValue = legacyName.stringValue;

                    scopes.GetArrayElementAtIndex(0)
                        .intValue = 0;

                    delays.GetArrayElementAtIndex(0)
                        .floatValue = LegacyDelayValue();

                    SetSpecialReferences(
                        targets.GetArrayElementAtIndex(0),
                        names.GetArrayElementAtIndex(0),
                        teleports.GetArrayElementAtIndex(0),
                        videos.GetArrayElementAtIndex(0)
                    );

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
                    new GUIContent(
                        "Execution Scope",
                        "Choose Local Only, All Players, " +
                        "or the input's default Synced setting."
                    ),
                    Mathf.Clamp(scope.intValue, 0, 2),
                    new string[]
                    {
                        "Input Default",
                        "Local Only",
                        "All Players"
                    }
                );

                EditorGUI.EndDisabledGroup();

                if (!spawn && !teleport && !syncedVideo &&
                    !ResolvesToAllPlayers(scope.intValue) &&
                    UsesOwnerPersistence(
                        targetProp.objectReferenceValue,
                        nameProp.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        "This target saves its state for late joiners, " +
                        "so only its owner can change it. Set Execution " +
                        "Scope to All Players, or other players' " +
                        "activations will be ignored.",
                        MessageType.Warning
                    );
                }

                SerializedProperty eventDelay =
                    delays.GetArrayElementAtIndex(i);

                eventDelay.floatValue = Mathf.Max(
                    0f,
                    EditorGUILayout.FloatField(
                        new GUIContent(
                            "Delay (seconds)",
                            "Seconds from the input trigger until " +
                            "this event executes. Zero is immediate."
                        ),
                        eventDelay.floatValue
                    )
                );

                if (spawn)
                {
                    EditorGUILayout.HelpBox(
                        "SpawnObject uses the input's Synced " +
                        "setting, not this event's scope.",
                        MessageType.Info
                    );

                    PXR_PoolManager pool =
                        output != null
                            ? output.spawnPoolManager
                            : physics.spawnPoolManager;

                    if (Find("synced").boolValue && pool == null)
                    {
                        EditorGUILayout.HelpBox(
                            "Synced spawning requires a Pool Manager " +
                            "on the target output behavior.",
                            MessageType.Warning
                        );
                    }
                }

                if (teleport)
                {
                    EditorGUILayout.HelpBox(
                        "TeleportPlayer uses dedicated player " +
                        "targeting and network routing.",
                        MessageType.Info
                    );
                }

                if (syncedVideo)
                {
                    EditorGUILayout.HelpBox(
                        "Synchronized AVPro uses the video " +
                        "owner's network route.",
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
                    MoveEntry(
                        targets,
                        names,
                        scopes,
                        delays,
                        teleports,
                        videos,
                        i,
                        up ? i - 1 : i + 1
                    );

                    break;
                }

                if (remove)
                {
                    RemoveEntry(
                        targets,
                        names,
                        scopes,
                        delays,
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
                delays.arraySize = index + 1;
                teleports.arraySize = index + 1;
                videos.arraySize = index + 1;

                targets.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;

                names.GetArrayElementAtIndex(index)
                    .stringValue = "";

                scopes.GetArrayElementAtIndex(index)
                    .intValue = 0;

                delays.GetArrayElementAtIndex(index)
                    .floatValue = 0f;

                teleports.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;

                videos.GetArrayElementAtIndex(index)
                    .objectReferenceValue = null;
            }

            EditorGUI.indentLevel--;
        }

        // Update routing references even with foldouts collapsed.
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

    // Copy the old global delay to existing events once.
    // Legacy settings remain serialized but hidden.
    private void MigrateLegacyDelays()
    {
        SerializedProperty migrated =
            Find("eventDelaysMigrated");

        if (migrated == null || migrated.boolValue)
            return;

        float oldDelay = LegacyDelayValue();

        MigrateList(
            "primaryEventTargets",
            "primaryEventDelays",
            oldDelay
        );

        MigrateList(
            "dropEventTargets",
            "dropEventDelays",
            oldDelay
        );

        migrated.boolValue = true;
    }

    private float LegacyDelayValue()
    {
        SerializedProperty enabled = Find("useDelay");
        SerializedProperty delay = Find("delaySeconds");

        if (enabled == null ||
            !enabled.boolValue ||
            delay == null)
            return 0f;

        return Mathf.Max(0f, delay.floatValue);
    }

    private void MigrateList(
        string targetsPath,
        string delaysPath,
        float oldDelay)
    {
        SerializedProperty targets = Find(targetsPath);
        SerializedProperty delays = Find(delaysPath);

        if (targets == null || delays == null)
            return;

        int previousSize = delays.arraySize;
        delays.arraySize = targets.arraySize;

        for (int i = previousSize; i < delays.arraySize; i++)
        {
            delays.GetArrayElementAtIndex(i)
                .floatValue = oldDelay;
        }
    }

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
            nameProp.stringValue = "";

        UdonSharpBehaviour receiver =
            targetProp.objectReferenceValue
                as UdonSharpBehaviour;

        if (receiver == null)
            return;

        List<string> available =
            GetCallableMethods(receiver.GetType());

        string[] labels = new string[available.Count + 1];
        labels[0] = "<Select Function>";

        int selectedIndex = 0;

        for (int i = 0; i < available.Count; i++)
        {
            labels[i + 1] = available[i] + "()";

            if (nameProp.stringValue == available[i])
                selectedIndex = i + 1;
        }

        EditorGUI.BeginChangeCheck();

        int choice = EditorGUILayout.Popup(
            new GUIContent(
                "Event Name",
                "Select a public, parameterless method marked " +
                "with PXR_InputCallable on the target component."
            ),
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
                "This script has no public, parameterless " +
                "void methods marked [PXR_InputCallable].",
                MessageType.Info
            );
        }

        if (!string.IsNullOrEmpty(nameProp.stringValue) &&
            !available.Contains(nameProp.stringValue))
        {
            EditorGUILayout.HelpBox(
                "Saved function '" + nameProp.stringValue +
                "' is not currently whitelisted on this " +
                "target. The saved reference is preserved.",
                MessageType.Warning
            );
        }
    }

    // Reflection results per script type. Rebuilt automatically
    // after every script recompile (domain reload clears statics).
    private static readonly Dictionary<Type, List<string>> callableCache =
        new Dictionary<Type, List<string>>();

    private static List<string> GetCallableMethods(Type type)
    {
        List<string> cached;

        if (callableCache.TryGetValue(type, out cached))
            return cached;

        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly
        );

        List<string> available = new List<string>();

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

            if (!method.IsDefined(
                typeof(PXR_InputCallableAttribute),
                false))
            {
                continue;
            }

            if (!available.Contains(method.Name))
                available.Add(method.Name);
        }

        available.Sort(StringComparer.Ordinal);
        callableCache[type] = available;

        return available;
    }

    // Mirrors the runtime scope resolution in PXR_UnifiedInputs.
    private bool ResolvesToAllPlayers(int scope)
    {
        if (scope == (int)PXR_EventScope.LocalOnly)
            return false;

        if (scope == (int)PXR_EventScope.AllPlayers)
            return true;

        return Find("synced").boolValue;
    }

    // True when the selected function only acts on the
    // target owner's client because its state is persisted.
    private static bool UsesOwnerPersistence(
        UnityEngine.Object target,
        string method)
    {
        PXR_UnifiedOutputs output = target as PXR_UnifiedOutputs;

        if (output != null)
        {
            if (method == "ToggleObject")
                return output.persistObjectState;

            if (method == "ToggleComponent")
                return output.persistComponentState;

            if (method == "PlayAnimation")
                return output.persistAnimatorState &&
                    output.animationParameterType !=
                        PXR_AnimationParameterType.Trigger;

            return false;
        }

        PXR_UnifiedPhysics physics = target as PXR_UnifiedPhysics;

        if (physics == null)
            return false;

        bool radial =
            physics.radialContinuous && physics.persistRadialState;

        bool beam =
            physics.beamContinuous && physics.persistBeamState;

        if (method == "RadialForce" ||
            method == "StartRadialForce" ||
            method == "StopRadialForce")
            return radial;

        if (method == "BeamForce" ||
            method == "StartBeamForce" ||
            method == "StopBeamForce")
            return beam;

        if (method == "StopAllForces")
            return radial || beam;

        return false;
    }

    // True when any ordinary row of the list runs locally.
    private bool AnyLocalRows(
        string targetPath,
        string namePath,
        string scopePath)
    {
        SerializedProperty targets = Find(targetPath);
        SerializedProperty names = Find(namePath);
        SerializedProperty scopes = Find(scopePath);

        if (targets == null || names == null || scopes == null)
            return false;

        int count = Mathf.Min(targets.arraySize, names.arraySize);

        for (int i = 0; i < count; i++)
        {
            UnityEngine.Object target =
                targets.GetArrayElementAtIndex(i).objectReferenceValue;

            string method =
                names.GetArrayElementAtIndex(i).stringValue;

            if (target == null || string.IsNullOrEmpty(method))
                continue;

            if (method == "SpawnObject" || method == "TeleportPlayer")
                continue;

            PXR_UnifiedOutputs output = target as PXR_UnifiedOutputs;

            if (output != null &&
                method == "PlayAVProVideo" &&
                output.avProUseSyncedPrefab)
                continue;

            int scope = i < scopes.arraySize
                ? scopes.GetArrayElementAtIndex(i).intValue
                : 0;

            if (!ResolvesToAllPlayers(scope))
                return true;
        }

        return false;
    }

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

    private static void MoveEntry(
        SerializedProperty targets,
        SerializedProperty names,
        SerializedProperty scopes,
        SerializedProperty delays,
        SerializedProperty teleports,
        SerializedProperty videos,
        int from,
        int to)
    {
        targets.MoveArrayElement(from, to);
        names.MoveArrayElement(from, to);
        scopes.MoveArrayElement(from, to);
        delays.MoveArrayElement(from, to);
        teleports.MoveArrayElement(from, to);
        videos.MoveArrayElement(from, to);
    }

    private static void RemoveEntry(
        SerializedProperty targets,
        SerializedProperty names,
        SerializedProperty scopes,
        SerializedProperty delays,
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
                delays,
                teleports,
                videos,
                i + 1,
                i
            );
        }

        targets.arraySize = count - 1;
        names.arraySize = count - 1;
        scopes.arraySize = count - 1;
        delays.arraySize = count - 1;
        teleports.arraySize = count - 1;
        videos.arraySize = count - 1;
    }
}
#endif
