
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedColorSelect : UdonSharpBehaviour
{
    [UdonSynced] Color color;

    public void Start()
    {
        if (Networking.IsInstanceOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            color = GetComponent<Renderer>().material.color;
            RequestSerialization();
        }
    }

    public void _OnPress_Blue() {
        _SetColor(new Color(144f / 255, 175f / 255, 1));
    }

    public void _OnPress_Red()
    {
        _SetColor(Color.red);
    }

    public void _OnPress_Black()
    {
        _SetColor(Color.black);
    }

    public void _OnPress_Brown()
    {
        _SetColor(new Color(144f / 255, 85f / 255, 54f / 255));
    }

    public void _SetColor(Color newColor)
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        color = GetComponent<Renderer>().material.color = newColor;
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        GetComponent<Renderer>().material.color = color;
    }
}
