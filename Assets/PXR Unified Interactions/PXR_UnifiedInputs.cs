
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;

<<<<<<< Updated upstream
using Debug = UnityEngine.Debug;

// ============================================
// INPUT TYPES
// ============================================

=======
>>>>>>> Stashed changes
public enum PXR_InputType
{
    Interact = 0,
    Pickup = 1,
    PickupUse = 2,
    PlayerTriggerEnter = 3,
    PlayerTriggerExit = 4,
    Key = 5,
    UIButton = 7
}

public enum PXR_DropBehavior
{
    None = 0,
    RepeatFunction = 1,
    SeparateFunction = 2
}

public enum PXR_EventScope
{
    InputDefault = 0,
    LocalOnly = 1,
    AllPlayers = 2
}

<<<<<<< Updated upstream
// ============================================
// UNIFIED INPUTS
// ============================================

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_UnifiedInputs : UdonSharpBehaviour
{
    // ========================================
    // INTERACTION SETTINGS
    // ========================================

    [Header("Interaction")]

    public PXR_InputType interactionType =
        PXR_InputType.Interact;

    public PXR_DropBehavior dropBehavior =
        PXR_DropBehavior.None;

    // ========================================
    // NETWORK SETTINGS
    // ========================================

    [Header("Networking")]

    public bool synced = false;

    public bool useDelay = false;

    [Min(0f)]
    public float delaySeconds = 0f;

    // ========================================
    // KEY SETTINGS
    // ========================================

    [Header("Key")]

    public bool destroyKeyAfterUse = false;

    [Min(1)]
    public int requiredKeys = 1;

    public bool checkKeyTag = false;

    public string requiredKeyTag = "";

    public PXR_PoolManager pooledKeyPool;

    // Manually synchronized lock data resides on a
    // separate PXR_KeyLockState UdonBehaviour.
    public PXR_KeyLockState keyLockState;

    // ========================================
    // PRIMARY EVENTS
    // ========================================

    [Header("Primary Events")]

    public UdonSharpBehaviour[] primaryEventTargets;

    public string[] primaryEventNames;

    public int[] primaryEventScopes;

=======
[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_UnifiedInputs : UdonSharpBehaviour
{
    [Header("Interaction")]
    public PXR_InputType interactionType = PXR_InputType.Interact;
    public PXR_DropBehavior dropBehavior = PXR_DropBehavior.None;

    [Header("Networking")]
    public bool synced = false;

    // Legacy global delay is hidden but retained for scene migration.
    [HideInInspector] public bool useDelay = false;
    [HideInInspector, Min(0f)] public float delaySeconds = 0f;
    [HideInInspector] public bool eventDelaysMigrated = false;

    [Header("Key")]
    public bool destroyKeyAfterUse = false;
    [Min(1)] public int requiredKeys = 1;

    [Tooltip("After completion, dispatch outputs and reset the key count. Ordinary keys must exit before re-entry; new pooled generations count again.")]
    public bool repeatable = false;

    public bool checkKeyTag = false;
    public string requiredKeyTag = "";
    public PXR_PoolManager pooledKeyPool;
    public PXR_KeyLockState keyLockState;

    [Header("Primary Events")]
    public UdonSharpBehaviour[] primaryEventTargets;
    public string[] primaryEventNames;
    public int[] primaryEventScopes;

    [Tooltip("Independent delay in seconds for each primary event, starting when the input triggers.")]
    public float[] primaryEventDelays = new float[0];

>>>>>>> Stashed changes
    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryVideoTargets;

<<<<<<< Updated upstream
    // ========================================
    // DROP EVENTS
    // ========================================

    [Header("Drop Events")]

    public UdonSharpBehaviour[] dropEventTargets;

    public string[] dropEventNames;

    public int[] dropEventScopes;

=======
    [Header("Drop Events")]
    public UdonSharpBehaviour[] dropEventTargets;
    public string[] dropEventNames;
    public int[] dropEventScopes;

    [Tooltip("Independent delay in seconds for each separate On Drop event.")]
    public float[] dropEventDelays = new float[0];

>>>>>>> Stashed changes
    [HideInInspector]
    public PXR_UnifiedOutputs[] dropTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] dropVideoTargets;

<<<<<<< Updated upstream
    // ========================================
    // LEGACY EVENT REFERENCES
    // ========================================

    [HideInInspector]
    public UdonSharpBehaviour eventTarget;

    [HideInInspector]
    public string eventName = "";

    [HideInInspector]
    public UdonSharpBehaviour secondEventTarget;

    [HideInInspector]
    public string secondEventName = "";

    [HideInInspector]
    public PXR_UnifiedOutputs teleportPrimaryTarget;

    [HideInInspector]
    public PXR_UnifiedOutputs teleportDropTarget;

    // ========================================
    // INITIALIZATION
    // ========================================

    private void Start()
    {
        // Only objects configured for Interact
        // should display VRChat's Udon interact
        // tooltip and highlight.

        RefreshInteractionState();

        // All persistent Key records are initialized
        // by the assigned PXR_KeyLockState.

        // Player triggers require trigger
        // colliders rather than solid colliders.

        if (interactionType ==
                PXR_InputType.PlayerTriggerEnter ||
            interactionType ==
                PXR_InputType.PlayerTriggerExit)
        {
            Collider[] colliders =
                GetComponents<Collider>();

            for (int i = 0;
                 i < colliders.Length;
                 i++)
            {
                if (Utilities.IsValid(colliders[i]))
                {
                    colliders[i].isTrigger = true;
                }
=======
    // Legacy event references are retained for existing scenes.
    [HideInInspector] public UdonSharpBehaviour eventTarget;
    [HideInInspector] public string eventName = "";
    [HideInInspector] public UdonSharpBehaviour secondEventTarget;
    [HideInInspector] public string secondEventName = "";
    [HideInInspector] public PXR_UnifiedOutputs teleportPrimaryTarget;
    [HideInInspector] public PXR_UnifiedOutputs teleportDropTarget;

    private void Start()
    {
        RefreshInteractionState();

        if (interactionType == PXR_InputType.PlayerTriggerEnter ||
            interactionType == PXR_InputType.PlayerTriggerExit)
        {
            Collider[] colliders = GetComponents<Collider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (Utilities.IsValid(colliders[i]))
                    colliders[i].isTrigger = true;
>>>>>>> Stashed changes
            }
        }
    }

<<<<<<< Updated upstream
    // ========================================
    // INTERACTION HIGHLIGHT
    // ========================================

    // The presence of an Interact() override
    // normally makes VRChat display the Udon
    // interaction tooltip and highlight.
    //
    // DisableInteractive suppresses this
    // without disabling the UdonBehaviour.
    //
    // VRC_Pickup may provide its own separate
    // pickup interaction indicators.

=======
>>>>>>> Stashed changes
    public void RefreshInteractionState()
    {
        DisableInteractive =
            interactionType != PXR_InputType.Interact;
    }

<<<<<<< Updated upstream
    // ========================================
    // STANDARD INPUT CALLBACKS
    // ========================================

    public override void Interact()
    {
        if (interactionType ==
            PXR_InputType.Interact)
        {
            Trigger();
        }
=======
    public override void Interact()
    {
        if (interactionType == PXR_InputType.Interact)
            Trigger();
>>>>>>> Stashed changes
    }

    public override void OnPickup()
    {
<<<<<<< Updated upstream
        if (interactionType ==
            PXR_InputType.Pickup)
        {
            Trigger();
        }
=======
        if (interactionType == PXR_InputType.Pickup)
            Trigger();
>>>>>>> Stashed changes
    }

    public override void OnDrop()
    {
<<<<<<< Updated upstream
        if (interactionType !=
            PXR_InputType.Pickup)
            return;

        if (dropBehavior ==
            PXR_DropBehavior.RepeatFunction)
        {
            Trigger();
        } else if (dropBehavior ==
                   PXR_DropBehavior.SeparateFunction)
        {
            TriggerDrop();
        }
=======
        if (interactionType != PXR_InputType.Pickup)
            return;

        if (dropBehavior == PXR_DropBehavior.RepeatFunction)
            Trigger();
        else if (dropBehavior == PXR_DropBehavior.SeparateFunction)
            TriggerDrop();
>>>>>>> Stashed changes
    }

    public override void OnPickupUseDown()
    {
<<<<<<< Updated upstream
        if (interactionType ==
            PXR_InputType.PickupUse)
        {
            Trigger();
        }
    }

    public override void OnPlayerTriggerEnter(
        VRCPlayerApi player)
    {
        if (interactionType !=
            PXR_InputType.PlayerTriggerEnter)
            return;

        if (Utilities.IsValid(player) &&
            player.isLocal)
        {
            Trigger();
        }
    }

    public override void OnPlayerTriggerExit(
        VRCPlayerApi player)
    {
        if (interactionType !=
            PXR_InputType.PlayerTriggerExit)
            return;

        if (Utilities.IsValid(player) &&
            player.isLocal)
        {
            Trigger();
        }
=======
        if (interactionType == PXR_InputType.PickupUse)
            Trigger();
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (interactionType != PXR_InputType.PlayerTriggerEnter)
            return;

        if (Utilities.IsValid(player) && player.isLocal)
            Trigger();
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (interactionType != PXR_InputType.PlayerTriggerExit)
            return;

        if (Utilities.IsValid(player) && player.isLocal)
            Trigger();
>>>>>>> Stashed changes
    }

    public void UIButtonPress()
    {
<<<<<<< Updated upstream
        if (interactionType ==
            PXR_InputType.UIButton)
        {
            Trigger();
        }
    }

    // ========================================
    // ORDINARY KEY COLLISIONS
    // ========================================

    private void OnCollisionEnter(Collision collision)
    {
=======
        if (interactionType == PXR_InputType.UIButton)
            Trigger();
    }

    // Ordinary keys use the separately synchronized lock state.
    // Pooled projectiles use the pool manager's acknowledgment path.
    private void OnCollisionEnter(Collision collision)
    {
        // The lock state decides which client registers the key
        // (the one physically controlling it).
>>>>>>> Stashed changes
        if (interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(keyLockState) ||
            !Utilities.IsValid(collision))
            return;

<<<<<<< Updated upstream
        GameObject other = collision.gameObject;
        if (!Utilities.IsValid(other))
            return;

        // Pool collisions use the pool manager's
        // slot/generation + acknowledgment path.
        PXR_PooledObject pooled =
            other.GetComponent<PXR_PooledObject>();
        if (Utilities.IsValid(pooled))
            return;

        Rigidbody rb = collision.rigidbody;
        if (Utilities.IsValid(rb))
        {
            pooled = rb.GetComponent<PXR_PooledObject>();
            if (Utilities.IsValid(pooled))
                return;
        }

        Key key = other.GetComponent<Key>();
        if (!Utilities.IsValid(key))
            key = other.GetComponentInChildren<Key>();

        if (!Utilities.IsValid(key) &&
            Utilities.IsValid(rb))
        {
            key = rb.GetComponent<Key>();
            if (!Utilities.IsValid(key))
                key = rb.GetComponentInChildren<Key>();
        }

        if (!Utilities.IsValid(key))
            return;

        // This call is local. The Key state checks
        // authority before accepting any collision.
        keyLockState.TryRegisterOrdinaryKey(key);
    }

    // Retain the existing public reset/status API.
    public int GetKeyCount()
    {
        return Utilities.IsValid(keyLockState)
            ? keyLockState.GetKeyCount() : 0;
=======
        keyLockState.HandleKeyCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(keyLockState))
            return;

        keyLockState.NotifyOrdinaryKeyExit(collision);
    }

    public int GetKeyCount()
    {
        return Utilities.IsValid(keyLockState)
            ? keyLockState.GetKeyCount()
            : 0;
>>>>>>> Stashed changes
    }

    public bool AreKeysComplete()
    {
        return Utilities.IsValid(keyLockState) &&
            keyLockState.AreKeysComplete();
    }

    public void ResetKeys()
    {
        if (Utilities.IsValid(keyLockState))
            keyLockState.ResetKeys();
    }

<<<<<<< Updated upstream
    // Called locally by the authoritative Key state.
    // The supplied identity belongs to the last
    // accepted Key that completed the lock.
    public void CompleteKeyFromState(
        int playerId,
        string playerName)
=======
    public void CompleteKeyFromState(int playerId, string playerName)
>>>>>>> Stashed changes
    {
        if (!Utilities.IsValid(keyLockState) ||
            !Networking.IsOwner(keyLockState.gameObject))
            return;

        DispatchEvents(false, playerId, playerName);
    }

<<<<<<< Updated upstream
    // ========================================
    // PUBLIC TRIGGER METHODS
    // ========================================

=======
>>>>>>> Stashed changes
    [PXR_InputCallable]
    public void Trigger()
    {
        DispatchFromLocalPlayer(false);
    }

    [PXR_InputCallable]
    public void TriggerDrop()
    {
        DispatchFromLocalPlayer(true);
    }

<<<<<<< Updated upstream
    private void DispatchFromLocalPlayer(
        bool drop)
    {
        VRCPlayerApi player =
            Networking.LocalPlayer;

=======
    private void DispatchFromLocalPlayer(bool drop)
    {
        VRCPlayerApi player = Networking.LocalPlayer;
>>>>>>> Stashed changes
        int playerId = -1;
        string playerName = "";

        if (Utilities.IsValid(player))
        {
            playerId = player.playerId;
            playerName = player.displayName;
        }

<<<<<<< Updated upstream
        DispatchEvents(
            drop,
            playerId,
            playerName
        );
    }

    // ========================================
    // EVENT DISPATCH
    // ========================================

    private void DispatchEvents(
        bool drop,
        int playerId,
        string playerName)
    {
        // Specialized events are handled first.

        DispatchTeleports(
            drop,
            playerId,
            playerName
        );

        DispatchSyncedVideos(drop);

        DispatchSpawns(drop);

        // Ordinary events follow their
        // local or network scope.

=======
        DispatchEvents(drop, playerId, playerName);
    }

    // Special routes retain their original implementation.
    // Ordinary All Players events are broadcast once per activation.
    private void DispatchEvents(bool drop, int playerId, string playerName)
    {
        DispatchTeleports(drop, playerId, playerName);
        DispatchSyncedVideos(drop);
        DispatchSpawns(drop);

>>>>>>> Stashed changes
        if (HasScope(drop, false))
        {
            if (drop)
                LocalDropTrigger();
            else
                LocalTrigger();
        }

        if (HasScope(drop, true))
        {
            SendCustomNetworkEvent(
                NetworkEventTarget.All,
                drop
                    ? nameof(NetworkDropTrigger)
                    : nameof(NetworkTrigger)
            );
        }
    }

<<<<<<< Updated upstream
    // ========================================
    // SYNCHRONIZED AVPRO
    // ========================================

    private bool IsSyncedVideo(
        bool drop,
        int index)
    {
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

=======
    private float LegacyDelay()
    {
        return useDelay ? Mathf.Max(0f, delaySeconds) : 0f;
    }

    private float DelayForEvent(bool drop, int index)
    {
        float[] delays = drop
            ? dropEventDelays
            : primaryEventDelays;

        if (eventDelaysMigrated &&
            delays != null &&
            index >= 0 &&
            index < delays.Length)
        {
            return Mathf.Max(0f, delays[index]);
        }

        // Support worlds whose Inspector data has not yet migrated.
        return LegacyDelay();
    }

    // Schedule directly on the target: each activation has its own
    // pending event and cannot overwrite an earlier activation.
    private void InvokeWithDelay(
        UdonSharpBehaviour receiver,
        string method,
        float delay)
    {
        if (!Utilities.IsValid(receiver) ||
            string.IsNullOrEmpty(method))
            return;

        if (delay > 0f)
        {
            receiver.SendCustomEventDelayedSeconds(method, delay);
        } else
        {
            receiver.SendCustomEvent(method);
        }
    }

    private bool IsSyncedVideo(bool drop, int index)
    {
        string[] names = drop ? dropEventNames : primaryEventNames;
>>>>>>> Stashed changes
        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

<<<<<<< Updated upstream
        return
            names != null &&
=======
        return names != null &&
>>>>>>> Stashed changes
            videos != null &&
            index >= 0 &&
            index < names.Length &&
            index < videos.Length &&
            names[index] == "PlayAVProVideo" &&
            Utilities.IsValid(videos[index]) &&
            videos[index].avProUseSyncedPrefab;
    }

<<<<<<< Updated upstream
    private void DispatchSyncedVideos(
        bool drop)
    {
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

=======
    private void DispatchSyncedVideos(bool drop)
    {
        string[] names = drop ? dropEventNames : primaryEventNames;
>>>>>>> Stashed changes
        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

        if (names == null || videos == null)
            return;

<<<<<<< Updated upstream
        int count = Mathf.Min(
            names.Length,
            videos.Length
        );
=======
        int count = Mathf.Min(names.Length, videos.Length);
>>>>>>> Stashed changes

        for (int i = 0; i < count; i++)
        {
            if (!IsSyncedVideo(drop, i))
                continue;

<<<<<<< Updated upstream
            PXR_UnifiedOutputs output =
                videos[i];

            if (useDelay &&
                delaySeconds > 0f)
            {
                output.SendCustomEventDelayedSeconds(
                    "PlayAVProVideo",
                    delaySeconds
                );
            } else
            {
                output.PlayAVProVideo();
            }
        }
    }

    // ========================================
    // SPAWN ROUTING
    // ========================================

    // Input Synced determines whether spawn
    // requests create local instantiated
    // objects or use the networked pool.

    private void DispatchSpawns(
        bool drop)
=======
            InvokeWithDelay(
                videos[i],
                "PlayAVProVideo",
                DelayForEvent(drop, i)
            );
        }
    }

    private void DispatchSpawns(bool drop)
>>>>>>> Stashed changes
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

<<<<<<< Updated upstream
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        if (targets != null &&
            targets.Length > 0)
=======
        string[] names = drop ? dropEventNames : primaryEventNames;

        if (targets != null && targets.Length > 0)
>>>>>>> Stashed changes
        {
            if (names == null)
                return;

<<<<<<< Updated upstream
            int count = Mathf.Min(
                targets.Length,
                names.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (names[i] != "SpawnObject")
                    continue;

                if (!Utilities.IsValid(targets[i]))
=======
            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (names[i] != "SpawnObject" ||
                    !Utilities.IsValid(targets[i]))
>>>>>>> Stashed changes
                    continue;

                string method = synced
                    ? "SpawnPooledObject"
                    : "SpawnObject";

<<<<<<< Updated upstream
                if (useDelay &&
                    delaySeconds > 0f)
                {
                    targets[i].SendCustomEventDelayedSeconds(
                        method,
                        delaySeconds
                    );
                } else
                {
                    targets[i].SendCustomEvent(method);
                }
=======
                InvokeWithDelay(
                    targets[i],
                    method,
                    DelayForEvent(drop, i)
                );
>>>>>>> Stashed changes
            }

            return;
        }

        UdonSharpBehaviour legacyTarget = drop
            ? secondEventTarget
            : eventTarget;

        string legacyName = drop
            ? secondEventName
            : eventName;

        if (Utilities.IsValid(legacyTarget) &&
            legacyName == "SpawnObject")
        {
            string method = synced
                ? "SpawnPooledObject"
                : "SpawnObject";

<<<<<<< Updated upstream
            if (useDelay &&
                delaySeconds > 0f)
            {
                legacyTarget.SendCustomEventDelayedSeconds(
                    method,
                    delaySeconds
                );
            } else
            {
                legacyTarget.SendCustomEvent(method);
            }
        }
    }

    // =====================================
    // TELEPORT ROUTING
    // =====================================

=======
            InvokeWithDelay(legacyTarget, method, LegacyDelay());
        }
    }

>>>>>>> Stashed changes
    private void DispatchTeleports(
        bool drop,
        int playerId,
        string playerName)
    {
<<<<<<< Updated upstream
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

=======
        string[] names = drop ? dropEventNames : primaryEventNames;
>>>>>>> Stashed changes
        PXR_UnifiedOutputs[] teleports = drop
            ? dropTeleportTargets
            : primaryTeleportTargets;

<<<<<<< Updated upstream
        float delay = useDelay
            ? Mathf.Max(0f, delaySeconds)
            : 0f;

        if (names != null &&
            names.Length > 0)
        {
            for (int i = 0;
                 i < names.Length;
                 i++)
=======
        if (names != null && names.Length > 0)
        {
            for (int i = 0; i < names.Length; i++)
>>>>>>> Stashed changes
            {
                if (names[i] != "TeleportPlayer")
                    continue;

<<<<<<< Updated upstream
                if (teleports == null ||
                    i >= teleports.Length)
=======
                if (teleports == null || i >= teleports.Length)
>>>>>>> Stashed changes
                    continue;

                if (!Utilities.IsValid(teleports[i]))
                    continue;

                teleports[i].RequestTeleport(
                    playerId,
                    playerName,
                    transform.position,
<<<<<<< Updated upstream
                    delay
=======
                    DelayForEvent(drop, i)
>>>>>>> Stashed changes
                );
            }

            return;
        }

        string legacyName = drop
            ? secondEventName
            : eventName;

        PXR_UnifiedOutputs legacyTarget = drop
            ? teleportDropTarget
            : teleportPrimaryTarget;

        if (legacyName == "TeleportPlayer" &&
            Utilities.IsValid(legacyTarget))
        {
            legacyTarget.RequestTeleport(
                playerId,
                playerName,
                transform.position,
<<<<<<< Updated upstream
                delay
=======
                LegacyDelay()
>>>>>>> Stashed changes
            );
        }
    }

<<<<<<< Updated upstream
    // ========================================
    // EVENT SCOPE CHECK
    // ========================================

    private bool HasScope(
        bool drop,
        bool network)
=======
    private bool HasScope(bool drop, bool network)
>>>>>>> Stashed changes
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

<<<<<<< Updated upstream
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        int[] scopes = drop
            ? dropEventScopes
            : primaryEventScopes;

        if (targets != null &&
            targets.Length > 0)
=======
        string[] names = drop ? dropEventNames : primaryEventNames;
        int[] scopes = drop ? dropEventScopes : primaryEventScopes;

        if (targets != null && targets.Length > 0)
>>>>>>> Stashed changes
        {
            if (names == null)
                return false;

<<<<<<< Updated upstream
            int count = Mathf.Min(
                targets.Length,
                names.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]))
                    continue;

                if (string.IsNullOrEmpty(names[i]))
=======
            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]) ||
                    string.IsNullOrEmpty(names[i]))
