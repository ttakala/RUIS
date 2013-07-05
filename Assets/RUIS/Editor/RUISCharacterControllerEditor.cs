/*****************************************************************************

Content    :   Inspector behaviour for RUISCharacterController script
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Mikael Matveinen, Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISCharacterController))]
[CanEditMultipleObjects]
public class RUISCharacterControllerEditor : Editor
{
    SerializedProperty characterPivotType;
    SerializedProperty kinectPlayerId;
    SerializedProperty moveControllerId;
    SerializedProperty ignorePitchAndRoll;
    SerializedProperty groundLayers;
    SerializedProperty groundedErrorTweaker;

    public void OnEnable()
    {
        characterPivotType = serializedObject.FindProperty("characterPivotType");
        kinectPlayerId = serializedObject.FindProperty("kinectPlayerId");
        moveControllerId = serializedObject.FindProperty("moveControllerId");
        ignorePitchAndRoll = serializedObject.FindProperty("ignorePitchAndRoll");
        groundLayers = serializedObject.FindProperty("groundLayers");
        groundedErrorTweaker = serializedObject.FindProperty("groundedErrorTweaker");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(characterPivotType, new GUIContent("Character Pivot Type", "What kind of pivot should be used for the character"));

        switch (characterPivotType.enumValueIndex)
        {
            case (int)RUISCharacterController.CharacterPivotType.KinectHead:
            case (int)RUISCharacterController.CharacterPivotType.KinectHip:
            case (int)RUISCharacterController.CharacterPivotType.KinectCOM:
                EditorGUILayout.PropertyField(kinectPlayerId, new GUIContent("Kinect Player ID", "Between 0 and 3"));
                break;
            case (int)RUISCharacterController.CharacterPivotType.MoveController:
                EditorGUILayout.PropertyField(moveControllerId, new GUIContent("PS Move ID", "Between 0 and 3"));
                break;
        }

        EditorGUILayout.PropertyField(ignorePitchAndRoll, new GUIContent("Ignore Pitch and Roll", "Should the pitch and roll values of the pivot rotation be taken into account when transforming directions into character coordinates?"));

        EditorGUILayout.PropertyField(groundLayers, new GUIContent("Ground Layers", "The layers to take into account when checking whether the character is grounded"));

        EditorGUILayout.PropertyField(groundedErrorTweaker, new GUIContent("Grounded error tweaker", "This value can be adjusted to allow for some leniency in the checks whether the character is grounded"));

        serializedObject.ApplyModifiedProperties();
    }
}
