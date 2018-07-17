/*****************************************************************************

Content    :   Inspector behaviour for RUISSkeletonController script
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen.
               All Rights reserved.
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

	float minScale = 0.01f; // Should not be negative!
	float maxScale = 3.0f;
	float minThickness = 0.3f; // Should not be negative!
	float maxThickness = 3.0f;

	// Below those SerializedProperty fields that are marked with public can be kept (saved) after exiting play mode, if the SerializedProperty links to a
	// float, integer, boolean, Vector3, or Enum (other data types can be added)

	SerializedProperty bodyTrackingDevice;

	public SerializedProperty keepPlayModeChanges;
	public SerializedProperty playerId;

	SerializedProperty switchToAvailableKinect;
	SerializedProperty useHierarchicalModel;

	public SerializedProperty updateRootPosition;
	public SerializedProperty updateJointPositions;
	public SerializedProperty updateJointRotations;
	
	public SerializedProperty rootSpeedScaling;
//	public SerializedProperty rootOffset;
	public SerializedProperty hmdRotatesHead;
	public SerializedProperty hmdMovesHead;
	public SerializedProperty neckInterpolate;
	public SerializedProperty hmdLocalOffset;
	public SerializedProperty neckInterpolateBlend;

	public SerializedProperty scaleHierarchicalModelBones;
	public SerializedProperty scaleBoneLengthOnly;

//	public SerializedProperty boneLengthAxis;

	public SerializedProperty limbsAreScaled;
	public SerializedProperty independentTorsoSegmentsScaling;
	public SerializedProperty scalingNeck;
	public SerializedProperty scalingClavicles;
	public SerializedProperty heightAffectsOffsets;

	public SerializedProperty forceChestPosition;
	public SerializedProperty forceNeckPosition;
	public SerializedProperty forceHeadPosition;
	public SerializedProperty forceClaviclePosition;

	public SerializedProperty torsoThickness;
	public SerializedProperty rightArmThickness;
	public SerializedProperty leftArmThickness;
	public SerializedProperty rightLegThickness;
	public SerializedProperty leftLegThickness;

	public SerializedProperty rightUpperArmThickness;
	public SerializedProperty rightForearmThickness;
	public SerializedProperty leftUpperArmThickness;
	public SerializedProperty leftForearmThickness;
	public SerializedProperty rightThighThickness;
	public SerializedProperty rightShinThickness;
	public SerializedProperty leftThighThickness;
	public SerializedProperty leftShinThickness;

	public SerializedProperty filterPosition;
	public SerializedProperty filterHeadPositionOnly;
	public SerializedProperty positionNoiseCovariance;

	public SerializedProperty filterRotations;
	public SerializedProperty rotationNoiseCovariance;
	public SerializedProperty customMocapFrameRate;
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

	public SerializedProperty maxAngularVelocity;
	public SerializedProperty maxFingerAngularVelocity;
	public SerializedProperty forearmLengthRatio;
	public SerializedProperty shinLengthRatio;

	public SerializedProperty fistMaking;
	SerializedProperty kinect2Thumbs;
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

	public SerializedProperty thumbRotationOffset;
	public SerializedProperty indexFRotationOffset;
	public SerializedProperty middleFRotationOffset;
	public SerializedProperty ringFRotationOffset;
	public SerializedProperty littleFRotationOffset;

	public SerializedProperty clenchedThumbAngleTM;
	public SerializedProperty clenchedThumbAngleMCP;
	public SerializedProperty clenchedThumbAngleIP;
	public SerializedProperty clenchedFingerAngleMCP;
	public SerializedProperty clenchedFingerAnglePIP;
	public SerializedProperty clenchedFingerAngleDIP;

	public SerializedProperty pelvisScaleAdjust;
	public SerializedProperty chestScaleAdjust;
	public SerializedProperty neckScaleAdjust;
	public SerializedProperty headScaleAdjust;
	public SerializedProperty clavicleScaleAdjust;
//	public SerializedProperty shoulderScaleAdjust;
//	public SerializedProperty hipScaleAdjust;
//	public SerializedProperty handScaleAdjust;
	public SerializedProperty leftHandScaleAdjust;
	public SerializedProperty rightHandScaleAdjust;
//	public SerializedProperty footScaleAdjust;
	public SerializedProperty leftFootScaleAdjust;
	public SerializedProperty rightFootScaleAdjust;

	public SerializedProperty headsetDragsBody;
	public SerializedProperty yawCorrectIMU;
	public SerializedProperty yawCorrectAngularVelocity;
	SerializedProperty yawCorrectResetButton;

	SerializedProperty customParent;

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

//	SerializedProperty customHMDSource;
	public SerializedProperty headsetCoordinates;

	SerializedProperty customConversionType;

	public SerializedProperty avatarCollider;
	public SerializedProperty colliderRadius;
	public SerializedProperty colliderLengthOffset;
	public SerializedProperty createFingerColliders;
	public SerializedProperty pelvisHasBoxCollider;
	public SerializedProperty chestHasBoxCollider;
	public SerializedProperty headHasBoxCollider;
	public SerializedProperty handHasBoxCollider;
	public SerializedProperty footHasBoxCollider;
	public SerializedProperty fingerHasBoxCollider;

	public SerializedProperty pelvisDepthMult;
	public SerializedProperty pelvisWidthMult;
	public SerializedProperty pelvisLengthMult;
	public SerializedProperty chestDepthMult;
	public SerializedProperty chestWidthMult;
	public SerializedProperty chestLengthMult;
	public SerializedProperty neckRadiusMult;
	public SerializedProperty headDepthMult;
	public SerializedProperty headWidthMult;
	public SerializedProperty headLengthMult;
	public SerializedProperty shoulderRadiusMult;
	public SerializedProperty elbowRadiusMult;
	public SerializedProperty handDepthMult;
	public SerializedProperty handWidthMult;
	public SerializedProperty handLengthMult;
	public SerializedProperty fingerRadiusMult;
	public SerializedProperty fingerLengthMult;
	public SerializedProperty fingerTaperValue;
	public SerializedProperty thumbRadiusMult;
	public SerializedProperty thumbLengthMult;
	public SerializedProperty thumbTaperValue;
	public SerializedProperty thighRadiusMult;
	public SerializedProperty shinRadiusMult;
	public SerializedProperty footDepthMult;
	public SerializedProperty footWidthMult;
	public SerializedProperty footLengthMult;

	GUIStyle coloredBoldStyle = new GUIStyle();
	GUIStyle coloredBoldItalicStyle = new GUIStyle();
	Color normalLabelColor;
	Color normalGUIColor;
	bool normalLabelColorWasSaved = false;

	GUIStyle boldItalicStyle  = new GUIStyle();
	GUIStyle italicStyle      = new GUIStyle();
	GUIStyle boldFoldoutStyle = null;
	GUIStyle coloredBoldFoldoutStyle = null;

	RUISSkeletonController skeletonController;
	Animator animator;
	string obtainedTargetTransforms = "";
	string obtainedSourceTransforms = "";

	static bool showLocalOffsets;
	static bool showTargetFingers;
	static bool showSourceFingers;
	static bool showColliderAdjustments;

	public void OnEnable()
	{
		coloredBoldStyle.fontStyle = FontStyle.Bold;
		coloredBoldStyle.normal.textColor = customLabelColor;
		coloredBoldItalicStyle.fontStyle = FontStyle.BoldAndItalic;
		coloredBoldItalicStyle.normal.textColor = customLabelColor;
		boldItalicStyle.fontStyle = FontStyle.BoldAndItalic;
		italicStyle.fontStyle = FontStyle.Italic;
		italicStyle.alignment = TextAnchor.MiddleCenter;

		keepPlayModeChanges = serializedObject.FindProperty("keepPlayModeChanges");

		bodyTrackingDevice = serializedObject.FindProperty("bodyTrackingDevice");
		playerId = serializedObject.FindProperty("playerId");
		switchToAvailableKinect = serializedObject.FindProperty("switchToAvailableKinect");

        useHierarchicalModel = serializedObject.FindProperty("useHierarchicalModel");

        updateRootPosition = serializedObject.FindProperty("updateRootPosition");
        updateJointPositions = serializedObject.FindProperty("updateJointPositions");
        updateJointRotations = serializedObject.FindProperty("updateJointRotations");
		
		rootSpeedScaling = serializedObject.FindProperty("rootSpeedScaling");
//		rootOffset = serializedObject.FindProperty("rootOffset");
		hmdRotatesHead = serializedObject.FindProperty("hmdRotatesHead");
		hmdMovesHead = serializedObject.FindProperty("hmdMovesHead");
		neckInterpolate = serializedObject.FindProperty("neckInterpolate");
		hmdLocalOffset = serializedObject.FindProperty("hmdLocalOffset");
		neckInterpolateBlend = serializedObject.FindProperty("neckInterpolateBlend");

		scaleHierarchicalModelBones = serializedObject.FindProperty("scaleHierarchicalModelBones");
		scaleBoneLengthOnly = serializedObject.FindProperty("scaleBoneLengthOnly");
//		boneLengthAxis = serializedObject.FindProperty("torsoBoneLengthAxis");
		limbsAreScaled = serializedObject.FindProperty("limbsAreScaled");
		independentTorsoSegmentsScaling = serializedObject.FindProperty("independentTorsoSegmentsScaling");
		scalingNeck = serializedObject.FindProperty("scalingNeck");
		scalingClavicles = serializedObject.FindProperty("scalingClavicles");
		heightAffectsOffsets = serializedObject.FindProperty("heightAffectsOffsets");

		forceChestPosition = serializedObject.FindProperty("forceChestPosition");
		forceNeckPosition  = serializedObject.FindProperty("forceNeckPosition");
		forceHeadPosition  = serializedObject.FindProperty("forceHeadPosition");
		forceClaviclePosition = serializedObject.FindProperty("forceClaviclePosition");

		torsoThickness = serializedObject.FindProperty("torsoThickness");
		rightArmThickness 	= serializedObject.FindProperty("rightArmThickness");
		leftArmThickness 	= serializedObject.FindProperty("leftArmThickness");
		rightLegThickness 	= serializedObject.FindProperty("rightLegThickness");
		leftLegThickness 	= serializedObject.FindProperty("leftLegThickness");

		rightUpperArmThickness 	= serializedObject.FindProperty("rightUpperArmThickness");
		rightForearmThickness 	= serializedObject.FindProperty("rightForearmThickness");
		leftUpperArmThickness 	= serializedObject.FindProperty("leftUpperArmThickness");
		leftForearmThickness 	= serializedObject.FindProperty("leftForearmThickness");
		rightThighThickness 	= serializedObject.FindProperty("rightThighThickness");
		rightShinThickness 		= serializedObject.FindProperty("rightShinThickness");
		leftThighThickness 		= serializedObject.FindProperty("leftThighThickness");
		leftShinThickness 		= serializedObject.FindProperty("leftShinThickness");

		filterRotations = serializedObject.FindProperty("filterRotations");
		rotationNoiseCovariance = serializedObject.FindProperty("rotationNoiseCovariance");
		customMocapFrameRate = serializedObject.FindProperty("customMocapFrameRate");
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
		maxAngularVelocity = serializedObject.FindProperty("maxAngularVelocity");
		maxFingerAngularVelocity = serializedObject.FindProperty("maxFingerAngularVelocity");
        forearmLengthRatio = serializedObject.FindProperty("forearmLengthRatio");
		shinLengthRatio = serializedObject.FindProperty("shinLengthRatio");
		
		fistMaking = serializedObject.FindProperty("fistMaking");
		kinect2Thumbs = serializedObject.FindProperty("kinect2Thumbs");

		headsetDragsBody = serializedObject.FindProperty("headsetDragsBody");
		yawCorrectIMU    = serializedObject.FindProperty("yawCorrectIMU");
		yawCorrectAngularVelocity = serializedObject.FindProperty("yawCorrectAngularVelocity");
		yawCorrectResetButton     = serializedObject.FindProperty("yawCorrectResetButton");

		customParent	= serializedObject.FindProperty("customParent");

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

//		customHMDSource		= serializedObject.FindProperty("customHMDSource");
		headsetCoordinates	= serializedObject.FindProperty("headsetCoordinates");

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

		thumbRotationOffset   = serializedObject.FindProperty("thumbRotationOffset");
		indexFRotationOffset  = serializedObject.FindProperty("indexFRotationOffset");
		middleFRotationOffset = serializedObject.FindProperty("middleFRotationOffset");
		ringFRotationOffset   = serializedObject.FindProperty("ringFRotationOffset");
		littleFRotationOffset = serializedObject.FindProperty("littleFRotationOffset");

		clenchedThumbAngleTM   = serializedObject.FindProperty("clenchedThumbAngleTM");
		clenchedThumbAngleMCP  = serializedObject.FindProperty("clenchedThumbAngleMCP");
		clenchedThumbAngleIP   = serializedObject.FindProperty("clenchedThumbAngleIP");
		clenchedFingerAngleMCP = serializedObject.FindProperty("clenchedFingerAngleMCP");
		clenchedFingerAnglePIP = serializedObject.FindProperty("clenchedFingerAnglePIP");
		clenchedFingerAngleDIP = serializedObject.FindProperty("clenchedFingerAngleDIP");

		pelvisScaleAdjust 	= serializedObject.FindProperty("pelvisScaleAdjust");
		chestScaleAdjust 	= serializedObject.FindProperty("chestScaleAdjust");
		neckScaleAdjust 	= serializedObject.FindProperty("neckScaleAdjust");
		headScaleAdjust 	= serializedObject.FindProperty("headScaleAdjust");
		clavicleScaleAdjust = serializedObject.FindProperty("clavicleScaleAdjust");
//		shoulderScaleAdjust = serializedObject.FindProperty("shoulderScaleAdjust");
//		hipScaleAdjust		= serializedObject.FindProperty("hipScaleAdjust");
//		handScaleAdjust		= serializedObject.FindProperty("handScaleAdjust");
		leftHandScaleAdjust		= serializedObject.FindProperty("leftHandScaleAdjust");
		rightHandScaleAdjust	= serializedObject.FindProperty("rightHandScaleAdjust");
//		footScaleAdjust		= serializedObject.FindProperty("footScaleAdjust");
		leftFootScaleAdjust		= serializedObject.FindProperty("leftFootScaleAdjust");
		rightFootScaleAdjust	= serializedObject.FindProperty("rightFootScaleAdjust");

		avatarCollider		  = serializedObject.FindProperty("avatarCollider");
		colliderRadius		  = serializedObject.FindProperty("colliderRadius");
		colliderLengthOffset  = serializedObject.FindProperty("colliderLengthOffset");
		createFingerColliders = serializedObject.FindProperty("createFingerColliders");
		pelvisHasBoxCollider  = serializedObject.FindProperty("pelvisHasBoxCollider");
		chestHasBoxCollider   = serializedObject.FindProperty("chestHasBoxCollider");
		headHasBoxCollider    = serializedObject.FindProperty("headHasBoxCollider");
		handHasBoxCollider    = serializedObject.FindProperty("handHasBoxCollider");
		footHasBoxCollider    = serializedObject.FindProperty("footHasBoxCollider");
		fingerHasBoxCollider  = serializedObject.FindProperty("fingerHasBoxCollider");

		pelvisDepthMult 		= serializedObject.FindProperty("pelvisDepthMult");
		pelvisWidthMult 		= serializedObject.FindProperty("pelvisWidthMult");
		pelvisLengthMult 		= serializedObject.FindProperty("pelvisLengthMult");
		chestDepthMult 			= serializedObject.FindProperty("chestDepthMult");
		chestWidthMult 			= serializedObject.FindProperty("chestWidthMult");
		chestLengthMult 		= serializedObject.FindProperty("chestLengthMult");
		neckRadiusMult 			= serializedObject.FindProperty("neckRadiusMult");
		headDepthMult 			= serializedObject.FindProperty("headDepthMult");
		headWidthMult 			= serializedObject.FindProperty("headWidthMult");
		headLengthMult 			= serializedObject.FindProperty("headLengthMult");
		shoulderRadiusMult 		= serializedObject.FindProperty("shoulderRadiusMult");
		elbowRadiusMult 		= serializedObject.FindProperty("elbowRadiusMult");
		handDepthMult 			= serializedObject.FindProperty("handDepthMult");
		handWidthMult 			= serializedObject.FindProperty("handWidthMult");
		handLengthMult 			= serializedObject.FindProperty("handLengthMult");
		fingerRadiusMult		= serializedObject.FindProperty("fingerRadiusMult");
		fingerLengthMult		= serializedObject.FindProperty("fingerLengthMult");
		fingerTaperValue		= serializedObject.FindProperty("fingerTaperValue");
		thumbRadiusMult		    = serializedObject.FindProperty("thumbRadiusMult");
		thumbLengthMult		    = serializedObject.FindProperty("thumbLengthMult");
		thumbTaperValue		    = serializedObject.FindProperty("thumbTaperValue");
		thighRadiusMult 		= serializedObject.FindProperty("thighRadiusMult");
		shinRadiusMult 			= serializedObject.FindProperty("shinRadiusMult");
		footDepthMult 			= serializedObject.FindProperty("footDepthMult");
		footWidthMult 			= serializedObject.FindProperty("footWidthMult");
		footLengthMult 			= serializedObject.FindProperty("footLengthMult");

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
		 
		EditorGUILayout.PropertyField(bodyTrackingDevice, new GUIContent("Body Tracking Device", "The source device for avatar body tracking.")); 

		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect1SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID) 
		{
			EditorGUILayout.PropertyField(switchToAvailableKinect, new GUIContent(  "Switch To Available Kinect", "When enabled, RUIS InputManager "
																				+ "settings are read upon application start, and \"Body Tracking "
																				+ "Device\" is switched from Kinect 1 to Kinect 2 in run-time if "
																				+ "the latter is enabled but the former is not, and vice versa."));
		}

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

		EditorGUILayout.Slider(minimumConfidenceToUpdate, 0, 1, new GUIContent("Min Confidence to Update", "The minimum confidence in joint "
																		+ "positions and rotations needed to update these values. The confidence is either "
																		+ "0; 0,5; or 1. This setting is only relevant when using Kinect tracking, or when "
																		+ "using a script that modifies the joint tracking confidence values in real-time."));

		if(!EditorApplication.isPlaying)
			GUI.color = keepPlayModeChangesRGB;

		SwitchToKeepChangesFieldColor();
		EditorGUILayout.PropertyField(keepPlayModeChanges, new GUIContent("Keep PlayMode Changes", "Any changes made in PlayMode will persist "
														+ "for those variables that are marked with RGB(" + keepPlayModeChangesRGB.r + ", " 
														+ keepPlayModeChangesRGB.g + ", " + keepPlayModeChangesRGB.b  +  ") when exiting PlayMode.\n"
		                                                + "PlayMode changes will persist only if the GameObject with this script is not deleted! "
		                                                + "PlayMode changes will NOT persist if made in one scene, and the PlayMode exit occurs in a "
		                                                + "new scene loaded in run-time, even if that scene is the same where the changes were "
		                                          		+ "originally made."));

		if(!EditorApplication.isPlaying)
			GUI.color = normalGUIColor;
		
		RUISEditorUtility.HorizontalRuler();

		SwitchToNormalFieldColor();
        EditorGUILayout.PropertyField(useHierarchicalModel, new GUIContent(  "Hierarchical Model", "Is the model rig hierarchical (ẗhe bone Transforms form "
																		+ "a deep tree structure in the Hierarchy window) or non-hierarchical (all bone "
																		+ "Transforms are on same level)? In almost all cases this option should be "
																		+ "enabled."));

		SwitchToKeepChangesFieldColor();

		/*
		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(boneLengthAxis, new GUIContent( "Bone Length Axis", "Determines the axis that points the bone direction in each " 
																	+ "joint transform of the animation rig. This value depends on your rig. You can "
																	+ "discover the correct axis by examining the animation rig hierarchy, by looking "
																	+ "at the directional axis between parent joints and their child joints in local "
																	+ "coordinate system. IMPORTANT: Disable the below \"Length Only\" scaling option "
																	+ "if the same localScale axis is not consistently used in all the joints of the "
																	+ "animation rig."));
		EditorGUI.indentLevel--;
		GUI.enabled = true;
		*/

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(updateRootPosition, new GUIContent(  "Update Root Position", "Update the position of this GameObject according "
		                                                                 + "to the skeleton root position"));

		GUI.enabled = updateRootPosition.boolValue;
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(rootSpeedScaling, new GUIContent(  "Root Speed Scaling", "Multiply skeleton root position, making the avatar move "
		                                                               + "larger distances than mocap tracking area allows. This is not propagated to "
		                                                               + "Skeleton Wands or other devices (e.g. head trackers) even if they are calibrated "
		                                                               + "with mocap system's coordinate frame. Default and recommended value is (1,1,1)."));
		EditorGUI.indentLevel--;

		GUI.enabled = true;
		EditorGUILayout.PropertyField(updateJointPositions, new GUIContent(  "Update Joint Positions", "Place Joint Positions according to the "
																			+ "motion capture input. If \"Scale Body\" is enabled, then the "
																			+ "tracked positions are not explicitly assigned, rather they come "
																			+ "from the combination of tracked rotations and bone lengths."));
		
		EditorGUI.indentLevel++;


		EditorGUIUtility.labelWidth = ((float) Screen.width) / 2.8f;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));

		EditorGUILayout.PropertyField(forceChestPosition, new GUIContent( "Chest Mocap", "Chest position will be obtained from the \"Body "
																		+ "Tracking Device\" when this option is enabled (adjusting \"Chest "
																		+ "Offset\" might be required). If this option is disabled, then the "
																		+ "Chest position will be determined by the pose and scale of Pelvis. "
																		+ "Enable this option if \"Torso Thickness\" or \"Pelvis Scale Adjust\" "
																		+ "is not set to 1.\nNOTE: Chest rotation is applied regardless whether "
																		+ "this option is enabled or not."));
		EditorGUILayout.PropertyField(forceClaviclePosition, new GUIContent("Clavicle Mocap", "Clavicle positions will be obtained from the "
																		+ "\"Body Tracking Device\" when this option is enabled (adjusting "
																		+ "\"Clavicle Offset\" might be required). If this option is disabled, "
																		+ "then the Clavicle positions will be determined by the pose and scale "
																		+ "of <parent> (Chest or Neck). Enable this option if \"Torso "
																		+ "Thickness\" or \"<parent> Scale Adjust\" is not set to 1.\nNOTE: "
																		+ "Clavicle rotations are applied regardless whether this option is "
																		+ "enabled or not."));

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));

		EditorGUILayout.PropertyField(forceNeckPosition, new GUIContent(  "Neck Mocap", "Neck position will be obtained from the \"Body "
																		+ "Tracking Device\" when this option is enabled (adjusting \"Neck "
																		+ "Offset\" might be required). If this option is disabled, then the "
																		+ "Neck position will be determined by the pose and scale of Chest. "
																		+ "Enable this option if \"Torso Thickness\" or \"Chest Scale Adjust\" "
																		+ "is not set to 1.\nNOTE: If left disabled, this option can be "
																		+ "overriden by \"Neck Pose Interpolation\". Neck rotation is applied "
																		+ "regardless whether this option is enabled or not."));
		EditorGUILayout.PropertyField(forceHeadPosition, new GUIContent(  "Head Mocap", "Head position will be obtained from the \"Body "
																		+ "Tracking Device\" when this option is enabled (adjusting \"Head "
																		+ "Offset\" might be required). If this option is disabled, then the "
																		+ "Head position will be determined by the pose and scale of Neck. "
																		+ "Enable this option if \"Torso Thickness\" or \"Neck Scale Adjust\" is "
																		+ "not set to 1.\nNOTE: If left disabled, this option can be overriden "
																		+ "by \"HMD Moves Head\". Head rotation is applied regardless whether "
																		+ "this option is enabled or not."));

		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

		EditorGUILayout.PropertyField(filterPosition, new GUIContent(  "Filter Positions",   "Smoothen the root, shoulder, and hip positions with "
															+ "a basic Kalman filter. Enabling this option is especially important when using "
															+ "Kinect. Disable this option when using more accurate and responsive mocap systems."));
		if(filterPosition.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(filterHeadPositionOnly, new GUIContent("Filter Only Head Position", "Smoothen ONLY the head position with a basic "
																				+ "Kalman filter, and do not smooth other joints. This is a _hacky_ setting "
																				+ "that adds latency (magnitude controlled by \"Position Smoothness\", try "
																				+ "values up to 10000) into the avatar head positioning. It is for those cases "
																				+ "where \"HMD Moves Head\" is enabled, and the HMD tracking has less latency "
																				+ "than \"Body Tracking Device\", making the body drag behind the head. Even "
																				+ "in those cases enabling this option makes sense only if the avatar can be "
																				+ "seen via a mirror or by other users."));
			
			EditorGUILayout.PropertyField(positionNoiseCovariance, new GUIContent("Position Smoothness", "Sets the magnitude of position smoothing "
																				+ "(measurement noise variance). Larger values makes the movement "
																				+ "smoother, at the expense of responsiveness. Default value is 100."));
			positionNoiseCovariance.floatValue = Mathf.Clamp(positionNoiseCovariance.floatValue, 0.1f, float.MaxValue);

			// HACK: fourJointsNoiseCovariance is usually half of positionNoiseCovariance, but never less than 100 units away
			skeletonController.fourJointsNoiseCovariance = Mathf.Max(0.5f*positionNoiseCovariance.floatValue, positionNoiseCovariance.floatValue - 100);
			EditorGUI.indentLevel--;
		}
					
		EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(updateJointRotations, new GUIContent(  "Update Joint Rotations", "Joint Rotations will be updated according to "
																									 + "the motion capture input."));

		EditorGUI.indentLevel++;

		GUI.enabled = updateJointRotations.boolValue;
		EditorGUILayout.PropertyField(maxAngularVelocity, new GUIContent( "Max Joint Angular Velocity", "Maximum angular velocity for all joints (except "
																		+ "fingers). This value can be used for damping avatar joint movement (smaller values). "
																		+ "The values are in degrees. For Kinect and similar devices, a value of 360 is "
																		+ "suitable. For more accurate and responsive mocap systems, this value can easily "
																		+ "be set to 7200 or more, so that very fast motions are not restricted."));
		GUI.enabled = true;

		EditorGUILayout.PropertyField(maxFingerAngularVelocity, new GUIContent("Max Finger Angular Velocity", "Maximum angular velocity for finger joints, "
																		+ "which can be used for damping finger movement (smaller values). The values "
																		+ "are in degrees. For noisy and inaccurate finger tracking systems, a value of "
																		+ "360 is suitable. For more accurate and responsive mocap systems, this value "
																		+ "can easily be set to 7200 or more, so that very fast motions are not restricted."));

		EditorGUILayout.PropertyField(filterRotations, new GUIContent("Filter Rotations",   "Smoothen rotations with a basic Kalman filter. For now this is "
		                                                              						+ "only done for the arm joints."));
		if(filterRotations.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(rotationNoiseCovariance, new GUIContent("Rotation Smoothness", "Sets the magnitude of rotation smoothing "
																+ "(measurement noise variance). Larger values make the rotation smoother, "
																+ "at the expense of responsiveness. Default value for Kinect is 500. Use smaller "
																+ "values for more accurate and responsive mocap systems."));


			EditorGUILayout.PropertyField(customMocapFrameRate, new GUIContent("Updates Per Second", "How many times per second is the \"Body Tracking "
																	+ "Device\" providing updates (on average)? This determines the Kalman filter update " 
																	+ "interval. For Kinect the value should be 30."));
			customMocapFrameRate.intValue = Mathf.Clamp(customMocapFrameRate.intValue, 1, int.MaxValue);
			skeletonController.customMocapUpdateInterval = 1.0f / ((float) customMocapFrameRate.intValue);

			EditorGUI.indentLevel--;
		}

		if(   Application.isEditor && skeletonController && skeletonController.skeletonManager 
		   && skeletonController.skeletonManager.skeletons[skeletonController.BodyTrackingDeviceID, skeletonController.playerId] != null)
		{
			skeletonController.skeletonManager.skeletons[skeletonController.BodyTrackingDeviceID, skeletonController.playerId].filterRotations = filterRotations.boolValue;
			skeletonController.skeletonManager.skeletons[skeletonController.BodyTrackingDeviceID, skeletonController.playerId].rotationNoiseCovariance = 
																																rotationNoiseCovariance.floatValue;
		}

		EditorGUI.indentLevel--;

