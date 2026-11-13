
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[RequireComponent(typeof(Collider)), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ColliderTrigger : UdonSharpBehaviour
{
    private Collider col;
    private bool inside = false;

    public UdonSharpBehaviour[] enterEventReceivers;
    public UdonSharpBehaviour[] exitEventReceivers;
    public string customEnterEventName = "_EnterTrigger";
    public string customExitEventName = "_ExitTrigger";

    void Start()
    {
        col = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        if (Networking.LocalPlayer != null)
        {
            Vector3 playerPos = Networking.LocalPlayer.GetPosition();

            if (inside)
            {
                if(!col.bounds.Contains(playerPos))
                {
                    inside = false;
                    foreach (UdonSharpBehaviour b in exitEventReceivers) b.SendCustomEvent(customExitEventName);
                }
            }
            else
            {
                if (col.bounds.Contains(playerPos))
                {
                    inside = true;
                    foreach (UdonSharpBehaviour b in enterEventReceivers) b.SendCustomEvent(customEnterEventName);
                }
            }
        } 
    }
}
