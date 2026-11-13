<<<<<<< Updated upstream
#if UNITY_EDITOR
=======

#if UNITY_EDITOR

>>>>>>> Stashed changes
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
<<<<<<< Updated upstream
using UdonSharp;
using UdonSharpEditor;
using VRC.SDK3.Components;

// Editor-only documentation. No changes to Udon runtime scripts.
=======
using UdonSharpEditor;
using VRC.SDK3.Components;

>>>>>>> Stashed changes
public static class PXR_InspectorGuidance
{
    private static readonly Dictionary<string, string> descriptions =
        new Dictionary<string, string>();

    static PXR_InspectorGuidance()
    {
<<<<<<< Updated upstream
        Add("PXR_UnifiedInputs", "interactionType", "Select which action causes this input to dispatch its configured output events.");
        Add("PXR_UnifiedInputs", "dropBehavior", "For Pickup inputs only: choose whether dropping does nothing, repeats the primary events, or fires separate drop events.");
        Add("PXR_UnifiedInputs", "synced", "Determines the default scope for ordinary output events and whether SpawnObject uses a networked pool. This does not synchronize pickup movement.");
        Add("PXR_UnifiedInputs", "useDelay", "Wait before executing output events. Physical key registration itself is immediate.");
        Add("PXR_UnifiedInputs", "delaySeconds", "Seconds to wait before performing the configured event; only used when Use Delay is enabled.");
        Add("PXR_UnifiedInputs", "destroyKeyAfterUse", "If enabled, each accepted ordinary Key is consumed, disabling configured key visuals/colliders. If disabled, it remains intact.");
        Add("PXR_UnifiedInputs", "requiredKeys", "Number of distinct accepted keys required before the lock dispatches its configured outputs.");
        Add("PXR_UnifiedInputs", "checkKeyTag", "If enabled, only Key objects whose tag matches Required Key Tag are accepted.");
        Add("PXR_UnifiedInputs", "requiredKeyTag", "Custom tag string to compare against Key.tag or pooled PXR_PooledObject.keyTag.");
        Add("PXR_UnifiedInputs", "pooledKeyPool", "Manager that owns the VRCObjectPool used for pooled keys striking this lock.");
        Add("PXR_UnifiedInputs", "keyLockState", "Required for Key mode. Separate manually synchronized component that stores counts, duplicate identities, and lock completion.");
        Add("PXR_UnifiedInputs", "primaryEventTargets", "UdonSharp behaviours receiving the primary event calls.");
        Add("PXR_UnifiedInputs", "primaryEventNames", "Method names selected for each primary event target.");
        Add("PXR_UnifiedInputs", "primaryEventScopes", "Per-event override: Input Default, Local Only, or All Players.");
        Add("PXR_UnifiedInputs", "primaryTeleportTargets", "Internal references for special teleport routing; maintained by the custom Inspector.");
        Add("PXR_UnifiedInputs", "primaryVideoTargets", "Internal references for synchronized AVPro routing; maintained by the custom Inspector.");
        Add("PXR_UnifiedInputs", "dropEventTargets", "Event receivers used when Pickup > Separate Function is selected.");
        Add("PXR_UnifiedInputs", "dropEventNames", "Methods invoked for the separate drop event.");
        Add("PXR_UnifiedInputs", "dropEventScopes", "Per-event execution scope for a separate drop event.");
        Add("PXR_UnifiedInputs", "dropTeleportTargets", "Internal references for drop-event teleport routing.");
        Add("PXR_UnifiedInputs", "dropVideoTargets", "Internal references for drop-event synchronized AVPro routing.");
        Add("PXR_UnifiedInputs", "eventTarget", "Legacy single primary event receiver retained for older scene configurations.");
        Add("PXR_UnifiedInputs", "eventName", "Legacy single primary event method name.");
        Add("PXR_UnifiedInputs", "secondEventTarget", "Legacy single drop event receiver.");
        Add("PXR_UnifiedInputs", "secondEventName", "Legacy single drop event method name.");
        Add("PXR_UnifiedInputs", "teleportPrimaryTarget", "Legacy special teleport target for primary events.");
        Add("PXR_UnifiedInputs", "teleportDropTarget", "Legacy special teleport target for drop events.");
        Add("PXR_UnifiedOutputs", "toggleObjectTarget", "GameObject whose active state will be changed by ToggleObject().");
        Add("PXR_UnifiedOutputs", "objectToggle", "When true, invert the target active state each time; when false, use Object State.");
        Add("PXR_UnifiedOutputs", "objectState", "Fixed target active state to apply when Toggle Object is disabled.");
        Add("PXR_UnifiedOutputs", "persistObjectState", "Serialize the active state so late joiners receive it; requires network ownership.");
        Add("PXR_UnifiedOutputs", "destroyTarget", "GameObject to deactivate using DestroyObject(). Does not permanently destroy a scene object.");
        Add("PXR_UnifiedOutputs", "targetAnimator", "Animator component whose selected parameter is controlled by PlayAnimation().");
        Add("PXR_UnifiedOutputs", "animationParameterType", "Which Animator parameter type PlayAnimation() will modify: Bool, Trigger, or Int.");
        Add("PXR_UnifiedOutputs", "animationBoolName", "Exact name of the Animator Boolean parameter.");
        Add("PXR_UnifiedOutputs", "animationToggleBool", "Invert the current Boolean state instead of setting a fixed value.");
        Add("PXR_UnifiedOutputs", "animationBoolValue", "Fixed Boolean value used when Toggle Bool is disabled.");
        Add("PXR_UnifiedOutputs", "animationTriggerName", "Exact name of the Animator Trigger parameter to fire.");
        Add("PXR_UnifiedOutputs", "animationIntName", "Exact name of the Animator Integer parameter.");
        Add("PXR_UnifiedOutputs", "animationIncrement", "Amount added to the integer each call. Use zero to choose a fixed value instead.");
        Add("PXR_UnifiedOutputs", "animationMaxValue", "Maximum integer state before the incrementing sequence wraps to zero.");
        Add("PXR_UnifiedOutputs", "animationFixedValue", "Integer value to set when Increment is zero.");
        Add("PXR_UnifiedOutputs", "persistAnimatorState", "Persist Boolean or Integer Animator state for late joiners; Triggers are not persistent.");
        Add("PXR_UnifiedOutputs", "targetAudioSource", "AudioSource that will play clips when PlaySound() is invoked.");
        Add("PXR_UnifiedOutputs", "audioClips", "Ordered playlist. Calls advance through clips; when empty, play the AudioSource assigned clip.");
        Add("PXR_UnifiedOutputs", "avProUseSyncedPrefab", "Use the SDK-provided UdonSyncPlayer AVPro implementation instead of a direct video player.");
        Add("PXR_UnifiedOutputs", "avProSyncPlayer", "UdonBehaviour controlling the SDK synchronized AVPro player.");
        Add("PXR_UnifiedOutputs", "avProPlayer", "VRCAVProVideoPlayer to control directly when Use SDK Synced Player is off.");
        Add("PXR_UnifiedOutputs", "avProVideoUrl", "Video URL authored in Unity and passed to the selected AVPro player.");
        Add("PXR_UnifiedOutputs", "teleportMode", "Choose whether TeleportPlayer targets the initiating user, a username, players within range, or everyone.");
        Add("PXR_UnifiedOutputs", "teleportUsername", "Exact display name used by Teleport By Username mode.");
        Add("PXR_UnifiedOutputs", "teleportRange", "Distance from the initiating input used to select players in Teleport By Range mode (metres).");
        Add("PXR_UnifiedOutputs", "teleportTargets", "List of Transform destinations selected in order when teleporting players.");
        Add("PXR_UnifiedOutputs", "teleportTarget", "Legacy single teleport destination, retained for old scenes.");
        Add("PXR_UnifiedOutputs", "spawnPrefab", "Prefab instantiated locally when UnifiedInputs Synced is off.");
        Add("PXR_UnifiedOutputs", "spawnTransform", "World position and rotation used to place the new object.");
        Add("PXR_UnifiedOutputs", "velocity", "Initial direction and speed relative to Spawn Transform. Non-physics movement uses Transform updates.");
        Add("PXR_UnifiedOutputs", "spawnPoolManager", "PXR_PoolManager responsible for pooled networked spawning when UnifiedInputs Synced is on.");
        Add("PXR_UnifiedOutputs", "despawnMode", "Choose no automatic return, return after collision, or return after time for pooled objects.");
        Add("PXR_UnifiedOutputs", "despawnAfterSeconds", "Seconds before a pooled object is returned when Despawn After Time is selected.");
        Add("PXR_UnifiedOutputs", "collisionReturnDelay", "Grace period in seconds before returning a pooled object after collision.");
        Add("PXR_UnifiedOutputs", "componentTargetType", "Component class affected by ToggleComponent(): Behaviour, Renderer, or Collider.");
        Add("PXR_UnifiedOutputs", "targetBehaviour", "Behaviour component to enable or disable when Component Target Type is Behaviour.");
        Add("PXR_UnifiedOutputs", "targetRenderer", "Renderer component to enable or disable when Component Target Type is Renderer.");
        Add("PXR_UnifiedOutputs", "targetCollider", "Collider component to enable or disable when Component Target Type is Collider.");
        Add("PXR_UnifiedOutputs", "componentToggle", "Invert enabled state each call; otherwise apply the fixed Enabled value.");
        Add("PXR_UnifiedOutputs", "componentEnabled", "Fixed enabled state to apply when Toggle is disabled.");
        Add("PXR_UnifiedOutputs", "persistComponentState", "Synchronize the resulting enabled state for late joiners.");
        Add("PXR_UnifiedOutputs", "targetObject", "Legacy target-object reference retained for older serialized configurations.");
        Add("PXR_UnifiedOutputs", "enableObjectTarget", "Legacy GameObject activation target; preserved for compatibility.");
        Add("PXR_UnifiedOutputs", "disableObjectTarget", "Legacy GameObject deactivation target; preserved for compatibility.");
        Add("PXR_UnifiedOutputs", "destroyObjectTarget", "Legacy deactivate target retained for older serialized configurations.");
        Add("PXR_UnifiedOutputs", "targetComponent", "Legacy component reference; use Target Behaviour, Renderer or Collider instead.");
        Add("PXR_UnifiedOutputs", "enableChainEvent", "Allow this output to invoke a second UdonSharp method after its main action.");
        Add("PXR_UnifiedOutputs", "chainEventTarget", "Target UdonSharpBehaviour to receive the chained event.");
        Add("PXR_UnifiedOutputs", "chainEventName", "Parameterless public method invoked on the Chain Event Target.");
        Add("PXR_UnifiedOutputs", "chainEventDelay", "Seconds before calling the chained event.");
        Add("PXR_UnifiedPhysics", "radialRange", "Radius in metres in which eligible rigidbodies receive radial force.");
        Add("PXR_UnifiedPhysics", "radialForce", "Strength of radial force or impulse applied to each eligible Rigidbody.");
        Add("PXR_UnifiedPhysics", "radialAttract", "Pull objects toward the emitter instead of pushing them away.");
        Add("PXR_UnifiedPhysics", "radialContinuous", "Apply the force continuously until stopped; when off, apply one impulse per activation.");
        Add("PXR_UnifiedPhysics", "persistRadialState", "Synchronize whether continuous radial force is active for late joiners.");
        Add("PXR_UnifiedPhysics", "beamRange", "Length in metres of the beam force volume.");
        Add("PXR_UnifiedPhysics", "beamForce", "Strength of the force applied to eligible rigidbodies in the beam.");
        Add("PXR_UnifiedPhysics", "beamAttract", "Pull objects toward the beam origin instead of pushing away.");
        Add("PXR_UnifiedPhysics", "beamDirection", "Local-space beam direction; (0,0,1) follows Transform.forward.");
        Add("PXR_UnifiedPhysics", "beamRadius", "Half-width in metres of the beam box volume.");
        Add("PXR_UnifiedPhysics", "beamContinuous", "Apply the beam force continuously until stopped.");
        Add("PXR_UnifiedPhysics", "persistBeamState", "Persist active continuous beam state across clients and late joiners.");
        Add("PXR_UnifiedPhysics", "spawnPrefab", "Prefab used for local physics spawning when input Synced is off.");
        Add("PXR_UnifiedPhysics", "spawnTransform", "Location and orientation where objects are spawned.");
        Add("PXR_UnifiedPhysics", "velocity", "Initial Rigidbody velocity relative to Spawn Transform (metres per second).");
        Add("PXR_UnifiedPhysics", "spawnPoolManager", "Manager and VRCObjectPool used for synchronized physics spawning.");
        Add("PXR_UnifiedPhysics", "despawnMode", "How a pooled physics object returns to the pool.");
        Add("PXR_UnifiedPhysics", "despawnAfterSeconds", "Lifetime in seconds for timed pool return.");
        Add("PXR_UnifiedPhysics", "collisionReturnDelay", "Delay after collision before returning to the pool.");
        Add("PXR_UnifiedPhysics", "affectedLayers", "Physics layers eligible for radial and beam overlap tests.");
        Add("PXR_UnifiedPhysics", "maxColliders", "Maximum collider results processed for each physics query; increase for dense scenes.");
        Add("PXR_UnifiedPhysics", "enableChainEvent", "Call another UdonSharp behavior after the selected physics action.");
        Add("PXR_UnifiedPhysics", "chainEventTarget", "UdonSharpBehaviour receiving the chained event.");
        Add("PXR_UnifiedPhysics", "chainEventName", "Name of the parameterless chained event method.");
        Add("PXR_UnifiedPhysics", "chainEventDelay", "Delay in seconds before invoking the chained event.");
        Add("PXR_UnifiedCreature", "navAgent", "NavMeshAgent that moves the creature on the network owner. Requires a baked NavMesh.");
        Add("PXR_UnifiedCreature", "animator", "Optional Animator for the creature walking state.");
        Add("PXR_UnifiedCreature", "walkingBoolName", "Name of the Animator bool parameter set while walking.");
        Add("PXR_UnifiedCreature", "playOnStart", "Start the configured creature sequence automatically.");
        Add("PXR_UnifiedCreature", "loop", "Restart from the first step when the sequence finishes.");
        Add("PXR_UnifiedCreature", "stepTypes", "Ordered sequence of Stand and Move steps.");
        Add("PXR_UnifiedCreature", "standDurations", "Duration in seconds to remain still during each Stand step.");
        Add("PXR_UnifiedCreature", "moveTargets", "Target Transform for each Move step.");
        Add("PXR_UnifiedCreature", "moveSpeeds", "Movement speed for each Move step.");
        Add("PXR_UnifiedCreature", "arrivalDistance", "Distance threshold in metres for considering a Move step finished.");
        Add("PXR_UnifiedCreature", "maxMoveSeconds", "Maximum time to attempt a Move step before advancing.");
        Add("PXR_UnifiedCreature", "debugLogs", "Log movement and ownership diagnostics to the Unity/VRChat console.");
        Add("PXR_UnifiedMenu", "adminOnly", "Restrict opening this menu to usernames in Admin List.");
        Add("PXR_UnifiedMenu", "adminList", "PXR_AdminNames component containing permitted administrator display names.");
        Add("PXR_UnifiedMenu", "rightHand", "Anchor the menu to the right hand when checked, otherwise to the left hand.");
        Add("PXR_UnifiedMenu", "offset", "Position offset relative to the selected hand in metres; positive Z extends the panel outward.");
        Add("PXR_UnifiedMenu", "rotationOffset", "Additional local rotation in degrees for a readable book-like panel.");
        Add("PXR_UnifiedMenu", "facePlayer", "When enabled, orient the menu toward the player. When disabled, follow the hand’s horizontal direction.");
        Add("PXR_UnifiedMenu", "handYawOffset", "Additional yaw in degrees to calibrate the menu’s orientation relative to the controller.");
        Add("PXR_UnifiedMenu", "menuContent", "Child GameObject containing the world-space menu Canvas; hidden when menu is closed.");
        Add("PXR_UnifiedMenu", "leftThumbstickButton", "Input Manager binding used for left-controller thumbstick press.");
        Add("PXR_UnifiedMenu", "rightThumbstickButton", "Input Manager binding used for right-controller thumbstick press.");
        Add("PXR_UnifiedMenu", "enableDesktopFallback", "Allow desktop users to open or close the menu with Desktop Toggle Key.");
        Add("PXR_UnifiedMenu", "desktopToggleKey", "Keyboard key used to toggle the menu in desktop mode.");
        Add("PXR_UnifiedMenu", "desktopOffset", "Position of the desktop menu relative to the player’s head in metres.");
        Add("PXR_PoolManager", "objectPool", "VRCObjectPool on the same GameObject; owns networked pooled spawn/return.");
        Add("PXR_PoolManager", "keyLocks", "Registered PXR_KeyLockState components receiving pooled key impacts.");
        Add("PXR_PoolManager", "ownershipTimeout", "Seconds to wait while claiming ownership of a pooled object.");
        Add("PXR_PoolManager", "keyAcknowledgementTimeout", "Maximum wait for a key lock acknowledgment before giving up.");
        Add("PXR_PoolManager", "keyRetryInterval", "Time in seconds between retries of a pending pooled key report.");
        Add("PXR_PoolManager", "debugLogs", "Log pool lifecycle and registration diagnostics.");
        Add("PXR_PooledObject", "poolManager", "PXR_PoolManager that owns this pooled object’s lifecycle.");
        Add("PXR_PooledObject", "actsAsKey", "Treat pooled collisions with registered locks as key registration attempts.");
        Add("PXR_PooledObject", "keyTag", "Custom tag matched by a lock when this pooled object acts as a key.");
        Add("PXR_PooledObject", "debugLogs", "Log object arming, collisions, ownership, and disarming.");
        Add("Key", "tag", "Custom key identifier compared with a lock’s Required Key Tag when Check Key Tag is enabled.");
        Add("Key", "pickupReference", "VRC Pickup on the physical key; used to attribute key completion to the holder or post-release owner. Assign explicitly if Key is on a child.");
        Add("Key", "visualRoot", "Optional child GameObject that contains only key visuals; hidden when the key is consumed.");
        Add("Key", "collisionColliders", "List of colliders to disable when the key is consumed. Include the physical root collider if it is separate.");
        Add("PXR_KeyLockState", "input", "PXR_UnifiedInputs configured as a Key lock; supplies requirements and receives completion events.");
        Add("PXR_KeyLockState", "keyCount", "Number of distinct keys accepted since the last reset; synchronized by the lock owner.");
        Add("PXR_KeyLockState", "keysComplete", "True after the required count has been met; synchronized to late joiners.");
        Add("PXR_AdminNames", "adminNames", "Exact VRChat display names allowed to use administrator-only toolkit features.");
        Add("PXR_AdminLock", "adminList", "PXR_AdminNames component used to determine which local users see protected content.");
        Add("PXR_AdminLock", "protectedObjects", "GameObjects whose visibility/active state is controlled by administrator access.");
        Add("PXR_AdminLock", "protectedContent", "Parent GameObject containing admin-only elements.");
    }

