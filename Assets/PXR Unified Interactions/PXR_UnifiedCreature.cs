
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

using Debug = UnityEngine.Debug;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class PXR_UnifiedCreature : UdonSharpBehaviour
{
    // =====================================
    // COMPONENTS
    // =====================================

    [Header("Components")]
    public NavMeshAgent navAgent;
    public Animator animator;

    [Header("Animation")]
    public string walkingBoolName = "IsWalking";

    // =====================================
    // SEQUENCE SETTINGS
    // =====================================

    [Header("Sequence")]
    public bool playOnStart = true;
    public bool loop = true;

    // Parallel arrays form an ordered list:
    //
    // stepTypes:      0 = Stand, 1 = Move
    // standDurations: seconds for Stand
    // moveTargets:    destination for Move
    // moveSpeeds:     units/second for Move
    //
    // PXR_UnifiedCreatureEditor presents
    // these together as one sequence.

    [HideInInspector]
    public int[] stepTypes = new int[0];

    [HideInInspector]
    public float[] standDurations = new float[0];

    [HideInInspector]
    public Transform[] moveTargets = new Transform[0];

    [HideInInspector]
    public float[] moveSpeeds = new float[0];

    // =====================================
    // NAVIGATION SETTINGS
    // =====================================

    [Header("Navigation")]

    [Min(0.05f)]
    public float arrivalDistance = 0.25f;

    [Tooltip(
        "Maximum seconds for a Move step before " +
        "skipping an unreachable or stuck target."
    )]
    [Min(1f)]
    public float maxMoveSeconds = 30f;

    [Header("Diagnostics")]
    public bool debugLogs = false;

    // =====================================
    // SYNCHRONIZED DISPLAY STATE
    // =====================================

    [UdonSynced]
    private int syncedStepIndex = -1;

    [UdonSynced]
    private bool syncedWalking = false;

<<<<<<< Updated upstream
=======
    // Whether the owner's sequence is running. A new owner
    // resumes this state instead of falling back to
    // playOnStart. syncedRunStateValid is false until the
    // first owner has set it.
    [UdonSynced]
    private bool syncedRunning = false;

    [UdonSynced]
    private bool syncedRunStateValid = false;

>>>>>>> Stashed changes
    // =====================================
    // OWNER-LOCAL SEQUENCE STATE
    // =====================================

    private bool authorityConfigured;
    private bool wasOwner;

    private bool running;
    private bool stepStarted;
    private bool skipCurrentStep;

    private int currentStep;

    private float standEndTime;
    private float moveStartTime;
    private float nextNavMeshWarning;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        // This component never provides
        // an Interact() callback.
        DisableInteractive = true;

        if (!Utilities.IsValid(navAgent))
            navAgent = GetComponent<NavMeshAgent>();

        if (!Utilities.IsValid(animator))
            animator = GetComponentInChildren<Animator>();

        if (!Utilities.IsValid(navAgent))
        {
            Debug.LogError(
                "[PXR_UnifiedCreature] Missing " +
                "NavMeshAgent on " + gameObject.name
            );
            return;
        }

        ApplyAnimation(syncedWalking);
        ConfigureAuthority(Networking.IsOwner(gameObject));
    }

    // =====================================
    // OWNERSHIP
    // =====================================

    private void ConfigureAuthority(bool owner)
    {
        authorityConfigured = true;
        wasOwner = owner;

        if (!Utilities.IsValid(navAgent))
            return;

        // Only the owner performs pathfinding
        // and moves the creature.
        //
        // Other clients receive its transform
        // through VRCObjectSync.

        navAgent.enabled = owner;

        if (owner)
        {
            // When ownership changes, restart
            // the currently synchronized step
            // rather than restarting the entire
            // sequence from step zero.

            currentStep = syncedStepIndex >= 0
                ? syncedStepIndex
                : 0;

            if (stepTypes != null &&
                currentStep >= stepTypes.Length)
            {
                currentStep = 0;
            }

<<<<<<< Updated upstream
            running = playOnStart;
=======
            SetRunning(
                syncedRunStateValid
                    ? syncedRunning
                    : playOnStart
            );

>>>>>>> Stashed changes
            stepStarted = false;
            skipCurrentStep = false;

            if (debugLogs)
            {
                Debug.Log(
                    "[PXR_UnifiedCreature] " +
                    "Local client became owner. " +
                    "Step=" + currentStep
                );
            }
        } else
        {
            running = false;
            stepStarted = false;
            skipCurrentStep = false;

            ApplyAnimation(syncedWalking);
        }
    }

    public override void OnOwnershipTransferred(
        VRCPlayerApi newOwner)
    {
        // Reconfigure on the next Update.
        authorityConfigured = false;
    }

    public override void OnDeserialization()
    {
        if (!Networking.IsOwner(gameObject))
            ApplyAnimation(syncedWalking);
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        if (!Utilities.IsValid(navAgent))
            return;

        bool owner = Networking.IsOwner(gameObject);

        if (!authorityConfigured ||
            owner != wasOwner)
        {
            ConfigureAuthority(owner);
        }

        if (!owner || !running)
            return;

        if (stepTypes == null ||
            stepTypes.Length == 0)
        {
            SetWalking(false);
            return;
        }

        // A NavMeshAgent must be positioned on
        // a valid NavMesh before it can navigate.
        //
        // This also allows runtime-generated
        // NavMeshes time to become available.

        if (!navAgent.isOnNavMesh)
        {
            SetWalking(false);

            if (debugLogs &&
                Time.realtimeSinceStartup >=
                nextNavMeshWarning)
            {
                nextNavMeshWarning =
                    Time.realtimeSinceStartup + 5f;

                Debug.LogWarning(
                    "[PXR_UnifiedCreature] " +
                    "Agent is not on a NavMesh: " +
                    gameObject.name
                );
            }

            return;
        }

        if (!stepStarted)
        {
            BeginStep(currentStep);
            return;
        }

        if (skipCurrentStep)
        {
            AdvanceStep();
            return;
        }

        int type = stepTypes[currentStep];

        if (type == 0)
        {
            // STAND
            if (Time.realtimeSinceStartup >=
                standEndTime)
            {
                AdvanceStep();
            }
        } else
        {
            // MOVE
            UpdateMoveStep();
        }
    }

    // =====================================
    // START A SEQUENCE STEP
    // =====================================

    private void BeginStep(int index)
    {
        if (!Networking.IsOwner(gameObject) ||
            !navAgent.isOnNavMesh)
            return;

        if (index < 0 ||
            index >= stepTypes.Length)
            return;

        currentStep = index;
        stepStarted = true;
        skipCurrentStep = false;

        syncedStepIndex = index;

        int type = stepTypes[index];

        if (type == 0)
        {
            BeginStand(index);
        } else
        {
            BeginMove(index);
        }
    }

    // =====================================
    // STAND
    // =====================================

    private void BeginStand(int index)
    {
        navAgent.isStopped = true;
        navAgent.ResetPath();

        SetWalking(false);

        float duration = 0f;

        if (standDurations != null &&
            index < standDurations.Length)
        {
            duration = standDurations[index];
        }

        // Prevent a zero-duration looping
        // sequence from consuming many steps
        // in the same frame.

        duration = Mathf.Max(0.05f, duration);

        standEndTime =
            Time.realtimeSinceStartup + duration;

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_UnifiedCreature] STAND " +
                "step=" + index +
                " duration=" + duration
            );
        }
    }

    // =====================================
    // MOVE
    // =====================================

    private void BeginMove(int index)
    {
        Transform destination = null;

        if (moveTargets != null &&
            index < moveTargets.Length)
        {
            destination = moveTargets[index];
        }

        if (!Utilities.IsValid(destination))
        {
            Debug.LogWarning(
                "[PXR_UnifiedCreature] " +
                "Move step " + index +
                " has no destination. Skipping."
            );

            SetWalking(false);
            skipCurrentStep = true;
            return;
        }

        float speed = 1f;

        if (moveSpeeds != null &&
            index < moveSpeeds.Length)
        {
            speed = moveSpeeds[index];
        }

        navAgent.speed = Mathf.Max(0.05f, speed);

        navAgent.stoppingDistance =
            Mathf.Max(0.05f, arrivalDistance);

        navAgent.isStopped = false;

        // NavMeshAgent computes a route using
        // the baked or runtime NavMesh.

        bool accepted = navAgent.SetDestination(
            destination.position
        );

        if (!accepted)
        {
            Debug.LogWarning(
                "[PXR_UnifiedCreature] " +
                "NavMesh rejected destination " +
                "for step " + index
            );

            navAgent.isStopped = true;
            SetWalking(false);
            skipCurrentStep = true;
            return;
        }

        moveStartTime = Time.realtimeSinceStartup;

        SetWalking(true);

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_UnifiedCreature] MOVE " +
                "step=" + index +
                " target=" + destination.name +
                " speed=" + navAgent.speed
            );
        }
    }

    // =====================================
    // MOVE COMPLETION
    // =====================================

    private void UpdateMoveStep()
    {
        // NavMesh routes may take several
        // frames to calculate.

        if (!navAgent.pathPending)
        {
            float remaining =
                navAgent.remainingDistance;

            float tolerance =
                Mathf.Max(
                    0.05f,
                    arrivalDistance
                );

            // Only complete successfully at
            // the end of a complete route.

            if (navAgent.pathStatus ==
                    NavMeshPathStatus.PathComplete &&
                remaining <= tolerance &&
                navAgent.velocity.sqrMagnitude <=
                    0.04f)
            {
                AdvanceStep();
                return;
            }
        }

        // Never allow an unreachable target
        // to trap the sequence indefinitely.

        if (Time.realtimeSinceStartup -
            moveStartTime >=
            Mathf.Max(1f, maxMoveSeconds))
        {
            Debug.LogWarning(
                "[PXR_UnifiedCreature] " +
                "Move timed out at step " +
                currentStep
            );

            AdvanceStep();
        }
    }

    // =====================================
    // NEXT STEP
    // =====================================

    private void AdvanceStep()
    {
        int next = currentStep + 1;

        if (next >= stepTypes.Length)
        {
            if (!loop)
            {
                FinishSequence();
                return;
            }

            next = 0;
        }

        currentStep = next;
        stepStarted = false;
        skipCurrentStep = false;
    }

