
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PXR_AdminNames : UdonSharpBehaviour
{
    [Header("World Administrators")]
    public string[] adminNames = new string[0];

    public bool IsAdmin(VRCPlayerApi player)
    {
        if (!Utilities.IsValid(player))
            return false;

        return IsAdminName(player.displayName);
    }

    public bool IsAdminName(string playerName)
    {
        if (string.IsNullOrEmpty(playerName) ||
            adminNames == null)
            return false;

        for (int i = 0; i < adminNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(adminNames[i]) &&
                adminNames[i] == playerName)
                return true;
        }

        return false;
    }

    public bool IsLocalPlayerAdmin()
    {
        return IsAdmin(Networking.LocalPlayer);
    }
}
