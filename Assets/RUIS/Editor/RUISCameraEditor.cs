using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISCamera))]
[CanEditMultipleObjects]
public class RUISCameraEditor : Editor
{
    SerializedProperty horizontalFOV;
    SerializedProperty verticalFOV;
//	SerializedProperty cullingMask;
//	SerializedProperty near;
//	SerializedProperty far;

//    RUISCamera camera;

    void OnEnable()
    {
//        near = serializedObject.FindProperty("near");
//		far = serializedObject.FindProperty("far");
//		cullingMask = serializedObject.FindProperty("cullingMask");

        horizontalFOV = serializedObject.FindProperty("horizontalFOV");
        verticalFOV = serializedObject.FindProperty("verticalFOV");
        

//        camera = target as RUISCamera;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

//		EditorGUILayout.PropertyField(cullingMask, new GUIContent("Culling Mask", "Camera culling mask for center, left, and right cameras. Overwrites center, left, and right camera culling masks."));

//        EditorGUILayout.LabelField("Clipping Planes");
//        EditorGUI.indentLevel++;
//        
//		EditorGUILayout.BeginHorizontal();
//        EditorGUILayout.PropertyField(near, new GUIContent("Near", "Near clipping plane distance for center, left, and right cameras."));
//		EditorGUILayout.PropertyField(far, new GUIContent("Far", "Far clipping plane distance for center, left, and right cameras."));
//        EditorGUILayout.EndHorizontal();
//		EditorGUI.indentLevel--;

		EditorGUILayout.LabelField("Field of View (for non-HMDs)");
        EditorGUI.indentLevel++;
        
		EditorGUILayout.BeginHorizontal();
		horizontalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Horizontal",	  "Horizontal field of view for center, left, and right cameras. "
																						+ "Only applies if the camera is not rendering to a head-mounted display."), horizontalFOV.floatValue, 0, 179);
		verticalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Vertical", 		  "Vertical field of view for center, left, and right cameras. "
																						+ "Only applies if the camera is not rendering to a head-mounted display."), verticalFOV.floatValue, 0, 179);
        EditorGUILayout.EndHorizontal();
        
		EditorGUI.indentLevel--;

//		if(camera && camera.centerCamera)
//		{
//			camera.centerCamera.nearClipPlane = near.floatValue;
//			camera.centerCamera.farClipPlane  = far.floatValue;
//		}
//		if(camera && camera.leftCamera)
//		{
//			camera.leftCamera.nearClipPlane = near.floatValue;
//			camera.leftCamera.farClipPlane  = far.floatValue;
//		}
//		if(camera && camera.rightCamera)
//		{
//			camera.rightCamera.nearClipPlane = near.floatValue;
//			camera.rightCamera.farClipPlane  = far.floatValue;
//		}
//        camera.GetComponent<Camera>().nearClipPlane = near.floatValue;
//        camera.GetComponent<Camera>().farClipPlane = far.floatValue;
//        camera.GetComponent<Camera>().fieldOfView = horizontalFOV.floatValue;

        serializedObject.ApplyModifiedProperties();
	}
}
