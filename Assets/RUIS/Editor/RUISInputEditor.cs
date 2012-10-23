using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISInputManager))]
[CanEditMultipleObjects]
public class RUISInputEditor : Editor {
    RUISInputManager inputConfig;

    SerializedProperty filename;

    SerializedProperty loadFromTextFileInEditor;

    SerializedProperty psMoveEnabled;
    
    SerializedProperty connectToMoveOnStartup;
    SerializedProperty psMoveIp;
    SerializedProperty psMovePort;

    void OnEnable()
    {
        inputConfig = target as RUISInputManager;

        filename = serializedObject.FindProperty("filename");

        psMoveEnabled = serializedObject.FindProperty("enablePSMove");
        loadFromTextFileInEditor = serializedObject.FindProperty("loadFromTextFileInEditor");
        connectToMoveOnStartup = serializedObject.FindProperty("connectToPSMoveOnStartup");
        psMoveIp = serializedObject.FindProperty("PSMoveIP");
        psMovePort = serializedObject.FindProperty("PSMovePort");
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

        EditorGUILayout.PropertyField(loadFromTextFileInEditor, new GUIContent("Load from File in Editor"));

        EditorGUILayout.PropertyField(psMoveEnabled, new GUIContent("PS Move Enabled"));
        if (psMoveEnabled.boolValue)
        {
            EditorGUILayout.PropertyField(connectToMoveOnStartup, new GUIContent("Auto-connect to Move.Me"));

            EditorGUILayout.PropertyField(psMoveIp, new GUIContent("PS Move IP"));
            EditorGUILayout.PropertyField(psMovePort, new GUIContent("PS Move Port"));
        }

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
