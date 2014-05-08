using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISCamera))]
[CanEditMultipleObjects]
public class RUISCameraEditor : Editor
{
	
	SerializedProperty fovFactor;

    SerializedProperty near;
    SerializedProperty far;
    SerializedProperty horizontalFOV;
    SerializedProperty verticalFOV;
    SerializedProperty cullingMask;

//	RUISCamera camera;
	Camera leftEye;
	Camera rightEye;

	private bool updateNear;
	private bool updateFar;
	private bool updateFieldOfView;

    void OnEnable()
    {

        near = serializedObject.FindProperty("near");
        far = serializedObject.FindProperty("far");

        horizontalFOV = serializedObject.FindProperty("horizontalFOV");
        verticalFOV = serializedObject.FindProperty("verticalFOV");
        
        cullingMask = serializedObject.FindProperty("cullingMask");

//        camera = target as RUISCamera;
    }

    public override void OnInspectorGUI()
    {
		serializedObject.Update();

        EditorGUILayout.PropertyField(cullingMask, new GUIContent("Culling Mask", "Camera culling mask"));

        EditorGUILayout.LabelField("Clipping Planes");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginChangeCheck();
		GUI.SetNextControlName("near");
        EditorGUILayout.PropertyField(near, new GUIContent("Near", "Near clipping plane distance (affects all child Camera scripts as well as OVRCamera Controller)"));
		if(EditorGUI.EndChangeCheck() || ( Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "near") )
			updateNear = true;
		else
			updateNear = false;
		
		EditorGUI.BeginChangeCheck();
		GUI.SetNextControlName("far");
		EditorGUILayout.PropertyField(far, new GUIContent("Far", "Far clipping plane distance (affects all child Camera scripts as well as OVRCamera Controller)"));
		if(EditorGUI.EndChangeCheck() || ( Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "far") )
			updateFar = true;
		else
			updateFar = false;

        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Fields of View");
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
		
		EditorGUI.BeginChangeCheck();
		GUI.SetNextControlName("horizontal");
		horizontalFOV.floatValue = EditorGUILayout.Slider(new GUIContent(  "Horizontal", "Horizontal Field of View (this value is ignored if Oculus Rift or "
		                                                                 + "Head Tracked CAVE Display is used)"), horizontalFOV.floatValue, 0, 179);
		if(EditorGUI.EndChangeCheck() || ( Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "horizontal") )
			updateFieldOfView = true;
		else
			updateFieldOfView = false;


		verticalFOV.floatValue = EditorGUILayout.Slider(new GUIContent(  "Vertical", "Vertical Field of View (this value is ignored if Oculus Rift or "
		                                                               + "Head Tracked CAVE Display is used)"), verticalFOV.floatValue, 0, 179);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel--;

		if(targets != null)
		{
			foreach(RUISCamera cameraScript in targets)
			{
				if(updateNear)
					cameraScript.camera.nearClipPlane = near.floatValue;
				if(updateFar)
					cameraScript.camera.farClipPlane = far.floatValue;
				if(updateFieldOfView)
					cameraScript.camera.fieldOfView = horizontalFOV.floatValue;

				OVRCameraController oculusCameraController = cameraScript.gameObject.GetComponent<OVRCameraController>();
				if(oculusCameraController)
				{
					if(updateNear)
						oculusCameraController.NearClipPlane = near.floatValue;
					if(updateFar)
						oculusCameraController.FarClipPlane  = far.floatValue;
				}

				if(cameraScript.rightCamera)
				{
					if(updateNear)
						cameraScript.rightCamera.nearClipPlane = near.floatValue;
					if(updateFar)
						cameraScript.rightCamera.farClipPlane = far.floatValue;
					if(updateFieldOfView)
						cameraScript.rightCamera.fieldOfView = horizontalFOV.floatValue;
				}
				
				if(cameraScript.leftCamera)
				{
					if(updateNear)
						cameraScript.leftCamera.nearClipPlane = near.floatValue;
					if(updateFar)
						cameraScript.leftCamera.farClipPlane = far.floatValue;
					if(updateFieldOfView)
						cameraScript.leftCamera.fieldOfView = horizontalFOV.floatValue;
				}
			}
			
//			near.floatValue = camera.camera.nearClipPlane;
//			far.floatValue = camera.camera.farClipPlane;
//			horizontalFOV.floatValue = camera.camera.fieldOfView;
		}

        serializedObject.ApplyModifiedProperties();
	}
}
