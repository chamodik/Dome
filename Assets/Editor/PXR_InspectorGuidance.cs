
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UdonSharpEditor;
using VRC.SDK3.Components;

public static class PXR_InspectorGuidance
{
    private static readonly Dictionary<string, string> descriptions =
        new Dictionary<string, string>();

    static PXR_InspectorGuidance()
    {
        // ------------------------------------
        // UNIFIED INPUTS
        // ------------------------------------

        Add("PXR_UnifiedInputs", "interactionType",
            "Choose which interaction activates the input.");

        Add("PXR_UnifiedInputs", "dropBehavior",
            "Pickup only: do nothing on drop, repeat Primary Events, or execute separate On Drop Events.");

        Add("PXR_UnifiedInputs", "synced",
            "Sets the default scope for ordinary events and chooses local or pooled spawning. Does not synchronize pickup movement.");

        Add("PXR_UnifiedInputs", "useDelay",
            "Legacy global delay flag, retained for scene migration.");

        Add("PXR_UnifiedInputs", "delaySeconds",
            "Legacy global delay value, retained for scene migration.");

        Add("PXR_UnifiedInputs", "eventDelaysMigrated",
            "Internal marker indicating that existing global delays were copied to event rows.");

        Add("PXR_UnifiedInputs", "destroyKeyAfterUse",
            "Consume accepted ordinary keys and disable their configured visuals and colliders.");

        Add("PXR_UnifiedInputs", "requiredKeys",
            "Number of accepted keys required before executing the output events.");

        Add("PXR_UnifiedInputs", "repeatable",
            "After reaching Required Keys, execute outputs and reset the count for another cycle.");

        Add("PXR_UnifiedInputs", "checkKeyTag",
            "Only accept keys matching Required Key Tag when enabled.");

        Add("PXR_UnifiedInputs", "requiredKeyTag",
            "Tag compared against Key.tag or PXR_PooledObject.keyTag.");

        Add("PXR_UnifiedInputs", "pooledKeyPool",
            "Pool manager responsible for pooled keys that strike this lock.");

        Add("PXR_UnifiedInputs", "keyLockState",
            "Separately synchronized lock state containing key counts and accepted identities.");

        Add("PXR_UnifiedInputs", "primaryEventTargets",
            "UdonSharp components receiving Primary Event calls.");

        Add("PXR_UnifiedInputs", "primaryEventNames",
            "Public method selected for each Primary Event.");

        Add("PXR_UnifiedInputs", "primaryEventScopes",
            "Use Input Default, Local Only, or All Players for each ordinary event.");

        Add("PXR_UnifiedInputs", "primaryEventDelays",
            "Seconds after input activation before each Primary Event executes. Zero is immediate.");

        Add("PXR_UnifiedInputs", "primaryTeleportTargets",
            "Internal references for specialized teleport routing.");

        Add("PXR_UnifiedInputs", "primaryVideoTargets",
            "Internal references for synchronized AVPro routing.");

        Add("PXR_UnifiedInputs", "dropEventTargets",
            "UdonSharp components receiving separate On Drop Events.");

        Add("PXR_UnifiedInputs", "dropEventNames",
            "Public method selected for each separate drop event.");

        Add("PXR_UnifiedInputs", "dropEventScopes",
            "Execution scope for each separate On Drop Event.");

        Add("PXR_UnifiedInputs", "dropEventDelays",
            "Seconds after dropping the pickup before each separate drop event executes.");

        Add("PXR_UnifiedInputs", "dropTeleportTargets",
            "Internal teleport references for separate drop events.");

        Add("PXR_UnifiedInputs", "dropVideoTargets",
            "Internal synchronized AVPro references for drop events.");

        Add("PXR_UnifiedInputs", "eventTarget",
            "Legacy single primary event target.");

        Add("PXR_UnifiedInputs", "eventName",
            "Legacy single primary event method name.");

        Add("PXR_UnifiedInputs", "secondEventTarget",
            "Legacy single drop event target.");

        Add("PXR_UnifiedInputs", "secondEventName",
            "Legacy single drop event method name.");

        Add("PXR_UnifiedInputs", "teleportPrimaryTarget",
            "Legacy primary teleport routing reference.");

        Add("PXR_UnifiedInputs", "teleportDropTarget",
            "Legacy drop teleport routing reference.");

        // ------------------------------------
        // UNIFIED OUTPUTS
        // ------------------------------------

        Add("PXR_UnifiedOutputs", "toggleObjectTarget",
            "GameObject controlled by ToggleObject().");

        Add("PXR_UnifiedOutputs", "objectToggle",
            "Invert the object's active state, or use Object State when disabled.");

        Add("PXR_UnifiedOutputs", "objectState",
            "Fixed active state used when object toggling is disabled.");

        Add("PXR_UnifiedOutputs", "persistObjectState",
            "Synchronize the resulting object state for late joiners.");

        Add("PXR_UnifiedOutputs", "destroyTarget",
            "GameObject deactivated by DestroyObject().");

        Add("PXR_UnifiedOutputs", "targetAnimator",
            "Animator component controlled by PlayAnimation().");

        Add("PXR_UnifiedOutputs", "animationParameterType",
            "Choose a Boolean, Trigger, or Integer Animator parameter.");

        Add("PXR_UnifiedOutputs", "animationBoolName",
            "Exact name of the Animator Boolean parameter.");

        Add("PXR_UnifiedOutputs", "animationToggleBool",
            "Invert the Boolean each time instead of setting a fixed value.");

        Add("PXR_UnifiedOutputs", "animationBoolValue",
            "Fixed Boolean value used when Toggle Bool is disabled.");

        Add("PXR_UnifiedOutputs", "animationTriggerName",
            "Exact name of the Animator Trigger parameter.");

        Add("PXR_UnifiedOutputs", "animationIntName",
            "Exact name of the Animator Integer parameter.");

        Add("PXR_UnifiedOutputs", "animationIncrement",
            "Amount added to the integer per call. Zero selects a fixed value.");

        Add("PXR_UnifiedOutputs", "animationMaxValue",
            "Maximum integer before the incrementing sequence wraps to zero.");

        Add("PXR_UnifiedOutputs", "animationFixedValue",
            "Fixed integer when Increment is zero.");

        Add("PXR_UnifiedOutputs", "persistAnimatorState",
            "Persist Animator Boolean or Integer state; Triggers are not persistent.");

        Add("PXR_UnifiedOutputs", "targetAudioSource",
            "AudioSource used by PlaySound().");

        Add("PXR_UnifiedOutputs", "audioClips",
            "Ordered playlist. If empty, use the AudioSource's existing clip.");

        Add("PXR_UnifiedOutputs", "avProUseSyncedPrefab",
            "Use the SDK synchronized AVPro player instead of direct playback.");

        Add("PXR_UnifiedOutputs", "avProSyncPlayer",
            "UdonBehaviour controlling synchronized AVPro playback.");

        Add("PXR_UnifiedOutputs", "avProPlayer",
            "Direct VRCAVProVideoPlayer used when synchronization is disabled.");

        Add("PXR_UnifiedOutputs", "avProVideoUrl",
            "Preset video URL supplied to the selected AVPro player.");

        Add("PXR_UnifiedOutputs", "teleportMode",
            "Choose initiating user, username, players within range, or everyone.");

        Add("PXR_UnifiedOutputs", "teleportUsername",
            "Exact display name for teleport-by-username mode.");

        Add("PXR_UnifiedOutputs", "teleportRange",
            "Distance from the initiating input for range-based teleportation.");

        Add("PXR_UnifiedOutputs", "teleportTargets",
            "Destination Transforms selected in sequence.");

        Add("PXR_UnifiedOutputs", "teleportTarget",
            "Legacy single teleport destination.");

        Add("PXR_UnifiedOutputs", "spawnPrefab",
            "Prefab instantiated locally when UnifiedInputs Synced is off.");

        Add("PXR_UnifiedOutputs", "spawnTransform",
            "World position and rotation used when spawning.");

        Add("PXR_UnifiedOutputs", "velocity",
            "Initial movement vector relative to Spawn Transform, in metres per second.");

        Add("PXR_UnifiedOutputs", "spawnPoolManager",
            "Pool manager used when UnifiedInputs Synced is enabled.");

        Add("PXR_UnifiedOutputs", "despawnMode",
            "Select no automatic return, return on collision, or timed return.");

        Add("PXR_UnifiedOutputs", "despawnAfterSeconds",
            "Despawn After Time: seconds before a pooled object returns or a local spawn is destroyed.");

        Add("PXR_UnifiedOutputs", "collisionReturnDelay",
            "Grace period following collision before pool return.");

        Add("PXR_UnifiedOutputs", "componentTargetType",
            "Choose a Behaviour, Renderer, or Collider for ToggleComponent().");

        Add("PXR_UnifiedOutputs", "targetBehaviour",
            "Behaviour to enable or disable.");

        Add("PXR_UnifiedOutputs", "targetRenderer",
            "Renderer to enable or disable.");

        Add("PXR_UnifiedOutputs", "targetCollider",
            "Collider to enable or disable.");

        Add("PXR_UnifiedOutputs", "componentToggle",
            "Invert the component state, or apply a fixed state.");

        Add("PXR_UnifiedOutputs", "componentEnabled",
            "Fixed component state when Toggle is disabled.");

        Add("PXR_UnifiedOutputs", "persistComponentState",
            "Synchronize the component's resulting enabled state.");

        Add("PXR_UnifiedOutputs", "targetObject",
            "Legacy target-object reference.");

        Add("PXR_UnifiedOutputs", "enableObjectTarget",
            "Legacy object activation target.");

        Add("PXR_UnifiedOutputs", "disableObjectTarget",
            "Legacy object deactivation target.");

        Add("PXR_UnifiedOutputs", "destroyObjectTarget",
            "Legacy object deactivation reference.");

        Add("PXR_UnifiedOutputs", "targetComponent",
            "Legacy component reference.");

        // ------------------------------------
        // UNIFIED PHYSICS
        // ------------------------------------

        Add("PXR_UnifiedPhysics", "radialRange",
            "Radius in metres for radial force detection.");

        Add("PXR_UnifiedPhysics", "radialForce",
            "Strength of radial impulse or continuous force.");

        Add("PXR_UnifiedPhysics", "radialAttract",
            "Pull objects toward the emitter rather than pushing away.");

        Add("PXR_UnifiedPhysics", "radialContinuous",
            "Continue applying radial force until stopped.");

        Add("PXR_UnifiedPhysics", "persistRadialState",
            "Synchronize the active continuous radial-force state.");

        Add("PXR_UnifiedPhysics", "beamRange",
            "Length of the oriented beam volume in metres.");

        Add("PXR_UnifiedPhysics", "beamForce",
            "Strength applied to eligible Rigidbodies inside the beam.");

        Add("PXR_UnifiedPhysics", "beamAttract",
            "Apply force toward the beam origin.");

        Add("PXR_UnifiedPhysics", "beamDirection",
            "Local direction of the beam. (0,0,1) is forward.");

        Add("PXR_UnifiedPhysics", "beamRadius",
            "Half-width of the oriented beam volume.");

        Add("PXR_UnifiedPhysics", "beamContinuous",
            "Continue applying beam force until stopped.");

        Add("PXR_UnifiedPhysics", "persistBeamState",
            "Synchronize the active continuous beam-force state.");

        Add("PXR_UnifiedPhysics", "spawnPrefab",
            "Prefab used for local Rigidbody spawning.");

        Add("PXR_UnifiedPhysics", "spawnTransform",
            "Position and orientation used for spawning.");

        Add("PXR_UnifiedPhysics", "velocity",
            "Initial Rigidbody velocity in Spawn Transform local axes.");

        Add("PXR_UnifiedPhysics", "spawnPoolManager",
            "Manager providing synchronized pooled physics spawning.");

        Add("PXR_UnifiedPhysics", "despawnMode",
            "How a pooled Rigidbody returns to the pool.");

        Add("PXR_UnifiedPhysics", "despawnAfterSeconds",
            "Despawn After Time: lifetime before pool return or local-spawn destruction.");

        Add("PXR_UnifiedPhysics", "collisionReturnDelay",
            "Delay after collision before pool return.");

        Add("PXR_UnifiedPhysics", "affectedLayers",
            "Physics layers included in radial and beam overlap queries.");

        Add("PXR_UnifiedPhysics", "maxColliders",
            "Maximum colliders processed by each overlap query.");

        // ------------------------------------
        // CREATURE
        // ------------------------------------

        Add("PXR_UnifiedCreature", "navAgent",
            "Owner-driven NavMeshAgent. Requires a baked NavMesh.");

        Add("PXR_UnifiedCreature", "animator",
            "Optional Animator for displaying creature movement.");

        Add("PXR_UnifiedCreature", "walkingBoolName",
            "Animator Boolean parameter used for walking.");

        Add("PXR_UnifiedCreature", "playOnStart",
            "Start the sequence automatically.");

        Add("PXR_UnifiedCreature", "loop",
            "Restart from the first step when the sequence finishes.");

        Add("PXR_UnifiedCreature", "stepTypes",
            "Sequence of Stand and Move steps.");

        Add("PXR_UnifiedCreature", "standDurations",
            "Time spent stationary in each Stand step.");

        Add("PXR_UnifiedCreature", "moveTargets",
            "Destination Transform for each Move step.");

        Add("PXR_UnifiedCreature", "moveSpeeds",
            "Movement speed for each Move step.");

        Add("PXR_UnifiedCreature", "arrivalDistance",
            "Distance at which a Move step is considered complete.");

        Add("PXR_UnifiedCreature", "maxMoveSeconds",
            "Maximum time to spend attempting a Move step.");

        Add("PXR_UnifiedCreature", "debugLogs",
            "Show movement and ownership diagnostics.");

        // ------------------------------------
        // MENU
        // ------------------------------------

        Add("PXR_UnifiedMenu", "adminOnly",
            "Allow only users included in the administrator list.");

        Add("PXR_UnifiedMenu", "adminList",
            "PXR_AdminNames component listing administrators.");

        Add("PXR_UnifiedMenu", "rightHand",
            "Attach to the right hand when checked, otherwise the left.");

        Add("PXR_UnifiedMenu", "offset",
            "Position offset relative to the selected hand.");

        Add("PXR_UnifiedMenu", "rotationOffset",
            "Additional panel rotation in degrees.");

        Add("PXR_UnifiedMenu", "facePlayer",
            "Orient toward the player rather than following hand yaw.");

        Add("PXR_UnifiedMenu", "handYawOffset",
            "Additional controller yaw calibration.");

        Add("PXR_UnifiedMenu", "menuContent",
            "Child GameObject containing the world-space menu.");

        Add("PXR_UnifiedMenu", "leftThumbstickButton",
            "Input binding for left thumbstick press.");

        Add("PXR_UnifiedMenu", "rightThumbstickButton",
            "Input binding for right thumbstick press.");

        Add("PXR_UnifiedMenu", "enableDesktopFallback",
            "Allow a keyboard shortcut in desktop mode.");

        Add("PXR_UnifiedMenu", "desktopToggleKey",
            "Keyboard shortcut for opening or closing the menu.");

        Add("PXR_UnifiedMenu", "desktopOffset",
            "Desktop menu position relative to the player's head.");

        // ------------------------------------
        // POOL, KEY AND ADMIN
        // ------------------------------------

        Add("PXR_PoolManager", "objectPool",
            "VRCObjectPool controlling shared object allocation.");

        Add("PXR_PoolManager", "keyLocks",
            "Registered key-lock states receiving pooled-key collisions.");

        Add("PXR_PoolManager", "ownershipTimeout",
            "Maximum time to wait for pool-object ownership.");

        Add("PXR_PoolManager", "keyAcknowledgementTimeout",
            "Maximum wait for a lock acknowledgment.");

        Add("PXR_PoolManager", "keyRetryInterval",
            "Time between retries of a pending pooled-key report.");

        Add("PXR_PoolManager", "debugLogs",
            "Log pool ownership and lifecycle diagnostics.");

        Add("PXR_PooledObject", "poolManager",
            "Manager controlling this pooled object's lifecycle.");

        Add("PXR_PooledObject", "actsAsKey",
            "Register impacts with compatible key locks.");

        Add("PXR_PooledObject", "keyTag",
            "Tag compared with the lock's Required Key Tag.");

        Add("PXR_PooledObject", "debugLogs",
            "Log pooled-object collisions and lifecycle events.");

        Add("Key", "tag",
            "Custom key identifier compared against Required Key Tag.");

        Add("Key", "pickupReference",
            "VRC Pickup on the physical key. Used to identify the holder or post-release owner.");

        Add("Key", "visualRoot",
            "Optional child GameObject containing the key visuals. Hidden when the key is consumed.");

        Add("Key", "collisionColliders",
            "Colliders disabled when the key is consumed.");

        Add("PXR_KeyLockState", "input",
            "PXR_UnifiedInputs configured as this Key lock.");

        Add("PXR_KeyLockState", "keyCount",
            "Number of accepted keys in the current counting cycle.");

        Add("PXR_KeyLockState", "keysComplete",
            "Whether the lock has completed its required count.");

        Add("PXR_AdminNames", "adminNames",
            "VRChat display names permitted to use admin features.");

        Add("PXR_AdminLock", "adminList",
            "Administrator list used to determine local access.");

        Add("PXR_AdminLock", "protectedObjects",
            "Objects hidden from non-administrators.");

        Add("PXR_AdminLock", "protectedContent",
            "Legacy protected object reference.");
    }

