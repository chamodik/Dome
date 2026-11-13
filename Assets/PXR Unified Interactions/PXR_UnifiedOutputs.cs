
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDK3.Video.Components.AVPro;

public enum PXR_ComponentTargetType
{
    Behaviour = 0,
    Renderer = 1,
    Collider = 2
}

public enum PXR_AnimationParameterType
{
    Boolean = 0,
    Trigger = 1,
    Integer = 2
}

public enum PXR_TeleportMode
{
    TeleportUser = 0,
    TeleportByUsername = 1,
    TeleportByRange = 2,
    TeleportAll = 3
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_UnifiedOutputs : UdonSharpBehaviour
{
<<<<<<< Updated upstream
    // =====================================
    // TOGGLE OBJECT
    // =====================================

=======
    [Header("Toggle Object")]
>>>>>>> Stashed changes
    public GameObject toggleObjectTarget;
    public bool objectToggle = true;
    public bool objectState = true;
    public bool persistObjectState = false;

<<<<<<< Updated upstream
    // =====================================
    // DESTROY / DEACTIVATE
    // =====================================

    public GameObject destroyTarget;

    // =====================================
    // ANIMATION
    // =====================================

=======
    [Header("Destroy Object")]
    public GameObject destroyTarget;

    [Header("Animation")]
>>>>>>> Stashed changes
    public Animator targetAnimator;

    public PXR_AnimationParameterType animationParameterType =
        PXR_AnimationParameterType.Boolean;

    public string animationBoolName = "Activate";
    public bool animationToggleBool = false;
    public bool animationBoolValue = true;

    public string animationTriggerName = "Activate";
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    public string animationIntName = "State";
    public int animationIncrement = 1;
    public int animationMaxValue = 3;
    public int animationFixedValue = 0;
<<<<<<< Updated upstream

    public bool persistAnimatorState = false;

    // =====================================
    // SOUND
    // =====================================


    [Header("Sound")]
    public AudioSource targetAudioSource;

    // Ordered playlist. Supports one or more clips.
    // If empty, the AudioSource's existing clip
    // is used for backward compatibility.
    public AudioClip[] audioClips = new AudioClip[0];

    // Local, unsynchronized playback position.
    // Each client maintains its own counter.
    private int nextAudioClipIndex = 0;


    // =====================================
    // AVPRO
    // =====================================

    [Header("AVPro Video")]

=======
    public bool persistAnimatorState = false;

    [Header("Sound")]
    public AudioSource targetAudioSource;

    [Tooltip("Optional ordered playlist. If empty, use the AudioSource's assigned clip.")]
    public AudioClip[] audioClips = new AudioClip[0];

    private int nextAudioClipIndex = 0;

    [Header("AVPro Video")]
>>>>>>> Stashed changes
    public bool avProUseSyncedPrefab = true;
    public UdonBehaviour avProSyncPlayer;
    public VRCAVProVideoPlayer avProPlayer;
    public VRCUrl avProVideoUrl;

<<<<<<< Updated upstream
    // =====================================
    // TELEPORT
    // =====================================

=======
    [Header("Teleport")]
>>>>>>> Stashed changes
    public PXR_TeleportMode teleportMode =
        PXR_TeleportMode.TeleportUser;

    public string teleportUsername = "";
    public float teleportRange = 5f;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    public Transform[] teleportTargets;

    [HideInInspector]
    public Transform teleportTarget;

    [UdonSynced]
    private int teleportNextIndex;

    private int[] pendingPlayerIds;
    private string[] pendingNames;
    private Vector3[] pendingOrigins;
    private int[] pendingIndices;
    private float[] pendingTimes;
    private int pendingCount;
<<<<<<< Updated upstream

    // =====================================
    // SPAWN OBJECT
    // =====================================

    [Header("Spawn Object")]

    public GameObject spawnPrefab;
    public Transform spawnTransform;

    [Tooltip(
        "Velocity in Spawn Transform local axes, " +
        "in metres per second."
    )]
    public Vector3 velocity = Vector3.forward;

    [Header("Networked Pool Settings")]

=======
    private bool teleportRecheckPending;

    // Teleport network event rate limit (events per second, 1-100).
    // VRChat's default of 5/s queues rapid requests and delivers
    // them late.
    private const int TELEPORT_EVENT_RATE = 10;

    [Header("Spawn Object")]
    public GameObject spawnPrefab;
    public Transform spawnTransform;

    [Tooltip("Velocity in Spawn Transform local axes, in metres per second.")]
    public Vector3 velocity = Vector3.forward;

    [Header("Networked Pool Settings")]
>>>>>>> Stashed changes
    public PXR_PoolManager spawnPoolManager;

    public PXR_DespawnMode despawnMode =
        PXR_DespawnMode.DontDespawn;

    [Min(0.05f)]
    public float despawnAfterSeconds = 5f;

    [Min(0.02f)]
    public float collisionReturnDelay = 0.35f;

    private Transform[] movingObjects;
    private Vector3[] movingVelocities;
    private int movingCount;

<<<<<<< Updated upstream
    // =====================================
    // TOGGLE COMPONENT
    // =====================================

=======
    [Header("Toggle Component")]
>>>>>>> Stashed changes
    public PXR_ComponentTargetType componentTargetType =
        PXR_ComponentTargetType.Behaviour;

    public Behaviour targetBehaviour;
    public Renderer targetRenderer;
    public Collider targetCollider;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
    public bool componentToggle = true;
    public bool componentEnabled = true;
    public bool persistComponentState = false;

<<<<<<< Updated upstream
    // =====================================
    // CHAIN EVENT
    // =====================================

    public bool enableChainEvent = false;
    public UdonSharpBehaviour chainEventTarget;
    public string chainEventName = "";

    [Min(0f)]
    public float chainEventDelay = 0f;

    // =====================================
    // LEGACY REFERENCES
    // =====================================

    [HideInInspector]
    public GameObject targetObject;

    [HideInInspector]
    public GameObject enableObjectTarget;

    [HideInInspector]
    public GameObject disableObjectTarget;

    [HideInInspector]
    public GameObject destroyObjectTarget;

    [HideInInspector]
    public Component targetComponent;

    // =====================================
    // PERSISTENT STATES
    // =====================================

    [UdonSynced]
    private bool syncedObjectInitialized;

    [UdonSynced]
    private bool syncedObjectEnabled;

    [UdonSynced]
    private bool syncedComponentInitialized;

    [UdonSynced]
    private bool syncedComponentEnabled;

    [UdonSynced]
    private bool syncedAnimatorBoolInitialized;

    [UdonSynced]
    private bool syncedAnimatorBool;

    [UdonSynced]
    private bool syncedAnimatorIntInitialized;

    [UdonSynced]
    private int syncedAnimatorInt;

    // =====================================
    // INITIALIZATION
    // =====================================
=======
    // Legacy Inspector references remain for existing scenes.
    [HideInInspector] public GameObject targetObject;
    [HideInInspector] public GameObject enableObjectTarget;
    [HideInInspector] public GameObject disableObjectTarget;
    [HideInInspector] public GameObject destroyObjectTarget;
    [HideInInspector] public Component targetComponent;

    [UdonSynced] private bool syncedObjectInitialized;
    [UdonSynced] private bool syncedObjectEnabled;
    [UdonSynced] private bool syncedComponentInitialized;
    [UdonSynced] private bool syncedComponentEnabled;
    [UdonSynced] private bool syncedAnimatorBoolInitialized;
    [UdonSynced] private bool syncedAnimatorBool;
    [UdonSynced] private bool syncedAnimatorIntInitialized;
    [UdonSynced] private int syncedAnimatorInt;
>>>>>>> Stashed changes

    private void Start()
    {
        ApplyPersistentStates();
    }

    public override void OnDeserialization()
    {
        ApplyPersistentStates();
    }

    private void ApplyPersistentStates()
    {
<<<<<<< Updated upstream
        if (persistObjectState &&
            syncedObjectInitialized)
=======
        if (persistObjectState && syncedObjectInitialized)
>>>>>>> Stashed changes
        {
            GameObject obj = GetToggleObject();

            if (Utilities.IsValid(obj))
                obj.SetActive(syncedObjectEnabled);
        }

<<<<<<< Updated upstream
        if (persistComponentState &&
            syncedComponentInitialized)
        {
            ApplyComponentState(syncedComponentEnabled);
        }
=======
        if (persistComponentState && syncedComponentInitialized)
            ApplyComponentState(syncedComponentEnabled);
>>>>>>> Stashed changes

        if (persistAnimatorState &&
            Utilities.IsValid(targetAnimator))
        {
            if (syncedAnimatorBoolInitialized &&
                !string.IsNullOrEmpty(animationBoolName))
            {
                targetAnimator.SetBool(
                    animationBoolName,
                    syncedAnimatorBool
                );
            }

            if (syncedAnimatorIntInitialized &&
                !string.IsNullOrEmpty(animationIntName))
            {
                targetAnimator.SetInteger(
                    animationIntName,
                    syncedAnimatorInt
                );
            }
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // CHAIN EVENT
    // =====================================

    private void RunChainEvent()
    {
        if (!enableChainEvent)
            return;

        if (!Utilities.IsValid(chainEventTarget))
            return;

        if (string.IsNullOrEmpty(chainEventName))
            return;

        if (chainEventTarget == this)
            return;

        if (chainEventDelay > 0f)
        {
            chainEventTarget.SendCustomEventDelayedSeconds(
                chainEventName,
                chainEventDelay
            );
        } else
        {
            chainEventTarget.SendCustomEvent(
                chainEventName
            );
        }
    }

    [PXR_InputCallable]
    public void ChainEvent()
    {
        RunChainEvent();
    }

    // =====================================
    // OBJECT FUNCTIONS
    // =====================================

=======
>>>>>>> Stashed changes
    private GameObject GetToggleObject()
    {
        if (Utilities.IsValid(toggleObjectTarget))
            return toggleObjectTarget;

        return targetObject;
    }

    [PXR_InputCallable]
    public void ToggleObject()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        GameObject obj = GetToggleObject();

        if (!Utilities.IsValid(obj))
            return;

        if (persistObjectState)
        {
            if (!Networking.IsOwner(gameObject))
                return;

            bool previous = syncedObjectInitialized
                ? syncedObjectEnabled
                : obj.activeSelf;

            syncedObjectEnabled = objectToggle
                ? !previous
                : objectState;

            syncedObjectInitialized = true;

            obj.SetActive(syncedObjectEnabled);
            RequestSerialization();
            return;
        }

        obj.SetActive(
            objectToggle ? !obj.activeSelf : objectState
        );
    }

    public void EnableObject()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        GameObject obj = enableObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

        if (Utilities.IsValid(obj))
            obj.SetActive(true);
    }

    public void DisableObject()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        GameObject obj = disableObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

        if (Utilities.IsValid(obj))
            obj.SetActive(false);
    }

