using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISCamera))]
[CanEditMultipleObjects]
public class RUISCameraEditor : Editor
{
    SerializedProperty near;
    SerializedProperty far;
    SerializedProperty horizontalFOV;
    SerializedProperty verticalFOV;
    SerializedProperty headTracker;
    SerializedProperty cullingMask;

    RUISCamera camera;

    void OnEnable()
    {
        near = serializedObject.FindProperty("near");
        far = serializedObject.FindProperty("far");

        horizontalFOV = serializedObject.FindProperty("horizontalFOV");
        verticalFOV = serializedObject.FindProperty("verticalFOV");
        
        headTracker = serializedObject.FindProperty("headTracker");
        
        cullingMask = serializedObject.FindProperty("cullingMask");

        camera = target as RUISCamera;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(headTracker, new GUIContent(  "CAVE Head Tracker", "The head tracker object to use for perspective "
			                                                          + "distortion with CAVE-like displays. This is used only if the associated "
			                                                          + "RUISDisplay has 'Head Tracked CAVE Display' enabled."));

        EditorGUILayout.PropertyField(cullingMask, new GUIContent("Culling Mask", "Camera culling mask"));

        EditorGUILayout.LabelField("Clipping Planes");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(near, new GUIContent("Near", "Near clipping plane distance"));
            EditorGUILayout.PropertyField(far, new GUIContent("Far", "Far clipping plane distance"));
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Fields of View");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
            horizontalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Horizontal", "Horizontal Field of View"), horizontalFOV.floatValue, 0, 179);
            verticalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Vertical", "Vertical Field of View"), verticalFOV.floatValue, 0, 179);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

        camera.camera.nearClipPlane = near.floatValue;
        camera.camera.farClipPlane = far.floatValue;
        camera.camera.fieldOfView = horizontalFOV.floatValue;

        serializedObject.ApplyModifiedProperties();
	}
}