    private static void Add(
        string script,
        string field,
        string description)
    {
        descriptions[script + "." + field] = description;
    }

    public static string Tooltip(string script, string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        string value;

        if (descriptions.TryGetValue(
            script + "." + field, out value))
        {
            return value;
        }

        if (descriptions.TryGetValue(
            "PXR_UnifiedOutputs." + field, out value))
        {
            return value;
        }

        if (descriptions.TryGetValue(
            "PXR_UnifiedPhysics." + field, out value))
        {
            return value;
        }

        return "Configure " +
            ObjectNames.NicifyVariableName(field) +
            " for this toolkit component.";
    }

    private static GUIContent Label(
        SerializedProperty property,
        GUIContent label)
    {
        string text = label != null
            ? label.text
            : property.displayName;

        string field = property.name;

        // Unity's array elements use "data" instead of
        // the serialized parent field name.
        if (field == "data")
        {
            string path = property.propertyPath;
            int dot = path.IndexOf('.');

            if (dot > 0)
                field = path.Substring(0, dot);
        }

        string tooltip = Tooltip(
            property.serializedObject.targetObject
                .GetType().Name,
            field
        );

        if (label != null &&
            !string.IsNullOrEmpty(label.tooltip))
        {
            tooltip += "\n" + label.tooltip;
        }

        return new GUIContent(text, tooltip);
    }

