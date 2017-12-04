/*****************************************************************************

Content    :   Functionality to control a skeleton using Kinect
Authors    :   Mikael Matveinen, Tuukka Takala, Heikki Heiskanen
Copyright  :   Copyright 2016 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen.
               All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public enum RUISAxis
{
	X,
	Y,
	Z
}

public class RUISSkeletonController : MonoBehaviour
{

	public Transform root;
	public Transform torso;   // pelvis // previously: torso
	public Transform chest;   // new
	public Transform neck;
	public Transform head;
	public Transform rightClavicle; // new
	public Transform rightShoulder;
	public Transform rightElbow;
	public Transform rightHand;
	public Transform rightHip;
	public Transform rightKnee;
	public Transform rightFoot;
	public Transform leftClavicle; // new
	public Transform leftShoulder;
	public Transform leftElbow;
	public Transform leftHand;
	public Transform leftHip;
	public Transform leftKnee;
	public Transform leftFoot;
	public Transform leftThumb;
	public Transform rightThumb;

	// Transform sources for custom motion tracking
	public Transform customRoot;
	public Transform customTorso; // pelvis // previously: torso
	public Transform customChest; // new
	public Transform customNeck;
	public Transform customHead;
	public Transform customRightClavicle; // new
	public Transform customRightShoulder;
	public Transform customRightElbow;
	public Transform customRightHand;
	public Transform customRightHip;
	public Transform customRightKnee;
	public Transform customRightFoot;
	public Transform customLeftClavicle; // new
	public Transform customLeftShoulder;
	public Transform customLeftElbow;
	public Transform customLeftHand;
	public Transform customLeftHip;
	public Transform customLeftKnee;
	public Transform customLeftFoot;
	public Transform customLeftThumb;
	public Transform customRightThumb;

	public Vector3 pelvisOffset   = Vector3.zero;
	public Vector3 chestOffset	  = Vector3.zero;
	public Vector3 neckOffset	  = Vector3.zero;
	public Vector3 headOffset	  = Vector3.zero;
	public Vector3 clavicleOffset = Vector3.zero;
	public Vector3 shoulderOffset = Vector3.zero;
	public Vector3 hipOffset 	  = Vector3.zero;

	public Vector3 feetRotationOffset = Vector3.zero;

	public float pelvisScaleAdjust   = 1;
	public float chestScaleAdjust 	 = 1;
	public float neckScaleAdjust 	 = 1;
	public float headScaleAdjust 	 = 1;
	public float clavicleScaleAdjust = 1;

	public bool fistCurlFingers = true;
	public bool externalCurlTrigger = false;
	public bool trackThumbs = false;
	public bool trackWrist = true;
	public bool trackAnkle = true;
	public bool rotateWristFromElbow = true;

	public bool neckParentsShoulders = false;
	
	public RUISSkeletonManager.Skeleton.handState leftHandStatus = RUISSkeletonManager.Skeleton.handState.open;
	public RUISSkeletonManager.Skeleton.handState rightHandStatus = RUISSkeletonManager.Skeleton.handState.open;

	public RUISSkeletonManager.Skeleton.handState externalLeftStatus = RUISSkeletonManager.Skeleton.handState.open;
	public RUISSkeletonManager.Skeleton.handState externalRightStatus = RUISSkeletonManager.Skeleton.handState.open;

	private RUISSkeletonManager.Skeleton.handState lastLeftHandStatus, lastRightHandStatus;


	private RUISInputManager inputManager;
	public RUISSkeletonManager skeletonManager;
	private RUISCoordinateSystem coordinateSystem;
	public RUISCharacterController characterController;

	public enum BodyTrackingDeviceType
	{
		Kinect1,
		Kinect2,
		GenericMotionTracker
	}

	public enum CustomConversionType
	{
		None, 
		Custom_1, 
		Custom_2
	}

	/// <summary>Never set/substitute this variable. Set bodyTrackingDeviceID variable instead.</summary>
	public BodyTrackingDeviceType bodyTrackingDevice = BodyTrackingDeviceType.Kinect2;

	// *** HACK: Ideally bodyTrackingDevice would have public getter and public setter, and bodyTrackingDeviceID would have public getter
	//			and private setter. But since bodyTrackingDevice is exposed in the Custom Inspector (RUISSkeletonController) and I was too
	//			lazy to make a C# property work properly there, I've created the following hack.
	//			Developers should not set the value of bodyTrackingDevice variable, instead they should set the bodyTrackingDeviceID variable
	//			to either RUISSkeletonManager.kinect1SensorID, RUISSkeletonManager.kinect2SensorID, or RUISSkeletonManager.customSensorID.
	//			Failing to do so will create issues when using Custom / GenericMotionTracker for skeleton tracking.
	private int _bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;

	/// <summary>This value should be either RUISSkeletonManager.kinect1SensorID, RUISSkeletonManager.kinect2SensorID, or RUISSkeletonManager.customSensorID.</summary>
	public int BodyTrackingDeviceID
	{
		get
		{
			return _bodyTrackingDeviceID;
		}
		set
		{
			this._bodyTrackingDeviceID = value;
			switch(this._bodyTrackingDeviceID)
			{
				case RUISSkeletonManager.kinect1SensorID:
					bodyTrackingDevice = BodyTrackingDeviceType.Kinect1;
					break;
				case RUISSkeletonManager.kinect2SensorID:
					bodyTrackingDevice = BodyTrackingDeviceType.Kinect2;
					break;
				case RUISSkeletonManager.customSensorID:
					bodyTrackingDevice = BodyTrackingDeviceType.GenericMotionTracker;
					if(inputManager)
					{
						switch(customConversionType)
						{
							case CustomConversionType.Custom_1:
								deviceConversion = inputManager.customDevice1Conversion;
								customSourceDevice = RUISDevice.Custom_1;
								break;
							case CustomConversionType.Custom_2:
								deviceConversion = inputManager.customDevice2Conversion;
								customSourceDevice = RUISDevice.Custom_2;
								break;
							case CustomConversionType.None:
								deviceConversion = new RUISCoordinateSystem.DeviceCoordinateConversion();
								customSourceDevice = RUISDevice.Null;
								break;
						}
					}
					break;
			}
		}
	}

	public CustomConversionType customConversionType = CustomConversionType.None;
	private RUISCoordinateSystem.DeviceCoordinateConversion deviceConversion;
	private RUISDevice customSourceDevice = RUISDevice.Null;

	public int playerId = 0;
	public bool switchToAvailableKinect = false;

	private Vector3 skeletonPosition = Vector3.zero;
	private Vector3 tempPosition = Vector3.zero;
	private Quaternion tempRotation = Quaternion.identity;


	public bool updateRootPosition = true;
	public Vector3 rootSpeedScaling = Vector3.one;
	public Vector3 rootOffset = Vector3.zero;

	public bool updateJointPositions = true;
	public bool updateJointRotations = true;

	public bool useHierarchicalModel = true;
	public bool scaleHierarchicalModelBones = true;
	public bool scaleBoneLengthOnly = false;
	public RUISAxis boneLengthAxis = RUISAxis.X;
	public float maxScaleFactor = 0.01f;

	public float torsoThickness = 1;
	public float rightArmThickness = 1;
	public float leftArmThickness = 1;
	public float rightLegThickness = 1;
	public float leftLegThickness = 1;

	public float minimumConfidenceToUpdate = 0.5f;
	public float rotationDamping = 360.0f;

	// Constrained between [0, -180] in Unity Editor script
	public float handRollAngleMinimum = -180; 

	// Constrained between [0,  180] in Unity Editor script
	public float handRollAngleMaximum = 180; 

	public bool hmdRotatesHead = true;
	public bool hmdMovesHead = true;

	public bool followHmdPosition { get; private set; }

	public Quaternion trackedDeviceYawRotation { get; private set; }

	public bool followMoveController { get; private set; }

	private int followMoveID = 0;
	private RUISPSMoveWand psmove;

	private Vector3 torsoDirection = Vector3.down;
	private Quaternion torsoRotation = Quaternion.identity;

	private float deltaTime = 0.03f;

	public bool filterPosition = true;
	private KalmanFilter positionKalman;
	private double[] measuredPos = { 0, 0, 0 };
	private double[] pos = { 0, 0, 0 };
	public float positionNoiseCovariance = 100;

	private KalmanFilter[] fourJointsKalman = new KalmanFilter[4];
	public float fourJointsNoiseCovariance = 50;
	private Vector3[] fourJointPositions = new Vector3[4];
	
	public bool filterRotations = false;
	public float rotationNoiseCovariance = 200;
	// Offset Z rotation of the thumb. Default value is 45, but it might depend on your avatar rig.
	public float thumbZRotationOffset = 45;

	private Dictionary<Transform, Quaternion> jointInitialRotations;
	private Dictionary<Transform, Vector3> jointInitialLocalPositions;
	private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;

	public float adjustVerticalHipsPosition = 0; // *** OPTIHACK TODO remove
	private Vector3 spineDirection = Vector3.zero; // *** OPTIHACK TODO remove
	//private RUISSkeletonManager.JointData adjustedHipJoint = new RUISSkeletonManager.JointData();

	private float torsoOffset = 0.0f;

	private float torsoScale = 1.0f;

	public float neckHeightTweaker = 0.0f; // *** OPTIHACK TODO remove
	private Vector3 neckOriginalLocalPosition; // *** OPTIHACK TODO remove

	private bool tweakableHips = false; // *** OPTIHACK TODO remove
	private Vector3 chestOriginalLocalPosition; // *** OPTIHACK TODO remove

	public float forearmLengthRatio = 1.0f;
	public float shinLengthRatio = 1.0f;

	private Vector3 prevRightShoulder;
	private Vector3 prevRightForearmScale;
	private Vector3 prevRightHandScale;
	private Vector3 prevRightHip;
	private Vector3 prevRightShinScale;
	private Vector3 prevRightFootScale;
	private Vector3 prevLeftShoulder;
	private Vector3 prevLeftForearmScale;
	private Vector3 prevLeftHandScale;
	private Vector3 prevLeftHip;
	private Vector3 prevLeftShinScale;
	private Vector3 prevLeftFootScale;
	
	//	Ovr.HmdType ovrHmdVersion = Ovr.HmdType.None; //06to08

	// 2 hands, 5 fingers, 3 finger bones
	Quaternion[,,] initialFingerRotations = new Quaternion[2, 5, 3];
	// For quick access to finger gameobjects
	Transform[,,] fingerTransforms = new Transform[2, 5, 3];

	// NOTE: The below phalange rotations are set in Start() method !!! See clause that starts with switch(boneLengthAxis)
	// Thumb phalange rotations when hand is clenched to a fist
	public Quaternion clenchedRotationThumbTM;
	public Quaternion clenchedRotationThumbMCP;
	public Quaternion clenchedRotationThumbIP;
	
	// Phalange rotations of other fingers when hand is clenched to a fist
	public Quaternion clenchedRotationMCP;
	public Quaternion clenchedRotationPIP;
	public Quaternion clenchedRotationDIP;

	public bool leftThumbHasIndependentRotations = false;
	public Quaternion clenchedLeftThumbTM;
	public Quaternion clenchedLeftThumbMCP;
	public Quaternion clenchedLeftThumbIP;

	void Awake()
	{
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;

		if(inputManager)
		{
			if(switchToAvailableKinect)
			{
				if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect1
				   && !inputManager.enableKinect && inputManager.enableKinect2)
				{
					bodyTrackingDevice = BodyTrackingDeviceType.Kinect2;
				}
				else if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2
				        && !inputManager.enableKinect2 && inputManager.enableKinect)
				{
					bodyTrackingDevice = BodyTrackingDeviceType.Kinect1;
				}
			}
		}

		// Following substitution ensures that customConversionType, customSourceDevice, and deviceConversion variables will be set in bodyTrackingDeviceID setter
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
			BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
		
		coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
		if(!coordinateSystem)
		{
			Debug.LogError(typeof(RUISCoordinateSystem) + " not found in the scene! Disabling this script.");
			this.enabled = false;
		}

		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
			BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2)
			BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
			BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;

		followHmdPosition = false;
		followMoveController = false;
		trackedDeviceYawRotation = Quaternion.identity;
		
		jointInitialRotations 	   = new Dictionary<Transform, Quaternion>();
		jointInitialLocalPositions = new Dictionary<Transform, Vector3>();
		jointInitialDistances 	   = new Dictionary<KeyValuePair<Transform, Transform>, float>();
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3, 3);

		neckParentsShoulders = leftShoulder.IsChildOf(neck) || rightShoulder.IsChildOf(neck);

		for(int i = 0; i < fourJointsKalman.Length; ++i)
		{
			fourJointsKalman[i] = new KalmanFilter();
			fourJointsKalman[i].initialize(3, 3);
			fourJointPositions[i] = Vector3.zero;
		}
	}

	void Start()
	{
		
		if(skeletonManager == null)
		{
			skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
			if(!skeletonManager)
				Debug.LogError("The scene is missing " + typeof(RUISSkeletonManager) + " script!");
		}

		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
			skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking = true;

		switch(customConversionType)
		{
			case CustomConversionType.Custom_1:
				deviceConversion = inputManager.customDevice1Conversion;
				customSourceDevice = RUISDevice.Custom_1;
				break;
			case CustomConversionType.Custom_2:
				deviceConversion = inputManager.customDevice2Conversion;
				customSourceDevice = RUISDevice.Custom_2;
				break;
			default:
				deviceConversion = new RUISCoordinateSystem.DeviceCoordinateConversion();
				deviceConversion = null;
				break;
		}

		// Disable features that are only available for Kinect2 or custom motion tracker
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
		{
			fistCurlFingers = false;
			trackThumbs = false;
			trackWrist = false;
			trackAnkle = false;
			rotateWristFromElbow = false;
		}

		if(useHierarchicalModel)
		{
			// Fix all shoulder and hip rotations to match the default kinect rotations (T-pose)
			rightShoulder.rotation = FindFixingRotation(rightShoulder.position, rightElbow.position, transform.right) * rightShoulder.rotation;
			leftShoulder.rotation = FindFixingRotation(leftShoulder.position, leftElbow.position, -transform.right) * leftShoulder.rotation;
			rightHip.rotation = FindFixingRotation(rightHip.position, rightFoot.position, -transform.up) * rightHip.rotation;
			leftHip.rotation = FindFixingRotation(leftHip.position, leftFoot.position, -transform.up) * leftHip.rotation;

			Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
			Vector3 assumedRootPos = Vector3.Scale((rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4, scaler); 
															
			Vector3 realRootPos = Vector3.Scale(torso.position, scaler);

			Vector3 torsoUp = head.position - torso.position;
			torsoUp.Normalize();

			if(BodyTrackingDeviceID != RUISSkeletonManager.customSensorID) // *** OPTIHACK
				torsoOffset = Vector3.Dot(realRootPos - assumedRootPos, torsoUp);

			if(neck)
			{
				neckOriginalLocalPosition = neck.localPosition;
				if(neck.parent)
				{
					// Below relates to adjustVerticalHipsPosition, which is mostly relevant to Kinect based avatar animation
					if(bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker)
					{
						if(!chest)
							chest = neck.parent;
						if(chest == torso)
						{
							Debug.Log(typeof(RUISSkeletonController) + ": Hierarchical model stored in GameObject " + this.name
							+ " does not have enough joints between neck and torso for Hips Vertical Tweaker to work.");
						}
						else
						{
							chestOriginalLocalPosition = chest.localPosition;
							tweakableHips = true;
						}
					}
				}
			}
		}

		SaveInitialRotation(root);
		SaveInitialRotation(head);
		SaveInitialRotation(neck); // *** OPTIHACK
		SaveInitialRotation(chest); // *** OPTIHACK
		SaveInitialRotation(torso);
		SaveInitialRotation(rightClavicle); // *** OPTIHACK
		SaveInitialRotation(rightShoulder);
		SaveInitialRotation(rightElbow);
		SaveInitialRotation(rightHand);
		SaveInitialRotation(leftClavicle); // *** OPTIHACK
		SaveInitialRotation(leftShoulder);
		SaveInitialRotation(leftElbow);
		SaveInitialRotation(leftHand);
		SaveInitialRotation(rightHip);
		SaveInitialRotation(rightKnee);
		SaveInitialRotation(rightFoot);
		SaveInitialRotation(leftHip);
		SaveInitialRotation(leftKnee);
		SaveInitialRotation(leftFoot);

		SaveInitialRotation(leftThumb);
		SaveInitialRotation(rightThumb);

		SaveInitialFingerRotations();

		SaveInitialLocalPosition(chest);
		SaveInitialLocalPosition(neck);
		SaveInitialLocalPosition(head);
		SaveInitialLocalPosition(rightClavicle);
		SaveInitialLocalPosition(leftClavicle);

		if(rightClavicle)
			SaveInitialDistance(rightClavicle, rightShoulder);
		SaveInitialDistance(rightShoulder, rightElbow);
		SaveInitialDistance(rightElbow,   rightHand);
		if(leftClavicle)
			SaveInitialDistance(leftClavicle, leftShoulder);
		SaveInitialDistance(leftShoulder, leftElbow);
		SaveInitialDistance(leftElbow,    leftHand);

		SaveInitialDistance(rightHip,  rightKnee);
		SaveInitialDistance(rightKnee, rightFoot);
		SaveInitialDistance(leftHip,   leftKnee);
		SaveInitialDistance(leftKnee,  leftFoot);

//		SaveInitialDistance(torso, head); // *** OPTIHACK THIS WAS NOT USED ANYWHERE??
		if(chest)
		{
			SaveInitialDistance(torso, chest); // *** OPTIHACK
			SaveInitialDistance(chest, neck); // *** OPTIHACK
		}
		if(neck)
		{
			SaveInitialDistance(neck, head); // *** OPTIHACK
			if(leftClavicle)
				SaveInitialDistance(neck, leftClavicle);
			if(rightClavicle)
				SaveInitialDistance(neck, rightClavicle);
		}

		SaveInitialDistance(rightShoulder, leftShoulder);
		SaveInitialDistance(rightHip, leftHip);


		if(rightShoulder)
			prevRightShoulder = rightShoulder.localScale;

		if(rightElbow)
			prevRightForearmScale = rightElbow.localScale;

		if(rightHand)
			prevRightHandScale = rightHand.localScale;

		if(rightHip)
			prevRightHip = rightHip.localScale;

		if(rightKnee)
			prevRightShinScale = rightKnee.localScale;

		if(rightFoot)
			prevRightFootScale = rightFoot.localScale;

		if(leftShoulder)
			prevLeftShoulder = leftShoulder.localScale;
		
		if(leftElbow)
			prevLeftForearmScale = leftElbow.localScale;

		if(leftHand)
			prevLeftHandScale = leftHand.localScale;
		
		if(leftHip)
			prevLeftHip = leftHip.localScale;

		if(leftKnee)
			prevLeftShinScale = leftKnee.localScale;

		if(leftFoot)
			prevLeftFootScale = leftFoot.localScale;


		// Finger clench rotations: these depend on your animation rig
		// Also see method handleFingersCurling() and its clenchedRotationThumbTM_corrected and clenchedRotationThumbIP_corrected
		// variables, if you are not tracking thumbs with Kinect 2. They also depend on your animation rig.
		switch(boneLengthAxis)
		{
			case RUISAxis.X:
				// Thumb phalange rotations when hand is clenched to a fist
				clenchedRotationThumbTM = Quaternion.Euler(45, 0, 0); 
				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, -25);
				clenchedRotationThumbIP = Quaternion.Euler(0, 0, -80);
				// Phalange rotations of other fingers when hand is clenched to a fist
				clenchedRotationMCP = Quaternion.Euler(0, 0, -45);
				clenchedRotationPIP = Quaternion.Euler(0, 0, -100);
				clenchedRotationDIP = Quaternion.Euler(0, 0, -70);
				break;
			case RUISAxis.Y:
				// Thumb phalange rotations when hand is clenched to a fist
				clenchedRotationThumbTM = Quaternion.Euler(0, 0, 0); 
				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, 0);
				clenchedRotationThumbIP = Quaternion.Euler(0, 0, 80);
				// Phalange rotations of other fingers when hand is clenched to a fist
				clenchedRotationMCP = Quaternion.Euler(45, 0, 0);
				clenchedRotationPIP = Quaternion.Euler(100, 0, 0);
				clenchedRotationDIP = Quaternion.Euler(70, 0, 0);
				break;
			case RUISAxis.Z: // TODO: Not yet tested with a real rig
				// Thumb phalange rotations when hand is clenched to a fist
				clenchedRotationThumbTM = Quaternion.Euler(45, 0, 0); 
				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, -25);
				clenchedRotationThumbIP = Quaternion.Euler(0, 0, -80);
				// Phalange rotations of other fingers when hand is clenched to a fist
				clenchedRotationMCP = Quaternion.Euler(0, -45, 0);
				clenchedRotationPIP = Quaternion.Euler(0, -100, 0);
				clenchedRotationDIP = Quaternion.Euler(0, -70, 0);
				break;
		}
		if(leftThumbHasIndependentRotations)
		{
			clenchedLeftThumbTM  = Quaternion.Euler(15, 0, -25);
			clenchedLeftThumbMCP = Quaternion.Euler(0, 0, 0);
			clenchedLeftThumbIP  = Quaternion.Euler(0, 0, 30);
		}

		if(inputManager)
		{
			if(gameObject.transform.parent != null)
			{
				characterController = gameObject.transform.parent.GetComponent<RUISCharacterController>();
				if(characterController != null)
				{
					if(characterController.characterPivotType == RUISCharacterController.CharacterPivotType.MoveController
					   &&	inputManager.enablePSMove)
					{
						followMoveController = true;
						followMoveID = characterController.moveControllerId;
//						if(		 gameObject.GetComponent<RUISKinectAndMecanimCombiner>() == null 
//							||	!gameObject.GetComponent<RUISKinectAndMecanimCombiner>().enabled )
						Debug.LogWarning("Using PS Move controller #" + characterController.moveControllerId + " as a source "
						+	"for avatar root position of " + gameObject.name + ", because PS Move is enabled"
						+	"and the PS Move controller has been assigned as a "
						+	"Character Pivot in " + gameObject.name + "'s parent GameObject");
					}

					if(   !inputManager.enableKinect && !inputManager.enableKinect2 && !followMoveController
					   && bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker)
					{
						if(RUISDisplayManager.IsHmdPresent())
						{
							followHmdPosition = true;
							Debug.LogWarning("Using " + RUISDisplayManager.GetHmdModel() + " as a Character Pivot for " + gameObject.name
							+ ", because Kinects are disabled and " + RUISDisplayManager.GetHmdModel() + " was detected.");
						}
					}
				}
			}
		}

		skeletonPosition = transform.localPosition;

		if(hmdRotatesHead && !RUISDisplayManager.IsHmdPresent())
			hmdRotatesHead = false;

		// HACK for filtering Kinect 2 arm rotations
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRotations = filterRotations;
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rotationNoiseCovariance = rotationNoiseCovariance;
		for(int i = 0; i < skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot.Length; ++i)
		{
			if(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot[i] != null)
				skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot[i].rotationNoiseCovariance = rotationNoiseCovariance;
		}
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].thumbZRotationOffset = thumbZRotationOffset;

		if(!trackAnkle)
		{
			if(leftFoot)
				leftFoot.localRotation = Quaternion.Euler(feetRotationOffset) * leftFoot.localRotation;
			if(rightFoot)
				rightFoot.localRotation = Quaternion.Euler(feetRotationOffset) * rightFoot.localRotation;
		}

	}

	void LateUpdate()
	{
		deltaTime = Time.deltaTime; //1.0f / vr.hmd_DisplayFrequency;

		// If a custom skeleton tracking source is used, save its data into skeletonManager (which is a little 
		// topsy turvy) so we can utilize same code as we did with Kinect 1 and 2
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
		{
			SetCustomJointData(customRoot, 			ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].root, 		1, 1);
			SetCustomJointData(customTorso, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso, 		1, 1);
			SetCustomJointData(customChest, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest, 		1, 1);
			SetCustomJointData(customNeck, 			ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck, 		1, 1);
			SetCustomJointData(customHead, 			ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head, 		1, 1);
			SetCustomJointData(customLeftClavicle, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftClavicle, 1, 1);
			SetCustomJointData(customLeftShoulder, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder, 1, 1);
			SetCustomJointData(customLeftElbow, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftElbow, 	1, 1);
			SetCustomJointData(customLeftHand, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHand, 	1, 1);
			SetCustomJointData(customLeftThumb, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftThumb, 	1, 1);
			SetCustomJointData(customLeftHip, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip, 		1, 1);
			SetCustomJointData(customLeftKnee, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftKnee, 	1, 1);
			SetCustomJointData(customLeftFoot, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftFoot, 	1, 1);
			SetCustomJointData(customRightClavicle, ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightClavicle, 1, 1);
			SetCustomJointData(customRightShoulder, ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, 1, 1);
			SetCustomJointData(customRightElbow, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightElbow, 	 1, 1);
			SetCustomJointData(customRightHand, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHand, 	 1, 1);
			SetCustomJointData(customRightThumb, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightThumb, 	 1, 1);
			SetCustomJointData(customRightHip, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip, 	 1, 1);
			SetCustomJointData(customRightKnee, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightKnee, 	 1, 1);
			SetCustomJointData(customRightFoot, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightFoot, 	 1, 1);
		}

		// Update skeleton based on data fetched from skeletonManager
		if(skeletonManager != null && skeletonManager.skeletons[BodyTrackingDeviceID, playerId] != null
		    && skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking)
		{
						
//			if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID && !skeletonManager.isNewKinect2Frame)
//				return;

			float maxAngularVelocity;
//			if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID)
//				maxAngularVelocity = skeletonManager.kinect2FrameDeltaT * rotationDamping;
//			else 
			maxAngularVelocity = deltaTime * rotationDamping;


			// Obtained new body tracking data. TODO test that Kinect 1 still works
//			if(bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame)
			{
				UpdateSkeletonPosition();

				UpdateTransform(ref torso, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso, maxAngularVelocity);
				UpdateTransform(ref chest, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest, maxAngularVelocity); // *** OPTIHACK
				UpdateTransform(ref neck,  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck,  maxAngularVelocity); // *** OPTIHACK
				UpdateTransform(ref head,  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head,  maxAngularVelocity);

				// *** OPTIHACK
				UpdateTransform(ref leftClavicle,  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftClavicle,  maxAngularVelocity); 
				UpdateTransform(ref rightClavicle, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightClavicle, maxAngularVelocity);

				if(chest)
					chest.localPosition = chestOffset + jointInitialLocalPositions[chest];
				if(neck)
					neck.localPosition  = neckOffset  + jointInitialLocalPositions[neck];
				if(head)
					head.localPosition  = headOffset  + jointInitialLocalPositions[head];
				if(leftClavicle)
					leftClavicle.localPosition  = clavicleOffset + jointInitialLocalPositions[leftClavicle];
				if(rightClavicle)
					rightClavicle.localPosition = clavicleOffset + jointInitialLocalPositions[rightClavicle];
			}
				
			if(hmdRotatesHead && RUISDisplayManager.IsHmdPresent())
			{
				if(coordinateSystem)
				{
					Quaternion hmdRotation = Quaternion.identity;
					if(coordinateSystem.applyToRootCoordinates)
					{
//						if(ovrHmdVersion == Ovr.HmdType.DK1 || ovrHmdVersion == Ovr.HmdType.DKHD) //06to08
//							oculusRotation = coordinateSystem.GetOculusRiftOrientationRaw();
//						else //06to08
//							oculusRotation = coordinateSystem.ConvertRotation(Quaternion.Inverse(coordinateSystem.GetOculusCameraOrientationRaw()) 
//								                                                  * coordinateSystem.GetOculusRiftOrientationRaw(), RUISDevice.Oculus_DK2);
						hmdRotation = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), RUISDevice.OpenVR);
					}
					else
						hmdRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head);
//					else if(OVRManager.display != null) //06to08
//						oculusRotation = OVRManager.display.GetHeadPose().orientation; 

					if(useHierarchicalModel)
						head.rotation = transform.rotation * hmdRotation /*skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head.rotation*/ *
						(jointInitialRotations.ContainsKey(head) ? jointInitialRotations[head] : Quaternion.identity);
					else
						head.localRotation = hmdRotation; //skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head;
				}
			}
			
			// Obtained new body tracking data. TODO test that Kinect 1 still works
