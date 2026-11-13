
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

public class CrankHoldDetector : UdonSharpBehaviour
{
    public GameObject holdIndicator;

    private bool insideZone = false;
    private bool isHolding = false;

    void Start()
    {
        if (holdIndicator != null)
            holdIndicator.SetActive(false);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal) return;

        insideZone = true;
        Debug.Log("[Catapult] Entered crank zone");
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null || !player.isLocal) return;

        insideZone = false;
        StopHolding();
    }

    public override void InputUse(
        bool value,
        UdonInputEventArgs args)
    {
        if (value && insideZone && !isHolding)
        {
            isHolding = true;
            Debug.Log("[Catapult] Holding crank");

            if (holdIndicator != null)
                holdIndicator.SetActive(true);
        }
        else if (!value && isHolding)
        {
            StopHolding();
        }
    }

    private void StopHolding()
    {
        if (!isHolding) return;

        isHolding = false;
        Debug.Log("[Catapult] Released crank");

        if (holdIndicator != null)
            holdIndicator.SetActive(false);
    }
}

