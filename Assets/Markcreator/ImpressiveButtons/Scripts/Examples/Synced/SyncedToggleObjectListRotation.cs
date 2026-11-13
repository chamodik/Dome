
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggleObjectListRotation : UdonSharpBehaviour
{
    public GameObject[] targetObjectList;
    public GameObject startObject;
    [UdonSynced] int currentIndex = -1;

    public void Start()
    {
        if (Networking.IsInstanceOwner && startObject != null) currentIndex = IndexOf(targetObjectList, startObject);
        _UpdateActiveObject();
    }

    public void _OnPress()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        currentIndex++;
        if (currentIndex >= targetObjectList.Length) currentIndex = 0;
        RequestSerialization();
        _UpdateActiveObject();
    }

    public void _UpdateActiveObject()
    {
        foreach (GameObject o in targetObjectList) if (o != null) o.SetActive(false);
        if (currentIndex >= 0 && targetObjectList[currentIndex] != null) targetObjectList[currentIndex].SetActive(true);
    }

    public override void OnDeserialization()
    {
        _UpdateActiveObject();
    }

    private int IndexOf(Object[] list, Object target)
    {
        for (int i = 0; i < list.Length; i++) if (list[i] != null && list[i].Equals(target)) return i;
        return -1;
    }
}
