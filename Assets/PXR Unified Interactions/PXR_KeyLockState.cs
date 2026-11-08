using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_KeyLockState : UdonSharpBehaviour
{
    [Header("Key Lock Input")]
    public PXR_UnifiedInputs input;

    [Header("Synchronized Key State (read only at runtime)")]
    [UdonSynced] public int keyCount;
    [UdonSynced] public bool keysComplete;

    [UdonSynced]
    private int[] registeredPoolSlots = new int[0];

    [UdonSynced]
    private int[] registeredPoolGenerations = new int[0];

    // Ordinary key identities remain local to the
    // lock-state owner, as in the original toolkit.
    private GameObject[] countedKeys;

    private void Start()
    {
        DisableInteractive = true;
        countedKeys = new GameObject[Needed()];
        InitializeKeyRecords();
    }

    private int Needed()
    {
        return Utilities.IsValid(input)
            ? Mathf.Max(1, input.requiredKeys) : 1;
    }

    private bool RecordsReady()
    {
        int needed = Needed();
        return registeredPoolSlots != null &&
            registeredPoolGenerations != null &&
            registeredPoolSlots.Length == needed &&
            registeredPoolGenerations.Length == needed;
    }

    private void InitializeKeyRecords()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        if (RecordsReady())
            return;

        // Never discard synchronized identities
        // while a partially completed lock is active.
        if (keyCount > 0)
            return;

        int needed = Needed();
        registeredPoolSlots = new int[needed];
        registeredPoolGenerations = new int[needed];
        for (int i = 0; i < needed; i++)
        {
            registeredPoolSlots[i] = -1;
            registeredPoolGenerations[i] = -1;
        }
        RequestSerialization();
    }

    // Optional: if a collider is placed on this
    // object's GameObject, it can receive contact
    // directly instead of relying on input forwarding.
    private void OnCollisionEnter(Collision collision)
    {
        if (!Utilities.IsValid(collision) ||
            !Utilities.IsValid(input) ||
            input.interactionType != PXR_InputType.Key)
            return;

        GameObject other = collision.gameObject;
        if (!Utilities.IsValid(other))
            return;

        PXR_PooledObject pooled =
            other.GetComponent<PXR_PooledObject>();
        if (Utilities.IsValid(pooled))
            return;

        Rigidbody rb = collision.rigidbody;
        if (Utilities.IsValid(rb) &&
            Utilities.IsValid(
                rb.GetComponent<PXR_PooledObject>()))
            return;

        Key key = other.GetComponent<Key>();
        if (!Utilities.IsValid(key))
            key = other.GetComponentInChildren<Key>();

        if (!Utilities.IsValid(key) &&
            Utilities.IsValid(rb))
        {
            key = rb.GetComponent<Key>();
            if (!Utilities.IsValid(key))
                key = rb.GetComponentInChildren<Key>();
        }

        if (Utilities.IsValid(key))
            TryRegisterOrdinaryKey(key);
    }

    // Called on the authoritative lock owner by
    // PXR_UnifiedInputs, or from this component's
    // OnCollisionEnter when it has its own collider.
    public void TryRegisterOrdinaryKey(Key key)
    {
        if (!Networking.IsOwner(gameObject) ||
            !Utilities.IsValid(input) ||
            input.interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(key) ||
            keysComplete || key.IsConsumed())
            return;

        if (input.checkKeyTag &&
            (string.IsNullOrEmpty(input.requiredKeyTag) ||
             key.tag != input.requiredKeyTag))
            return;

        int needed = Needed();
        if (keyCount >= needed)
            return;

        if (countedKeys == null ||
            countedKeys.Length != needed)
            countedKeys = new GameObject[needed];

        // Prevent re-counting an ordinary Key.
        for (int i = 0; i < countedKeys.Length; i++)
        {
            if (countedKeys[i] == key.gameObject)
                return;
        }

        // Attribute a pickup to its holder or
        // post-release owner. Do not guess an owner
        // for a non-pickup key.
        int userId = key.GetAttributedPlayerId();
        string userName = key.GetAttributedPlayerName();

        countedKeys[keyCount] = key.gameObject;
        CompleteRegistration(userId, userName);

        if (input.destroyKeyAfterUse)
            key.RequestConsume();
    }

    [NetworkCallable]
    public void OwnerReceivePooledKey(
        int slot,
        int generation,
        int lockIndex,
        int userId,
        string userName)
    {
        if (!Networking.IsOwner(gameObject) ||
            !Utilities.IsValid(input) ||
            input.interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(input.pooledKeyPool) ||
            slot < 0 || generation <= 0)
            return;

        PXR_PoolManager pool = input.pooledKeyPool;
        VRCPlayerApi sender = NetworkCalling.CallingPlayer;
        VRCPlayerApi poolOwner =
            Networking.GetOwner(pool.gameObject);

        if (!Utilities.IsValid(sender) ||
            !Utilities.IsValid(poolOwner) ||
            sender.playerId != poolOwner.playerId ||
            pool.FindLockIndex(this) != lockIndex)
            return;

        PXR_PooledObject pooled = pool.GetPooledObject(slot);
        if (!Utilities.IsValid(pooled) ||
            !pooled.actsAsKey)
            return;

        InitializeKeyRecords();
        if (!RecordsReady())
            return; // pool retries until records are ready

        bool accepted = false;
        bool tagMatches = !input.checkKeyTag ||
            (!string.IsNullOrEmpty(input.requiredKeyTag) &&
             pooled.keyTag == input.requiredKeyTag);

        if (tagMatches)
        {
            if (ContainsPooledKey(slot, generation))
            {
                // A duplicate request is acknowledged
                // but does not affect count or user.
                accepted = true;
            } else if (!keysComplete && keyCount < Needed())
            {
                registeredPoolSlots[keyCount] = slot;
                registeredPoolGenerations[keyCount] =
                    generation;
                accepted = true;
                CompleteRegistration(userId, userName);
            }
        }

        pool.SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(PXR_PoolManager.OwnerKeyAcknowledged),
            slot,
            generation,
            lockIndex,
            accepted
        );
    }

    private bool ContainsPooledKey(int slot, int generation)
    {
        if (!RecordsReady())
            return false;

        int count = Mathf.Min(
            keyCount, registeredPoolSlots.Length);
        for (int i = 0; i < count; i++)
        {
            if (registeredPoolSlots[i] == slot &&
                registeredPoolGenerations[i] == generation)
                return true;
        }
        return false;
    }

    private void CompleteRegistration(int userId, string userName)
    {
        if (!Networking.IsOwner(gameObject))
            return;

        keyCount++;
        if (keyCount >= Needed())
            keysComplete = true;

        RequestSerialization();

        if (keysComplete && Utilities.IsValid(input))
        {
            // Identity is passed directly for this
            // completion event; it need not be synced.
            input.CompleteKeyFromState(userId, userName);
        }
    }

    public int GetKeyCount()
    {
        return keyCount;
    }

    public bool AreKeysComplete()
    {
        return keysComplete;
    }

    public void ResetKeys()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        keyCount = 0;
        keysComplete = false;
        countedKeys = new GameObject[Needed()];

        int needed = Needed();
        registeredPoolSlots = new int[needed];
        registeredPoolGenerations = new int[needed];
        for (int i = 0; i < needed; i++)
        {
            registeredPoolSlots[i] = -1;
            registeredPoolGenerations[i] = -1;
        }
        RequestSerialization();
    }
}
