
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TrashItem : UdonSharpBehaviour
{
    public GameObject trashVisual;
    public Collider trashCollider;
    public VRCPickup pickup;

    [UdonSynced] private bool isTrashed = false;

    private void Start()
    {
        UpdateTrash();
    }

    public override void OnPickup()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void DisposeTrash()
    {
        if (isTrashed) return;
        if (pickup.IsHeld) return;
        if (!Networking.IsOwner(gameObject)) return;

        isTrashed = true;
        UpdateTrash();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        UpdateTrash();
    }

    private void UpdateTrash()
    {
        trashVisual.SetActive(!isTrashed);
        trashCollider.enabled = !isTrashed;
        pickup.pickupable = !isTrashed;
    }
}

