/*****************************************************************************

Content    :   Custom editor script for RUISInputManager
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Mikael Matveinen, Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISInputManager))]
[CanEditMultipleObjects]
public class RUISInputManagerEditor : Editor {
    RUISInputManager inputConfig;

    SerializedProperty xmlSchema;
    SerializedProperty filename;

    SerializedProperty loadFromTextFileInEditor;

    SerializedProperty psMoveEnabled;
    SerializedProperty connectToMoveOnStartup;
    SerializedProperty psMoveIp;
    SerializedProperty psMovePort;
    SerializedProperty inGameMoveCalibration;
    SerializedProperty amountOfPSMoveControllers;

    SerializedProperty kinectEnabled;
    SerializedProperty maxNumberOfKinectPlayers;
	SerializedProperty floorDetectionOnSceneStart;

    void OnEnable()
    {
        inputConfig = target as RUISInputManager;

        xmlSchema = serializedObject.FindProperty("xmlSchema");
        filename = serializedObject.FindProperty("filename");

        psMoveEnabled = serializedObject.FindProperty("enablePSMove");
        loadFromTextFileInEditor = serializedObject.FindProperty("loadFromTextFileInEditor");
        connectToMoveOnStartup = serializedObject.FindProperty("connectToPSMoveOnStartup");
        psMoveIp = serializedObject.FindProperty("PSMoveIP");
        psMovePort = serializedObject.FindProperty("PSMovePort");
        inGameMoveCalibration = serializedObject.FindProperty("enableMoveCalibrationDuringPlay");
        amountOfPSMoveControllers = serializedObject.FindProperty("amountOfPSMoveControllers");

        kinectEnabled = serializedObject.FindProperty("enableKinect");
        maxNumberOfKinectPlayers = serializedObject.FindProperty("maxNumberOfKinectPlayers");
		floorDetectionOnSceneStart = serializedObject.FindProperty("kinectFloorDetection");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import from XML"))
            {
                if (Import())
                {
                    //success
                }
                else
                {
                    //failure
                }
            }
            if (GUILayout.Button("Export to XML"))
            {
                if (Export())
                {
                    //success
                }
                else
                {
                    //failure
                }
            }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(filename, new GUIContent("Filename"));
        EditorGUILayout.PropertyField(xmlSchema, new GUIContent("XML Schema"));

        EditorGUILayout.PropertyField(loadFromTextFileInEditor, new GUIContent("Load from File in Editor", "Load PSMove IP and Port from " + filename.stringValue + " while in editor. Otherwise use the values specified here. Outside the editor the applicable values are loaded from the external file."));


        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(psMoveEnabled, new GUIContent("PS Move Enabled"));

        if (psMoveEnabled.boolValue)
        {
            EditorGUI.indentLevel += 2;

            EditorGUILayout.PropertyField(psMoveIp, new GUIContent("PS Move IP", "PS Move IP address"));
            EditorGUILayout.PropertyField(psMovePort, new GUIContent("PS Move Port"));

            EditorGUILayout.PropertyField(connectToMoveOnStartup, new GUIContent("Auto-connect to Move.Me", "Connect to the Move.me server on startup."));

            EditorGUILayout.PropertyField(inGameMoveCalibration, new GUIContent("In-game Move calibration", "Enables the default Move Calibration by pressing the home button. Caution: Recalibration may change the coordinate system! Recommended setting is to keep this unchecked."));

            EditorGUILayout.PropertyField(amountOfPSMoveControllers, new GUIContent("Max amount of controllers connected", "Maximum amount of controllers connected. All RUISPSMoveControllers with a controller id outside of the range will get disabled to prevent accidents."));
            amountOfPSMoveControllers.intValue = Mathf.Clamp(amountOfPSMoveControllers.intValue, 0, 4);

            EditorGUI.indentLevel -= 2;
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(kinectEnabled, new GUIContent("Kinect Enabled"));
        if (kinectEnabled.boolValue)
        {
            EditorGUI.indentLevel += 2;

            EditorGUILayout.PropertyField(maxNumberOfKinectPlayers, new GUIContent("Max Kinect Players", "Number of concurrently tracked skeletons"));
			EditorGUILayout.PropertyField(floorDetectionOnSceneStart, new GUIContent("Floor Detection On Scene Start", "Kinect tries to detect floor and adjusts the coordinate system automatically when the scene is run."));
			
            EditorGUI.indentLevel -= 2;
        }


        serializedObject.ApplyModifiedProperties();
    }

    private bool Import()
    {
        string filename = EditorUtility.OpenFilePanel("Import Input Configuration", null, "xml");
        if (filename.Length != 0)
        {
            return inputConfig.Import(filename);
        }
        else
        {
            return false;
        }
    }

    private bool Export()
    {
        string filename = EditorUtility.SaveFilePanel("Export Input Configuration", null, "inputConfig", "xml");
        if (filename.Length != 0)
            return inputConfig.Export(filename);
        else
            return false;
    }
}
