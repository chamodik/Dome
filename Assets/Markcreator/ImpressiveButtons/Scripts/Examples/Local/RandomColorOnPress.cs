
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class RandomColorOnPress : UdonSharpBehaviour
{
    public void _OnPress()
    {
        GetComponent<Renderer>().material.color = new Color(
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              Random.Range(0f, 1f)
          );
    }
}
