/*****************************************************************************

Content    :   Inspector behavior for a RUISDisplay
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

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
    SerializedProperty displayWidth;
    SerializedProperty displayHeight;
    SerializedProperty horizontalFOV;
    SerializedProperty verticalFOV;

    SerializedProperty isStereo;
    SerializedProperty isHMD;
    SerializedProperty isObliqueFrustum;
    SerializedProperty isKeystoneCorrected;
    SerializedProperty camera;
    SerializedProperty eyeSeparation;
    SerializedProperty stereoType;
    SerializedProperty useDoubleTheSpace;
    GUIStyle displayBoxStyle;

    SerializedProperty displayCenterPosition;
    SerializedProperty displayNormal;
    SerializedProperty displayUp;
    

    private Texture2D monoDisplayTexture;
    private Texture2D stereoDisplayTexture;

    RUISDisplayManager displayManager;

    void OnEnable()
    {
        xmlFilename = serializedObject.FindProperty("xmlFilename");
        displaySchema = serializedObject.FindProperty("displaySchema");

        resolutionX = serializedObject.FindProperty("resolutionX");
        resolutionY = serializedObject.FindProperty("resolutionY");
        displayWidth = serializedObject.FindProperty("width");
        displayHeight = serializedObject.FindProperty("height");
        horizontalFOV = serializedObject.FindProperty("horizontalFOV");
        verticalFOV = serializedObject.FindProperty("verticalFOV");

        isStereo = serializedObject.FindProperty("isStereo");
        isHMD = serializedObject.FindProperty("isHMD");
        isObliqueFrustum = serializedObject.FindProperty("isObliqueFrustum");
        isKeystoneCorrected = serializedObject.FindProperty("isKeystoneCorrected");
        camera = serializedObject.FindProperty("_linkedCamera");
        eyeSeparation = serializedObject.FindProperty("eyeSeparation");
        stereoType = serializedObject.FindProperty("stereoType");
        useDoubleTheSpace = serializedObject.FindProperty("useDoubleTheSpace");

        displayCenterPosition = serializedObject.FindProperty("displayCenterPosition");
        displayNormal = serializedObject.FindProperty("displayNormalInternal");
        displayUp = serializedObject.FindProperty("displayUpInternal");

        displayBoxStyle = new GUIStyle();
        displayBoxStyle.normal.textColor = Color.white;
        displayBoxStyle.alignment = TextAnchor.MiddleCenter;
        displayBoxStyle.border = new RectOffset(2, 2, 2, 2);
        displayBoxStyle.margin = new RectOffset(10, 10, 2, 2);

        monoDisplayTexture = Resources.Load("RUIS/Editor/Textures/monodisplay") as Texture2D;
        stereoDisplayTexture = Resources.Load("RUIS/Editor/Textures/stereodisplay") as Texture2D;

        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
    }

    public void OnGUI()
    {
    }

    public override void OnInspectorGUI()
    {


        serializedObject.Update();

        EditorGUILayout.PropertyField(displaySchema, new GUIContent("XML Schema", "Do not modify this unless you know what you're doing"));
        EditorGUILayout.PropertyField(xmlFilename, new GUIContent("XML filename", "The XML file with the display specifications"));

        EditorGUILayout.PropertyField(resolutionX, new GUIContent("Resolution X", "The pixel width of the display"));
        EditorGUILayout.PropertyField(resolutionY, new GUIContent("Resolution Y", "The pixel height of the display"));

        EditorGUILayout.PropertyField(isStereo, new GUIContent("Stereo Display", "Is this display stereo?"));
        if (isStereo.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(eyeSeparation, new GUIContent("Eye Separation", "Eye separation for the stereo image"));
            EditorGUILayout.PropertyField(stereoType, new GUIContent("Stereo Type", "The type of stereo to use"));
            EditorGUILayout.PropertyField(useDoubleTheSpace, new GUIContent("Double the Space used", "Calculate the total resolution of the display based on stereo type. \nSideBySide: Double horizontal resolution \nTopAndBottom: Double vertical resolution."));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(horizontalFOV, new GUIContent("Horizontal Field of View", "The horizontal FOV of the display if not using a head tracked view"));
        EditorGUILayout.PropertyField(verticalFOV, new GUIContent("Vertical Field of View", "The vertical FOV of the display if not using a head tracked view"));
        
        EditorGUILayout.PropertyField(camera, new GUIContent("Attached Camera", "The RUISCamera that renders to this display"));

        

        EditorGUILayout.PropertyField(isHMD, new GUIContent("Head-Mounted Display", "Is this display a HMD?"));

        if (!isHMD.boolValue)
        {
            //disabled for now EditorGUILayout.PropertyField(isKeystoneCorrected, new GUIContent("Keystone Correction", "Should this display be keystone corrected?"));

            EditorGUILayout.PropertyField(isObliqueFrustum, new GUIContent("Head Tracked View", "Should the projection matrix be skewed to use this display as a head tracked viewport"));

            if (isObliqueFrustum.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(displayWidth, new GUIContent("Display Width", "The real-world width of the display"));
                EditorGUILayout.PropertyField(displayHeight, new GUIContent("Display Height", "The real-world height of the display"));
                EditorGUILayout.PropertyField(displayCenterPosition, new GUIContent("Display Center Position", "The location of the screen center in Unity coordinates"));
                EditorGUILayout.PropertyField(displayNormal, new GUIContent("Display Normal Vector", "The normal vector of the display (will be normalized)"));
                EditorGUILayout.PropertyField(displayUp, new GUIContent("Display Up Vector", "The up vector of the display (will be normalized)"));
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            isObliqueFrustum.boolValue = false;
            isKeystoneCorrected.boolValue = false;
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

        displayManager.CalculateTotalResolution();
        PlayerSettings.defaultScreenWidth = displayManager.totalRawResolutionX;
        PlayerSettings.defaultScreenHeight = displayManager.totalRawResolutionY;
    }

}
