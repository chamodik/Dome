
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UdonSharpEditor;

[CustomEditor(typeof(PXR_UnifiedCreature))]
public class PXR_UnifiedCreatureEditor : Editor
{
    private readonly string[] typeNames =
    {
        "Stand",
        "Move"
    };

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            return;

        serializedObject.Update();

        EditorGUILayout.HelpBox(
            "Required: NavMeshAgent on the creature and a baked NavMesh. " +
            "For networked movement, use VRCObjectSync on the root.\n" +
            "Optional: Animator with a Bool parameter matching Walking Bool Name. " +
            "The owner runs pathfinding; remote players receive synchronized movement.",
            MessageType.Info);

        EditorGUILayout.LabelField(
            "Components",
            EditorStyles.boldLabel
        );

        DrawProperty("navAgent");
        DrawProperty("animator");
        DrawProperty("walkingBoolName");

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Sequence Settings",
            EditorStyles.boldLabel
        );

        DrawProperty("playOnStart");
        DrawProperty("loop");

        EditorGUILayout.Space(8);

        DrawSequence();

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Navigation",
            EditorStyles.boldLabel
        );

        DrawProperty("arrivalDistance");
        DrawProperty("maxMoveSeconds");

        EditorGUILayout.Space(8);

        DrawProperty("debugLogs");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawProperty(string name)
    {
        SerializedProperty property =
            serializedObject.FindProperty(name);

        if (property != null)
            PXR_InspectorGuidance.PropertyField(property);
    }

    private void DrawSequence()
    {
        SerializedProperty types =
            serializedObject.FindProperty("stepTypes");

        SerializedProperty durations =
            serializedObject.FindProperty("standDurations");

        SerializedProperty targets =
            serializedObject.FindProperty("moveTargets");

        SerializedProperty speeds =
            serializedObject.FindProperty("moveSpeeds");

        if (types == null ||
            durations == null ||
            targets == null ||
            speeds == null)
        {
            EditorGUILayout.HelpBox(
                "Creature sequence fields could not be found.",
                MessageType.Error
            );
            return;
        }

        // Keep the four arrays aligned.
        int count = types.arraySize;

        durations.arraySize = count;
        targets.arraySize = count;
        speeds.arraySize = count;

        EditorGUILayout.LabelField(
            "Behavior Sequence",
            EditorStyles.boldLabel
        );

        if (count == 0)
        {
            EditorGUILayout.HelpBox(
                "No behaviors configured. Add a Stand or Move step.",
                MessageType.Info
            );
        }

        for (int i = 0; i < count; i++)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox
            );

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                "Step " + (i + 1),
                EditorStyles.boldLabel
            );

            bool moveUp = GUILayout.Button(
                "Up",
                GUILayout.Width(40)
            );

            bool moveDown = GUILayout.Button(
                "Down",
                GUILayout.Width(50)
            );

            bool remove = GUILayout.Button(
                "X",
                GUILayout.Width(25)
            );

            EditorGUILayout.EndHorizontal();

            if (moveUp && i > 0)
            {
                MoveAll(
                    types, durations,
                    targets, speeds,
                    i, i - 1
                );

                EditorGUILayout.EndVertical();
                break;
            }

            if (moveDown && i < count - 1)
            {
                MoveAll(
                    types, durations,
                    targets, speeds,
                    i, i + 1
                );

                EditorGUILayout.EndVertical();
                break;
            }

            if (remove)
            {
                RemoveStep(
                    types, durations,
                    targets, speeds,
                    i
                );

                EditorGUILayout.EndVertical();
                break;
            }

            SerializedProperty type =
                types.GetArrayElementAtIndex(i);

            type.intValue = EditorGUILayout.Popup(
                new GUIContent("Behavior", "Stand waits for the specified duration; Move navigates toward the assigned destination."),
                Mathf.Clamp(type.intValue, 0, 1),
                typeNames
            );

            if (type.intValue == 0)
            {
                PXR_InspectorGuidance.PropertyField(
                    durations.GetArrayElementAtIndex(i),
                    new GUIContent("Idle Duration (s)")
                );
            } else
            {
                PXR_InspectorGuidance.PropertyField(
                    targets.GetArrayElementAtIndex(i),
                    new GUIContent("Destination")
                );

                PXR_InspectorGuidance.PropertyField(
                    speeds.GetArrayElementAtIndex(i),
                    new GUIContent("Move Speed")
                );
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add Stand"))
        {
            AddStep(
                types, durations,
                targets, speeds, 0
            );
        }

        if (GUILayout.Button("Add Move"))
        {
            AddStep(
                types, durations,
                targets, speeds, 1
            );
        }

        EditorGUILayout.EndHorizontal();
    }

    private void AddStep(
        SerializedProperty types,
        SerializedProperty durations,
        SerializedProperty targets,
        SerializedProperty speeds,
        int newType)
    {
        int index = types.arraySize;
        int newSize = index + 1;

        types.arraySize = newSize;
        durations.arraySize = newSize;
        targets.arraySize = newSize;
        speeds.arraySize = newSize;

        types.GetArrayElementAtIndex(index).intValue =
            newType;

        durations.GetArrayElementAtIndex(index).floatValue =
            3f;

        targets.GetArrayElementAtIndex(index).objectReferenceValue =
            null;

        speeds.GetArrayElementAtIndex(index).floatValue =
            2f;
    }

    private void MoveAll(
        SerializedProperty types,
        SerializedProperty durations,
        SerializedProperty targets,
        SerializedProperty speeds,
        int from,
        int to)
    {
        types.MoveArrayElement(from, to);
        durations.MoveArrayElement(from, to);
        targets.MoveArrayElement(from, to);
        speeds.MoveArrayElement(from, to);
    }

    private void RemoveStep(
        SerializedProperty types,
        SerializedProperty durations,
        SerializedProperty targets,
        SerializedProperty speeds,
        int index)
    {
        int last = types.arraySize - 1;

        // Move the removed step to the end,
        // maintaining alignment of all arrays.
        MoveAll(
            types, durations,
            targets, speeds,
            index, last
        );

        types.arraySize = last;
        durations.arraySize = last;
        targets.arraySize = last;
        speeds.arraySize = last;
    }
}

#endif
