
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


[RequireComponent(typeof(VRC_Pickup))]
[RequireComponent(typeof(AudioSource))]
public class Flight : UdonSharpBehaviour
{
    private VRCPlayerApi playerLocal;
    private bool isActive;
    [SerializeField] float speed = 50f;
    [SerializeField] AudioSource flightAudio;

    void Start()
    {
        playerLocal = Networking.LocalPlayer;
    }

    private void OnPickupUseDown()
    {
        isActive = true;
        flightAudio.Play();
    }

    private void OnPickupUseUp()
    {
        isActive = false;
        flightAudio.Stop();
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            playerLocal.SetVelocity(Vector3.ClampMagnitude(playerLocal.GetVelocity() + transform.forward, speed));
        }
    }
}
