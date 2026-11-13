
using UdonSharp;
using UnityEngine;

public class CatapultWindupSteps : UdonSharpBehaviour
{
    public Animator catapultAnimator;
    public int totalCranks = 10;
    public float stepDuration = 0.25f;

    private int crankCount = 0;
    private float progress = 0f;
    private float targetProgress = 0f;

    void Start()
    {
        catapultAnimator.Play("Base Layer.Windup", 0, 0f);
        catapultAnimator.speed = 0f;
    }

    public override void Interact()
    {
        if (crankCount >= totalCranks) return;

        crankCount++;
        targetProgress = (float)crankCount / totalCranks;

        Debug.Log("Catapult crank: " + crankCount);
    }

    
    void Update()
    {
        if (progress >= targetProgress) return;

        float speed = 1f / (totalCranks * stepDuration);

        progress = Mathf.MoveTowards(
            progress,
            targetProgress,
            speed * Time.deltaTime
        );

        catapultAnimator.Play(
            "Base Layer.Windup", 0, progress
        );

        catapultAnimator.Update(0f);
    }

}