    private static void Add(string script, string field, string description)
=======
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
>>>>>>> Stashed changes
    {
        descriptions[script + "." + field] = description;
    }

    public static string Tooltip(string script, string field)
    {
<<<<<<< Updated upstream
        if (string.IsNullOrEmpty(field)) return "";
        string value;
        if (descriptions.TryGetValue(script + "." + field, out value))
            return value;
        if (descriptions.TryGetValue("PXR_UnifiedOutputs." + field, out value))
            return value;
        if (descriptions.TryGetValue("PXR_UnifiedPhysics." + field, out value))
            return value;
        return "Configure " + ObjectNames.NicifyVariableName(field) +
            " for this toolkit component.";
    }

    private static GUIContent Label(SerializedProperty property, GUIContent label)
    {
        string text = label != null ? label.text : property.displayName;
        // Array elements are named "data" by Unity; document the parent field.
        string field = property.name;
=======
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
>>>>>>> Stashed changes
        if (field == "data")
        {
            string path = property.propertyPath;
            int dot = path.IndexOf('.');
<<<<<<< Updated upstream
            if (dot > 0) field = path.Substring(0, dot);
        }
        string tooltip = Tooltip(property.serializedObject.targetObject.GetType().Name,
            field);
        if (label != null && !string.IsNullOrEmpty(label.tooltip))
            tooltip += "\n" + label.tooltip;
        return new GUIContent(text, tooltip);
    }