>>>>>>> Stashed changes
                    continue;

                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

<<<<<<< Updated upstream
                if (scopes != null &&
                    i < scopes.Length)
                {
                    scope = scopes[i];
                }

                bool all = synced;

                if (scope ==
                    (int)PXR_EventScope.LocalOnly)
                {
                    all = false;
                } else if (scope ==
                           (int)PXR_EventScope.AllPlayers)
                {
                    all = true;
                }
=======
                if (scopes != null && i < scopes.Length)
                    scope = scopes[i];

                bool all = synced;

                if (scope == (int)PXR_EventScope.LocalOnly)
                    all = false;
                else if (scope == (int)PXR_EventScope.AllPlayers)
                    all = true;
>>>>>>> Stashed changes

                if (all == network)
                    return true;
            }

            return false;
        }

        UdonSharpBehaviour legacy = drop
            ? secondEventTarget
            : eventTarget;

        string legacyName = drop
            ? secondEventName
            : eventName;

<<<<<<< Updated upstream
        return
            Utilities.IsValid(legacy) &&
=======
        return Utilities.IsValid(legacy) &&
>>>>>>> Stashed changes
            !string.IsNullOrEmpty(legacyName) &&
            legacyName != "SpawnObject" &&
            legacyName != "TeleportPlayer" &&
            synced == network;
    }