    public static bool PropertyField(
        SerializedProperty property)
    {
        return EditorGUILayout.PropertyField(
            property,
            Label(property, null),
            true
        );
    }

    public static bool PropertyField(
        SerializedProperty property,
        bool children)
    {
        return EditorGUILayout.PropertyField(
            property,
            Label(property, null),
            children
        );
    }

    public static bool PropertyField(
        SerializedProperty property,
        GUIContent label)
    {
        return EditorGUILayout.PropertyField(
            property,
            Label(property, label),
            true
        );
    }

    public static bool PropertyField(
        SerializedProperty property,
        GUIContent label,
        bool children)
    {
        return EditorGUILayout.PropertyField(
            property,
            Label(property, label),
            children
        );
    }

    private static void Guide(
        string title,
        string required,
        string optional,
        string note)
    {
        EditorGUILayout.Space(3);

        EditorGUILayout.LabelField(
            "Component requirements: " + title,
            EditorStyles.miniBoldLabel
        );

        EditorGUILayout.HelpBox(
            "Required\n" + required +
            (string.IsNullOrEmpty(optional)
                ? ""
                : "\n\nOptional\n" + optional) +
            (string.IsNullOrEmpty(note)
                ? ""
                : "\n\nNote\n" + note),
            MessageType.Info
        );
    }

