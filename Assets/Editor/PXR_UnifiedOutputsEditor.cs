
#if UNITY_EDITOR

<<<<<<< Updated upstream
using UnityEngine;
using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using VRC.SDKBase;
using System;
using System.Collections.Generic;
using System.Reflection;
=======
using System;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using VRC.SDKBase;
>>>>>>> Stashed changes

[CustomEditor(typeof(PXR_UnifiedOutputs))]
public class PXR_UnifiedOutputsEditor : Editor
{
<<<<<<< Updated upstream
    private bool showChain = false;
=======
>>>>>>> Stashed changes
    private bool showObject;
    private bool showDestroy;
    private bool showAnimation;
    private bool showSound;
<<<<<<< Updated upstream
    private bool showVideo = false;
    private bool showTeleport;
    private bool showSpawn = false;
=======
    private bool showVideo;
    private bool showTeleport;
    private bool showSpawn;
>>>>>>> Stashed changes
    private bool showComponent;

    public override void OnInspectorGUI()
    {
<<<<<<< Updated upstream
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(
            target))
=======
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
>>>>>>> Stashed changes
            return;

        serializedObject.Update();

        bool urlChanged = false;
        string newUrl = "";

        Info(
<<<<<<< Updated upstream
            "Configure output behaviors, then " +
            "select their public functions through " +
            "PXR_UnifiedInputs."
        );

        // =====================================
        // CHAIN EVENT
        // =====================================

        if (Begin(ref showChain, "Chain Event"))
        {
            PXR_InspectorGuidance.OutputRequirements("Chain");
            SerializedProperty enabled =
                Find("enableChainEvent");

            PXR_InspectorGuidance.PropertyField(
                enabled,
                new GUIContent("Enable Chain Event")
            );

            if (enabled.boolValue)
            {
                DrawMethodSelector(
                    Find("chainEventTarget"),
                    Find("chainEventName")
                );

                DrawNonnegativeFloat(
                    "chainEventDelay",
                    "Chain Delay (seconds)"
                );

                if (Find("chainEventTarget")
                    .objectReferenceValue == target)
                {
                    Warning(
                        "Direct self-chaining is blocked."
                    );
                }

                Warning(
                    "Indirect event cycles are not " +
                    "automatically detected."
                );
            }
        }
        End(showChain);

        // =====================================
        // TOGGLE OBJECT
        // =====================================

        if (Begin(ref showObject, "ToggleObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements("ToggleObject");
=======
            "Configure output behaviors here and select their " +
            "public functions through PXR_UnifiedInputs. " +
            "Each input event has its own execution delay."
        );

        // ------------------------------------
        // TOGGLE OBJECT
        // ------------------------------------

        if (Begin(ref showObject, "ToggleObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "ToggleObject"
            );

>>>>>>> Stashed changes
            PXR_InspectorGuidance.PropertyField(
                Find("toggleObjectTarget"),
                new GUIContent("Target GameObject")
            );

<<<<<<< Updated upstream
            SerializedProperty toggle =
                Find("objectToggle");
=======
            SerializedProperty toggle = Find("objectToggle");
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                toggle,
                new GUIContent("Toggle Object")
            );

<<<<<<< Updated upstream
            EditorGUI.BeginDisabledGroup(
                toggle.boolValue
            );
=======
            EditorGUI.BeginDisabledGroup(toggle.boolValue);
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                Find("objectState"),
                new GUIContent("Object State")
            );

            EditorGUI.EndDisabledGroup();

            PXR_InspectorGuidance.PropertyField(
                Find("persistObjectState"),
                new GUIContent("Persist Object State")
            );

            if (Find("persistObjectState").boolValue)
            {
                Info(
<<<<<<< Updated upstream
                    "The output owner stores the " +
                    "resulting state for late joiners."
=======
                    "The output owner stores the resulting " +
                    "state for late joiners."
>>>>>>> Stashed changes
                );

                GameObject obj =
                    Find("toggleObjectTarget")
<<<<<<< Updated upstream
                    .objectReferenceValue as GameObject;
=======
                        .objectReferenceValue as GameObject;
>>>>>>> Stashed changes

                if (obj ==
                    ((PXR_UnifiedOutputs)target).gameObject)
                {
                    Warning(
                        "Do not deactivate the GameObject " +
                        "containing this controller."
                    );
                }
            }
        }
