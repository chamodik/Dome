using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.UdonNetworkCalling;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class WorldAccessManager : UdonSharpBehaviour
{
    [UdonSynced]
    private int[] authorizedPlayerIds = new int[0];

    public bool IsLocalPlayerAuthorized()
    {
        VRCPlayerApi player = Networking.LocalPlayer;

        if (!Utilities.IsValid(player))
            return false;

        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            if (authorizedPlayerIds[i] == player.playerId)
                return true;
        }

        return false;
    }

    public void RequestRegistration()
    {
        SendCustomNetworkEvent(
            NetworkEventTarget.Owner,
            nameof(RegisterPlayer)
        );
    }

    [NetworkCallable]
    public void RegisterPlayer()
    {
        if (!Networking.IsOwner(gameObject))
            return;

        VRCPlayerApi player = NetworkCalling.CallingPlayer;

        if (!Utilities.IsValid(player))
            return;

        int playerId = player.playerId;

        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            if (authorizedPlayerIds[i] == playerId)
                return;
        }

        int[] updated = new int[authorizedPlayerIds.Length + 1];

        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            updated[i] = authorizedPlayerIds[i];
        }

        updated[updated.Length - 1] = playerId;

        authorizedPlayerIds = updated;

        RequestSerialization();
    }


    public bool IsPlayerWhitelisted(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player) || authorizedPlayerIds == null)
            return false;

        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            if (authorizedPlayerIds[i] == player.playerId)
                return true;
        }

        return false;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // Only the network owner modifies the whitelist.
        if (!Networking.IsOwner(gameObject))
            return;

        int leavingPlayerId = player.playerId;
        int removeIndex = -1;

        // Find the leaving player in the whitelist.
        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            if (authorizedPlayerIds[i] == leavingPlayerId)
            {
                removeIndex = i;
                break;
            }
        }

        // Player wasn't registered, so nothing to remove.
        if (removeIndex == -1)
            return;

        // Create an array with one fewer entry.
        int[] updated = new int[authorizedPlayerIds.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < authorizedPlayerIds.Length; i++)
        {
            if (i == removeIndex)
                continue;

            updated[newIndex] = authorizedPlayerIds[i];
            newIndex++;
        }

        // Replace the old whitelist.
        authorizedPlayerIds = updated;

        // Send the updated whitelist to everyone.
        RequestSerialization();
    }

}