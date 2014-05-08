using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISCamera))]
[CanEditMultipleObjects]
public class RUISCameraEditor : Editor
{
	
	SerializedProperty fovFactor;
	SerializedProperty aspFactor;

    SerializedProperty near;
    SerializedProperty far;
    SerializedProperty horizontalFOV;
    SerializedProperty verticalFOV;
    SerializedProperty cullingMask;

	RUISCamera camera;
	Camera leftEye;
	Camera rightEye;

    void OnEnable()
    {
        near = serializedObject.FindProperty("near");
        far = serializedObject.FindProperty("far");

        horizontalFOV = serializedObject.FindProperty("horizontalFOV");
        verticalFOV = serializedObject.FindProperty("verticalFOV");
        
        cullingMask = serializedObject.FindProperty("cullingMask");

        camera = target as RUISCamera;
    }

    public override void OnInspectorGUI()
    {
		serializedObject.Update();

        EditorGUILayout.PropertyField(cullingMask, new GUIContent("Culling Mask", "Camera culling mask"));

        EditorGUILayout.LabelField("Clipping Planes");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(near, new GUIContent("Near", "Near clipping plane distance (affects all Camera scripts as well as OVRCamera Controller)"));
		EditorGUILayout.PropertyField(far, new GUIContent("Far", "Far clipping plane distance (affects all Camera scripts as well as OVRCamera Controller)"));
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Fields of View");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
		horizontalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Horizontal", "Horizontal Field of View (this value is ignored if Oculus Rift is used)"), 
		                                                  				 horizontalFOV.floatValue, 0, 179);
		verticalFOV.floatValue = EditorGUILayout.Slider(new GUIContent("Vertical", "Vertical Field of View (this value is ignored if Oculus Rift is used)"), 
		                                                				verticalFOV.floatValue, 0, 179);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

		// Without this conditional (GUI.changed) the near/far values from Camera script cannot be edited individually
		if(GUI.changed || Event.current.keyCode == KeyCode.Return)
		{
			camera.camera.nearClipPlane = near.floatValue;
			camera.camera.farClipPlane = far.floatValue;
			camera.camera.fieldOfView = horizontalFOV.floatValue;

			near.floatValue = camera.camera.nearClipPlane;
			far.floatValue = camera.camera.farClipPlane;
			horizontalFOV.floatValue = camera.camera.fieldOfView;

			OVRCameraController oculusCameraController = camera.gameObject.GetComponent<OVRCameraController>();
			if(oculusCameraController)
			{
				oculusCameraController.NearClipPlane = near.floatValue;
				oculusCameraController.FarClipPlane  = far.floatValue;
			}

			if(camera.rightCamera)
			{
				camera.rightCamera.nearClipPlane = near.floatValue;
				camera.rightCamera.farClipPlane = far.floatValue;
				camera.rightCamera.fieldOfView = horizontalFOV.floatValue;
			}
			
			if(camera.leftCamera)
			{
				camera.leftCamera.nearClipPlane = near.floatValue;
				camera.leftCamera.farClipPlane = far.floatValue;
				camera.leftCamera.fieldOfView = horizontalFOV.floatValue;
			}
		}

        serializedObject.ApplyModifiedProperties();
	}
}