    public static void InputRequirements(
        PXR_InputType type,
        GameObject source)
    {
        switch (type)
        {
            case PXR_InputType.Interact:
                Guide(
                    "Interact",
                    "Collider on input GameObject; PXR_UnifiedInputs.",
                    "Output UdonSharpBehaviour on selected target.",
                    "VRChat interaction prompt is enabled."
                );

                if (source.GetComponent<Collider>() == null)
                {
                    EditorGUILayout.HelpBox(
                        "Missing Collider on this object.",
                        MessageType.Warning
                    );
                }
                break;

            case PXR_InputType.Pickup:
            case PXR_InputType.PickupUse:
                Guide(
                    type == PXR_InputType.Pickup
                        ? "Pickup"
                        : "Pickup Use",
                    "Rigidbody + Collider + VRC Pickup + PXR_UnifiedInputs.",
                    "VRCObjectSync on the same GameObject for networked pickup movement.",
                    type == PXR_InputType.Pickup
                        ? "Pickup runs on grab; On Drop is configurable."
                        : "Pickup Use runs when the holder presses Use."
                );

                if (source.GetComponent<Collider>() == null ||
                    source.GetComponent<Rigidbody>() == null ||
                    source.GetComponent<VRCPickup>() == null)
                {
                    EditorGUILayout.HelpBox(
                        "Pickup is missing Collider, Rigidbody, " +
                        "or VRC Pickup.",
                        MessageType.Warning
                    );
                }

                if (source.GetComponent<VRCObjectSync>() == null)
                {
                    EditorGUILayout.HelpBox(
                        "No VRCObjectSync: remote users may not " +
                        "see pickup movement.",
                        MessageType.Info
                    );
                }
                break;

            case PXR_InputType.PlayerTriggerEnter:
            case PXR_InputType.PlayerTriggerExit:
                Guide(
                    type == PXR_InputType.PlayerTriggerEnter
                        ? "Player Trigger Enter"
                        : "Player Trigger Exit",
                    "Trigger Collider and PXR_UnifiedInputs.",
                    "Rigidbody if required by the collision setup.",
                    "Only local-player entry or exit dispatches the input."
                );

                Collider collider = source.GetComponent<Collider>();

                if (collider == null || !collider.isTrigger)
                {
                    EditorGUILayout.HelpBox(
                        "Requires a Collider with Is Trigger enabled.",
                        MessageType.Warning
                    );
                }
                break;

            case PXR_InputType.Key:
                Guide(
                    "Key Lock",
                    "Collider, PXR_UnifiedInputs, and PXR_KeyLockState (Manual).",
                    "Ordinary keys: Key, Collider and Rigidbody. " +
                    "Pooled keys: PXR_PoolManager, PXR_PooledObject, " +
                    "VRCObjectPool and VRCObjectSync.",
                    "Register the lock state in Pool Manager > Key Locks. " +
                    "Keep Manual lock state separate from VRCObjectSync."
                );

                if (source.GetComponent<Collider>() == null)
                {
                    EditorGUILayout.HelpBox(
                        "Missing lock Collider.",
                        MessageType.Warning
                    );
                }
                break;

            case PXR_InputType.UIButton:
                Guide(
                    "UI Button",
                    "World Space Canvas, Graphic Raycaster, " +
                    "VRC UI Shape, EventSystem and Unity Button.",
                    "BoxCollider covering the Canvas.",
                    "Canvas must use the Default layer. " +
                    "Configure Button OnClick to invoke UIButtonPress."
                );
                break;
        }
    }

