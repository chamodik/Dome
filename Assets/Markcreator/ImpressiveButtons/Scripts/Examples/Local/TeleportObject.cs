
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TeleportObject : UdonSharpBehaviour
{
    public GameObject targetObject;
    public Transform targetLocation;

    public void _OnPress()
    {
        targetObject.transform.SetPositionAndRotation(targetLocation.position, targetLocation.rotation);
    }
}