    public static bool PropertyField(SerializedProperty property)
    {
        return EditorGUILayout.PropertyField(property, Label(property, null), true);
    }

    public static bool PropertyField(SerializedProperty property, bool children)
    {
        return EditorGUILayout.PropertyField(property, Label(property, null), children);
    }

    public static bool PropertyField(SerializedProperty property, GUIContent label)
    {
        return EditorGUILayout.PropertyField(property, Label(property, label), true);
    }

    public static bool PropertyField(SerializedProperty property,
        GUIContent label, bool children)
    {
        return EditorGUILayout.PropertyField(property, Label(property, label), children);
    }

    private static void Guide(string title, string required, string optional, string note)
    {
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Component requirements: " + title,
            EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox(
            "Required\n" + required +
            (string.IsNullOrEmpty(optional) ? "" : "\n\nOptional\n" + optional) +
            (string.IsNullOrEmpty(note) ? "" : "\n\nNote\n" + note),
            MessageType.Info);
    }

    public static void InputRequirements(PXR_InputType type, GameObject source)
=======

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
>>>>>>> Stashed changes
    {
        switch (type)
        {
            case PXR_InputType.Interact:
<<<<<<< Updated upstream
                Guide("Interact", "Collider on the input GameObject; PXR_UnifiedInputs.", "Output UdonSharpBehaviour on the chosen event target.",
                    "VRChat interaction prompt is enabled for this input type.");
                if (source.GetComponent<Collider>() == null)
                    EditorGUILayout.HelpBox("Missing Collider on this object.", MessageType.Warning);
                break;
            case PXR_InputType.Pickup:
            case PXR_InputType.PickupUse:
                Guide(type == PXR_InputType.Pickup ? "Pickup" : "Pickup Use",
                    "Rigidbody + Collider + VRC Pickup on this GameObject; PXR_UnifiedInputs.",
                    "VRC Object Sync on the same GameObject to show held movement to all players.",
                    type == PXR_InputType.Pickup ? "Pickup runs on grab; optional On Drop Behavior." : "Pickup Use runs when the holder presses Use. Synced controls output events, not prop movement.");
                if (source.GetComponent<Collider>() == null || source.GetComponent<Rigidbody>() == null ||
                    source.GetComponent<VRCPickup>() == null)
                    EditorGUILayout.HelpBox("This pickup is missing Collider, Rigidbody, or VRC Pickup.", MessageType.Warning);
                if (source.GetComponent<VRCObjectSync>() == null)
                    EditorGUILayout.HelpBox("No VRC Object Sync: remote players may not see held-object movement.", MessageType.Info);
                break;
            case PXR_InputType.PlayerTriggerEnter:
            case PXR_InputType.PlayerTriggerExit:
                Guide(type == PXR_InputType.PlayerTriggerEnter ? "Player Trigger Enter" : "Player Trigger Exit",
                    "Collider set to Is Trigger on the input object; PXR_UnifiedInputs.",
                    "Rigidbody as needed by the Unity collision setup.",
                    "Only local-player entry/exit dispatches the event.");
                Collider c = source.GetComponent<Collider>();
                if (c == null || !c.isTrigger)
                    EditorGUILayout.HelpBox("Requires a trigger Collider (Is Trigger enabled).", MessageType.Warning);
                break;
            case PXR_InputType.Key:
                Guide("Key Lock", "Collider on the lock; PXR_UnifiedInputs; assigned PXR_KeyLockState (Manual).",
                    "Ordinary key: Key + Collider (+ Rigidbody for physical collision). Pooled key: PXR_PoolManager, PXR_PooledObject, VRCObjectPool, VRCObjectSync.",
                    "Assign the lock state to the Pool Manager Key Locks array for pooled impacts. The state must not share a GameObject with VRCObjectSync.");
                if (source.GetComponent<Collider>() == null)
                    EditorGUILayout.HelpBox("Missing lock Collider.", MessageType.Warning);
                break;
            case PXR_InputType.UIButton:
                Guide("UI Button", "World Space Canvas + Graphic Raycaster + VRC Ui Shape (same Canvas GameObject); EventSystem in scene; Unity Button.",
                    "BoxCollider covering the Canvas; Canvas on Default layer.",
                    "Button OnClick: target UdonBehaviour.SendCustomEvent(\"UIButtonPress\"). PXR_UnifiedInputs can live on a separate GameObject.");
=======
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
>>>>>>> Stashed changes
                break;
        }
    }

