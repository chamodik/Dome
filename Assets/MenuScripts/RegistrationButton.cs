
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class RegistrationButton : UdonSharpBehaviour
{
    public WorldAccessManager accessManager;

    public override void Interact()
    {
        if (!Utilities.IsValid(accessManager))
            return;

        accessManager.RequestRegistration();
    }
}
