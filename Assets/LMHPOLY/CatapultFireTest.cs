
using UdonSharp;
using UnityEngine;

public class CatapultFireTest : UdonSharpBehaviour
{
    public Transform armHingePivot;
    public Vector3 firedRotation;
    public float fireDuration = 0.35f;

    // Star particle effect
    public ParticleSystem starParticles;

    private Quaternion startRotation;
    private Quaternion endRotation;
    private float timer;
    private bool isFiring;

    public override void Interact()
    {
        if (armHingePivot == null || isFiring) return;

        startRotation = armHingePivot.localRotation;
        endRotation = Quaternion.Euler(firedRotation);

        timer = 0f;
        isFiring = true;

        // Activate stars when catapult fires
        if (starParticles != null)
        {
            starParticles.Play();
        }
    }

    void Update()
    {
        if (!isFiring) return;

        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            timer / Mathf.Max(fireDuration, 0.05f)
        );

        armHingePivot.localRotation = Quaternion.Slerp(
            startRotation,
            endRotation,
            progress
        );

        if (progress >= 1f)
            isFiring = false;
    }
}
