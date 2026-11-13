
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class NetworkedTeleportObject : UdonSharpBehaviour
{
    public GameObject targetObject;
    public Transform targetLocation;

    public void _OnPress()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TriggerTeleport");
    }

    public void TriggerTeleport()
    {
        targetObject.transform.SetPositionAndRotation(targetLocation.position, targetLocation.rotation);
    }
}
