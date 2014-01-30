/*****************************************************************************

Content    :   Inspector behaviour for RUISCharacterLocomotion script
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISCharacterLocomotion))]
[CanEditMultipleObjects]
public class RUISCharacterLocomotionEditor : Editor {
	
	SerializedProperty turnRightKey;
    SerializedProperty turnLeftKey;

    SerializedProperty rotationScaler;

    SerializedProperty speed;
	SerializedProperty runAdder;
    SerializedProperty maxVelocityChange;

    SerializedProperty usePSNavigationController;
    SerializedProperty PSNaviControllerID;
	SerializedProperty strafeInsteadTurning;
	
	SerializedProperty useRazerHydra;
	SerializedProperty razerHydraID;
	
    SerializedProperty jumpStrength;
	SerializedProperty jumpSpeedEffect;
	SerializedProperty aerialMobility;

    public void OnEnable()
    {
        turnRightKey = serializedObject.FindProperty("turnRightKey");
        turnLeftKey = serializedObject.FindProperty("turnLeftKey");
        rotationScaler = serializedObject.FindProperty("rotationScaler");
        speed = serializedObject.FindProperty("speed");
		runAdder = serializedObject.FindProperty("runAdder");
        maxVelocityChange = serializedObject.FindProperty("maxVelocityChange");
        usePSNavigationController = serializedObject.FindProperty("usePSNavigationController");
        PSNaviControllerID = serializedObject.FindProperty("PSNaviControllerID");
        strafeInsteadTurning = serializedObject.FindProperty("strafeInsteadTurning");
        jumpStrength = serializedObject.FindProperty("jumpStrength");
		useRazerHydra = serializedObject.FindProperty("useRazerHydra");
		razerHydraID = serializedObject.FindProperty("razerHydraID");
		jumpSpeedEffect = serializedObject.FindProperty("jumpSpeedEffect");
		aerialMobility = serializedObject.FindProperty("aerialMobility");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
		
		if(jumpSpeedEffect.floatValue < 0)
			jumpSpeedEffect.floatValue = 0;

        EditorGUILayout.PropertyField(turnRightKey, new GUIContent("Turn Right Key", "Which key is used to rotate the character to rigth"));
        EditorGUILayout.PropertyField(turnLeftKey, new GUIContent("Turn Left Key", "Which key is used to rotate the character to left"));
        EditorGUILayout.PropertyField(rotationScaler, new GUIContent("Rotation Speed", "How fast is the character rotating when pressing turn key (degrees/s)"));
        EditorGUILayout.PropertyField(speed, new GUIContent("Moving Speed", "How fast is the character's walking speed (m/s) with Input.GetAxis()"));
		EditorGUILayout.PropertyField(runAdder, new GUIContent(  "Sprint Effect", "How much the 'Sprint' button affects the Moving Speed. Value 0 means no effect, "
		                                                       + "value 1 means double speed and so on. This value should be non-negative."));
		EditorGUILayout.PropertyField(maxVelocityChange, new GUIContent(  "Max Acceleration", "The maximum rate of velocity change (m/s^2) when "
		                                                                + "starting or stopping locomotion."));
		EditorGUILayout.PropertyField(jumpStrength, new GUIContent(  "Jump Strength", "Mass-invariant impulse force that is applied when jumping. Note that this "
		                                                           + "value has a non-linear relationship with the resulting jump height (which also depends on "
		                                                           + "the gravity)."));
		EditorGUILayout.PropertyField(jumpSpeedEffect, new GUIContent("Speed Effect on Jump", "How much speed affects the Jump Strength. Value 0 means no effect, "
																	+ "value 1 means double strength when moving at sprint speed and so on."));
		EditorGUILayout.PropertyField(aerialMobility, new GUIContent("Aerial Mobility", "At which rate the character can change his velocity while jumping "
																	+ "or airborne. This value is relative to Moving Speed."));
		
        EditorGUILayout.PropertyField(useRazerHydra, new GUIContent("Use Razer Hydra", "Enable walking with a Razer Hydra controller"));
		
		if(useRazerHydra.boolValue)
		{
	        EditorGUI.indentLevel += 2;
			
	        EditorGUILayout.PropertyField(razerHydraID, new GUIContent("Controller ID", "LEFT or RIGHT"));
	        EditorGUILayout.PropertyField(strafeInsteadTurning, new GUIContent("Strafe, Don't Turn", "Whether horizontal direction of analog stick will strafe or turn"));
	        
	        EditorGUI.indentLevel -= 2;
		}
		
        EditorGUILayout.PropertyField(usePSNavigationController, new GUIContent("Use PS Navi Controller", "Enable walking with a PS Navigation controller"));
		
		if(usePSNavigationController.boolValue)
		{
	        EditorGUI.indentLevel += 2;
			
			PSNaviControllerID.intValue = Mathf.Clamp(PSNaviControllerID.intValue, 0, 6);
	        EditorGUILayout.PropertyField(PSNaviControllerID, new GUIContent("Controller ID", "Between 0 and 6"));
	        EditorGUILayout.PropertyField(strafeInsteadTurning, new GUIContent("Strafe, Don't Turn", "Whether horizontal direction of analog stick will strafe or turn"));
	        
	        EditorGUI.indentLevel -= 2;
		}
		
        serializedObject.ApplyModifiedProperties();
    }
}