//			if(bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame)
			{
				UpdateTransform(ref leftShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder, maxAngularVelocity);
				UpdateTransform(ref rightShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, maxAngularVelocity);

				if(trackWrist || !useHierarchicalModel)
				{
					UpdateTransform(ref leftHand, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHand, 2 * maxAngularVelocity);
					UpdateTransform(ref rightHand, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHand, 2 * maxAngularVelocity);
				}

				UpdateTransform(ref leftHip, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip, maxAngularVelocity);
				UpdateTransform(ref rightHip, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip, maxAngularVelocity);
				UpdateTransform(ref leftKnee, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftKnee, maxAngularVelocity);
				UpdateTransform(ref rightKnee, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightKnee, maxAngularVelocity);
				
				UpdateTransform(ref rightElbow, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightElbow, maxAngularVelocity);
				UpdateTransform(ref leftElbow, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftElbow, maxAngularVelocity);

				if(trackAnkle || !useHierarchicalModel)
				{
					UpdateTransform(ref leftFoot, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftFoot, maxAngularVelocity);
					UpdateTransform(ref rightFoot, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightFoot, maxAngularVelocity);
				}
				// *** TODO in below clause we need to have initial localRotations...
//#if UNITY_EDITOR
//				else
//				{
//					if(!trackAnkle)
//					{
//						if(leftFoot)
//							leftFoot.localRotation  = Quaternion.Euler(feetRotationOffset) * jointInitialRotations[leftFoot];
//						if(rightFoot)
//							rightFoot.localRotation = Quaternion.Euler(feetRotationOffset) * jointInitialRotations[rightFoot];
//					}
//				}
//#endif
			
//				// TODO: Restore this when implementation is fixed
//				if(rotateWristFromElbow && bodyTrackingDevice == bodyTrackingDeviceType.Kinect2)
//				{
//					if (useHierarchicalModel)
//					{
//						if(leftElbow && leftHand)
//							leftElbow.rotation  = leftHand.rotation;
//						if(rightElbow && rightHand)
//							rightElbow.rotation = rightHand.rotation;
//					}
//					else
//					{
//						if(leftElbow && leftHand)
//							leftElbow.localRotation  = leftHand.localRotation;
//						if(rightElbow && rightHand)
//							rightElbow.localRotation = rightHand.localRotation;
//					}
//					//				UpdateTransform (ref rightElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand);
//					//				UpdateTransform (ref leftElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand);
//				}
	
				if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2)
				{
					leftHandStatus = (skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHandStatus);
					rightHandStatus = (skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHandStatus);

					if(fistCurlFingers && !externalCurlTrigger)
						handleFingersCurling(trackThumbs);

					if(trackThumbs)
					{
						if(rightThumb)
							UpdateTransform(ref rightThumb, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightThumb, maxAngularVelocity);
						if(leftThumb)
							UpdateTransform(ref leftThumb, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftThumb, maxAngularVelocity);
					}
				}
				
				// There is some other trigger that determines fist curling (e.g. RUISButtonGestureRecognizer)
				if(externalCurlTrigger)
					handleFingersCurling(trackThumbs);
			}

			if(!useHierarchicalModel)
			{
				if(!trackWrist)
				{
					if(leftHand != null && leftElbow != null)
						leftHand.localRotation = leftElbow.localRotation;
					if(rightHand != null && rightElbow != null)
						rightHand.localRotation = rightElbow.localRotation;
				}
			}
			else
			{
				if(scaleHierarchicalModelBones)
				{
					UpdateBoneScalings();

					torsoRotation = Quaternion.Slerp(torsoRotation, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso.rotation, deltaTime * rotationDamping);
					torsoDirection = torsoRotation * Vector3.down;

					// *** NOTE how pelvisOffset is scaled with torso.localScale.x   <-- This assumes uniform scaling of torso
					if(torso == root)
						torso.position = transform.TransformPoint(- torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition)
																  + skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso.rotation * pelvisOffset * torso.localScale.x);
					else
						torso.position = transform.TransformPoint(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso.position - skeletonPosition
																  - torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition)
																  + skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso.rotation * pelvisOffset * torso.localScale.x);

					// HACK TODO: in Kinect 1/2 skeletonManager.skeletons[].torso = skeletonManager.skeletons[].root, so lets use filtered version of that (==skeletonPosition)
					spineDirection = transform.TransformPoint(-torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition - 1));
