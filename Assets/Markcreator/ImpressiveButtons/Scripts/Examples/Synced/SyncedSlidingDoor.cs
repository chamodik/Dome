
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedSlidingDoor : UdonSharpBehaviour
{
    public float speed = 2f;
    Vector3 targetPosition = new Vector3(-1, 0, 0);
    [UdonSynced] Vector3 startPosition;
    [UdonSynced] bool opening = false;

    void Start()
    {
        if (Networking.IsInstanceOwner)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            startPosition = gameObject.transform.localPosition;
            RequestSerialization();
        }
    }

    void Update()
    {
        if(opening)
        {
            if (Vector3.Distance(gameObject.transform.localPosition, startPosition + targetPosition) > 0.01f) gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, startPosition + targetPosition, speed * Time.deltaTime);
        } else
        {
            if (Vector3.Distance(gameObject.transform.localPosition, startPosition) > 0.01f) gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, startPosition, speed * Time.deltaTime);
        }
    }

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        opening = !opening;
        RequestSerialization();
    }
}