<<<<<<< Updated upstream
    // ========================================
    // LOCAL AND NETWORK RECEIVERS
    // ========================================

    [NetworkCallable]
    public void NetworkTrigger()
    {
        if (useDelay &&
            delaySeconds > 0f)
        {
            SendCustomEventDelayedSeconds(
                nameof(ExecuteNetworkEvent),
                delaySeconds
            );
        } else
        {
            ExecuteNetworkEvent();
        }
    }

    [NetworkCallable]
    public void NetworkDropTrigger()
    {
        if (useDelay &&
            delaySeconds > 0f)
        {
            SendCustomEventDelayedSeconds(
                nameof(ExecuteNetworkDropEvent),
                delaySeconds
            );
        } else
        {
            ExecuteNetworkDropEvent();
        }
=======
    // Network delivery does not introduce another global delay.
    // Each receiver independently schedules its configured rows.
    //
    // Rate limit (events per second, 1-100). VRChat's default of
    // 5/s queues rapid activations and delivers them late.
    private const int TRIGGER_EVENT_RATE = 20;

    [NetworkCallable(TRIGGER_EVENT_RATE)]
    public void NetworkTrigger()
    {
        ExecuteNetworkEvent();
    }

    [NetworkCallable(TRIGGER_EVENT_RATE)]
    public void NetworkDropTrigger()
    {
        ExecuteNetworkDropEvent();
>>>>>>> Stashed changes
    }

    public void LocalTrigger()
    {
<<<<<<< Updated upstream
        if (useDelay &&
            delaySeconds > 0f)
        {
            SendCustomEventDelayedSeconds(
                nameof(ExecuteEvent),
                delaySeconds
            );
        } else
        {
            ExecuteEvent();
        }
=======
        ExecuteEvent();
>>>>>>> Stashed changes
    }

    public void LocalDropTrigger()
    {
<<<<<<< Updated upstream
        if (useDelay &&
            delaySeconds > 0f)
        {
            SendCustomEventDelayedSeconds(
                nameof(ExecuteDropEvent),
                delaySeconds
            );
        } else
        {
            ExecuteDropEvent();
        }
=======
        ExecuteDropEvent();
>>>>>>> Stashed changes
    }

    public void ExecuteEvent()
    {
        ExecuteList(false, false);
    }

    public void ExecuteDropEvent()
    {
        ExecuteList(true, false);
    }

    public void ExecuteNetworkEvent()
    {
        ExecuteList(false, true);
    }

    public void ExecuteNetworkDropEvent()
    {
        ExecuteList(true, true);
    }

<<<<<<< Updated upstream
    // ========================================
    // EXECUTE ORDINARY EVENTS
    // ========================================

    private void ExecuteList(
        bool drop,
        bool network)
=======
    private void ExecuteList(bool drop, bool network)
>>>>>>> Stashed changes
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

<<<<<<< Updated upstream
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        int[] scopes = drop
            ? dropEventScopes
            : primaryEventScopes;

        if (targets != null &&
            targets.Length > 0)
=======
        string[] names = drop ? dropEventNames : primaryEventNames;
        int[] scopes = drop ? dropEventScopes : primaryEventScopes;

        if (targets != null && targets.Length > 0)
>>>>>>> Stashed changes
        {
            if (names == null)
                return;

<<<<<<< Updated upstream
            int count = Mathf.Min(
                targets.Length,
                names.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]))
                    continue;

                if (string.IsNullOrEmpty(names[i]))
                    continue;

                // Special event types have already
                // been processed by DispatchEvents.

