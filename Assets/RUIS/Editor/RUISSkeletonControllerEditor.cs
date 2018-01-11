/*****************************************************************************

Content    :   Inspector behaviour for RUISSkeletonController script
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using System;
using System.Reflection;
#endif

[CustomEditor(typeof(RUISSkeletonController))]
[CanEditMultipleObjects]
public class RUISSkeletonControllerEditor : Editor
{
	Color customLabelColor   = new Color(0.0f, 0.5f, 0.8f);
	Color keepPlayModeChangesRGB = new Color(1.0f, 0.9f, 0.5f);

	// Below those SerializedProperty fields that are marked with public can be kept (saved) after exiting play mode

	SerializedProperty bodyTrackingDevice;

	public SerializedProperty keepPlayModeChanges;
	public SerializedProperty playerId;

	SerializedProperty switchToAvailableKinect;
	SerializedProperty useHierarchicalModel;

	public SerializedProperty updateRootPosition;
	public SerializedProperty updateJointPositions;
	public SerializedProperty updateJointRotations;
	
	public SerializedProperty rootSpeedScaling;
	public SerializedProperty rootOffset;
	public SerializedProperty hmdRotatesHead;
	public SerializedProperty hmdMovesHead;
	public SerializedProperty hmdLocalOffset;

	public SerializedProperty scaleHierarchicalModelBones;
	public SerializedProperty scaleBoneLengthOnly;

	public SerializedProperty boneLengthAxis;

	public SerializedProperty limbsAreScaled;
	public SerializedProperty independentTorsoSegmentsScaling;
	public SerializedProperty heightAffectsOffsets;
	public SerializedProperty torsoThickness;
	public SerializedProperty rightArmThickness;
	public SerializedProperty leftArmThickness;
	public SerializedProperty rightLegThickness;
	public SerializedProperty leftLegThickness;

	public SerializedProperty filterPosition;
	public SerializedProperty filterHeadPositionOnly;
	public SerializedProperty positionNoiseCovariance;

	public SerializedProperty filterRotations;
	public SerializedProperty rotationNoiseCovariance;
	public SerializedProperty thumbZRotationOffset;

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
	SerializedProperty leftIndexF;
	SerializedProperty rightIndexF;
	SerializedProperty leftMiddleF;
	SerializedProperty rightMiddleF;
	SerializedProperty leftRingF;
	SerializedProperty rightRingF;
	SerializedProperty leftLittleF;
	SerializedProperty rightLittleF;

	public SerializedProperty maxScaleFactor;

	SerializedProperty minimumConfidenceToUpdate;

	public SerializedProperty rotationDamping;
	public SerializedProperty forearmLengthTweaker;
	public SerializedProperty shinLengthTweaker;

	public SerializedProperty fistCurlFingers;
	public SerializedProperty trackThumbs;
	public SerializedProperty trackWrist;
	public SerializedProperty trackAnkle;
//	SerializedProperty rotateWristFromElbow;

	public SerializedProperty pelvisOffset;
	public SerializedProperty chestOffset;
	public SerializedProperty neckOffset;
	public SerializedProperty headOffset;
	public SerializedProperty clavicleOffset;
	public SerializedProperty shoulderOffset;
	public SerializedProperty elbowOffset;
	public SerializedProperty handOffset;
	public SerializedProperty hipOffset;
	public SerializedProperty kneeOffset;
	public SerializedProperty footOffset;

	public SerializedProperty pelvisRotationOffset;
	public SerializedProperty chestRotationOffset;
	public SerializedProperty neckRotationOffset;
	public SerializedProperty headRotationOffset;
	public SerializedProperty clavicleRotationOffset;
	public SerializedProperty shoulderRotationOffset;
	public SerializedProperty elbowRotationOffset;
	public SerializedProperty handRotationOffset;
	public SerializedProperty hipRotationOffset;
	public SerializedProperty kneeRotationOffset;
	public SerializedProperty feetRotationOffset;

	public SerializedProperty pelvisScaleAdjust;
	public SerializedProperty chestScaleAdjust;
	public SerializedProperty neckScaleAdjust;
	public SerializedProperty headScaleAdjust;
	public SerializedProperty clavicleScaleAdjust;
	public SerializedProperty shoulderScaleAdjust;
	public SerializedProperty handScaleAdjust;
	public SerializedProperty hipScaleAdjust;
	public SerializedProperty footScaleAdjust;

	SerializedProperty isIMUMocap;

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

	SerializedProperty customRightThumb;
	SerializedProperty customRightIndexF;
	SerializedProperty customRightMiddleF;
	SerializedProperty customRightRingF;
	SerializedProperty customRightLittleF;

	SerializedProperty customLeftThumb;
	SerializedProperty customLeftIndexF;
	SerializedProperty customLeftMiddleF;
	SerializedProperty customLeftRingF;
	SerializedProperty customLeftLittleF;

	SerializedProperty customConversionType;

	GUIStyle coloredBoldStyle = new GUIStyle();
	GUIStyle coloredBoldItalicStyle = new GUIStyle();
	Color normalLabelColor;
	Color normalGUIColor;
	GUIStyle boldItalicStyle = new GUIStyle();
	GUIStyle boldFoldoutStyle = null;
	GUIStyle coloredBoldFoldoutStyle = null;

	RUISSkeletonController skeletonController;
	Animator animator;
	string missedBones = "";
	string obtainedTransforms = "";

	static bool showLocalOffsets;
	static bool showTargetFingers;
	static bool showSourceFingers;

	public void OnEnable()
	{
		coloredBoldStyle.fontStyle = FontStyle.Bold;
		coloredBoldStyle.normal.textColor = customLabelColor;
		coloredBoldItalicStyle.fontStyle = FontStyle.BoldAndItalic;
		coloredBoldItalicStyle.normal.textColor = customLabelColor;
		boldItalicStyle.fontStyle = FontStyle.BoldAndItalic;
		
		keepPlayModeChanges = serializedObject.FindProperty("keepPlayModeChanges");

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
		hmdLocalOffset = serializedObject.FindProperty("hmdLocalOffset");

		scaleHierarchicalModelBones = serializedObject.FindProperty("scaleHierarchicalModelBones");
		scaleBoneLengthOnly = serializedObject.FindProperty("scaleBoneLengthOnly");
		boneLengthAxis = serializedObject.FindProperty("boneLengthAxis");
		limbsAreScaled = serializedObject.FindProperty("limbsAreScaled");
		independentTorsoSegmentsScaling = serializedObject.FindProperty("independentTorsoSegmentsScaling");
		heightAffectsOffsets = serializedObject.FindProperty("heightAffectsOffsets");
		torsoThickness = serializedObject.FindProperty("torsoThickness");
		rightArmThickness = serializedObject.FindProperty("rightArmThickness");
		leftArmThickness = serializedObject.FindProperty("leftArmThickness");
		rightLegThickness = serializedObject.FindProperty("rightLegThickness");
		leftLegThickness = serializedObject.FindProperty("leftLegThickness");

		filterRotations = serializedObject.FindProperty("filterRotations");
		rotationNoiseCovariance = serializedObject.FindProperty("rotationNoiseCovariance");
		thumbZRotationOffset = serializedObject.FindProperty("thumbZRotationOffset");

		filterPosition = serializedObject.FindProperty("filterPosition");
		filterHeadPositionOnly = serializedObject.FindProperty("filterHeadPositionOnly");
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

		leftThumb 	 = serializedObject.FindProperty("leftThumb");
		rightThumb   = serializedObject.FindProperty("rightThumb");
		leftIndexF   = serializedObject.FindProperty("leftIndexF");
		rightIndexF  = serializedObject.FindProperty("rightIndexF");
		leftMiddleF  = serializedObject.FindProperty("leftMiddleF");
		rightMiddleF = serializedObject.FindProperty("rightMiddleF");
		leftRingF    = serializedObject.FindProperty("leftRingF");
		rightRingF   = serializedObject.FindProperty("rightRingF");
		leftLittleF  = serializedObject.FindProperty("leftLittleF");
		rightLittleF = serializedObject.FindProperty("rightLittleF");

		trackWrist = serializedObject.FindProperty ("trackWrist");
		trackAnkle = serializedObject.FindProperty ("trackAnkle");
//		rotateWristFromElbow = serializedObject.FindProperty ("rotateWristFromElbow");

        maxScaleFactor = serializedObject.FindProperty("maxScaleFactor");
        minimumConfidenceToUpdate = serializedObject.FindProperty("minimumConfidenceToUpdate");
        rotationDamping = serializedObject.FindProperty("rotationDamping");
        forearmLengthTweaker = serializedObject.FindProperty("forearmLengthRatio");
		shinLengthTweaker = serializedObject.FindProperty("shinLengthRatio");
		
		fistCurlFingers = serializedObject.FindProperty("fistCurlFingers");
		trackThumbs = serializedObject.FindProperty("trackThumbs");

		isIMUMocap = serializedObject.FindProperty("isIMUMocap");

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

		customLeftClavicle	= serializedObject.FindProperty("customLeftClavicle");
		customLeftShoulder	= serializedObject.FindProperty("customLeftShoulder");
		customLeftElbow		= serializedObject.FindProperty("customLeftElbow");
		customLeftHand		= serializedObject.FindProperty("customLeftHand");
		customLeftHip 		= serializedObject.FindProperty("customLeftHip");
		customLeftKnee 		= serializedObject.FindProperty("customLeftKnee");
		customLeftFoot 		= serializedObject.FindProperty("customLeftFoot");

		customRightThumb  	= serializedObject.FindProperty("customRightThumb");
		customRightIndexF	= serializedObject.FindProperty("customRightIndexF");
		customRightMiddleF	= serializedObject.FindProperty("customRightMiddleF");
		customRightRingF	= serializedObject.FindProperty("customRightRingF");
		customRightLittleF	= serializedObject.FindProperty("customRightLittleF");

		customLeftThumb 	= serializedObject.FindProperty("customLeftThumb");
		customLeftIndexF	= serializedObject.FindProperty("customLeftIndexF");
		customLeftMiddleF	= serializedObject.FindProperty("customLeftMiddleF");
		customLeftRingF		= serializedObject.FindProperty("customLeftRingF");
		customLeftLittleF	= serializedObject.FindProperty("customLeftLittleF");


		customConversionType = serializedObject.FindProperty("customConversionType");

		pelvisOffset 	= serializedObject.FindProperty("pelvisOffset");
		chestOffset 	= serializedObject.FindProperty("chestOffset");
		neckOffset 		= serializedObject.FindProperty("neckOffset");
		headOffset 		= serializedObject.FindProperty("headOffset");
		clavicleOffset 	= serializedObject.FindProperty("clavicleOffset");
		shoulderOffset  = serializedObject.FindProperty("shoulderOffset");
		elbowOffset 	= serializedObject.FindProperty("elbowOffset");
		handOffset 		= serializedObject.FindProperty("handOffset");
		hipOffset 		= serializedObject.FindProperty("hipOffset");
		kneeOffset 		= serializedObject.FindProperty("kneeOffset");
		footOffset 		= serializedObject.FindProperty("footOffset");

		pelvisRotationOffset   = serializedObject.FindProperty("pelvisRotationOffset");
		chestRotationOffset    = serializedObject.FindProperty("chestRotationOffset");
		neckRotationOffset     = serializedObject.FindProperty("neckRotationOffset");
		headRotationOffset     = serializedObject.FindProperty("headRotationOffset");
		clavicleRotationOffset = serializedObject.FindProperty("clavicleRotationOffset");
		shoulderRotationOffset = serializedObject.FindProperty("shoulderRotationOffset");
		elbowRotationOffset    = serializedObject.FindProperty("elbowRotationOffset");
		handRotationOffset     = serializedObject.FindProperty("handRotationOffset");
		hipRotationOffset      = serializedObject.FindProperty("hipRotationOffset");
		kneeRotationOffset     = serializedObject.FindProperty("kneeRotationOffset");
		feetRotationOffset     = serializedObject.FindProperty("feetRotationOffset");;

		pelvisScaleAdjust 	= serializedObject.FindProperty("pelvisScaleAdjust");
		chestScaleAdjust 	= serializedObject.FindProperty("chestScaleAdjust");
		neckScaleAdjust 	= serializedObject.FindProperty("neckScaleAdjust");
		headScaleAdjust 	= serializedObject.FindProperty("headScaleAdjust");
		clavicleScaleAdjust = serializedObject.FindProperty("clavicleScaleAdjust");
		shoulderScaleAdjust = serializedObject.FindProperty("shoulderScaleAdjust");
		handScaleAdjust		= serializedObject.FindProperty("handScaleAdjust");
		hipScaleAdjust		= serializedObject.FindProperty("hipScaleAdjust");
		footScaleAdjust		= serializedObject.FindProperty("footScaleAdjust");

		skeletonController = target as RUISSkeletonController;

		#if UNITY_EDITOR
		RUISSkeletonControllerCheckPlayModeChanges.SaveFieldNames(this);
		Selection.selectionChanged += SelectionChangedCallback;
		#endif
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		normalGUIColor = GUI.color;	

		if(EditorStyles.foldout != null)
		{
			if(boldFoldoutStyle == null)
			{
				boldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				boldFoldoutStyle.fontStyle = FontStyle.Bold;
				boldFoldoutStyle.normal.textColor = EditorStyles.label.normal.textColor;
				boldFoldoutStyle.onNormal.textColor = EditorStyles.label.normal.textColor;
				boldFoldoutStyle.focused.textColor = EditorStyles.label.normal.textColor;
				boldFoldoutStyle.onFocused.textColor = EditorStyles.label.normal.textColor;
				boldFoldoutStyle.active.textColor = EditorStyles.label.normal.textColor;
				boldFoldoutStyle.onActive.textColor = EditorStyles.label.normal.textColor;
			}
			if(coloredBoldFoldoutStyle == null)
			{
				coloredBoldFoldoutStyle = new GUIStyle(EditorStyles.foldout);
				coloredBoldFoldoutStyle.fontStyle = FontStyle.Bold;
				coloredBoldFoldoutStyle.normal.textColor = customLabelColor;
				coloredBoldFoldoutStyle.onNormal.textColor = customLabelColor;
				coloredBoldFoldoutStyle.focused.textColor = customLabelColor;
				coloredBoldFoldoutStyle.onFocused.textColor = customLabelColor;
				coloredBoldFoldoutStyle.active.textColor = customLabelColor;
				coloredBoldFoldoutStyle.onActive.textColor = customLabelColor;

			}
		}

		EditorGUILayout.Space();
		 
		EditorGUILayout.PropertyField(bodyTrackingDevice, new GUIContent("Body Tracking Device", "The source device for body tracking.")); 

		if(!EditorApplication.isPlaying)
			GUI.color = keepPlayModeChangesRGB;

		SwitchToKeepChangesFieldColor();
		EditorGUILayout.PropertyField(keepPlayModeChanges, new GUIContent("Keep PlayMode Changes", "Any changes made in PlayMode will persist "
														+ "for those variables that are marked with RGB(" + keepPlayModeChangesRGB.r + ", " 
														+ keepPlayModeChangesRGB.g + ", " + keepPlayModeChangesRGB.b  +  ") when exiting PlayMode."));
		if(!EditorApplication.isPlaying)
			GUI.color = normalGUIColor;
		

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


        EditorGUILayout.PropertyField(useHierarchicalModel, new GUIContent(  "Hierarchical Model", "Is the model rig hierarchical (ẗhe bone Transforms form "
																		+ "a deep tree structure in the Hierarchy window) or non-hierarchical (all bone "
																		+ "Transforms are on same level)? In almost all cases this option should be "
																		+ "enabled."));

		SwitchToKeepChangesFieldColor();

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

		GUI.enabled = true;
		EditorGUILayout.PropertyField(updateJointPositions, new GUIContent(  "Update Joint Positions", "Place Joint Positions according to the "
																			+ "motion capture input. If \"Scale Bones\" is enabled, then the "
																			+ "tracked positions are not explicitly assigned, rather they come "
																			+ "from the combination of tracked rotations and bone lengths."));
		
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(filterPosition, new GUIContent(  "Filter Positions",   "Smoothen the root, shoulder, and hip positions with "
															+ "a basic Kalman filter. Enabling this option is especially important when using "
															+ "Kinect. Disable this option when using more accurate and responsive mocap systems."));
		if(filterPosition.boolValue)
		{
			// *** OPTIHACK TODO tooltip
			EditorGUILayout.PropertyField(filterHeadPositionOnly, new GUIContent("Filter Only Head Position", "Smoothen ONLY the head position with "
																							+ "a basic Kalman filter, and do not smooth other joints."));
			
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

		if(hmdMovesHead.boolValue) // *** OPTIHACK TODO tooltip
			EditorGUILayout.PropertyField(hmdLocalOffset, new GUIContent("HMD Local Offset", "Offset the head model in HMD's local coordinate system."));
		

		EditorGUI.indentLevel--;

		GUI.enabled = true;
        EditorGUILayout.PropertyField(updateJointRotations, new GUIContent(  "Update Joint Rotations", "Joint Rotations will be updated according to "
																									 + "the motion capture input."));

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
        EditorGUILayout.PropertyField(scaleHierarchicalModelBones, new GUIContent(  "Scale Body", "Scale the torso of the model based on the "
		                                                                          + "real-life torso length of the tracked person, making the "
																				  + "model size correspond to the tracked person size. This "
																				  + "option is only available for hierarchical models."));

		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(boneLengthAxis, new GUIContent(  "Bone Length Axis", "Determines the axis that points the bone direction in each " 
		                                                             + "joint transform of the animation rig. This value depends on your rig. You can "
																	 + "discover the correct axis by examining the animation rig hierarchy, by looking"
																	 + "at the directional axis between parent joints and their child joints in local "
																	 + "coordinate system. IMPORTANT: Disable the below \"Scale Length Only\" option "
																	 + "if the same localScale axis is not consistently used in all the joints of the "
																	 + "animation rig."));

		GUI.enabled = scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.PropertyField(independentTorsoSegmentsScaling, new GUIContent("Torso Segments", "Apply uniform scaling to individual "
																			+ "torso segments (abdomen and chest) to resolve segment proportion " 
																			+ "differences between the user and avatar model. If you leave this "
																			+ "disabled, then translations between segments and a single scale "
																			+ "accross the whole torso are used to resolve the differences. This "
																			+ "option is effective when you use more accurate mocap systems than "
																			+ "Kinect."));

		EditorGUILayout.Slider(torsoThickness, 0.1f, 3, new GUIContent(  "Torso Thickness", "Uniform scale that gets applied to the torso. This also "
																					+ "affects automatic model joint translation, so for example "
																					+ "neck position wouldn't get scaled too high above shoulders."));
		EditorGUILayout.PropertyField(limbsAreScaled, new GUIContent(  "Scale Limbs", "In most cases you should enable this option. The limbs will be "
																	+ "scaled so that their proportions match to that of the tracked user. If this "
																	+ "option is disabled, then the bone joints are simply translated (if \"Update "
																	+ "Joint Positions\" is enabled), which will likely result in broken avatars "
																	+ "especially on users who are smaller than the model size."));

		if(limbsAreScaled.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(scaleBoneLengthOnly, new GUIContent(  "Length Only", "Scale the limb length (localScale.x/y/z) but not the "
		                                                                  + "bone thickness (localScale.yz/xz/xy). WARNING: Enabling this option could "
		                                                                  + "lead to peculiar results, depending on the animation rig. At the very least "
																		  + "it leads to non-uniform scaling, for which there are slight mitigations in "
																		  + "the code."));
		
			if(scaleBoneLengthOnly.boolValue)
			{
				EditorGUILayout.Slider(rightArmThickness, 0.1f, 3, new GUIContent("Right Arm Thickness", "Thickness scale for right arm around its Length Axis."));
				EditorGUILayout.Slider(leftArmThickness,  0.1f, 3, new GUIContent("Left Arm Thickness", "Thickness scale for left arm around its Length Axis."));
				EditorGUILayout.Slider(rightLegThickness, 0.1f, 3, new GUIContent("Right Leg Thickness", "Thickness scale for right leg around its Length Axis."));
				EditorGUILayout.Slider(leftLegThickness,  0.1f, 3, new GUIContent("Left Leg Thickness", "Thickness scale for left leg around its Length Axis."));
			}
			EditorGUI.indentLevel--;
		}

		EditorGUI.indentLevel--;

//        if(!useHierarchicalModel.boolValue)
//		{
//			scaleHierarchicalModelBones.boolValue = false;
//		}
//		if(!scaleHierarchicalModelBones.boolValue)
//			scaleBoneLengthOnly.boolValue = false;

        GUI.enabled = true;

		EditorGUILayout.Space();

		SwitchToNormalFieldColor();
		
		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID) 
		{
			RUISEditorUtility.HorizontalRuler();

			EditorGUILayout.LabelField("  Custom Mocap Source Transforms", coloredBoldItalicStyle);

			normalLabelColor = EditorStyles.label.normal.textColor;
			EditorStyles.label.normal.textColor = customLabelColor;

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(isIMUMocap, new GUIContent("IMU Motion Capture", "Is the motion captured with an IMU suit (e.g. Perception "
															+ "Neuron, Xsens)? Such suits measure relative joint rotations, and can only roughly "
															+ "estimate joint positions. Enable this option if you are using your IMU suit together "
															+ "with a HMD or some other device that has its rotation or position tracked."));

			if(isIMUMocap.boolValue)
			{
				EditorGUI.indentLevel++;

				EditorGUI.indentLevel--;
			}

			EditorGUILayout.PropertyField(customConversionType, new GUIContent("Coordinate Conversion", "The conversion that will be applied to the "
																		+ "Source Transform poses before copying them to Target Transforms (below). "
																		+ "The conversions are defined in " + typeof(RUISInputManager)
																		+ "'s \"Custom 1\" and \"Custom 2\" settings, and also stored in the "
																		+ "associated 'inputConfig.xml'-file."));

			EditorGUILayout.PropertyField(customRoot, new GUIContent("Source Root", "REQUIRED: The source Transform for the skeleton hierarchy's root bone. This "
																	+ "Transform can be same as the below 'Pelvis' source Transform.\nThe source Transforms of "
																	+ "this section should be moved by realtime input from a custom mocap system." /* \nIf you want "
																	+ "this avatar to copy the pose of another " + typeof(RUISSkeletonController) + " that "
																	+ "has the Custom Source fields already set, then you only need to use the same "
																	+ "\"Skeleton ID\" and you can leave the below Custom Source fields empty." */));
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Torso and Head", coloredBoldStyle);
			EditorGUILayout.PropertyField(customTorso, 	 new GUIContent("Pelvis", 	"REQUIRED: The source Transform with tracked pose of the pelvis. Its world frame "
																		+ "location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customChest, 	 new GUIContent("Chest", 	"Optional: The source Transform with tracked pose of the chest. Its world frame "
																		+ "location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customNeck, 	 new GUIContent("Neck", 	"Optional: The source Transform with tracked pose of the neck. Its world frame "
																		+ "location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customHead, 	 new GUIContent("Head", 	"Optional: The source Transform with tracked pose of the head. Its world frame "
																		+ "location and rotation will be utilized, so be mindful about parent Transforms."));
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Arms", coloredBoldStyle);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));

			EditorGUILayout.PropertyField(customLeftClavicle, 	new GUIContent("Left Clavicle", "Optional: The source Transform with tracked pose of the left clavicle. "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customLeftShoulder, 	new GUIContent("Left Shoulder", "REQUIRED: The source Transform with tracked pose of the left shoulder (upper arm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customLeftElbow, 	  	new GUIContent("Left Elbow",	"Optional: The source Transform with tracked pose of the left elbow (forearm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customLeftHand, 		new GUIContent("Left Hand", 	"Optional: The source Transform with tracked pose of the  left wrist (hand). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightClavicle, new GUIContent("Right Clavicle", "Optional: The source Transform with tracked pose of the right clavicle. "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightShoulder, new GUIContent("Right Shoulder", "REQUIRED: The source Transform with tracked pose of the right shoulder (upper arm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightElbow,    new GUIContent("Right Elbow", 	"Optional: The source Transform with tracked pose of the right elbow (forearm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightHand,	   new GUIContent("Right Hand",	    "Optional: The source Transform with tracked pose of the right wrist (hand). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Source for Legs", coloredBoldStyle);
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customLeftHip,  new GUIContent("Left Hip",  "REQUIRED: The source Transform with tracked pose of the left hip (thigh). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customLeftKnee, new GUIContent("Left Knee", "Optional: The source Transform with tracked pose of the left knee (shin). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customLeftFoot, new GUIContent("Left Foot", "Optional: The source Transform with tracked pose of the left ankle (foot). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightHip,  new GUIContent("Right Hip",	"REQUIRED: The source Transform with tracked pose of the right hip (thigh). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightKnee, new GUIContent("Right Knee", "Optional: The source Transform with tracked pose of the right knee (shin). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightFoot, new GUIContent("Right Foot", "Optional: The source Transform with tracked pose of the right ankle (foot). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;
			
			EditorGUILayout.Space();

			showSourceFingers = EditorGUILayout.Foldout(showSourceFingers, "Source for Fingers", true, coloredBoldFoldoutStyle);
			if(showSourceFingers)
			{
				EditorGUIUtility.labelWidth = Screen.width / 6;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
				EditorGUILayout.PropertyField(customLeftThumb, new GUIContent ("Left Thumb CMC", "The source Transform for the 'root' of left thumb, also known as carpometacarpal (CMC) "
					+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftIndexF,  new GUIContent ("Left Index Finger MCP",	"The source Transform for the 'root' of left index finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftMiddleF, new GUIContent ("Left Middle Finger MCP", "The source Transform for the 'root' of left middle finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftRingF,   new GUIContent ("Left Ring Finger MCP", "The source Transform for the 'root' of left ring finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftLittleF, new GUIContent ("Left Little Finger MCP", "The source Transform for the 'root' of left little finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
				EditorGUILayout.PropertyField(customRightThumb, new GUIContent ("Right Thumb CMC", "The source Transform for the 'root' of right thumb, also known as carpometacarpal (CMC) "
					+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightIndexF,  new GUIContent ("Right Index Finger MCP", "The source Transform for the 'root' of right index finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightMiddleF, new GUIContent ("Right Middle Finger MCP", "The source Transform for the 'root' of right middle finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightRingF,   new GUIContent ("Right Ring Finger MCP", "The source Transform for the 'root' of right ring finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightLittleF, new GUIContent ("Right Little Finger MCP", "The source Transform for the 'root' of right little finger, also known as "
					+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 0;
			}

			EditorStyles.label.normal.textColor = normalLabelColor;
		}

		RUISEditorUtility.HorizontalRuler();

		EditorGUILayout.LabelField("  Avatar Target Transforms", boldItalicStyle);

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
		{	
			SwitchToKeepChangesFieldColor();
			EditorGUILayout.PropertyField(trackWrist, new GUIContent("Track Wrist Rotation", "Track the rotation of the hand bone"));
			SwitchToNormalFieldColor();
		}

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
		{
			SwitchToKeepChangesFieldColor();
			EditorGUILayout.PropertyField(trackAnkle, new GUIContent("Track Ankle Rotation", "Track the rotation of the ankle bone"));
			SwitchToNormalFieldColor();
		}

		EditorGUILayout.Space();

		showTargetFingers = EditorGUILayout.Foldout(showTargetFingers, "Finger Targets", true, boldFoldoutStyle);
		if(showTargetFingers)
		{
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(leftThumb,   new GUIContent ("Left Thumb CMC", "The 'root' of left thumb, also known as carpometacarpal (CMC) "
																+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
			EditorGUILayout.PropertyField(leftIndexF,  new GUIContent ("Left Index Finger MCP",	"The 'root' of left index finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftMiddleF, new GUIContent ("Left Middle Finger MCP", "The 'root' of left middle finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftRingF,   new GUIContent ("Left Ring Finger MCP", "The 'root' of left ring finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftLittleF, new GUIContent ("Left Little Finger MCP", "The 'root' of left little finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(rightThumb,   new GUIContent ("Right Thumb CMC", "The 'root' of right thumb, also known as carpometacarpal (CMC) "
																+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
			EditorGUILayout.PropertyField(rightIndexF,  new GUIContent ("Right Index Finger MCP", "The 'root' of right index finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightMiddleF, new GUIContent ("Right Middle Finger MCP", "The 'root' of right middle finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightRingF,   new GUIContent ("Right Ring Finger MCP", "The 'root' of right ring finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightLittleF, new GUIContent ("Right Little Finger MCP", "The 'root' of right little finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID)
			{
				SwitchToKeepChangesFieldColor();
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
				SwitchToNormalFieldColor();
			}

		}

		EditorGUILayout.Space();

		RUISEditorUtility.HorizontalRuler();

		SwitchToKeepChangesFieldColor();

		EditorGUILayout.LabelField("  Adjustments", boldItalicStyle);
        EditorGUILayout.PropertyField(rotationDamping, new GUIContent(  "Max Joint Angular Velocity", "Maximum joint angular velocity can be used "
		                                                              + "for damping avatar bone movement (smaller values). The values are in "
																	  + "degrees. For Kinect and similar devices, a value of 360 is suitable. For "
																	  + "more accurate and responsive mocap systems, this value can easily be set "
																	  + "to 7200 or more, so that very fast motions are not restricted."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.PropertyField(maxScaleFactor, new GUIContent(  "Max Scale Rate", "The maximum amount the scale of a bone can "
		                                                             + "change per second when using \"Hierarchical Model\" and \"Scale Bones\""));

		GUI.enabled = true;
		SwitchToNormalFieldColor();
		EditorGUILayout.PropertyField(minimumConfidenceToUpdate, new GUIContent(  "Min Confidence to Update", "The minimum confidence in joint "
																				+ "positions and rotations needed to update these values. "
																				+ "The confidence is either 0; 0,5; or 1. This setting is only "
																				+ "relevant with Kinect tracking."));
		minimumConfidenceToUpdate.floatValue = Mathf.Clamp01(minimumConfidenceToUpdate.floatValue);
		SwitchToKeepChangesFieldColor();

		GUI.enabled = scaleHierarchicalModelBones.boolValue;

		EditorGUILayout.PropertyField(heightAffectsOffsets, new GUIContent("Scaled Offsets", "When this option is enabled, then the below position "
																+ "offsets are scaled according to the detected user height. The scale factor "
																+ "comes from the user's sitting height (pelvis to head distance), where sitting "
																+ "height of 1 meter equals to factor of 1."));

		EditorGUILayout.Space();

		GUI.enabled = true;

		// *** OPTIHACK
		EditorGUILayout.PropertyField(pelvisOffset, new GUIContent("Pelvis Offset",   "Offsets pelvis joint position in its local frame in meters. WARNING: "
																					+ "This also offsets the absolute positions of all spine and clavicle "
						                                                            + "joints! The offset is relative to the scale of pelvis joint. This setting "
						                                                            + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(pelvisScaleAdjust, 0.01f, 3, new GUIContent("Pelvis Scale Adjust",   "Scales pelvis. This setting has effect only when \"Hierarchical "
		                                                                   + "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.PropertyField(chestOffset, new GUIContent("Chest Offset",   "Offsets chest joint position in its local frame in meters. "
		                                                          + "WARNING: This also offsets the absolute positions of neck, head and "
		                                                          + "clavicle joints! The offset is relative to the scale of pelvis joint. This setting "
		                                                          + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(chestScaleAdjust, 0.01f, 3, new GUIContent("Chest Scale Adjust",   "Scales chest. This setting has effect only when \"Hierarchical "
		                                                                + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(neckOffset, new GUIContent("Neck Offset",   "Offsets neck joint position in its local frame in meters. "
		                                                         + "WARNING: This also offsets the absolute positions of head and "
		                                                         + "clavicle joints! The offset is relative to the scale of chest joint. This setting "
		                                                         + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(neckScaleAdjust, 0.01f, 3, new GUIContent("Neck Scale Adjust",   "Scales neck. This setting has effect only when \"Hierarchical "
		                                                               + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(headOffset, new GUIContent("Head Offset",   "Offsets head joint position in its local frame in meters. "
		                                                         + "The offset is relative to the scale of neck joint. This setting has effect only when \"Hierarchical "
		                                                         + "Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(headScaleAdjust, 0.01f, 3, new GUIContent("Head Scale Adjust",   "Scales head. This setting has effect only when \"Hierarchical "
		                                                              + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(clavicleOffset, new GUIContent("Clavicle Offset",   "Offsets clavicle joint positions in their local frame in meters. "
																	 + "The offset is relative to the scale of neck joint. This setting has effect only when "
		                                                             + "\"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(clavicleScaleAdjust, 0.01f, 3, new GUIContent("Clavice Scale Adjust",   "Scales clavicles. This setting has effect only when \"Hierarchical "
		                                                              + "Model\" and \"Scale Bones\" are enabled."));
		
		EditorGUILayout.PropertyField(shoulderOffset, new GUIContent("Shoulder Offset",   "Offsets shoulder joint positions in their local frame in meters. "
																						+ "WARNING: This also offsets the absolute positions of all the "
																						+ "arm joints! Offset is relative to the scale of parent joint. This setting "
		                                                   						        + "has effect only when \"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		GUI.enabled = !limbsAreScaled.boolValue;
		EditorGUILayout.PropertyField(elbowOffset, new GUIContent("Elbow Offset",   "Offsets elbow joint positions in their "
																+ "local frame in meters. This offset has no effect if \"Scale Limbs\" is enabled."));
		EditorGUILayout.PropertyField(handOffset, new GUIContent("Hand Offset",   "Offsets hand joint positions in their "
																+ "local frame in meters. This offset has no effect if \"Scale Limbs\" is enabled."));
		GUI.enabled = true;

		// *** OPTIHACK
		EditorGUILayout.PropertyField(hipOffset, new GUIContent("Hip Offset",	"Offsets hip joint positions in their local frame in meters. WARNING: This also "
					                                                          + "offsets the absolute positions of all the leg joints! Offset is relative to the "
					                                                          + "scale of pelvis joint. This setting has effect only when \"Hierarchical Model\" and " 
																			  + "\"Scale Bones\" are enabled."));

		GUI.enabled = !limbsAreScaled.boolValue;
		EditorGUILayout.PropertyField(kneeOffset, new GUIContent("Knee Offset",   "Offsets knee joint positions in their "
																+ "local frame in meters. This offset has no effect if \"Scale Limbs\" is enabled."));
		EditorGUILayout.PropertyField(footOffset, new GUIContent("Foot Offset",   "Offsets foot joint positions in their "
																+ "local frame in meters. This offset has no effect if \"Scale Limbs\" is enabled."));

		EditorGUILayout.Space();

		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.Slider(forearmLengthTweaker, 0.01f, 3, new GUIContent(   "Forearm Scale Adjust", "The forearm length ratio "
			                                                                   + "compared to the real-world value, use this to lengthen or "
			                                                                   + "shorten the forearms. Only used if \"Hierarchical Model\" is enabled"));
		EditorGUILayout.Slider(shinLengthTweaker, 0.01f, 3, new GUIContent(   "Shin Scale Adjust", "The shin length ratio compared to the "
			                                                                + "real-world value, use this to lengthen or shorten the "
			                                                                + "shins. Only used if \"Hierarchical Model\" is enabled"));
		EditorGUILayout.Space();

		EditorGUILayout.Slider(handScaleAdjust, 0.01f, 3, new GUIContent("Hand Scale Adjust", "Scales hands. This setting has effect only when "
																		+ "\"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Slider(footScaleAdjust, 0.01f, 3, new GUIContent("Foot Scale Adjust", "Scales feet. This setting has effect only when "
																		+ "\"Hierarchical Model\" and \"Scale Bones\" are enabled."));

		EditorGUILayout.Space();

		showLocalOffsets = EditorGUILayout.Foldout(showLocalOffsets, "Local Rotation Offsets", true);
		if(showLocalOffsets)
		{
			EditorGUI.indentLevel += 1;
			EditorGUILayout.PropertyField(pelvisRotationOffset, new GUIContent("Pelvis (Rot)", "Offsets the pelvis joint rotation in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(chestRotationOffset, new GUIContent("Chest (Rot)", "Offsets the chest joint rotation in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(neckRotationOffset, new GUIContent("Neck (Rot)", "Offsets the neck joint rotation in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(headRotationOffset, new GUIContent("Head (Rot)", "Offsets the head joint rotation in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(clavicleRotationOffset, new GUIContent("Clavicles (Rot)", "Offsets the clavicle joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(shoulderRotationOffset, new GUIContent("Shoulders (Rot)", "Offsets the shoulder joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(elbowRotationOffset, new GUIContent("Elbows (Rot)", "Offsets the elbow joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(handRotationOffset, new GUIContent("Hands (Rot)", "Offsets the hand joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(hipRotationOffset, new GUIContent("Hips (Rot)", "Offsets the hip joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(kneeRotationOffset, new GUIContent("Knees (Rot)", "Offsets the knee joint rotations in the local body segment frame (Euler angles)."));
			EditorGUILayout.PropertyField(feetRotationOffset, new GUIContent("Feet (Rot)", "Offsets the foot joint rotations in the local body segment frame (Euler angles)."));
			EditorGUI.indentLevel -= 1;
		}

		SwitchToNormalFieldColor();

		GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

	private void SwitchToKeepChangesFieldColor()
	{
		if(EditorApplication.isPlaying && keepPlayModeChanges.boolValue)
			GUI.color = keepPlayModeChangesRGB;
	}

	private void SwitchToNormalFieldColor()
	{
		if(EditorApplication.isPlaying)
			GUI.color = normalGUIColor;
	}

	#if UNITY_EDITOR
	public void Awake()
	{
		selectedTransforms = Selection.transforms;
		RUISSkeletonControllerCheckPlayModeChanges.serializedObject = serializedObject;
		RUISSkeletonControllerCheckPlayModeChanges.SaveFieldNames(this);
	}
	#endif

	#if UNITY_EDITOR
	public void OnDisable()
	{
		RUISSkeletonControllerCheckPlayModeChanges.SaveFieldNames(this);
		if(EditorApplication.isPlaying)
		{
			foreach(Transform transform in selectedTransforms)
			{
				if(transform)
				{
					skeletonController = transform.GetComponent<RUISSkeletonController>();
					if(skeletonController)
					{
						bool isSelected = false;
						foreach(Transform selectedTransform in Selection.transforms)
						{
							if(transform == selectedTransform)
							{
								isSelected = true;
								break;
							}
						}
						if(!isSelected)
						{
							// Update stored variables of those RUISSkeletonControllers that were just selected
							RUISSkeletonControllerCheckPlayModeChanges.SaveSkeletonControllerVariables(skeletonController);
						}
					}
				}
			}
		}
		Selection.selectionChanged -= SelectionChangedCallback;
	}
	#endif

	#if UNITY_EDITOR
	Transform[] selectedTransforms = new Transform[1];
	public void SelectionChangedCallback()
	{
//		string gameObjectName = "";
//		if(target)
//			gameObjectName = ((RUISSkeletonController) target).gameObject.name;
//		Debug.Log("SelectionChangedCallback " + gameObjectName);

		selectedTransforms = Selection.transforms;
		RUISSkeletonControllerCheckPlayModeChanges.serializedObject = serializedObject;
	}
	#endif
}


#if UNITY_EDITOR
[InitializeOnLoad]
public static class RUISSkeletonControllerCheckPlayModeChanges
{
	public static SerializedObject serializedObject;

	static List<GameObject> gameObjects;
	static FieldInfo[] editorFields;
	static List<string> fieldNameList;
	static Dictionary<GameObject, System.Object[]> controllerProperties;

	static RUISSkeletonControllerCheckPlayModeChanges() 
	{
		gameObjects = new List<GameObject>();
		controllerProperties = new Dictionary<GameObject, System.Object[]>();
		EditorApplication.playmodeStateChanged += PlaymodeStateChange;

		editorFields = typeof(RUISSkeletonControllerEditor).GetFields();
		fieldNameList = new List<string>();
	}

	static void PlaymodeStateChange()
	{
		RUISSkeletonController skeletonController;
		
		// From edit mode to play mode
//		if(!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
//		{
//		}

		// Exiting play mode
		if(EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
		{
			// Save variables of all RUISSkeletonControllers that were selected at some point during play mode
			foreach(Transform transform in Selection.transforms)
			{
				if(transform)
				{
					skeletonController = transform.GetComponent<RUISSkeletonController>();
					if(skeletonController)
					{
						SaveSkeletonControllerVariables(skeletonController);
					}
				}
			}

			Dictionary<GameObject, System.Object[]> copiedControllerProperties = new Dictionary<GameObject, System.Object[]>();
			List<GameObject> transientGameObjects = new List<GameObject>();
			foreach(KeyValuePair<GameObject, System.Object[]> entry in controllerProperties)
			{
				if(entry.Key && entry.Value != null)
				{
					skeletonController = entry.Key.GetComponent<RUISSkeletonController>();
					if(skeletonController && skeletonController.mecanimCombiner)
					{
						if(copiedControllerProperties.ContainsKey(skeletonController.mecanimCombiner.gameObject))
							copiedControllerProperties.Remove(skeletonController.mecanimCombiner.gameObject);
						copiedControllerProperties.Add(skeletonController.mecanimCombiner.gameObject, entry.Value);

						transientGameObjects.Add(entry.Key);
					}
				}
			}
			foreach(GameObject gameObject in transientGameObjects)
			{
				// Remove gameObjects that will be deleted when exiting play mode
				if(copiedControllerProperties.ContainsKey(gameObject))
					copiedControllerProperties.Remove(gameObject);
			}
			foreach(KeyValuePair<GameObject, System.Object[]> entry in copiedControllerProperties)
			{
				if(entry.Key && entry.Value != null)
				{
					// Add gameObjects where the RUISSkeletonController script reappear when exiting play mode
					if(controllerProperties.ContainsKey(entry.Key))
						controllerProperties.Remove(entry.Key);
					controllerProperties.Add(entry.Key, entry.Value);
				}
			}

//			Debug.Log("Save");
		}
		else if(!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) // Exited play mode
		{
			// Load stored variables of all RUISSkeletonControllers that were selected at some point during play mode
			foreach(KeyValuePair<GameObject, System.Object[]> entry in controllerProperties)
			{
				if(entry.Key && entry.Value != null)
				{
					skeletonController = entry.Key.GetComponent<RUISSkeletonController>();
					// *** OPTIHACK5 TODO THIS DOESN'T WORK WITH MECANIMBLENDED CHARACTER
					if(		skeletonController && fieldNameList != null && (fieldNameList.IndexOf("keepPlayModeChanges") >= 0 
						&& ((bool) entry.Value[fieldNameList.IndexOf("keepPlayModeChanges")])))
					{
//						Debug.Log(" shutting down " + entry.Value.Length + " " + fieldNameList.Count);
						for(int i = 0; i < entry.Value.Length; ++i)
						{
							if(i < fieldNameList.Count && typeof(RUISSkeletonController).GetField(fieldNameList[i]) != null && entry.Value[i] != null)
								typeof(RUISSkeletonController).GetField(fieldNameList[i]).SetValue(skeletonController, entry.Value[i]);
						}
						EditorUtility.SetDirty(skeletonController);
					}
				}
			}
//			Debug.Log("Load");
				
			gameObjects.Clear();
			controllerProperties.Clear();
		}
	}

	public static void SaveFieldNames(RUISSkeletonControllerEditor editor)
	{
		if(fieldNameList != null && fieldNameList.Count > 0)
			return;

		fieldNameList = new List<string>();

		editorFields = typeof(RUISSkeletonControllerEditor).GetFields();
		if(editor && editorFields != null)
		{
			for(int i = 0; i < editorFields.Length; ++i) // TODO match fields by their name 
			{
				if(editorFields[i] != null && editorFields[i].GetValue(editor) != null && editorFields[i].FieldType == typeof(SerializedProperty))
				{
					switch(((SerializedProperty) editorFields[i].GetValue(editor)).propertyType)
					{
						case SerializedPropertyType.Float:
						case SerializedPropertyType.Integer:
						case SerializedPropertyType.Boolean:
						case SerializedPropertyType.Vector3:
						case SerializedPropertyType.Enum:
							fieldNameList.Add(((SerializedProperty) editorFields[i].GetValue(editor)).name);
						break;
					}
				}
			}

//			foreach(string name in fieldNameList)
//				Debug.Log("  var name: " + name);
		}
	}


	public static void SaveSkeletonControllerVariables(RUISSkeletonController skeletonController)
	{
		if(skeletonController && editorFields != null)
		{		
			if(controllerProperties.ContainsKey(skeletonController.gameObject))
				controllerProperties.Remove(skeletonController.gameObject);
			
			// Add variables to dictionary
			System.Object[] savedValues = new System.Object[fieldNameList.Count];

			for(int i = 0; i < fieldNameList.Count; ++i)
			{
				if(typeof(RUISSkeletonController).GetField(fieldNameList[i]) != null)
					savedValues[i] = typeof(RUISSkeletonController).GetField(fieldNameList[i]).GetValue(skeletonController);
				else
					savedValues[i] = null;
			}

			controllerProperties.Add(skeletonController.gameObject, savedValues);
		}
	}
}
#endif