//        if(!useHierarchicalModel.boolValue)
//		{
//			scaleHierarchicalModelBones.boolValue = false;
//		}
//		if(!scaleHierarchicalModelBones.boolValue)
//			scaleBoneLengthOnly.boolValue = false;

//		EditorGUILayout.PropertyField(customHMDSource, new GUIContent("Custom HMD", "Leave this field empty if you want to use head-mounted displays that are "
//											+ "natively supported by Unity (e.g. Oculus Rift, Vive, OpenVR headsets...). If you are using any other HMDs with "
//											+ "tracking, link this field to the Transform whose position and rotation is controlled by the HMD tracking."));

		EditorGUILayout.PropertyField(headsetCoordinates, new GUIContent("HMD Coordinate Frame", "If you are using the head-mounted display tracking for "
																		+ "rotating or positioning the avatar head, or for \"HMD Drags Body\", then "
																		+ "you need to set this field to the correct HMD coordinate frame that is "
																		+ "calibrated with the \"Body Tracking Device\" coordinate frame. In most cases this "
																		+ "option should be set to 'OpenVR' or 'UnityXR'. If \"HMD Drags Body\" "
																		+ "is enabled, consider setting this field to 'None' so that there are no additional "
																		+ "offsets in the avatar root."));
		EditorGUI.indentLevel++;

		EditorGUILayout.PropertyField(headsetDragsBody, new GUIContent("HMD Drags Body", "Moves the whole avatar in real-time so that its head is co-located "
																	+ "with the tracked position of the head-mounted display. Enable this option if the user is "
																	+ "wearing a position tracked head-mounted display and their body is tracked with an IMU "
																	+ "suit (e.g. Perception Neuron, Xsens). Such suits measure relative joint rotations, and "
																	+ "can only roughly estimate joint positions.\nThis option can also be enabled if the "
																	+ "coordinate alignment (calibration) between the HMD and the \"Body Tracking Device\" (e.g. "
																	+ "Kinect) is noticeably off, or if the \"Body Tracking Device\" has considerably more latency "
																	+ "when compared to the HMD tracking. In the latter case the avatar's feet can temporarily "
																	+ "disappear under the virtual floor when crouching quickly, and the avatar will also "
																	+ "'glide' unnaturally when the user sways their head quickly.\nYou can use \"HMD Local "
																	+ "Offset\" to adjust how the HMD view is positioned with relation to the avatar head "
																	+ "model. When this option is enabled, it is also good to enable \"HMD Rotates Head\"."));

		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(yawCorrectIMU, new GUIContent(  "IMU Yaw Correct", "Enable this option only when using a head-mounted display, and the "
																	+ "avatar is animated with an IMU suit (e.g. Perception Neuron, Xsens). This will slowly "
																	+ "rotate the whole body of the avatar around up-axis so that the avatar's head is aligned "
																	+ "with the head-mounted display, which effectively applies drift correction to the IMU "
																	+ "suit yaw drift."));

		EditorGUI.indentLevel++;
		GUI.enabled = yawCorrectIMU.boolValue;
		EditorGUILayout.PropertyField(yawCorrectAngularVelocity, new GUIContent("Max Angular Velocity", "The rate at which yaw drift correction is applied "
																									  + "(degrees per second)."));

		SwitchToNormalFieldColor();
		EditorGUILayout.PropertyField(yawCorrectResetButton, new GUIContent("Reset Yaw Button", "The button that can be used to immediately apply the drift "
																	+ "correction, if the head-mounted display and IMU head-tracker were misaligned upon "
																	+ "running the Awake() of this component (e.g. during loading a scene). Pressing this "
																	+ "button only has effect when the Game window is active."));
		SwitchToKeepChangesFieldColor();

		GUI.enabled = true;
		EditorGUI.indentLevel--;

		EditorGUI.indentLevel--;

		EditorGUILayout.PropertyField(hmdRotatesHead, new GUIContent( "HMD Rotates Head", "Rotate avatar head using orientation from the connected "
																	+ "head-mounted display. NOTE: The " + skeletonController.bodyTrackingDevice 
																	+ " coordinate system " 
																	+ ((bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)? ("(currently "
																	+ "set to " + customConversionType.enumDisplayNames[customConversionType.enumValueIndex]
																	+ " in the below \"Custom Mocap Source Transforms\" section) "):"") + "must be calibrated "
																	+ "or otherwise aligned with the \"HMD Coordinate Frame\"!" 
																	+ ((bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)? 
																	  (" Alternatively the above \"IMU Yaw Correct\" option must be enabled if you are using "
																	+ "an IMU suit (e.g. Perception Neuron, Xsens) as the \"Body Tracking Device\"."):"")));

		EditorGUILayout.PropertyField(hmdMovesHead, new GUIContent("HMD Moves Head", "Make avatar head follow the position tracking of the connected " 
																+ "head-mounted display. NOTE: The " + skeletonController.bodyTrackingDevice + " coordinate "
																+ "system " + ((bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)? 
																	("(currently set to " 
																+ customConversionType.enumDisplayNames[customConversionType.enumValueIndex] + " in the below "
																+ "\"Custom Mocap Source Transforms\" section) "):"") +  "must be aligned (calibrated) with "
																+ "the \"HMD Coordinate Frame\"! Inaccuracies in the calibration will make the head-body "
																+ "placement look weird."));

		EditorGUILayout.PropertyField(neckInterpolate, new GUIContent("Neck Pose Interpolation", "Avatar's neck position will be interpolated between chest "
																	+ "and head position, while its rotation will point towards the head position. This means "
																	+ "that any neck pose input from the \"Body Tracking Device\" will be ignored completely.\n"
																	+ "Enabling this option is recommended when \"HMD Moves Head\" is enabled, especially if "
																	+ "the \"Body Tracking Device\" has noticeably more latency than the head-mounted display "
																	+ "position tracking, or if the coordinate system alignment (calibration) between the two "
																	+ "two tracking systems is inaccurate. NOTE: The interpolated pose is affected by the "
																	+ "below \"Neck Offset\" value. Moreover, the interpolated neck position affects chest "
																	+ "scaling if below \"Scale Body\" and \"Torso Segments\" options are enabled."));
		if(neckInterpolate.boolValue)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.Slider(neckInterpolateBlend, 0.01f, 0.99f, new GUIContent("Interpolation Value", "Position of neck will be interpolated from chest "
																					+ "root (0) to head position (1)."));
			EditorGUI.indentLevel--;
		}

		GUI.enabled = (hmdMovesHead.boolValue || headsetDragsBody.boolValue);
		EditorGUILayout.PropertyField(hmdLocalOffset, new GUIContent("HMD Local Offset", "Offset the avatar head in the head-mounted display's local coordinate "
																+ "system. This is useful for positioning the HMD view with the avatar's eyes, which also "
																+ "improves the avatar's head positioning with relation to user's neck. If \"HMD Drags "
																+ "Body\" option is enabled, then the whole avatar position is offset from the HMD "
																+ "position; in this case, make sure that \"HMD Root Offset\" is set to zero if you want "
																+ "the user's head position to match the avatar's head position perfectly."));

		GUI.enabled = true;