//					spineDirection = transform.TransformPoint (skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.position - skeletonPosition 
//					                                           - torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition - 1));
					                                           
					// HACK TODO: in Kinect 1/2 skeletonManager.skeletons[].torso = skeletonManager.skeletons[].root, so lets use filtered version of that (==skeletonPosition)
					spineDirection = skeletonPosition - spineDirection;
//					spineDirection = torso.position - spineDirection;
					spineDirection.Normalize();

					// Obtained new body tracking data. TODO test that Kinect 1 still works
//					if(bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame)
					{
						float deltaT;
//						if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID)
//							deltaT = skeletonManager.kinect2FrameDeltaT;
//						else
						deltaT = deltaTime;
						ForceUpdatePosition(ref rightShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, 0, deltaT);
						ForceUpdatePosition(ref leftShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder, 1, deltaT);
						ForceUpdatePosition(ref rightHip, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip, 2, deltaT);
						ForceUpdatePosition(ref leftHip, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip, 3, deltaT);
					}

				}
			}

			if(updateRootPosition)
			{
//				Vector3 newRootPosition = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.position;
//				measuredPos [0] = newRootPosition.x;
//				measuredPos [1] = newRootPosition.y;
//				measuredPos [2] = newRootPosition.z;
//				positionKalman.setR (deltaTime * positionNoiseCovariance);
//				positionKalman.predict ();
//				positionKalman.update (measuredPos);
//				pos = positionKalman.getState ();

				// Root speed scaling is applied here
				transform.localPosition = Vector3.Scale(skeletonPosition, rootSpeedScaling);
//				transform.localPosition = Vector3.Scale(new Vector3 ((float)pos [0], (float)pos [1], (float)pos [2]), rootSpeedScaling);
			}
		} 
		 
		if(characterController)
		{
			// If character controller pivot is PS Move
			if(followMoveController && inputManager)
			{
				psmove = inputManager.GetMoveWand(followMoveID);
				if(psmove)
				{
					float moveYaw = psmove.localRotation.eulerAngles.y;
					trackedDeviceYawRotation = Quaternion.Euler(0, moveYaw, 0);

					if(!skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking)
					{
//						skeletonPosition = psmove.localPosition - trackedDeviceYawRotation * characterController.psmoveOffset;
//						skeletonPosition.y = 0;
						tempPosition = psmove.localPosition - trackedDeviceYawRotation * characterController.psmoveOffset;
						skeletonPosition.x = tempPosition.x;
						skeletonPosition.z = tempPosition.z;

						if(characterController.headRotatesBody)
							tempRotation = UpdateTransformWithTrackedDevice(ref root, moveYaw);
//							UpdateTransformWithPSMove (ref torso,  moveYaw);
//							UpdateTransformWithPSMove (ref head, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftShoulder, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftElbow, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftHand, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightShoulder, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightElbow, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightHand, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftHip, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftKnee, moveYawRotation);
//							UpdateTransformWithPSMove (ref leftFoot, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightHip, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightKnee, moveYawRotation);
//							UpdateTransformWithPSMove (ref rightFoot, moveYawRotation);

						if(updateRootPosition)
							transform.localPosition = skeletonPosition + tempRotation*rootOffset;
					}
				}
			}

			if(followHmdPosition)
			{
				float hmdYaw = 0;
				if(coordinateSystem)
				{
					if(coordinateSystem.applyToRootCoordinates)
					{
//						if(ovrHmdVersion == Ovr.HmdType.DK1 || ovrHmdVersion == Ovr.HmdType.DKHD) //06to08
//							oculusYaw = coordinateSystem.GetOculusRiftOrientationRaw().eulerAngles.y;
//						else //06to08
						{
//							skeletonPosition = coordinateSystem.ConvertLocation(coordinateSystem.GetHmdRawPosition(), RUISDevice.OpenVR);
//							skeletonPosition.y = 0;
							tempPosition = coordinateSystem.ConvertLocation(coordinateSystem.GetHmdRawPosition(), RUISDevice.OpenVR);
							skeletonPosition.x = tempPosition.x;
							skeletonPosition.z = tempPosition.z;
//							oculusYaw = coordinateSystem.ConvertRotation(Quaternion.Inverse(coordinateSystem.GetOculusCameraOrientationRaw()) * coordinateSystem.GetOculusRiftOrientationRaw(),
//						                                          	     RUISDevice.Oculus_DK2).eulerAngles.y; //06to08
							hmdYaw = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), RUISDevice.OpenVR).eulerAngles.y;
						}
					}
					else
					{
//						skeletonPosition = coordinateSystem.GetHmdRawPosition();
//						skeletonPosition.y = 0;
						tempPosition = coordinateSystem.GetHmdRawPosition();
						skeletonPosition.x = tempPosition.x;
						skeletonPosition.z = tempPosition.z;
						hmdYaw = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head).eulerAngles.y;
					}
				}

				trackedDeviceYawRotation = Quaternion.Euler(0, hmdYaw, 0);

				if(characterController.headRotatesBody)
					tempRotation = UpdateTransformWithTrackedDevice(ref root, hmdYaw);
					
				if(updateRootPosition) 
					transform.localPosition = skeletonPosition + tempRotation*rootOffset;
				
			}
		}

		TweakHipPosition();
		TweakNeckHeight();
	}

	private void SetCustomJointData(Transform sourceTransform, ref RUISSkeletonManager.JointData jointToSet, float posConfidence, float rotConfidence)
	{
		if(sourceTransform)
		{
			jointToSet.rotation = coordinateSystem.ConvertRotation(RUISCoordinateSystem.ConvertRawRotation(sourceTransform.rotation, 
																										   deviceConversion), customSourceDevice);
			jointToSet.position = coordinateSystem.ConvertLocation(RUISCoordinateSystem.ConvertRawLocation(sourceTransform.position, 
																										   deviceConversion), customSourceDevice);
			jointToSet.positionConfidence = posConfidence;
			jointToSet.rotationConfidence = rotConfidence;
		}
	}

	private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet, float maxAngularVelocity)
	{
		if(transformToUpdate == null)
		{
			return;
		}

		if(updateJointPositions && !useHierarchicalModel && jointToGet.positionConfidence >= minimumConfidenceToUpdate)
		{
			// HACK TODO: in Kinect 1/2 skeletonManager.skeletons[].torso = skeletonManager.skeletons[].root, so lets use filtered version of that (==skeletonPosition)
			if(jointToGet.jointID == RUISSkeletonManager.Joint.Torso)
				transformToUpdate.localPosition = pelvisOffset;
			else
				transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
		}

		Quaternion rotationOffset = Quaternion.identity;

		if(jointToGet.jointID == RUISSkeletonManager.Joint.LeftFoot)
			rotationOffset = Quaternion.Euler(feetRotationOffset);
		if(jointToGet.jointID == RUISSkeletonManager.Joint.RightFoot)
			rotationOffset = Quaternion.Euler(feetRotationOffset);

		if(updateJointRotations && jointToGet.rotationConfidence >= minimumConfidenceToUpdate)
		{
			if(useHierarchicalModel)
			{
				Quaternion newRotation = transform.rotation * jointToGet.rotation * rotationOffset *
				                                     (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
				transformToUpdate.rotation = Quaternion.RotateTowards(transformToUpdate.rotation, newRotation, maxAngularVelocity);
			}
			else
			{	// *** TODO check that rotationOffset multiplication belongs to right side
				transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation,  jointToGet.rotation * rotationOffset, maxAngularVelocity);
			}
		}
	}

	// Here tracked device can mean PS Move or Oculus Rift DK2+
	private Quaternion UpdateTransformWithTrackedDevice(ref Transform transformToUpdate, float controllerYaw)
	{
		Quaternion yaw = Quaternion.identity;

		if(transformToUpdate == null)
			return yaw;
		
		if(updateJointRotations)
		{
			if(useHierarchicalModel)
			{
//                Quaternion newRotation = transform.rotation * jointToGet.rotation *
//                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
//                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, deltaTime * rotationDamping);
				yaw = Quaternion.Euler(new Vector3(0, controllerYaw, 0));
				Quaternion newRotation = transform.rotation * yaw *
				                         (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
				transformToUpdate.rotation = newRotation;
				return yaw;
			}
			else
			{
				transformToUpdate.localRotation = Quaternion.Euler(new Vector3(0, controllerYaw, 0));
				return transformToUpdate.localRotation;
//              transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, deltaTime * rotationDamping);
			}
		}
		return yaw;
	}
			
	private Vector3 jointOffset = Vector3.zero; // *** OPTIHACK

	private void ForceUpdatePosition(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet, int jointID, float deltaT)
	{
		if(transformToUpdate == null)
			return;

//		if(jointID == 2 || jointID == 3) // HACK: for now saving performance by not filtering hips
//			transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);
//		else
		{
			if(filterPosition)
			{
				measuredPos[0] = jointToGet.position.x;
				measuredPos[1] = jointToGet.position.y;
				measuredPos[2] = jointToGet.position.z;

				fourJointsKalman[jointID].setR(deltaT * fourJointsNoiseCovariance);
				fourJointsKalman[jointID].predict();
				fourJointsKalman[jointID].update(measuredPos);
				pos = fourJointsKalman[jointID].getState();

				fourJointPositions[jointID].Set((float)pos[0], (float)pos[1], (float)pos[2]);
			}
			else
				fourJointPositions[jointID] = jointToGet.position;
			

			switch(jointToGet.jointID) // *** OPTIHACK
			{
				case RUISSkeletonManager.Joint.LeftShoulder:
					jointOffset = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder.rotation * shoulderOffset;
					break;
				case RUISSkeletonManager.Joint.RightShoulder:
					jointOffset.Set(-shoulderOffset.x, shoulderOffset.y, shoulderOffset.z);
					jointOffset = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder.rotation * jointOffset;
					break;
				case RUISSkeletonManager.Joint.LeftHip:
					jointOffset = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip.rotation * hipOffset;
					break;
				case RUISSkeletonManager.Joint.RightHip:
					jointOffset.Set(-hipOffset.x, hipOffset.y, hipOffset.z);
					jointOffset = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip.rotation * jointOffset;
					break;
				default:
					jointOffset.Set(0, 0, 0);
					break;
			}
			
			// *** NOTE the use of transformToUpdate.parent.localScale.x <-- assumes uniform scale from the parent (clavicle/torso)
			transformToUpdate.position = transform.TransformPoint(fourJointPositions[jointID] - skeletonPosition + jointOffset * transformToUpdate.parent.localScale.x);
		}
//		transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);
	}

	private Vector3 newRootPosition;

	// Gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
	private void UpdateSkeletonPosition()
	{
		if(filterPosition)
		{
			newRootPosition = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].root.position;

			measuredPos[0] = newRootPosition.x;
			measuredPos[1] = newRootPosition.y;
			measuredPos[2] = newRootPosition.z;
			positionKalman.setR(deltaTime * positionNoiseCovariance); // HACK doesn't take into account Kinect's own update deltaT
			positionKalman.predict();
			positionKalman.update(measuredPos);
			pos = positionKalman.getState();

			skeletonPosition = new Vector3((float)pos[0], (float)pos[1], (float)pos[2]);
		}
		else
			skeletonPosition = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].root.position;
			