    [PXR_InputCallable]
    public void DestroyObject()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        GameObject obj = destroyTarget;

        if (!Utilities.IsValid(obj))
            obj = destroyObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

<<<<<<< Updated upstream
        // Logical removal, not network destruction.
=======
        // Local deactivation, not permanent network destruction.
>>>>>>> Stashed changes
        if (Utilities.IsValid(obj))
            obj.SetActive(false);
    }

<<<<<<< Updated upstream
    // =====================================
    // ANIMATION
    // =====================================

    [PXR_InputCallable]
    public void PlayAnimation()
    {
        RunChainEvent();

=======
    [PXR_InputCallable]
    public void PlayAnimation()
    {
>>>>>>> Stashed changes
        if (!Utilities.IsValid(targetAnimator))
            return;

        if (animationParameterType ==
            PXR_AnimationParameterType.Trigger)
        {
            if (!string.IsNullOrEmpty(animationTriggerName))
                targetAnimator.SetTrigger(animationTriggerName);

            return;
        }

        if (animationParameterType ==
            PXR_AnimationParameterType.Boolean)
        {
            if (string.IsNullOrEmpty(animationBoolName))
                return;

            if (persistAnimatorState)
            {
                if (!Networking.IsOwner(gameObject))
                    return;

                bool previous = syncedAnimatorBoolInitialized
                    ? syncedAnimatorBool
                    : targetAnimator.GetBool(animationBoolName);

                syncedAnimatorBool = animationToggleBool
                    ? !previous
                    : animationBoolValue;

                syncedAnimatorBoolInitialized = true;

                targetAnimator.SetBool(
                    animationBoolName,
                    syncedAnimatorBool
                );

                RequestSerialization();
                return;
            }

            bool next = animationToggleBool
                ? !targetAnimator.GetBool(animationBoolName)
                : animationBoolValue;

<<<<<<< Updated upstream
            targetAnimator.SetBool(
                animationBoolName,
                next
            );
=======
            targetAnimator.SetBool(animationBoolName, next);
>>>>>>> Stashed changes
            return;
        }

        if (animationParameterType ==
            PXR_AnimationParameterType.Integer)
        {
            if (string.IsNullOrEmpty(animationIntName))
                return;

            if (persistAnimatorState &&
                !Networking.IsOwner(gameObject))
                return;

            int previous =
                targetAnimator.GetInteger(animationIntName);

            if (persistAnimatorState &&
                syncedAnimatorIntInitialized)
<<<<<<< Updated upstream
                previous = syncedAnimatorInt;
=======
            {
                previous = syncedAnimatorInt;
            }
>>>>>>> Stashed changes

            int nextValue = animationFixedValue;

            if (animationIncrement != 0)
            {
<<<<<<< Updated upstream
                // Floating intermediate avoids overflow.
=======
>>>>>>> Stashed changes
                double next =
                    (double)previous + animationIncrement;

                int maxValue =
                    Mathf.Max(0, animationMaxValue);

<<<<<<< Updated upstream
                nextValue = (next < 0 || next > maxValue)
                    ? 0
                    : (int)next;
=======
                nextValue =
                    next < 0 || next > maxValue
                        ? 0
                        : (int)next;
>>>>>>> Stashed changes
            }

            targetAnimator.SetInteger(
                animationIntName,
                nextValue
            );

            if (persistAnimatorState)
            {
                syncedAnimatorInt = nextValue;
                syncedAnimatorIntInitialized = true;
                RequestSerialization();
            }
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // SOUND
    // =====================================


    [PXR_InputCallable]
    public void PlaySound()
    {
        // Validate the assigned Unity AudioSource.
=======
    [PXR_InputCallable]
    public void PlaySound()
    {
>>>>>>> Stashed changes
        if (!Utilities.IsValid(targetAudioSource))
        {
            Debug.LogWarning(
                "[PXR_UnifiedOutputs] PlaySound: " +
                "No AudioSource assigned."
            );
            return;
        }

<<<<<<< Updated upstream
        // ----------------------------------------
        // LEGACY MODE
        // ----------------------------------------
        //
        // If the new playlist is empty, continue
        // using the AudioClip already assigned
        // directly to the Unity AudioSource.
        //
        // Always stop before playing so another
        // interaction restarts the current clip.

        if (audioClips == null ||
            audioClips.Length == 0)
=======
        // Backward compatibility: use the AudioSource clip
        // when the playlist is empty.
        if (audioClips == null || audioClips.Length == 0)
>>>>>>> Stashed changes
        {
            if (!Utilities.IsValid(targetAudioSource.clip))
            {
                Debug.LogWarning(
                    "[PXR_UnifiedOutputs] PlaySound: " +
                    "No AudioClip configured."
                );
                return;
            }

            targetAudioSource.Stop();
            targetAudioSource.Play();
            return;
        }

<<<<<<< Updated upstream
        // ----------------------------------------
        // PLAYLIST MODE
        // ----------------------------------------

        int count = audioClips.Length;

        // Protect against an out-of-range counter
        // if the array's size has changed.
=======
        int count = audioClips.Length;
>>>>>>> Stashed changes
        int startIndex = nextAudioClipIndex % count;

        if (startIndex < 0)
            startIndex = 0;

        int selectedIndex = -1;

<<<<<<< Updated upstream
        // Find the next valid AudioClip.
        //
        // Empty playlist entries are skipped,
        // without blocking subsequent playback.

=======
>>>>>>> Stashed changes
        for (int offset = 0; offset < count; offset++)
        {
            int index = (startIndex + offset) % count;

            if (Utilities.IsValid(audioClips[index]))
            {
                selectedIndex = index;
                break;
            }
        }

<<<<<<< Updated upstream
        // Nothing playable in the array.
=======
>>>>>>> Stashed changes
        if (selectedIndex < 0)
        {
            Debug.LogWarning(
                "[PXR_UnifiedOutputs] PlaySound: " +
                "All AudioClip entries are empty."
            );
            return;
        }

<<<<<<< Updated upstream
        // ----------------------------------------
        // STOP PREVIOUS AUDIO
        // ----------------------------------------
        //
        // Stops the currently playing clip,
        // regardless of whether it has finished.

        targetAudioSource.Stop();

        // ----------------------------------------
        // ASSIGN NEXT CLIP
        // ----------------------------------------

=======
        targetAudioSource.Stop();

>>>>>>> Stashed changes
        AudioClip selectedClip =
            audioClips[selectedIndex];

        targetAudioSource.clip = selectedClip;
<<<<<<< Updated upstream

        // ----------------------------------------
        // PLAY SELECTED CLIP
        // ----------------------------------------

        targetAudioSource.Play();

        // ----------------------------------------
        // ADVANCE PLAYLIST
        // ----------------------------------------
        //
        // After the last element, wrap to zero.
        //
        // This counter remains local to each
        // client. UnifiedInputs determines
        // whether PlaySound runs locally or
        // on all players.

        nextAudioClipIndex =
            (selectedIndex + 1) % count;
    }


    // =====================================
    // AVPRO
    // =====================================

    [PXR_InputCallable]
    public void PlayAVProVideo()
    {
        RunChainEvent();

=======
        targetAudioSource.Play();

        // Playlist position remains local to each client.
        nextAudioClipIndex = (selectedIndex + 1) % count;
    }

    [PXR_InputCallable]
    public void PlayAVProVideo()
    {
>>>>>>> Stashed changes
        if (VRCUrl.IsNullOrEmpty(avProVideoUrl))
            return;

        if (avProUseSyncedPrefab)
        {
            if (!Utilities.IsValid(avProSyncPlayer))
                return;

<<<<<<< Updated upstream
            // This method is intended to be called
            // once by the initiating client's Input.
=======
>>>>>>> Stashed changes
            SendCustomNetworkEvent(
                NetworkEventTarget.All,
                nameof(ApplyAVProVideoOnOwner)
            );
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
            return;
        }

        if (Utilities.IsValid(avProPlayer))
            avProPlayer.PlayURL(avProVideoUrl);
    }

    [NetworkCallable]
    public void ApplyAVProVideoOnOwner()
    {
<<<<<<< Updated upstream
        if (!avProUseSyncedPrefab)
            return;

        if (!Utilities.IsValid(avProSyncPlayer))
            return;

        if (VRCUrl.IsNullOrEmpty(avProVideoUrl))
            return;

        if (!Networking.IsOwner(avProSyncPlayer.gameObject))
            return;
=======
        if (!avProUseSyncedPrefab ||
            !Utilities.IsValid(avProSyncPlayer) ||
            VRCUrl.IsNullOrEmpty(avProVideoUrl) ||
            !Networking.IsOwner(avProSyncPlayer.gameObject))
        {
            return;
        }
>>>>>>> Stashed changes

        avProSyncPlayer.SetProgramVariable(
            "url",
            avProVideoUrl
        );

        avProSyncPlayer.RequestSerialization();
    }

<<<<<<< Updated upstream
    // =====================================
    // TELEPORT ENTRY POINT
    // =====================================

=======
    // Direct method remains available for compatibility.
    // UnifiedInputs normally uses RequestTeleport instead.
>>>>>>> Stashed changes
    [PXR_InputCallable]
    public void TeleportPlayer()
    {
        VRCPlayerApi player = Networking.LocalPlayer;

        if (!Utilities.IsValid(player))
            return;

        RequestTeleport(
            player.playerId,
            player.displayName,
            transform.position,
            0f
        );
    }

    public void RequestTeleport(
        int playerId,
        string playerName,
        Vector3 sourcePosition,
        float delay)
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerTeleportRequest),
            playerId,
            playerName,
            sourcePosition,
            Mathf.Max(0f, delay)
        );
    }

<<<<<<< Updated upstream
    [NetworkCallable]
=======
    [NetworkCallable(TELEPORT_EVENT_RATE)]
>>>>>>> Stashed changes
    public void OwnerTeleportRequest(
        int playerId,
        string playerName,
        Vector3 sourcePosition,
        float delay)
    {
<<<<<<< Updated upstream
        if (!Networking.IsOwner(gameObject))
            return;

        if (!HasTeleportDestination())
=======
        if (!Networking.IsOwner(gameObject) ||
            !HasTeleportDestination())
>>>>>>> Stashed changes
            return;

        int selected = teleportNextIndex;

        teleportNextIndex =
            teleportNextIndex == int.MaxValue
                ? 0
                : teleportNextIndex + 1;

        RequestSerialization();

        SendCustomNetworkEvent(
            NetworkEventTarget.All,
            nameof(NetworkTeleport),
            playerId,
            playerName,
            sourcePosition,
            selected,
            delay
        );
    }

<<<<<<< Updated upstream
    [NetworkCallable]
=======
    [NetworkCallable(TELEPORT_EVENT_RATE)]
>>>>>>> Stashed changes
    public void NetworkTeleport(
        int playerId,
        string playerName,
        Vector3 sourcePosition,
        int destinationIndex,
        float delay)
    {
        if (delay <= 0f)
        {
            ExecuteTeleport(
                playerId,
                playerName,
                sourcePosition,
                destinationIndex
            );
        } else
        {
            QueueTeleport(
                playerId,
                playerName,
                sourcePosition,
                destinationIndex,
                delay
            );
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // DELAYED TELEPORT QUEUE
    // =====================================

=======
>>>>>>> Stashed changes
    private void QueueTeleport(
        int playerId,
        string playerName,
        Vector3 origin,
        int index,
        float delay)
    {
        if (pendingPlayerIds == null)
        {
            pendingPlayerIds = new int[8];
            pendingNames = new string[8];
            pendingOrigins = new Vector3[8];
            pendingIndices = new int[8];
            pendingTimes = new float[8];
        }

        if (pendingCount >= pendingPlayerIds.Length)
        {
            int capacity = pendingPlayerIds.Length * 2;

            int[] ids = new int[capacity];
            string[] names = new string[capacity];
            Vector3[] origins = new Vector3[capacity];
            int[] indices = new int[capacity];
            float[] times = new float[capacity];

            for (int i = 0; i < pendingCount; i++)
            {
                ids[i] = pendingPlayerIds[i];
                names[i] = pendingNames[i];
                origins[i] = pendingOrigins[i];
                indices[i] = pendingIndices[i];
                times[i] = pendingTimes[i];
            }

            pendingPlayerIds = ids;
            pendingNames = names;
            pendingOrigins = origins;
            pendingIndices = indices;
            pendingTimes = times;
        }

        pendingPlayerIds[pendingCount] = playerId;
        pendingNames[pendingCount] = playerName;
        pendingOrigins[pendingCount] = origin;
        pendingIndices[pendingCount] = index;
        pendingTimes[pendingCount] = Time.time + delay;
        pendingCount++;

        SendCustomEventDelayedSeconds(
            nameof(ProcessTeleportQueue),
            delay
        );
    }

    public void ProcessTeleportQueue()
    {
<<<<<<< Updated upstream
        float now = Time.time;
=======
        // Small tolerance for scheduler timing jitter.
        float now = Time.time + 0.02f;
>>>>>>> Stashed changes
        int i = 0;

        while (i < pendingCount)
        {
            if (pendingTimes[i] > now)
            {
                i++;
                continue;
            }

            int playerId = pendingPlayerIds[i];
            string name = pendingNames[i];
            Vector3 origin = pendingOrigins[i];
            int index = pendingIndices[i];

            int last = pendingCount - 1;

            pendingPlayerIds[i] = pendingPlayerIds[last];
            pendingNames[i] = pendingNames[last];
            pendingOrigins[i] = pendingOrigins[last];
            pendingIndices[i] = pendingIndices[last];
            pendingTimes[i] = pendingTimes[last];

            pendingNames[last] = null;
            pendingCount--;

            ExecuteTeleport(
                playerId,
                name,
                origin,
                index
            );
        }
<<<<<<< Updated upstream
=======

        // Backstop: if anything is still waiting, check again
        // when the soonest remaining entry is due.
        if (pendingCount > 0 && !teleportRecheckPending)
        {
            float soonest = pendingTimes[0];

            for (int j = 1; j < pendingCount; j++)
            {
                if (pendingTimes[j] < soonest)
                    soonest = pendingTimes[j];
            }

            teleportRecheckPending = true;

            SendCustomEventDelayedSeconds(
                nameof(_TeleportQueueRecheck),
                Mathf.Max(0.02f, soonest - Time.time)
            );
        }
    }

    public void _TeleportQueueRecheck()
    {
        teleportRecheckPending = false;
        ProcessTeleportQueue();
>>>>>>> Stashed changes
    }

    private void ExecuteTeleport(
        int playerId,
        string playerName,
        Vector3 origin,
        int destinationIndex)
    {
        VRCPlayerApi local = Networking.LocalPlayer;

        if (!Utilities.IsValid(local))
            return;

        bool eligible = false;

        if (teleportMode == PXR_TeleportMode.TeleportUser)
        {
<<<<<<< Updated upstream
            eligible = playerId >= 0 &&
                       local.playerId == playerId;
=======
            eligible =
                playerId >= 0 &&
                local.playerId == playerId;
>>>>>>> Stashed changes

            if (eligible &&
                !string.IsNullOrEmpty(playerName))
            {
                eligible =
                    local.displayName == playerName;
            }
        } else if (teleportMode ==
                   PXR_TeleportMode.TeleportByUsername)
        {
            eligible =
                !string.IsNullOrEmpty(teleportUsername) &&
                local.displayName == teleportUsername;
        } else if (teleportMode ==
                   PXR_TeleportMode.TeleportByRange)
        {
<<<<<<< Updated upstream
            float range =
                Mathf.Max(0f, teleportRange);

            Vector3 delta =
                local.GetPosition() - origin;

            eligible =
                delta.sqrMagnitude <= range * range;
=======
            float range = Mathf.Max(0f, teleportRange);

            Vector3 delta = local.GetPosition() - origin;
            eligible = delta.sqrMagnitude <= range * range;
>>>>>>> Stashed changes
        } else if (teleportMode ==
                   PXR_TeleportMode.TeleportAll)
        {
            eligible = true;
        }

        if (!eligible)
            return;

        Transform destination =
            GetTeleportDestination(destinationIndex);

        if (Utilities.IsValid(destination))
        {
            local.TeleportTo(
                destination.position,
                destination.rotation
            );
        }
    }

    private bool HasTeleportDestination()
    {
        if (teleportTargets != null)
        {
            for (int i = 0; i < teleportTargets.Length; i++)
            {
                if (Utilities.IsValid(teleportTargets[i]))
                    return true;
            }
        }

        return Utilities.IsValid(teleportTarget);
    }

    private Transform GetTeleportDestination(int index)
    {
        int count = 0;

        if (teleportTargets != null)
        {
            for (int i = 0; i < teleportTargets.Length; i++)
            {
                if (Utilities.IsValid(teleportTargets[i]))
                    count++;
            }
        }

        if (count == 0)
            return teleportTarget;

        int selected = index % count;
        int current = 0;

        for (int i = 0; i < teleportTargets.Length; i++)
        {
            if (!Utilities.IsValid(teleportTargets[i]))
                continue;

            if (current == selected)
                return teleportTargets[i];

            current++;
        }

        return null;
    }

<<<<<<< Updated upstream
    // =====================================
    // LOCAL NON-PHYSICS SPAWNING
    // =====================================

    [PXR_InputCallable]
    public void SpawnObject()
    {
        RunChainEvent();

=======
    [PXR_InputCallable]
    public void SpawnObject()
    {
>>>>>>> Stashed changes
        if (!Utilities.IsValid(spawnPrefab) ||
            !Utilities.IsValid(spawnTransform))
            return;

<<<<<<< Updated upstream
        GameObject spawned =
            VRCInstantiate(spawnPrefab);
=======
        // Same Udon extern as the obsolete VRCInstantiate().
        GameObject spawned = Instantiate(spawnPrefab);
>>>>>>> Stashed changes

        if (!Utilities.IsValid(spawned))
            return;

        spawned.transform.SetPositionAndRotation(
            spawnTransform.position,
            spawnTransform.rotation
        );

        spawned.SetActive(true);

<<<<<<< Updated upstream
=======
        ScheduleLocalDespawn(spawned);

>>>>>>> Stashed changes
        if (velocity.sqrMagnitude <= 0f)
            return;

        Vector3 worldVelocity =
            spawnTransform.TransformDirection(velocity);

        RegisterMovingObject(
            spawned.transform,
            worldVelocity
        );
    }

<<<<<<< Updated upstream
    // =====================================
    // POOLED NON-PHYSICS SPAWNING
    // =====================================

    // Invoked internally when Inputs.Synced = true.

    public void SpawnPooledObject()
    {
        RunChainEvent();

=======
    // Invoked by UnifiedInputs for Synced spawning.
    public void SpawnPooledObject()
    {
>>>>>>> Stashed changes
        if (!Utilities.IsValid(spawnPoolManager) ||
            !Utilities.IsValid(spawnTransform))
            return;

        Vector3 worldVelocity =
            spawnTransform.TransformDirection(velocity);

        spawnPoolManager.RequestSpawn(
            false,
            spawnTransform.position,
            spawnTransform.rotation,
            worldVelocity,
            (int)despawnMode,
            despawnAfterSeconds,
            collisionReturnDelay
        );
    }

<<<<<<< Updated upstream
    // =====================================
    // LOCAL MOVEMENT REGISTRATION
    // =====================================

=======
>>>>>>> Stashed changes
    private void RegisterMovingObject(
        Transform obj,
        Vector3 worldVelocity)
    {
        if (movingObjects == null)
        {
            movingObjects = new Transform[8];
            movingVelocities = new Vector3[8];
        }

        if (movingCount >= movingObjects.Length)
        {
            int capacity = movingObjects.Length * 2;

<<<<<<< Updated upstream
            Transform[] objects =
                new Transform[capacity];

            Vector3[] velocities =
                new Vector3[capacity];
=======
            Transform[] objects = new Transform[capacity];
            Vector3[] velocities = new Vector3[capacity];
>>>>>>> Stashed changes

            for (int i = 0; i < movingCount; i++)
            {
                objects[i] = movingObjects[i];
                velocities[i] = movingVelocities[i];
            }

            movingObjects = objects;
            movingVelocities = velocities;
        }

        movingObjects[movingCount] = obj;
        movingVelocities[movingCount] = worldVelocity;
        movingCount++;
<<<<<<< Updated upstream
    }

    // =====================================
    // COMPONENT HELPERS
    // =====================================
=======

        StartMoveLoop();
    }

    // =========================================
    // LOCAL SPAWN DESPAWN (SYNCED OFF)
    // =========================================

    // Despawn After Time also applies to local spawns.
    // Don't Despawn and Despawn on Collision leave local
    // spawns untouched, exactly as before.

    private GameObject[] localDespawnObjects;
    private float[] localDespawnTimes;
    private int localDespawnCount;

    private void ScheduleLocalDespawn(GameObject obj)
    {
        if (despawnMode != PXR_DespawnMode.AfterTime ||
            !Utilities.IsValid(obj))
            return;

        float lifetime = Mathf.Max(0.05f, despawnAfterSeconds);

        if (localDespawnObjects == null)
        {
            localDespawnObjects = new GameObject[8];
            localDespawnTimes = new float[8];
        }

        if (localDespawnCount >= localDespawnObjects.Length)
        {
            int capacity = localDespawnObjects.Length * 2;

            GameObject[] objects = new GameObject[capacity];
            float[] times = new float[capacity];

            for (int i = 0; i < localDespawnCount; i++)
            {
                objects[i] = localDespawnObjects[i];
                times[i] = localDespawnTimes[i];
            }

            localDespawnObjects = objects;
            localDespawnTimes = times;
        }

        localDespawnObjects[localDespawnCount] = obj;
        localDespawnTimes[localDespawnCount] = Time.time + lifetime;
        localDespawnCount++;

        SendCustomEventDelayedSeconds(
            nameof(_ProcessLocalDespawns),
            lifetime
        );
    }

    public void _ProcessLocalDespawns()
    {
        // Small tolerance for scheduler timing jitter.
        float now = Time.time + 0.05f;
        int i = 0;

        while (i < localDespawnCount)
        {
            if (localDespawnTimes[i] > now)
            {
                i++;
                continue;
            }

            GameObject obj = localDespawnObjects[i];
            int last = localDespawnCount - 1;

            localDespawnObjects[i] = localDespawnObjects[last];
            localDespawnTimes[i] = localDespawnTimes[last];
            localDespawnObjects[last] = null;
            localDespawnCount--;

            // The movement loop drops destroyed objects itself.
            if (Utilities.IsValid(obj))
                Destroy(obj);
        }
    }
>>>>>>> Stashed changes

    private bool HasComponentTarget()
    {
        if (componentTargetType ==
            PXR_ComponentTargetType.Behaviour)
<<<<<<< Updated upstream
            return Utilities.IsValid(targetBehaviour);

        if (componentTargetType ==
            PXR_ComponentTargetType.Renderer)
            return Utilities.IsValid(targetRenderer);
=======
        {
            return Utilities.IsValid(targetBehaviour);
        }

        if (componentTargetType ==
            PXR_ComponentTargetType.Renderer)
        {
            return Utilities.IsValid(targetRenderer);
        }
>>>>>>> Stashed changes

        return Utilities.IsValid(targetCollider);
    }

    private bool CurrentComponentState()
    {
        if (componentTargetType ==
            PXR_ComponentTargetType.Behaviour)
        {
            if (Utilities.IsValid(targetBehaviour))
                return targetBehaviour.enabled;
        } else if (componentTargetType ==
                   PXR_ComponentTargetType.Renderer)
        {
            if (Utilities.IsValid(targetRenderer))
                return targetRenderer.enabled;
        } else
        {
            if (Utilities.IsValid(targetCollider))
                return targetCollider.enabled;
        }

        return false;
    }

    private void ApplyComponentState(bool value)
    {
        if (componentTargetType ==
            PXR_ComponentTargetType.Behaviour)
        {
            if (Utilities.IsValid(targetBehaviour))
                targetBehaviour.enabled = value;
        } else if (componentTargetType ==
                   PXR_ComponentTargetType.Renderer)
        {
            if (Utilities.IsValid(targetRenderer))
                targetRenderer.enabled = value;
        } else
        {
            if (Utilities.IsValid(targetCollider))
                targetCollider.enabled = value;
        }
    }

    [PXR_InputCallable]
    public void ToggleComponent()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        if (!HasComponentTarget())
            return;

        if (persistComponentState)
        {
            if (!Networking.IsOwner(gameObject))
                return;

            bool previous = syncedComponentInitialized
                ? syncedComponentEnabled
                : CurrentComponentState();

            syncedComponentEnabled = componentToggle
                ? !previous
                : componentEnabled;

            syncedComponentInitialized = true;

            ApplyComponentState(syncedComponentEnabled);
            RequestSerialization();
            return;
        }

        bool value = componentToggle
            ? !CurrentComponentState()
            : componentEnabled;

        ApplyComponentState(value);
    }

<<<<<<< Updated upstream
    // Legacy wrappers.
    public void EnableComponent()
    {
        RunChainEvent();
=======
    // Legacy wrappers remain available.
    public void EnableComponent()
    {
>>>>>>> Stashed changes
        ApplyComponentState(true);
    }

    public void DisableComponent()
    {
<<<<<<< Updated upstream
        RunChainEvent();
=======
>>>>>>> Stashed changes
        ApplyComponentState(false);
    }

    public void SetComponentEnabled()
    {
<<<<<<< Updated upstream
        RunChainEvent();
        ApplyComponentState(componentEnabled);
    }

    // =====================================
    // LOCAL RUNTIME UPDATE
    // =====================================

    private void Update()
    {
        if (pendingCount > 0)
            ProcessTeleportQueue();

=======
        ApplyComponentState(componentEnabled);
    }

    // =========================================
    // MOVING LOCAL SPAWNS
    // =========================================

    // Moving local spawns are stepped once per frame by a
    // one-frame delayed event that runs only while objects are
    // moving. There is no Update(), so idle Outputs components
    // cost nothing per frame. Delayed teleports are processed by
    // their own delayed events (see QueueTeleport).

    private bool moveLoopRunning;
    private int lastMoveFrame = -1;

    private void StartMoveLoop()
    {
        if (moveLoopRunning &&
            Time.frameCount - lastMoveFrame <= 2)
            return;

        moveLoopRunning = true;

        SendCustomEventDelayedFrames(
            nameof(_MoveTick),
            1
        );
    }

    private void OnEnable()
    {
        if (movingCount > 0)
            StartMoveLoop();

        if (pendingCount > 0)
            ProcessTeleportQueue();
    }

    public void _MoveTick()
    {
        // A second loop started in the same frame stops here.
        if (Time.frameCount == lastMoveFrame)
            return;

        lastMoveFrame = Time.frameCount;

        // Matches the old Update(): no movement while this
        // component's GameObject is inactive. OnEnable restarts.
        if (!gameObject.activeInHierarchy)
        {
            moveLoopRunning = false;
            return;
        }

        StepMovingObjects();

        if (movingCount > 0)
        {
            SendCustomEventDelayedFrames(
                nameof(_MoveTick),
                1
            );
        } else
        {
            moveLoopRunning = false;
        }
    }

    private void StepMovingObjects()
    {
>>>>>>> Stashed changes
        int i = 0;

        while (i < movingCount)
        {
            Transform obj = movingObjects[i];

            if (!Utilities.IsValid(obj))
            {
                int last = movingCount - 1;

                movingObjects[i] = movingObjects[last];
<<<<<<< Updated upstream
                movingVelocities[i] =
                    movingVelocities[last];
=======
                movingVelocities[i] = movingVelocities[last];
>>>>>>> Stashed changes

                movingObjects[last] = null;
                movingCount--;
                continue;
            }

            if (obj.gameObject.activeInHierarchy)
            {
                obj.position +=
                    movingVelocities[i] * Time.deltaTime;
            }

            i++;
        }
    }
}