<<<<<<< Updated upstream
        End(showObject);

        // =====================================
        // DESTROY OBJECT
        // =====================================

        if (Begin(ref showDestroy, "DestroyObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements("DestroyObject");
=======

        End(showObject);

        // ------------------------------------
        // DESTROY OBJECT
        // ------------------------------------

        if (Begin(ref showDestroy, "DestroyObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "DestroyObject"
            );

>>>>>>> Stashed changes
            PXR_InspectorGuidance.PropertyField(
                Find("destroyTarget"),
                new GUIContent("Target GameObject")
            );

            Warning(
<<<<<<< Updated upstream
                "This function currently performs " +
                "SetActive(false), not permanent " +
                "networked destruction."
            );
        }
        End(showDestroy);

        // =====================================
        // ANIMATION
        // =====================================

        if (Begin(ref showAnimation, "PlayAnimation()"))
        {
            PXR_InspectorGuidance.OutputRequirements("PlayAnimation");
            SerializedProperty animator =
                Find("targetAnimator");

            SerializedProperty type =
                Find("animationParameterType");
=======
                "This function performs SetActive(false), " +
                "not permanent synchronized destruction."
            );
        }

        End(showDestroy);

        // ------------------------------------
        // ANIMATION
        // ------------------------------------

        if (Begin(ref showAnimation, "PlayAnimation()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "PlayAnimation"
            );

            SerializedProperty animator = Find("targetAnimator");
            SerializedProperty type = Find("animationParameterType");
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                animator,
                new GUIContent("Target Animator")
            );

            PXR_InspectorGuidance.PropertyField(
                type,
                new GUIContent("Parameter Type")
            );

            PXR_AnimationParameterType selected =
                (PXR_AnimationParameterType)type.intValue;

<<<<<<< Updated upstream
            if (selected ==
                PXR_AnimationParameterType.Boolean)
=======
            if (selected == PXR_AnimationParameterType.Boolean)
>>>>>>> Stashed changes
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("animationBoolName"),
                    new GUIContent("Bool Name")
                );

                SerializedProperty toggle =
                    Find("animationToggleBool");

                PXR_InspectorGuidance.PropertyField(
                    toggle,
                    new GUIContent("Toggle Bool")
                );

<<<<<<< Updated upstream
                EditorGUI.BeginDisabledGroup(
                    toggle.boolValue
                );
=======
                EditorGUI.BeginDisabledGroup(toggle.boolValue);
>>>>>>> Stashed changes

                PXR_InspectorGuidance.PropertyField(
                    Find("animationBoolValue"),
                    new GUIContent("Bool Value")
                );

                EditorGUI.EndDisabledGroup();
            } else if (selected ==
                       PXR_AnimationParameterType.Trigger)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("animationTriggerName"),
                    new GUIContent("Trigger Name")
                );
            } else
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("animationIntName"),
                    new GUIContent("Int Name")
                );

                SerializedProperty increment =
                    Find("animationIncrement");

                PXR_InspectorGuidance.PropertyField(
                    increment,
                    new GUIContent("Increment")
                );

                EditorGUI.BeginDisabledGroup(
                    increment.intValue == 0
                );

                PXR_InspectorGuidance.PropertyField(
                    Find("animationMaxValue"),
                    new GUIContent("Max Value")
                );

                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(
                    increment.intValue != 0
                );

                PXR_InspectorGuidance.PropertyField(
                    Find("animationFixedValue"),
                    new GUIContent("Fixed Value")
                );

                EditorGUI.EndDisabledGroup();

                SerializedProperty max =
                    Find("animationMaxValue");

<<<<<<< Updated upstream
                max.intValue =
                    Mathf.Max(0, max.intValue);
            }

            if (selected !=
                PXR_AnimationParameterType.Trigger)
=======
                max.intValue = Mathf.Max(0, max.intValue);
            }

            if (selected != PXR_AnimationParameterType.Trigger)
>>>>>>> Stashed changes
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("persistAnimatorState"),
                    new GUIContent("Persist Animator State")
                );
            }

            ValidateAnimator(animator, selected);
        }
