
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ToggleObjectListRotation : UdonSharpBehaviour
{
    public GameObject[] targetObjectList;
    public GameObject startObject;
    private int currentIndex = -1;

    public void Start()
    {
        foreach (GameObject o in targetObjectList) if (o != null) o.SetActive(false);
        if (startObject != null)
        {
            startObject.SetActive(true);
            currentIndex = IndexOf(targetObjectList, startObject);
        }
    }

    public void _OnPress()
    {
        if (currentIndex >= 0 && targetObjectList[currentIndex] != null) targetObjectList[currentIndex].SetActive(false);
        currentIndex++;
        if (currentIndex == targetObjectList.Length) currentIndex = 0;
        if (currentIndex >= 0 && targetObjectList[currentIndex] != null) targetObjectList[currentIndex].SetActive(true);
    }

    private int IndexOf(Object[] list, Object target)
    {
        for (int i = 0; i < list.Length; i++) if (list[i] != null && list[i].Equals(target)) return i;
        return -1;
    }
}
