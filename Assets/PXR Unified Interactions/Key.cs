
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Key : UdonSharpBehaviour
{
    [Header("Key Settings")]
    public new string tag = "";

    // =====================================
    // TELEPORT USER ATTRIBUTION
    // =====================================

    [Header("Teleport User Attribution")]

    [Tooltip(
        "VRC Pickup on the physical key root. " +
        "Assign explicitly when Key is on a child."
    )]
    public VRCPickup pickupReference;

    public int GetAttributedPlayerId()
    {
        VRCPlayerApi player = GetAttributedPlayer();

        return Utilities.IsValid(player)
            ? player.playerId
            : -1;
    }

    public string GetAttributedPlayerName()
    {
        VRCPlayerApi player = GetAttributedPlayer();

        return Utilities.IsValid(player)
            ? player.displayName
            : "";
    }

    private VRCPlayerApi GetAttributedPlayer()
    {
        VRCPickup pickup = pickupReference;

        if (!Utilities.IsValid(pickup))
            pickup = GetComponent<VRCPickup>();

        if (!Utilities.IsValid(pickup))
            return null;

        VRCPlayerApi holder = pickup.currentPlayer;

        if (Utilities.IsValid(holder))
            return holder;

        return Networking.GetOwner(pickup.gameObject);
    }

    // =====================================
    // VISUAL SETTINGS
    // =====================================

    [Header("Visual Root")]

    [Tooltip(
        "Optional child containing only " +
        "the key's visuals."
    )]
    public GameObject visualRoot;

    [Header("Collision Colliders")]

    [Tooltip(
        "Colliders to disable when the key " +
        "is consumed."
    )]
    public Collider[] collisionColliders;

    // =====================================
    // SYNCHRONIZED STATE
    // =====================================

    [UdonSynced]
    private bool consumed;

    private bool[] originalColliderStates;
    private bool originalVisualState;

    // MeshRenderer on the same GameObject
    // as this Key component.
    private MeshRenderer localMeshRenderer;
    private bool originalMeshRendererState;

    private bool cached;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        CacheDefaults();
        ApplyConsumedState();
    }

    private void CacheDefaults()
    {
        if (cached)
            return;

        if (collisionColliders == null)
            collisionColliders = new Collider[0];

        originalColliderStates =
            new bool[collisionColliders.Length];

        for (int i = 0;
             i < collisionColliders.Length;
             i++)
        {
            if (Utilities.IsValid(collisionColliders[i]))
            {
                originalColliderStates[i] =
                    collisionColliders[i].enabled;
            }
        }

        if (Utilities.IsValid(visualRoot) &&
            visualRoot != gameObject)
        {
            originalVisualState = visualRoot.activeSelf;
        }

        // Find only a MeshRenderer attached
        // to this exact GameObject.
        //
        // Child renderers are not affected.

        localMeshRenderer =
            GetComponent<MeshRenderer>();

        if (Utilities.IsValid(localMeshRenderer))
        {
            originalMeshRendererState =
                localMeshRenderer.enabled;
        }

        cached = true;
    }

    private void OnEnable()
    {
        CacheDefaults();
        ApplyConsumedState();
    }

    public override void OnDeserialization()
    {
        ApplyConsumedState();
    }

    // =====================================
    // CONSUMPTION
    // =====================================

    public bool IsConsumed()
    {
        return consumed;
    }

    public void RequestConsume()
    {
        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerConsume)
        );
    }

    [NetworkCallable]
    public void OwnerConsume()
    {
        ConsumeGlobally();
    }

    public void ConsumeGlobally()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        if (consumed)
            return;

        consumed = true;

        ApplyConsumedState();
        RequestSerialization();
    }

    // =====================================
    // RESET
    // =====================================

    public void ResetForReuse()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        consumed = false;

        ApplyConsumedState();
        RequestSerialization();
    }

    public void HideKey()
    {
        ApplyConsumedState();
    }

    // =====================================
    // APPLY CONSUMED STATE
    // =====================================

    private void ApplyConsumedState()
    {
        CacheDefaults();

        if (!cached)
            return;

        // ---------------------------------
        // OPTIONAL VISUAL ROOT
        // ---------------------------------

        if (Utilities.IsValid(visualRoot) &&
            visualRoot != gameObject)
        {
            visualRoot.SetActive(
                consumed
                    ? false
                    : originalVisualState
            );
        }

        // ---------------------------------
        // LOCAL MESH RENDERER
        // ---------------------------------

        // Disable the MeshRenderer attached
        // to the same GameObject as Key.
        //
        // Restore its original state when
        // the Key is reset.

        if (Utilities.IsValid(localMeshRenderer))
        {
            localMeshRenderer.enabled =
                consumed
                    ? false
                    : originalMeshRendererState;
        }

        // ---------------------------------
        // COLLIDERS
        // ---------------------------------

        for (int i = 0;
             i < collisionColliders.Length;
             i++)
        {
            Collider col = collisionColliders[i];

            if (!Utilities.IsValid(col))
                continue;

            col.enabled = consumed
                ? false
                : originalColliderStates[i];
        }
    }
}