<<<<<<< Updated upstream
        End(showAnimation);

        // =====================================
        // SOUND
        // =====================================


        if (Begin(ref showSound, "PlaySound()"))
        {
            PXR_InspectorGuidance.OutputRequirements("PlaySound");
=======

        End(showAnimation);

        // ------------------------------------
        // SOUND
        // ------------------------------------

        if (Begin(ref showSound, "PlaySound()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "PlaySound"
            );

>>>>>>> Stashed changes
            SerializedProperty audioSource =
                Find("targetAudioSource");

            SerializedProperty audioClips =
                Find("audioClips");

            PXR_InspectorGuidance.PropertyField(
                audioSource,
                new GUIContent("Target AudioSource")
            );

            PXR_InspectorGuidance.PropertyField(
                audioClips,
                new GUIContent("Audio Clips"),
                true
            );

            AudioSource audio =
                audioSource.objectReferenceValue as AudioSource;

            if (audio == null)
            {
                Warning("Assign an AudioSource.");
            } else if (audioClips.arraySize == 0)
            {
                if (audio.clip == null)
                    Warning("AudioSource has no AudioClip.");

                Info(
<<<<<<< Updated upstream
                    "No clips in the array. PlaySound " +
                    "will use the clip assigned directly " +
                    "to the AudioSource."
=======
                    "With no clips in the array, PlaySound " +
                    "uses the clip assigned to the AudioSource."
>>>>>>> Stashed changes
                );
            } else
            {
                Info(
<<<<<<< Updated upstream
                    "Each PlaySound call stops the current " +
                    "clip and plays the next clip in the " +
                    "array. The sequence loops after the " +
                    "last clip."
=======
                    "Each call stops the current clip and plays " +
                    "the next valid clip in the playlist. " +
                    "The sequence loops after the last clip."
>>>>>>> Stashed changes
                );

                for (int i = 0; i < audioClips.arraySize; i++)
                {
                    if (audioClips.GetArrayElementAtIndex(i)
                        .objectReferenceValue == null)
                    {
                        Warning(
                            "Audio Clip " + (i + 1) +
                            " is empty and will be skipped."
                        );
                    }
                }
            }

            Info(
<<<<<<< Updated upstream
                "For spatial audio, add VRC Spatial " +
                "Audio Source alongside the Unity " +
                "AudioSource on the same GameObject."
            );
        }
        End(showSound);

        // =====================================
        // AVPRO
        // =====================================

        if (Begin(ref showVideo, "PlayAVProVideo()"))
        {
            PXR_InspectorGuidance.OutputRequirements("PlayAVProVideo");
=======
                "For spatial audio, add VRC Spatial Audio Source " +
                "alongside the Unity AudioSource."
            );
        }

        End(showSound);

        // ------------------------------------
        // AVPRO
        // ------------------------------------

        if (Begin(ref showVideo, "PlayAVProVideo()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "PlayAVProVideo"
            );

>>>>>>> Stashed changes
            SerializedProperty useSample =
                Find("avProUseSyncedPrefab");

            PXR_InspectorGuidance.PropertyField(
                useSample,
                new GUIContent("Use SDK Synced Player")
            );

            if (useSample.boolValue)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("avProSyncPlayer"),
                    new GUIContent("AVPro Sync Player")
                );

                Info(
<<<<<<< Updated upstream
                    "Assign the backing UdonBehaviour " +
                    "for the SDK UdonSyncPlayer (AVPro) " +
                    "synchronization graph."
=======
                    "Assign the backing UdonBehaviour for " +
                    "the SDK UdonSyncPlayer (AVPro) graph."
>>>>>>> Stashed changes
                );

                if (Find("avProSyncPlayer")
                    .objectReferenceValue == null)
                {
                    Warning(
                        "Assign the synchronized video " +
                        "player's UdonBehaviour."
                    );
                }
            } else
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("avProPlayer"),
                    new GUIContent("Direct AVPro Player")
                );

                Warning(
<<<<<<< Updated upstream
                    "Direct AVPro playback does not " +
                    "maintain synchronized playback " +
                    "state for late joiners."
=======
                    "Direct AVPro playback does not maintain " +
                    "synchronized playback for late joiners."
>>>>>>> Stashed changes
                );
            }

            PXR_UnifiedOutputs proxy =
                (PXR_UnifiedOutputs)target;

            string currentUrl = "";

            if (proxy.avProVideoUrl != null)
                currentUrl = proxy.avProVideoUrl.Get();

            EditorGUI.BeginChangeCheck();

            newUrl = EditorGUILayout.TextField(
<<<<<<< Updated upstream
                new GUIContent("Preset Video URL", "HTTPS video URL used when PlayAVProVideo() is triggered."),
                currentUrl
            );

            urlChanged =
                EditorGUI.EndChangeCheck();

            if (string.IsNullOrWhiteSpace(newUrl))
            {
                Warning(
                    "Assign a preset video URL."
                );
=======
                new GUIContent(
                    "Preset Video URL",
                    "HTTPS URL used when PlayAVProVideo() is triggered."
                ),
                currentUrl
            );

            urlChanged = EditorGUI.EndChangeCheck();

            if (string.IsNullOrWhiteSpace(newUrl))
            {
                Warning("Assign a preset video URL.");
>>>>>>> Stashed changes
            } else if (!newUrl.StartsWith(
                  "https://",
                  StringComparison.OrdinalIgnoreCase))
            {
                Warning(
<<<<<<< Updated upstream
                    "HTTPS is recommended. Verify the " +
                    "URL is supported by VRChat."
=======
                    "HTTPS is recommended. Verify that VRChat " +
                    "supports the supplied video URL."
>>>>>>> Stashed changes
                );
            }

            Info(
<<<<<<< Updated upstream
                "The URL is authored in Unity. " +
                "Players do not enter URLs through " +
                "the PXR playback action."
            );
        }
        End(showVideo);

        // =====================================
        // TELEPORT
        // =====================================

        if (Begin(ref showTeleport, "TeleportPlayer()"))
        {
            PXR_InspectorGuidance.OutputRequirements("TeleportPlayer");
            SerializedProperty mode =
                Find("teleportMode");
=======
                "The URL is configured in Unity, not entered " +
                "by players through the PXR action."
            );
        }

        End(showVideo);

        // ------------------------------------
        // TELEPORT
        // ------------------------------------

        if (Begin(ref showTeleport, "TeleportPlayer()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "TeleportPlayer"
            );

            SerializedProperty mode = Find("teleportMode");
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                mode,
                new GUIContent("Teleport Mode")
            );

            PXR_TeleportMode selected =
                (PXR_TeleportMode)mode.intValue;

            if (selected ==
                PXR_TeleportMode.TeleportByUsername)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("teleportUsername"),
                    new GUIContent("Display Name")
                );
            }

            if (selected ==
                PXR_TeleportMode.TeleportByRange)
            {
                DrawNonnegativeFloat(
                    "teleportRange",
                    "Range"
                );
            }

            PXR_InspectorGuidance.PropertyField(
                Find("teleportTargets"),
                new GUIContent("Teleport Destinations"),
                true
            );

            if (Find("teleportTargets").arraySize == 0)
            {
<<<<<<< Updated upstream
                Warning(
                    "Assign at least one destination."
                );
            }
        }
        End(showTeleport);

        // =====================================
        // SPAWN OBJECT
        // =====================================

        if (Begin(ref showSpawn, "SpawnObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements("SpawnObject");
=======
                Warning("Assign at least one destination.");
            }
        }

        End(showTeleport);

        // ------------------------------------
        // SPAWNING
        // ------------------------------------

        if (Begin(ref showSpawn, "SpawnObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "SpawnObject"
            );

>>>>>>> Stashed changes
            PXR_InspectorGuidance.PropertyField(
                Find("spawnPrefab"),
                new GUIContent("Spawn Prefab (Local)")
            );

            PXR_InspectorGuidance.PropertyField(
                Find("spawnTransform"),
                new GUIContent("Spawn Transform")
            );

            PXR_InspectorGuidance.PropertyField(
                Find("velocity"),
                new GUIContent(
                    "Velocity",
                    "Local-space velocity relative to " +
<<<<<<< Updated upstream
                    "Spawn Transform, in m/s."
                )
            );

            if (Find("velocity").vector3Value ==
                Vector3.zero)
            {
                Info(
                    "Zero velocity: spawned objects " +
                    "receive no scripted movement."
=======
                    "Spawn Transform, in metres per second."
                )
            );

            if (Find("velocity").vector3Value == Vector3.zero)
            {
                Info(
                    "Zero velocity: spawned objects receive " +
                    "no scripted movement."
>>>>>>> Stashed changes
                );
            } else
            {
                Info(
<<<<<<< Updated upstream
                    "Non-physics objects are moved by " +
                    "Transform at constant velocity."
=======
                    "Non-physics objects move at constant " +
                    "velocity using their Transform."
>>>>>>> Stashed changes
                );
            }

            Heading("Networked Pool Settings");

            PXR_InspectorGuidance.PropertyField(
                Find("spawnPoolManager"),
                new GUIContent("Pool Manager")
            );

            DrawDespawnControls();

            Info(
<<<<<<< Updated upstream
                "UnifiedInputs determines spawn mode:\n" +
                "Synced OFF: Spawn Prefab locally.\n" +
                "Synced ON: request a shared pooled object.\n\n" +
                "Despawn policies apply to pooled objects."
            );

            GameObject prefab =
                Find("spawnPrefab")
                .objectReferenceValue as GameObject;
=======
                "UnifiedInputs selects the spawning route:\n" +
                "Synced OFF: spawn a local prefab.\n" +
                "Synced ON: request a shared pooled object.\n\n" +
                "Despawn After Time applies to both. " +
                "Despawn on Collision applies to pooled objects only."
            );

            PXR_PoolManager spawnPool =
                Find("spawnPoolManager")
                    .objectReferenceValue as PXR_PoolManager;

            WarnIfPoolNeedsTrigger(spawnPool);
            WarnIfPoolTransfersOnCollision(spawnPool);

            GameObject prefab =
                Find("spawnPrefab")
                    .objectReferenceValue as GameObject;
