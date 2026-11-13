
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ToggleMaterial : UdonSharpBehaviour
{
    public bool on = true;
    public Renderer targetObject;
    public Material materialOn;
    public Material materialOff;

    void Start()
    {
        _UpdateMaterial();
    }

    public void _OnPress()
    {
        on = !on;
        _UpdateMaterial();
    }

    public void _UpdateMaterial()
    {
        targetObject.material = on ? materialOn : materialOff;
    }
}
