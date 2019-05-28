using UnityEngine;
using UnityEditor;

using static GenericsMethods.TextStyles;

public abstract class InspectorEditor : Editor
{

    protected virtual void Title(string value)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        var style = h3;
        style.padding = new RectOffset(0, 0, 1, 1);
        style.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    protected virtual void Title(string value, GUIStyle style)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        style.padding = new RectOffset(0, 0, 1, 1);
        style.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }


    protected virtual void Subtitle(string value)
    {
        EditorGUILayout.Space();

        var style = h4;
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));

        EditorGUILayout.Space();
    }

    protected virtual void Subtitle(string value, GUIStyle style)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(value, style, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
    }
}