>>>>>>> Stashed changes

            if (prefab != null &&
                prefab.GetComponent<Rigidbody>() != null)
            {
                Warning(
                    "The local prefab has a Rigidbody. " +
                    "For Rigidbody velocity, use " +
                    "PXR_UnifiedPhysics.SpawnObject()."
                );
            }
        }
<<<<<<< Updated upstream
        End(showSpawn);

        // =====================================
        // COMPONENT TOGGLE
        // =====================================

        if (Begin(ref showComponent, "ToggleComponent()"))
        {
            PXR_InspectorGuidance.OutputRequirements("ToggleComponent");
=======

        End(showSpawn);

        // ------------------------------------
        // TOGGLE COMPONENT
        // ------------------------------------

        if (Begin(ref showComponent, "ToggleComponent()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "ToggleComponent"
            );

>>>>>>> Stashed changes
            SerializedProperty type =
                Find("componentTargetType");

            PXR_InspectorGuidance.PropertyField(
                type,
                new GUIContent("Component Category")
            );

            PXR_ComponentTargetType selected =
                (PXR_ComponentTargetType)type.intValue;

            if (selected ==
                PXR_ComponentTargetType.Behaviour)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("targetBehaviour"),
                    new GUIContent("Target Behaviour")
                );
            } else if (selected ==
                       PXR_ComponentTargetType.Renderer)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("targetRenderer"),
                    new GUIContent("Target Renderer")
                );
            } else
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("targetCollider"),
                    new GUIContent("Target Collider")
                );
            }

