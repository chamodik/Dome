
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggleObject : UdonSharpBehaviour
{
    public GameObject targetObject;
    [UdonSynced] public bool active = true;

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        targetObject.SetActive(active = !active);
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        targetObject.SetActive(active);
    }
}
