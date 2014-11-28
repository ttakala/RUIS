using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RUISSkeletonWand))]
[CanEditMultipleObjects]
public class RUISSkeletonWandEditor : Editor
{
	SerializedProperty playerId;
	SerializedProperty bodyTrackingDevice;
	SerializedProperty gestureSelectionMethod;
	SerializedProperty wandStart;
	SerializedProperty wandEnd;
	SerializedProperty visualizerThreshold;
	SerializedProperty visualizerWidth;
	SerializedProperty visualizerHeight;
	SerializedProperty wandColor;
	SerializedProperty gestureRecognizer;
	SerializedProperty wandPositionVisualizer;
	SerializedObject gestureSelectionMethodLink; 
	SerializedProperty guiGestureSelectionMethodChoiceLink;
	SerializedProperty gestureScriptLink;
	SerializedProperty showVisualizer;
	
	RUISSkeletonWand skeletonWand;
	RUISGestureRecognizer[] gestureRecognizerScripts;
	
	void OnEnable()
	{
		playerId = serializedObject.FindProperty("playerId");
		bodyTrackingDevice = serializedObject.FindProperty("bodyTrackingDevice");
		gestureSelectionMethod = serializedObject.FindProperty("gestureSelectionMethod");
		wandStart = serializedObject.FindProperty("wandStart");
		wandEnd = serializedObject.FindProperty("wandEnd");
		visualizerThreshold = serializedObject.FindProperty("visualizerThreshold");
		visualizerWidth = serializedObject.FindProperty("visualizerWidth");
		visualizerHeight = serializedObject.FindProperty("visualizerHeight");
		wandColor = serializedObject.FindProperty("wandColor");
		gestureRecognizer = serializedObject.FindProperty("gestureRecognizer");
		wandPositionVisualizer = serializedObject.FindProperty("wandPositionVisualizer");
		showVisualizer = serializedObject.FindProperty("showVisualizer");
		
		skeletonWand = target as RUISSkeletonWand;
		
		if(skeletonWand) {
			gestureSelectionMethodLink = new SerializedObject(skeletonWand);
			guiGestureSelectionMethodChoiceLink = gestureSelectionMethodLink.FindProperty("gestureSelectionMethod");
			gestureScriptLink = gestureSelectionMethodLink.FindProperty("gestureSelectionScriptName");
		}
		
	}
	
	public override void OnInspectorGUI()
	{
		
		RUISSkeletonWand script = (RUISSkeletonWand) target;
		string[] _choices = { };
		
		if(skeletonWand) {
			gestureRecognizerScripts = skeletonWand.gameObject.GetComponents<RUISGestureRecognizer>();
			List<string> _drowndownElements = new List<string>();
			
			for(int i = 0; i < gestureRecognizerScripts.Length; i++) {
				_drowndownElements.Add(gestureRecognizerScripts[i].ToString().Replace("SkeletonWand (RUIS", "").Replace(")", ""));
			}
			_choices = _drowndownElements.ToArray(); 
		}
		
		serializedObject.Update();
		if(skeletonWand) gestureSelectionMethodLink.Update();
		if(skeletonWand) guiGestureSelectionMethodChoiceLink.intValue = EditorGUILayout.Popup("Gesture Recognizer", guiGestureSelectionMethodChoiceLink.intValue, _choices);
		if(skeletonWand) gestureScriptLink.stringValue = gestureRecognizerScripts[guiGestureSelectionMethodChoiceLink.intValue].ToString();
		
		EditorGUILayout.PropertyField(playerId, new GUIContent("Player ID", ""));
		EditorGUILayout.PropertyField(bodyTrackingDevice, new GUIContent("Body Tracking Device", ""));
		EditorGUILayout.PropertyField(wandStart, new GUIContent("Wand Start Point", ""));
		EditorGUILayout.PropertyField(wandEnd, new GUIContent("Wand End Point", ""));
		
		EditorGUILayout.PropertyField(showVisualizer, new GUIContent("Show Visualizer", ""));
		if(showVisualizer.boolValue) {
			EditorGUI.indentLevel += 2;
			EditorGUILayout.PropertyField(visualizerThreshold, new GUIContent("Visualizer Threshold", ""));
			EditorGUILayout.PropertyField(visualizerWidth, new GUIContent("Visualizer Width", ""));
			EditorGUILayout.PropertyField(visualizerHeight, new GUIContent("Visualizer Height", ""));
			EditorGUI.indentLevel -= 2;	
		}
		EditorGUILayout.PropertyField(wandColor, new GUIContent("Wand Color", ""));
		EditorGUILayout.PropertyField(wandPositionVisualizer, new GUIContent("Wand Position Visualizer", ""));
		serializedObject.ApplyModifiedProperties();
		if(skeletonWand) gestureSelectionMethodLink.ApplyModifiedProperties();
	}
}
