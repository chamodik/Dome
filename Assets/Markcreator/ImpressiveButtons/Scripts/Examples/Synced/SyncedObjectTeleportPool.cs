
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedObjectTeleportPool : UdonSharpBehaviour
{
    public GameObject[] objectPool;
    public Transform targetLocation;
    [UdonSynced] int index = 0;

    public void _OnPress()
    {
        if (objectPool.Length > 0)
        {
            GameObject targetObject = objectPool[index];
            Networking.SetOwner(Networking.LocalPlayer, targetObject);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TriggerTeleport");

            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            index++;
            if (index > objectPool.Length - 1) index = 0;
            RequestSerialization();            
        }
    }

    public void TriggerTeleport()
    {
        GameObject targetObject = objectPool[index];
        targetObject.transform.SetPositionAndRotation(targetLocation.position, targetLocation.rotation);
    }
}
