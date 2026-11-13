
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SimpleDoor : UdonSharpBehaviour
{
    Vector3 startPosition;
    Quaternion startRotation;

    void Start()
    {
        startPosition = gameObject.transform.localPosition;
        startRotation = gameObject.transform.localRotation;
    }

    public void _OnPress()
    {
        if (gameObject.transform.localRotation == startRotation)
        {
            gameObject.transform.localPosition = startPosition + new Vector3(-0.5f, 0, -0.5f);
            gameObject.transform.localRotation = startRotation * Quaternion.Euler(0, 90, 0); // Open door by 90 degrees
        } else
        {
            gameObject.transform.localPosition = startPosition;
            gameObject.transform.localRotation = startRotation;
        }
    }
}
