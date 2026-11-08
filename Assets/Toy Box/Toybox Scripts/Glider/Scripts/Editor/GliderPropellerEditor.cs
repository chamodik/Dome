using UnityEditor;
using UnityEngine;

namespace TORISOUP.Gliders.Editor
{
    [CustomEditor(typeof(GliderPropeller))]
    public class GliderPropellerEditor : UnityEditor.Editor
    {
        GliderPropeller glider;
        GUIStyle _style = new GUIStyle();

        void OnEnable()
        {
            glider = target as GliderPropeller;
            _style.normal.textColor = Color.black;
            _style.fontSize = 20;
        }

        void OnSceneGUI()
        {
            if(!glider._drawGizmo) return;
            
            EditorGUI.BeginChangeCheck();

            var orig = glider.transform.position;
            var d = new Vector3(orig.x, glider._limitHeight, orig.z);
            var p = Handles.PositionHandle(d, Quaternion.identity);
            Handles.Label(d, $"Glider limit height: {glider._limitHeight}m", _style);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(glider, "Changed glider limit height");
                glider._limitHeight = p.y;
            }
        }
    }
}