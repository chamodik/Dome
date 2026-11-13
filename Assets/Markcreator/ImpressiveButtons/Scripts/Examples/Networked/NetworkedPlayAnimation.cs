
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class NetworkedPlayAnimation : UdonSharpBehaviour
{
    public Animator animator;
    public string anim;

    public void _OnPress()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayAnimation");
    }

    public void PlayAnimation()
    {
        animator.Play(anim);
    }
}
