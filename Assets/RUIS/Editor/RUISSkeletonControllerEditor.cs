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
	
	SerializedProperty rootSpeedScaling;
	SerializedProperty rootOffset;
	SerializedProperty hmdRotatesHead;
	SerializedProperty hmdMovesHead;

	SerializedProperty scaleHierarchicalModelBones;
	SerializedProperty scaleBoneLengthOnly;
	SerializedProperty boneLengthAxis;
	SerializedProperty torsoThickness;
	SerializedProperty rightArmThickness;
	SerializedProperty leftArmThickness;
	SerializedProperty rightLegThickness;
	SerializedProperty leftLegThickness;

	SerializedProperty filterPosition;
	SerializedProperty positionNoiseCovariance;

	SerializedProperty filterRotations;
	SerializedProperty rotationNoiseCovariance;
	SerializedProperty thumbZRotationOffset;

    SerializedProperty rootBone;
	SerializedProperty chestBone;
	SerializedProperty torsoBone;
	SerializedProperty neckBone;
	SerializedProperty headBone;

	SerializedProperty leftClavicle;
    SerializedProperty leftShoulderBone;
    SerializedProperty leftElbowBone;
    SerializedProperty leftHandBone;
	SerializedProperty rightClavicle;
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
	SerializedProperty trackWrist;
	SerializedProperty trackAnkle;
