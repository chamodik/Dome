
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
    // =====================================
    // TOGGLE OBJECT
    // =====================================

    public GameObject toggleObjectTarget;
    public bool objectToggle = true;
    public bool objectState = true;
    public bool persistObjectState = false;

    // =====================================
    // DESTROY / DEACTIVATE
    // =====================================

    public GameObject destroyTarget;

    // =====================================
    // ANIMATION
    // =====================================

    public Animator targetAnimator;

    public PXR_AnimationParameterType animationParameterType =
        PXR_AnimationParameterType.Boolean;

    public string animationBoolName = "Activate";
    public bool animationToggleBool = false;
    public bool animationBoolValue = true;

    public string animationTriggerName = "Activate";

    public string animationIntName = "State";
    public int animationIncrement = 1;
    public int animationMaxValue = 3;
    public int animationFixedValue = 0;

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

    public bool avProUseSyncedPrefab = true;
    public UdonBehaviour avProSyncPlayer;
    public VRCAVProVideoPlayer avProPlayer;
    public VRCUrl avProVideoUrl;

    // =====================================
    // TELEPORT
    // =====================================

    public PXR_TeleportMode teleportMode =
        PXR_TeleportMode.TeleportUser;

    public string teleportUsername = "";
    public float teleportRange = 5f;

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

    // =====================================
    // TOGGLE COMPONENT
    // =====================================

    public PXR_ComponentTargetType componentTargetType =
        PXR_ComponentTargetType.Behaviour;

    public Behaviour targetBehaviour;
    public Renderer targetRenderer;
    public Collider targetCollider;

    public bool componentToggle = true;
    public bool componentEnabled = true;
    public bool persistComponentState = false;

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
        if (persistObjectState &&
            syncedObjectInitialized)
        {
            GameObject obj = GetToggleObject();

            if (Utilities.IsValid(obj))
                obj.SetActive(syncedObjectEnabled);
        }

        if (persistComponentState &&
            syncedComponentInitialized)
        {
            ApplyComponentState(syncedComponentEnabled);
        }

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

    private GameObject GetToggleObject()
    {
        if (Utilities.IsValid(toggleObjectTarget))
            return toggleObjectTarget;

        return targetObject;
    }

    [PXR_InputCallable]
    public void ToggleObject()
    {
        RunChainEvent();

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
        RunChainEvent();

        GameObject obj = enableObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

        if (Utilities.IsValid(obj))
            obj.SetActive(true);
    }

    public void DisableObject()
    {
        RunChainEvent();

        GameObject obj = disableObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

        if (Utilities.IsValid(obj))
            obj.SetActive(false);
    }

    [PXR_InputCallable]
    public void DestroyObject()
    {
        RunChainEvent();

        GameObject obj = destroyTarget;

        if (!Utilities.IsValid(obj))
            obj = destroyObjectTarget;

        if (!Utilities.IsValid(obj))
            obj = targetObject;

        // Logical removal, not network destruction.
        if (Utilities.IsValid(obj))
            obj.SetActive(false);
    }

    // =====================================
    // ANIMATION
    // =====================================

    [PXR_InputCallable]
    public void PlayAnimation()
    {
        RunChainEvent();

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

            targetAnimator.SetBool(
                animationBoolName,
                next
            );
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
                previous = syncedAnimatorInt;

            int nextValue = animationFixedValue;

            if (animationIncrement != 0)
            {
                // Floating intermediate avoids overflow.
                double next =
                    (double)previous + animationIncrement;

                int maxValue =
                    Mathf.Max(0, animationMaxValue);

                nextValue = (next < 0 || next > maxValue)
                    ? 0
                    : (int)next;
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

    // =====================================
    // SOUND
    // =====================================


    [PXR_InputCallable]
    public void PlaySound()
    {
        // Validate the assigned Unity AudioSource.
        if (!Utilities.IsValid(targetAudioSource))
        {
            Debug.LogWarning(
                "[PXR_UnifiedOutputs] PlaySound: " +
                "No AudioSource assigned."
            );
            return;
        }

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

        // ----------------------------------------
        // PLAYLIST MODE
        // ----------------------------------------

        int count = audioClips.Length;

        // Protect against an out-of-range counter
        // if the array's size has changed.
        int startIndex = nextAudioClipIndex % count;

        if (startIndex < 0)
            startIndex = 0;

        int selectedIndex = -1;

        // Find the next valid AudioClip.
        //
        // Empty playlist entries are skipped,
        // without blocking subsequent playback.

        for (int offset = 0; offset < count; offset++)
        {
            int index = (startIndex + offset) % count;

            if (Utilities.IsValid(audioClips[index]))
            {
                selectedIndex = index;
                break;
            }
        }

        // Nothing playable in the array.
        if (selectedIndex < 0)
        {
            Debug.LogWarning(
                "[PXR_UnifiedOutputs] PlaySound: " +
                "All AudioClip entries are empty."
            );
            return;
        }

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

        AudioClip selectedClip =
            audioClips[selectedIndex];

        targetAudioSource.clip = selectedClip;

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

        if (VRCUrl.IsNullOrEmpty(avProVideoUrl))
            return;

        if (avProUseSyncedPrefab)
        {
            if (!Utilities.IsValid(avProSyncPlayer))
                return;

            // This method is intended to be called
            // once by the initiating client's Input.
            SendCustomNetworkEvent(
                NetworkEventTarget.All,
                nameof(ApplyAVProVideoOnOwner)
            );
            return;
        }

        if (Utilities.IsValid(avProPlayer))
            avProPlayer.PlayURL(avProVideoUrl);
    }

    [NetworkCallable]
    public void ApplyAVProVideoOnOwner()
    {
        if (!avProUseSyncedPrefab)
            return;

        if (!Utilities.IsValid(avProSyncPlayer))
            return;

        if (VRCUrl.IsNullOrEmpty(avProVideoUrl))
            return;

        if (!Networking.IsOwner(avProSyncPlayer.gameObject))
            return;

        avProSyncPlayer.SetProgramVariable(
            "url",
            avProVideoUrl
        );

        avProSyncPlayer.RequestSerialization();
    }

    // =====================================
    // TELEPORT ENTRY POINT
    // =====================================

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
        RunChainEvent();

        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerTeleportRequest),
            playerId,
            playerName,
            sourcePosition,
            Mathf.Max(0f, delay)
        );
    }

    [NetworkCallable]
    public void OwnerTeleportRequest(
        int playerId,
        string playerName,
        Vector3 sourcePosition,
        float delay)
    {
        if (!Networking.IsOwner(gameObject))
            return;

        if (!HasTeleportDestination())
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

    [NetworkCallable]
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

    // =====================================
    // DELAYED TELEPORT QUEUE
    // =====================================

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
        float now = Time.time;
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
            eligible = playerId >= 0 &&
                       local.playerId == playerId;

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
            float range =
                Mathf.Max(0f, teleportRange);

            Vector3 delta =
                local.GetPosition() - origin;

            eligible =
                delta.sqrMagnitude <= range * range;
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

    // =====================================
    // LOCAL NON-PHYSICS SPAWNING
    // =====================================

    [PXR_InputCallable]
    public void SpawnObject()
    {
        RunChainEvent();

        if (!Utilities.IsValid(spawnPrefab) ||
            !Utilities.IsValid(spawnTransform))
            return;

        GameObject spawned =
            VRCInstantiate(spawnPrefab);

        if (!Utilities.IsValid(spawned))
            return;

        spawned.transform.SetPositionAndRotation(
            spawnTransform.position,
            spawnTransform.rotation
        );

        spawned.SetActive(true);

        if (velocity.sqrMagnitude <= 0f)
            return;

        Vector3 worldVelocity =
            spawnTransform.TransformDirection(velocity);

        RegisterMovingObject(
            spawned.transform,
            worldVelocity
        );
    }

    // =====================================
    // POOLED NON-PHYSICS SPAWNING
    // =====================================

    // Invoked internally when Inputs.Synced = true.

    public void SpawnPooledObject()
    {
        RunChainEvent();

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

    // =====================================
    // LOCAL MOVEMENT REGISTRATION
    // =====================================

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

            Transform[] objects =
                new Transform[capacity];

            Vector3[] velocities =
                new Vector3[capacity];

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
    }

    // =====================================
    // COMPONENT HELPERS
    // =====================================

    private bool HasComponentTarget()
    {
        if (componentTargetType ==
            PXR_ComponentTargetType.Behaviour)
            return Utilities.IsValid(targetBehaviour);

        if (componentTargetType ==
            PXR_ComponentTargetType.Renderer)
            return Utilities.IsValid(targetRenderer);

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
        RunChainEvent();

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

    // Legacy wrappers.
    public void EnableComponent()
    {
        RunChainEvent();
        ApplyComponentState(true);
    }

    public void DisableComponent()
    {
        RunChainEvent();
        ApplyComponentState(false);
    }

    public void SetComponentEnabled()
    {
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

        int i = 0;

        while (i < movingCount)
        {
            Transform obj = movingObjects[i];

            if (!Utilities.IsValid(obj))
            {
                int last = movingCount - 1;

                movingObjects[i] = movingObjects[last];
                movingVelocities[i] =
                    movingVelocities[last];

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
