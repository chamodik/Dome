using UdonSharp;
using UnityEngine;

public class ThankYouBarrier : UdonSharpBehaviour
{
    public GameObject explosionEmitter;
    public GameObject thankYouObject;

    private bool hasTriggered = false;

    public void OnParticleCollision(GameObject other)
    {
        if (hasTriggered || other != explosionEmitter)
            return;

        hasTriggered = true;

        // Reveal Thank You
        if (thankYouObject != null)
        {
            thankYouObject.SetActive(true);
        }
    }
}
