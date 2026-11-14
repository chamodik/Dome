
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;

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

    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] primaryVideoTargets;

    [Header("Drop Events")]
    public UdonSharpBehaviour[] dropEventTargets;
    public string[] dropEventNames;
    public int[] dropEventScopes;

    [Tooltip("Independent delay in seconds for each separate On Drop event.")]
    public float[] dropEventDelays = new float[0];

    [HideInInspector]
    public PXR_UnifiedOutputs[] dropTeleportTargets;

    [HideInInspector]
    public PXR_UnifiedOutputs[] dropVideoTargets;

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
            }
        }
    }

    public void RefreshInteractionState()
    {
        DisableInteractive =
            interactionType != PXR_InputType.Interact;
    }

    public override void Interact()
    {
        if (interactionType == PXR_InputType.Interact)
            Trigger();
    }

    public override void OnPickup()
    {
        if (interactionType == PXR_InputType.Pickup)
            Trigger();
    }

    public override void OnDrop()
    {
        if (interactionType != PXR_InputType.Pickup)
            return;

        if (dropBehavior == PXR_DropBehavior.RepeatFunction)
            Trigger();
        else if (dropBehavior == PXR_DropBehavior.SeparateFunction)
            TriggerDrop();
    }

    public override void OnPickupUseDown()
    {
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
    }

    public void UIButtonPress()
    {
        if (interactionType == PXR_InputType.UIButton)
            Trigger();
    }

    // Ordinary keys use the separately synchronized lock state.
    // Pooled projectiles use the pool manager's acknowledgment path.
    private void OnCollisionEnter(Collision collision)
    {
        // The lock state decides which client registers the key
        // (the one physically controlling it).
        if (interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(keyLockState) ||
            !Utilities.IsValid(collision))
            return;

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

    public void CompleteKeyFromState(int playerId, string playerName)
    {
        if (!Utilities.IsValid(keyLockState) ||
            !Networking.IsOwner(keyLockState.gameObject))
            return;

        DispatchEvents(false, playerId, playerName);
    }

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

    private void DispatchFromLocalPlayer(bool drop)
    {
        VRCPlayerApi player = Networking.LocalPlayer;
        int playerId = -1;
        string playerName = "";

        if (Utilities.IsValid(player))
        {
            playerId = player.playerId;
            playerName = player.displayName;
        }

        DispatchEvents(drop, playerId, playerName);
    }

    // Special routes retain their original implementation.
    // Ordinary All Players events are broadcast once per activation.
    private void DispatchEvents(bool drop, int playerId, string playerName)
    {
        DispatchTeleports(drop, playerId, playerName);
        DispatchSyncedVideos(drop);
        DispatchSpawns(drop);

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
        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

        return names != null &&
            videos != null &&
            index >= 0 &&
            index < names.Length &&
            index < videos.Length &&
            names[index] == "PlayAVProVideo" &&
            Utilities.IsValid(videos[index]) &&
            videos[index].avProUseSyncedPrefab;
    }

    private void DispatchSyncedVideos(bool drop)
    {
        string[] names = drop ? dropEventNames : primaryEventNames;
        PXR_UnifiedOutputs[] videos = drop
            ? dropVideoTargets
            : primaryVideoTargets;

        if (names == null || videos == null)
            return;

        int count = Mathf.Min(names.Length, videos.Length);

        for (int i = 0; i < count; i++)
        {
            if (!IsSyncedVideo(drop, i))
                continue;

            InvokeWithDelay(
                videos[i],
                "PlayAVProVideo",
                DelayForEvent(drop, i)
            );
        }
    }

    private void DispatchSpawns(bool drop)
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop ? dropEventNames : primaryEventNames;

        if (targets != null && targets.Length > 0)
        {
            if (names == null)
                return;

            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (names[i] != "SpawnObject" ||
                    !Utilities.IsValid(targets[i]))
                    continue;

                string method = synced
                    ? "SpawnPooledObject"
                    : "SpawnObject";

                InvokeWithDelay(
                    targets[i],
                    method,
                    DelayForEvent(drop, i)
                );
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

            InvokeWithDelay(legacyTarget, method, LegacyDelay());
        }
    }

    private void DispatchTeleports(
        bool drop,
        int playerId,
        string playerName)
    {
        string[] names = drop ? dropEventNames : primaryEventNames;
        PXR_UnifiedOutputs[] teleports = drop
            ? dropTeleportTargets
            : primaryTeleportTargets;

        if (names != null && names.Length > 0)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] != "TeleportPlayer")
                    continue;

                if (teleports == null || i >= teleports.Length)
                    continue;

                if (!Utilities.IsValid(teleports[i]))
                    continue;

                teleports[i].RequestTeleport(
                    playerId,
                    playerName,
                    transform.position,
                    DelayForEvent(drop, i)
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
                LegacyDelay()
            );
        }
    }

    private bool HasScope(bool drop, bool network)
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop ? dropEventNames : primaryEventNames;
        int[] scopes = drop ? dropEventScopes : primaryEventScopes;

        if (targets != null && targets.Length > 0)
        {
            if (names == null)
                return false;

            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]) ||
                    string.IsNullOrEmpty(names[i]))
                    continue;

                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

                if (scopes != null && i < scopes.Length)
                    scope = scopes[i];

                bool all = synced;

                if (scope == (int)PXR_EventScope.LocalOnly)
                    all = false;
                else if (scope == (int)PXR_EventScope.AllPlayers)
                    all = true;

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

        return Utilities.IsValid(legacy) &&
            !string.IsNullOrEmpty(legacyName) &&
            legacyName != "SpawnObject" &&
            legacyName != "TeleportPlayer" &&
            synced == network;
    }

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
    }

    public void LocalTrigger()
    {
        ExecuteEvent();
    }

    public void LocalDropTrigger()
    {
        ExecuteDropEvent();
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

    private void ExecuteList(bool drop, bool network)
    {
        UdonSharpBehaviour[] targets = drop
            ? dropEventTargets
            : primaryEventTargets;

        string[] names = drop ? dropEventNames : primaryEventNames;
        int[] scopes = drop ? dropEventScopes : primaryEventScopes;

        if (targets != null && targets.Length > 0)
        {
            if (names == null)
                return;

            int count = Mathf.Min(targets.Length, names.Length);

            for (int i = 0; i < count; i++)
            {
                if (!Utilities.IsValid(targets[i]) ||
                    string.IsNullOrEmpty(names[i]))
                    continue;

                // Routed separately; never execute twice.
                if (names[i] == "SpawnObject" ||
                    names[i] == "TeleportPlayer" ||
                    IsSyncedVideo(drop, i))
                {
                    continue;
                }

                int scope = 0;

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
                    );
                }
            }

            return;
        }

        UdonSharpBehaviour legacy = drop
            ? secondEventTarget
            : eventTarget;

        string legacyName = drop
            ? secondEventName
            : eventName;

        if (!Utilities.IsValid(legacy) ||
            string.IsNullOrEmpty(legacyName) ||
            legacyName == "TeleportPlayer" ||
            legacyName == "SpawnObject" ||
            synced != network)
        {
            return;
        }

        InvokeWithDelay(legacy, legacyName, LegacyDelay());
    }
}