<<<<<<< Updated upstream
            SerializedProperty toggle =
                Find("componentToggle");
=======
            SerializedProperty toggle = Find("componentToggle");
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                toggle,
                new GUIContent("Toggle")
            );

<<<<<<< Updated upstream
            EditorGUI.BeginDisabledGroup(
                toggle.boolValue
            );
=======
            EditorGUI.BeginDisabledGroup(toggle.boolValue);
>>>>>>> Stashed changes

            PXR_InspectorGuidance.PropertyField(
                Find("componentEnabled"),
                new GUIContent("Enabled")
            );

            EditorGUI.EndDisabledGroup();

            PXR_InspectorGuidance.PropertyField(
                Find("persistComponentState"),
                new GUIContent("Persist Component State")
            );
        }
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
        End(showComponent);

        serializedObject.ApplyModifiedProperties();

<<<<<<< Updated upstream
        // =====================================
        // EDITOR-ONLY AVPRO URL CONVERSION
        // =====================================

=======
>>>>>>> Stashed changes
        if (urlChanged)
        {
            PXR_UnifiedOutputs proxy =
                (PXR_UnifiedOutputs)target;

            Undo.RecordObject(
                proxy,
                "Edit PXR AVPro Preset URL"
            );

<<<<<<< Updated upstream
            proxy.avProVideoUrl =
                new VRCUrl(newUrl);

            proxy.ApplyProxyModifications();

=======
            // UdonSharp 1.x edits the behaviour directly; the old
            // ApplyProxyModifications() call is an obsolete no-op.
            proxy.avProVideoUrl = new VRCUrl(newUrl);

            PrefabUtility.RecordPrefabInstancePropertyModifications(proxy);
>>>>>>> Stashed changes
            EditorUtility.SetDirty(proxy);
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // DESPAWN POLICY CONTROLS
    // =====================================

    private void DrawDespawnControls()
    {
        SerializedProperty policy =
            Find("despawnMode");

        policy.intValue = EditorGUILayout.Popup(
            new GUIContent("Despawn Mode", PXR_InspectorGuidance.Tooltip("PXR_UnifiedOutputs", "despawnMode")),
=======
    private void DrawDespawnControls()
    {
        SerializedProperty policy = Find("despawnMode");

        policy.intValue = EditorGUILayout.Popup(
            new GUIContent(
                "Despawn Mode",
                PXR_InspectorGuidance.Tooltip(
                    "PXR_UnifiedOutputs", "despawnMode"
                )
            ),
>>>>>>> Stashed changes
            Mathf.Clamp(policy.intValue, 0, 2),
            new string[]
            {
                "Don't Despawn",
                "Despawn on Collision",
                "Despawn After Time"
            }
        );

        if (policy.intValue == 1)
        {
            DrawMinimumFloat(
                "collisionReturnDelay",
                "Collision Return Delay (seconds)",
                0.02f
            );

            Info(
<<<<<<< Updated upstream
                "The object returns after the first " +
                "qualifying collision and grace period."
=======
                "The object returns after the first qualifying " +
                "collision and the configured grace period. " +
                "Pooled objects only; local spawns are not affected."
>>>>>>> Stashed changes
            );
        } else if (policy.intValue == 2)
        {
            DrawMinimumFloat(
                "despawnAfterSeconds",
                "Despawn After (seconds)",
                0.05f
            );
<<<<<<< Updated upstream
        } else
        {
            Info(
                "The object remains active until " +
                "another action returns it. Pool " +
                "capacity is finite."
=======

            Info(
                "Pooled objects return to the pool; local spawns " +
                "(Synced OFF) are destroyed after this time."
            );
        } else
        {
            Info(
                "The object remains active until another action " +
                "returns it. Pool capacity is finite."
>>>>>>> Stashed changes
            );
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // CHAIN METHOD SELECTOR
    // =====================================

    private static void DrawMethodSelector(
        SerializedProperty targetProperty,
        SerializedProperty nameProperty)
    {
        EditorGUI.BeginChangeCheck();

        PXR_InspectorGuidance.PropertyField(
            targetProperty,
            new GUIContent("Chain Event Target")
        );

        if (EditorGUI.EndChangeCheck())
            nameProperty.stringValue = "";

        UdonSharpBehaviour receiver =
            targetProperty.objectReferenceValue
            as UdonSharpBehaviour;

        if (receiver == null)
        {
            Info("Assign a Chain Event Target.");
            return;
        }

        MethodInfo[] methods =
            receiver.GetType().GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );

        List<string> names =
            new List<string>();

        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];

            if (method.ReturnType != typeof(void) ||
                method.GetParameters().Length != 0 ||
                method.IsSpecialName ||
                method.Name.StartsWith("_"))
                continue;

            string name = method.Name;

            if (receiver is PXR_UnifiedOutputs &&
                !IsOutputMethod(name))
                continue;

            if (receiver is PXR_UnifiedPhysics &&
                !IsPhysicsMethod(name))
                continue;

            names.Add(name);
        }

        names.Sort(StringComparer.Ordinal);

        string[] options =
            new string[names.Count + 1];

        options[0] = "<Select Function>";

        int current = 0;

        for (int i = 0; i < names.Count; i++)
        {
            options[i + 1] = names[i] + "()";

            if (nameProperty.stringValue == names[i])
                current = i + 1;
        }

        EditorGUI.BeginChangeCheck();

        int next = EditorGUILayout.Popup(
            new GUIContent("Chain Event Name", PXR_InspectorGuidance.Tooltip("PXR_UnifiedOutputs", "chainEventName")),
            current,
            options
        );

        if (EditorGUI.EndChangeCheck())
        {
            nameProperty.stringValue =
                next == 0 ? "" : names[next - 1];
        }
    }

    // =====================================
    // METHOD WHITELISTS
    // =====================================

    private static bool IsOutputMethod(string name)
    {
        return
            name == "ToggleObject" ||
            name == "ToggleComponent" ||
            name == "PlayAnimation" ||
            name == "PlaySound" ||
            name == "TeleportPlayer" ||
            name == "SpawnObject" ||
            name == "DestroyObject" ||
            name == "PlayAVProVideo" ||
            name == "ChainEvent";
    }

    private static bool IsPhysicsMethod(string name)
    {
        return
            name == "RadialForce" ||
            name == "BeamForce" ||
            name == "SpawnObject" ||
            name == "StartRadialForce" ||
            name == "StopRadialForce" ||
            name == "StartBeamForce" ||
            name == "StopBeamForce" ||
            name == "StopAllForces" ||
            name == "ChainEvent";
    }

    // =====================================
    // ANIMATOR VALIDATION
    // =====================================

=======
>>>>>>> Stashed changes
    private void ValidateAnimator(
        SerializedProperty property,
        PXR_AnimationParameterType type)
    {
        Animator animator =
            property.objectReferenceValue as Animator;

        if (animator == null)
        {
            Warning("Assign a Target Animator.");
            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
<<<<<<< Updated upstream
            Warning(
                "The Animator has no controller."
            );
            return;
        }

=======
            Warning("The Animator has no controller.");
            return;
        }

        // Animator.parameters is usually empty outside Play mode,
        // so read the parameters from the controller asset.
        RuntimeAnimatorController runtimeController =
            animator.runtimeAnimatorController;

        AnimatorOverrideController overrideController =
            runtimeController as AnimatorOverrideController;

        if (overrideController != null)
            runtimeController = overrideController.runtimeAnimatorController;

        UnityEditor.Animations.AnimatorController controller =
            runtimeController as UnityEditor.Animations.AnimatorController;

        // Unknown controller type: skip rather than warn falsely.
        if (controller == null)
            return;

>>>>>>> Stashed changes
        string name = "";

        AnimatorControllerParameterType requiredType =
            AnimatorControllerParameterType.Bool;

<<<<<<< Updated upstream
        if (type ==
            PXR_AnimationParameterType.Boolean)
        {
            name = Find("animationBoolName").stringValue;
            requiredType =
                AnimatorControllerParameterType.Bool;
        } else if (type ==
                   PXR_AnimationParameterType.Trigger)
        {
            name =
                Find("animationTriggerName").stringValue;

            requiredType =
                AnimatorControllerParameterType.Trigger;
        } else
        {
            name = Find("animationIntName").stringValue;
            requiredType =
                AnimatorControllerParameterType.Int;
=======
        if (type == PXR_AnimationParameterType.Boolean)
        {
            name = Find("animationBoolName").stringValue;
            requiredType = AnimatorControllerParameterType.Bool;
        } else if (type == PXR_AnimationParameterType.Trigger)
        {
            name = Find("animationTriggerName").stringValue;
            requiredType = AnimatorControllerParameterType.Trigger;
        } else
        {
            name = Find("animationIntName").stringValue;
            requiredType = AnimatorControllerParameterType.Int;
>>>>>>> Stashed changes
        }

        bool found = false;

        foreach (AnimatorControllerParameter parameter
<<<<<<< Updated upstream
                 in animator.parameters)
=======
                 in controller.parameters)
>>>>>>> Stashed changes
        {
            if (parameter.name == name &&
                parameter.type == requiredType)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            Warning(
<<<<<<< Updated upstream
                "Animator parameter not found " +
                "or its type does not match."
=======
                "Animator parameter not found or " +
                "its type does not match."
>>>>>>> Stashed changes
            );
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // INSPECTOR UTILITIES
    // =====================================
=======
    // Non-physics pooled objects are kinematic, and Unity reports no
    // collisions between kinematic and static colliders. Collision
    // despawn therefore needs a trigger collider on the projectile.
    private void WarnIfPoolNeedsTrigger(PXR_PoolManager pool)
    {
        if (pool == null ||
            pool.objectPool == null ||
            Find("despawnMode").intValue !=
                (int)PXR_DespawnMode.OnCollision)
            return;

        GameObject[] objects = pool.objectPool.Pool;

        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj == null)
                continue;

            Collider collider = obj.GetComponent<Collider>();

            if (collider == null || !collider.isTrigger)
            {
                Warning(
                    "Non-physics pooled objects only detect walls " +
                    "through a trigger collider. Enable Is Trigger " +
                    "on the projectile's collider (" + obj.name + ")."
                );
                return;
            }
        }
    }

    // A collision ownership transfer moves a pooled object away from the
    // pool owner, which then has to reclaim it before returning it or
    // registering it as a key. Grabbable objects (VRCPickup) are skipped
    // because players legitimately take ownership of them.
    private static void WarnIfPoolTransfersOnCollision(PXR_PoolManager pool)
    {
        if (pool == null || pool.objectPool == null)
            return;

        GameObject[] objects = pool.objectPool.Pool;

        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj == null ||
                obj.GetComponent<VRC.SDK3.Components.VRCPickup>() != null)
                continue;

            VRC.SDK3.Components.VRCObjectSync sync =
                obj.GetComponent<VRC.SDK3.Components.VRCObjectSync>();

            if (sync == null)
                continue;

            SerializedProperty transfer =
                new SerializedObject(sync)
                    .FindProperty("AllowCollisionOwnershipTransfer");

            if (transfer != null && transfer.boolValue)
            {
                Warning(
                    "Pooled object '" + obj.name + "' allows collision " +
                    "ownership transfer. A collision can take it away " +
                    "from the pool owner and delay its return or key " +
                    "registration. Turn off Allow Collision Ownership " +
                    "Transfer on its VRCObjectSync."
                );
                return;
            }
        }
    }
