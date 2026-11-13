
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class TeleportPlayer : UdonSharpBehaviour
{
    public Transform targetLocation;

    public void _OnPress()
    {
        if(Networking.LocalPlayer != null) Networking.LocalPlayer.TeleportTo(targetLocation.position, targetLocation.rotation);
    }
}
