
using UdonSharp;
using UnityEngine;

// Send onPress event along to multiple other scripts
// No support for named press events (yet)
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RedirectOnPressEvent : UdonSharpBehaviour
{
    [SerializeField] UdonSharpBehaviour[] redirectToBehaviors;
    [SerializeField] string customRedirectEventName = "_OnPress";


    public void _OnPress()
    {
        foreach (UdonSharpBehaviour b in redirectToBehaviors) if (b != null) b.SendCustomEvent(customRedirectEventName);
    }
}
