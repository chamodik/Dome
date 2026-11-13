
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_UnifiedPhysics : UdonSharpBehaviour
{
<<<<<<< Updated upstream
    // =====================================
    // RADIAL FORCE
    // =====================================

    [Header("Radial Force")]

    [Min(0f)]
    public float radialRange = 5f;

=======
    [Header("Radial Force")]
    [Min(0f)] public float radialRange = 5f;
>>>>>>> Stashed changes
    public float radialForce = 10f;
    public bool radialAttract = false;
    public bool radialContinuous = false;
    public bool persistRadialState = true;

<<<<<<< Updated upstream
    // =====================================
    // BEAM FORCE
    // =====================================

    [Header("Beam Force")]

    [Min(0f)]
    public float beamRange = 5f;

    public float beamForce = 10f;
    public bool beamAttract = false;

    [Tooltip(
        "Local-space direction. (0,0,1) = forward."
    )]
    public Vector3 beamDirection = Vector3.forward;

    [Min(0.01f)]
    public float beamRadius = 0.5f;

    public bool beamContinuous = false;
    public bool persistBeamState = true;

    // =====================================
    // SPAWN OBJECT
    // =====================================

    [Header("Spawn Object")]

    public GameObject spawnPrefab;
    public Transform spawnTransform;

    [Tooltip(
        "Initial velocity in the Spawn Transform's " +
        "local axes, in metres per second."
    )]
    public Vector3 velocity = Vector3.forward;

    [Header("Networked Pool Settings")]

=======
    [Header("Beam Force")]
    [Min(0f)] public float beamRange = 5f;
    public float beamForce = 10f;
    public bool beamAttract = false;

    [Tooltip("Local-space direction. (0,0,1) = forward.")]
    public Vector3 beamDirection = Vector3.forward;

    [Min(0.01f)] public float beamRadius = 0.5f;
    public bool beamContinuous = false;
    public bool persistBeamState = true;

    [Header("Spawn Object")]
    public GameObject spawnPrefab;
    public Transform spawnTransform;

    [Tooltip("Initial velocity in Spawn Transform local axes, in metres per second.")]
    public Vector3 velocity = Vector3.forward;

    [Header("Networked Pool Settings")]
>>>>>>> Stashed changes
    public PXR_PoolManager spawnPoolManager;

    public PXR_DespawnMode despawnMode =
        PXR_DespawnMode.DontDespawn;

<<<<<<< Updated upstream
    [Min(0.05f)]
    public float despawnAfterSeconds = 5f;

    [Min(0.02f)]
    public float collisionReturnDelay = 0.35f;

    // =====================================
    // PHYSICS DETECTION
    // =====================================

    [Header("Physics Detection")]
    public LayerMask affectedLayers = -1;

    [Range(8, 256)]
    public int maxColliders = 64;

    // =====================================
    // CHAIN EVENT
    // =====================================

    [Header("Chain Event")]
    public bool enableChainEvent = false;

    public UdonSharpBehaviour chainEventTarget;
    public string chainEventName = "";

    [Min(0f)]
    public float chainEventDelay = 0f;

    // =====================================
    // SYNCHRONIZED STATE
    // =====================================

    [UdonSynced]
    private bool syncedRadialActive;

    [UdonSynced]
    private bool syncedBeamActive;
=======
    [Min(0.05f)] public float despawnAfterSeconds = 5f;
    [Min(0.02f)] public float collisionReturnDelay = 0.35f;

    [Header("Physics Detection")]
    public LayerMask affectedLayers = -1;
    [Range(8, 256)] public int maxColliders = 64;

    [UdonSynced] private bool syncedRadialActive;
    [UdonSynced] private bool syncedBeamActive;
>>>>>>> Stashed changes

    private bool radialActive;
    private bool beamActive;

<<<<<<< Updated upstream
    // =====================================
    // PHYSICS BUFFERS
    // =====================================

=======
>>>>>>> Stashed changes
    private Collider[] overlapResults;
    private Rigidbody[] processedBodies;
    private bool bufferWarningSent;

<<<<<<< Updated upstream
=======
    // Continuous forces repeat the overlap query only every few
    // physics steps. Force is still applied every step, using the
    // eligible bodies found by the most recent query.
    private const int CONTINUOUS_QUERY_INTERVAL = 5;

    private Rigidbody[] radialBodies;
    private int radialBodyCount;
    private int radialQueryCountdown;

    private Rigidbody[] beamBodies;
    private int beamBodyCount;
    private int beamQueryCountdown;

    // Local (Synced OFF) spawns waiting for Despawn After Time.
    private GameObject[] localDespawnObjects;
    private float[] localDespawnTimes;
    private int localDespawnCount;

>>>>>>> Stashed changes
    private void Start()
    {
        InitializeBuffers();
        ApplyPersistentState();
    }

    public override void OnDeserialization()
    {
        ApplyPersistentState();
    }

    private void ApplyPersistentState()
    {
        if (radialContinuous && persistRadialState)
            radialActive = syncedRadialActive;

        if (beamContinuous && persistBeamState)
            beamActive = syncedBeamActive;
    }

    private void InitializeBuffers()
    {
        if (overlapResults != null &&
<<<<<<< Updated upstream
            processedBodies != null)
            return;

        int size = Mathf.Clamp(
            maxColliders, 8, 256
        );

        overlapResults = new Collider[size];
        processedBodies = new Rigidbody[size];
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
    // RADIAL FORCE ENTRY POINTS
    // =====================================

    [PXR_InputCallable]
    public void RadialForce()
    {
        RunChainEvent();

=======
            processedBodies != null &&
            radialBodies != null &&
            beamBodies != null)
            return;

        int size = Mathf.Clamp(maxColliders, 8, 256);

        overlapResults = new Collider[size];
        processedBodies = new Rigidbody[size];
        radialBodies = new Rigidbody[size];
        beamBodies = new Rigidbody[size];
    }

    [PXR_InputCallable]
    public void RadialForce()
    {
>>>>>>> Stashed changes
        if (!radialContinuous)
        {
            ApplyRadialForce(ForceMode.Impulse);
            return;
        }

        SetRadialActive(!GetRadialActive());
    }

    [PXR_InputCallable]
    public void StartRadialForce()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        if (!radialContinuous)
        {
            ApplyRadialForce(ForceMode.Impulse);
            return;
        }

        SetRadialActive(true);
    }

    [PXR_InputCallable]
    public void StopRadialForce()
    {
<<<<<<< Updated upstream
        RunChainEvent();
        SetRadialActive(false);
    }

    // =====================================
    // BEAM FORCE ENTRY POINTS
    // =====================================

    [PXR_InputCallable]
    public void BeamForce()
    {
        RunChainEvent();

=======
        SetRadialActive(false);
    }

    [PXR_InputCallable]
    public void BeamForce()
    {
>>>>>>> Stashed changes
        if (!beamContinuous)
        {
            ApplyBeamForce(ForceMode.Impulse);
            return;
        }

        SetBeamActive(!GetBeamActive());
    }

    [PXR_InputCallable]
    public void StartBeamForce()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        if (!beamContinuous)
        {
            ApplyBeamForce(ForceMode.Impulse);
            return;
        }

        SetBeamActive(true);
    }

    [PXR_InputCallable]
    public void StopBeamForce()
    {
<<<<<<< Updated upstream
        RunChainEvent();
=======
>>>>>>> Stashed changes
        SetBeamActive(false);
    }

    [PXR_InputCallable]
    public void StopAllForces()
    {
<<<<<<< Updated upstream
        RunChainEvent();

=======
>>>>>>> Stashed changes
        SetRadialActive(false);
        SetBeamActive(false);
    }