//		if (skeletonManager.skeletons[bodyTrackingDeviceID, playerId].root.positionConfidence >= minimumConfidenceToUpdate)
//        {
//			skeletonPosition = skeletonManager.skeletons[bodyTrackingDeviceID, playerId].root.position;
//        }
	}
	
	private void SaveInitialLocalPosition(Transform bodyPart)
	{
		if(bodyPart)
			jointInitialLocalPositions[bodyPart] = bodyPart.localPosition;
	}

	private void SaveInitialRotation(Transform bodyPart)
	{
		if(bodyPart)
			jointInitialRotations[bodyPart] = GetInitialRotation(bodyPart);
	}

	private void SaveInitialDistance(Transform rootTransform, Transform distanceTo)
	{
		Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
		jointInitialDistances.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), 
			Vector3.Distance(Vector3.Scale(rootTransform.position, scaler), Vector3.Scale(distanceTo.position, scaler)));
	}

	private Quaternion GetInitialRotation(Transform bodyPart)
	{
		return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
	}
			
	private float cumulativeScale = 1;
	private float limbStartScale = 1;
	private float neckScale = 1;

	private void UpdateBoneScalings()
	{
		if(!ConfidenceGoodEnoughForScaling())
			return;

		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK
		{
			cumulativeScale = UpdateUniformBoneScaling(torso, chest, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].torso, 
			                                           skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest, pelvisScaleAdjust, 1);
			torsoScale = cumulativeScale;

			cumulativeScale = UpdateUniformBoneScaling(chest, neck, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest, 
			                                           skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck, chestScaleAdjust, cumulativeScale);
			
			if(!neckParentsShoulders)
				neckScale = cumulativeScale;

			cumulativeScale = UpdateUniformBoneScaling(neck, head, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck, 
			                                           skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head, neckScaleAdjust, cumulativeScale);
			if(neckParentsShoulders)
				neckScale = cumulativeScale;

			if(head && neckScale != 0) // cumulativeScale contains the accumulated scale of head's parent
				head.localScale = (headScaleAdjust / cumulativeScale) * Vector3.one; 
		}
		else
		{
			torsoScale = UpdateTorsoScale();
			limbStartScale = torsoScale; 
			if(head && neckScale != 0)
				head.localScale = (headScaleAdjust/torsoScale) * Vector3.one; // *** OPTIHACK added this
		}

		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK
			limbStartScale = UpdateUniformBoneScaling(rightClavicle, rightShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightClavicle, 
				                                      skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, clavicleScaleAdjust, neckScale);
			
		cumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, 
											skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightElbow, limbStartScale);
		UpdateBoneScaling(rightElbow, rightHand, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightElbow, 
						  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHand, cumulativeScale);

		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK
			limbStartScale = UpdateUniformBoneScaling(leftClavicle, leftShoulder, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftClavicle, 
				                                      skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder, clavicleScaleAdjust, neckScale);

		cumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder, 
											skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftElbow, limbStartScale);
		UpdateBoneScaling(leftElbow, leftHand, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftElbow, 
						  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHand, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(rightHip, rightKnee, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip, 
											skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightKnee, torsoScale);
		UpdateBoneScaling(rightKnee, rightFoot, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightKnee, 
						  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightFoot, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(leftHip, leftKnee, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip, 
											skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftKnee, torsoScale);
		UpdateBoneScaling(leftKnee, leftFoot, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftKnee, 
						  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftFoot, cumulativeScale);
	}

	private float UpdateUniformBoneScaling( Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, 
		                                   RUISSkeletonManager.JointData comparisonBoneTracker, float adjustScale, float accumulatedScale		)
	{
		if(!boneToScale)
			return accumulatedScale;
			

		float modelBoneLength = 1;
		float extremityTweaker = 1;
		float playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);

		if(boneToScale && comparisonBone)
			modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];

		float newScale = playerBoneLength / modelBoneLength;

		switch(boneToScaleTracker.jointID)
		{
			case RUISSkeletonManager.Joint.LeftKnee:   extremityTweaker = shinLengthRatio; break;
			case RUISSkeletonManager.Joint.RightKnee:  extremityTweaker = shinLengthRatio; break;
			case RUISSkeletonManager.Joint.LeftElbow:  extremityTweaker = forearmLengthRatio; break;
			case RUISSkeletonManager.Joint.RightElbow: extremityTweaker = forearmLengthRatio; break;
		}

		boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, adjustScale * extremityTweaker * (newScale / accumulatedScale) * Vector3.one,
			 										 maxScaleFactor * deltaTime);

		switch(boneLengthAxis)
		{
			case RUISAxis.X: return accumulatedScale * boneToScale.localScale.x;
			case RUISAxis.Y: return accumulatedScale * boneToScale.localScale.y;
			case RUISAxis.Z: return accumulatedScale * boneToScale.localScale.z;
		}
		return accumulatedScale * boneToScale.localScale.x;
	}
			
	private float UpdateBoneScaling(Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, 
									RUISSkeletonManager.JointData comparisonBoneTracker, float accumulatedScale)
	{
		if(!boneToScale)
			return accumulatedScale;

		float modelBoneLength = 1;
		float playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
		float newScale = 1;
		float thickness = 1;
		float parentBoneThickness = 1;
		float extremityTweaker = 1;
		float skewedScaleTweak = 1;
		float thicknessU = 1;
		float thicknessV = 1;
		bool isExtremityJoint = false;
		Vector3 previousScale = Vector3.one;

		if(boneToScale && comparisonBone)
			modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];

		newScale = playerBoneLength / modelBoneLength; //playerBoneLength / modelBoneLength / accumulatedScale;
		
		switch(boneToScaleTracker.jointID)
		{
			case RUISSkeletonManager.Joint.LeftHip:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				previousScale = prevLeftHip;
				break;
			case RUISSkeletonManager.Joint.RightHip:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				previousScale = prevRightHip;
				break;
			case RUISSkeletonManager.Joint.LeftShoulder:
				thickness = leftArmThickness;
				thicknessU = leftArmThickness;
				thicknessV = leftArmThickness;
				previousScale = prevLeftShoulder;
				break;
			case RUISSkeletonManager.Joint.RightShoulder:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				previousScale = prevRightShoulder;
				break;
			case RUISSkeletonManager.Joint.LeftKnee:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevLeftShinScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.RightKnee:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevRightShinScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.LeftElbow:
				thickness = leftArmThickness;
				thicknessU = leftArmThickness;
				thicknessV = leftArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevLeftForearmScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.RightElbow:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevRightForearmScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.LeftHand:
				thickness = leftArmThickness;
				thicknessU = leftArmThickness;
				thicknessV = leftArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevLeftHandScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.RightHand:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevRightHandScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.LeftFoot:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevLeftFootScale;
				isExtremityJoint = true;
				break;
			case RUISSkeletonManager.Joint.RightFoot:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevRightFootScale;
				isExtremityJoint = true;
				break;
		}
		

		if(scaleBoneLengthOnly)
		{
				
			if(isExtremityJoint && boneToScale.parent && comparisonBone)
			{
				Vector3 u, v, w; // *** TODO remove w
				switch(boneLengthAxis)
				{
					case RUISAxis.X: u = Vector3.up;      v = Vector3.forward; w = Vector3.right;   break;
					case RUISAxis.Y: u = Vector3.forward; v = Vector3.right;   w = Vector3.up;      break;
					case RUISAxis.Z: u = Vector3.right;   v = Vector3.up;      w = Vector3.forward; break;
					default: u = Vector3.up; v = Vector3.forward; break;
				}
				// *** OPTIHACK2
//				float jointAngle = Vector3.Angle(boneToScale.position - comparisonBone.position, boneToScale.parent.position - boneToScale.position);
//				float cosAngle = Mathf.Cos(Mathf.Deg2Rad * jointAngle);
//				float sinAngle = Mathf.Sin(Mathf.Deg2Rad * jointAngle);

//				skewedScaleTweak = extremityTweaker / (accumulatedScale * cosAngle * cosAngle + thickness * sinAngle * sinAngle);
				skewedScaleTweak = extremityTweaker * CalculateScale(boneToScale.position - comparisonBone.position, 
																	 boneToScale.parent.position - boneToScale.position, thickness, accumulatedScale);

				// Below is a bit of a hack (average of thickness and accumulatedScale). A proper solution would have two thickness axes
//				thickness = thickness / (0.5f*(thickness + accumulatedScale) * sinAngle * sinAngle + thickness * cosAngle * cosAngle);
					thicknessU = thickness * CalculateScale(boneToScale.rotation * u, 
						boneToScale.parent.position - boneToScale.position, thickness, accumulatedScale);
					thicknessV = thickness * CalculateScale(boneToScale.rotation * v, 
						boneToScale.parent.position - boneToScale.position, thickness, accumulatedScale);
			
			}
			else
			{
				skewedScaleTweak = extremityTweaker / accumulatedScale;
				thickness  /= accumulatedScale;
				thicknessU /= accumulatedScale;
				thicknessV /= accumulatedScale;
			}

			switch(boneLengthAxis) // Calculating untweaked scales
			{
				case RUISAxis.X:
					boneToScale.localScale = new Vector3(skewedScaleTweak * Mathf.MoveTowards(previousScale.x, newScale, maxScaleFactor * deltaTime), thicknessU, thicknessV);
					break;
				case RUISAxis.Y:
					boneToScale.localScale = new Vector3(thicknessV, skewedScaleTweak * Mathf.MoveTowards(previousScale.y, newScale, maxScaleFactor * deltaTime), thicknessU);
					break;
				case RUISAxis.Z:
					boneToScale.localScale = new Vector3(thicknessU, thicknessV, skewedScaleTweak * Mathf.MoveTowards(previousScale.z, newScale, maxScaleFactor * deltaTime));
					break;
			}

			// Save untweaked scales
			switch(boneToScaleTracker.jointID)
			{
				case RUISSkeletonManager.Joint.LeftHip:
					prevLeftHip = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftKnee:  
					prevLeftShinScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftFoot:  
					prevLeftFootScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftShoulder:
					prevLeftShoulder = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftElbow:   
					prevLeftForearmScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftHand:   
					prevLeftHandScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightShoulder:
					prevRightShoulder = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightElbow: 
					prevRightForearmScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightHand: 
					prevRightHandScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightHip:
					prevRightHip = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightKnee:   
					prevRightShinScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightFoot:   
					prevRightFootScale = newScale * Vector3.one;
					break;
			}

			// *** There are actually two disting thickness values (axes orthogonal to bone length), there is no way around it
		}
		else
			boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, extremityTweaker * 
														 (newScale / accumulatedScale) * Vector3.one, maxScaleFactor * deltaTime);


