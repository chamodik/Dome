
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedRandomColorOnPress : UdonSharpBehaviour
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

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        color = GetComponent<Renderer>().material.color = new Color(
              Random.Range(0f, 1f),
              Random.Range(0f, 1f),
              Random.Range(0f, 1f)
          );
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        GetComponent<Renderer>().material.color = color;
    }
}
