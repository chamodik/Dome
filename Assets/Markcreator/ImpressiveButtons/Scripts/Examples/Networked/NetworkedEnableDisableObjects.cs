
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class NetworkedEnableDisableObjects : UdonSharpBehaviour
{
    public GameObject[] toDisable;
    public GameObject[] toEnable;

    public void _OnPress()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TriggerEnableDisable");
    }

    public void TriggerEnableDisable()
    {
        foreach (GameObject disable in toDisable) disable.SetActive(false);
        foreach (GameObject enable in toEnable) enable.SetActive(true);
    }
}