=======
            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]) ||
                    string.IsNullOrEmpty(names[i]))
                    continue;

                // Routed separately; never execute twice.
>>>>>>> Stashed changes
                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

<<<<<<< Updated upstream
                if (scopes != null &&
                    i < scopes.Length)
                {
                    scope = scopes[i];
                }

                bool all = synced;

                if (scope ==
                    (int)PXR_EventScope.LocalOnly)
                {
                    all = false;
                } else if (scope ==
                           (int)PXR_EventScope.AllPlayers)
                {
                    all = true;
                }

                if (all == network)
                {
                    targets[i].SendCustomEvent(
                        names[i]
=======
                if (scopes != null && i < scopes.Length)
                    scope = scopes[i];

                bool all = synced;

                if (scope == (int)PXR_EventScope.LocalOnly)
                    all = false;
                else if (scope == (int)PXR_EventScope.AllPlayers)
                    all = true;

                if (all == network)
                {
                    InvokeWithDelay(
                        targets[i],
                        names[i],
                        DelayForEvent(drop, i)
>>>>>>> Stashed changes
                    );
                }
            }

            return;
        }

<<<<<<< Updated upstream
        // Legacy single-target support.

        UdonSharpBehaviour legacy = drop
            ? secondEventTarget
            : eventTarget;
=======
        UdonSharpBehaviour legacy = drop
            ? secondEventTarget
            : eventTarget;

>>>>>>> Stashed changes
        string legacyName = drop
            ? secondEventName
            : eventName;

<<<<<<< Updated upstream
        if (!Utilities.IsValid(legacy))
            return;

        if (string.IsNullOrEmpty(legacyName))
            return;

        if (legacyName == "TeleportPlayer" ||
            legacyName == "SpawnObject")
            return;

        if (synced != network)
            return;

        legacy.SendCustomEvent(legacyName);
=======
        if (!Utilities.IsValid(legacy) ||
            string.IsNullOrEmpty(legacyName) ||
            legacyName == "TeleportPlayer" ||
            legacyName == "SpawnObject" ||
            synced != network)
        {
            return;
        }

        InvokeWithDelay(legacy, legacyName, LegacyDelay());
>>>>>>> Stashed changes
    }
}
