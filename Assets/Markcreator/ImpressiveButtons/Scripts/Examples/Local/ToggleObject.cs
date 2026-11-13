
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ToggleObject : UdonSharpBehaviour
{
    public GameObject targetObject;

    public void _OnPress()
    {
        targetObject.SetActive(!targetObject.activeSelf);
    }
}