<<<<<<< Updated upstream
    private void FinishSequence()
    {
        running = false;
=======
    // Owner only: sets the local and synchronized run state.
    // Continuous sync propagates it to other clients.
    private void SetRunning(bool value)
    {
        running = value;

        if (!Networking.IsOwner(gameObject))
            return;

        syncedRunning = value;
        syncedRunStateValid = true;
    }

    private void FinishSequence()
    {
        SetRunning(false);
>>>>>>> Stashed changes
        stepStarted = false;

        if (navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        SetWalking(false);

        if (debugLogs)
        {
            Debug.Log(
                "[PXR_UnifiedCreature] " +
                "Sequence completed."
            );
        }
    }

    // =====================================
    // ANIMATION
    // =====================================

    private void SetWalking(bool walking)
    {
        if (!Networking.IsOwner(gameObject))
            return;

        syncedWalking = walking;
        ApplyAnimation(walking);

        // Continuous Udon synchronization
        // propagates syncedWalking to
        // non-owner clients.
    }

    private void ApplyAnimation(bool walking)
    {
        if (!Utilities.IsValid(animator))
            return;

        if (string.IsNullOrEmpty(walkingBoolName))
            return;

        animator.SetBool(walkingBoolName, walking);
    }

    // =====================================
    // OPTIONAL EXTERNAL CONTROLS
    // =====================================

    // Can be called from PXR_UnifiedInputs.
    // The request is routed to the creature
    // owner, who controls the sequence.

    [PXR_InputCallable]
    public void RestartSequence()
    {
        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerRestartSequence)
        );
    }

    [PXR_InputCallable]
    public void StopSequence()
    {
        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerStopSequence)
        );
    }

    [NetworkCallable]
    public void OwnerRestartSequence()
    {
        if (!Networking.IsOwner(gameObject))
            return;

<<<<<<< Updated upstream
        running = true;
=======
        SetRunning(true);
>>>>>>> Stashed changes
        currentStep = 0;
        stepStarted = false;
        skipCurrentStep = false;

        syncedStepIndex = 0;

        if (Utilities.IsValid(navAgent) &&
            navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }

        SetWalking(false);
    }

    [NetworkCallable]
    public void OwnerStopSequence()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        FinishSequence();
    }
}
