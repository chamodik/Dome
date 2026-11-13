
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class NetworkedEnableObject : UdonSharpBehaviour
{
    public GameObject targetObject;

    public void _OnPress()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TriggerEnable");
    }

    public void TriggerEnable()
    {
        targetObject.SetActive(true);
    }
}
