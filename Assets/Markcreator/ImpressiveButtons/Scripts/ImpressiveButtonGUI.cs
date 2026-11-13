#if UNITY_EDITOR && !COMPILER_UDONSHARP
using Markcreator.ImpressiveButtons;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

public class ImpressiveButtonGUI : MonoBehaviour
{
}

[CustomEditor(typeof(ImpressiveButtonGUI))]
[CanEditMultipleObjects]
public class ImpressiveButtonGUIEditor : Editor
{
    ImpressiveButtonGUI o;
    void OnEnable()
    {
        o = target as ImpressiveButtonGUI;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle(GUI.skin.button) { richText = true };
        style.padding = new RectOffset(10, 10, 10, 10);
        if (GUILayout.Button("Show Button Settings", style))
        {
            UdonBehaviour settings = null;
            foreach(UdonBehaviour b in o.gameObject.GetComponentsInChildren<UdonBehaviour>())
            {
                if (b.programSource.name.Equals(typeof(ImpressiveButton).Name)) settings = b;
            }
            if (settings) Selection.activeObject = settings;
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
}
#endif
