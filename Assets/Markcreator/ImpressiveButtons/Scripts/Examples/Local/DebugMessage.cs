
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DebugMessage : UdonSharpBehaviour
{
    [SerializeField] string message;

    public void _OnPress()
    {
        Debug.Log("[DebugMessage] " + gameObject.name + ": " + message);
    }
}