>>>>>>> Stashed changes

    private SerializedProperty Find(string name)
    {
        return serializedObject.FindProperty(name);
    }

    private void DrawNonnegativeFloat(
        string propertyName,
        string label)
    {
<<<<<<< Updated upstream
        DrawMinimumFloat(
            propertyName,
            label,
            0f
        );
=======
        DrawMinimumFloat(propertyName, label, 0f);
>>>>>>> Stashed changes
    }

    private void DrawMinimumFloat(
        string propertyName,
        string label,
        float minimum)
    {
<<<<<<< Updated upstream
        SerializedProperty property =
            Find(propertyName);
=======
        SerializedProperty property = Find(propertyName);
>>>>>>> Stashed changes

        PXR_InspectorGuidance.PropertyField(
            property,
            new GUIContent(label)
        );

        property.floatValue =
            Mathf.Max(minimum, property.floatValue);
    }

<<<<<<< Updated upstream
    private static bool Begin(
        ref bool expanded,
        string label)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        expanded = EditorGUILayout.Foldout(
            expanded,
            label,
            true
=======
    private static bool Begin(ref bool expanded, string label)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        expanded = EditorGUILayout.Foldout(
            expanded, label, true
>>>>>>> Stashed changes
        );

        if (expanded)
            EditorGUI.indentLevel++;

        return expanded;
    }

    private static void End(bool expanded)
    {
        if (expanded)
            EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
    }

<<<<<<< Updated upstream
    private static void Heading(string text)
    {
        EditorGUILayout.LabelField(
            text,
            EditorStyles.boldLabel
        );
    }

    private static void Info(string text)
    {
        EditorGUILayout.HelpBox(
            text,
            MessageType.Info
        );
    }

    private static void Warning(string text)
    {
        EditorGUILayout.HelpBox(
            text,
            MessageType.Warning
        );
    }
}

=======
    private static void Heading(string value)
    {
        EditorGUILayout.LabelField(
            value, EditorStyles.boldLabel
        );
    }

    private static void Info(string value)
    {
        EditorGUILayout.HelpBox(value, MessageType.Info);
    }

    private static void Warning(string value)
    {
        EditorGUILayout.HelpBox(value, MessageType.Warning);
    }
}
>>>>>>> Stashed changes
#endif
