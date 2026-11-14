
#if UNITY_EDITOR

using System;
using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using VRC.SDKBase;

[CustomEditor(typeof(PXR_UnifiedOutputs))]
public class PXR_UnifiedOutputsEditor : Editor
{
    private bool showObject;
    private bool showDestroy;
    private bool showAnimation;
    private bool showSound;
    private bool showVideo;
    private bool showTeleport;
    private bool showSpawn;
    private bool showComponent;

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        serializedObject.Update();

        bool urlChanged = false;
        string newUrl = "";

        Info(
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

            PXR_InspectorGuidance.PropertyField(
                Find("toggleObjectTarget"),
                new GUIContent("Target GameObject")
            );

            SerializedProperty toggle = Find("objectToggle");

            PXR_InspectorGuidance.PropertyField(
                toggle,
                new GUIContent("Toggle Object")
            );

            EditorGUI.BeginDisabledGroup(toggle.boolValue);

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
                    "The output owner stores the resulting " +
                    "state for late joiners."
                );

                GameObject obj =
                    Find("toggleObjectTarget")
                        .objectReferenceValue as GameObject;

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

        End(showObject);

        // ------------------------------------
        // DESTROY OBJECT
        // ------------------------------------

        if (Begin(ref showDestroy, "DestroyObject()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "DestroyObject"
            );

            PXR_InspectorGuidance.PropertyField(
                Find("destroyTarget"),
                new GUIContent("Target GameObject")
            );

            Warning(
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

            if (selected == PXR_AnimationParameterType.Boolean)
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

                EditorGUI.BeginDisabledGroup(toggle.boolValue);

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

                max.intValue = Mathf.Max(0, max.intValue);
            }

            if (selected != PXR_AnimationParameterType.Trigger)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("persistAnimatorState"),
                    new GUIContent("Persist Animator State")
                );
            }

            ValidateAnimator(animator, selected);
        }

        End(showAnimation);

        // ------------------------------------
        // SOUND
        // ------------------------------------

        if (Begin(ref showSound, "PlaySound()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "PlaySound"
            );

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
                    "With no clips in the array, PlaySound " +
                    "uses the clip assigned to the AudioSource."
                );
            } else
            {
                Info(
                    "Each call stops the current clip and plays " +
                    "the next valid clip in the playlist. " +
                    "The sequence loops after the last clip."
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
                    "Assign the backing UdonBehaviour for " +
                    "the SDK UdonSyncPlayer (AVPro) graph."
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
                    "Direct AVPro playback does not maintain " +
                    "synchronized playback for late joiners."
                );
            }

            PXR_UnifiedOutputs proxy =
                (PXR_UnifiedOutputs)target;

            string currentUrl = "";

            if (proxy.avProVideoUrl != null)
                currentUrl = proxy.avProVideoUrl.Get();

            EditorGUI.BeginChangeCheck();

            newUrl = EditorGUILayout.TextField(
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
            } else if (!newUrl.StartsWith(
                  "https://",
                  StringComparison.OrdinalIgnoreCase))
            {
                Warning(
                    "HTTPS is recommended. Verify that VRChat " +
                    "supports the supplied video URL."
                );
            }

            Info(
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
                    "Spawn Transform, in metres per second."
                )
            );

            if (Find("velocity").vector3Value == Vector3.zero)
            {
                Info(
                    "Zero velocity: spawned objects receive " +
                    "no scripted movement."
                );
            } else
            {
                Info(
                    "Non-physics objects move at constant " +
                    "velocity using their Transform."
                );
            }

            Heading("Networked Pool Settings");

            PXR_InspectorGuidance.PropertyField(
                Find("spawnPoolManager"),
                new GUIContent("Pool Manager")
            );

            DrawDespawnControls();

            Info(
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

        End(showSpawn);

        // ------------------------------------
        // TOGGLE COMPONENT
        // ------------------------------------

        if (Begin(ref showComponent, "ToggleComponent()"))
        {
            PXR_InspectorGuidance.OutputRequirements(
                "ToggleComponent"
            );

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

            SerializedProperty toggle = Find("componentToggle");

            PXR_InspectorGuidance.PropertyField(
                toggle,
                new GUIContent("Toggle")
            );

            EditorGUI.BeginDisabledGroup(toggle.boolValue);

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

        End(showComponent);

        serializedObject.ApplyModifiedProperties();

        if (urlChanged)
        {
            PXR_UnifiedOutputs proxy =
                (PXR_UnifiedOutputs)target;

            Undo.RecordObject(
                proxy,
                "Edit PXR AVPro Preset URL"
            );

            // UdonSharp 1.x edits the behaviour directly; the old
            // ApplyProxyModifications() call is an obsolete no-op.
            proxy.avProVideoUrl = new VRCUrl(newUrl);

            PrefabUtility.RecordPrefabInstancePropertyModifications(proxy);
            EditorUtility.SetDirty(proxy);
        }
    }

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
                "The object returns after the first qualifying " +
                "collision and the configured grace period. " +
                "Pooled objects only; local spawns are not affected."
            );
        } else if (policy.intValue == 2)
        {
            DrawMinimumFloat(
                "despawnAfterSeconds",
                "Despawn After (seconds)",
                0.05f
            );

            Info(
                "Pooled objects return to the pool; local spawns " +
                "(Synced OFF) are destroyed after this time."
            );
        } else
        {
            Info(
                "The object remains active until another action " +
                "returns it. Pool capacity is finite."
            );
        }
    }

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

        string name = "";

        AnimatorControllerParameterType requiredType =
            AnimatorControllerParameterType.Bool;

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
        }

        bool found = false;

        foreach (AnimatorControllerParameter parameter
                 in controller.parameters)
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
                "Animator parameter not found or " +
                "its type does not match."
            );
        }
    }

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

    private SerializedProperty Find(string name)
    {
        return serializedObject.FindProperty(name);
    }

    private void DrawNonnegativeFloat(
        string propertyName,
        string label)
    {
        DrawMinimumFloat(propertyName, label, 0f);
    }

    private void DrawMinimumFloat(
        string propertyName,
        string label,
        float minimum)
    {
        SerializedProperty property = Find(propertyName);

        PXR_InspectorGuidance.PropertyField(
            property,
            new GUIContent(label)
        );

        property.floatValue =
            Mathf.Max(minimum, property.floatValue);
    }

    private static bool Begin(ref bool expanded, string label)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        expanded = EditorGUILayout.Foldout(
            expanded, label, true
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
#endif
