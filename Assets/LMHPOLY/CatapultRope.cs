using UdonSharp;
using UnityEngine;

public class CatapultRope : UdonSharpBehaviour
{
    public Transform bodyAnchor;
    public Transform armAnchor;
    public LineRenderer rope;

    void Start()
    {
        rope.positionCount = 2;
        rope.useWorldSpace = true;
    }

    void LateUpdate()
    {
        rope.SetPosition(0, bodyAnchor.position);
        rope.SetPosition(1, armAnchor.position);
    }
}
