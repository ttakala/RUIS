using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(RUISDisplay))]
[CanEditMultipleObjects]
public class RUISDisplayEditor : Editor {
    SerializedProperty resolutionX;
    SerializedProperty resolutionY;
    SerializedProperty isStereo;
    SerializedProperty isHMD;
    SerializedProperty camera;
    SerializedProperty stereoType;
    SerializedProperty useDoubleTheSpace;
    GUIStyle displayBoxStyle;

    private Texture2D monoDisplayTexture;
    private Texture2D stereoDisplayTexture;

    void OnEnable()
    {
        resolutionX = serializedObject.FindProperty("resolutionX");
        resolutionY = serializedObject.FindProperty("resolutionY");
        isStereo = serializedObject.FindProperty("isStereo");
        isHMD = serializedObject.FindProperty("isHMD");
        camera = serializedObject.FindProperty("camera");
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

        EditorGUILayout.PropertyField(resolutionX, new GUIContent("Resolution X"));
        EditorGUILayout.PropertyField(resolutionY, new GUIContent("Resolution Y"));
        
        EditorGUILayout.PropertyField(camera, new GUIContent("Attached Camera"));

        EditorGUILayout.PropertyField(isHMD, new GUIContent("Head-Mounted Display"));

        EditorGUILayout.PropertyField(isStereo, new GUIContent("Stereo Display"));
        if (isStereo.boolValue)
        {
            EditorGUILayout.PropertyField(stereoType, new GUIContent("Stereo Type"));
            EditorGUILayout.PropertyField(useDoubleTheSpace, new GUIContent("Double the Space used"));
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
