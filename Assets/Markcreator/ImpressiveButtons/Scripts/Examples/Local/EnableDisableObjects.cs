
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class EnableDisableObjects : UdonSharpBehaviour
{
    public GameObject[] toDisable;
    public GameObject[] toEnable;

    public void _OnPress()
    {
        foreach (GameObject disable in toDisable) disable.SetActive(false);
        foreach (GameObject enable in toEnable) enable.SetActive(true);
    }
}