//			if(boneToScaleTracker.jointID == RUISSkeletonManager.Joint.RightElbow)
//				print(skewedScaleTweak + " " + thicknessU + " " + thicknessV + " " + newScale + " " + boneToScale.localScale);

		switch(boneLengthAxis)
		{
			case RUISAxis.X: return accumulatedScale * boneToScale.localScale.x;
			case RUISAxis.Y: return accumulatedScale * boneToScale.localScale.y;
			case RUISAxis.Z: return accumulatedScale * boneToScale.localScale.z;
		}
		return accumulatedScale * boneToScale.localScale.x;
	}


	private float CalculateScale(Vector3 direction, Vector3 parentDirection, float parentThicknessScale, float parentLengthScale)
	{
		float jointAngle = Vector3.Angle(direction, parentDirection);
		float cosAngle = Mathf.Cos(Mathf.Deg2Rad * jointAngle);
		float sinAngle = Mathf.Sin(Mathf.Deg2Rad * jointAngle);

		return 1 / (parentLengthScale * cosAngle * cosAngle + parentThicknessScale * sinAngle * sinAngle);
	}

	private float UpdateTorsoScale()
	{
		//average hip to shoulder length and compare it to the one found in the model - scale accordingly
		//we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
		float modelLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, leftHip)] +
		                          jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2;
