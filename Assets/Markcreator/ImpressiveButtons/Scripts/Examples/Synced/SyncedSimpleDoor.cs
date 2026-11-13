
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedSimpleDoor : UdonSharpBehaviour
{
    [UdonSynced] Vector3 startPosition;
    [UdonSynced] Quaternion startRotation;
    [UdonSynced] bool open = false;

    void Start()
    {
        if (Networking.IsInstanceOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            startPosition = gameObject.transform.localPosition;
            startRotation = gameObject.transform.localRotation;
            RequestSerialization();
        }
    }

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        open = !open;
        RequestSerialization();
        OnDeserialization();
    }

    public override void OnDeserialization()
    {
        if (open)
        {
            gameObject.transform.localPosition = startPosition + new Vector3(-0.5f, 0, -0.5f);
            gameObject.transform.localRotation = startRotation * Quaternion.Euler(0, 90, 0); // Open door by 90 degrees
        }
        else
        {
            gameObject.transform.localPosition = startPosition;
            gameObject.transform.localRotation = startRotation;
        }
    }
}