<<<<<<< Updated upstream
    // =====================================
    // FORCE STATE
    // =====================================

=======
>>>>>>> Stashed changes
    private bool GetRadialActive()
    {
        if (radialContinuous && persistRadialState)
            return syncedRadialActive;

        return radialActive;
    }

    private bool GetBeamActive()
    {
        if (beamContinuous && persistBeamState)
            return syncedBeamActive;

        return beamActive;
    }

    private void SetRadialActive(bool value)
    {
        if (radialContinuous && persistRadialState)
        {
            if (!Networking.IsOwner(gameObject))
                return;

            if (syncedRadialActive == value)
                return;

            syncedRadialActive = value;
            radialActive = value;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
            RequestSerialization();
            return;
        }

        radialActive = value;
    }

    private void SetBeamActive(bool value)
    {
        if (beamContinuous && persistBeamState)
        {
            if (!Networking.IsOwner(gameObject))
                return;

            if (syncedBeamActive == value)
                return;

            syncedBeamActive = value;
            beamActive = value;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
            RequestSerialization();
            return;
        }

        beamActive = value;
    }

<<<<<<< Updated upstream
    // =====================================
    // LOCAL PHYSICS SPAWNING
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
        {
            Debug.LogWarning(
<<<<<<< Updated upstream
                "[PXR_UnifiedPhysics] Missing spawn " +
                "prefab or spawn transform."
=======
                "[PXR_UnifiedPhysics] Missing spawn prefab " +
                "or spawn transform."
>>>>>>> Stashed changes
            );
            return;
        }

        Rigidbody prefabBody =
            spawnPrefab.GetComponent<Rigidbody>();

        if (!Utilities.IsValid(prefabBody))
        {
            Debug.LogWarning(
<<<<<<< Updated upstream
                "[PXR_UnifiedPhysics] Prefab needs " +
                "a root Rigidbody."
=======
                "[PXR_UnifiedPhysics] Prefab needs a root Rigidbody."
>>>>>>> Stashed changes
            );
            return;
        }

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
        Rigidbody body =
            spawned.GetComponent<Rigidbody>();

        if (!Utilities.IsValid(body) ||
            body.isKinematic)
            return;

        // Assign a fresh initial velocity.
        // A zero vector means stationary at spawn.
