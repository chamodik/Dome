
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

using Debug = UnityEngine.Debug;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_PoolManager : UdonSharpBehaviour
{
    // =========================================
    // INSPECTOR
    // =========================================

    [Header("Object Pool")]
    public VRCObjectPool objectPool;

    [Header("Optional Key Locks")]
    public PXR_KeyLockState[] keyLocks;

    [Header("Lifecycle Settings")]

    [Min(0.5f)]
    public float ownershipTimeout = 3f;

    [Min(1f)]
    public float keyAcknowledgementTimeout = 5f;

    [Min(0.1f)]
    public float keyRetryInterval = 0.5f;

    [Header("Diagnostics")]
    public bool debugLogs = false;

    // =========================================
    // SLOT STATES
    // =========================================

    private const int STATE_FREE = 0;
    private const int STATE_INITIALIZING = 1;
    private const int STATE_ACTIVE = 2;
    private const int STATE_RETURNING = 3;

    // =========================================
    // CACHED POOL COMPONENTS
    // =========================================

    private GameObject[] objects;
    private PXR_PooledObject[] pooledObjects;
    private VRCObjectSync[] objectSyncs;
    private Rigidbody[] rigidbodies;

    // =========================================
    // PER-SLOT LIFECYCLE
    // =========================================

    private int[] states;
    private int[] generations;
    private int[] policies;

    // All timestamps below use
    // Time.realtimeSinceStartup.
    //
    // -1 means that no return is scheduled.

    private float[] returnAt;
    private float[] collisionDelay;

    // =========================================
    // PENDING SPAWN DATA
    // =========================================

    // Retained because object ownership can
    // take time to transfer.

    private Vector3[] spawnPositions;
    private Quaternion[] spawnRotations;
    private Vector3[] spawnVelocities;

    private bool[] spawnPhysics;
    private float[] spawnLifetimes;

    private float[] ownershipStartedAt;

    // =========================================
    // KEY ACKNOWLEDGMENT
    // =========================================

    private bool[] awaitingKeyAck;
    private int[] pendingLockIndex;

    // Spawn requester is the attributed user for
    // pooled projectile Keys, per slot/generation.
    private int[] spawnUserIds;
    private string[] spawnUserNames;

    private float[] keyTimeoutAt;
    private float[] nextKeyRetryAt;

    // =========================================
    // MANAGER STATE
    // =========================================

    private bool initialized;
    private bool ownerInitialized;

    // Number of slots not in STATE_FREE. Update skips the
    // per-slot loop entirely while this is zero.
    private int busySlotCount;

    // Network event rate limits (events per second, 1-100).
    // VRChat's default is 5/s; extra events queue locally,
    // which shows up as growing delay under rapid fire.
    private const int SPAWN_EVENT_RATE = 30;
    private const int KEY_EVENT_RATE = 20;

    // =========================================
    // INITIALIZATION
    // =========================================

    private void Start()
    {
        Initialize();
    }

    private float Now()
    {
        return Time.realtimeSinceStartup;
    }

    private bool IsPoolOwner()
    {
        return
            Utilities.IsValid(objectPool) &&
            objectPool.gameObject == gameObject &&
            Networking.IsOwner(gameObject);
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (!Utilities.IsValid(objectPool))
        {
            Debug.LogError(
                "[PXR_PoolManager] Missing Object Pool."
            );
            return;
        }

        if (objectPool.gameObject != gameObject)
        {
            Debug.LogError(
                "[PXR_PoolManager] VRCObjectPool " +
                "must be on the same GameObject."
            );
            return;
        }

        if (objectPool.Pool == null)
            return;

        objects = objectPool.Pool;

        int count = objects.Length;

        pooledObjects = new PXR_PooledObject[count];
        objectSyncs = new VRCObjectSync[count];
        rigidbodies = new Rigidbody[count];

        states = new int[count];
        generations = new int[count];
        policies = new int[count];

        returnAt = new float[count];
        collisionDelay = new float[count];

        spawnPositions = new Vector3[count];
        spawnRotations = new Quaternion[count];
        spawnVelocities = new Vector3[count];

        spawnPhysics = new bool[count];
        spawnLifetimes = new float[count];

        ownershipStartedAt = new float[count];

        awaitingKeyAck = new bool[count];
        pendingLockIndex = new int[count];
        spawnUserIds = new int[count];
        spawnUserNames = new string[count];

        keyTimeoutAt = new float[count];
        nextKeyRetryAt = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject obj = objects[i];

            states[i] = STATE_FREE;
            returnAt[i] = -1f;
            pendingLockIndex[i] = -1;
            spawnUserIds[i] = -1;
            spawnUserNames[i] = "";

            if (!Utilities.IsValid(obj))
                continue;

            pooledObjects[i] =
                obj.GetComponent<PXR_PooledObject>();

            objectSyncs[i] =
                obj.GetComponent<VRCObjectSync>();

            rigidbodies[i] =
                obj.GetComponent<Rigidbody>();

            if (!Utilities.IsValid(pooledObjects[i]))
            {
                Debug.LogWarning(
                    "[PXR_PoolManager] Slot " + i +
                    " missing PXR_PooledObject."
                );
            }

            if (!Utilities.IsValid(objectSyncs[i]))
            {
                Debug.LogWarning(
                    "[PXR_PoolManager] Slot " + i +
                    " missing VRCObjectSync."
                );
            }
        }

        initialized = true;

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PoolManager] Initialized " +
                count + " pool slots."
            );
        }
    }

    // =========================================
    // OWNER INITIALIZATION
    // =========================================

    // No lifecycle variables are synchronized.
    //
    // When pool ownership changes, the new
    // owner reclaims existing pool objects.
    //
    // This intentionally resets their lifetimes
    // instead of attempting to reconstruct them.

    private void EnsureOwnerReady()
    {
        Initialize();

        if (!initialized ||
            !IsPoolOwner() ||
            ownerInitialized)
            return;

        ownerInitialized = true;

        for (int i = 0; i < objects.Length; i++)
        {
            ClearSlot(i);

            GameObject obj = objects[i];

            if (!Utilities.IsValid(obj))
                continue;

            if (Utilities.IsValid(pooledObjects[i]))
                pooledObjects[i].Disarm();

            if (obj.activeSelf)
            {
                objectPool.Return(obj);

                if (debugLogs)
                {
                    Debug.Log(
                        "[PXR_PoolManager] Reclaimed " +
                        "slot " + i
                    );
                }
            }
        }
    }

    // =========================================
    // PUBLIC SPAWN INTERFACE
    // =========================================

    // Signature is unchanged.
    //
    // Existing PXR_UnifiedPhysics and
    // PXR_UnifiedOutputs scripts can use it.

    public void RequestSpawn(
        bool physics,
        Vector3 position,
        Quaternion rotation,
        Vector3 worldVelocity,
        int policy,
        float lifetime,
        float collisionGrace)
    {
        if (!Utilities.IsValid(objectPool))
            return;

        if (objectPool.gameObject != gameObject)
        {
            Debug.LogError(
                "[PXR_PoolManager] Object Pool " +
                "and Manager must share a GameObject."
            );
            return;
        }

        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerSpawnRequest),
            physics,
            position,
            rotation,
            worldVelocity,
            policy,
            lifetime,
            collisionGrace
        );
    }

    // =========================================
    // OWNER SPAWN REQUEST
    // =========================================

    [NetworkCallable(SPAWN_EVENT_RATE)]
    public void OwnerSpawnRequest(
        bool physics,
        Vector3 position,
        Quaternion rotation,
        Vector3 worldVelocity,
        int policy,
        float lifetime,
        float collisionGrace)
    {
        if (!IsPoolOwner())
            return;

        EnsureOwnerReady();

        if (!initialized)
            return;

        GameObject obj =
            objectPool.TryToSpawn();

        if (!Utilities.IsValid(obj))
        {
            if (debugLogs)
            {
                Debug.LogWarning(
                    "[PXR_PoolManager] Pool exhausted."
                );
            }

            return;
        }

        int slot = FindSlot(obj);

        if (slot < 0)
        {
            objectPool.Return(obj);
            return;
        }

        if (!Utilities.IsValid(pooledObjects[slot]) ||
            !Utilities.IsValid(objectSyncs[slot]) ||
            (physics &&
             !Utilities.IsValid(rigidbodies[slot])))
        {
            Debug.LogError(
                "[PXR_PoolManager] Invalid components " +
                "on slot " + slot
            );

            objectPool.Return(obj);
            ClearSlot(slot);
            return;
        }

        if (states[slot] == STATE_FREE)
            busySlotCount++;

        // New generation for every reuse.
        generations[slot] =
            generations[slot] == int.MaxValue
                ? 1
                : generations[slot] + 1;

        states[slot] = STATE_INITIALIZING;

        policies[slot] = Mathf.Clamp(
            policy,
            0,
            2
        );

        spawnPositions[slot] = position;
        spawnRotations[slot] = rotation;
        spawnVelocities[slot] = worldVelocity;

        spawnPhysics[slot] = physics;

        spawnLifetimes[slot] =
            Mathf.Max(0.05f, lifetime);

        collisionDelay[slot] =
            Mathf.Max(0.02f, collisionGrace);

        ownershipStartedAt[slot] = Now();

        // Read the sender of the actual networked
        // spawn request, not the pool's owner.
        VRCPlayerApi requester =
            NetworkCalling.CallingPlayer;

        // Outside a network call (e.g. the owner requesting a
        // spawn from itself), the caller is the local player.
        if (!Utilities.IsValid(requester))
            requester = Networking.LocalPlayer;

        spawnUserIds[slot] =
            Utilities.IsValid(requester)
                ? requester.playerId : -1;
        spawnUserNames[slot] =
            Utilities.IsValid(requester)
                ? requester.displayName : "";

        returnAt[slot] = -1f;

        awaitingKeyAck[slot] = false;
        pendingLockIndex[slot] = -1;

        TryInitializeSpawn(slot);
    }

    // =========================================
    // INITIALIZE A SPAWNED OBJECT
    // =========================================

    private void TryInitializeSpawn(int slot)
    {
        if (states[slot] != STATE_INITIALIZING)
            return;

        GameObject obj = objects[slot];

        if (!Utilities.IsValid(obj))
        {
            ClearSlot(slot);
            return;
        }

        // Only the current pool owner should
        // initialize the object's physics state.

        if (!Networking.IsOwner(obj))
        {
            VRCPlayerApi local =
                Networking.LocalPlayer;

            if (Utilities.IsValid(local))
            {
                Networking.SetOwner(local, obj);
            }

            if (!Networking.IsOwner(obj))
            {
                if (Now() - ownershipStartedAt[slot] >=
                    Mathf.Max(0.5f, ownershipTimeout))
                {
                    Debug.LogWarning(
                        "[PXR_PoolManager] Spawn " +
                        "ownership timeout at slot " +
                        slot
                    );

                    objectPool.Return(obj);
                    ClearSlot(slot);
                }

                return;
            }
        }

        Rigidbody body = rigidbodies[slot];
        VRCObjectSync sync = objectSyncs[slot];

        // Configure physics mode through
        // VRCObjectSync instead of changing
        // Rigidbody.isKinematic directly.

        if (Utilities.IsValid(body))
        {
            sync.SetKinematic(!spawnPhysics[slot]);

            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        sync.FlagDiscontinuity();

        obj.transform.SetPositionAndRotation(
            spawnPositions[slot],
            spawnRotations[slot]
        );

        if (Utilities.IsValid(body))
        {
            body.position = spawnPositions[slot];
            body.rotation = spawnRotations[slot];

            if (spawnPhysics[slot])
            {
                body.velocity =
                    spawnVelocities[slot];
            }
        }

        // Impacts only matter for collision despawn or keys.
        pooledObjects[slot].reportCollisionImpacts =
            policies[slot] == (int)PXR_DespawnMode.OnCollision;

        pooledObjects[slot].Arm(
            this,
            slot,
            generations[slot],
            spawnPhysics[slot],
            spawnVelocities[slot]
        );

        states[slot] = STATE_ACTIVE;

        // Timers begin only once the object
        // has actually been initialized.

        if (policies[slot] ==
            (int)PXR_DespawnMode.AfterTime)
        {
            returnAt[slot] =
                Now() + spawnLifetimes[slot];
        }

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PoolManager] SPAWN " +
                "slot=" + slot +
                " generation=" + generations[slot] +
                " policy=" + policies[slot]
            );
        }
    }

    // =========================================
    // COLLISION REPORTING
    // =========================================

    // Called locally by PXR_PooledObject.
    //
    // The pooled object and pool manager
    // are normally owned by the same player.

    public void NotifyImpact(
        int slot,
        int generation,
        PXR_UnifiedInputs hitLock)
    {
        if (!IsPoolOwner() ||
            !ValidActive(slot, generation))
            return;

        float now = Now();

        // Any collision can cause despawning.
        //
        // The lock is not required for the
        // OnCollision despawn policy.

        if (policies[slot] ==
                (int)PXR_DespawnMode.OnCollision &&
            returnAt[slot] < 0f)
        {
            returnAt[slot] =
                now + collisionDelay[slot];

            if (debugLogs)
            {
                Debug.Log(
                    "[PXR_PoolManager] COLLISION " +
                    "slot=" + slot +
                    " returnAt=" + returnAt[slot]
                );
            }
        }

        // The remaining collision logic is
        // only relevant for actual pooled Keys
        // striking a registered Key lock.

        if (!Utilities.IsValid(hitLock))
            return;

        if (!pooledObjects[slot].actsAsKey)
            return;

        if (hitLock.interactionType !=
            PXR_InputType.Key)
            return;

        PXR_KeyLockState state = hitLock.keyLockState;
        if (!Utilities.IsValid(state))
            return;

        int lockIndex = FindLockIndex(state);

        if (lockIndex < 0)
        {
            Debug.LogWarning(
                "[PXR_PoolManager] Key struck an " +
                "unregistered lock."
            );

            return;
        }

        if (awaitingKeyAck[slot])
            return;

        awaitingKeyAck[slot] = true;
        pendingLockIndex[slot] = lockIndex;

        keyTimeoutAt[slot] =
            now + Mathf.Max(
                1f,
                keyAcknowledgementTimeout
            );

        nextKeyRetryAt[slot] =
            now + Mathf.Max(
                0.1f,
                keyRetryInterval
            );

        SendKeyRequest(slot);

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PoolManager] KEY HIT " +
                "slot=" + slot +
                " lock=" + lockIndex
            );
        }
    }

    // =========================================
    // SEND KEY REGISTRATION TO LOCK OWNER
    // =========================================

    private void SendKeyRequest(int slot)
    {
        int lockIndex = pendingLockIndex[slot];

        if (keyLocks == null ||
            lockIndex < 0 ||
            lockIndex >= keyLocks.Length)
            return;

        PXR_KeyLockState keyLock =
            keyLocks[lockIndex];

        if (!Utilities.IsValid(keyLock))
            return;

        keyLock.SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(
                PXR_KeyLockState.OwnerReceivePooledKey
            ),
            slot,
            generations[slot],
            lockIndex,
            spawnUserIds[slot],
            spawnUserNames[slot]
        );
    }

    // =========================================
    // KEY ACKNOWLEDGMENT
    // =========================================

    [NetworkCallable(KEY_EVENT_RATE)]
    public void OwnerKeyAcknowledged(
        int slot,
        int generation,
        int lockIndex,
        bool accepted)
    {
        if (!IsPoolOwner() ||
            !ValidActive(slot, generation))
            return;

        if (!awaitingKeyAck[slot] ||
            pendingLockIndex[slot] != lockIndex)
            return;

        if (keyLocks == null ||
            lockIndex < 0 ||
            lockIndex >= keyLocks.Length)
            return;

        PXR_KeyLockState keyLock =
            keyLocks[lockIndex];

        if (!Utilities.IsValid(keyLock))
            return;

        // Only the owner of that lock may
        // acknowledge the registration.

        VRCPlayerApi sender =
            NetworkCalling.CallingPlayer;

        // Outside a network call (pool owner and lock owner are
        // the same player), the caller is the local player.
        if (!Utilities.IsValid(sender))
            sender = Networking.LocalPlayer;

        VRCPlayerApi lockOwner =
            Networking.GetOwner(
                keyLock.gameObject
            );

        if (!Utilities.IsValid(sender) ||
            !Utilities.IsValid(lockOwner) ||
            sender.playerId != lockOwner.playerId)
            return;

        awaitingKeyAck[slot] = false;
        pendingLockIndex[slot] = -1;

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PoolManager] KEY ACK " +
                "slot=" + slot +
                " accepted=" + accepted
            );
        }

        // The existing return deadline remains.
        //
        // In OnCollision mode, the object will
        // return when its collision grace
        // period expires.
    }

    // =========================================
    // MAIN LIFECYCLE UPDATE
    // =========================================

    private void Update()
    {
        if (!initialized)
            Initialize();

        if (!initialized)
            return;

        if (!IsPoolOwner())
        {
            ownerInitialized = false;
            return;
        }

        EnsureOwnerReady();

        // Nothing spawning, active or returning.
        if (busySlotCount <= 0)
            return;

        float now = Now();

        for (int i = 0; i < objects.Length; i++)
        {
            // -------------------------------
            // WAITING FOR OBJECT OWNERSHIP
            // -------------------------------

            if (states[i] == STATE_INITIALIZING)
            {
                TryInitializeSpawn(i);
                continue;
            }

            // -------------------------------
            // ACTIVE OBJECT
            // -------------------------------

            if (states[i] == STATE_ACTIVE)
            {
                // Key registration can delay
                // return briefly.

                if (awaitingKeyAck[i])
                {
                    if (now >= keyTimeoutAt[i])
                    {
                        Debug.LogWarning(
                            "[PXR_PoolManager] Key " +
                            "acknowledgment timeout " +
                            "at slot " + i
                        );

                        awaitingKeyAck[i] = false;
                        pendingLockIndex[i] = -1;
                    } else if (now >= nextKeyRetryAt[i])
                    {
                        SendKeyRequest(i);

                        nextKeyRetryAt[i] =
                            now + Mathf.Max(
                                0.1f,
                                keyRetryInterval
                            );
                    }
                }

                // IMPORTANT:
                //
                // returnAt uses a LOCAL
                // nonnegative clock.
                //
                // No server timestamp is used.
                //
                // -1 means no return scheduled.

                if (!awaitingKeyAck[i] &&
                    returnAt[i] >= 0f &&
                    now >= returnAt[i])
                {
                    states[i] = STATE_RETURNING;

                    ownershipStartedAt[i] = now;

                    if (debugLogs)
                    {
                        Debug.Log(
                            "[PXR_PoolManager] " +
                            "RETURN DUE slot=" + i
                        );
                    }
                }
            }

            // -------------------------------
            // RETURN TO POOL
            // -------------------------------

            if (states[i] == STATE_RETURNING)
            {
                TryReturn(i);
            }
        }
    }

    // =========================================
    // RESET AND RETURN
    // =========================================

    private void TryReturn(int slot)
    {
        GameObject obj = objects[slot];

        if (!Utilities.IsValid(obj))
        {
            ClearSlot(slot);
            return;
        }

        if (!obj.activeSelf)
        {
            ClearSlot(slot);
            return;
        }

        // The manager remains authoritative
        // over returning objects.
        //
        // Reclaim ownership if needed before
        // resetting movement.

        if (!Networking.IsOwner(obj))
        {
            VRCPlayerApi local =
                Networking.LocalPlayer;

            if (Utilities.IsValid(local))
            {
                Networking.SetOwner(local, obj);
            }

            if (!Networking.IsOwner(obj))
            {
                if (Now() -
                    ownershipStartedAt[slot] >=
                    Mathf.Max(0.5f, ownershipTimeout))
                {
                    Debug.LogWarning(
                        "[PXR_PoolManager] Return " +
                        "ownership timeout at slot " +
                        slot
                    );

                    // The pool owner can still
                    // return its own pool entry.

                    objectPool.Return(obj);
                    ClearSlot(slot);
                }

                return;
            }
        }

        // Stop the pooled object's local
        // movement and collision processing.

        PXR_PooledObject pooled =
            pooledObjects[slot];

        if (Utilities.IsValid(pooled))
            pooled.Disarm();

        // VRCObjectSync.Respawn() resets:
        //
        // Position
        // Rotation
        // Linear velocity
        // Angular velocity
        //
        // No separate home-position arrays
        // or manual Rigidbody reset needed.

        VRCObjectSync sync = objectSyncs[slot];

        if (Utilities.IsValid(sync))
            sync.Respawn();

        // Only the pool owner performs Return.

        objectPool.Return(obj);

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PoolManager] RETURN " +
                "slot=" + slot +
                " active=" + obj.activeSelf
            );
        }

        ClearSlot(slot);
    }

    // =========================================
    // CLEAR SLOT
    // =========================================

    private void ClearSlot(int slot)
    {
        if (states[slot] != STATE_FREE)
            busySlotCount = Mathf.Max(0, busySlotCount - 1);

        states[slot] = STATE_FREE;

        policies[slot] = 0;
        returnAt[slot] = -1f;

        awaitingKeyAck[slot] = false;
        pendingLockIndex[slot] = -1;
        spawnUserIds[slot] = -1;
        spawnUserNames[slot] = "";

        // Generation deliberately remains
        // unchanged until the next spawn.
    }

    // =========================================
    // VALIDATION
    // =========================================

    private bool ValidActive(
        int slot,
        int generation)
    {
        return
            initialized &&
            slot >= 0 &&
            slot < objects.Length &&
            states[slot] == STATE_ACTIVE &&
            generations[slot] == generation &&
            Utilities.IsValid(objects[slot]) &&
            objects[slot].activeSelf;
    }

    // =========================================
    // PUBLIC LOOKUPS
    // =========================================

    // Retained for PXR_UnifiedInputs
    // and PXR_PooledObject compatibility.

    public int FindSlot(GameObject obj)
    {
        if (!initialized)
            Initialize();

        if (!initialized ||
            !Utilities.IsValid(obj))
            return -1;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] == obj)
                return i;
        }

        return -1;
    }

    public int FindLockIndex(
        PXR_KeyLockState keyLock)
    {
        if (keyLocks == null ||
            !Utilities.IsValid(keyLock))
            return -1;

        for (int i = 0; i < keyLocks.Length; i++)
        {
            if (keyLocks[i] == keyLock)
                return i;
        }

        return -1;
    }

    public GameObject GetPooledGameObject(int slot)
    {
        if (!initialized)
            Initialize();

        if (!initialized ||
            slot < 0 ||
            slot >= objects.Length)
            return null;

        return objects[slot];
    }

    public PXR_PooledObject GetPooledObject(int slot)
    {
        if (!initialized)
            Initialize();

        if (!initialized ||
            slot < 0 ||
            slot >= pooledObjects.Length)
            return null;

        return pooledObjects[slot];
    }

    // =========================================
    // OWNERSHIP CHANGE
    // =========================================

    public override void OnOwnershipTransferred(
        VRCPlayerApi newOwner)
    {
        if (Utilities.IsValid(newOwner) &&
            newOwner.isLocal)
        {
            // Reclaim active objects when
            // this client becomes pool owner.

            ownerInitialized = false;
        }
    }
}
