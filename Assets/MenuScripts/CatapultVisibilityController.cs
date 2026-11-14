
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class CatapultVisibilityController : UdonSharpBehaviour
{
    [Header("References")]
    public GameObject CatapultRoot;
    public Toggle CatapultVisibilityToggle;
    public WorldAccessManager accessManager;

    [UdonSynced]
    private bool catapultVisible = true;

    private bool updatingToggleFromCode = false;

    private void Start()
    {
        ApplyVisibility();
    }

    public void UpdateCatapultVisibility()
    {
        if (CatapultVisibilityToggle == null || updatingToggleFromCode)
            return;

        // Only players registered through the owl can change visibility.
        if (accessManager == null ||
            !accessManager.IsLocalPlayerAuthorized())
        {
            ApplyVisibility();
            return;
        }

        bool newVisibility = CatapultVisibilityToggle.isOn;

        if (newVisibility == catapultVisible)
            return;

        // The owner controls synchronized state.
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        catapultVisible = newVisibility;

        ApplyVisibility();
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (CatapultRoot != null)
        {
            CatapultRoot.SetActive(catapultVisible);
        }

        if (CatapultVisibilityToggle != null)
        {
            updatingToggleFromCode = true;
            CatapultVisibilityToggle.isOn = catapultVisible;
            updatingToggleFromCode = false;
        }
    }
}
