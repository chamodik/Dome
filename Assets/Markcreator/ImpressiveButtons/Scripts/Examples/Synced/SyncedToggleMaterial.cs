
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggleMaterial : UdonSharpBehaviour
{
    [UdonSynced] public bool on = true;
    public Renderer targetObject;
    public Material materialOn;
    public Material materialOff;

    void Start()
    {
        _UpdateMaterial();
    }

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        on = !on;
        RequestSerialization();
        _UpdateMaterial();
    }

    public void _UpdateMaterial()
    {
        targetObject.material = on ? materialOn : materialOff;
    }

    public override void OnDeserialization()
    {
        _UpdateMaterial();
    }
}
