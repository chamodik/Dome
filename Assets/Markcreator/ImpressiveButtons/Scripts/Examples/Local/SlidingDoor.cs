
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SlidingDoor : UdonSharpBehaviour
{
    public float speed = 2f;
    Vector3 startPosition;
    Vector3 targetPosition;
    bool opening = false;

    void Start()
    {
        startPosition = gameObject.transform.localPosition;
        targetPosition = startPosition + new Vector3(-1, 0, 0);
    }

    void Update()
    {
        if(opening)
        {
            if(Vector3.Distance(gameObject.transform.localPosition, targetPosition) > 0.01f) gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, targetPosition, speed * Time.deltaTime);
        } else
        {
            if (Vector3.Distance(gameObject.transform.localPosition, startPosition) > 0.01f) gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, startPosition, speed * Time.deltaTime);
        }
    }

    public void _OnPress()
    {
        opening = !opening;
    }
}
