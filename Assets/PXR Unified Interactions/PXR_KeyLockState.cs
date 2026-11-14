using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

// Persistent key state is separated from PXR_UnifiedInputs
// so physical pickups can use VRCObjectSync.
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_KeyLockState : UdonSharpBehaviour
{
    [Header("Key Lock Input")]
    [Tooltip("The PXR_UnifiedInputs configured as a Key lock.")]
    public PXR_UnifiedInputs input;

    [Header("Synchronized Key State (Runtime)")]
    [UdonSynced] public int keyCount;
    [UdonSynced] public bool keysComplete;

    // Slot and generation identities for the current cycle.
    [UdonSynced]
    private int[] registeredPoolSlots = new int[0];
    [UdonSynced]
    private int[] registeredPoolGenerations = new int[0];

    // Last accepted generation PER POOL SLOT, retained across
    // repeatable cycles. Prevents acknowledgment retries and
    // repeated collisions from counting a projectile twice.
    [UdonSynced]
    private int[] lastAcceptedGenerationBySlot = new int[0];
    // The pool manager's owner can change, resetting its
    // owner-local generation sequence. Keep owner identity
    // with the last accepted generation to permit reuse.
    [UdonSynced]
    private int[] lastAcceptedPoolOwnerBySlot = new int[0];

    // Ordinary keys already accepted in the current cycle, by
    // Key.GetKeyId(). Synced because the client that registers a
    // key takes lock ownership, and the next owner must not count
    // the same key twice in a multi-key lock.
    [UdonSynced]
    private string[] countedKeyIds = new string[0];

    // Ordinary keys currently touching the lock, on this client.
    // These remain latched after a repeatable cycle completes until
    // they exit, so a resting key cannot repeatedly trigger a 1-key
    // lock.
    private GameObject[] latchedKeys;

    // Key components parallel to latchedKeys, used to release
    // latches for keys that were consumed (a disabled collider
    // never reports OnCollisionExit).
    private Key[] latchedKeyRefs;

    // Pooled-key report rate limit (events per second, 1-100).
    private const int KEY_EVENT_RATE = 20;

    private void Start()
    {
        DisableInteractive = true;
        latchedKeys = new GameObject[Mathf.Max(8, Needed())];
        latchedKeyRefs = new Key[latchedKeys.Length];
        InitializeKeyRecords();
    }

    private int Needed()
    {
        return Utilities.IsValid(input)
            ? Mathf.Max(1, input.requiredKeys) : 1;
    }

    private int PoolSize()
    {
        if (!Utilities.IsValid(input) ||
            !Utilities.IsValid(input.pooledKeyPool) ||
            !Utilities.IsValid(input.pooledKeyPool.objectPool) ||
            input.pooledKeyPool.objectPool.Pool == null)
            return 0;

        return input.pooledKeyPool.objectPool.Pool.Length;
    }

    private bool RecordsReady()
    {
        int needed = Needed();
        return registeredPoolSlots != null &&
            registeredPoolGenerations != null &&
            registeredPoolSlots.Length == needed &&
            registeredPoolGenerations.Length == needed;
    }

    private bool HistoryReady()
    {
        int poolSize = PoolSize();
        return poolSize > 0 &&
            lastAcceptedGenerationBySlot != null &&
            lastAcceptedPoolOwnerBySlot != null &&
            lastAcceptedGenerationBySlot.Length == poolSize &&
            lastAcceptedPoolOwnerBySlot.Length == poolSize;
    }

    private void InitializeKeyRecords()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        bool changed = false;

        if (!RecordsReady() && keyCount == 0)
        {
            int needed = Needed();
            registeredPoolSlots = new int[needed];
            registeredPoolGenerations = new int[needed];
            for (int i = 0; i < needed; i++)
            {
                registeredPoolSlots[i] = -1;
                registeredPoolGenerations[i] = -1;
            }
            changed = true;
        }

        if (EnsureCountedKeyIds())
            changed = true;

        int size = PoolSize();
        if (size > 0 && !HistoryReady())
        {
            int[] old = lastAcceptedGenerationBySlot;
            int[] oldOwners = lastAcceptedPoolOwnerBySlot;
            int[] next = new int[size];
            int[] nextOwners = new int[size];

            // Retain previously accepted generations when the
            // pool size changes, and migrate current records.
            if (old != null && oldOwners != null)
            {
                int previous = Mathf.Min(old.Length, oldOwners.Length);
                for (int i = 0; i < previous && i < size; i++)
                {
                    next[i] = old[i];
                    nextOwners[i] = oldOwners[i];
                }
            }

            if (registeredPoolSlots != null &&
                registeredPoolGenerations != null)
            {
                int n = Mathf.Min(
                    registeredPoolSlots.Length,
                    registeredPoolGenerations.Length);
                for (int i = 0; i < n; i++)
                {
                    int slot = registeredPoolSlots[i];
                    int gen = registeredPoolGenerations[i];
                    if (slot >= 0 && slot < size &&
                        gen > next[slot])
                    {
                        next[slot] = gen;
                        // This is a migration fallback for an
                        // existing partially counted lock.
                        VRCPlayerApi poolOwner = Networking.GetOwner(
                            input.pooledKeyPool.gameObject);
                        nextOwners[slot] = Utilities.IsValid(poolOwner)
                            ? poolOwner.playerId : -1;
                    }
                }
            }
            lastAcceptedGenerationBySlot = next;
            lastAcceptedPoolOwnerBySlot = nextOwners;
            changed = true;
        }

        if (changed)
            RequestSerialization();
    }

    private Key FindKey(Collision collision)
    {
        if (!Utilities.IsValid(collision))
            return null;

        GameObject other = collision.gameObject;
        if (!Utilities.IsValid(other))
            return null;

        // Pooled keys are owned by PXR_PoolManager.
        PXR_PooledObject pooled =
            other.GetComponent<PXR_PooledObject>();
        if (Utilities.IsValid(pooled))
            return null;

        Rigidbody rb = collision.rigidbody;
        if (Utilities.IsValid(rb))
        {
            pooled = rb.GetComponent<PXR_PooledObject>();
            if (Utilities.IsValid(pooled))
                return null;
        }

        Key key = other.GetComponent<Key>();
        if (!Utilities.IsValid(key))
            key = other.GetComponentInChildren<Key>();

        if (!Utilities.IsValid(key) && Utilities.IsValid(rb))
        {
            key = rb.GetComponent<Key>();
            if (!Utilities.IsValid(key))
                key = rb.GetComponentInChildren<Key>();
        }
        return key;
    }

    // Optional direct collider on the lock-state object.
    private void OnCollisionEnter(Collision collision)
    {
        HandleKeyCollision(collision);
    }

    // Also called from the PXR_UnifiedInputs collider.
    //
    // The client physically controlling the key decides: the holder,
    // or the owner of a free key's body. A key placed by hand is
    // moved by sync on every other screen and reports no contact
    // there. That client claims lock ownership (if needed) and
    // registers the key; every other client ignores the contact, so
    // one insertion is never counted twice.
    public void HandleKeyCollision(Collision collision)
    {
        if (keysComplete ||
            !Utilities.IsValid(input) ||
            input.interactionType != PXR_InputType.Key)
            return;

        Key key = FindKey(collision);
        if (!Utilities.IsValid(key))
            return;

        Rigidbody rb = collision.rigidbody;
        GameObject body = Utilities.IsValid(rb)
            ? rb.gameObject
            : collision.gameObject;

        if (!key.HasLocalAuthority(body) || !CanAcceptOrdinaryKey(key))
            return;

        if (!Networking.IsOwner(gameObject))
        {
            VRCPlayerApi local = Networking.LocalPlayer;
            if (!Utilities.IsValid(local))
                return;

            Networking.SetOwner(local, gameObject);

            if (!Networking.IsOwner(gameObject))
                return;
        }

        TryRegisterOrdinaryKey(key);
    }

    private void OnCollisionExit(Collision collision)
    {
        NotifyOrdinaryKeyExit(collision);
    }

    // Also called from the PXR_UnifiedInputs collider.
    // Latches are per client, so every client clears its own,
    // including after lock ownership has moved elsewhere.
    public void NotifyOrdinaryKeyExit(Collision collision)
    {
        if (!HasAnyLatch())
            return;

        Key key = FindKey(collision);
        if (!Utilities.IsValid(key) || latchedKeys == null)
            return;

        for (int i = 0; i < latchedKeys.Length; i++)
        {
            if (latchedKeys[i] == key.gameObject)
                ClearLatch(i);
        }
    }

    private bool HasAnyLatch()
    {
        if (latchedKeys == null)
            return false;

        for (int i = 0; i < latchedKeys.Length; i++)
            if (Utilities.IsValid(latchedKeys[i]))
                return true;

        return false;
    }

    // Sizes the counted-key list to Required Keys, keeping entries.
    // Returns true when the array changed.
    private bool EnsureCountedKeyIds()
    {
        int needed = Needed();

        if (countedKeyIds != null && countedKeyIds.Length == needed)
            return false;

        string[] next = new string[needed];
        for (int i = 0; i < needed; i++)
        {
            next[i] = countedKeyIds != null && i < countedKeyIds.Length &&
                countedKeyIds[i] != null
                    ? countedKeyIds[i]
                    : "";
        }

        countedKeyIds = next;
        return true;
    }

    private bool IsCounted(string id)
    {
        if (countedKeyIds == null || string.IsNullOrEmpty(id))
            return false;

        for (int i = 0; i < countedKeyIds.Length; i++)
            if (countedKeyIds[i] == id)
                return true;

        return false;
    }

    // Every rule an ordinary key must pass, checked against this
    // client's view. Used before claiming ownership, so a key that
    // would be rejected never moves the lock to another player.
    private bool CanAcceptOrdinaryKey(Key key)
    {
        if (!Utilities.IsValid(input) ||
            input.interactionType != PXR_InputType.Key ||
            !Utilities.IsValid(key) ||
            keysComplete || key.IsConsumed())
            return false;

        if (input.checkKeyTag &&
            (string.IsNullOrEmpty(input.requiredKeyTag) ||
             key.tag != input.requiredKeyTag))
            return false;

        if (keyCount >= Needed() || IsLatched(key.gameObject))
            return false;

        return !IsCounted(key.GetKeyId());
    }

    private void ClearLatch(int index)
    {
        latchedKeys[index] = null;

        if (latchedKeyRefs != null &&
            index < latchedKeyRefs.Length)
            latchedKeyRefs[index] = null;
    }

    // Also releases latches whose key can no longer report
    // an exit: deactivated, destroyed or consumed keys.
    private bool IsLatched(GameObject obj)
    {
        if (latchedKeys == null)
            return false;

        bool found = false;

        for (int i = 0; i < latchedKeys.Length; i++)
        {
            GameObject latched = latchedKeys[i];

            if (!Utilities.IsValid(latched))
                continue;

            Key latchedKey =
                latchedKeyRefs != null && i < latchedKeyRefs.Length
                    ? latchedKeyRefs[i]
                    : null;

            if (!latched.activeInHierarchy ||
                (Utilities.IsValid(latchedKey) &&
                 latchedKey.IsConsumed()))
            {
                ClearLatch(i);
                continue;
            }

            if (latched == obj)
                found = true;
        }

        return found;
    }

    private void Latch(Key key)
    {
        GameObject obj = key.gameObject;

        if (latchedKeys == null)
            latchedKeys = new GameObject[8];

        if (latchedKeyRefs == null ||
            latchedKeyRefs.Length != latchedKeys.Length)
        {
            Key[] refs = new Key[latchedKeys.Length];

            if (latchedKeyRefs != null)
            {
                int n = Mathf.Min(
                    latchedKeyRefs.Length, refs.Length);
                for (int i = 0; i < n; i++)
                    refs[i] = latchedKeyRefs[i];
            }

            latchedKeyRefs = refs;
        }

        for (int i = 0; i < latchedKeys.Length; i++)
        {
            if (!Utilities.IsValid(latchedKeys[i]))
            {
                latchedKeys[i] = obj;
                latchedKeyRefs[i] = key;
                return;
            }
        }

        // Grow only if many distinct ordinary keys remain
        // within the lock without exiting.
        int oldSize = latchedKeys.Length;
        GameObject[] grown = new GameObject[oldSize * 2];
        Key[] grownRefs = new Key[oldSize * 2];
        for (int i = 0; i < oldSize; i++)
        {
            grown[i] = latchedKeys[i];
            grownRefs[i] = latchedKeyRefs[i];
        }
        grown[oldSize] = obj;
        grownRefs[oldSize] = key;
        latchedKeys = grown;
        latchedKeyRefs = grownRefs;
    }

    public void TryRegisterOrdinaryKey(Key key)
    {
        if (!Networking.IsOwner(gameObject) ||
            !CanAcceptOrdinaryKey(key))
            return;

        EnsureCountedKeyIds();

        int userId = key.GetAttributedPlayerId();
        string userName = key.GetAttributedPlayerName();

        // Latch BEFORE dispatching completion, which may
        // synchronously reset the cycle when Repeatable is on.
        Latch(key);
        if (keyCount < countedKeyIds.Length)
            countedKeyIds[keyCount] = key.GetKeyId();
        CompleteRegistration(userId, userName);

        // Unchanged: consumed keys only when the lock has
        // Destroy Key After Use explicitly enabled.
        if (input.destroyKeyAfterUse)
            key.ConsumeAsLocalPlayer();
    }

    [NetworkCallable(KEY_EVENT_RATE)]
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

        // Outside a network call (lock owner also owns the pool),
        // the caller is the local player.
        if (!Utilities.IsValid(sender))
            sender = Networking.LocalPlayer;

        VRCPlayerApi poolOwner = Networking.GetOwner(pool.gameObject);
        if (!Utilities.IsValid(sender) ||
            !Utilities.IsValid(poolOwner) ||
            sender.playerId != poolOwner.playerId ||
            pool.FindLockIndex(this) != lockIndex)
            return;

        PXR_PooledObject pooled = pool.GetPooledObject(slot);
        if (!Utilities.IsValid(pooled) || !pooled.actsAsKey)
            return;

        InitializeKeyRecords();
        if (!RecordsReady() || !HistoryReady() ||
            slot >= lastAcceptedGenerationBySlot.Length)
            return; // Pool manager will retry.

        bool tagMatches = !input.checkKeyTag ||
            (!string.IsNullOrEmpty(input.requiredKeyTag) &&
             pooled.keyTag == input.requiredKeyTag);
        bool accepted = false;

        if (tagMatches)
        {
            // A retry or an older generation is acknowledged
            // without advancing the next repeatable cycle.
            if (poolOwner.playerId == lastAcceptedPoolOwnerBySlot[slot] &&
                generation <= lastAcceptedGenerationBySlot[slot])
            {
                accepted = true;
            }
            else if (!keysComplete && keyCount < Needed())
            {
                registeredPoolSlots[keyCount] = slot;
                registeredPoolGenerations[keyCount] = generation;
                lastAcceptedGenerationBySlot[slot] = generation;
                lastAcceptedPoolOwnerBySlot[slot] = poolOwner.playerId;
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

    // Clears only one counting cycle. The per-slot generation
    // history and the currently-touching ordinary key latch
    // must survive a repeatable auto reset.
    private void ClearCycle()
    {
        keyCount = 0;
        keysComplete = false;

        int needed = Needed();
        countedKeyIds = new string[needed];
        for (int i = 0; i < needed; i++)
            countedKeyIds[i] = "";
        if (!RecordsReady())
        {
            registeredPoolSlots = new int[needed];
            registeredPoolGenerations = new int[needed];
        }
        for (int i = 0; i < needed; i++)
        {
            registeredPoolSlots[i] = -1;
            registeredPoolGenerations[i] = -1;
        }
    }

    private void CompleteRegistration(int userId, string userName)
    {
        if (!Networking.IsOwner(gameObject))
            return;

        keyCount++;
        if (keyCount >= Needed())
        {
            keysComplete = true;

            // This invokes the existing UnifiedInputs output
            // pipeline with the final accepted key's identity.
            if (Utilities.IsValid(input))
                input.CompleteKeyFromState(userId, userName);

            if (Utilities.IsValid(input) && input.repeatable)
                ClearCycle();
        }

        // Serializes the current count/state and persistent
        // generation history. For Repeatable, completed cycles
        // settle back at zero after the output is dispatched.
        RequestSerialization();
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

        // Manual reset also preserves per-slot history so an
        // already-fired projectile cannot count a second time.
        ClearCycle();
        RequestSerialization();
    }
}
