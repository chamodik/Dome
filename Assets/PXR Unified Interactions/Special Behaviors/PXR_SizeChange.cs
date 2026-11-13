
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PXR_SizeChange : UdonSharpBehaviour
{
    private const float MIN_HEIGHT = 0.2f;
    private const float MAX_HEIGHT = 10.0f;

    [Header("Administrator Exclusion")]

    [Tooltip(
        "Players listed in PXR_AdminNames will not " +
        "be resized. If no list is assigned, " +
        "the operation is cancelled."
    )]
    public PXR_AdminNames adminList;

    [Header("Possible Eye Heights (Metres)")]

    [Tooltip(
        "Absolute avatar eye heights in metres. " +
        "Each non-admin player randomly selects one. " +
        "Values are clamped to 0.2–10.0 metres."
    )]
    public float[] possibleSizes =
    {
        0.5f,
        1.0f,
        1.5f,
        2.0f
    };

    private void Start()
    {
        DisableInteractive = true;
    }

    public void ActivateButton()
    {
        SendCustomNetworkEvent(
            NetworkEventTarget.All,
            nameof(RandomizeSize)
        );
    }

    [NetworkCallable]

    [PXR_InputCallable]

    public void RandomizeSize()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (!Utilities.IsValid(localPlayer))
            return;

        if (!Utilities.IsValid(adminList))
        {
            Debug.LogWarning(
                "[PXR_SizeChange] Missing Admin List."
            );
            return;
        }

        // Administrators are excluded.
        if (adminList.IsLocalPlayerAdmin())
            return;

        if (possibleSizes == null ||
            possibleSizes.Length == 0)
        {
            Debug.LogWarning(
                "[PXR_SizeChange] No sizes configured."
            );
            return;
        }

        // Count valid positive, finite heights.
        int validCount = 0;

        for (int i = 0; i < possibleSizes.Length; i++)
        {
            float height = possibleSizes[i];

            if (height > 0f &&
                !float.IsNaN(height) &&
                !float.IsInfinity(height))
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            Debug.LogWarning(
                "[PXR_SizeChange] No valid heights found."
            );
            return;
        }

        // Select one valid entry uniformly.
        int selection = Random.Range(0, validCount);

        for (int i = 0; i < possibleSizes.Length; i++)
        {
            float height = possibleSizes[i];

            if (height <= 0f ||
                float.IsNaN(height) ||
                float.IsInfinity(height))
            {
                continue;
            }

            if (selection == 0)
            {
                float targetHeight = Mathf.Clamp(
                    height,
                    MIN_HEIGHT,
                    MAX_HEIGHT
                );

                localPlayer.SetAvatarEyeHeightByMeters(
                    targetHeight
                );

                return;
            }

            selection--;
        }
    }
}
