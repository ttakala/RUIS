using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISDisplay))]
[CanEditMultipleObjects]
public class RUISDisplayEditor : Editor {
    SerializedProperty xmlFilename;
    SerializedProperty displaySchema;

    SerializedProperty resolutionX;
    SerializedProperty resolutionY;
    SerializedProperty isStereo;
    SerializedProperty isHMD;
    SerializedProperty isKeystoneCorrected;
    SerializedProperty camera;
    SerializedProperty stereoType;
    SerializedProperty useDoubleTheSpace;
    GUIStyle displayBoxStyle;

    private Texture2D monoDisplayTexture;
    private Texture2D stereoDisplayTexture;

    void OnEnable()
    {
        xmlFilename = serializedObject.FindProperty("xmlFilename");
        displaySchema = serializedObject.FindProperty("displaySchema");

        resolutionX = serializedObject.FindProperty("resolutionX");
        resolutionY = serializedObject.FindProperty("resolutionY");
        isStereo = serializedObject.FindProperty("isStereo");
        isHMD = serializedObject.FindProperty("isHMD");
        isKeystoneCorrected = serializedObject.FindProperty("isKeystoneCorrected");
        camera = serializedObject.FindProperty("linkedCamera");
        stereoType = serializedObject.FindProperty("stereoType");
        useDoubleTheSpace = serializedObject.FindProperty("useDoubleTheSpace");

        displayBoxStyle = new GUIStyle();
        displayBoxStyle.normal.textColor = Color.white;
        displayBoxStyle.alignment = TextAnchor.MiddleCenter;
        displayBoxStyle.border = new RectOffset(2, 2, 2, 2);
        displayBoxStyle.margin = new RectOffset(10, 10, 2, 2);

        monoDisplayTexture = Resources.Load("RUIS/Editor/Textures/monodisplay") as Texture2D;
        stereoDisplayTexture = Resources.Load("RUIS/Editor/Textures/stereodisplay") as Texture2D;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(displaySchema, new GUIContent("XML Schema", "Do not modify this unless you know what you're doing"));
        EditorGUILayout.PropertyField(xmlFilename, new GUIContent("XML filename", "The XML file with the display specifications"));

        EditorGUILayout.PropertyField(resolutionX, new GUIContent("Resolution X", "The width of the display"));
        EditorGUILayout.PropertyField(resolutionY, new GUIContent("Resolution Y", "The height of the display"));
        
        EditorGUILayout.PropertyField(camera, new GUIContent("Attached Camera", "The RUISCamera that renders to this display"));

        EditorGUILayout.PropertyField(isKeystoneCorrected, new GUIContent("Keystone Correction", "Should this display be keystone corrected?"));

        EditorGUILayout.PropertyField(isHMD, new GUIContent("Head-Mounted Display", "Is this display a HMD?"));

        EditorGUILayout.PropertyField(isStereo, new GUIContent("Stereo Display", "Is this display stereo?"));
        if (isStereo.boolValue)
        {
            EditorGUILayout.PropertyField(stereoType, new GUIContent("Stereo Type", "The type of stereo to use"));
            EditorGUILayout.PropertyField(useDoubleTheSpace, new GUIContent("Double the Space used", "Calculate the total resolution of the display based on stereo type. \nSideBySide: Double horizontal resolution \nTopAndBottom: Double vertical resolution."));
        }

        serializedObject.ApplyModifiedProperties();


        int optimalWidth = Screen.width - 4;
        int optimalHeight = (int)((float)resolutionY.intValue / resolutionX.intValue * optimalWidth);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal(GUILayout.Height(optimalHeight));

        
        if (isStereo.boolValue)
        {
            displayBoxStyle.normal.background = stereoDisplayTexture;
        }
        else
        {
            displayBoxStyle.normal.background = monoDisplayTexture;
        }

        RUISDisplay display = target as RUISDisplay;
        int requiredX = display.rawResolutionX;
        int requiredY = display.rawResolutionY;
        string boxText = string.Format("{0}\nTotal required resolution {1}x{2}", target.name, requiredX, requiredY);

        GUILayout.Box(boxText, displayBoxStyle, GUILayout.Width(optimalWidth), GUILayout.Height(optimalHeight));

        EditorGUILayout.EndHorizontal();
    }
}