    public static void OutputRequirements(string action)
    {
        switch (action)
        {
<<<<<<< Updated upstream
            case "Chain":
                Guide("Chain Event", "Target UdonSharpBehaviour exposing a public parameterless method.", "Delay in seconds.", "Avoid chains that form cycles."); break;
            case "ToggleObject":
                Guide("Toggle Object", "Target GameObject reference.", "Persist state if late joiners should see the result.", "Avoid disabling this controller’s own GameObject."); break;
            case "DestroyObject":
                Guide("Destroy Object", "Target GameObject reference.", "None.", "This function deactivates via SetActive(false); it is not permanent networked destruction."); break;
            case "PlayAnimation":
                Guide("Play Animation", "Animator component, RuntimeAnimatorController, and matching Bool/Trigger/Int parameter.", "Persist Bool/Int state when needed.", "Animator parameter name and type must match exactly."); break;
            case "PlaySound":
                Guide("Play Sound", "AudioSource and an assigned AudioClip or nonempty Audio Clips array.", "VRC Spatial Audio Source on the AudioSource GameObject for spatial audio.", "Each call advances to the next playlist clip when using an array."); break;
            case "PlayAVProVideo":
                Guide("AVPro Video", "SDK UdonSyncPlayer AVPro UdonBehaviour OR VRCAVProVideoPlayer; valid video URL.", "Video display material/renderer and audio output from the chosen player setup.", "Direct AVPro playback is not a synchronized late-joiner playback system."); break;
            case "TeleportPlayer":
                Guide("Teleport Player", "At least one destination Transform; PXR_UnifiedInputs for initiating-player attribution.", "Username for by-name mode; range for by-range mode.", "TeleportUser requires a known initiating player. No Collider is needed on the output object."); break;
            case "SpawnObject":
                Guide("Spawn Object", "Spawn Transform and local prefab (Synced OFF), or configured PXR_PoolManager (Synced ON).",
                    "For networked spawning: VRCObjectPool + pooled objects with PXR_PooledObject and VRCObjectSync.", "The input’s Synced checkbox selects local instantiation or networked pooling."); break;
            case "ToggleComponent":
                Guide("Toggle Component", "Reference to the chosen Behaviour, Renderer, or Collider component.", "Persist state for late joiners.", "The assigned component class must match Component Target Type."); break;
=======
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
>>>>>>> Stashed changes
        }
    }

