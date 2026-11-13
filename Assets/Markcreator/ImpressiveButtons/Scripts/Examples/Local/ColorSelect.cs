
using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ColorSelect : UdonSharpBehaviour
{

    public void _OnPress_Blue() {
        GetComponent<Renderer>().material.color = new Color(144f / 255, 175f / 255, 1);
    }

    public void _OnPress_Red()
    {
        GetComponent<Renderer>().material.color = Color.red;
    }

    public void _OnPress_Black()
    {
        GetComponent<Renderer>().material.color = Color.black;
    }

    public void _OnPress_Brown()
    {
        GetComponent<Renderer>().material.color = new Color(144f / 255, 85f / 255, 54f / 255);
    }
}