    public static void OutputRequirements(string action)
    {
        switch (action)
        {
            case "ToggleObject":
                Guide(
                    "Toggle Object",
                    "Target GameObject reference.",
                    "Persist state for late joiners.",
                    "Avoid deactivating the controller's GameObject."
                );
                break;

            case "DestroyObject":
                Guide(
                    "Destroy Object",
                    "Target GameObject reference.",
                    "",
                    "SetActive(false), not permanent networked destruction."
                );
                break;

            case "PlayAnimation":
                Guide(
                    "Play Animation",
                    "Animator, Animator Controller, matching parameter.",
                    "Persist Bool/Int state when required.",
                    "Animator parameter names and types must match."
                );
                break;

            case "PlaySound":
                Guide(
                    "Play Sound",
                    "AudioSource with an assigned clip or playlist.",
                    "VRC Spatial Audio Source for spatial audio.",
                    "Playlist advances by one valid clip per call."
                );
                break;

            case "PlayAVProVideo":
                Guide(
                    "AVPro Video",
                    "SDK synchronized AVPro UdonBehaviour or " +
                    "VRCAVProVideoPlayer and valid video URL.",
                    "Video display renderer and audio output.",
                    "Direct playback is not a synchronized " +
                    "late-joiner playback system."
                );
                break;

            case "TeleportPlayer":
                Guide(
                    "Teleport Player",
                    "Destination Transform and PXR_UnifiedInputs " +
                    "for initiating-user attribution.",
                    "Username or range, depending on mode.",
                    "TeleportUser requires a known initiating player."
                );
                break;

            case "SpawnObject":
                Guide(
                    "Spawn Object",
                    "Spawn Transform and local prefab (Synced OFF), " +
                    "or configured Pool Manager (Synced ON).",
                    "Network pool needs VRCObjectPool, " +
                    "PXR_PooledObject and VRCObjectSync.",
                    "UnifiedInputs Synced determines the spawn route."
                );
                break;

            case "ToggleComponent":
                Guide(
                    "Toggle Component",
                    "Assigned Behaviour, Renderer or Collider.",
                    "Persist enabled state for late joiners.",
                    "Component Category must match the reference."
                );
                break;
        }
    }

