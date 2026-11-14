
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

    // True when this client is the one physically controlling the
    // key: it is holding it, or nobody holds it and it owns the
    // key's physics body. Only that client sees reliable contacts
    // (a held key is moved by sync on everyone else's screen), so
    // only that client may register the key with a lock.
    public bool HasLocalAuthority(GameObject body)
    {
        VRCPickup pickup = pickupReference;

        if (!Utilities.IsValid(pickup))
            pickup = GetComponent<VRCPickup>();

        if (!Utilities.IsValid(pickup) && Utilities.IsValid(body))
            pickup = body.GetComponent<VRCPickup>();

        if (Utilities.IsValid(pickup))
        {
            VRCPlayerApi holder = pickup.currentPlayer;

            if (Utilities.IsValid(holder))
                return holder.isLocal;

            return Networking.IsOwner(pickup.gameObject);
        }

        return Networking.IsOwner(
            Utilities.IsValid(body) ? body : gameObject
        );
    }

    // Identifies this key identically on every client, so a lock can
    // remember which keys it counted after its ownership changes.
    // Built from the scene hierarchy at load (root index and name,
    // then the sibling index at each level).
    private string keyId = "";

    public string GetKeyId()
    {
        if (keyId.Length > 0)
            return keyId;

        Transform t = transform;
        string path = "";

        while (t.parent != null)
        {
            path = "/" + t.GetSiblingIndex() + path;
            t = t.parent;
        }

        keyId = t.GetSiblingIndex() + ":" + t.name + path;
        return keyId;
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

    // True when Visual Root is an ancestor of this Key (commonly the
    // pickup root). Deactivating it would also deactivate this Key,
    // and VRChat sends no synced data from inactive objects, so other
    // players would never learn the key was consumed. In that case
    // the root is hidden in place: renderers and colliders off,
    // pickup disabled, body frozen.
    private bool visualRootContainsKey;
    private bool hiddenInPlace;

    private Renderer[] rootRenderers;
    private bool[] rootRendererStates;
    private Collider[] rootColliders;
    private bool[] rootColliderStates;
    private VRCPickup rootPickup;
    private bool rootPickupable;
    private Rigidbody rootBody;
    private bool rootBodyKinematic;
    private VRCObjectSync rootSync;

    private bool cached;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        CacheDefaults();
        ApplyConsumedState();

        // Capture the identity before anything can reparent the key.
        GetKeyId();
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

            visualRootContainsKey =
                transform.IsChildOf(visualRoot.transform);

            if (visualRootContainsKey)
                CacheRootState();
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

    private void CacheRootState()
    {
        rootRenderers =
            visualRoot.GetComponentsInChildren<Renderer>(true);
        rootRendererStates = new bool[rootRenderers.Length];
        for (int i = 0; i < rootRenderers.Length; i++)
            rootRendererStates[i] = rootRenderers[i].enabled;

        rootColliders =
            visualRoot.GetComponentsInChildren<Collider>(true);
        rootColliderStates = new bool[rootColliders.Length];
        for (int i = 0; i < rootColliders.Length; i++)
            rootColliderStates[i] = rootColliders[i].enabled;

        rootPickup = pickupReference;
        if (!Utilities.IsValid(rootPickup))
            rootPickup = visualRoot.GetComponent<VRCPickup>();
        if (Utilities.IsValid(rootPickup))
            rootPickupable = rootPickup.pickupable;

        rootBody = visualRoot.GetComponent<Rigidbody>();
        if (Utilities.IsValid(rootBody))
        {
            rootBodyKinematic = rootBody.isKinematic;
            rootSync = rootBody.GetComponent<VRCObjectSync>();
        }
    }

    // Runs on every client when the consumed state changes.
    private void ApplyHiddenInPlace(bool hide)
    {
        if (hide == hiddenInPlace)
            return;

        hiddenInPlace = hide;

        for (int i = 0; i < rootRenderers.Length; i++)
        {
            if (Utilities.IsValid(rootRenderers[i]))
                rootRenderers[i].enabled =
                    hide ? false : rootRendererStates[i];
        }

        for (int i = 0; i < rootColliders.Length; i++)
        {
            if (Utilities.IsValid(rootColliders[i]))
                rootColliders[i].enabled =
                    hide ? false : rootColliderStates[i];
        }

        if (Utilities.IsValid(rootPickup))
        {
            if (hide)
            {
                VRCPlayerApi holder = rootPickup.currentPlayer;
                if (Utilities.IsValid(holder) && holder.isLocal)
                    rootPickup.Drop();
            }

            rootPickup.pickupable = hide ? false : rootPickupable;
        }

        // Freeze the body so a collider-less key doesn't fall out of
        // the world and keep streaming movement. With VRCObjectSync,
        // only the body's owner changes it; the kinematic state syncs.
        if (!Utilities.IsValid(rootBody))
            return;

        if (Utilities.IsValid(rootSync))
        {
            if (!Networking.IsOwner(rootBody.gameObject))
                return;

            if (hide && !rootBody.isKinematic)
            {
                rootBody.velocity = Vector3.zero;
                rootBody.angularVelocity = Vector3.zero;
            }

            rootSync.SetKinematic(hide ? true : rootBodyKinematic);
            return;
        }

        if (hide && !rootBody.isKinematic)
        {
            rootBody.velocity = Vector3.zero;
            rootBody.angularVelocity = Vector3.zero;
        }

        rootBody.isKinematic = hide ? true : rootBodyKinematic;
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

    // Called by the client that registered this key with a lock (the
    // one physically controlling it). Taking ownership of the Key and
    // consuming locally avoids relying on an event reaching whoever
    // owns this object, which can differ from the pickup's owner when
    // Key sits on a child. Falls back to the owner event if ownership
    // cannot be taken.
    public void ConsumeAsLocalPlayer()
    {
        if (consumed)
            return;

        if (!Networking.IsOwner(gameObject))
        {
            VRCPlayerApi local = Networking.LocalPlayer;

            if (Utilities.IsValid(local))
                Networking.SetOwner(local, gameObject);

            if (!Networking.IsOwner(gameObject))
            {
                RequestConsume();
                return;
            }
        }

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
            if (visualRootContainsKey)
            {
                ApplyHiddenInPlace(consumed);
            }
            else
            {
                visualRoot.SetActive(
                    consumed
                        ? false
                        : originalVisualState
                );
            }
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
