using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomDevicePoseInfo))]
[CanEditMultipleObjects]
public class CustomDevicePoseInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
		EditorStyles.textField.wordWrap = true;
		GUILayout.TextArea("Add a component that on every frame updates this GameObject's transform.position variable with the "
							+ "'raw' position of the tracked custom device.\n\nYou also need to enable the corresponding "
							+ "CustomDevice in RUISInputManager, where you can modify global settings for CustomDevice input "
							+ "conversion, in case the tracked pose is not represented in Unity's left-handed coordinate "
							+ "system.");
    }
}
