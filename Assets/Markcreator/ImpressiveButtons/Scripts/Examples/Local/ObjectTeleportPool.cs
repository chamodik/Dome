
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ObjectTeleportPool : UdonSharpBehaviour
{
    public GameObject[] objectPool;
    public Transform targetLocation;
    int index = 0;

    public void _OnPress()
    {
        if (objectPool.Length > 0)
        {
            GameObject targetObject = objectPool[index];
            targetObject.transform.SetPositionAndRotation(targetLocation.position, targetLocation.rotation);

            index++;
            if (index > objectPool.Length - 1) index = 0;
        }
    }
}
