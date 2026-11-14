
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UdonSharpEditor;

[CustomEditor(typeof(PXR_UnifiedPhysics))]
public class PXR_UnifiedPhysicsEditor : Editor
{
    private bool showRadial;
    private bool showBeam;
    private bool showSpawn;
    private bool showDetection;

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        serializedObject.Update();

        Info(
            "PXR Unified Physics provides radial forces, " +
            "beam forces, and Rigidbody spawning. " +
            "Use PXR_UnifiedInputs for event delays and scope."
        );

        if (Begin(ref showRadial, "RadialForce()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements(
                "RadialForce"
            );

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
                        "The emitter owner synchronizes the " +
                        "active state. Select All Players " +
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

        if (Begin(ref showBeam, "BeamForce()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements(
                "BeamForce"
            );

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
                        "The emitter owner synchronizes the " +
                        "active state. Select All Players " +
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
                    "Zero direction defaults to local +Z."
                );
            }

            Warning(
                "The beam uses an oriented box and " +
                "does not stop at walls."
            );
        }

        End(showBeam);
        EditorGUILayout.Space(4);

        if (Begin(ref showSpawn, "SpawnObject()"))
        {
            PXR_InspectorGuidance.PhysicsRequirements(
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
                    "Initial Rigidbody velocity in " +
                    "Spawn Transform local axes, m/s."
                )
            );

            if (Find("velocity").vector3Value == Vector3.zero)
            {
                Info(
                    "Zero velocity: Rigidbody starts without " +
                    "linear movement. Gravity may still act."
                );
            } else
            {
                Info(
                    "Velocity is transformed into world space " +
                    "and assigned to the Rigidbody."
                );
            }

            Heading("Networked Pool Settings");

            PXR_InspectorGuidance.PropertyField(
                Find("spawnPoolManager"),
                new GUIContent("Pool Manager")
            );

            DrawDespawnControls();

            Info(
                "UnifiedInputs selects the spawn route:\n" +
                "Synced OFF: instantiate the local prefab.\n" +
                "Synced ON: request one pooled object."
            );

            WarnIfPoolTransfersOnCollision(
                Find("spawnPoolManager")
                    .objectReferenceValue as PXR_PoolManager
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

        if (Begin(ref showDetection, "Shared Physics Detection"))
        {
            PXR_InspectorGuidance.PhysicsRequirements(
                "Detection"
            );

            PXR_InspectorGuidance.PropertyField(
                Find("affectedLayers"),
                new GUIContent("Affected Layers")
            );

            SerializedProperty capacity = Find("maxColliders");

            PXR_InspectorGuidance.PropertyField(
                capacity,
                new GUIContent("Max Colliders")
            );

            capacity.intValue = Mathf.Clamp(
                capacity.intValue, 8, 256
            );

            if (Find("affectedLayers").intValue == 0)
            {
                Warning("No affected layers selected.");
            }

            Info(
                "These settings affect radial and beam " +
                "queries, but not SpawnObject."
            );
        }

        End(showDetection);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDespawnControls()
    {
        SerializedProperty policy = Find("despawnMode");

        policy.intValue = EditorGUILayout.Popup(
            new GUIContent(
                "Despawn Mode",
                PXR_InspectorGuidance.Tooltip(
                    "PXR_UnifiedPhysics", "despawnMode"
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
                "The first collision schedules return " +
                "after the grace period. Pooled objects only; " +
                "local spawns are not affected."
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
                "No automatic return. Pooled objects consume " +
                "pool capacity until returned."
            );
        }
    }

    private SerializedProperty Find(string name)
    {
        return serializedObject.FindProperty(name);
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
}
#endif
