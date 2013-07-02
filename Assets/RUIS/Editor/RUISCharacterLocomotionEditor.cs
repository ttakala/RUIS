/*****************************************************************************

Content    :   Inspector behaviour for RUISCharacterLocomotion script
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISCharacterLocomotionControl))]
[CanEditMultipleObjects]
public class RUISCharacterLocomotionEditor : Editor {
	
	SerializedProperty turnRightKey;
    SerializedProperty turnLeftKey;

    SerializedProperty rotationScaler;

    SerializedProperty speed;
    SerializedProperty gravity;
    SerializedProperty maxVelocityChange;

    SerializedProperty usePSNavigationController;
    SerializedProperty PSNaviControllerID;
	SerializedProperty strafeInsteadTurning;

    SerializedProperty jumpStrength;

    public void OnEnable()
    {
        turnRightKey = serializedObject.FindProperty("turnRightKey");
        turnLeftKey = serializedObject.FindProperty("turnLeftKey");
        rotationScaler = serializedObject.FindProperty("rotationScaler");
        speed = serializedObject.FindProperty("speed");
        gravity = serializedObject.FindProperty("gravity");
        maxVelocityChange = serializedObject.FindProperty("maxVelocityChange");
        usePSNavigationController = serializedObject.FindProperty("usePSNavigationController");
        PSNaviControllerID = serializedObject.FindProperty("PSNaviControllerID");
        strafeInsteadTurning = serializedObject.FindProperty("strafeInsteadTurning");
        jumpStrength = serializedObject.FindProperty("jumpStrength");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(turnRightKey, new GUIContent("Turn Right Key", "Which key is used to rotate the character to rigth"));
        EditorGUILayout.PropertyField(turnLeftKey, new GUIContent("Turn Left Key", "Which key is used to rotate the character to left"));
        EditorGUILayout.PropertyField(rotationScaler, new GUIContent("Rotation Speed", "How fast is the character rotating when pressing turn key"));
        EditorGUILayout.PropertyField(speed, new GUIContent("Speed", "How fast is the character moving with Input.GetAxis()"));
        EditorGUILayout.PropertyField(maxVelocityChange, new GUIContent("Max Velocity Change", "How fast character can change existing velocity (e.g. sliding)"));
		EditorGUILayout.PropertyField(jumpStrength, new GUIContent("Jump Strength", "Mass-invariant impulse force that is applied when jumping"));
		
        EditorGUILayout.PropertyField(usePSNavigationController, new GUIContent("Use PSNavigation Controller", "Enable character controls for PS Navigation controller"));
		
		
		if(usePSNavigationController.boolValue)
		{
	        EditorGUI.indentLevel += 2;
			
	        EditorGUILayout.PropertyField(PSNaviControllerID, new GUIContent("Controller ID", "Between 0 and 6"));
	        EditorGUILayout.PropertyField(strafeInsteadTurning, new GUIContent("Strafe, Don't Turn", "Whether horizontal direction of analog stick will strafe or turn"));
	        
	        EditorGUI.indentLevel += 2;
		}
		
        serializedObject.ApplyModifiedProperties();
    }
}