//	SerializedProperty rotateWristFromElbow;

	SerializedProperty pelvisOffset;
	SerializedProperty chestOffset;
	SerializedProperty neckOffset;
	SerializedProperty headOffset;
	SerializedProperty clavicleOffset;
	SerializedProperty shoulderOffset;
	SerializedProperty hipOffset;

	SerializedProperty feetRotationOffset;

	SerializedProperty pelvisScaleAdjust;
	SerializedProperty chestScaleAdjust;
	SerializedProperty neckScaleAdjust;
	SerializedProperty headScaleAdjust;
	SerializedProperty clavicleScaleAdjust;
	SerializedProperty handScaleAdjust;
	SerializedProperty footScaleAdjust;

	SerializedProperty customRoot;
	SerializedProperty customTorso;
	SerializedProperty customChest;
	SerializedProperty customNeck;
	SerializedProperty customHead;
	SerializedProperty customRightClavicle;
	SerializedProperty customRightShoulder;
	SerializedProperty customRightElbow;
	SerializedProperty customRightHand;
	SerializedProperty customRightHip;
	SerializedProperty customRightKnee;
	SerializedProperty customRightFoot;
	SerializedProperty customLeftClavicle;
	SerializedProperty customLeftShoulder;
	SerializedProperty customLeftElbow;
	SerializedProperty customLeftHand;
	SerializedProperty customLeftHip;
	SerializedProperty customLeftKnee;
	SerializedProperty customLeftFoot;
	SerializedProperty customLeftThumb;
	SerializedProperty customRightThumb;

	SerializedProperty customConversionType;

	GUIStyle customLabelStyle = new GUIStyle();
	GUIStyle customItalicLabelStyle = new GUIStyle();
	Color normalLabelColor;
	Color customLabelColor = new Color(0.0f, 0.5f, 0.8f);
	GUIStyle italicLabelStyle = new GUIStyle();

	RUISSkeletonController skeletonController;
	Animator animator;
	string missedBones = "";
	string obtainedTransforms = "";

    public void OnEnable()
    {
		customLabelStyle.fontStyle = FontStyle.Bold;
		customLabelStyle.normal.textColor = customLabelColor;
		customItalicLabelStyle.fontStyle = FontStyle.BoldAndItalic;
		customItalicLabelStyle.normal.textColor = customLabelColor;
		italicLabelStyle.fontStyle = FontStyle.BoldAndItalic;

		bodyTrackingDevice = serializedObject.FindProperty("bodyTrackingDevice");
		playerId = serializedObject.FindProperty("playerId");
		switchToAvailableKinect = serializedObject.FindProperty("switchToAvailableKinect");

        useHierarchicalModel = serializedObject.FindProperty("useHierarchicalModel");

        updateRootPosition = serializedObject.FindProperty("updateRootPosition");
        updateJointPositions = serializedObject.FindProperty("updateJointPositions");
        updateJointRotations = serializedObject.FindProperty("updateJointRotations");
		
		rootSpeedScaling = serializedObject.FindProperty("rootSpeedScaling");
		rootOffset = serializedObject.FindProperty("rootOffset");
		hmdRotatesHead = serializedObject.FindProperty("hmdRotatesHead");
		hmdMovesHead = serializedObject.FindProperty("hmdMovesHead");

		scaleHierarchicalModelBones = serializedObject.FindProperty("scaleHierarchicalModelBones");
		scaleBoneLengthOnly = serializedObject.FindProperty("scaleBoneLengthOnly");
		boneLengthAxis = serializedObject.FindProperty("boneLengthAxis");
		torsoThickness = serializedObject.FindProperty("torsoThickness");
		rightArmThickness = serializedObject.FindProperty("rightArmThickness");
		leftArmThickness = serializedObject.FindProperty("leftArmThickness");
		rightLegThickness = serializedObject.FindProperty("rightLegThickness");
		leftLegThickness = serializedObject.FindProperty("leftLegThickness");

		filterRotations = serializedObject.FindProperty("filterRotations");
		rotationNoiseCovariance = serializedObject.FindProperty("rotationNoiseCovariance");
		thumbZRotationOffset = serializedObject.FindProperty("thumbZRotationOffset");

		filterPosition = serializedObject.FindProperty("filterPosition");
		positionNoiseCovariance = serializedObject.FindProperty("positionNoiseCovariance");

        rootBone 	= serializedObject.FindProperty("root");
        headBone 	= serializedObject.FindProperty("head");
        neckBone 	= serializedObject.FindProperty("neck");
		torsoBone 	= serializedObject.FindProperty("torso");
		chestBone 	= serializedObject.FindProperty("chest");

		leftClavicle		= serializedObject.FindProperty("leftClavicle");
        leftShoulderBone 	= serializedObject.FindProperty("leftShoulder");
        leftElbowBone		= serializedObject.FindProperty("leftElbow");
        leftHandBone 		= serializedObject.FindProperty("leftHand");
		rightClavicle 		= serializedObject.FindProperty("rightClavicle");
        rightShoulderBone 	= serializedObject.FindProperty("rightShoulder");
        rightElbowBone 		= serializedObject.FindProperty("rightElbow");
        rightHandBone 		= serializedObject.FindProperty("rightHand");

        leftHipBone 	= serializedObject.FindProperty("leftHip");
        leftKneeBone 	= serializedObject.FindProperty("leftKnee");
        leftFootBone 	= serializedObject.FindProperty("leftFoot");
        rightHipBone 	= serializedObject.FindProperty("rightHip");
        rightKneeBone 	= serializedObject.FindProperty("rightKnee");
        rightFootBone 	= serializedObject.FindProperty("rightFoot");

		leftThumb = serializedObject.FindProperty ("leftThumb");
		rightThumb = serializedObject.FindProperty ("rightThumb");
		trackWrist = serializedObject.FindProperty ("trackWrist");
		trackAnkle = serializedObject.FindProperty ("trackAnkle");
//		rotateWristFromElbow = serializedObject.FindProperty ("rotateWristFromElbow");
		
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
		
		customRoot		= serializedObject.FindProperty("customRoot");
		customTorso  	= serializedObject.FindProperty("customTorso");
		customChest		= serializedObject.FindProperty("customChest");
		customNeck		= serializedObject.FindProperty("customNeck");
		customHead		= serializedObject.FindProperty("customHead");

		customRightClavicle = serializedObject.FindProperty("customRightClavicle");
		customRightShoulder = serializedObject.FindProperty("customRightShoulder");
		customRightElbow  	= serializedObject.FindProperty("customRightElbow");
		customRightHand  	= serializedObject.FindProperty("customRightHand");
		customRightHip  	= serializedObject.FindProperty("customRightHip");
		customRightKnee  	= serializedObject.FindProperty("customRightKnee");
		customRightFoot  	= serializedObject.FindProperty("customRightFoot");
		customRightThumb  	= serializedObject.FindProperty("customRightThumb");

		customLeftClavicle	= serializedObject.FindProperty("customLeftClavicle");
		customLeftShoulder	= serializedObject.FindProperty("customLeftShoulder");
		customLeftElbow		= serializedObject.FindProperty("customLeftElbow");
		customLeftHand		= serializedObject.FindProperty("customLeftHand");
		customLeftHip 		= serializedObject.FindProperty("customLeftHip");
		customLeftKnee 		= serializedObject.FindProperty("customLeftKnee");
		customLeftFoot 		= serializedObject.FindProperty("customLeftFoot");
		customLeftThumb 	= serializedObject.FindProperty("customLeftThumb");

		customConversionType = serializedObject.FindProperty("customConversionType");

		pelvisOffset 	= serializedObject.FindProperty("pelvisOffset");
		chestOffset 	= serializedObject.FindProperty("chestOffset");
		neckOffset 		= serializedObject.FindProperty("neckOffset");
		headOffset 		= serializedObject.FindProperty("headOffset");
		clavicleOffset 	= serializedObject.FindProperty("clavicleOffset");
		shoulderOffset  = serializedObject.FindProperty("shoulderOffset");
		hipOffset 		= serializedObject.FindProperty("hipOffset");

		feetRotationOffset = serializedObject.FindProperty("feetRotationOffset");

		pelvisScaleAdjust 	= serializedObject.FindProperty("pelvisScaleAdjust");
		chestScaleAdjust 	= serializedObject.FindProperty("chestScaleAdjust");
		neckScaleAdjust 	= serializedObject.FindProperty("neckScaleAdjust");
		headScaleAdjust 	= serializedObject.FindProperty("headScaleAdjust");
		clavicleScaleAdjust = serializedObject.FindProperty("clavicleScaleAdjust");
		handScaleAdjust		= serializedObject.FindProperty("handScaleAdjust");
		footScaleAdjust		= serializedObject.FindProperty("footScaleAdjust");

		skeletonController = target as RUISSkeletonController;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		EditorGUILayout.Space();
		 
		EditorGUILayout.PropertyField(bodyTrackingDevice, new GUIContent("Body Tracking Device", "The source device for body tracking.")); 

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(playerId, new GUIContent("Skeleton ID", "The skeleton ID number as reported by Kinect (i.e. 0 for the first "
														+ "person to be tracked, 1 for the second, and so on). When using "
														+ RUISSkeletonController.BodyTrackingDeviceType.GenericMotionTracker + ", increment this "
														+ "index for any additional person that you are concurrently tracking. This value needs "
														+ "to between 0 and " + (RUISSkeletonManager.maxTrackedSkeletons - 1) + "."));
		playerId.intValue = Mathf.Clamp(playerId.intValue, 0, RUISSkeletonManager.maxTrackedSkeletons - 1);

		switch(bodyTrackingDevice.enumValueIndex)
		{
			case RUISSkeletonManager.kinect1SensorID:
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
				playerId.intValue = Mathf.Clamp(playerId.intValue, 0, RUISSkeletonManager.kinect1HardwareLimit - 1);
				break;
			case RUISSkeletonManager.kinect2SensorID:
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
				break;
			case RUISSkeletonManager.customSensorID:
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
				break;
		}

		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect1SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID) 
		{
			EditorGUILayout.PropertyField(switchToAvailableKinect, new GUIContent(  "Switch To Available Kinect", "Examine RUIS InputManager settings, and "
			                                                                      + "switch Body Tracking Device from Kinect 1 to Kinect 2 in run-time if "
			                                                                      + "the latter is enabled but the former is not, and vice versa."));
        }
		
		RUISEditorUtility.HorizontalRuler();

        EditorGUILayout.PropertyField(useHierarchicalModel, new GUIContent(  "Hierarchical Model", "Is the model rig hierarchical (a tree) "
		                                                                   + "instead of non-hierarchical (all bones are on same level)? "
																		   + "in almost all cases this option should be enabled."));

        EditorGUILayout.PropertyField(updateRootPosition, new GUIContent(  "Update Root Position", "Update the position of this GameObject according "
		                                                                 + "to the skeleton root position"));

		GUI.enabled = updateRootPosition.boolValue;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(rootSpeedScaling, new GUIContent(  "Root Speed Scaling", "Multiply skeleton root position, making the avatar move "
		                                                               + "larger distances than mocap tracking area allows. This is not propagated to "
		                                                               + "Skeleton Wands or other devices (e.g. head trackers) even if they are calibrated "
		                                                               + "with mocap system's coordinate frame. Default and recommended value is (1,1,1)."));

		EditorGUILayout.PropertyField(rootOffset, new GUIContent(  "HMD Root Offset", "This offset is applied only when the \"Body Tracking Device\" is not available, "
																 + "and the avatar follows the head-mounted display position. The offset is useful if your "
																 + "view in those situations appears to be in a wrong place inside the avatar 3D model "));
		
		EditorGUI.indentLevel--;

        GUI.enabled = !useHierarchicalModel.boolValue;
		EditorGUILayout.PropertyField(updateJointPositions, new GUIContent(  "Update Joint Positions", "If \"Hierarchical Model\" is enabled, "
																			+ "Joint Positions are always updated implicitly through "
																			+ "Joint Rotations (and scales if \"Scale Bones\" is enabled)."));
		
        if(useHierarchicalModel.boolValue)
			updateJointPositions.boolValue = true;

		GUI.enabled = true;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(filterPosition, new GUIContent(  "Filter Positions",   "Smoothen the root, shoulder, and hip positions with "
															+ "a basic Kalman filter. Enabling this option is especially important when using "
															+ "Kinect. Disable this option when using more accurate and responsive mocap systems."));
		if(filterPosition.boolValue)
		{
			EditorGUILayout.PropertyField(positionNoiseCovariance, new GUIContent("Position Smoothness", "Sets the magnitude of position smoothing "
																				+ "(measurement noise variance). Larger values makes the movement "
																				+ "smoother, at the expense of responsiveness. Default value is 100."));
			positionNoiseCovariance.floatValue = Mathf.Clamp(positionNoiseCovariance.floatValue, 0.1f, float.MaxValue);

			// HACK: fourJointsNoiseCovariance is usually half of positionNoiseCovariance, but never less than 100 units away
			skeletonController.fourJointsNoiseCovariance = Mathf.Max(0.5f*positionNoiseCovariance.floatValue, positionNoiseCovariance.floatValue - 100);
		}

		EditorGUILayout.PropertyField(hmdMovesHead, new GUIContent("HMD Moves Head", "Make avatar head follow the position tracking of the connected " 
														+ "head-mounted display. NOTE: The " + skeletonController.bodyTrackingDevice 
														+ " coordinate system must be calibrated with the HMD position tracking coordinate system!"));

		EditorGUI.indentLevel--;

		GUI.enabled = true;
        EditorGUILayout.PropertyField(updateJointRotations, new GUIContent(  "Update Joint Rotations", "Enabling this option is especially "
		                                                                   + "important for hierarchical models when using Kinect. Disable this "
																		   + "option when using more accurate and responsive mocap systems."));

		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(filterRotations, new GUIContent(  "Filter Rotations",   "Smoothen rotations with a basic Kalman filter. For now this is "
		                                                              						+ "only done for the arm joints of Kinect 2 tracked skeletons."));
		if(filterRotations.boolValue)
		{
			EditorGUILayout.PropertyField(rotationNoiseCovariance, new GUIContent("Rotation Smoothness", "Sets the magnitude of rotation smoothing "
																+ "(measurement noise variance). Larger values make the rotation smoother, "
																+ "at the expense of responsiveness. Default value for Kinect is 500. Use smaller "
																+ "values for more accurate and responsive mocap systems."));
		}

		if(   Application.isEditor && skeletonController && skeletonController.skeletonManager 
		   && skeletonController.skeletonManager.skeletons [skeletonController.BodyTrackingDeviceID, skeletonController.playerId] != null)
		{
			skeletonController.skeletonManager.skeletons [skeletonController.BodyTrackingDeviceID, skeletonController.playerId].filterRotations = filterRotations.boolValue;
			skeletonController.skeletonManager.skeletons [skeletonController.BodyTrackingDeviceID, skeletonController.playerId].rotationNoiseCovariance = 
																																rotationNoiseCovariance.floatValue;
		}

		EditorGUILayout.PropertyField(hmdRotatesHead, new GUIContent("HMD Rotates Head", "Rotate avatar head using orientation from the connected "
														+ "head-mounted display.NOTE: The " + skeletonController.bodyTrackingDevice 
														+ " coordinate system must be calibrated or otherwise aligned with the HMD's coordinate system!"));

		EditorGUI.indentLevel--;

        GUI.enabled = useHierarchicalModel.boolValue;
        EditorGUILayout.PropertyField(scaleHierarchicalModelBones, new GUIContent(  "Scale Bones", "Scale the bones of the model based on the "
		                                                                          + "real-life limb lengths of the tracked person, making the "
																				  + "model size correspond to the tracked person size. This "
																				  + "option is only available for hierarchical models."));

		GUI.enabled = scaleHierarchicalModelBones.boolValue;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(boneLengthAxis, new GUIContent(  "Bone Length Axis", "Determines the axis that points the bone direction in each " 
		                                                             + "joint transform of the animation rig. This value depends on your rig, and it is "
		                                                             + "only used if you have \"Scale Length Only\" enabled or you are using Kinect 2 to "
		                                                             + "curl fingers (fist clenching). You can discover the correct axis by examining "
		                                                             + "the animation rig hierarchy, by looking at the directional axis between parent "
		                                                             + "joints and their child joints in local coordinate system. IMPORTANT: Disable the "
		                                                             + "below \"Scale Length Only\" option if the same localScale axis is not consistently "
		                                                             + "used in all the joints of the animation rig."));
		EditorGUILayout.PropertyField(scaleBoneLengthOnly, new GUIContent(  "Scale Length Only", "Scale the bone length (localScale.x/y/z) but not the "
		                                                                  + "bone thickness (localScale.yz/xz/xy). WARNING: Enabling this option could "
		                                                                  + "lead to peculiar results, depending on the animation rig. At the very least "
																		  + "it leads to non-uniform scaling, for which there are slight mitigations in "
																		  + "the code."));

		if(scaleBoneLengthOnly.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(torsoThickness, new GUIContent(  "Torso Thickness", "Thickness scale for torso around its Length Axis."));
			EditorGUILayout.PropertyField(rightArmThickness, new GUIContent(  "Right Arm Thickness", "Thickness scale for right arm around its Length Axis."));
			EditorGUILayout.PropertyField(leftArmThickness,  new GUIContent(  "Left Arm Thickness", "Thickness scale for left arm around its Length Axis."));
			EditorGUILayout.PropertyField(rightLegThickness, new GUIContent(  "Right Leg Thickness", "Thickness scale for right leg around its Length Axis."));
			EditorGUILayout.PropertyField(leftLegThickness,  new GUIContent(  "Left Leg Thickness", "Thickness scale for left leg around its Length Axis."));
			EditorGUI.indentLevel--;
		}

		EditorGUI.indentLevel--;

        if(!useHierarchicalModel.boolValue)
		{
			scaleHierarchicalModelBones.boolValue = false;
		}
		if(!scaleHierarchicalModelBones.boolValue)
			scaleBoneLengthOnly.boolValue = false;

        GUI.enabled = true;

		EditorGUILayout.Space();
		
		
		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID) 
		{	
			RUISEditorUtility.HorizontalRuler();

			EditorGUILayout.LabelField("  Custom Mocap Source Transforms", customItalicLabelStyle);

			normalLabelColor = EditorStyles.label.normal.textColor;
			EditorStyles.label.normal.textColor = customLabelColor;

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(customConversionType, new GUIContent("Coordinate Conversion", "The conversion that will be applied to the "
																		+ "Source Transform poses before copying them to Target Transforms (below). "
																		+ "The conversions are defined in " + typeof(RUISInputManager)
																		+ "'s \"Custom 1\" and \"Custom 2\" settings, and also stored in the "
																		+ "associated 'inputConfig.xml'-file."));

			EditorGUILayout.PropertyField(customRoot, new GUIContent("Source Root", "The source Transform for the skeleton hierarchy's root bone. "
																	+ "The source Transforms of this section should be moved by realtime input from "
																	+ "a custom mocap system.\nIf you want this avatar to copy "
																	+ "the pose of another " + typeof(RUISSkeletonController) + " that has the "
																	+ "Custom Source fields already set, then you only need to use the same "
																	+ "\"Skeleton ID\" and you can leave the below Custom Source fields empty."));
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Torso and Head", customLabelStyle);
			EditorGUILayout.PropertyField(customTorso, 	 new GUIContent("Pelvis", 	"The pelvis bone, has to be parent or grandparent of all the "
																				  + "other bones except root bone. Can be same as root bone."));
			EditorGUILayout.PropertyField(customChest, 	 new GUIContent("Chest", 	"The chest bone, has to be child or grandchild of pelvis."));
			EditorGUILayout.PropertyField(customNeck, 	 new GUIContent("Neck", 	"The neck bone, has to be child or grandchild of chest."));
			EditorGUILayout.PropertyField(customHead, 	 new GUIContent("Head", 	"The head bone, has to be child or grandchild of neck."));
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Arms", customLabelStyle);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));

			EditorGUILayout.PropertyField(customLeftClavicle, 	new GUIContent("Left Clavicle",   "The left clavicle bone, "
			                                                                   					+ "has to be child of neck."));
			EditorGUILayout.PropertyField(customLeftShoulder, 	new GUIContent("Left Shoulder",   "The left shoulder bone (upper arm), "
																								+ "has to be child or grandchild of neck."));
			EditorGUILayout.PropertyField(customLeftElbow, 	  	new GUIContent("Left Elbow",	  "The left elbow bone (forearm), "
																								+ "has to be child of left shoulder."));
			EditorGUILayout.PropertyField(customLeftHand, 		new GUIContent("Left Hand", 	  "The left wrist bone (hand), "
																								+ "has to be child of left elbow."));
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightClavicle, 	new GUIContent("Right Clavicle",  "The right clavicle bone, "
			                                                                   					+ "has to be child of neck."));
			EditorGUILayout.PropertyField(customRightShoulder,	new GUIContent("Right Shoulder",  "The right shoulder bone (upper arm), "
																								+ "has to be child or grandchild of neck."));
			EditorGUILayout.PropertyField(customRightElbow, 	new GUIContent("Right Elbow", 	  "The right elbow bone (forearm), "
																								+ "has to be child of right shoulder."));
			EditorGUILayout.PropertyField(customRightHand,		new GUIContent("Right Hand",	  "The right wrist bone (hand), "
																								+ "has to be child of right elbow."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Legs", customLabelStyle);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customLeftHip,  new GUIContent("Left Hip",  "The left hip bone (thigh), "
																					+ "has to be child or grandchild of pelvis."));
			EditorGUILayout.PropertyField(customLeftKnee, new GUIContent("Left Knee", "The left knee bone (shin), "
																					+ "has to be child of left hip."));
			EditorGUILayout.PropertyField(customLeftFoot, new GUIContent("Left Foot", "The left ankle bone (foot), "
																					+ "has to be child of left knee."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightHip,  new GUIContent("Right Hip",	"The right hip bone (thigh), "
																					  + "has to be child or grandchild of pelvis."));
			EditorGUILayout.PropertyField(customRightKnee, new GUIContent("Right Knee", "The right knee bone (shin), "
																					  + "has to be child of right hip."));
			EditorGUILayout.PropertyField(customRightFoot, new GUIContent("Right Foot", "The right ankle bone (foot), "
																					  + "has to be child of right knee."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Fingers", customLabelStyle);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customLeftThumb, new GUIContent("Left Thumb",   "The left thumb, has to be child of left hand."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightThumb, new GUIContent("Right Thumb", "The right thumb, has to be child of right hand."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			EditorStyles.label.normal.textColor = normalLabelColor;

		}

		RUISEditorUtility.HorizontalRuler();

		EditorGUILayout.LabelField("  Avatar Target Transforms", italicLabelStyle);

		EditorGUILayout.Space();
		string mocapSource = "Kinect.";
		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
			mocapSource = "the above \"Custom Mocap Source Transforms\".";

		if(GUILayout.Button(new GUIContent("Obtain Targets from Animator", "Attempt to automatically obtain below Target Transforms "
											+ "from an Animator component, if such a component can be found from this GameObject "
											+ "or its children. WARNING: Make sure that the Transforms are correct!")))
		{
			if(skeletonController)
			{
				animator = skeletonController.GetComponentInChildren<Animator>();
				if(animator)
				{
					rootBone.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.LastBone); //
					torsoBone.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.Hips);
					chestBone.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.Spine);
					neckBone.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.Neck); //
					headBone.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.Head);
					leftClavicle.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
					leftShoulderBone.objectReferenceValue 	= animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
					leftElbowBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
					leftHandBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftHand);
					rightClavicle.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightShoulder);
					rightShoulderBone.objectReferenceValue 	= animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
					rightElbowBone.objectReferenceValue 	= animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
					rightHandBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightHand);
					leftHipBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
					leftKneeBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
					leftFootBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.LeftFoot);
					rightHipBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
					rightKneeBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
					rightFootBone.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightFoot);
					leftThumb.objectReferenceValue 			= animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
					rightThumb.objectReferenceValue 		= animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);

					if(		rootBone.objectReferenceValue && ((Transform) rootBone.objectReferenceValue).IsChildOf(animator.transform)
						&& 	animator.transform != ((Transform) rootBone.objectReferenceValue))
					{
						// Iterate until rootBone is direct child of animator.transform
						Transform child = (Transform) rootBone.objectReferenceValue;
						for(int i=0; i<100; ++i)
						{
							if(child.parent == animator.transform)
							{
								rootBone.objectReferenceValue = child;
								break;
							}
							if(i==99)
							{
								rootBone.objectReferenceValue = null;
								break;
							}
							child = child.parent;
						}
					}

					if(!neckBone.objectReferenceValue && headBone.objectReferenceValue)
						neckBone.objectReferenceValue = ((Transform) headBone.objectReferenceValue).parent;

					if(!rootBone.objectReferenceValue)
						missedBones += "Root, ";
					if(!torsoBone.objectReferenceValue)
						missedBones += "Pelvis, ";
					if(!chestBone.objectReferenceValue)
						missedBones += "Chest, ";
					if(!neckBone.objectReferenceValue)
						missedBones += "Neck, ";
					if(!headBone.objectReferenceValue)
						missedBones += "Head, ";
					if(!leftClavicle.objectReferenceValue)
						missedBones += "Left Clavicle, ";
					if(!leftShoulderBone.objectReferenceValue)
						missedBones += "Left Shoulder, ";
					if(!leftElbowBone.objectReferenceValue)
						missedBones += "Left Elbow, ";
					if(!leftHandBone.objectReferenceValue)
						missedBones += "Left Hand, ";
					if(!rightClavicle.objectReferenceValue)
						missedBones += "Right Clavicle, ";
					if(!rightShoulderBone.objectReferenceValue)
						missedBones += "Right Shoulder, ";
					if(!rightElbowBone.objectReferenceValue)
						missedBones += "Right Elbow, ";
					if(!rightHandBone.objectReferenceValue)
						missedBones += "Right Hand, ";
					if(!leftHipBone.objectReferenceValue)
						missedBones += "Left Hip, ";
					if(!leftKneeBone.objectReferenceValue)
						missedBones += "Left Knee, ";
					if(!leftFootBone.objectReferenceValue)
						missedBones += "Left Foot, ";
					if(!leftThumb.objectReferenceValue)
						missedBones += "Left Thumb, ";
					if(!rightThumb.objectReferenceValue)
						missedBones += "Right Thumb, ";

					EditorStyles.textField.wordWrap = true;
					if(!string.IsNullOrEmpty(missedBones))
					{
						missedBones = missedBones.Substring(0, missedBones.Length - 2);
						Debug.LogWarning("Obtained some Target Transforms from Animator component of '" + animator.gameObject.name 
										+ "' GameObject. The following Transforms were NOT obtained: " + missedBones + ". Please check that the "
										+ "automatically obtained Transforms correspond to the semantic labels by clicking the below Target "
										+ "Transform fields.");

						obtainedTransforms =   "Obtained some Target Transforms but not all. Please check that they correspond to the semantic "
											 + "labels by clicking the below target Transform fields.";
					}
					else
					{
						Debug.LogWarning("Obtained all Target Transforms from Animator component of '" + animator.gameObject.name 
							+ "' GameObject. Please check that the automatically obtained Transforms correspond to the semantic labels by clicking "
							+ "the below Target Transform fields.");

						obtainedTransforms =   "Obtained Target Transforms. Please check that they correspond to the semantic labels by clicking "
											 + "the below target Transform fields.";
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Failed to obtain Targets", "Could not find an Animator component in this GameObject " 
												+ "or its children. You must assign the Avatar Target Transforms manually.", "OK");
					obtainedTransforms = "";
				}
			}
			
		}

		if(!string.IsNullOrEmpty(obtainedTransforms))
		{
			GUI.enabled = false;
			EditorGUILayout.TextArea(obtainedTransforms);
			GUI.enabled = true;
		}

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(rootBone, new GUIContent("Target Root", "The target Transform that is the animated avatar's root bone in "
																			+ "the skeleton hierarchy. The target Transforms of this section will "
																			+ "be moved by " + mocapSource));
		EditorGUILayout.Space();

        EditorGUILayout.LabelField("Torso and Head Targets", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(torsoBone, 	new GUIContent("Pelvis", 		"The pelvis bone, has to be parent or grandparent of all the "
																				  + "other bones except root bone. Can be same as root bone."));
		EditorGUILayout.PropertyField(chestBone, 	new GUIContent("Chest", 		"The chest bone, has to be child or grandchild of pelvis. "
																				  + "Can be 'None'."));
		EditorGUILayout.PropertyField(neckBone, 	new GUIContent("Neck", 			"The neck bone, has to be child or grandchild of chest."));
		EditorGUILayout.PropertyField(headBone, 	new GUIContent("Head", 			"The head bone, has to be child or grandchild of neck."));

		EditorGUILayout.Space();
		
		EditorGUILayout.LabelField("Arm Targets", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = Screen.width / 6;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(leftClavicle, 	 new GUIContent("Left Clavicle",   "The left clavicle bone, "
		                                                                       			 + "has to be child of neck."));
		EditorGUILayout.PropertyField(leftShoulderBone,  new GUIContent("Left Shoulder",   "The left shoulder bone (upper arm), "
																						 + "has to be child or grandchild of neck."));
		EditorGUILayout.PropertyField(leftElbowBone, 	 new GUIContent("Left Elbow", 	   "The left elbow bone (forearm), "
																						 + "has to be child of left shoulder."));
		EditorGUILayout.PropertyField(leftHandBone, 	 new GUIContent("Left Hand", 	   "The left wrist bone (hand), "
																						 + "has to be child of left elbow."));

		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(rightClavicle, 	 new GUIContent("Right Clavicle",   "The right clavicle bone, "
		                                                            					  + "has to be child of neck."));
		EditorGUILayout.PropertyField(rightShoulderBone, new GUIContent("Right Shoulder",	"The right shoulder bone (upper arm), "
																						  + "has to be child or grandchild of neck."));
		EditorGUILayout.PropertyField(rightElbowBone, 	 new GUIContent("Right Elbow",		"The right elbow bone (forearm), "
																						  + "has to be child of right shoulder."));
		EditorGUILayout.PropertyField(rightHandBone,	 new GUIContent("Right Hand",		"The right wrist bone (hand), "
																						  + "has to be child of right elbow."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
			EditorGUILayout.PropertyField(trackWrist, new GUIContent("Track Wrist Rotation", "Track the rotation of the hand bone"));

		// TODO: Restore this when implementation is fixed
//		if (bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID)
//			EditorGUILayout.PropertyField(rotateWristFromElbow, new GUIContent("Wrist Rotates Lower Arm", "Should wrist rotate whole lower arm or just the hand?"));

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Leg Targets", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = Screen.width / 6;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(leftHipBone,   new GUIContent("Left Hip",	   "The left hip bone (thigh), "
																				 + "has to be child or grandchild of pelvis."));
		EditorGUILayout.PropertyField(leftKneeBone,  new GUIContent("Left Knee",   "The left knee bone (shin), has to be child of left hip."));
		EditorGUILayout.PropertyField(leftFootBone,  new GUIContent("Left Foot",   "The left ankle bone (foot), has to be child of left knee."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(rightHipBone,  new GUIContent("Right Hip", 	"The right hip bone (thigh), "
																				  + "has to be child or grandchild of pelvis."));
		EditorGUILayout.PropertyField(rightKneeBone, new GUIContent("Right Knee", 	"The right knee bone (shin), has to be child of right hip."));
		EditorGUILayout.PropertyField(rightFootBone, new GUIContent("Right Foot", 	"The right ankle bone (foot), has to be child of right knee."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

		if (bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
			EditorGUILayout.PropertyField(trackAnkle, new GUIContent("Track Ankle Rotation", "Track the rotation of the ankle bone"));
		
		EditorGUILayout.Space();

		if (bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID) 
		{
			EditorGUILayout.LabelField("Finger Targets", EditorStyles.boldLabel);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(leftThumb, new GUIContent ("Left Thumb",		"The left thumb, has to be child of left hand."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(rightThumb, new GUIContent ("Right Thumb",	"The right thumb, has to be child of right hand."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID)
			{
				EditorGUILayout.PropertyField(fistCurlFingers, new GUIContent(  "Track Fist Clenching", "When user is making a fist, curl finger joints "
				                                                              + "(child gameObjects under 'Left Hand' and 'Right Hand' whose name include "
				                                                              + "the substring 'finger' or 'Finger'.). If you have assigned 'Left Thumb' " +
				                                                              	"and 'Right Thumb', they will receive a slightly different finger curling."));
				EditorGUILayout.PropertyField(trackThumbs, new GUIContent("Track Thumbs", "Track thumb movement."));
				if(trackThumbs.boolValue)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(thumbZRotationOffset, new GUIContent("Z Rotation Offset",   "Offset Z rotation of the thumb. Default value is "
						+ "45, but it might depend on your avatar rig."));
					EditorGUI.indentLevel--;
					if(   Application.isEditor && skeletonController && skeletonController.skeletonManager 
						&& skeletonController.skeletonManager.skeletons [skeletonController.BodyTrackingDeviceID, skeletonController.playerId] != null)
						skeletonController.skeletonManager.skeletons [skeletonController.BodyTrackingDeviceID, skeletonController.playerId].thumbZRotationOffset = 
							thumbZRotationOffset.floatValue;
				}
			}

		}

		EditorGUILayout.Space();

		RUISEditorUtility.HorizontalRuler();
		
		EditorGUILayout.LabelField("  Adjustments", italicLabelStyle);
        EditorGUILayout.PropertyField(rotationDamping, new GUIContent(  "Max Joint Angular Velocity", "Maximum joint angular velocity can be used "
		                                                              + "for damping avatar bone movement (smaller values). The values are in "
																	  + "degrees. For Kinect and similar devices, a value of 360 is suitable. For "
																	  + "more accurate and responsive mocap systems, this value can easily be set "
																	  + "to 7200 or more, so that very fast motions are not restricted."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.PropertyField(maxScaleFactor, new GUIContent(  "Max Scale Rate", "The maximum amount the scale of a bone can "
		                                                             + "change per second when using \"Hierarchical Model\" and \"Scale Bones\""));

		GUI.enabled = true;
		EditorGUILayout.PropertyField(minimumConfidenceToUpdate, new GUIContent(  "Min Confidence to Update", "The minimum confidence in joint "
			+ "positions and rotations needed to update these values. "
			+ "The confidence is either 0; 0,5; or 1. This setting is only "
			+ "relevant with Kinect tracking."));

		EditorGUILayout.Space();

		GUI.enabled = scaleHierarchicalModelBones.boolValue;

		// *** OPTIHACK
		EditorGUILayout.PropertyField(pelvisOffset, new GUIContent("Pelvis Offset",   "Offsets pelvis joint position in its local frame. WARNING: "
																					+ "This also offsets the absolute positions of all spine and clavicle "
						                                                            + "joints! The offset is relative to the scale of pelvis joint. This setting "
						                                                            + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(pelvisScaleAdjust, 0.01f, 3, new GUIContent("Pelvis Scale Adjust",   "Scales pelvis. This setting has effect only when \"Hierarchical "
		                                                                   + "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.PropertyField(chestOffset, new GUIContent("Chest Offset",   "Offsets chest joint position in its local frame. "
		                                                          + "WARNING: This also offsets the absolute positions of neck, head and "
		                                                          + "clavicle joints! The offset is relative to the scale of pelvis joint. This setting "
		                                                          + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(chestScaleAdjust, 0.01f, 3, new GUIContent("Chest Scale Adjust",   "Scales chest. This setting has effect only when \"Hierarchical "
		                                                                + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(neckOffset, new GUIContent("Neck Offset",   "Offsets neck joint position in its local frame. "
		                                                         + "WARNING: This also offsets the absolute positions of head and "
		                                                         + "clavicle joints! The offset is relative to the scale of chest joint. This setting "
		                                                         + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(neckScaleAdjust, 0.01f, 3, new GUIContent("Neck Scale Adjust",   "Scales neck. This setting has effect only when \"Hierarchical "
		                                                               + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(headOffset, new GUIContent("Head Offset",   "Offsets head joint position in its local frame. "
		                                                         + "The offset is relative to the scale of neck joint. This setting has effect only when \"Hierarchical "
		                                                         + "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(headScaleAdjust, 0.01f, 3, new GUIContent("Head Scale Adjust",   "Scales head. This setting has effect only when \"Hierarchical "
		                                                              + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(clavicleOffset, new GUIContent("Clavicle Offset",   "Offsets clavicle joint positions in their "
		                                                             + "local frame. The offset is relative to the scale of neck joint. This setting has effect only when "
		                                                             + "\"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(clavicleScaleAdjust, 0.01f, 3, new GUIContent("Clavice Scale Adjust",   "Scales clavicles. This setting has effect only when \"Hierarchical "
		                                                              + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(shoulderOffset, new GUIContent("Shoulder Offset",   "Offsets shoulder joint positions in their local frame. "
																						+ "WARNING: This also offsets the absolute positions of all the "
																						+ "arm joints! Offset is relative to the scale of parent joint. This setting "
		                                                   						        + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));
		// *** OPTIHACK
		EditorGUILayout.PropertyField(hipOffset, new GUIContent("Hip Offset",	"Offsets hip joint positions in their local frame. WARNING: This also "
					                                                          + "offsets the absolute positions of all the leg joints! Offset is relative to the "
					                                                          + "scale of pelvis joint. This setting has effect only when \"Hierarchical Model\" and " 
																			  + "\"Scale Bones\" are enabled."));

		EditorGUILayout.Space();

		EditorGUILayout.Slider(handScaleAdjust, 0.01f, 3, new GUIContent("Hand Scale Adjust", "Scales hands. This setting has effect only when \"Hierarchical "
																		+ "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(footScaleAdjust, 0.01f, 3, new GUIContent("Foot Scale Adjust", "Scales feet. This setting has effect only when \"Hierarchical "
																		+ "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.PropertyField(feetRotationOffset, new GUIContent("Foot Rotation Offset", "Offsets the joint rotations of both feet in their local frame."));
		

		EditorGUILayout.Space();

		// *** OPTIHACK Consider removing adjustVerticalHipsPosition because pelvisOffset does the same thing and more
//		EditorGUILayout.PropertyField(adjustVerticalHipsPosition, new GUIContent(  "Hips Vertical Tweaker", "Offset the tracked hip center point "
//		                                                                         + "position in the spine direction (usually vertical axis) when "
//		                                                                         + "\"Hierarchical Model\" and \"Scale Bones\" are enabled."));
		// *** TODO remove this from RUISSkeletonController
//		EditorGUILayout.PropertyField(neckHeightTweaker, new GUIContent(  "Neck Height Tweaker", "Offset the tracked neck position in the spine "
//		                                                                + "direction (usually vertical axis) when \"Hierarchical Model\" and "
//		                                                                + "\"Scale Bones\" are enabled."));

		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.Slider(forearmLengthTweaker, 0.01f, 3, new GUIContent(   "Forearm Scale Adjust", "The forearm length ratio "
			                                                                   + "compared to the real-world value, use this to lengthen or "
			                                                                   + "shorten the forearms. Only used if \"Hierarchical Model\" is enabled"));
		EditorGUILayout.Slider(shinLengthTweaker, 0.01f, 3, new GUIContent(   "Shin Scale Adjust", "The shin length ratio compared to the "
			                                                                + "real-world value, use this to lengthen or shorten the "
			                                                                + "shins. Only used if \"Hierarchical Model\" is enabled"));
		EditorGUILayout.Space();
		
		GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
}
