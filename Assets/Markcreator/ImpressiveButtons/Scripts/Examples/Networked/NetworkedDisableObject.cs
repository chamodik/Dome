
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class NetworkedDisableObject : UdonSharpBehaviour
{
    public GameObject targetObject;

    public void _OnPress()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TriggerDisable");
    }

    public void TriggerDisable()
    {
        targetObject.SetActive(false);
    }
}
