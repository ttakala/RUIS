/*****************************************************************************

Content    :   Inspector behaviour for RUISM2KCalibrationEditor script
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISM2KCalibration))]
[CanEditMultipleObjects]
public class RUISM2KCalibrationEditor : Editor
{
	SerializedProperty First_device;
	SerializedProperty Second_device;
	SerializedProperty  minNumberOfSamplesToTake;
	SerializedProperty  samplesPerSecond ;
	SerializedProperty  rightHandVisualizer;
	SerializedProperty  calibrationSphere;
	SerializedProperty  calibrationCube;
	SerializedProperty  coordinateSystem;
	SerializedProperty  moveController;
	SerializedProperty  xmlFilename;
	SerializedProperty  calibrationGameObjects;
	SerializedProperty  calibrationReviewGameObjects;
	SerializedProperty  floorPlane;
	SerializedProperty  kinectModelObject;
	SerializedProperty  psEyeModelObject;
	SerializedProperty  kinectIcon;
	SerializedProperty  moveIcon;
	SerializedProperty usePSMove;
	SerializedProperty  resolutionX;
	SerializedProperty  resolutionY;
	SerializedProperty  psMoveIP;
	SerializedProperty  psMovePort;
	SerializedProperty  userViewer;
	
	public void OnEnable()
	{
		First_device = serializedObject.FindProperty("First_device");
		Second_device  = serializedObject.FindProperty("Second_device");
		minNumberOfSamplesToTake = serializedObject.FindProperty("minNumberOfSamplesToTake");
		samplesPerSecond = serializedObject.FindProperty("samplesPerSecond");
		rightHandVisualizer = serializedObject.FindProperty("rightHandVisualizer");
		calibrationSphere = serializedObject.FindProperty("calibrationSphere");
		calibrationCube = serializedObject.FindProperty("calibrationCube");
		coordinateSystem = serializedObject.FindProperty("coordinateSystem");
		moveController = serializedObject.FindProperty("moveController");
		xmlFilename = serializedObject.FindProperty("xmlFilename");
		calibrationGameObjects = serializedObject.FindProperty("calibrationGameObjects");
		calibrationReviewGameObjects = serializedObject.FindProperty("calibrationReviewGameObjects");
		floorPlane = serializedObject.FindProperty("floorPlane");
		kinectModelObject = serializedObject.FindProperty("kinectModelObject");
		psEyeModelObject = serializedObject.FindProperty("psEyeModelObject");
		kinectIcon = serializedObject.FindProperty("kinectIcon");
		moveIcon = serializedObject.FindProperty("moveIcon");
		usePSMove = serializedObject.FindProperty("usePSMove");
		resolutionX = serializedObject.FindProperty("resolutionX");
		resolutionY = serializedObject.FindProperty("resolutionY");
		psMoveIP = serializedObject.FindProperty("psMoveIP");
		psMovePort = serializedObject.FindProperty("psMovePort");
		userViewer = serializedObject.FindProperty("userViewer");
	}
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.PropertyField(First_device, new GUIContent("2First calibration device", ""));
		EditorGUILayout.PropertyField(Second_device, new GUIContent("Second_device", ""));
		EditorGUILayout.PropertyField(minNumberOfSamplesToTake, new GUIContent("minNumberOfSamplesToTake", ""));
		EditorGUILayout.PropertyField(samplesPerSecond, new GUIContent("samplesPerSecond", ""));
		EditorGUILayout.PropertyField(rightHandVisualizer, new GUIContent("rightHandVisualizer", ""));
		EditorGUILayout.PropertyField(calibrationSphere, new GUIContent("calibrationSphere", ""));
		EditorGUILayout.PropertyField(calibrationCube, new GUIContent("calibrationCube", ""));
		EditorGUILayout.PropertyField(coordinateSystem, new GUIContent("coordinateSystem", ""));
		EditorGUILayout.PropertyField(moveController, new GUIContent("moveController", ""));
		EditorGUILayout.PropertyField(xmlFilename, new GUIContent("xmlFilename", ""));
		EditorGUILayout.PropertyField(calibrationGameObjects, new GUIContent("calibrationGameObjects", ""));
		EditorGUILayout.PropertyField(calibrationReviewGameObjects, new GUIContent("calibrationReviewGameObjects", ""));
		EditorGUILayout.PropertyField(floorPlane, new GUIContent("floorPlane", ""));
		EditorGUILayout.PropertyField(kinectModelObject, new GUIContent("kinectModelObject", ""));
		EditorGUILayout.PropertyField(psEyeModelObject, new GUIContent("psEyeModelObject", ""));
		EditorGUILayout.PropertyField(kinectIcon, new GUIContent("kinectIcon", ""));
		EditorGUILayout.PropertyField(moveIcon, new GUIContent("moveIcon", ""));
		EditorGUILayout.PropertyField(usePSMove, new GUIContent("usePSMove", ""));
		EditorGUILayout.PropertyField(resolutionX, new GUIContent("resolutionX", ""));
		EditorGUILayout.PropertyField(resolutionY, new GUIContent("resolutionY", ""));
		EditorGUILayout.PropertyField(psMoveIP, new GUIContent("psMoveIP", ""));
		EditorGUILayout.PropertyField(psMovePort, new GUIContent("psMovePort", ""));
		EditorGUILayout.PropertyField(userViewer, new GUIContent("userViewer", ""));
		serializedObject.ApplyModifiedProperties();
	}
	
		
}	
	