//		EditorGUILayout.PropertyField(rootOffset, new GUIContent("HMD Root Offset", "This offset is applied only when the \"Body Tracking Device\" is not "
//																+ "available and the avatar follows the head-mounted display position. "
//																+ "The offset is useful if your view in those "
//																+ "situations appears to be in a wrong place inside the avatar 3D model "));

		EditorGUI.indentLevel--;


		RUISEditorUtility.HorizontalRuler();
		EditorGUILayout.LabelField("  Body Scaling and Offsets", boldItalicStyle);

		SwitchToKeepChangesFieldColor();
		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.PropertyField(scaleHierarchicalModelBones, new GUIContent("Scale Body", "Enabling this option allows the avatar body to be scaled. If "
																				+ "no other scaling options are enabled except this, then only the avatar " 
																				+ "torso is uniformly scaled based on the real-life torso length of the "
																				+ "tracked person. This makes the avatar torso size correspond to the tracked "
																				+ "person size. This option is only available for hierarchical models."));

		EditorGUI.indentLevel++;

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.PropertyField(maxScaleFactor, new GUIContent( "Max Scale Rate", "The maximum rate of change for the local scale of each bone (body "
																	+ "segment). This value limits the body proportions' rate of change when the body " 
																	+ "segment lengths detected by the \"Body Tracking Device\" are not constant. With "
																	+ "noisy bone length detection (e.g. Kinect), a smaller value like 0.1 is good.\nYou can "
																	+ "also make a script that sets this value to 0 when the body proportions have \"settled\" "
																	+ "according to some criteria. The unit is [unitless] per second."));

		EditorGUILayout.PropertyField(independentTorsoSegmentsScaling, new GUIContent("Torso Segments", "Apply uniform scaling to individual "
																					+ "torso segments (abdomen and chest) to resolve segment proportion " 
																					+ "differences between the user and avatar model. If you leave this "
																					+ "disabled, then translations between segments and a single scale "
																					+ "accross the whole torso are used to resolve the differences. Enabling "
																					+ "this option is useful when using more accurate mocap systems than "
																					+ "Kinect."));

		EditorGUI.indentLevel++;
		GUI.enabled = independentTorsoSegmentsScaling.boolValue && useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;

		EditorGUILayout.PropertyField(scalingNeck, new GUIContent("Neck Scaling", "Uniformly scale the neck bone in real-time based on the ratio between "
		                                                        	+ "the detected neck-head distance and the original avatar model neck bone length. "
		                                                        	+ "Usually it is best to keep this option disabled."));
		EditorGUILayout.PropertyField(scalingClavicles, new GUIContent("Clavicle Scaling", "Uniformly scale the left and right clavicle bones in real-time "
		                                                            + "based on the ratio between the detected clavicle-shoulder distance and the original "
		                                                            + "avatar model clavicle bone length.\nNOTE: This option has no effect when using Kinect, "
		                                                            + "or if the Left/Right Clavicle Sources are not set in the \"Custom Mocap Source "
		                                                            + "Transforms\" section. Usually it is best to keep this option disabled."));
		
		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUI.indentLevel--;

		EditorGUILayout.Slider(torsoThickness, minThickness, maxThickness, new GUIContent("Torso Thickness", "Uniform scale that gets applied to the torso. "
																						+ "This does not affect joint positions."));
		EditorGUILayout.PropertyField(limbsAreScaled, new GUIContent( "Scale Limbs", "In most cases you should enable this option. The limbs will be "
																	+ "scaled so that their proportions match to that of the tracked user. If this "
																	+ "option is disabled, then the bone joints are simply translated (if \"Update "
																	+ "Joint Positions\" is enabled), which will likely result in broken avatars "
																	+ "especially on users who are smaller than the initial avatar model rig."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue && limbsAreScaled.boolValue;

		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(scaleBoneLengthOnly, new GUIContent("Length Only", "Scale the limb length (localScale.x/y/z) but not the "
																		+ "bone thickness (localScale.yz/xz/xy). WARNING: Enabling this option could "
																		+ "lead to peculiar results, depending on the animation rig. At the very least "
																		+ "it leads to non-uniform scaling, for which there are slight mitigations in "
																		+ "the code."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue && limbsAreScaled.boolValue && scaleBoneLengthOnly.boolValue;
		EditorGUILayout.Slider(leftArmThickness, minThickness, maxThickness, new GUIContent("Left Arm Thickness", "Thickness scale for left arm (both upper "
																							+ "arm and forearm) around its Length Axis."));
		EditorGUI.indentLevel++;
		EditorGUILayout.Slider(leftUpperArmThickness, minThickness, maxThickness, new GUIContent("Upper Arm Thickness", "Thickness scale for left upper arm "
																								+ "around its Length Axis."));
		EditorGUILayout.Slider(leftForearmThickness,  minThickness, maxThickness, new GUIContent("Forearm Thickness", "Thickness scale for left forearm "
																								+ "around its Length Axis."));
		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(leftHandScaleAdjust,   minScale, maxScale, new GUIContent("Hand Scale", "Uniformly scales the left hand."));
		EditorGUI.indentLevel--;

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue && limbsAreScaled.boolValue && scaleBoneLengthOnly.boolValue;
		EditorGUILayout.Slider(rightArmThickness, minThickness, maxThickness, new GUIContent("Right Arm Thickness", "Thickness scale for right arm (both upper "
																							+ "arm and forearm) around its Length Axis."));
		EditorGUI.indentLevel++;
		EditorGUILayout.Slider(rightUpperArmThickness, minThickness, maxThickness, new GUIContent("Upper Arm Thickness", "Thickness scale for right upper arm "
																								+ "around its Length Axis."));
		EditorGUILayout.Slider(rightForearmThickness,  minThickness, maxThickness, new GUIContent("Forearm Thickness", "Thickness scale for right forearm "
																								+ "around its Length Axis."));
		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(rightHandScaleAdjust,   minScale, maxScale, new GUIContent("Hand Scale", "Uniformly scales the right hand."));
		EditorGUI.indentLevel--;

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue && limbsAreScaled.boolValue && scaleBoneLengthOnly.boolValue;
		EditorGUILayout.Slider(leftLegThickness, minThickness, maxThickness, new GUIContent("Left Leg Thickness", "Thickness scale for left leg (both thigh "
																							+ "and shin) around its Length Axis."));
		EditorGUI.indentLevel++;
		EditorGUILayout.Slider(leftThighThickness,  minThickness, maxThickness, new GUIContent("Thigh Thickness", "Thickness scale for left thigh around its "
																								+ "Length Axis."));
		EditorGUILayout.Slider(leftShinThickness,  	minThickness, maxThickness, new GUIContent("Shin Thickness", "Thickness scale for left shin around its "
																								+ "Length Axis."));
		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(leftFootScaleAdjust, minScale, maxScale, new GUIContent("Foot Scale", "Uniformly scales the left foot."));
		EditorGUI.indentLevel--;

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue && limbsAreScaled.boolValue && scaleBoneLengthOnly.boolValue;
		EditorGUILayout.Slider(rightLegThickness, minThickness, maxThickness, new GUIContent("Right Leg Thickness", "Thickness scale for right leg (both thigh "
																							+ "and shin) around its Length Axis."));
		EditorGUI.indentLevel++;
		EditorGUILayout.Slider(rightThighThickness,  minThickness, maxThickness, new GUIContent("Thigh Thickness", "Thickness scale for right thigh around its "
																							+ "Length Axis."));
		EditorGUILayout.Slider(rightShinThickness, 	 minThickness, maxThickness, new GUIContent("Shin Thickness", "Thickness scale for right shin around its "
																							+ "Length Axis."));
		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(rightFootScaleAdjust, minScale, maxScale, new GUIContent("Foot Scale", "Uniformly scales the right foot."));
		EditorGUI.indentLevel--;


		EditorGUILayout.Space();


		EditorGUI.indentLevel--;
		EditorGUI.indentLevel--;

		GUI.enabled = useHierarchicalModel.boolValue;
		EditorGUILayout.Slider(forearmLengthRatio, minScale, maxScale, new GUIContent("Forearm Length Adjust", "The forearm length ratio compared to the "
																					+ "tracked forearm length. You can use this to lengthen or shorten "
																					+ "the forearms. Only the length is scaled if \"Length Only\" is "
																					+ "enabled, otherwise uniform scale is applied. An alternative way to "
																					+ "change forearm length is to adjust the \"Hand Offset\" value."));
		EditorGUILayout.Slider(shinLengthRatio,    minScale, maxScale, new GUIContent("Shin Length Adjust", "The shin length ratio compared to the "
																					+ "tracked shin length. You can use this to lengthen or shorten "
																					+ "the shins (lower legs). Only the length is scaled if \"Length "
																					+ "Only\" is enabled, otherwise uniform scale is applied. An alternative "
																					+ "way to change shin length is to adjust the \"Foot Offset\" value."));
		EditorGUILayout.Space();

		GUI.enabled = true;

		EditorGUILayout.PropertyField(heightAffectsOffsets, new GUIContent("Scaled Offsets", "When this option is enabled, then the below position "
																		+ "offsets are scaled according to the detected user height. The scale factor "
																		+ "comes from the user's sitting height (pelvis to head distance), where sitting "
																		+ "height of 1 meter equals to factor of 1. In other words, when this option is "
																		+ "enabled, the below position offsets are proportional to the height of the user."));

		EditorGUILayout.Space();

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(pelvisScaleAdjust, minScale, maxScale, new GUIContent("Pelvis Scale Adjust", "Scales pelvis."));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(pelvisOffset, new GUIContent("   Pelvis Offset", "Offsets pelvis joint position in its local frame in meters."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(chestScaleAdjust, minScale, maxScale, new GUIContent("Chest Scale Adjust", "Scales chest."));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(chestOffset, new GUIContent("   Chest Offset", "Offsets chest joint position in its local frame in meters."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(neckScaleAdjust, minScale, maxScale, new GUIContent("Neck Scale Adjust", "Scales neck."));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(neckOffset, new GUIContent("   Neck Offset", "Offsets neck joint position in its local frame in meters."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(headScaleAdjust, minScale, maxScale, new GUIContent("Head Scale Adjust", "Scales head."));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(headOffset, new GUIContent("   Head Offset", "Offsets head joint position in its local frame in meters."));

		GUI.enabled = useHierarchicalModel.boolValue && scaleHierarchicalModelBones.boolValue;
		EditorGUILayout.Slider(clavicleScaleAdjust, minScale, maxScale, new GUIContent("Clavice Scale Adjust", "Scales clavicles."));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(clavicleOffset, new GUIContent("   Clavicle Offset", "Offsets clavicle joint positions in their local frame in meters."));

		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(shoulderOffset, new GUIContent("Shoulder Offset",   "Offsets shoulder joint positions in their local frame in meters. "
																	+ "WARNING: This also offsets the absolute positions of all the arm joints if \"Scale "
																	+ "Body\" is enabled!"));
		EditorGUILayout.PropertyField(elbowOffset, new GUIContent("Elbow Offset",   "Offsets elbow joint positions in their local frame in meters. If \"Scale "
																+ "Body\" is enabled, then the offset should be non-zero only in the upper arm bone's local "
																+ "length axis direction: this changes the proportional length between upper arms and forearms "
																+ "(offset along other axes does not move the elbow joints in the excepted manner, but scales "
																+ "up the length of upper arms and forearms)."));
		EditorGUILayout.PropertyField(handOffset, new GUIContent("Hand Offset", "Offsets hand joint positions in their local frame in meters. If \"Scale "
																+ "Body\" is enabled, then the offset should be non-zero only in the forearm bone's local "
																+ "length axis direction: this changes the length of forearms (offset along other axes does "
																+ "not move the hand joints in the excepted manner, but scales up the length of forearms)."));
		EditorGUILayout.PropertyField(hipOffset, new GUIContent("Hip Offset",	"Offsets hip joint positions in their local frame in meters. WARNING: This "
																+ "also offsets the absolute positions of all the leg joints if \"Scale Body\" is enabled!"));
		EditorGUILayout.PropertyField(kneeOffset, new GUIContent("Knee Offset", "Offsets knee joint positions in their local frame in meters. If \"Scale "
																+ "Body\" is enabled, then the offset should be non-zero only in the thigh bone's local "
																+ "length axis direction: this changes the proportional length between thighs and shins "
																+ "(offset along other axes does not move the knee joints in the excepted manner, but scales "
																+ "up the length of thighs and shins)."));
		EditorGUILayout.PropertyField(footOffset, new GUIContent("Foot Offset",   "Offsets foot joint positions in their local frame in meters. If \"Scale "
																+ "Body\" is enabled, then the offset should be non-zero only in the shin bone's local "
																+ "length axis direction: this changes the length of shins (offset along other axes does "
																+ "not move the foot joints in the excepted manner, but scales up the length of shins)."));
		EditorGUILayout.Space();

		GUI.enabled = true;
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

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(thumbRotationOffset,   new GUIContent("Thumb (Rot)", "Offsets the thumb CMC joint (thumb root) rotation in the "
																						+ "local hand coordinate frame (Euler angles)."));
			EditorGUILayout.PropertyField(indexFRotationOffset,  new GUIContent("Index Finger (Rot)", "Offsets the index finger MCP joint (finger root) "
																						+ "rotation in the local hand coordinate frame (Euler angles)."));
			EditorGUILayout.PropertyField(middleFRotationOffset, new GUIContent("Middle Finger (Rot)", "Offsets the middle finger MCP joint (finger root) "
																						+ "rotation in the local hand coordinate frame (Euler angles)."));
			EditorGUILayout.PropertyField(ringFRotationOffset,   new GUIContent("Ring Finger (Rot)", "Offsets the ring finger MCP joint (finger root) "
																						+ "rotation in the local hand coordinate frame (Euler angles)."));
			EditorGUILayout.PropertyField(littleFRotationOffset, new GUIContent("Little Finger (Rot)", "Offsets the little finger MCP joint (finger root) "
																						+ "rotation in the local hand coordinate frame (Euler angles)."));

			EditorGUI.indentLevel -= 1;
		}

		//		EditorGUILayout.Slider(handScaleAdjust, minScale, maxScale, new GUIContent("Hand Scale Adjust", "Scales hands. This setting has effect only when "
		//																				+ "\"Hierarchical Model\" and \"Scale Body\" are enabled."));
		//
		//		EditorGUILayout.Slider(footScaleAdjust, minScale, maxScale, new GUIContent("Foot Scale Adjust", "Scales feet. This setting has effect only when "
		//																				+ "\"Hierarchical Model\" and \"Scale Body\" are enabled."));


		GUI.enabled = true;
		SwitchToNormalFieldColor();
		
		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID) 
		{
			RUISEditorUtility.HorizontalRuler();

			if(!normalLabelColorWasSaved)
			{
				normalLabelColor = EditorStyles.label.normal.textColor;
				normalLabelColorWasSaved = true;
			}

			SwitchToNormalFieldColor();
			EditorGUILayout.LabelField("  Custom Mocap Source Transforms", coloredBoldItalicStyle);

			EditorStyles.label.normal.textColor = customLabelColor;

			EditorGUILayout.PropertyField(customConversionType, new GUIContent("Coordinate Frame and Conversion", "The coordinate frame (coordinate system) "
																	+ "of the custom body tracking device. If you are NOT using an IMU suit (e.g. "
																	+ "Perception Neuron, Xsens) as a custom body tracking device, then this coordinate "
																	+ "frame should be aligned (calibrated) with any other devices that you will use "
																	+ "simultaneously (e.g. head-mounted display).\nThis setting also defines the "
																	+ "coordinate conversion that will be applied to the Source Transform poses before "
																	+ "copying them to Target Transforms (further below). The conversions are defined in " 
																	+ typeof(RUISInputManager) + "'s \"Custom 1\" and \"Custom 2\" settings, and also "
																	+ "stored in the associated 'inputConfig.xml'-file."));

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(customParent, new GUIContent("Parent Transform", "Optional: This Transform should be parent to the intended "
				 													+ "\"Custom Mocap Source Transforms\" that will get their position and rotation from "
																	+ "the mocap system. When the Parent Transform is assigned, you can press the below "
																	+ "\"Obtain Sources by Name\" button."));

			GUI.enabled = customParent.objectReferenceValue != null;
			if(GUILayout.Button(new GUIContent("Obtain Sources by Name", "Attempt to automatically obtain the below (blue) Source Transforms from the "
																	+ "above Parent Transform's child Transforms, by matching the child names with " 
																	+ "the Source Transform field labels: joint name synonyms are recognized, letter case "
																	+ "does not matter, and left and right joints should be denoted with a handedness "
																	+ "prefix \"left\", \"l_\", \"l-\", or \"right\", \"r_\", \"r-\", respectively. "
																	+ "WARNING: Make sure that the obtained Transforms are correct!")))
			{
				if(skeletonController && customParent.objectReferenceValue)
				{
					List<string>[] sourceReport = new List<string>[3];
					sourceReport[0] = new List<string>(); // found customSources (individual names)
					sourceReport[1] = new List<string>(); // not found customSources (individual names)
					sourceReport[2] = new List<string>(); // multiple candidates (full sentences)
					List<Transform> alreadyAssigned = new List<Transform>();

					RUISSkeletonControllerCustomSources.ExtractSkeletonNamePrefix((Transform) customParent.objectReferenceValue);

					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.Root, null, 
																			null, alreadyAssigned, new string[] {"references"}, ref sourceReport, customRoot);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.Torso, null, 
																			null, alreadyAssigned, null, ref sourceReport, customTorso);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.Chest, null, 
																			null, alreadyAssigned, null, ref sourceReport, customChest);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.Neck, null, 
																			null, alreadyAssigned, null, ref sourceReport, customNeck);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.Head, null, 
																			null, alreadyAssigned, null, ref sourceReport, customHead);

					if(RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftClavicle, null, 
																			   RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftClavicle))
						RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftShoulder, "arm", 
																				RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned,
																				RUISSkeletonControllerCustomSources.foreArmVariations, ref sourceReport, customLeftShoulder);
					else
					{
						// Check for evidence that clavicles are named as shoulders
						if(RUISSkeletonControllerCustomSources.TripletExists((Transform) customParent.objectReferenceValue, RUISSkeletonControllerCustomSources.armTripletNames, 
																			 RUISSkeletonControllerCustomSources.armTripletExcludeNames											))
						{
							RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftClavicle, "shoulder",
																					RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftClavicle);
							if(sourceReport.Length > 1)  // There has now been two attempts to find LeftClavicle: remove one of the "not found" reports
								sourceReport[1].Remove(RUISSkeletonManager.Joint.LeftClavicle.ToString());
						}	
						RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftShoulder, "arm", 
																				RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned,
																				new string[] {"shoulder", "forearm", "fore_arm", "fore-arm", "fore arm", "lowerarm", "lower_arm", "lower-arm",
																							  "lower arm", "lowarm", "low_arm", "low-arm", "low arm"}, ref sourceReport, customLeftShoulder);
					}

					if(RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightClavicle, null, 
																			   RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightClavicle))
						RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightShoulder, "arm", 
																				RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned,
																				RUISSkeletonControllerCustomSources.foreArmVariations, ref sourceReport, customRightShoulder);
					else
					{
						// Check for evidence that clavicles are named as shoulders
						if(RUISSkeletonControllerCustomSources.TripletExists((Transform) customParent.objectReferenceValue, RUISSkeletonControllerCustomSources.armTripletNames, 
																			 RUISSkeletonControllerCustomSources.armTripletExcludeNames											))
						{
							RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightClavicle, "shoulder",
																					RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightClavicle);
							if(sourceReport.Length > 1) // There has now been two attempts to find RightClavicle: remove one of the "not found" reports
								sourceReport[1].Remove(RUISSkeletonManager.Joint.RightClavicle.ToString());
						}
						RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightShoulder, "arm", 
																				RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned,
																				new string[] {"shoulder", "forearm", "fore_arm", "fore-arm", "fore arm", "lowerarm", "lower_arm", "lower-arm",
																							  "lower arm", "lowarm", "low_arm", "low-arm", "low arm"}, ref sourceReport, customRightShoulder);
					}

					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftElbow, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftElbow);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightElbow, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightElbow);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftHand, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftHand);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightHand, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightHand);

					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftHip, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, new string[] {"hips"}, ref sourceReport, customLeftHip);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightHip, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, new string[] {"hips"}, ref sourceReport, customRightHip);

					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftKnee, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned,
																			RUISSkeletonControllerCustomSources.upperLegVariations, ref sourceReport, customLeftKnee);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightKnee, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.upperLegVariations, ref sourceReport, customRightKnee);

					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftFoot, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftFoot);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightFoot, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightFoot);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftThumb, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, null, ref sourceReport, customLeftThumb);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightThumb, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, null, ref sourceReport, customRightThumb);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftIndexFinger, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customLeftIndexF);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightIndexFinger, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customRightIndexF);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftMiddleFinger, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customLeftMiddleF);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightMiddleFinger, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customRightMiddleF);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftRingFinger, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customLeftRingF);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightRingFinger, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customRightRingF);
					
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.LeftLittleFinger, null, 
																			RUISSkeletonControllerCustomSources.leftStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customLeftLittleF);
					RUISSkeletonControllerCustomSources.FindSourceTransform((Transform) customParent.objectReferenceValue, RUISSkeletonManager.Joint.RightLittleFinger, null, 
																			RUISSkeletonControllerCustomSources.rightStrings, alreadyAssigned, 
																			RUISSkeletonControllerCustomSources.neuronExceptions, ref sourceReport, customRightLittleF);
		
					if(sourceReport.Length > 0)
					{
						string fullSourceReport = "Obtained by name " + sourceReport[0].Count;
						if(sourceReport.Length > 1)
							fullSourceReport = fullSourceReport + " out of " + (sourceReport[0].Count + sourceReport[1].Count);
						fullSourceReport = fullSourceReport + " Custom Mocap Source Transforms";
						if(sourceReport.Length > 2 && sourceReport[2].Count > 0)
							fullSourceReport = fullSourceReport + ", where " + sourceReport[2].Count + " had multiple candidates";

						obtainedSourceTransforms =    fullSourceReport + " (see Console for details). Please check that they are correct by clicking the below "
													+ "Source Transform fields.";
						obtainedSourceTransforms = obtainedSourceTransforms.Replace(" by name", "");

						fullSourceReport = fullSourceReport + ". Details are listed below.\n";

						if(RUISSkeletonControllerCustomSources.SkeletonNamePrefix != "")
							fullSourceReport = 	  fullSourceReport + "--Detected the prefix \"" + RUISSkeletonControllerCustomSources.SkeletonNamePrefix 
												+ "\" for the Custom Mocap Source Transform names.\n";
					
						for(int i = 0; i < sourceReport.Length; ++i)
						{
							if(sourceReport[i].Count > 0)
							{
								string appendedNames = "";
								string delimiter = ", ";
								if(i == 2)
									delimiter = "\n";
								foreach(string notFoundName in sourceReport[i])
								{
									if(appendedNames.Length == 0)
										appendedNames += notFoundName;
									else 
										appendedNames += delimiter + notFoundName;
								}
								switch(i)
								{
									case 0:
										fullSourceReport = fullSourceReport + "--Custom Mocap Source Transforms that WERE found:    ( " + appendedNames + " )\n";
										break;
									case 1:
										fullSourceReport = fullSourceReport + "--Custom Mocap Source Transforms that were NOT found:    ( " + appendedNames + " )\n";
										break;
									case 2:
										fullSourceReport = fullSourceReport + "--Custom Mocap Source Transforms with multiple candidates:\n" + appendedNames + "\n";
										break;
								}
							}
						}
						Debug.LogWarning(fullSourceReport);
					}
				}
			}

			if(!string.IsNullOrEmpty(obtainedSourceTransforms))
			{
				GUI.enabled = false;
				EditorStyles.textField.wordWrap = true;
				EditorGUILayout.TextArea(obtainedSourceTransforms);
			}

			GUI.enabled = true;

			EditorGUILayout.Space();

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
			EditorGUILayout.PropertyField(customLeftHand, 		new GUIContent("Left Hand", 	"Optional: The source Transform with tracked pose of the  left hand (wrist). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightClavicle, new GUIContent("Right Clavicle", "Optional: The source Transform with tracked pose of the right clavicle. "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightShoulder, new GUIContent("Right Shoulder", "REQUIRED: The source Transform with tracked pose of the right shoulder (upper arm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightElbow,    new GUIContent("Right Elbow", 	"Optional: The source Transform with tracked pose of the right elbow (forearm). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightHand,	   new GUIContent("Right Hand",	    "Optional: The source Transform with tracked pose of the right hand (wrist). "
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
			EditorGUILayout.PropertyField(customLeftFoot, new GUIContent("Left Foot", "Optional: The source Transform with tracked pose of the left foot (ankle). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(customRightHip,  new GUIContent("Right Hip",	"REQUIRED: The source Transform with tracked pose of the right hip (thigh). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightKnee, new GUIContent("Right Knee", "Optional: The source Transform with tracked pose of the right knee (shin). "
														+ "Its world frame location and rotation will be utilized, so be mindful about parent Transforms."));
			EditorGUILayout.PropertyField(customRightFoot, new GUIContent("Right Foot", "Optional: The source Transform with tracked pose of the right foot (ankle). "
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
				EditorGUILayout.PropertyField(customLeftThumb, new GUIContent ("Left Thumb CMC", "Optional: The source Transform for the 'root' of left thumb, also known "
					+ "as carpometacarpal (CMC) joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftIndexF,  new GUIContent ("Left Index Finger MCP",	"Optional: The source Transform for the 'root' of left index finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftMiddleF, new GUIContent ("Left Middle Finger MCP", "Optional: The source Transform for the 'root' of left middle finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftRingF,   new GUIContent ("Left Ring Finger MCP", "Optional: The source Transform for the 'root' of left ring finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customLeftLittleF, new GUIContent ("Left Little Finger MCP", "Optional: The source Transform for the 'root' of left little finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
				EditorGUILayout.PropertyField(customRightThumb, new GUIContent ("Right Thumb CMC", "Optional: The source Transform for the 'root' of right thumb, also known "
					+ "as carpometacarpal (CMC) joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightIndexF,  new GUIContent ("Right Index Finger MCP", "Optional: The source Transform for the 'root' of right index finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightMiddleF, new GUIContent ("Right Middle Finger MCP", "Optional: The source Transform for the 'root' of right middle finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightRingF,   new GUIContent ("Right Ring Finger MCP", "Optional: The source Transform for the 'root' of right ring finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.PropertyField(customRightLittleF, new GUIContent ("Right Little Finger MCP", "Optional: The source Transform for the 'root' of right little finger, "
					+ "also known as metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
				EditorGUIUtility.labelWidth = 0;
			}

			if(normalLabelColorWasSaved)
				EditorStyles.label.normal.textColor = normalLabelColor;
		}

		RUISEditorUtility.HorizontalRuler();

		SwitchToNormalFieldColor();
		EditorGUILayout.LabelField("  Avatar Target Transforms", boldItalicStyle);

		if(GUILayout.Button(new GUIContent("Obtain Targets from Animator", "Attempt to automatically obtain below Target Transforms "
											+ "from an Animator component, if such a component can be found from this GameObject "
											+ "or its children. WARNING: Make sure that the obtained Transforms are correct!\n"
											+ "All previously assigned Target Transforms will be replaced!")))
		{
			if(skeletonController)
			{
				string consoleReport = "";
				Undo.RegisterFullObjectHierarchyUndo(skeletonController.gameObject, "Obtain Avatar Target Transforms from Animator");
				if(skeletonController.AutoAssignJointTargetsFromAvatar(out obtainedTargetTransforms, out consoleReport))
					Debug.LogWarning(consoleReport);
				else
				{
					obtainedTargetTransforms = "";
					EditorUtility.DisplayDialog("Failed to obtain Avatar Target Transforms", "Could not find an Animator component " 
								+ "in this GameObject or its children. You must assign the Avatar Target Transforms manually.", "OK");
				}
			}
		}

		if(!string.IsNullOrEmpty(obtainedTargetTransforms))
		{
			GUI.enabled = false;
			EditorStyles.textField.wordWrap = true;
			EditorGUILayout.TextArea(obtainedTargetTransforms);
			GUI.enabled = true;
		}

		EditorGUILayout.Space();

		string mocapSource = "Kinect.";
		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
			mocapSource = "the above \"Custom Mocap Source Transforms\".";
		
		EditorGUILayout.PropertyField(rootBone, new GUIContent("Target Root", "REQUIRED: The target Transform that is the animated avatar's root bone in "
																			+ "the skeleton hierarchy. The target Transforms of this section will "
																			+ "be moved by " + mocapSource));
		EditorGUILayout.Space();

        EditorGUILayout.LabelField("Torso and Head Targets", EditorStyles.boldLabel);
		EditorGUILayout.PropertyField(torsoBone, 	new GUIContent("Pelvis", 		"REQUIRED: The pelvis joint, has to be parent or grandparent of all the "
																				  + "other joints except root. Can be same as root."));
		EditorGUILayout.PropertyField(chestBone, 	new GUIContent("Chest", 		"Optional: The chest joint, has to be child or grandchild of pelvis."));
		EditorGUILayout.PropertyField(neckBone, 	new GUIContent("Neck", 			"Optional: The neck joint, has to be child or grandchild of chest."));
		EditorGUILayout.PropertyField(headBone, 	new GUIContent("Head", 			"REQUIRED: The head joint, has to be child or grandchild of neck."));

		EditorGUILayout.Space();
		
		EditorGUILayout.LabelField("Arm Targets", EditorStyles.boldLabel);
		EditorGUIUtility.labelWidth = Screen.width / 6;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(leftClavicle, 	 new GUIContent("Left Clavicle",   "Optional: The left clavicle joint, "
		                                                                       			 + "has to be child or grandchild of neck or chest."));
		EditorGUILayout.PropertyField(leftShoulderBone,  new GUIContent("Left Shoulder",   "REQUIRED: The left shoulder joint (upper arm), "
																						 + "has to be child or grandchild of left clavicle, neck, or chest."));
		EditorGUILayout.PropertyField(leftElbowBone, 	 new GUIContent("Left Elbow", 	   "Optional: The left elbow joint (forearm), "
																						 + "has to be child of left shoulder."));
		EditorGUILayout.PropertyField(leftHandBone, 	 new GUIContent("Left Hand", 	   "Optional: The left hand joint (wrist), "
																						 + "has to be child of left elbow."));

		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(rightClavicle, 	 new GUIContent("Right Clavicle",   "Optional: The right clavicle joint, "
																						  + "has to be child or grandchild of neck or chest."));
		EditorGUILayout.PropertyField(rightShoulderBone, new GUIContent("Right Shoulder",	"REQUIRED: The right shoulder joint (upper arm), "
																						  + "has to be child or grandchild of right clavicle, neck, or chest."));
		EditorGUILayout.PropertyField(rightElbowBone, 	 new GUIContent("Right Elbow",		"Optional: The right elbow joint (forearm), "
																						  + "has to be child of right shoulder."));
		EditorGUILayout.PropertyField(rightHandBone,	 new GUIContent("Right Hand",		"Optional: The right hand joint (wrist), "
																						  + "has to be child of right elbow."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

//		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
		{	
			SwitchToKeepChangesFieldColor();
			EditorGUILayout.PropertyField(trackWrist, new GUIContent("Track Hand Rotation", "Track the rotation of the hand (wrist). You might want to "
												+ "disable this when using Kinect 2 or some other mocap system with poor hand (wrist) rotation tracking."));
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
		EditorGUILayout.PropertyField(leftHipBone,   new GUIContent("Left Hip",	   "REQUIRED: The left hip joint (thigh), "
																				 + "has to be child or grandchild of pelvis."));
		EditorGUILayout.PropertyField(leftKneeBone,  new GUIContent("Left Knee",   "Optional: The left knee joint (shin), has to be child of left hip."));
		EditorGUILayout.PropertyField(leftFootBone,  new GUIContent("Left Foot",   "Optional: The left foot joint (ankle), has to be child of left knee."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
		EditorGUILayout.PropertyField(rightHipBone,  new GUIContent("Right Hip", 	"REQUIRED: The right hip joint (thigh), "
																				  + "has to be child or grandchild of pelvis."));
		EditorGUILayout.PropertyField(rightKneeBone, new GUIContent("Right Knee", 	"Optional: The right knee joint (shin), has to be child of right hip."));
		EditorGUILayout.PropertyField(rightFootBone, new GUIContent("Right Foot", 	"Optional: The right foot joint (ankle), has to be child of right knee."));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

//		if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID || bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.customSensorID)
		{
			SwitchToKeepChangesFieldColor();
			EditorGUILayout.PropertyField(trackAnkle, new GUIContent("Track Foot Rotation", "Track the rotation of the foot (ankle). You might want to "
												+ "disable this when using Kinect 2 or some other mocap system with poor foot (ankle) rotation tracking."));
			SwitchToNormalFieldColor();
		}

		EditorGUILayout.Space();

		showTargetFingers = EditorGUILayout.Foldout(showTargetFingers, "Finger Targets", true, boldFoldoutStyle);
		if(showTargetFingers)
		{
			EditorGUIUtility.labelWidth = Screen.width / 6;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(leftThumb,   new GUIContent ("Left Thumb CMC", "Optional: The 'root' of left thumb, also known as carpometacarpal (CMC) "
																+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
			EditorGUILayout.PropertyField(leftIndexF,  new GUIContent ("Left Index Finger MCP",	"Optional: The 'root' of left index finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftMiddleF, new GUIContent ("Left Middle Finger MCP", "Optional: The 'root' of left middle finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftRingF,   new GUIContent ("Left Ring Finger MCP", "Optional: The 'root' of left ring finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(leftLittleF, new GUIContent ("Left Little Finger MCP", "Optional: The 'root' of left little finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(GUILayout.Width (Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(rightThumb,   new GUIContent ("Right Thumb CMC", "Optional: The 'root' of right thumb, also known as carpometacarpal (CMC) "
																+ "joint. The remaining MCP and IP joints of the thumb will be assigned automatically."));
			EditorGUILayout.PropertyField(rightIndexF,  new GUIContent ("Right Index Finger MCP", "Optional: The 'root' of right index finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightMiddleF, new GUIContent ("Right Middle Finger MCP", "Optional: The 'root' of right middle finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightRingF,   new GUIContent ("Right Ring Finger MCP", "Optional: The 'root' of right ring finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.PropertyField(rightLittleF, new GUIContent ("Right Little Finger MCP", "Optional: The 'root' of right little finger, also known as "
									+ "metacarpophalangeal (MCP) joint. The remaining PIP and DIP joints of the finger will be assigned automatically."));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUIUtility.labelWidth = 0;

			EditorGUI.indentLevel++;

			SwitchToKeepChangesFieldColor();
			EditorGUILayout.PropertyField(fistMaking, new GUIContent("Fist Clench Animation", "Disable this option when you are using \"Custom Mocap Source "
																		+ "Transforms\" for posing avatar fingers!\nWhen this option is enabled, Kinect 2's "
																		+ "fist gesture recognition is utilized to pose the fingers to either the initial "
																		+ "finger pose or the fist pose (defined by below angles). As an alternative to "
																		+ "Kinect 2 tracking you can set the 'externalFistTrigger' variable to true and use "
																		+ "'externalLeftStatus' and 'externalRightStatus' variables to open and close the "
																		+ "fists. Remember to assign the above \"Left Thumb CMC\" and \"Right Thumb CMC\", "
																		+ "so that the thumb Target Transforms will be identified. For the purposes of finger "
																		+ "clenching, it is not required to assign the other finger CMC Target Transforms "
																		+ "if they are parented directly under \"Left Hand\" and \"Right Hand\" Target "
																		+ "Transforms and their names include the substring 'finger', 'Finger', or "
																		+ "'FINGER'"));

			if(fistMaking.boolValue)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(clenchedThumbAngleTM,  new GUIContent("Clenched Thumb TM (Rot)", "TM joint (thumb root) rotation for clenched "
																				  + "fist. Angles are in degrees."));
				EditorGUILayout.PropertyField(clenchedThumbAngleMCP, new GUIContent("Clenched Thumb MCP (Rot)", "MCP joint (second joint) rotation for clenched "
																				  + "fist. Angles are in degrees."));
				EditorGUILayout.PropertyField(clenchedThumbAngleIP,  new GUIContent("Clenched Thumb IP (Rot)", "IP joint (third joint) rotation for clenched "
																				  + "fist. Angles are in degrees."));
				EditorGUILayout.PropertyField(clenchedFingerAngleMCP, new GUIContent("Clenched Finger MCP (Rot)", "MCP joint (finger root) rotation for "
																		+ "index, middle, ring, and little fingers of clenched fist. Angles are in degrees."));
				EditorGUILayout.PropertyField(clenchedFingerAnglePIP, new GUIContent("Clenched Finger PIP (Rot)", "PIP joint (second joint) rotation for "
																		+ "index, middle, ring, and little fingers of clenched fist. Angles are in degrees."));
				EditorGUILayout.PropertyField(clenchedFingerAngleDIP, new GUIContent("Clenched Finger DIP (Rot)", "DIP joint (third joint) rotation for "
																		+ "index, middle, ring, and little fingers of clenched fist. Angles are in degrees."));
				EditorGUI.indentLevel--;
			}

			if(bodyTrackingDevice.enumValueIndex == RUISSkeletonManager.kinect2SensorID)
			{
				SwitchToNormalFieldColor();
				EditorGUILayout.PropertyField(kinect2Thumbs, new GUIContent("Kinect 2 Thumbs", "This enables Kinect 2 tracking for posing thumbs."));
				SwitchToKeepChangesFieldColor();

				if(kinect2Thumbs.boolValue)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(thumbZRotationOffset, new GUIContent("Z Rotation Offset",   "Offset Z rotation of the thumb. Default value is "
																					 + "45, but it might depend on your avatar rig. This offset is only applied "
																					 + "if Kinect 2 is used for tracking and \"Kinect 2 Thumbs\" is enabled."));
					EditorGUI.indentLevel--;
					if(    Application.isEditor && skeletonController && skeletonController.skeletonManager 
						&& skeletonController.skeletonManager.skeletons[skeletonController.BodyTrackingDeviceID, skeletonController.playerId] != null)
						skeletonController.skeletonManager.skeletons[skeletonController.BodyTrackingDeviceID, skeletonController.playerId].thumbZRotationOffset = 
							thumbZRotationOffset.floatValue;
				}
			}

			EditorGUI.indentLevel--;
		}

		RUISEditorUtility.HorizontalRuler();

		if(GUILayout.Button(new GUIContent("Add Colliders To Body Parts", "")))
		{
			if(skeletonController)
			{
				if(			   !torsoBone.objectReferenceValue || 		   !headBone.objectReferenceValue 
					||  !leftShoulderBone.objectReferenceValue || !rightShoulderBone.objectReferenceValue)
				{
					string missingBones = "";
					if(!torsoBone.objectReferenceValue)
						missingBones += "Pelvis, ";
					if(!headBone.objectReferenceValue)
						missingBones += "Head, ";
					if(!leftShoulderBone.objectReferenceValue)
						missingBones += "Left Shoulder, ";
					if(!rightShoulderBone.objectReferenceValue)
						missingBones += "Right Shoulder, ";
					if(missingBones.Length > 2)
						missingBones = missingBones.Substring(0, missingBones.Length - 2);
					EditorUtility.DisplayDialog(  "Unable to create Colliders", "Failed to add Colliders to body segments, because the following "
												+ "\"Avatar Target Transforms\" are not assigned: " + missingBones + ".", "OK");
				}
				else
				{
					Undo.RegisterFullObjectHierarchyUndo(skeletonController.gameObject, "Add Colliders To Body Parts undo");
					skeletonController.AddCollidersToBodySegments((RUISSkeletonController.AvatarColliderType) avatarCollider.enumValueIndex, 
																  colliderRadius.floatValue, colliderLengthOffset.floatValue, 
																  createFingerColliders.boolValue, pelvisHasBoxCollider.boolValue, 
																  chestHasBoxCollider.boolValue, headHasBoxCollider.boolValue, 
																  handHasBoxCollider.boolValue, footHasBoxCollider.boolValue,
																  fingerHasBoxCollider.boolValue											);
				}
			}
		}

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(avatarCollider, new GUIContent("Base Collider Type", "The Collider type that is used for all body segments."));
		EditorGUILayout.PropertyField(colliderRadius, new GUIContent("Base Radius", "Value in meters that acts as a common radius for Capsule "
															+ "Colliders, and as a common width and depth for Box Colliders (with the exception of "
															+ "Pelvis and Chest widths, which are calculated from the Shoulder and Hip distances). "
															+ "These common dimensions can be adjusted for each body segment by modifying the " 
															+ "multipliers within the below \"Collider Dimension Multipliers\" -section.\n\nModifying "
															+ "this value does not automatically adjust the dimensions of existing Colliders; "
															+ "you need to click the \"Add Colliders To Body Parts\" to see the effects."));
		EditorGUILayout.PropertyField(colliderLengthOffset, new GUIContent("Base Length Offset", "Length offset (meters) that extends the length of "
															+ "Colliders for Neck, Upper Arm, Forearm, Thigh, and Shin. This value can be negative."
															+ "\nThe length of other body segment Colliders can be adjusted by modifying the "
															+ "multipliers within the below \"Collider Dimension Multipliers\" -section.\n\nModifying "
															+ "this value does not automatically adjust the dimensions of existing Colliders; "
															+ "you need to click the \"Add Colliders To Body Parts\" to see the effects."));
		EditorGUILayout.PropertyField(createFingerColliders, new GUIContent("Create Finger Colliders", "If enabled, then Colliders will be created for "
															+ "Finger Target Transforms when pressing the \"Add Colliders To Body Parts\" -button."));

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Override Base Colliders", italicStyle);

		EditorGUIUtility.labelWidth = Screen.width / 2.8f;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 3));
		EditorGUILayout.PropertyField(pelvisHasBoxCollider, new GUIContent("Pelvis Box Collider",  "Force Pelvis (abdomen) to have a Box Collider, "
																								 + "overriding the \"Base Collider Type\" setting."));
		EditorGUILayout.PropertyField(chestHasBoxCollider,  new GUIContent("Chest Box Collider",   "Force Chest to have a Box Collider, "
																								 + "overriding the \"Base Collider Type\" setting."));
		EditorGUILayout.PropertyField(headHasBoxCollider,   new GUIContent("Head Box Collider",    "Force Head to have a Box Collider, "
																								 + "overriding the \"Base Collider Type\" setting."));

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 3));
		EditorGUILayout.PropertyField(handHasBoxCollider,   new GUIContent("Hand Box Colliders",   "Force Hands (palms) to have a Box Colliders, "
																								 + "overriding the \"Base Collider Type\" setting."));
		EditorGUILayout.PropertyField(footHasBoxCollider,   new GUIContent("Foot Box Colliders",   "Force Feet to have a Box Colliders, "
																								 + "overriding the \"Base Collider Type\" setting."));
		EditorGUILayout.PropertyField(fingerHasBoxCollider, new GUIContent("Finger Box Colliders", "Force Fingers (including Thumbs) to have a "
																								 + "Box Colliders, overriding the \"Base Collider "
																								 + "Type\" setting."));
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		EditorGUIUtility.labelWidth = 0;

		EditorGUILayout.Space();

		showColliderAdjustments = EditorGUILayout.Foldout(showColliderAdjustments, "Collider Dimension Multipliers", true, boldFoldoutStyle);
		if(showColliderAdjustments)
		{
			GUI.enabled = false;
			EditorStyles.textField.wordWrap = true;
			EditorGUILayout.TextArea("Changing these values does not automatically modify the existing Colliders. Click the the \"Add Colliders To "
								   + "Body Parts\" -button to see the changes.");
			GUI.enabled = true;
			EditorGUIUtility.labelWidth = Screen.width / 3.6f;
			EditorGUIUtility.fieldWidth = Screen.width / 9.7f;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(pelvisWidthMult, 	  new GUIContent("Pelvis Width", 	 "Pelvis (abdomen) Box Collider's width / Capsule "
																				+ "Collider's radius multiplier."));
			EditorGUILayout.PropertyField(pelvisDepthMult, 	  new GUIContent("Pelvis Depth", 	 "Pelvis (abdomen) Box Collider's depth multiplier. "
																				+ "This setting only has effect if Pelvis has a Box Collider."));
			EditorGUILayout.PropertyField(pelvisLengthMult,   new GUIContent("Pelvis Length", 	 "Pelvis (abdomen) Collider's length multiplier. "));
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));

			EditorGUILayout.PropertyField(chestWidthMult, 	  new GUIContent("Chest Width", 	 "Chest Box Collider's width / Capsule Collider's radius "
																				+ "multiplier."));
			EditorGUILayout.PropertyField(chestDepthMult, 	  new GUIContent("Chest Depth", 	 "Chest Box Collider's depth multiplier. This setting "
																				+ "only has effect if Chest has a Box Collider."));

			EditorGUILayout.PropertyField(chestLengthMult,	  new GUIContent("Chest Length", 	 "Chest Collider's length multiplier. "));
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 6));
			EditorGUILayout.Space(); // HACK to force "content" on this column, otherwise the column width is collapsed
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(neckRadiusMult, 	  new GUIContent("Neck Radius", 	 "Neck Capsule Collider's radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead."));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(headWidthMult, 	  new GUIContent("Head Width", 		 "Head Box Collider's width / Capsule Collider's radius "
																				+ "multiplier."));
			EditorGUILayout.PropertyField(headDepthMult, 	  new GUIContent("Head Depth", 		 "Head Box Collider's depth multiplier. This setting "
																				+ "only has effect if Head has a Box Collider."));
			EditorGUILayout.PropertyField(headLengthMult, 	  new GUIContent("Head Length", 	 "Head Collider's length multiplier."));

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 6));
			EditorGUILayout.Space(); // HACK to force "content" on this column, otherwise the column width is collapsed
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(shoulderRadiusMult, new GUIContent("Upper Arm Radius", "Upper Arm Capsule Collider's radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead."));
			EditorGUILayout.PropertyField(elbowRadiusMult, 	  new GUIContent("Forearm Radius",	 "Forearm Capsule Collider's radius multiplier, which also "
																			 	+ "acts as a width and depth multiplier if Box Collider is used instead."));
			EditorGUILayout.PropertyField(handWidthMult, 	  new GUIContent("Hand Width", 		 "Hand (palm) Box Collider's width / Capsule Collider's "
																				+ "radius multiplier.."));
			EditorGUILayout.PropertyField(handDepthMult, 	  new GUIContent("Hand Depth", 		 "Hand (palm) Box Collider's depth multiplier. This "
																				+ "setting only has effect if Hand has a Box Collider."));
			EditorGUILayout.PropertyField(handLengthMult, 	  new GUIContent("Hand Length", 	 "Hand (palm) Collider's length multiplier."));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(fingerRadiusMult,   new GUIContent("Finger Radius",	 "Finger Capsule Colliders' radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead. "
																				+ "The base finger radius is hand width multiplied by 0.24. Affects all "
																				+ "fingers except thumbs."));
			EditorGUILayout.PropertyField(fingerLengthMult,   new GUIContent("Finger Length", 	 "Finger Colliders' length multiplier. Affects all fingers "
																				+ "except thumbs."));
			EditorGUILayout.PropertyField(fingerTaperValue,   new GUIContent("Finger Taper", 	 "Finger Colliders' taper factor, which is used to "
																				+ "multiply the radius of subsequent finger phalanges. A value of 1 or "
																				+ "slightly below is recommended. Affects all fingers except thumbs."));

			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width / 2 - 23));
			EditorGUILayout.PropertyField(thighRadiusMult, 	  new GUIContent("Thigh Radius", 	 "Thigh Capsule Collider's radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead."));
			EditorGUILayout.PropertyField(shinRadiusMult, 	  new GUIContent("Shin Radius", 	 "Shin Capsule Collider's radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead."));
			EditorGUILayout.PropertyField(footWidthMult, 	  new GUIContent("Foot Width", 		 "Foot Box Collider's width / Capsule Collider's radius "
																				+ "multiplier."));
			EditorGUILayout.PropertyField(footDepthMult, 	  new GUIContent("Foot Depth", 		 "Foot Box Collider's depth multiplier. This setting "
																				+ "only has effect if Foot has a Box Collider."));
			EditorGUILayout.PropertyField(footLengthMult, 	  new GUIContent("Foot Length", 	 "Foot Collider's length multiplier."));

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(thumbRadiusMult,   new GUIContent("Thumb Radius",		 "Thumb Capsule Colliders' radius multiplier, which also "
																				+ "acts as a width and depth multiplier if Box Collider is used instead. "
																				+ "The base thumb radius is hand width multiplied by 0.24."));
			EditorGUILayout.PropertyField(thumbLengthMult,   new GUIContent("Thumb Length", 	 "Thumb Colliders' length multiplier."));
			EditorGUILayout.PropertyField(thumbTaperValue,   new GUIContent("Thumb Taper",		 "Thumb Colliders' taper factor, which is used to "
																				+ "multiply the radius of subsequent thumb phalanges. A value of 1 or "
																				+ "slightly below is recommended."));
			
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			EditorGUIUtility.fieldWidth = 0;
			EditorGUIUtility.labelWidth = 0;
		}

		EditorGUILayout.Space();

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
		if(normalLabelColorWasSaved)
			EditorStyles.label.normal.textColor = normalLabelColor;
		GUI.color = normalGUIColor;

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
public static class RUISSkeletonControllerCustomSources
{
	static Dictionary<RUISSkeletonManager.Joint, string[]> properSourceNames;
	public static string[] leftStrings  = {"left", "l_", "l-", " l "};
	public static string[] rightStrings = {"right", "r_", "r-", " r "};

	public static string SkeletonNamePrefix { get; private set; }

	public static List<string>[] armTripletNames = new List<string>[3];
	public static List<string>[] armTripletExcludeNames = new List<string>[3];

	public static string[] foreArmVariations = new string[] {"forearm", "fore_arm", "fore-arm", "fore arm", "lowarm", "low_arm", 
															 "low-arm", "low arm", "lowerarm", "lower_arm", "lower-arm", "lower arm"};
	public static string[] upperLegVariations = new string[] {"upleg", "up_leg", "up-leg", "up leg", "upperleg", "upper_leg", "upper-leg", "upper leg"};

	public static string[] neuronExceptions = {"_rightinhand", "_leftinhand"};


	static RUISSkeletonControllerCustomSources()
	{
		// tripletNames[0] (clavicle alternatives), tripletNames[1] (shoulder alternatives), tripletNames[2] (elbow alternatives)
		armTripletNames[0] = new List<string>(new string[] {"shoulder"});
		armTripletNames[1] = new List<string>(new string[] {"arm", "uparm", "up_arm", "up-arm", "up arm", "upperarm", "upper_arm", "upper-arm", "upper arm"});
		armTripletNames[2] = new List<string>(new string[] {"elbow"}); // When modifying this, note that it is assigned to properSourceNames (LeftElbow) below!
		armTripletNames[2].AddRange(foreArmVariations);

		// Unwanted substrings to the above alternatives. These are only for the shoulder alternative "arm", to exclude the below cases with "arm"
		armTripletExcludeNames[0] = new List<string>(new string[] {""});
		armTripletExcludeNames[1] = new List<string>(foreArmVariations);
		armTripletExcludeNames[2] = new List<string>(new string[] {""});

		properSourceNames = new Dictionary<RUISSkeletonManager.Joint, string[]>();
		properSourceNames.Add(RUISSkeletonManager.Joint.Root,  new string[] {"root", "reference"}); // not references
		properSourceNames.Add(RUISSkeletonManager.Joint.Torso, new string[] {"pelvis", "hips"});
		properSourceNames.Add(RUISSkeletonManager.Joint.Chest, new string[] {"chest", "spine1"});
		properSourceNames.Add(RUISSkeletonManager.Joint.Neck,  new string[] {"neck"});
		properSourceNames.Add(RUISSkeletonManager.Joint.Head,  new string[] {"head"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftClavicle, new string[] {"clavicle"}); // Perception Neuron: shoulder
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftShoulder, new string[] {"shoulder",  "uparm", "up_arm", "up-arm", "up arm",
																					"upperarm", "upper_arm", "upper-arm", "upper arm"  }); // Perception Neuron: arm
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftElbow, armTripletNames[2].ToArray());
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftHand,  new string[] {"hand", "wrist"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftHip,   new string[] {"hip", "thigh", "upleg", "up_leg", "up-leg", "up leg", "upperleg", 
																				 "upper_leg", "upper-leg", "upper leg"								 }); // not hips
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftKnee,  new string[] {"knee", "shin", "calf", "lowerleg", "lower_leg", "lower leg", "leg"}); // not above
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftFoot,  new string[] {"foot", "ankle"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftThumb, new string[] {"thumb"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftIndexFinger,  new string[] {"index"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftMiddleFinger, new string[] {"middle"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftRingFinger,   new string[] {"ring"});
		properSourceNames.Add(RUISSkeletonManager.Joint.LeftLittleFinger, new string[] {"little", "pinky"});
	}

	public static string CommonPrefix(string a, string b)
	{
		if (a == null || b == null)
			return "";

		int min = Math.Min(a.Length, b.Length);
		string prefix = "";
		for(int i = 0; i < min && a[i] == b[i]; ++i)
			prefix = prefix + a[i];

		return prefix;
	}

	public static void ExtractSkeletonNamePrefix(Transform parent)
	{
		List<string> acceptedNames = new List<string>();
		Dictionary<string, uint> histogram = new Dictionary<string, uint>();

		if(parent == null)
		{
			SkeletonNamePrefix = "";
			return;
		}

		foreach(RUISSkeletonManager.Joint joint in properSourceNames.Keys)
		{
			if(properSourceNames.ContainsKey(joint) && properSourceNames[joint] != null)
				acceptedNames.AddRange(new List<string>(properSourceNames[joint]));
		}

		Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
		int transformCount = allChildren.Length;
		foreach(Transform child in allChildren) 
		{
			foreach(string acceptedName in acceptedNames)
			{
				int startIndex = child.name.IndexOf(acceptedName, StringComparison.OrdinalIgnoreCase);
				if(startIndex > 0)
				{
					string prefix = child.name.Substring(0, startIndex);
					if(histogram.ContainsKey(prefix))
						histogram[prefix]++;
					else
						histogram[prefix] = 1;
					break;
				}
			}
		}

		List<KeyValuePair<string, uint>> sortedHistogram = new List<KeyValuePair<string, uint>>(histogram);
		sortedHistogram.Sort((firstPair, nextPair) => { return nextPair.Value.CompareTo(firstPair.Value); } );

//		foreach(KeyValuePair<string, uint> pair in sortedHistogram)
//			Debug.Log(pair.Key + " occurred " + pair.Value + " times");

		if(sortedHistogram.Count <= 0 || sortedHistogram[0].Value < 5)
		{
			SkeletonNamePrefix = "";
			return;
		}

		string firstCandidate = sortedHistogram[0].Key;
		int handednessIndex = firstCandidate.IndexOf("right", StringComparison.OrdinalIgnoreCase);
		if(handednessIndex < 0)
			handednessIndex = firstCandidate.IndexOf("left", StringComparison.OrdinalIgnoreCase);
		if(handednessIndex == 0)
		{
			SkeletonNamePrefix = "";
			return;
		}
		if(handednessIndex > 0)
			firstCandidate = firstCandidate.Substring(0, handednessIndex);
			
		if(sortedHistogram.Count > 1 && sortedHistogram[1].Value >= 5)
		{
			SkeletonNamePrefix = CommonPrefix(firstCandidate, sortedHistogram[1].Key);
			if(SkeletonNamePrefix == "")
				SkeletonNamePrefix = firstCandidate;
		}
		else
			SkeletonNamePrefix = firstCandidate;

		if(		SkeletonNamePrefix != null && SkeletonNamePrefix.Length == 1 
			&& (SkeletonNamePrefix.ToLower()[0] == 'r' || SkeletonNamePrefix.ToLower()[0] == 'l'))
			SkeletonNamePrefix = "";

		return;
	}
		
	public static bool TripletExists(Transform parent, List<string>[] names, List<string>[] notContaining)
	{

		if(parent == null || names == null || names.Length < 3)
		{
			return false;
		}

		Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
		if(allChildren == null)
			return false;

		for(int i = 0; i < 3; ++i)
		{
			if(names[i] == null)
				return false;

			bool foundMatch = false;
			foreach(Transform child in allChildren) 
			{
				string childName = null;
				if(child)
				{
					childName = child.name;
					if(!string.IsNullOrEmpty(SkeletonNamePrefix) && childName.StartsWith(SkeletonNamePrefix))
						childName.Remove(0, SkeletonNamePrefix.Length);
				}
				else
					continue;
				
				foreach(string acceptedName in names[i])
				{
					if(acceptedName == null)
						continue;
					if(childName.IndexOf(acceptedName, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						bool unwantedSubstring = false;
						if(notContaining != null && notContaining.Length > i && notContaining[i] != null)
						{
							foreach(string unwanted in notContaining[i])
							{
								if(unwanted == "")
									continue;
								if(childName.IndexOf(unwanted, StringComparison.OrdinalIgnoreCase) >= 0)
								{
									unwantedSubstring = true;
									break;
								}
							}
							if(unwantedSubstring)
								continue;
						}
						foundMatch = true;
						break;
					}
				}
				if(foundMatch)
					break;
			}
			if(!foundMatch)
				return false;
		}

		return true;
	}

	public static bool FindSourceTransform( Transform parent, RUISSkeletonManager.Joint joint, string additionalMatch, string[] handedness,
											List<Transform> alreadyAssigned, string[] notContaining, ref List<string>[] sourceReport, SerializedProperty sourceTransform)
	{
		// properSourceNames contains names only for left joints
		RUISSkeletonManager.Joint handedJoint = joint;
		switch(joint)
		{
			case RUISSkeletonManager.Joint.RightClavicle: 	handedJoint = RUISSkeletonManager.Joint.LeftClavicle; 	break;
			case RUISSkeletonManager.Joint.RightShoulder:	handedJoint = RUISSkeletonManager.Joint.LeftShoulder; 	break;
			case RUISSkeletonManager.Joint.RightElbow: 		handedJoint = RUISSkeletonManager.Joint.LeftElbow; 		break;
			case RUISSkeletonManager.Joint.RightHand: 		handedJoint = RUISSkeletonManager.Joint.LeftHand; 		break;
			case RUISSkeletonManager.Joint.RightHip: 		handedJoint = RUISSkeletonManager.Joint.LeftHip; 		break;
			case RUISSkeletonManager.Joint.RightKnee: 		handedJoint = RUISSkeletonManager.Joint.LeftKnee; 		break;
			case RUISSkeletonManager.Joint.RightFoot: 		handedJoint = RUISSkeletonManager.Joint.LeftFoot; 		break;
			case RUISSkeletonManager.Joint.RightThumb: 		handedJoint = RUISSkeletonManager.Joint.LeftThumb; 		break;
			case RUISSkeletonManager.Joint.RightIndexFinger:	handedJoint = RUISSkeletonManager.Joint.LeftIndexFinger;	break;
			case RUISSkeletonManager.Joint.RightMiddleFinger:	handedJoint = RUISSkeletonManager.Joint.LeftMiddleFinger;	break;
			case RUISSkeletonManager.Joint.RightRingFinger:		handedJoint = RUISSkeletonManager.Joint.LeftRingFinger;		break;
			case RUISSkeletonManager.Joint.RightLittleFinger:	handedJoint = RUISSkeletonManager.Joint.LeftLittleFinger;	break;
		}

		if(properSourceNames.ContainsKey(handedJoint))
		{
			List<Transform> candidates = new List<Transform>();
			List<string> acceptedNames = null;
			if(parent != null && properSourceNames.ContainsKey(handedJoint) && properSourceNames[handedJoint] != null)
				acceptedNames = new List<string>(properSourceNames[handedJoint]);
			else
			{
				if(sourceReport.Length > 1) // Did not find Custom Source for this joint
					sourceReport[1].Add(joint.ToString());
				return false;
			}

			if(additionalMatch != null)
				acceptedNames.Add(additionalMatch);

			Transform[] allChildren = parent.GetComponentsInChildren<Transform>();
			foreach(Transform child in allChildren) 
			{
				string childName = null;
				if(child)
				{
					childName = child.name;
					if(!string.IsNullOrEmpty(SkeletonNamePrefix) && childName.StartsWith(SkeletonNamePrefix))
						childName.Remove(0, SkeletonNamePrefix.Length);
				}
				else
					continue;

				bool unwantedSubstring = false;
				if(notContaining != null)
				{
					foreach(string unwanted in notContaining)
					{
						if(childName.IndexOf(unwanted, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							unwantedSubstring = true;
							break;
						}
					}
					if(unwantedSubstring)
						continue;
				}

				bool foundMatch = false;
				int substringIndex = -1;
				foreach(string acceptedName in acceptedNames)
				{
					substringIndex = childName.IndexOf(acceptedName, StringComparison.OrdinalIgnoreCase);
					if(substringIndex >= 0)
					{
						foundMatch = true;
						break;
					}
				}
				if(foundMatch)
				{
					if(handedness == null)
						candidates.Add(child);
					else
					{
						if(substringIndex > 0)
						{
							string leftOrRight = "";
							foreach(string prefix in handedness)
							{
								if(prefix.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0)
									leftOrRight = "left";
								else if(prefix.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0)
									leftOrRight = "right";
								
								if(childName.Substring(0, substringIndex).IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
								{
									candidates.Add(child);
									break;
								}
							}

							// Hack to check whether the matched joint name substring is preceded by an L or R prefix: [L/R]JointName
							if(leftOrRight == "left"  && substringIndex > 0 && childName[substringIndex - 1] == 'L')
								candidates.Add(child);
							if(leftOrRight == "right" && substringIndex > 0 && childName[substringIndex - 1] == 'R')
								candidates.Add(child);
						}
					}
				}
			}
				
			Transform unassignedCandidate = null;
			if(candidates.Count > 1)
			{
				string candidateNames = "";
				foreach(Transform candidate in candidates)
				{
					if(candidateNames.Length == 0)
						candidateNames += candidate.name;
					else 
						candidateNames += ", " + candidate.name;
				}

				if(sourceReport.Length > 2 && candidateNames != "") // Multiple candidates
					sourceReport[2].Add("----" + joint.ToString() + ":    { " + candidateNames + " }");
			}
			if(candidates.Count >= 1)
			{
				if(alreadyAssigned != null)
				{
					for(int i = 0; i < candidates.Count; ++i)
					{
						bool accepted = true;
						foreach(Transform assigned in alreadyAssigned)
						{
							if(candidates[i] == assigned && assigned != null)
								accepted = false;
						}
						if(accepted && candidates[i] != null)
						{
							unassignedCandidate = candidates[i];
							break;
						}
					}
				}
				else if(candidates[0] != null)
					unassignedCandidate = candidates[0];
				if(unassignedCandidate != null)
				{
					if(sourceReport.Length > 0) // Found Custom Source for this joint
						sourceReport[0].Add(joint.ToString());
					sourceTransform.objectReferenceValue = unassignedCandidate;
					if(alreadyAssigned != null)
						alreadyAssigned.Add(unassignedCandidate);
					return true;
				}
			}
			else
			{
				if(sourceReport.Length > 1) // Did not find Custom Source for this joint
					sourceReport[1].Add(joint.ToString());
			}
		}
		return false;
	}
}

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