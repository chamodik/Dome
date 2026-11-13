
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class EnableObject : UdonSharpBehaviour
{
    public GameObject targetObject;

    public void _OnPress()
    {
        targetObject.SetActive(true);
    }
}
