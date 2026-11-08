
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;

using Debug = UnityEngine.Debug;

// ============================================
// INPUT TYPES
// ============================================

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

    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryVideoTargets;

    // ========================================
    // DROP EVENTS
    // ========================================

    [Header("Drop Events")]

    public UdonSharpBehaviour[] dropEventTargets;

    public string[] dropEventNames;

    public int[] dropEventScopes;

    [HideInInspector]
    public PXR_UnifiedOutputs[] dropTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] dropVideoTargets;

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
            }
        }
    }

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

    public void RefreshInteractionState()
    {
        DisableInteractive =
            interactionType != PXR_InputType.Interact;
    }

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
    }

    public override void OnPickup()
    {
        if (interactionType ==
            PXR_InputType.Pickup)
        {
            Trigger();
        }
    }

    public override void OnDrop()
    {
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
    }

    public override void OnPickupUseDown()
    {
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
    }

    public void UIButtonPress()
    {
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
        if (interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(keyLockState) ||
            !Utilities.IsValid(collision))
            return;

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

    // Called locally by the authoritative Key state.
    // The supplied identity belongs to the last
    // accepted Key that completed the lock.
    public void CompleteKeyFromState(
        int playerId,
        string playerName)
    {
        if (!Utilities.IsValid(keyLockState) ||
            !Networking.IsOwner(keyLockState.gameObject))
            return;

        DispatchEvents(false, playerId, playerName);
    }

    // ========================================
    // PUBLIC TRIGGER METHODS
    // ========================================

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

    private void DispatchFromLocalPlayer(
        bool drop)
    {
        VRCPlayerApi player =
            Networking.LocalPlayer;

        int playerId = -1;
        string playerName = "";

        if (Utilities.IsValid(player))
        {
            playerId = player.playerId;
            playerName = player.displayName;
        }

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

        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

        return
            names != null &&
            videos != null &&
            index >= 0 &&
            index < names.Length &&
            index < videos.Length &&
            names[index] == "PlayAVProVideo" &&
            Utilities.IsValid(videos[index]) &&
            videos[index].avProUseSyncedPrefab;
    }

    private void DispatchSyncedVideos(
        bool drop)
    {
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

        if (names == null || videos == null)
            return;

        int count = Mathf.Min(
            names.Length,
            videos.Length
        );

        for (int i = 0; i < count; i++)
        {
            if (!IsSyncedVideo(drop, i))
                continue;

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
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        if (targets != null &&
            targets.Length > 0)
        {
            if (names == null)
                return;

            int count = Mathf.Min(
                targets.Length,
                names.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (names[i] != "SpawnObject")
                    continue;

                if (!Utilities.IsValid(targets[i]))
                    continue;

                string method = synced
                    ? "SpawnPooledObject"
                    : "SpawnObject";

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

    private void DispatchTeleports(
        bool drop,
        int playerId,
        string playerName)
    {
        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        PXR_UnifiedOutputs[] teleports = drop
            ? dropTeleportTargets
            : primaryTeleportTargets;

        float delay = useDelay
            ? Mathf.Max(0f, delaySeconds)
            : 0f;

        if (names != null &&
            names.Length > 0)
        {
            for (int i = 0;
                 i < names.Length;
                 i++)
            {
                if (names[i] != "TeleportPlayer")
                    continue;

                if (teleports == null ||
                    i >= teleports.Length)
                    continue;

                if (!Utilities.IsValid(teleports[i]))
                    continue;

                teleports[i].RequestTeleport(
                    playerId,
                    playerName,
                    transform.position,
                    delay
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
                delay
            );
        }
    }

    // ========================================
    // EVENT SCOPE CHECK
    // ========================================

    private bool HasScope(
        bool drop,
        bool network)
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        int[] scopes = drop
            ? dropEventScopes
            : primaryEventScopes;

        if (targets != null &&
            targets.Length > 0)
        {
            if (names == null)
                return false;

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

                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

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

        return
            Utilities.IsValid(legacy) &&
            !string.IsNullOrEmpty(legacyName) &&
            legacyName != "SpawnObject" &&
            legacyName != "TeleportPlayer" &&
            synced == network;
    }

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
    }

    public void LocalTrigger()
    {
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
    }

    public void LocalDropTrigger()
    {
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

    // ========================================
    // EXECUTE ORDINARY EVENTS
    // ========================================

    private void ExecuteList(
        bool drop,
        bool network)
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop
            ? dropEventNames
            : primaryEventNames;

        int[] scopes = drop
            ? dropEventScopes
            : primaryEventScopes;

        if (targets != null &&
            targets.Length > 0)
        {
            if (names == null)
                return;

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

                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

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
                    );
                }
            }

            return;
        }

        // Legacy single-target support.

        UdonSharpBehaviour legacy = drop
            ? secondEventTarget
            : eventTarget;
        string legacyName = drop
            ? secondEventName
            : eventName;

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
    }
}