    public static void PhysicsRequirements(string action)
    {
        switch (action)
        {
            case "RadialForce":
            case "BeamForce":
                Guide(
                    action == "RadialForce"
                        ? "Radial Force"
                        : "Beam Force",
                    "PXR_UnifiedPhysics and target non-trigger " +
                    "Colliders with non-kinematic Rigidbodies.",
                    "VRCObjectSync on affected objects for " +
                    "networked physics.",
                    "Only the Rigidbody owner applies force. " +
                    "Affected Layers limits eligible targets."
                );
                break;

            case "SpawnObject":
                Guide(
                    "Physics Spawn",
                    "Spawn Transform and Rigidbody prefab " +
                    "(Synced OFF), or PXR_PoolManager (Synced ON).",
                    "Networked pool: VRCObjectPool, PXR_PooledObject, " +
                    "VRCObjectSync and Rigidbody.",
                    "Velocity is measured in Spawn Transform local axes."
                );
                break;

            case "Detection":
                Guide(
                    "Physics Detection",
                    "Colliders on target objects in included layers.",
                    "Increase Max Colliders in dense scenes.",
                    "Only returned overlap results receive force."
                );
                break;
        }
    }
}

// Generic documented inspectors for toolkit components.
public class PXR_DocumentedInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        serializedObject.Update();

        SerializedProperty property =
            serializedObject.GetIterator();

        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;

            if (property.name == "m_Script")
                continue;

            PXR_InspectorGuidance.PropertyField(property, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(Key))]
public class PXR_KeyDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_KeyLockState))]
public class PXR_KeyLockDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_PoolManager))]
public class PXR_PoolManagerDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_PooledObject))]
public class PXR_PooledObjectDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_UnifiedMenu))]
public class PXR_MenuDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_AdminNames))]
public class PXR_AdminNamesDocumentedInspector : PXR_DocumentedInspector { }

[CustomEditor(typeof(PXR_AdminLock))]
public class PXR_AdminLockDocumentedInspector : PXR_DocumentedInspector { }

#endif