    public static void PhysicsRequirements(string action)
    {
        switch (action)
        {
<<<<<<< Updated upstream
            case "Chain": OutputRequirements("Chain"); break;
            case "RadialForce":
            case "BeamForce":
                Guide(action == "RadialForce" ? "Radial Force" : "Beam Force",
                    "PXR_UnifiedPhysics emitter; target objects need a non-trigger Collider and a non-kinematic Rigidbody.",
                    "VRCObjectSync on each affected object to synchronize physical movement; Force Kinematic On Remote is recommended.",
                    "Affected Layers filters physics targets. Continuous force can be persisted by the owner."); break;
            case "SpawnObject":
                Guide("Physics Spawn", "Spawn Transform and local Rigidbody prefab (Synced OFF), or PXR_PoolManager (Synced ON).",
                    "Network pool: VRCObjectPool, PXR_PooledObject, VRCObjectSync, Rigidbody on each pooled physics object.",
                    "Velocity is in the Spawn Transform’s local axes, in metres per second."); break;
            case "Detection":
                Guide("Physics Detection", "Colliders on target objects in the included layers.",
                    "Increase Max Colliders for crowded scenes.", "Only objects returned by the overlap query can receive force."); break;
=======
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
>>>>>>> Stashed changes
        }
    }
}

<<<<<<< Updated upstream
// Generic inspectors cover the toolkit scripts without a
// dedicated foldout editor. They provide tooltips for every visible field.
=======
// Generic documented inspectors for toolkit components.
>>>>>>> Stashed changes
public class PXR_DocumentedInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;
<<<<<<< Updated upstream
        serializedObject.Update();
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (prop.name == "m_Script") continue;
            PXR_InspectorGuidance.PropertyField(prop, true);
        }
=======

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

>>>>>>> Stashed changes
        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(Key))]
public class PXR_KeyDocumentedInspector : PXR_DocumentedInspector { }
<<<<<<< Updated upstream
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
=======

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

>>>>>>> Stashed changes
#endif
