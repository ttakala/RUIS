using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomDevicePoseInfo))]
[CanEditMultipleObjects]
public class CustomDevicePoseInfoEditor : Editor
{
    public override void OnInspectorGUI()
    {
		EditorStyles.textField.wordWrap = true;
		GUILayout.TextArea("Add a component that sets this GameObject's transform.position variable to the position of "
							+ "the tracked custom device.");
    }
}
