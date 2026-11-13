
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DisableObject : UdonSharpBehaviour
{
    public GameObject targetObject;

    public void _OnPress()
    {
        targetObject.SetActive(false);
    }
}
