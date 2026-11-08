
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_UnifiedPhysics : UdonSharpBehaviour
{
    // =====================================
    // RADIAL FORCE
    // =====================================

    [Header("Radial Force")]

    [Min(0f)]
    public float radialRange = 5f;

    public float radialForce = 10f;
    public bool radialAttract = false;
    public bool radialContinuous = false;
    public bool persistRadialState = true;

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

    public PXR_PoolManager spawnPoolManager;

    public PXR_DespawnMode despawnMode =
        PXR_DespawnMode.DontDespawn;

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

    private bool radialActive;
    private bool beamActive;

    // =====================================
    // PHYSICS BUFFERS
    // =====================================

    private Collider[] overlapResults;
    private Rigidbody[] processedBodies;
    private bool bufferWarningSent;

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
        RunChainEvent();

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
        RunChainEvent();

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
        RunChainEvent();
        SetBeamActive(false);
    }

    [PXR_InputCallable]
    public void StopAllForces()
    {
        RunChainEvent();

        SetRadialActive(false);
        SetBeamActive(false);
    }

    // =====================================
    // FORCE STATE
    // =====================================

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

            RequestSerialization();
            return;
        }

        beamActive = value;
    }

    // =====================================
    // LOCAL PHYSICS SPAWNING
    // =====================================

    [PXR_InputCallable]
    public void SpawnObject()
    {
        RunChainEvent();

        if (!Utilities.IsValid(spawnPrefab) ||
            !Utilities.IsValid(spawnTransform))
        {
            Debug.LogWarning(
                "[PXR_UnifiedPhysics] Missing spawn " +
                "prefab or spawn transform."
            );
            return;
        }

        Rigidbody prefabBody =
            spawnPrefab.GetComponent<Rigidbody>();

        if (!Utilities.IsValid(prefabBody))
        {
            Debug.LogWarning(
                "[PXR_UnifiedPhysics] Prefab needs " +
                "a root Rigidbody."
            );
            return;
        }

        GameObject spawned =
            VRCInstantiate(spawnPrefab);

        if (!Utilities.IsValid(spawned))
            return;

        spawned.transform.SetPositionAndRotation(
            spawnTransform.position,
            spawnTransform.rotation
        );

        spawned.SetActive(true);

        Rigidbody body =
            spawned.GetComponent<Rigidbody>();

        if (!Utilities.IsValid(body) ||
            body.isKinematic)
            return;

        // Assign a fresh initial velocity.
        // A zero vector means stationary at spawn.
        body.velocity =
            spawnTransform.TransformDirection(velocity);

        body.angularVelocity = Vector3.zero;
    }

    // =====================================
    // NETWORKED PHYSICS SPAWNING
    // =====================================

    // Invoked internally by UnifiedInputs
    // when its Synced checkbox is enabled.

    public void SpawnPooledObject()
    {
        RunChainEvent();

        if (!Utilities.IsValid(spawnPoolManager) ||
            !Utilities.IsValid(spawnTransform))
        {
            Debug.LogWarning(
                "[PXR_UnifiedPhysics] Missing pool " +
                "manager or spawn transform."
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

    // =====================================
    // RADIAL PHYSICS
    // =====================================

    private void ApplyRadialForce(ForceMode mode)
    {
        InitializeBuffers();

        float range = Mathf.Max(0f, radialRange);
        float strength = Mathf.Abs(radialForce);

        if (range <= 0f || strength <= 0f)
            return;

        Vector3 center = transform.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            range,
            overlapResults,
            affectedLayers.value,
            QueryTriggerInteraction.Ignore
        );

        CheckBufferCapacity(count);

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

            if (delta.sqrMagnitude < 0.000001f)
                continue;

            Vector3 direction = delta.normalized;

            if (radialAttract)
                direction = -direction;

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
        InitializeBuffers();

        float range = Mathf.Max(0f, beamRange);
        float radius = Mathf.Max(0.01f, beamRadius);
        float strength = Mathf.Abs(beamForce);

        if (range <= 0f || strength <= 0f)
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

        Vector3 extents = new Vector3(
            radius,
            radius,
            range * 0.5f
        );

        Vector3 up = transform.up;

        if (Mathf.Abs(
            Vector3.Dot(direction, up)) > 0.99f)
        {
            up = transform.right;
        }

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

        Vector3 forceDirection =
            beamAttract ? -direction : direction;

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

        if (body.gameObject == gameObject)
            return false;

        if (!Networking.IsOwner(body.gameObject))
            return false;

        if (body.isKinematic)
            return false;

        return true;
    }

    private bool AlreadyProcessed(
        Rigidbody body,
        int processedCount)
    {
        for (int i = 0; i < processedCount; i++)
        {
            if (processedBodies[i] == body)
                return true;
        }

        return false;
    }

    private void CheckBufferCapacity(int count)
    {
        if (bufferWarningSent ||
            overlapResults == null)
            return;

        if (count >= overlapResults.Length)
        {
            bufferWarningSent = true;

            Debug.LogWarning(
                "[PXR_UnifiedPhysics] Query buffer " +
                "full. Increase Max Colliders " +
                "or restrict physics layers."
            );
        }
    }

    // =====================================
    // CONTINUOUS FORCE UPDATE
    // =====================================

    private void FixedUpdate()
    {
        if (radialContinuous && radialActive)
            ApplyRadialForce(ForceMode.Acceleration);

        if (beamContinuous && beamActive)
            ApplyBeamForce(ForceMode.Acceleration);
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