//		float playerLength = (Vector3.Distance(skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightShoulder.position, 
//		                                       skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftShoulder.position) +
//		                      Vector3.Distance(skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightHip.position, 
//		                 					   skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftHip.position)) / 2;
//		float playerLength = (Vector3.Distance( rightShoulder.position,  leftShoulder.position) + // *** THIS IS WRONG, SCALING APPLIES ON THESE TRANSFORMS
//		                      Vector3.Distance(      rightHip.position,       leftHip.position)  ) / 2;
		float playerLength = (Vector3.Distance(fourJointPositions[0], fourJointPositions[1]) +
		                     Vector3.Distance(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip.position,
			                     skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip.position)) / 2;
//		float playerLength = (Vector3.Distance( fourJointPositions[0], fourJointPositions[1]) +
//		                      Vector3.Distance( fourJointPositions[2], fourJointPositions[3])  ) / 2;
		
		float newScale = Mathf.Abs(playerLength / modelLength) * (scaleBoneLengthOnly ? torsoThickness : 1);

		// Here we halve the maxScaleFactor because the torso is bigger than the limbs
		torsoScale = Mathf.Lerp(torsoScale, newScale, 0.5f * maxScaleFactor * deltaTime);

		if(scaleBoneLengthOnly)
		{
//			switch(boneLengthAxis)
//			{
//				case RUISAxis.X:
//					torso.localScale = new Vector3(torsoScale, torsoThickness * torsoScale, torsoThickness * torsoScale);
//					break;
//				case RUISAxis.Y:
//					torso.localScale = new Vector3(torsoThickness * torsoScale, torsoScale, torsoThickness * torsoScale);
//					break;
//				case RUISAxis.Z:
//					torso.localScale = new Vector3(torsoThickness * torsoScale, torsoThickness * torsoScale, torsoScale);
//					break;
//			}
			torso.localScale = torsoScale * Vector3.one;
		}
		else
			torso.localScale = torsoScale * Vector3.one;
		return torsoScale;
	}

	private Quaternion FindFixingRotation(Vector3 fromJoint, Vector3 toJoint, Vector3 wantedDirection)
	{
		Vector3 boneVector = toJoint - fromJoint;
		return Quaternion.FromToRotation(boneVector, wantedDirection);
	}

	private void TweakNeckHeight()
	{
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker || !neck)
			return;
		neck.localPosition = neckOriginalLocalPosition + neck.InverseTransformDirection(spineDirection) * neckHeightTweaker / torsoScale;
	}

	private void TweakHipPosition()
	{
		if(!tweakableHips)
			return;
		// TODO: Below needs to be modified
		//chest.position -= hipOffset;
		chest.localPosition = chestOriginalLocalPosition - chest.InverseTransformDirection(spineDirection.normalized)
																										* adjustVerticalHipsPosition / torsoScale;
	}

	public bool ConfidenceGoodEnoughForScaling()
	{
		return !(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder.positionConfidence	< minimumConfidenceToUpdate ||
				 skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftShoulder.positionConfidence	< minimumConfidenceToUpdate ||
				 skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip.positionConfidence		< minimumConfidenceToUpdate ||
				 skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip.positionConfidence		< minimumConfidenceToUpdate   );
	}

	private void handleFingersCurling(bool trackThumbs)
	{
		bool closeHand;
		int inv = 1;
		float rotationSpeed = 10.0f; // Per second
		Quaternion clenchedRotationThumbTM_corrected  = Quaternion.identity;
		Quaternion clenchedRotationThumbMCP_corrected = Quaternion.identity;
		Quaternion clenchedRotationThumbIP_corrected  = Quaternion.identity;
		
		RUISSkeletonManager.Skeleton.handState currentLeftHandStatus = leftHandStatus;
		RUISSkeletonManager.Skeleton.handState currentRightHandStatus = rightHandStatus;

		if(externalCurlTrigger)
		{
			currentLeftHandStatus = externalLeftStatus;
			currentRightHandStatus = externalRightStatus;
		}

		if(   currentLeftHandStatus == RUISSkeletonManager.Skeleton.handState.unknown
		   || currentLeftHandStatus == RUISSkeletonManager.Skeleton.handState.pointing)
		{
			currentLeftHandStatus = lastLeftHandStatus;
		}
		
		if(   currentRightHandStatus == RUISSkeletonManager.Skeleton.handState.unknown
		   || currentRightHandStatus == RUISSkeletonManager.Skeleton.handState.pointing)
		{
			currentRightHandStatus = lastRightHandStatus;
		}
		
		lastLeftHandStatus = currentLeftHandStatus;
		lastRightHandStatus = currentRightHandStatus;
		
		for(int i = 0; i < 2; i++)
		{ // Hands
			if(i == 0)
			{
				closeHand = (currentRightHandStatus == RUISSkeletonManager.Skeleton.handState.closed);
				inv = -1;
			}
			else
			{
				closeHand = (currentLeftHandStatus == RUISSkeletonManager.Skeleton.handState.closed);	
				inv = 1;
			}
			// Thumb rotation correction: these depend on your animation rig
			switch(boneLengthAxis)
			{
				case RUISAxis.X:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler(clenchedRotationThumbTM.eulerAngles.x * inv, 
																		  clenchedRotationThumbTM.eulerAngles.y, clenchedRotationThumbTM.eulerAngles.z);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedRotationThumbMCP.eulerAngles.x * inv, 
																		  clenchedRotationThumbMCP.eulerAngles.y, clenchedRotationThumbMCP.eulerAngles.z);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler(clenchedRotationThumbIP.eulerAngles.x * inv, 
																		  clenchedRotationThumbIP.eulerAngles.y, clenchedRotationThumbIP.eulerAngles.z);
				break;
				case RUISAxis.Y:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler(clenchedRotationThumbTM.eulerAngles.x, clenchedRotationThumbTM.eulerAngles.y,
					                                                      clenchedRotationThumbTM.eulerAngles.z * inv);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedRotationThumbMCP.eulerAngles.x, clenchedRotationThumbMCP.eulerAngles.y, 
						                                                  clenchedRotationThumbMCP.eulerAngles.z * inv);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler(clenchedRotationThumbIP.eulerAngles.x, clenchedRotationThumbIP.eulerAngles.y, 
						                                                  clenchedRotationThumbIP.eulerAngles.z * inv);
					break;
				case RUISAxis.Z:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler(clenchedRotationThumbTM.eulerAngles.x,
																		  clenchedRotationThumbTM.eulerAngles.y  * inv, clenchedRotationThumbTM.eulerAngles.z);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedRotationThumbMCP.eulerAngles.x,
																		  clenchedRotationThumbMCP.eulerAngles.y * inv, clenchedRotationThumbMCP.eulerAngles.z);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler(clenchedRotationThumbIP.eulerAngles.x,
																		  clenchedRotationThumbIP.eulerAngles.y  * inv, clenchedRotationThumbIP.eulerAngles.z);
					break;
			}
			// If left thumb rotatioons have been set separately
			if(leftThumbHasIndependentRotations && i == 1) // i == 1 is left hand
			{
				clenchedRotationThumbTM_corrected 	= clenchedLeftThumbTM;
				clenchedRotationThumbMCP_corrected 	= clenchedLeftThumbMCP;
				clenchedRotationThumbIP_corrected 	= clenchedLeftThumbIP;
			}

			for(int a = 0; a < 5; a++)
			{ // Fingers
				if(!closeHand && !(a == 4 && trackThumbs))
				{
					if(fingerTransforms[i, a, 0])
						fingerTransforms[i, a, 0].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 0].localRotation, initialFingerRotations[i, a, 0], deltaTime * rotationSpeed);
					if(fingerTransforms[i, a, 1])
						fingerTransforms[i, a, 1].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 1].localRotation, initialFingerRotations[i, a, 1], deltaTime * rotationSpeed);
					if(fingerTransforms[i, a, 2])
						fingerTransforms[i, a, 2].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 2].localRotation, initialFingerRotations[i, a, 2], deltaTime * rotationSpeed);
				}
				else
				{
					if(a != 4)
					{
						if(fingerTransforms[i, a, 0])
							fingerTransforms[i, a, 0].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 0].localRotation, clenchedRotationMCP, deltaTime * rotationSpeed);
						if(fingerTransforms[i, a, 1])
							fingerTransforms[i, a, 1].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 1].localRotation, clenchedRotationPIP, deltaTime * rotationSpeed);
						if(fingerTransforms[i, a, 2])
							fingerTransforms[i, a, 2].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 2].localRotation, clenchedRotationDIP, deltaTime * rotationSpeed);
					}
					else if(!trackThumbs)
					{ // Thumbs (if separate thumb  tracking is not enabled)
						if(fingerTransforms[i, a, 0])
							fingerTransforms[i, a, 0].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 0].localRotation,  clenchedRotationThumbTM_corrected, deltaTime * rotationSpeed);
						if(fingerTransforms[i, a, 1])
							fingerTransforms[i, a, 1].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 1].localRotation, clenchedRotationThumbMCP_corrected, deltaTime * rotationSpeed);
						if(fingerTransforms[i, a, 2])
							fingerTransforms[i, a, 2].localRotation = Quaternion.Slerp(fingerTransforms[i, a, 2].localRotation,  clenchedRotationThumbIP_corrected, deltaTime * rotationSpeed);
					}	
				}	
			}
		}
	}

	private void SaveInitialFingerRotations()
	{	
		Transform handObject;
		
		for(int i = 0; i < 2; i++)
		{ 
			if(i == 0)
				handObject = rightHand;
			else
				handObject = leftHand;

			if(handObject == null)
				continue;

			Transform[] fingers = handObject.GetComponentsInChildren<Transform>();
			
			int fingerIndex = 0;
			int index = 0;
			foreach(Transform finger in fingers)
			{
				if(finger.parent.transform.gameObject == handObject.transform.gameObject
				    && (finger.gameObject.name.Contains("finger") || finger.gameObject.name.Contains("Finger") || finger.gameObject.name.Contains("FINGER")))
				{
				
					if(fingerIndex > 4)
						break; // No mutant fingers allowed!
					
					if(finger == rightThumb || finger == leftThumb)
						index = 4; // Force thumb to have index == 4
					else
					{
						index = fingerIndex;
						fingerIndex++;
					}
				
					// First bone
					initialFingerRotations[i, index, 0] = finger.localRotation;
					fingerTransforms[i, index, 0] = finger;
					Transform[] nextFingerParts = finger.gameObject.GetComponentsInChildren<Transform>();
					foreach(Transform part1 in nextFingerParts)
					{
						if(part1.parent.transform.gameObject == finger.gameObject
						    && (part1.gameObject.name.Contains("finger") || part1.gameObject.name.Contains("Finger") || part1.gameObject.name.Contains("FINGER")))
						{
							// Second bone
							initialFingerRotations[i, index, 1] = part1.localRotation;
							fingerTransforms[i, index, 1] = part1;
							Transform[] nextFingerParts2 = finger.gameObject.GetComponentsInChildren<Transform>();
							foreach(Transform part2 in nextFingerParts2)
							{
								if(part2.parent.transform.gameObject == part1.gameObject
								    && (part2.gameObject.name.Contains("finger") || part2.gameObject.name.Contains("Finger") || part2.gameObject.name.Contains("FINGER")))
								{
									// Third bone
									initialFingerRotations[i, index, 2] = part2.localRotation;
									fingerTransforms[i, index, 2] = part2; 
								}
							}
						}
					}
				}
			}	
		}
	}

	
	private Quaternion limitZRotation(Quaternion inputRotation, float rollMinimum, float rollMaximum)
	{
		/**
		 * Argument inputRotation's roll angle (rotation around Z axis) is clamped between [rollMinimum, rollMaximum].
		 * Works only if effective rotation around Y axis is zero. Rotation around X axis is allowed though.
		 **/
	 
		float rollAngle = 0;
		Vector3 limitedRoll = Vector3.zero;
		Quaternion outputRotation = inputRotation;
		
		// Calculate the rotation of inputRotation where roll (rotation around Z axis) is omitted
		Quaternion rotationWithoutRoll = Quaternion.LookRotation(inputRotation * Vector3.forward, 
			                                 Vector3.Cross(inputRotation * Vector3.forward, Vector3.right)); 
		rollAngle = Quaternion.Angle(inputRotation, rotationWithoutRoll);
		
		// Is the roll to the left or to the right? Quaternion.Angle returns only positive values and omits rotation "direction"
		if((inputRotation * Vector3.up).x > 0)
			rollAngle *= -1;
		
		if(rollAngle > rollMaximum) // Rolling towards or over maximum angle
		{
			// Clamp to nearest limit
			if(rollAngle - rollMaximum < 0.5f * (360 - rollMaximum + rollMinimum))
				limitedRoll.z = rollMaximum;
			else
				limitedRoll.z = rollMinimum;
			outputRotation = rotationWithoutRoll * Quaternion.Euler(limitedRoll);
		}
		if(rollAngle < rollMinimum) // Rolling towards or below minimum angle
		{
			// Clamp to nearest limit
			if(rollMinimum - rollAngle < 0.5f * (360 - rollMaximum + rollMinimum))
				limitedRoll.z = rollMinimum;
			else
				limitedRoll.z = rollMaximum;
			outputRotation = rotationWithoutRoll * Quaternion.Euler(limitedRoll);
		}
		
		print(rollAngle + " " + rotationWithoutRoll.eulerAngles);
		
		return outputRotation;
	}
	
}
