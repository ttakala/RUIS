using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISInputManager))]
[CanEditMultipleObjects]
public class RUISInputManagerEditor : Editor {
    RUISInputManager inputConfig;

    SerializedProperty filename;

    SerializedProperty loadFromTextFileInEditor;

    SerializedProperty psMoveEnabled;
    SerializedProperty connectToMoveOnStartup;
    SerializedProperty psMoveIp;
    SerializedProperty psMovePort;
    SerializedProperty psMoveCalibrationEnabled;
    SerializedProperty amountOfPSMoveControllers;

    SerializedProperty kinectEnabled;

    void OnEnable()
    {
        inputConfig = target as RUISInputManager;

        filename = serializedObject.FindProperty("filename");

        psMoveEnabled = serializedObject.FindProperty("enablePSMove");
        loadFromTextFileInEditor = serializedObject.FindProperty("loadFromTextFileInEditor");
        connectToMoveOnStartup = serializedObject.FindProperty("connectToPSMoveOnStartup");
        psMoveIp = serializedObject.FindProperty("PSMoveIP");
        psMovePort = serializedObject.FindProperty("PSMovePort");
        psMoveCalibrationEnabled = serializedObject.FindProperty("enableMoveCalibrationDuringPlay");
        amountOfPSMoveControllers = serializedObject.FindProperty("amountOfPSMoveControllers");

        kinectEnabled = serializedObject.FindProperty("enableKinect");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(filename, new GUIContent("Filename"));

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
        

        EditorGUILayout.PropertyField(loadFromTextFileInEditor, new GUIContent("Load from File in Editor", "Load PSMove IP and Port from " + filename.stringValue + " while in editor. Otherwise use the values specified here. Outside the editor the applicable values are loaded from the external file."));


        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(psMoveEnabled, new GUIContent("PS Move Enabled"));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        if (psMoveEnabled.boolValue)
        {
            EditorGUILayout.PropertyField(psMoveIp, new GUIContent("PS Move IP"));
            EditorGUILayout.PropertyField(psMovePort, new GUIContent("PS Move Port"));

            EditorGUILayout.PropertyField(connectToMoveOnStartup, new GUIContent("Auto-connect to Move.Me", "Connect to the Move.me server on startup."));

            EditorGUILayout.PropertyField(psMoveCalibrationEnabled, new GUIContent("Enable PS Move Calibration", "Enables the default Move Calibration by pressing the home button. Caution: Recalibration may change the coordinate system!"));

            EditorGUILayout.PropertyField(amountOfPSMoveControllers, new GUIContent("Max amount of controllers connected", "Maximum amount of controllers connected. All RUISPSMoveControllers with a controller id outside of the range will get disabled to prevent accidents."));
            amountOfPSMoveControllers.intValue = Mathf.Clamp(amountOfPSMoveControllers.intValue, 0, 4);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(kinectEnabled, new GUIContent("Kinect Enabled"));


        serializedObject.ApplyModifiedProperties();
    }

    private bool Import()
    {
        string filename = EditorUtility.OpenFilePanel("Import Input Configuration", null, "txt");
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
        string filename = EditorUtility.SaveFilePanel("Export Input Configuration", null, "inputConfig", "txt");
        if (filename.Length != 0)
            return inputConfig.Export(filename);
        else
            return false;
    }
}
