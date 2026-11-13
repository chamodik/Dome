
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

public class Trash_Can : UdonSharpBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<VRCPickup>() != null)
        {
            other.gameObject.SetActive(false);
        }
    }
}
