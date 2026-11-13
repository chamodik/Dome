
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PlayAnimation : UdonSharpBehaviour
{
    public Animator animator;
    public string anim;

    public void _OnPress()
    {
        animator.Play(anim);
    }
}
