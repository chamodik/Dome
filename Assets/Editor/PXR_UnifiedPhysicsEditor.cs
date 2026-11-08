
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UdonSharp;
using UdonSharpEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(PXR_UnifiedPhysics))]
public class PXR_UnifiedPhysicsEditor : Editor
{
    private bool showChain = false;
    private bool showRadial = false;
    private bool showBeam = false;
    private bool showSpawn = false;
    private bool showDetection;

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(
            target))
            return;

        serializedObject.Update();

        Info(
            "PXR Unified Physics contains force " +
            "effects, Rigidbody spawning, and " +
            "chain events."
        );

        // =====================================
        // CHAIN EVENT
        // =====================================

        if (Begin(ref showChain, "Chain Event"))
        {
            PXR_InspectorGuidance.PhysicsRequirements("Chain");
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

                DrawMinimumFloat(
                    "chainEventDelay",
                    "Chain Delay (seconds)",
                    0f
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

        EditorGUILayout.Space(4);

        // =====================================
        // RADIAL FORCE
        // =====================================

        if (Begin(ref showRadial, "RadialForce()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements("RadialForce");
            DrawMinimumFloat(
                "radialRange",
                "Range",
                0f
            );

            PXR_InspectorGuidance.PropertyField(
                Find("radialForce"),
                new GUIContent("Force")
            );

            PXR_InspectorGuidance.PropertyField(
                Find("radialAttract"),
                new GUIContent("Attract")
            );

            SerializedProperty continuous =
                Find("radialContinuous");

            PXR_InspectorGuidance.PropertyField(
                continuous,
                new GUIContent("Continuous")
            );

            if (continuous.boolValue)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("persistRadialState"),
                    new GUIContent("Persist Radial State")
                );

                if (Find("persistRadialState").boolValue)
                {
                    Info(
                        "The emitter owner synchronizes " +
                        "the active state. Use All Players " +
                        "scope in UnifiedInputs."
                    );
                }
            } else
            {
                Info(
                    "Each activation applies one " +
                    "ForceMode.Impulse."
                );
            }
        }
        End(showRadial);

        EditorGUILayout.Space(4);

        // =====================================
        // BEAM FORCE
        // =====================================

        if (Begin(ref showBeam, "BeamForce()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements("BeamForce");
            DrawMinimumFloat(
                "beamRange",
                "Range",
                0f
            );

            PXR_InspectorGuidance.PropertyField(
                Find("beamForce"),
                new GUIContent("Force")
            );

            PXR_InspectorGuidance.PropertyField(
                Find("beamAttract"),
                new GUIContent("Attract")
            );

            PXR_InspectorGuidance.PropertyField(
                Find("beamDirection"),
                new GUIContent("Beam Direction")
            );

            DrawMinimumFloat(
                "beamRadius",
                "Beam Half-Width",
                0.01f
            );

            SerializedProperty continuous =
                Find("beamContinuous");

            PXR_InspectorGuidance.PropertyField(
                continuous,
                new GUIContent("Continuous")
            );

            if (continuous.boolValue)
            {
                PXR_InspectorGuidance.PropertyField(
                    Find("persistBeamState"),
                    new GUIContent("Persist Beam State")
                );

                if (Find("persistBeamState").boolValue)
                {
                    Info(
                        "The emitter owner synchronizes " +
                        "the active state. Use All Players " +
                        "scope in UnifiedInputs."
                    );
                }
            } else
            {
                Info(
                    "Each activation applies one " +
                    "ForceMode.Impulse."
                );
            }

            Info(
                "Local beam direction:\n" +
                "(0,0,1) = transform.forward\n" +
                "(1,0,0) = transform.right\n" +
                "(0,1,0) = transform.up"
            );

            if (Find("beamDirection")
                .vector3Value.sqrMagnitude < 0.000001f)
            {
                Warning(
                    "Zero direction defaults " +
                    "to local +Z at runtime."
                );
            }

            Warning(
                "The beam uses an oriented box " +
                "and does not stop at walls."
            );
        }
        End(showBeam);

        EditorGUILayout.Space(4);

        // =====================================
        // PHYSICS SPAWNING
        // =====================================

        if (Begin(ref showSpawn, "SpawnObject()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements("SpawnObject");
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
                    "Initial Rigidbody velocity in " +
                    "Spawn Transform local axes, m/s."
                )
            );

            if (Find("velocity").vector3Value ==
                Vector3.zero)
            {
                Info(
                    "Zero velocity: the Rigidbody " +
                    "starts with no linear movement. " +
                    "Gravity may still move it."
                );
            } else
            {
                Info(
                    "The velocity vector is converted " +
                    "to world space and assigned to " +
                    "the spawned Rigidbody."
                );
            }

            Heading("Networked Pool Settings");

            PXR_InspectorGuidance.PropertyField(
                Find("spawnPoolManager"),
                new GUIContent("Pool Manager")
            );

            DrawDespawnControls();

            Info(
                "UnifiedInputs determines spawn mode:\n" +
                "Synced OFF: instantiate local prefab.\n" +
                "Synced ON: request one pooled object."
            );

            GameObject prefab =
                Find("spawnPrefab")
                .objectReferenceValue as GameObject;

            if (prefab != null)
            {
                Rigidbody body =
                    prefab.GetComponent<Rigidbody>();

                if (body == null)
                {
                    Warning(
                        "Local Spawn Prefab is missing " +
                        "a root Rigidbody."
                    );
                } else if (body.isKinematic)
                {
                    Warning(
                        "Local Spawn Prefab Rigidbody " +
                        "is kinematic."
                    );
                }
            }
        }
        End(showSpawn);

        EditorGUILayout.Space(4);

        // =====================================
        // SHARED PHYSICS DETECTION
        // =====================================

        if (Begin(
            ref showDetection,
            "Shared Physics Detection"))
        {
            PXR_InspectorGuidance.PhysicsRequirements("Detection");
            PXR_InspectorGuidance.PropertyField(
                Find("affectedLayers"),
                new GUIContent("Affected Layers")
            );

            SerializedProperty capacity =
                Find("maxColliders");

            PXR_InspectorGuidance.PropertyField(
                capacity,
                new GUIContent("Max Colliders")
            );

            capacity.intValue = Mathf.Clamp(
                capacity.intValue,
                8,
                256
            );

            if (Find("affectedLayers").intValue == 0)
            {
                Warning(
                    "No affected layers selected."
                );
            }

            Info(
                "These settings affect radial and " +
                "beam queries, but not SpawnObject."
            );
        }
        End(showDetection);

        serializedObject.ApplyModifiedProperties();
    }

    // =====================================
    // DESPAWN CONTROLS
    // =====================================

    private void DrawDespawnControls()
    {
        SerializedProperty policy =
            Find("despawnMode");

        policy.intValue = EditorGUILayout.Popup(
            new GUIContent("Despawn Mode", PXR_InspectorGuidance.Tooltip("PXR_UnifiedPhysics", "despawnMode")),
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
                "The first collision schedules " +
                "return after the grace period."
            );
        } else if (policy.intValue == 2)
        {
            DrawMinimumFloat(
                "despawnAfterSeconds",
                "Despawn After (seconds)",
                0.05f
            );
        } else
        {
            Info(
                "No automatic return. Pooled objects " +
                "consume pool capacity until returned."
            );
        }
    }

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
            new GUIContent("Chain Event Name", PXR_InspectorGuidance.Tooltip("PXR_UnifiedPhysics", "chainEventName")),
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
    // INSPECTOR UTILITIES
    // =====================================

    private SerializedProperty Find(string name)
    {
        return serializedObject.FindProperty(name);
    }

    private void DrawMinimumFloat(
        string propertyName,
        string label,
        float minimum)
    {
        SerializedProperty property =
            Find(propertyName);

        PXR_InspectorGuidance.PropertyField(
            property,
            new GUIContent(label)
        );

        property.floatValue =
            Mathf.Max(minimum, property.floatValue);
    }

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
            value,
            EditorStyles.boldLabel
        );
    }

    private static void Info(string value)
    {
        EditorGUILayout.HelpBox(
            value,
            MessageType.Info
        );
    }

    private static void Warning(string value)
    {
        EditorGUILayout.HelpBox(
            value,
            MessageType.Warning
        );
    }
}

#endif
