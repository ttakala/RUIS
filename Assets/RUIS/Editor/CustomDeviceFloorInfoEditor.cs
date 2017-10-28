using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomDeviceFloorInfo))]
[CanEditMultipleObjects]
public class CustomDeviceFloorInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
		EditorStyles.textField.wordWrap = true;
		GUILayout.TextArea("OPTIONAL: If your custom device supports floor detection, add a component that sets this GameObject's "
							+ "transform.position variable to a 3D-point that resides on the floor plane. Similarly, the component should "
							+ "set the transform.rotation so that up-vector (local y-axis) is the normal vector of the floor plane. Both "
							+ "the 3D-point and the normal vector should be represented in the 'raw' coordinate system of the custom device."
							+ "\n\nYou also need to enable the corresponding CustomDevice in RUISInputManager, where you can modify "
							+ "global settings for CustomDevice input conversion, in case the tracked pose is not represented in Unity's "
							+ "left-handed coordinate system.");
    }
}
