
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerHeightController : UdonSharpBehaviour
{
    [Header("References")]
    public WorldAccessManager accessManager;

    [Header("Height Settings")]
    public float TallMultiplier = 1.5f;
    public float SmallMultiplier = 0.5f;

    // 1 = Tall, 2 = Small
    [UdonSynced] private int[] assignedPlayerIds = new int[0];
    [UdonSynced] private int[] assignedHeightGroups = new int[0];
    [UdonSynced] private bool assignmentsActive = false;

    private float originalEyeHeight;
    private bool heightModified = false;
    private bool previousManualScalingAllowed;
    private int lastAppliedGroup = 0;
    private float nextCheckTime = 0f;

    // Called locally by the menu's Start button.
    [PXR_InputCallable]
    public void StartRandomHeights()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (accessManager == null ||
            !accessManager.IsPlayerWhitelisted(localPlayer))
            return;

        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerStartRandomHeights)
        );
    }

    [NetworkCallable]
    public void OwnerStartRandomHeights()
    {
        if (!Networking.IsOwner(gameObject) || accessManager == null)
            return;

        VRCPlayerApi caller = NetworkCalling.CallingPlayer;

        if (!accessManager.IsPlayerWhitelisted(caller))
            return;

        VRCPlayerApi[] players = VRCPlayerApi.GetPlayers();

        int eligibleCount = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (Utilities.IsValid(players[i]) &&
                !accessManager.IsPlayerWhitelisted(players[i]))
            {
                eligibleCount++;
            }
        }

        int[] eligibleIds = new int[eligibleCount];
        int index = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (Utilities.IsValid(players[i]) &&
                !accessManager.IsPlayerWhitelisted(players[i]))
            {
                eligibleIds[index] = players[i].playerId;
                index++;
            }
        }

        // Randomize eligible players.
        Utilities.ShuffleArray(eligibleIds);

        assignedPlayerIds = eligibleIds;
        assignedHeightGroups = new int[eligibleCount];

        for (int i = 0; i < eligibleCount; i++)
        {
            assignedHeightGroups[i] =
                i < (eligibleCount + 1) / 2 ? 1 : 2;
        }

        assignmentsActive = true;

        RequestSerialization();
        ApplyLocalAssignment();
    }

    // Called locally by the menu's Reset button.
    [PXR_InputCallable]
    public void ResetPlayerHeights()
    {
        VRCPlayerApi localPlayer = Networking.LocalPlayer;

        if (accessManager == null ||
            !accessManager.IsPlayerWhitelisted(localPlayer))
            return;

        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(OwnerResetPlayerHeights)
        );
    }

    [NetworkCallable]
    public void OwnerResetPlayerHeights()
    {
        if (!Networking.IsOwner(gameObject) || accessManager == null)
            return;

        VRCPlayerApi caller = NetworkCalling.CallingPlayer;

        if (!accessManager.IsPlayerWhitelisted(caller))
            return;

        assignmentsActive = false;
        assignedPlayerIds = new int[0];
        assignedHeightGroups = new int[0];

        RequestSerialization();
        ApplyLocalAssignment();
    }

    public override void OnDeserialization()
    {
        ApplyLocalAssignment();
    }

    private void Update()
    {
        // Recheck periodically in case the owl whitelist changes.
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + 1f;
            ApplyLocalAssignment();
        }
    }

    public void ApplyLocalAssignment()
    {
        VRCPlayerApi player = Networking.LocalPlayer;

        if (!Utilities.IsValid(player) || accessManager == null)
            return;

        int group = 0;

        if (assignmentsActive &&
            !accessManager.IsPlayerWhitelisted(player))
        {
            int count = Mathf.Min(
                assignedPlayerIds.Length,
                assignedHeightGroups.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (assignedPlayerIds[i] == player.playerId)
                {
                    group = assignedHeightGroups[i];
                    break;
                }
            }
        }

        if (group == lastAppliedGroup)
            return;

        if (group == 0)
        {
            RestoreLocalHeight(player);
            return;
        }

        if (!heightModified)
        {
            originalEyeHeight = player.GetAvatarEyeHeightAsMeters();
            previousManualScalingAllowed =
                player.GetManualAvatarScalingAllowed();

            heightModified = true;
        }

        float multiplier =
            group == 1 ? TallMultiplier : SmallMultiplier;

        player.SetManualAvatarScalingAllowed(false);

        player.SetAvatarEyeHeightByMeters(
            Mathf.Clamp(originalEyeHeight * multiplier, 0.1f, 100f)
        );

        lastAppliedGroup = group;
    }

    private void RestoreLocalHeight(VRCPlayerApi player)
    {
        if (heightModified)
        {
            player.SetAvatarEyeHeightByMeters(originalEyeHeight);
            player.SetManualAvatarScalingAllowed(
                previousManualScalingAllowed
            );

            heightModified = false;
        }

        lastAppliedGroup = 0;
    }
}
