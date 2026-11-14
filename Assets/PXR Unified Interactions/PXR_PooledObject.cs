
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

using Debug = UnityEngine.Debug;

public enum PXR_DespawnMode
{
    DontDespawn = 0,
    OnCollision = 1,
    AfterTime = 2
}

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_PooledObject : UdonSharpBehaviour
{
    [Header("Pool")]
    public PXR_PoolManager poolManager;

    [Header("Optional Key Behavior")]
    public bool actsAsKey = false;
    public string keyTag = "";

    [Header("Diagnostics")]
    public bool debugLogs = false;

    private Rigidbody body;
    private Collider mainCollider;

    private int poolSlot = -1;
    private int spawnGeneration;

    private bool armed;
    private bool physicsMovement;

    private Vector3 movementVelocity;

    // Set by PXR_PoolManager before each Arm(). When false and this
    // object is not a key, impacts cannot affect the pool, so they
    // are skipped instead of forwarded. Not serialized or shown.
    [System.NonSerialized]
    public bool reportCollisionImpacts = true;

    private void Start()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        body = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
    }

    // IMPORTANT:
    //
    // There is intentionally no OnEnable()
    // or OnDisable() method.
    //
    // Pool lifecycle state is controlled
    // exclusively by Arm() and Disarm().
    //
    // This prevents delayed Udon OnEnable()
    // events from resetting the armed state
    // after the pool manager initializes
    // the object.

    // ========================================
    // SPAWN INITIALIZATION
    // ========================================

    public void Arm(
        PXR_PoolManager manager,
        int slot,
        int generation,
        bool usePhysics,
        Vector3 worldVelocity)
    {
        if (!Utilities.IsValid(manager))
        {
            Debug.LogError(
                "[PXR_PooledObject] Arm failed: " +
                "manager is invalid."
            );
            return;
        }

        if (!Networking.IsOwner(gameObject) ||
            !Networking.IsOwner(manager.gameObject))
        {
            Debug.LogWarning(
                "[PXR_PooledObject] Arm rejected: " +
                "ownership mismatch on " +
                gameObject.name
            );
            return;
        }

        CacheComponents();

        poolManager = manager;

        poolSlot = slot;
        spawnGeneration = generation;

        physicsMovement = usePhysics;
        movementVelocity = worldVelocity;

        armed = true;

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PooledObject] ARMED " +
                gameObject.name +
                " | Slot=" + poolSlot +
                " | Generation=" + spawnGeneration +
                " | Physics=" + physicsMovement +
                " | Rigidbody=" +
                Utilities.IsValid(body) +
                " | Kinematic=" +
                (Utilities.IsValid(body) &&
                 body.isKinematic) +
                " | RootCollider=" +
                Utilities.IsValid(mainCollider) +
                " | Trigger=" +
                (Utilities.IsValid(mainCollider) &&
                 mainCollider.isTrigger) +
                " | Layer=" + gameObject.layer
            );
        }
    }

    public void Disarm()
    {
        if (debugLogs && armed)
        {
            Debug.Log(
                "[PXR_PooledObject] DISARMED " +
                gameObject.name
            );
        }

        armed = false;
        movementVelocity = Vector3.zero;
    }

    // ========================================
    // AUTHORITY
    // ========================================

    private bool CanRun()
    {
        return
            armed &&
            gameObject.activeInHierarchy &&
            Utilities.IsValid(poolManager) &&
            Networking.IsOwner(gameObject) &&
            Networking.IsOwner(poolManager.gameObject);
    }

    // ========================================
    // NON-PHYSICS MOVEMENT
    // ========================================

    // Cheap local checks run before CanRun(), which makes
    // two Networking.IsOwner calls. Physics-mode objects and
    // idle objects therefore return after a single bool test.

    private void Update()
    {
        if (!armed ||
            physicsMovement ||
            movementVelocity.sqrMagnitude <= 0f)
            return;

        // No Rigidbody: move the Transform.

        if (Utilities.IsValid(body) || !CanRun())
            return;

        transform.position +=
            movementVelocity * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!armed ||
            physicsMovement ||
            movementVelocity.sqrMagnitude <= 0f)
            return;

        // Kinematic movement:
        // MovePosition allows trigger detection.

        if (!Utilities.IsValid(body) ||
            !body.isKinematic ||
            !CanRun())
            return;

        body.MovePosition(
            body.position +
            movementVelocity *
            Time.fixedDeltaTime
        );
    }

    // ========================================
    // COLLISION EVENTS
    // ========================================

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = null;

        if (Utilities.IsValid(collision))
            other = collision.gameObject;

        TraceContact("OnCollisionEnter", other);

        ReportImpact(other);
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        GameObject other = null;

        if (Utilities.IsValid(otherCollider))
            other = otherCollider.gameObject;

        TraceContact("OnTriggerEnter", other);

        ReportImpact(other);
    }

    // ========================================
    // VRCHAT PLAYER CONTACT
    // ========================================

    // These callbacks are intentionally
    // separate from ordinary Unity collisions.
    //
    // Player contact reporting is included
    // for diagnostics but does not automatically
    // despawn the object. This avoids a projectile
    // returning immediately because it touches
    // the spawning player's avatar.

    public override void OnPlayerCollisionEnter(
        VRCPlayerApi player)
    {
        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PooledObject] " +
                "OnPlayerCollisionEnter " +
                gameObject.name +
                " | Armed=" + armed
            );
        }
    }

    public override void OnPlayerTriggerEnter(
        VRCPlayerApi player)
    {
        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PooledObject] " +
                "OnPlayerTriggerEnter " +
                gameObject.name +
                " | Armed=" + armed
            );
        }
    }

    // ========================================
    // COLLISION TRACE
    // ========================================

    private void TraceContact(
        string eventName,
        GameObject other)
    {
        if (!debugLogs)
            return;

        string otherName = "NULL";

        if (Utilities.IsValid(other))
            otherName = other.name;

        bool managerValid =
            Utilities.IsValid(poolManager);

        bool managerOwned = false;

        if (managerValid)
        {
            managerOwned =
                Networking.IsOwner(
                    poolManager.gameObject
                );
        }

        Debug.Log(
            "[PXR_PooledObject] CONTACT " +
            " | Event=" + eventName +
            " | Object=" + gameObject.name +
            " | Other=" + otherName +
            " | Armed=" + armed +
            " | ObjectOwner=" +
                Networking.IsOwner(gameObject) +
            " | ManagerOwner=" + managerOwned
        );
    }

    // ========================================
    // REPORT IMPACT TO POOL OWNER
    // ========================================

    // Kept public for compatibility with
    // optional collision relays.
    //
    // The manager, not this component,
    // controls all object despawning.

    public void ReportImpact(GameObject other)
    {
        // Fast exit for bouncing objects whose impacts the
        // manager would ignore anyway. Diagnostics keep the
        // full path so debug output is unchanged.
        if (!debugLogs &&
            (!armed ||
             (!reportCollisionImpacts && !actsAsKey)))
            return;

        if (!CanRun())
        {
            if (debugLogs)
            {
                Debug.LogWarning(
                    "[PXR_PooledObject] " +
                    "IMPACT REJECTED " +
                    gameObject.name +
                    " | Armed=" + armed +
                    " | Active=" +
                        gameObject.activeInHierarchy +
                    " | ObjectOwner=" +
                        Networking.IsOwner(gameObject)
                );
            }

            return;
        }

        PXR_UnifiedInputs hitLock = null;

        if (actsAsKey &&
            Utilities.IsValid(other))
        {
            hitLock =
                other.GetComponent<
                    PXR_UnifiedInputs>();

            if (Utilities.IsValid(hitLock) &&
                hitLock.interactionType !=
                    PXR_InputType.Key)
            {
                hitLock = null;
            }
        }

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_PooledObject] " +
                "FORWARDING IMPACT " +
                gameObject.name +
                " | Slot=" + poolSlot +
                " | Generation=" + spawnGeneration +
                " | KeyLock=" +
                    Utilities.IsValid(hitLock)
            );
        }

        poolManager.NotifyImpact(
            poolSlot,
            spawnGeneration,
            hitLock
        );
    }
}
