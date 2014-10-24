/*****************************************************************************

Content    :   Inspector behaviour for RUISKinectAndMecanimCombiner script
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISSkeletonController))]
[CanEditMultipleObjects]
public class RUISSkeletonControllerEditor : Editor
{

	SerializedProperty bodyTrackingDevice;
	SerializedProperty playerId;
	SerializedProperty switchToAvailableKinect;

    SerializedProperty useHierarchicalModel;

    SerializedProperty updateRootPosition;
    SerializedProperty updateJointPositions;
    SerializedProperty updateJointRotations;

	SerializedProperty scaleHierarchicalModelBones;
	SerializedProperty scaleBoneLengthOnly;
	SerializedProperty boneLengthAxis;

    SerializedProperty rootBone;
    SerializedProperty headBone;
    SerializedProperty neckBone;
    SerializedProperty torsoBone;

    SerializedProperty leftShoulderBone;
    SerializedProperty leftElbowBone;
    SerializedProperty leftHandBone;
    SerializedProperty rightShoulderBone;
    SerializedProperty rightElbowBone;
    SerializedProperty rightHandBone;

    SerializedProperty leftHipBone;
    SerializedProperty leftKneeBone;
    SerializedProperty leftFootBone;
    SerializedProperty rightHipBone;
    SerializedProperty rightKneeBone;
    SerializedProperty rightFootBone;

	SerializedProperty leftThumb;
	SerializedProperty rightThumb;

    SerializedProperty maxScaleFactor;
    SerializedProperty minimumConfidenceToUpdate;
    SerializedProperty rotationDamping;
    SerializedProperty neckHeightTweaker;
	SerializedProperty forearmLengthTweaker;
	SerializedProperty shinLengthTweaker;
//	SerializedProperty adjustVerticalTorsoPosition;
	SerializedProperty adjustVerticalHipsPosition;

	SerializedProperty fistCurlFingers;
	SerializedProperty trackThumbs;
	SerializedProperty trackAnkle;
	SerializedProperty rotateWristFromElbow;
	
	SerializedProperty customRoot;
	SerializedProperty customHead;
	SerializedProperty customNeck;
	SerializedProperty customTorso;
	SerializedProperty customRightShoulder;
	SerializedProperty customRightElbow;
	SerializedProperty customRightHand;
	SerializedProperty customRightHip;
	SerializedProperty customRightKnee;
	SerializedProperty customRightFoot;
	SerializedProperty customLeftShoulder;
	SerializedProperty customLeftElbow;
	SerializedProperty customLeftHand;
	SerializedProperty customLeftHip;
	SerializedProperty customLeftKnee;
	SerializedProperty customLeftFoot;
	SerializedProperty customLeftThumb;
	SerializedProperty customRightThumb;
	

    public void OnEnable()
    {

		bodyTrackingDevice = serializedObject.FindProperty("bodyTrackingDevice");
		playerId = serializedObject.FindProperty("playerId");
		switchToAvailableKinect = serializedObject.FindProperty("switchToAvailableKinect");

        useHierarchicalModel = serializedObject.FindProperty("useHierarchicalModel");

        updateRootPosition = serializedObject.FindProperty("updateRootPosition");
        updateJointPositions = serializedObject.FindProperty("updateJointPositions");
        updateJointRotations = serializedObject.FindProperty("updateJointRotations");

		scaleHierarchicalModelBones = serializedObject.FindProperty("scaleHierarchicalModelBones");
		scaleBoneLengthOnly = serializedObject.FindProperty("scaleBoneLengthOnly");
		boneLengthAxis = serializedObject.FindProperty("boneLengthAxis");

        rootBone = serializedObject.FindProperty("root");
        headBone = serializedObject.FindProperty("head");
        neckBone = serializedObject.FindProperty("neck");
        torsoBone = serializedObject.FindProperty("torso");

        leftShoulderBone = serializedObject.FindProperty("leftShoulder");
        leftElbowBone = serializedObject.FindProperty("leftElbow");
        leftHandBone = serializedObject.FindProperty("leftHand");
        rightShoulderBone = serializedObject.FindProperty("rightShoulder");
        rightElbowBone = serializedObject.FindProperty("rightElbow");
        rightHandBone = serializedObject.FindProperty("rightHand");

        leftHipBone = serializedObject.FindProperty("leftHip");
        leftKneeBone = serializedObject.FindProperty("leftKnee");
        leftFootBone = serializedObject.FindProperty("leftFoot");
        rightHipBone = serializedObject.FindProperty("rightHip");
        rightKneeBone = serializedObject.FindProperty("rightKnee");
        rightFootBone = serializedObject.FindProperty("rightFoot");

		leftThumb = serializedObject.FindProperty ("leftThumb");
		rightThumb = serializedObject.FindProperty ("rightThumb");
		trackAnkle = serializedObject.FindProperty ("trackAnkle");
		rotateWristFromElbow = serializedObject.FindProperty ("rotateWristFromElbow");
		
//		adjustVerticalTorsoPosition = serializedObject.FindProperty("adjustVerticalTorsoPosition");
		adjustVerticalHipsPosition = serializedObject.FindProperty("adjustVerticalHipsPosition");
        maxScaleFactor = serializedObject.FindProperty("maxScaleFactor");
        minimumConfidenceToUpdate = serializedObject.FindProperty("minimumConfidenceToUpdate");
        rotationDamping = serializedObject.FindProperty("rotationDamping");
        neckHeightTweaker = serializedObject.FindProperty("neckHeightTweaker");
        forearmLengthTweaker = serializedObject.FindProperty("forearmLengthRatio");
		shinLengthTweaker = serializedObject.FindProperty("shinLengthRatio");
		
		fistCurlFingers = serializedObject.FindProperty("fistCurlFingers");
		trackThumbs = serializedObject.FindProperty("trackThumbs");
		
		customRoot  = serializedObject.FindProperty("customRoot");
		customHead  = serializedObject.FindProperty("customHead");
		customNeck  = serializedObject.FindProperty("customNeck");
		customTorso  = serializedObject.FindProperty("customTorso");
		customRightShoulder  = serializedObject.FindProperty("customRightShoulder");
		customRightElbow  = serializedObject.FindProperty("customRightElbow");
		customRightHand  = serializedObject.FindProperty("customRightHand");
		customRightHip  = serializedObject.FindProperty("customRightHip");
		customRightKnee  = serializedObject.FindProperty("customRightKnee");
		customRightFoot  = serializedObject.FindProperty("customRightFoot");
		customLeftShoulder  = serializedObject.FindProperty("customLeftShoulder");
		customLeftElbow  = serializedObject.FindProperty("customLeftElbow");
		customLeftHand  = serializedObject.FindProperty("customLeftHand");
		customLeftHip  = serializedObject.FindProperty("customLeftHip");
		customLeftKnee  = serializedObject.FindProperty("customLeftKnee");
		customLeftFoot  = serializedObject.FindProperty("customLeftFoot");
		customLeftThumb  = serializedObject.FindProperty("customLeftThumb");
		customRightThumb  = serializedObject.FindProperty("customRightThumb");
		
		
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
		 
		EditorGUILayout.Space();
		 
		EditorGUILayout.PropertyField(bodyTrackingDevice, new GUIContent("Body Tracking Device", "")); 
		
		EditorGUILayout.Space();
		if (bodyTrackingDevice.enumValueIndex == 0 || bodyTrackingDevice.enumValueIndex == 1) 
		{
			EditorGUILayout.PropertyField(playerId, new GUIContent("Kinect Player ID", "The Kinect player ID number"));
			EditorGUILayout.PropertyField(switchToAvailableKinect, new GUIContent(  "Switch To Available Kinect", "Examine RUIS InputManager settings, and "
			                                                                      + "switch Body Tracking Device from Kinect 1 to Kinect 2 in run-time if "
			                                                                      + "the latter is enabled but the former is not, and vice versa."));
        }
		
		RUISEditorUtility.HorizontalRuler();

        EditorGUILayout.PropertyField(useHierarchicalModel, new GUIContent(  "Hierarchical Model", "Is the model rig hierarchical (a tree) "
		                                                                   + "instead of non-hierarchical (all bones are on same level)?"));

        EditorGUILayout.PropertyField(updateRootPosition, new GUIContent(  "Update Root Position", "Update the position of this GameObject according "
		                                                                 + "to the skeleton root position"));

        GUI.enabled = !useHierarchicalModel.boolValue;
		EditorGUILayout.PropertyField(updateJointPositions, new GUIContent(  "Update Joint Positions", "Unavailable for hierarchical "
		                                                                   + "models, since there the skeleton structure already "
		                                                                   + "handles positions with joint rotations."));
        if (useHierarchicalModel.boolValue) updateJointPositions.boolValue = false;
        GUI.enabled = true;

        EditorGUILayout.PropertyField(updateJointRotations, new GUIContent(  "Update Joint Rotations", "Enabling this is especially "
		                                                                   + "important for hierarchical models."));

        GUI.enabled = useHierarchicalModel.boolValue;
        EditorGUILayout.PropertyField(scaleHierarchicalModelBones, new GUIContent(  "Scale Bones", "Scale the bones of the model based on the "
		                                                                          + "real-life lengths of the player bones. This option is only "
		                                                                          + "available for hierarchical models."));

		GUI.enabled = scaleHierarchicalModelBones.boolValue;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(scaleBoneLengthOnly, new GUIContent(  "Scale Length Only", "Scale the bone length (localScale.x/y/z) but not the "
		                                                                  + "bone thickness (localScale.yz/xz/xy). WARNING: Enabling this option could "
		                                                                  + "lead to peculiar results, depending on the animation rig."));
		EditorGUILayout.PropertyField(boneLengthAxis, new GUIContent(  "Length Axis for Scaling", "Determines the localScale axis along which each bone is "
		                                                             + "scaled, if only length is scaled. You can discover the correct axis by examining "
		                                                             + "the animation rig hierarchy, by looking at the directional axis between parent "
		                                                             + "joints and their child joints in local coordinate system. IMPORTANT: Disable the "
		                                                             + "above 'Scale Length Only' option if the same localScale axis is not consistently "
		                                                             + "used in all the joints of the animation rig."));
		EditorGUI.indentLevel--;

        if (!useHierarchicalModel.boolValue)
		{
			scaleHierarchicalModelBones.boolValue = false;
		}
		if (!scaleHierarchicalModelBones.boolValue)
			scaleBoneLengthOnly.boolValue = false;

        GUI.enabled = true;

		EditorGUILayout.Space();
		
		
		if (bodyTrackingDevice.enumValueIndex == 2) {
			
			EditorGUILayout.Space();
			
			EditorGUILayout.TextArea("Place your custom motion tracker transforms below.", GUILayout.Height(20));
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Torso and Head", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(customRoot, new GUIContent("Root Joint", "The skeleton hierarchy root bone"));
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(customTorso, new GUIContent("Torso", "The torso bone, has to be parent or grandparent of the hips"));
			EditorGUILayout.PropertyField(customNeck, new GUIContent("Neck", "The neck bone"));
			EditorGUILayout.PropertyField(customHead, new GUIContent("Head", "The head bone"));
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Arms", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
			EditorGUILayout.PropertyField(customLeftShoulder, new GUIContent("Left Shoulder", "The left shoulder bone (upper arm)"));
			EditorGUILayout.PropertyField(customLeftElbow, new GUIContent("Left Elbow", "The left elbow bone (forearm)"));
			EditorGUILayout.PropertyField(customLeftHand, new GUIContent("Left Hand", "The left wrist bone (hand)"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
			EditorGUILayout.PropertyField(customRightShoulder, new GUIContent("Right Shoulder", "The right shoulder bone (upper arm)"));
			EditorGUILayout.PropertyField(customRightElbow, new GUIContent("Right Elbow", "The right elbow bone (forearm)"));
			EditorGUILayout.PropertyField(customRightHand, new GUIContent("Right Hand", "The right wrist bone (hand)"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Legs", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
			EditorGUILayout.PropertyField(customLeftHip, new GUIContent("Left Hip", "The left hip bone (thigh)"));
			EditorGUILayout.PropertyField(customLeftKnee, new GUIContent("Left Knee", "The left knee bone (shin)"));
			EditorGUILayout.PropertyField(customLeftFoot, new GUIContent("Left Foot", "The left ankle bone (foot)"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
			EditorGUILayout.PropertyField(customRightHip, new GUIContent("Right Hip", "The right hip bone (thigh)"));
			EditorGUILayout.PropertyField(customRightKnee, new GUIContent("Right Knee", "The right knee bone (shin)"));
			EditorGUILayout.PropertyField(customRightFoot, new GUIContent("Right Foot", "The right ankle bone (foot)"));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField ("Source for Fingers", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.BeginVertical (GUILayout.Width (Screen.width / 2 - 20));
			EditorGUILayout.PropertyField (customLeftThumb, new GUIContent ("Left Thumb", "The thumb of the left hand"));
			EditorGUILayout.EndVertical ();
			EditorGUILayout.BeginVertical (GUILayout.Width (Screen.width / 2 - 20));
			EditorGUILayout.PropertyField (customRightThumb, new GUIContent ("Right Thumb", "The thumb of the right hand"));
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndHorizontal ();
			
			
		}
		RUISEditorUtility.HorizontalRuler();
        EditorGUILayout.LabelField("Torso and Head Joints", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rootBone, new GUIContent("Root Joint", "The skeleton hierarchy root bone"));
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(torsoBone, new GUIContent("Torso", "The torso bone, has to be parent or grandparent of the hips"));
		EditorGUILayout.PropertyField(neckBone, new GUIContent("Neck", "The neck bone"));
        EditorGUILayout.PropertyField(headBone, new GUIContent("Head", "The head bone"));

		EditorGUILayout.Space();
		
        EditorGUILayout.LabelField("Arm Joints", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
            EditorGUILayout.PropertyField(leftShoulderBone, new GUIContent("Left Shoulder", "The left shoulder bone (upper arm)"));
            EditorGUILayout.PropertyField(leftElbowBone, new GUIContent("Left Elbow", "The left elbow bone (forearm)"));
            EditorGUILayout.PropertyField(leftHandBone, new GUIContent("Left Hand", "The left wrist bone (hand)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
            EditorGUILayout.PropertyField(rightShoulderBone, new GUIContent("Right Shoulder", "The right shoulder bone (upper arm)"));
            EditorGUILayout.PropertyField(rightElbowBone, new GUIContent("Right Elbow", "The right elbow bone (forearm)"));
            EditorGUILayout.PropertyField(rightHandBone, new GUIContent("Right Hand", "The right wrist bone (hand)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
		
		if (bodyTrackingDevice.enumValueIndex == 1) {
			EditorGUILayout.PropertyField(rotateWristFromElbow, new GUIContent("Wrist Rotates Lower Arm", "Should wrist rotate whole lower arm or just the hand?"));
		}		

		EditorGUILayout.Space();
	
		EditorGUILayout.LabelField("Leg Joints", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        EditorGUILayout.PropertyField(leftHipBone, new GUIContent("Left Hip", "The left hip bone (thigh)"));
        EditorGUILayout.PropertyField(leftKneeBone, new GUIContent("Left Knee", "The left knee bone (shin)"));
        EditorGUILayout.PropertyField(leftFootBone, new GUIContent("Left Foot", "The left ankle bone (foot)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 20));
        EditorGUILayout.PropertyField(rightHipBone, new GUIContent("Right Hip", "The right hip bone (thigh)"));
        EditorGUILayout.PropertyField(rightKneeBone, new GUIContent("Right Knee", "The right knee bone (shin)"));
        EditorGUILayout.PropertyField(rightFootBone, new GUIContent("Right Foot", "The right ankle bone (foot)"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

		if (bodyTrackingDevice.enumValueIndex == 1) {
			EditorGUILayout.PropertyField(trackAnkle, new GUIContent("Track Ankle", "Track the rotation of the ankle bone"));
		}
		
		EditorGUILayout.Space();

		if (bodyTrackingDevice.enumValueIndex == 1) {
				EditorGUILayout.LabelField ("Finger Joints", EditorStyles.boldLabel);
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.BeginVertical (GUILayout.Width (Screen.width / 2 - 20));
				EditorGUILayout.PropertyField (leftThumb, new GUIContent ("Left Thumb", "The thumb of the left hand"));
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical (GUILayout.Width (Screen.width / 2 - 20));
				EditorGUILayout.PropertyField (rightThumb, new GUIContent ("Right Thumb", "The thumb of the right hand"));
				EditorGUILayout.EndVertical ();
				EditorGUILayout.EndHorizontal ();
			
			
			EditorGUILayout.PropertyField(fistCurlFingers, new GUIContent("Track Fist Curling", "Should finger in fist be curled when closing hand?"));
			EditorGUILayout.PropertyField(trackThumbs, new GUIContent("Track Thumbs", "Track thumb movement."));
			
		}
		
		RUISEditorUtility.HorizontalRuler();
		
        EditorGUILayout.LabelField("Tweaking", EditorStyles.boldLabel);
        GUI.enabled = scaleHierarchicalModelBones.boolValue;
        EditorGUILayout.PropertyField(maxScaleFactor, new GUIContent(  "Max Scale Rate", "The maximum amount the scale of a bone can "
		                                                             + "change per second when using Hierarchical Model and Scale Bones"));
        GUI.enabled = true;
        EditorGUILayout.PropertyField(minimumConfidenceToUpdate, new GUIContent(  "Min Confidence to Update", "The minimum confidence in joint "
		                                                                        + "positions and rotations needed to update these values. "
		                                                                        + "The confidence is either 0; 0,5; or 1."));
        EditorGUILayout.PropertyField(rotationDamping, new GUIContent(  "Max Joint Angular Velocity", "Maximum joint angular velocity can be used "
		                                                              + "for damping character bone movement (smaller values)"));

//		EditorGUILayout.PropertyField(adjustVerticalTorsoPosition, new GUIContent(  "Torso Vertical Tweaker", "Offset the tracked torso point "
//		                                                                          + "position in the spine direction (usually vertical axis). "
//			                                                                          + "Only used if Hierarchical Model is enabled"));
		EditorGUILayout.PropertyField(adjustVerticalHipsPosition, new GUIContent(  "Hips Vertical Tweaker", "Offset the tracked hip center point "
		                                                                          + "position in the spine direction (usually vertical axis). "
		                                                                          + "Only used if Hierarchical Model is enabled"));

        EditorGUILayout.PropertyField(neckHeightTweaker, new GUIContent("Neck Height Tweaker", "The height offset for the neck"));

		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.PropertyField(forearmLengthTweaker, new GUIContent(  "Forearm Length Tweaker", "The forearm length ratio "
		                                                                   + "compared to the real-world value, use this to lengthen "
		                                                                   + "or shorten the forearms. Only used if Hierarchical Model is enabled"));
		EditorGUILayout.PropertyField(shinLengthTweaker, new GUIContent(  "Shin Length Tweaker", "The shin length ratio compared to the "
		                                                                + "real-world value, use this to lengthen or shorten the "
		                                                                + "shins. Only used if Hierarchical Model is enabled"));
		EditorGUILayout.Space();
		
		GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
