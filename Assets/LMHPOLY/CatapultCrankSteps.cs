
using UdonSharp;
using UnityEngine;

public class CatapultCrankSteps : UdonSharpBehaviour
{
    public Transform armPivot;
    public int totalCranks = 10;

    public Vector3 startRotation =
        new Vector3(49.886f, -88.032f, -3.29f);

    public Vector3 woundRotation =
        new Vector3(-25.635f, -30.807f, -5.61f);

    private int crankCount = 0;

    void Start()
    {
        if (armPivot != null)
        {
            armPivot.localRotation =
                Quaternion.Euler(startRotation);
        }
    }

    public override void Interact()
    {
        Debug.Log("CATAPULT CLICK DETECTED!");
        
        if (armPivot == null) return;
        if (crankCount >= totalCranks) return;

        crankCount++;

        float progress =
            (float)crankCount / totalCranks;

        armPivot.localRotation = Quaternion.Slerp(
            Quaternion.Euler(startRotation),
            Quaternion.Euler(woundRotation),
            progress
        );

        Debug.Log("Crank: " + crankCount + "/10");
    }
}