=======
        ScheduleLocalDespawn(spawned);

        Rigidbody body = spawned.GetComponent<Rigidbody>();

        if (!Utilities.IsValid(body) || body.isKinematic)
            return;

>>>>>>> Stashed changes
        body.velocity =
            spawnTransform.TransformDirection(velocity);

        body.angularVelocity = Vector3.zero;
    }

<<<<<<< Updated upstream
    // =====================================
    // NETWORKED PHYSICS SPAWNING
    // =====================================

    // Invoked internally by UnifiedInputs
    // when its Synced checkbox is enabled.

    public void SpawnPooledObject()
    {
        RunChainEvent();

=======
    // Called by UnifiedInputs when Synced is enabled.
    public void SpawnPooledObject()
    {
>>>>>>> Stashed changes
        if (!Utilities.IsValid(spawnPoolManager) ||
            !Utilities.IsValid(spawnTransform))
        {
            Debug.LogWarning(
<<<<<<< Updated upstream
                "[PXR_UnifiedPhysics] Missing pool " +
                "manager or spawn transform."
=======
                "[PXR_UnifiedPhysics] Missing pool manager " +
                "or spawn transform."
>>>>>>> Stashed changes
            );
            return;
        }

        Vector3 worldVelocity =
            spawnTransform.TransformDirection(velocity);

        spawnPoolManager.RequestSpawn(
            true,
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
    // RADIAL PHYSICS
    // =====================================

    private void ApplyRadialForce(ForceMode mode)
    {
=======
    // =========================================
    // LOCAL SPAWN DESPAWN (SYNCED OFF)
    // =========================================

    // Despawn After Time also applies to local spawns.
    // Don't Despawn and Despawn on Collision leave local
    // spawns untouched, exactly as before.

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

            if (Utilities.IsValid(obj))
                Destroy(obj);
        }
    }

    // =========================================
    // RADIAL FORCE
    // =========================================

    // One-shot impulse: query, then push every eligible body.
    private void ApplyRadialForce(ForceMode mode)
    {
        int count = CollectRadialBodies(processedBodies);

        ApplyRadialToBodies(
            processedBodies,
            count,
            mode,
            false
        );
    }

    // Finds eligible Rigidbodies inside the radial sphere.
    private int CollectRadialBodies(Rigidbody[] results)
    {
>>>>>>> Stashed changes
        InitializeBuffers();

        float range = Mathf.Max(0f, radialRange);
        float strength = Mathf.Abs(radialForce);

        if (range <= 0f || strength <= 0f)
<<<<<<< Updated upstream
            return;

        Vector3 center = transform.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
=======
            return 0;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
>>>>>>> Stashed changes
            range,
            overlapResults,
            affectedLayers.value,
            QueryTriggerInteraction.Ignore
        );

        CheckBufferCapacity(count);

<<<<<<< Updated upstream
        int processedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapResults[i];

            if (!Utilities.IsValid(hit))
                continue;

            Rigidbody body = hit.attachedRigidbody;

            if (!IsEligibleBody(body))
                continue;

            if (AlreadyProcessed(body, processedCount))
                continue;

            processedBodies[processedCount] = body;
            processedCount++;

            Vector3 delta =
                body.position - center;
=======
        return CollectEligible(count, results);
    }

    private void ApplyRadialToBodies(
        Rigidbody[] bodies,
        int count,
        ForceMode mode,
        bool recheck)
    {
        float strength = Mathf.Abs(radialForce);

        if (count <= 0 || strength <= 0f)
            return;

        Vector3 center = transform.position;

        for (int i = 0; i < count; i++)
        {
            Rigidbody body = bodies[i];

            // Cached bodies may have been destroyed or
            // made kinematic since the last query.
            if (recheck &&
                (!Utilities.IsValid(body) || body.isKinematic))
                continue;

            Vector3 delta = body.position - center;
>>>>>>> Stashed changes

            if (delta.sqrMagnitude < 0.000001f)
                continue;

            Vector3 direction = delta.normalized;

            if (radialAttract)
                direction = -direction;

<<<<<<< Updated upstream
            body.AddForce(
                direction * strength,
                mode
            );
        }
    }

    // =====================================
    // BEAM PHYSICS
    // =====================================

    private void ApplyBeamForce(ForceMode mode)
    {
=======
            body.AddForce(direction * strength, mode);
        }
    }

    // =========================================
    // BEAM FORCE
    // =========================================

    private void ApplyBeamForce(ForceMode mode)
    {
        int count = CollectBeamBodies(processedBodies);

        ApplyBeamToBodies(
            processedBodies,
            count,
            mode,
            false
        );
    }

    private Vector3 GetBeamWorldDirection()
    {
        Vector3 localDirection = beamDirection;

        if (localDirection.sqrMagnitude < 0.000001f)
            localDirection = Vector3.forward;

        return transform.TransformDirection(
            localDirection.normalized
        ).normalized;
    }

    // Finds eligible Rigidbodies inside the oriented beam box.
    private int CollectBeamBodies(Rigidbody[] results)
    {
>>>>>>> Stashed changes
        InitializeBuffers();

        float range = Mathf.Max(0f, beamRange);
        float radius = Mathf.Max(0.01f, beamRadius);
        float strength = Mathf.Abs(beamForce);

        if (range <= 0f || strength <= 0f)
<<<<<<< Updated upstream
            return;

        Vector3 localDirection = beamDirection;

        if (localDirection.sqrMagnitude < 0.000001f)
            localDirection = Vector3.forward;

        Vector3 direction =
            transform.TransformDirection(
                localDirection.normalized
            ).normalized;

        Vector3 center =
            transform.position +
            direction * (range * 0.5f);
=======
            return 0;

        Vector3 direction = GetBeamWorldDirection();

        Vector3 center =
            transform.position + direction * (range * 0.5f);
>>>>>>> Stashed changes

        Vector3 extents = new Vector3(
            radius,
            radius,
            range * 0.5f
        );

        Vector3 up = transform.up;

<<<<<<< Updated upstream
        if (Mathf.Abs(
            Vector3.Dot(direction, up)) > 0.99f)
        {
            up = transform.right;
        }
=======
        if (Mathf.Abs(Vector3.Dot(direction, up)) > 0.99f)
            up = transform.right;
>>>>>>> Stashed changes

        Quaternion rotation =
            Quaternion.LookRotation(direction, up);

        int count = Physics.OverlapBoxNonAlloc(
            center,
            extents,
            overlapResults,
            rotation,
            affectedLayers.value,
            QueryTriggerInteraction.Ignore
        );

        CheckBufferCapacity(count);

<<<<<<< Updated upstream
        Vector3 forceDirection =
            beamAttract ? -direction : direction;

        int processedCount = 0;

        for (int i = 0; i < count; i++)
        {
=======
        return CollectEligible(count, results);
    }

    private void ApplyBeamToBodies(
        Rigidbody[] bodies,
        int count,
        ForceMode mode,
        bool recheck)
    {
        float strength = Mathf.Abs(beamForce);

        if (count <= 0 || strength <= 0f)
            return;

        Vector3 direction = GetBeamWorldDirection();

        Vector3 force =
            (beamAttract ? -direction : direction) * strength;

        for (int i = 0; i < count; i++)
        {
            Rigidbody body = bodies[i];

            if (recheck &&
                (!Utilities.IsValid(body) || body.isKinematic))
                continue;

            body.AddForce(force, mode);
        }
    }

    // =========================================
    // SHARED QUERY FILTERING
    // =========================================

    // Copies each eligible, distinct Rigidbody from the
    // overlap results into the supplied array.
    private int CollectEligible(int hitCount, Rigidbody[] results)
    {
        int found = 0;

        for (int i = 0; i < hitCount; i++)
        {
>>>>>>> Stashed changes
            Collider hit = overlapResults[i];

            if (!Utilities.IsValid(hit))
                continue;

            Rigidbody body = hit.attachedRigidbody;

<<<<<<< Updated upstream
            if (!IsEligibleBody(body))
                continue;

            if (AlreadyProcessed(body, processedCount))
                continue;

            processedBodies[processedCount] = body;
            processedCount++;

            body.AddForce(
                forceDirection * strength,
                mode
            );
        }
    }

    // =====================================
    // RIGIDBODY FILTERING
    // =====================================

    private bool IsEligibleBody(Rigidbody body)
    {
        if (!Utilities.IsValid(body))
            return false;

=======
            if (!IsEligibleBody(body) ||
                AlreadyProcessed(results, found, body))
                continue;

            results[found] = body;
            found++;
        }

        return found;
    }

    private bool IsEligibleBody(Rigidbody body)
    {
        // Cheapest checks first. Networking.IsOwner is the
        // most expensive call, so it runs last.
        if (!Utilities.IsValid(body))
            return false;

        if (body.isKinematic)
            return false;

>>>>>>> Stashed changes
        if (body.gameObject == gameObject)
            return false;

        if (!Networking.IsOwner(body.gameObject))
            return false;

<<<<<<< Updated upstream
        if (body.isKinematic)
            return false;

=======
>>>>>>> Stashed changes
        return true;
    }

    private bool AlreadyProcessed(
<<<<<<< Updated upstream
        Rigidbody body,
        int processedCount)
    {
        for (int i = 0; i < processedCount; i++)
        {
            if (processedBodies[i] == body)
=======
        Rigidbody[] list,
        int count,
        Rigidbody body)
    {
        for (int i = 0; i < count; i++)
        {
            if (list[i] == body)
>>>>>>> Stashed changes
                return true;
        }

        return false;
    }

    private void CheckBufferCapacity(int count)
    {
<<<<<<< Updated upstream
        if (bufferWarningSent ||
            overlapResults == null)
=======
        if (bufferWarningSent || overlapResults == null)
>>>>>>> Stashed changes
            return;

        if (count >= overlapResults.Length)
        {
            bufferWarningSent = true;

            Debug.LogWarning(
<<<<<<< Updated upstream
                "[PXR_UnifiedPhysics] Query buffer " +
                "full. Increase Max Colliders " +
                "or restrict physics layers."
=======
                "[PXR_UnifiedPhysics] Query buffer full. " +
                "Increase Max Colliders or restrict physics layers."
>>>>>>> Stashed changes
            );
        }
    }

<<<<<<< Updated upstream
    // =====================================
    // CONTINUOUS FORCE UPDATE
    // =====================================
=======
    // =========================================
    // CONTINUOUS FORCES
    // =========================================
>>>>>>> Stashed changes

    private void FixedUpdate()
    {
        if (radialContinuous && radialActive)
<<<<<<< Updated upstream
            ApplyRadialForce(ForceMode.Acceleration);

        if (beamContinuous && beamActive)
            ApplyBeamForce(ForceMode.Acceleration);
=======
            TickContinuousRadial();
        else
            radialQueryCountdown = 0;

        if (beamContinuous && beamActive)
            TickContinuousBeam();
        else
            beamQueryCountdown = 0;
    }

    private void TickContinuousRadial()
    {
        if (radialQueryCountdown <= 0)
        {
            radialBodyCount = CollectRadialBodies(radialBodies);
            radialQueryCountdown = CONTINUOUS_QUERY_INTERVAL;
        }

        radialQueryCountdown--;

        ApplyRadialToBodies(
            radialBodies,
            radialBodyCount,
            ForceMode.Acceleration,
            true
        );
    }

    private void TickContinuousBeam()
    {
        if (beamQueryCountdown <= 0)
        {
            beamBodyCount = CollectBeamBodies(beamBodies);
            beamQueryCountdown = CONTINUOUS_QUERY_INTERVAL;
        }

        beamQueryCountdown--;

        ApplyBeamToBodies(
            beamBodies,
            beamBodyCount,
            ForceMode.Acceleration,
            true
        );
>>>>>>> Stashed changes
    }

    private void OnDisable()
    {
        radialActive = false;
        beamActive = false;
    }

    private void OnEnable()
    {
        ApplyPersistentState();
    }

    public bool IsRadialActive()
    {
        return radialActive;
    }

    public bool IsBeamActive()
    {
        return beamActive;
    }
}
