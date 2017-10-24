/*****************************************************************************

Content    :   Inspector behaviour for RUISCoordinateCalibrationEditor script
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
 
[CustomEditor(typeof(RUISCoordinateCalibration))]
[CanEditMultipleObjects]
public class RUISCoordinateCalibrationEditor : Editor
{
	SerializedProperty firstDevice;
	SerializedProperty secondDevice;
	SerializedProperty numberOfSamplesToTake;
	SerializedProperty samplesPerSecond;
	SerializedProperty sampleMinDistance;
	SerializedProperty xmlFilename;
	SerializedProperty device1SamplePrefab;
	SerializedProperty device2SamplePrefab;
	SerializedProperty customDevice1Tracker;
	SerializedProperty customDevice2Tracker;
	SerializedProperty customDevice1FloorPoint;
	SerializedProperty customDevice2FloorPoint;

	GUIStyle italicStyle = new GUIStyle();
	
	public void OnEnable()
	{
		firstDevice = serializedObject.FindProperty("firstDevice");
		secondDevice  = serializedObject.FindProperty("secondDevice");
		numberOfSamplesToTake = serializedObject.FindProperty("numberOfSamplesToTake");
		samplesPerSecond = serializedObject.FindProperty("samplesPerSecond");
		sampleMinDistance = serializedObject.FindProperty("sampleMinDistance");
		xmlFilename = serializedObject.FindProperty("xmlFilename");
		device1SamplePrefab = serializedObject.FindProperty("device1SamplePrefab");
		device2SamplePrefab = serializedObject.FindProperty("device2SamplePrefab");
		customDevice1Tracker = serializedObject.FindProperty("customDevice1Tracker");
		customDevice2Tracker = serializedObject.FindProperty("customDevice2Tracker");
		customDevice1FloorPoint = serializedObject.FindProperty("customDevice1FloorPoint");
		customDevice2FloorPoint = serializedObject.FindProperty("customDevice2FloorPoint");
		italicStyle.fontStyle = FontStyle.Italic;
	}
	
	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(firstDevice, new GUIContent("1st Calibration Device", ""));
		EditorGUILayout.PropertyField(secondDevice, new GUIContent("2nd Calibration Device", ""));
		EditorGUILayout.PropertyField(numberOfSamplesToTake, new GUIContent("Number Of Samples To Take", "Calibration finishes after "
																			+ "this number of samples have been collected."));
		EditorGUILayout.PropertyField(samplesPerSecond, new GUIContent("Samples Per Second", "Rate at which device position is sampled"));
		EditorGUILayout.PropertyField(sampleMinDistance, new GUIContent("Sample Min Gap", "Minimum required distance between two "
																		+ "subsequent samples. If the new sample candidate is closer to "
																		+ "the previous accepted sample than this gap value, then the "
																		+ "new sample won't be accepted. NOTE: CURRENTLY DOES NOT APPLY "
																		+ "ALL DEVICE PAIRS"));

		RUISEditorUtility.HorizontalRuler();
		EditorGUILayout.PropertyField(xmlFilename, new GUIContent("Calibration XML File Name", ""));
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(device1SamplePrefab, new GUIContent("1st Device Sample Prefab", "Prefab that is used to visualize "
																			+ "tracked device samples during and after calibration"));
		EditorGUILayout.PropertyField(device2SamplePrefab, new GUIContent("2nd Device Sample Prefab", "Prefab that is used to visualize "
																			+ "tracked device samples during and after calibration"));
		
		RUISEditorUtility.HorizontalRuler();
		EditorGUILayout.LabelField("   Custom Tracked Devices", italicStyle);
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(customDevice1Tracker, new GUIContent("CustomDevice1 Tracked Pose", "Transform whose pose is updated "
																			+ "with the position of the tracked device. Link this field "
																			+ "with a Game Object that has a script or component that updates "
																			+ "its position with the location of the tracked custom device."));
		EditorGUILayout.PropertyField(customDevice1FloorPoint, new GUIContent("CustomDevice1 Floor Pose", "OPTIONAL: Transform which "
																			+ "includes a position that is a 3D point on the floor plane, "
																			+ "and an orientation whose up direction is the floor normal. "
																			+ "The Floor Pose should be in the same coordinate system as the "
																			+ "'CustomDevice1 Tracked Pose'. Set this to NONE if floor data "
																			+ "is not available to this device: then the floor will be "
																			+ "assumed to be in origin with a floor normal of (0,1,0)"));

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(customDevice2Tracker, new GUIContent("CustomDevice2 Tracked Pose", "Transform whose pose is updated "
																			+ "with the position of the tracked device. Link this field "
																			+ "with a Game Object that has a script or component that updates "
																			+ "its position with the location of the tracked custom device."));
		EditorGUILayout.PropertyField(customDevice2FloorPoint, new GUIContent("CustomDevice2 Floor Pose", "OPTIONAL: Transform which "
																			+ "includes a position that is a 3D point on the floor plane, "
																			+ "and an orientation whose up direction is the floor normal. "
																			+ "The Floor Pose should be in the same coordinate system as the "
																			+ "'CustomDevice2 Tracked Pose'. Set this to NONE if floor data "
																			+ "is not available to this device: then the floor will be "
																			+ "assumed to be in origin with a floor normal of (0,1,0)"));
		serializedObject.ApplyModifiedProperties();
	}
}