/*****************************************************************************

Content    :   Inspector behaviour for RUISKinectAndMecanimCombiner script
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISKinectAndMecanimCombiner))]
[CanEditMultipleObjects]
public class RUISKinectAndMecanimCombinerEditor : Editor
{
    SerializedProperty rootBlendWeight;
    SerializedProperty torsoBlendWeight;
    SerializedProperty headBlendWeight;
    SerializedProperty rightArmBlendWeight;
    SerializedProperty leftArmBlendWeight;
    SerializedProperty rightLegBlendWeight;
    SerializedProperty leftLegBlendWeight;

    SerializedProperty forceArmStartPosition;
    SerializedProperty forceLegStartPosition;

    public void OnEnable()
    {
        rootBlendWeight = serializedObject.FindProperty("rootBlendWeight");
        torsoBlendWeight = serializedObject.FindProperty("torsoBlendWeight");
        headBlendWeight = serializedObject.FindProperty("headBlendWeight");
        rightArmBlendWeight = serializedObject.FindProperty("rightArmBlendWeight");
        leftArmBlendWeight = serializedObject.FindProperty("leftArmBlendWeight");
        rightLegBlendWeight = serializedObject.FindProperty("rightLegBlendWeight");
        leftLegBlendWeight = serializedObject.FindProperty("leftLegBlendWeight");
        forceArmStartPosition = serializedObject.FindProperty("forceArmStartPosition");
        forceLegStartPosition = serializedObject.FindProperty("forceLegStartPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		var rightAlignmentStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleRight};

        EditorGUILayout.LabelField("Blend Weights");
        EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("", GUILayout.MinWidth(50), GUILayout.MaxWidth(50));
		EditorGUILayout.LabelField("Tracking", GUILayout.MinWidth(60));
		EditorGUILayout.LabelField("Animation      ", rightAlignmentStyle, GUILayout.MinWidth(60));
        EditorGUILayout.EndHorizontal();
		EditorGUI.indentLevel++;
		EditorGUIUtility.labelWidth = 80;
		EditorGUILayout.Slider(rootBlendWeight, 	0, 1, 	new GUIContent("Root", "Blend weight for skeleton root"), 	GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(torsoBlendWeight, 	0, 1, 	new GUIContent("Pelvis", "Blend weight for pelvis"), 		GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(headBlendWeight, 	0, 1, 	new GUIContent("Head", "Blend weight for head"), 			GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(rightArmBlendWeight, 0, 1, 	new GUIContent("Right Arm", "Blend weight for right arm"), 	GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(leftArmBlendWeight, 	0, 1, 	new GUIContent("Left Arm", "Blend weight for left arm"), 	GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(rightLegBlendWeight, 0, 1, 	new GUIContent("Right Leg", "Blend weight for right leg"), 	GUILayout.ExpandWidth(true));
		EditorGUILayout.Slider(leftLegBlendWeight, 	0, 1, 	new GUIContent("Left Leg", "Blend weight for left leg"), 	GUILayout.ExpandWidth(true));
		EditorGUIUtility.labelWidth = 0; // Set back to default
        EditorGUI.indentLevel--;

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(forceArmStartPosition, new GUIContent(  "Force Arm Position", "If enabled, the blended pose's shoulder positions "
		                                                                    + "follow exactly the mocap tracked shoulder positions. This way the "
																			+ "avatar's shoulders are as wide as the mocap system sees them, even when "
																			+ "playing arm animation."));
		EditorGUILayout.PropertyField(forceLegStartPosition, new GUIContent(  "Force Leg Position", "If enabled, the blended pose's hip positions "
		                                                                    + "follow exactly the mocap tracked hip positions. This way the avatar's "
																			+ "hips are as wide as the mocap system sees them, even when playing leg "
																			+ "animation."));

        serializedObject.ApplyModifiedProperties();
    }
}
