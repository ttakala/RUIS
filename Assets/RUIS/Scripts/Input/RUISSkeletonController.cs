/*****************************************************************************

Content    :   Functionality to control a skeleton using Kinect or mocap systems
Authors    :   Mikael Matveinen, Tuukka Takala, Heikki Heiskanen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen.
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

[DisallowMultipleComponent]
public class RUISSkeletonController : MonoBehaviour
{
	public RUISSkeletonManager.Skeleton skeleton = new RUISSkeletonManager.Skeleton();

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
	public Transform leftIndexF;
	public Transform rightIndexF;
	public Transform leftMiddleF;
	public Transform rightMiddleF;
	public Transform leftRingF;
	public Transform rightRingF;
	public Transform leftLittleF;
	public Transform rightLittleF;

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

	public Transform customRightThumb;
	public Transform customRightIndexF;
	public Transform customRightMiddleF;
	public Transform customRightRingF;
	public Transform customRightLittleF;
	public Transform customLeftThumb;
	public Transform customLeftIndexF;
	public Transform customLeftMiddleF;
	public Transform customLeftRingF;
	public Transform customLeftLittleF;

	public Transform customHMDSource;
	public RUISDevice headsetCoordinates = RUISDevice.OpenVR; 

	public Vector3 pelvisOffset   = Vector3.zero;
	public Vector3 chestOffset	  = Vector3.zero;
	public Vector3 neckOffset	  = Vector3.zero;
	public Vector3 headOffset	  = Vector3.zero;
	public Vector3 clavicleOffset = Vector3.zero;
	public Vector3 shoulderOffset = Vector3.zero;
	public Vector3 elbowOffset 	  = Vector3.zero;
	public Vector3 handOffset 	  = Vector3.zero;
	public Vector3 hipOffset 	  = Vector3.zero;
	public Vector3 kneeOffset 	  = Vector3.zero;
	public Vector3 footOffset 	  = Vector3.zero;

	public Vector3 pelvisRotationOffset   = Vector3.zero;
	public Vector3 chestRotationOffset	  = Vector3.zero;
	public Vector3 neckRotationOffset	  = Vector3.zero;
	public Vector3 headRotationOffset	  = Vector3.zero;
	public Vector3 clavicleRotationOffset = Vector3.zero;
	public Vector3 shoulderRotationOffset = Vector3.zero;
	public Vector3 elbowRotationOffset    = Vector3.zero;
	public Vector3 handRotationOffset 	  = Vector3.zero;
	public Vector3 hipRotationOffset 	  = Vector3.zero;
	public Vector3 kneeRotationOffset     = Vector3.zero;
	public Vector3 feetRotationOffset 	  = Vector3.zero;

	public Vector3 thumbRotationOffset	  = Vector3.zero;
	public Vector3 indexFRotationOffset   = Vector3.zero;
	public Vector3 middleFRotationOffset  = Vector3.zero;
	public Vector3 ringFRotationOffset    = Vector3.zero;
	public Vector3 littleFRotationOffset  = Vector3.zero;

	public float pelvisScaleAdjust   = 1;
	public float chestScaleAdjust 	 = 1;
	public float neckScaleAdjust 	 = 1;
	public float headScaleAdjust 	 = 1;
	public float clavicleScaleAdjust = 1;
//	public float shoulderScaleAdjust = 1; // TODO is this needed?
//	public float elbowScaleAdjust    = 1; // TODO is this needed?
	public float handScaleAdjust 	 = 1; // left and right separately
//	public float hipScaleAdjust 	 = 1; // TODO is this needed?
//	public float kneeScaleAdjust     = 1; // TODO is this needed?
	public float footScaleAdjust 	 = 1; // left and right separately

	/// <summary>
	/// If enabled, any finger tracking is overriden and externalLeftStatus and externalRightStatus variables can be used to open and close fist. 
	/// </summary>
	public bool externalFistTrigger = false;
	public bool fistMaking = true;
	public bool kinect2Thumbs = false;
	public bool trackWrist = false;
	public bool trackAnkle = false;
	public bool rotateWristFromElbow = true;
	public bool independentTorsoSegmentsScaling = false;
	public bool heightAffectsOffsets = false; // TODO

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
	public RUISKinectAndMecanimCombiner mecanimCombiner;

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
								customSourceDevice = RUISDevice.None;
								break;
						}
					}
					break;
			}
		}
	}

	public CustomConversionType customConversionType = CustomConversionType.None;
	private RUISCoordinateSystem.DeviceCoordinateConversion deviceConversion;
	private RUISDevice customSourceDevice = RUISDevice.None;

	public int playerId = 0;
	public bool switchToAvailableKinect = false;

	private Vector3 skeletonPosition = Vector3.zero;
	private Vector3 tempVector = Vector3.zero;
	private Quaternion tempRotation = Quaternion.identity;

	private bool hasBeenTracked = false;

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
	public bool limbsAreScaled = true;

	public float torsoThickness = 1;
	public float rightArmThickness = 1;
	public float leftArmThickness = 1;
	public float rightLegThickness = 1;
	public float leftLegThickness = 1;

	public float minimumConfidenceToUpdate = 0.5f;
	public float maxAngularVelocity = 360.0f;
	public float maxFingerAngularVelocity = 720.0f;

	// Constrained between [0, -180] in Unity Editor script
	public float handRollAngleMinimum = -180; 

	// Constrained between [0,  180] in Unity Editor script
	public float handRollAngleMaximum = 180; 

	public bool hmdRotatesHead = false;
	public bool hmdMovesHead = false;
	public bool followHmdPosition { get; private set; }
	public Vector3 hmdLocalOffset = Vector3.zero;

	public bool headsetDragsBody = false;
	public bool yawCorrectIMU = false;
	public float yawCorrectAngularVelocity = 2;
	public KeyCode yawCorrectResetButton = KeyCode.Space;
	KalmanFilter filterDrift;
	double[] measuredDrift = {0, 0};
	double[] filteredDrift = {0, 0};
	float driftNoiseCovariance = 5000;
	Quaternion rotationDrift = Quaternion.identity;
	Vector3 driftingForward;
	Vector3 driftlessForward;
	Vector3 driftVector;
	float initCorrectionVelocity = 36000;
	float currentCorrectionVelocity = 2;

	public Quaternion trackedDeviceYawRotation { get; private set; }

	public bool followMoveController { get; private set; }

	private int followMoveID = 0;
	private RUISPSMoveWand psmove;

	private Vector3 torsoDirection = Vector3.down;
	private Quaternion torsoRotation = Quaternion.identity;

	private float deltaTime = 0.03f;

	public bool filterPosition = true;
	public bool filterHeadPositionOnly = false;
	private KalmanFilter positionKalman;
	private double[] measuredPos = { 0, 0, 0 };
	private double[] pos = { 0, 0, 0 };
	public float positionNoiseCovariance = 100;

	private KalmanFilter[] fourJointsKalman = new KalmanFilter[5];
	public float fourJointsNoiseCovariance = 50;
	private Vector3[] forcedJointPositions = new Vector3[18];
	
	public bool filterRotations = false;
	public float rotationNoiseCovariance = 200;
	// Offset Z rotation of the thumb. Default value is 45, but it might depend on your avatar rig.
	public float thumbZRotationOffset = 45;

	private Dictionary<Transform, Quaternion> jointInitialRotations;
	private Dictionary<Transform, Vector3> jointInitialWorldPositions;
	private Dictionary<Transform, Vector3> jointInitialLocalPositions;
	private Dictionary<Transform, Vector3> jointInitialLocalScales;
	private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;
	private Dictionary<KeyValuePair<Transform, Transform>, float> trackedBoneLengths;
	private Dictionary<RUISSkeletonManager.Joint, float> automaticBoneScales;
	private Dictionary<RUISSkeletonManager.Joint, float> automaticBoneOffsets;

	private Vector3 initialLocalPosition = Vector3.zero;
	private Vector3 initialWorldPosition = Vector3.zero;
	private Vector3 initialLossyScale = Vector3.one;
	private Quaternion initialWorldRotation = Quaternion.identity;

	private float modelSpineLength = 0;
	private int customSpineJointCount = 0;
	private Transform[] trackedSpineJoints = new Transform[4];
	private RUISSkeletonManager.Joint highestSpineJoint = RUISSkeletonManager.Joint.RightShoulder; // Here RightShoulder refers to shoulders' midpoint

	private float torsoOffset = 0.0f;

	private float torsoScale = 1.0f;

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

	private Vector3 prevRightHandDelta;
	private Vector3 prevRightFootDelta;
	private Vector3 prevLeftHandDelta;
	private Vector3 prevLeftFootDelta;
	
	//	Ovr.HmdType ovrHmdVersion = Ovr.HmdType.None; //06to08

	// 2 hands, 5 fingers, 3 finger bones
	Quaternion[,,] initialFingerLocalRotations = new Quaternion[2, 5, 3];
	Quaternion[,,] initialFingerWorldRotations = new Quaternion[2, 5, 3];
	// For quick access to finger gameobjects
	Transform[,,] fingerTargets = new Transform[2, 5, 3];
	Transform[,,] fingerSources = new Transform[2, 5, 3];
	bool[,,] hasFingerTarget = new bool[2, 5, 3]; // Checking bool value is presumably faster than (fingerTargets[i,j,k] != null)
	bool[,,] hasFingerSource = new bool[2, 5, 3];
	float[,] fingerConfidence = new float[2, 5];

	bool noSourceFingers = true;

	// NOTE: The below clenchedRotation*s are set in Start() method !!! See clause that starts with switch(boneLengthAxis)
	// Thumb phalange rotations when hand is clenched to a fist
//	public Quaternion clenchedRotationThumbTM;
//	public Quaternion clenchedRotationThumbMCP;
//	public Quaternion clenchedRotationThumbIP;
	public Vector3 clenchedThumbAngleTM;
	public Vector3 clenchedThumbAngleMCP;
	public Vector3 clenchedThumbAngleIP;
	
	// Phalange rotations of other fingers when hand is clenched to a fist
//	public Quaternion clenchedRotationMCP;
//	public Quaternion clenchedRotationPIP;
//	public Quaternion clenchedRotationDIP;
	public Vector3 clenchedFingerAngleMCP;
	public Vector3 clenchedFingerAnglePIP;
	public Vector3 clenchedFingerAngleDIP;

	public bool leftThumbHasIndependentRotations = false; // *** OPTITRACK5 remove this
	public Quaternion clenchedLeftThumbTM;
	public Quaternion clenchedLeftThumbMCP;
	public Quaternion clenchedLeftThumbIP;

	public bool keepPlayModeChanges = true; // This is only for the custom inspector to use

	void Awake()
	{
		coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
		if(!coordinateSystem)
		{
			Debug.LogError(typeof(RUISCoordinateSystem) + " not found in the scene! The script will be disabled.");
			this.enabled = false;
		}

		followHmdPosition = false;
		followMoveController = false;
		trackedDeviceYawRotation = Quaternion.identity;
		
		jointInitialRotations 	   = new Dictionary<Transform, Quaternion>();
		jointInitialWorldPositions = new Dictionary<Transform, Vector3>();
		jointInitialLocalPositions = new Dictionary<Transform, Vector3>();
		jointInitialLocalScales    = new Dictionary<Transform, Vector3>();
		jointInitialDistances 	   = new Dictionary<KeyValuePair<Transform, Transform>, float>();
		trackedBoneLengths				   = new Dictionary<KeyValuePair<Transform, Transform>, float>();
		automaticBoneScales 	   = new Dictionary<RUISSkeletonManager.Joint, float>();
		automaticBoneOffsets 	   = new Dictionary<RUISSkeletonManager.Joint, float>();
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3, 3);

		for(int i = 0; i < fourJointsKalman.Length; ++i)
		{
			fourJointsKalman[i] = new KalmanFilter();
			fourJointsKalman[i].initialize(3, 3);
		}
		for(int i = 0; i < forcedJointPositions.Length; ++i)
			forcedJointPositions[i] = Vector3.zero;
		
		for(int i = 0; i < hasFingerTarget.GetLength(0); ++i)
			for(int j = 0; j < hasFingerTarget.GetLength(1); ++j)
				for(int k = 0; k < hasFingerTarget.GetLength(2); ++k)
					hasFingerTarget[i, j, k] = false;

		for(int i = 0; i < hasFingerSource.GetLength(0); ++i)
			for(int j = 0; j < hasFingerSource.GetLength(1); ++j)
				for(int k = 0; k < hasFingerSource.GetLength(2); ++k)
					hasFingerSource[i, j, k] = false;

		for(int i = 0; i < fingerConfidence.GetLength(0); ++i)
			for(int j = 0; j < fingerConfidence.GetLength(1); ++j)
				fingerConfidence[i, j] = 1.0f; // It's up to the developer to modify these values between 0 and 1 in real-time

		filterDrift = new KalmanFilter();
		filterDrift.initialize(2,2);
		currentCorrectionVelocity = yawCorrectAngularVelocity;
		if(yawCorrectIMU)
			StartCoroutine(CorrectYawImmediately());
	}

	void Start()
	{
		skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		if(!skeletonManager)
		{
			Debug.LogError("The scene is missing " + typeof(RUISSkeletonManager) + " script! The script will be disabled.");
			this.enabled = false;
		}

		if(!root)
		{
			Debug.LogError(typeof(RUISSkeletonController) + " of GameObject " + gameObject.name + " has not been assigned with a \"Target Root\" "
				+ "Transform. The script will be disabled.");
			this.enabled = false;
		}

		if(!torso)
		{
			Debug.LogWarning(typeof(RUISSkeletonController) + " of GameObject " + gameObject.name + " has not been assigned with a \"Pelvis Target\" "
							 + "Transform. It will automatically set to the Transform of " + root.name + "(\"Target Root\").");
			torso = root;
		}

		if(!RUISDisplayManager.IsHmdPresent())
		{
			if(headsetDragsBody || yawCorrectIMU || hmdRotatesHead || hmdMovesHead)
				Debug.LogWarning("Head-mounted display is not detected: Unable to comply with the enabled HMD options of " + typeof(RUISSkeletonController) + ".");
		}

		hasBeenTracked = false;

		if(inputManager) // *** OPTIHACK4 moved this here from Awake(), check that everything still works
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

		// *** OPTIHACK4 moved these 6 lines here from Awake(), check that everything still works
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
			BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2)
			BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
			BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
		
		// Disable features that are only available for Kinect2 or custom motion tracker
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
		{
//			fistCurlFingers = false;
			kinect2Thumbs = false;
//			trackWrist = false;
//			trackAnkle = false;
			rotateWristFromElbow = false;
		}

		// Following substitution ensures that customConversionType, customSourceDevice, and deviceConversion variables will be set in bodyTrackingDeviceID setter
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)  // *** OPTIHACK4 moved this here from Awake(), check that everything still works
		{
			BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
			minimumConfidenceToUpdate = 0.5f;
			kinect2Thumbs = false;

			// When it comes to GenericMotionTracker, it's up to the developer to change isTracking values in realtime
			skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking = true;
			skeleton.isTracking = true;
		}
		else
			skeleton.isTracking = false;

		switch(customConversionType)
		{
			case CustomConversionType.Custom_1:
				if(inputManager)
					deviceConversion = inputManager.customDevice1Conversion;
				else
					deviceConversion = new RUISCoordinateSystem.DeviceCoordinateConversion();
				customSourceDevice = RUISDevice.Custom_1;
				break;
			case CustomConversionType.Custom_2:
				if(inputManager)
					deviceConversion = inputManager.customDevice2Conversion;
				else
					deviceConversion = new RUISCoordinateSystem.DeviceCoordinateConversion();
				customSourceDevice = RUISDevice.Custom_2;
				break;
			default:
				deviceConversion = new RUISCoordinateSystem.DeviceCoordinateConversion();
				deviceConversion = null;
				break;
		}

		if(useHierarchicalModel)
		{
			// *** TODO: in which situations we should require the presence of left/rightShoulder & left/rightHip
			//           Torso scaling with Kinect DEPENDS ON EXISTENCE OF ALL 4 OF THOSE TARGET TRANSFORMS
			// Fix all shoulder and hip rotations to match the default kinect rotations (T-pose)
			if(rightShoulder && rightElbow)
				rightShoulder.rotation = FindFixingRotation(rightShoulder.position, rightElbow.position, transform.right) * rightShoulder.rotation;
			if(leftShoulder && leftElbow)
				leftShoulder.rotation = FindFixingRotation(leftShoulder.position, leftElbow.position, -transform.right) * leftShoulder.rotation;
			if(rightHip && rightFoot)
				rightHip.rotation = FindFixingRotation(rightHip.position, rightFoot.position, -transform.up) * rightHip.rotation;
			if(leftHip && leftFoot)
				leftHip.rotation = FindFixingRotation(leftHip.position, leftFoot.position, -transform.up) * leftHip.rotation;

			Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
//			Vector3 assumedRootPos = Vector3.Scale((rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4, scaler); 
			Vector3 assumedRootPos = Vector3.Scale((rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4, scaler); 
															
			Vector3 realRootPos = Vector3.Scale(torso.position, scaler);

			Vector3 torsoUp = head.position - torso.position;
			torsoUp.Normalize();

			// *** OPTIHACK5 Kinect1 and Kinect2 could need differing torsoOffset values... testing needed. This probably should be done in RUISSkeletonManager
//			if(BodyTrackingDeviceID != RUISSkeletonManager.customSensorID) // *** OPTIHACK
//				torsoOffset = Vector3.Dot(realRootPos - assumedRootPos, torsoUp);
		}

		// Stores (world) rotation, LocalPosition, and localScale
		SaveInitialTransform(root);
		SaveInitialTransform(head);
		SaveInitialTransform(neck); // *** OPTIHACK
		SaveInitialTransform(chest); // *** OPTIHACK
		SaveInitialTransform(torso);
		SaveInitialTransform(rightClavicle); // *** OPTIHACK
		SaveInitialTransform(rightShoulder);
		SaveInitialTransform(rightElbow);
		SaveInitialTransform(rightHand);
		SaveInitialTransform(leftClavicle); // *** OPTIHACK
		SaveInitialTransform(leftShoulder);
		SaveInitialTransform(leftElbow);
		SaveInitialTransform(leftHand);
		SaveInitialTransform(rightHip);
		SaveInitialTransform(rightKnee);
		SaveInitialTransform(rightFoot);
		SaveInitialTransform(leftHip);
		SaveInitialTransform(leftKnee);
		SaveInitialTransform(leftFoot);

		SaveInitialTransform(leftThumb);
		SaveInitialTransform(rightThumb);

		FindAndInitializeFingers(fingerTargets, isTargetFingers: true );
		FindAndInitializeFingers(fingerSources, isTargetFingers: false);

		SaveInitialDistance(rightClavicle, rightShoulder);
		SaveInitialDistance(rightShoulder, rightElbow);
		SaveInitialDistance(rightElbow,   rightHand);
		SaveInitialDistance(leftClavicle, leftShoulder);
		SaveInitialDistance(leftShoulder, leftElbow);
		SaveInitialDistance(leftElbow,    leftHand);

		SaveInitialDistance(rightHip,  rightKnee);
		SaveInitialDistance(rightKnee, rightFoot);
		SaveInitialDistance(leftHip,   leftKnee);
		SaveInitialDistance(leftKnee,  leftFoot);

		SaveInitialDistance(rightShoulder, leftShoulder);
		SaveInitialDistance(rightHip, leftHip);

		if(chest)
		{
			modelSpineLength += SaveInitialDistance(torso, chest); // *** OPTIHACK
			if(neck)
				modelSpineLength += SaveInitialDistance(chest, neck); // *** OPTIHACK
			else if(head)
				modelSpineLength += SaveInitialDistance(chest, head);
		}
		if(neck)
		{
			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customNeck)
				highestSpineJoint = RUISSkeletonManager.Joint.Neck;
			if(head)
				modelSpineLength += SaveInitialDistance(neck, head); // *** OPTIHACK
			if(leftClavicle)
				SaveInitialDistance(neck, leftClavicle);
			if(rightClavicle)
				SaveInitialDistance(neck, rightClavicle);
			if(!chest)
				modelSpineLength += SaveInitialDistance(torso, neck); // *** OPTIHACK
		}
		if(head)
		{
			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customHead)
				highestSpineJoint = RUISSkeletonManager.Joint.Head;
			if(!chest && !neck)
				modelSpineLength += SaveInitialDistance(torso, head);
		}
		if(bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker || (!chest && !neck && !head))
		{
			modelSpineLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, 		leftHip)] +
								jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2;
		}
			
		// *** TODO implementation that uses these
		automaticBoneScales[RUISSkeletonManager.Joint.Torso] = 1; // Are we multiplying (1) or adding (0)?
		automaticBoneScales[RUISSkeletonManager.Joint.Chest] = 1;
		automaticBoneScales[RUISSkeletonManager.Joint.Neck]  = 1;
		automaticBoneScales[RUISSkeletonManager.Joint.LeftClavicle]  = 1;
		automaticBoneScales[RUISSkeletonManager.Joint.RightClavicle] = 1;

		automaticBoneOffsets[RUISSkeletonManager.Joint.Torso] = 0; // Are we multiplying (1) or adding (0)?
		automaticBoneOffsets[RUISSkeletonManager.Joint.Chest] = 0;
		automaticBoneOffsets[RUISSkeletonManager.Joint.Neck]  = 0;
		automaticBoneOffsets[RUISSkeletonManager.Joint.Head]  = 0;
		automaticBoneOffsets[RUISSkeletonManager.Joint.LeftClavicle]  = 0;
		automaticBoneOffsets[RUISSkeletonManager.Joint.RightClavicle] = 0;

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

		prevRightHandDelta = prevRightHandScale;
		prevRightFootDelta = prevRightFootScale;
		prevLeftHandDelta = prevLeftHandScale;
		prevLeftFootDelta = prevLeftFootScale;

		// Finger clench rotations: these depend on your animation rig
		// Also see method handleFingersCurling() and its clenchedRotationThumbTM_corrected and clenchedRotationThumbIP_corrected
		// variables, if you are not tracking thumbs with Kinect 2. They also depend on your animation rig.
//		switch(boneLengthAxis)
//		{
//			case RUISAxis.X:
//				// Thumb phalange rotations when hand is clenched to a fist
//				clenchedRotationThumbTM = Quaternion.Euler(45, 0, 0); 
//				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, -25);
//				clenchedRotationThumbIP = Quaternion.Euler(0, 0, -80);
//				// Phalange rotations of other fingers when hand is clenched to a fist
//				clenchedRotationMCP = Quaternion.Euler(0, 0, -45);
//				clenchedRotationPIP = Quaternion.Euler(0, 0, -100);
//				clenchedRotationDIP = Quaternion.Euler(0, 0, -70);
//				break;
//			case RUISAxis.Y:
//				// Thumb phalange rotations when hand is clenched to a fist
//				clenchedRotationThumbTM = Quaternion.Euler(45, 0, 0); 
//				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, 25);
//				clenchedRotationThumbIP = Quaternion.Euler(0, 0, 80);
//				// Phalange rotations of other fingers when hand is clenched to a fist
//				clenchedRotationMCP = Quaternion.Euler(45, 0, 0);
//				clenchedRotationPIP = Quaternion.Euler(100, 0, 0);
//				clenchedRotationDIP = Quaternion.Euler(70, 0, 0);
//				break;
//			case RUISAxis.Z: // TODO: Not yet tested with a real rig
//				// Thumb phalange rotations when hand is clenched to a fist
//				clenchedRotationThumbTM = Quaternion.Euler(45, 0, 0); 
//				clenchedRotationThumbMCP = Quaternion.Euler(0, 0, -25);
//				clenchedRotationThumbIP = Quaternion.Euler(0, 0, -80);
//				// Phalange rotations of other fingers when hand is clenched to a fist
//				clenchedRotationMCP = Quaternion.Euler(0, -45, 0);
//				clenchedRotationPIP = Quaternion.Euler(0, -100, 0);
//				clenchedRotationDIP = Quaternion.Euler(0, -70, 0);
//				break;
//		}
//		if(leftThumbHasIndependentRotations) // *** OPTIHACK5 TODO HACK remove
//		{
//			clenchedLeftThumbTM  = Quaternion.Euler(15, 0, -25);
//			clenchedLeftThumbMCP = Quaternion.Euler(0, 0, 0);
//			clenchedLeftThumbIP  = Quaternion.Euler(0, 0, 30);
//		}

		if(inputManager)
		{
			// *** OPTIHACK4 added this, check that it works: Below if-clause means that if Kinect1/2 was enabled and detected, offsets and scaling works only when the skeleton is tracked
			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && (inputManager.enableKinect || inputManager.enableKinect2))
				hasBeenTracked = true;

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
		initialLocalPosition = transform.localPosition;
		initialWorldPosition = transform.position;
		initialWorldRotation = transform.rotation;
		initialLossyScale = transform.lossyScale;

		// HACK for filtering Kinect 2 arm rotations
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRotations = filterRotations;
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rotationNoiseCovariance = rotationNoiseCovariance;
		for(int i = 0; i < skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot.Length; ++i)
		{
			if(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot[i] != null)
				skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRot[i].rotationNoiseCovariance = rotationNoiseCovariance;
		}
		skeletonManager.skeletons[BodyTrackingDeviceID, playerId].thumbZRotationOffset = thumbZRotationOffset;

		if(neck && leftShoulder && rightShoulder)
			neckParentsShoulders = leftShoulder.IsChildOf(neck) || rightShoulder.IsChildOf(neck);

		// *** OPTIHACK4 following is not true because of trackedSpineJoints: "... you can leave the below Custom Source fields empty."
		trackedSpineJoints[customSpineJointCount] = customTorso;
		++customSpineJointCount;
		if(customChest)
		{
			trackedSpineJoints[customSpineJointCount] = customChest;
			++customSpineJointCount;
		}
		if(customNeck)
		{
			trackedSpineJoints[customSpineJointCount] = customNeck;
			++customSpineJointCount;
		}
		if(customHead)
		{
			trackedSpineJoints[customSpineJointCount] = customHead;
			++customSpineJointCount;
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
			SetCustomJointData(customLeftHip, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftHip, 		1, 1); // *** OPTIHACK make offsets work
			SetCustomJointData(customLeftKnee, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftKnee, 	1, 1); // *** OPTIHACK make symmetric along X
			SetCustomJointData(customLeftFoot, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].leftFoot, 	1, 1);
			SetCustomJointData(customRightClavicle, ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightClavicle, 1, 1);
			SetCustomJointData(customRightShoulder, ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightShoulder, 1, 1);
			SetCustomJointData(customRightElbow, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightElbow, 	 1, 1);
			SetCustomJointData(customRightHand, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHand, 	 1, 1);
			SetCustomJointData(customRightThumb, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightThumb, 	 1, 1);
			SetCustomJointData(customRightHip, 		ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightHip, 	 1, 1);
			SetCustomJointData(customRightKnee, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightKnee, 	 1, 1); // *** OPTIHACK make symmetric along X
			SetCustomJointData(customRightFoot, 	ref skeletonManager.skeletons[BodyTrackingDeviceID, playerId].rightFoot, 	 1, 1);
		}
		// Update skeleton based on data fetched from skeletonManager
		if(		skeletonManager != null && skeletonManager.skeletons[BodyTrackingDeviceID, playerId] != null
		    /*&&  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking */)
		{

			if(bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker) // Kinect is used; copy the value that has been set by it
				skeleton.isTracking = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking;

			CopySkeletonJointData(skeletonManager.skeletons[BodyTrackingDeviceID, playerId], skeleton);

			if(!hasBeenTracked && skeleton.isTracking)
				hasBeenTracked = true;
						
//			if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID && !skeletonManager.isNewKinect2Frame)
//				return;

			float maxAngularChange;
//			if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID)
//				maxAngularChange = skeletonManager.kinect2FrameDeltaT * maxAngularVelocity;
//			else 
			maxAngularChange = deltaTime * maxAngularVelocity;

			UpdateSkeletonPosition(); // Updates all skeleton.<joint> positions if *** is enabled

			// Obtained new body tracking data. TODO test that Kinect 1 still works
			if(skeleton.isTracking || !hasBeenTracked  /* && bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame */)
			{
				ApplyTranslationOffsets(); // *** OPTIHACK5 TODO skeleton.torso.position and rotation values are not used if skeleton.isTracking is false!!!!

				UpdateTransform(ref torso, skeleton.torso, maxAngularChange, pelvisRotationOffset);
				UpdateTransform(ref chest, skeleton.chest, maxAngularChange, chestRotationOffset); // *** OPTIHACK
				UpdateTransform(ref neck,  skeleton.neck,  maxAngularChange, neckRotationOffset); // *** OPTIHACK
				UpdateTransform(ref head,  skeleton.head,  maxAngularChange, headRotationOffset);

				// *** OPTIHACK
				UpdateTransform(ref leftClavicle,  skeleton.leftClavicle,  maxAngularChange, clavicleRotationOffset); 
				UpdateTransform(ref rightClavicle, skeleton.rightClavicle, maxAngularChange, clavicleRotationOffset);
			}

			// *** OPTIHACK4 these are unnecessary for hierarchicalModels because of the later calls to ForceUpdatePosition( chest/neck/... ) ?
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
				
			if(hmdRotatesHead && RUISDisplayManager.IsHmdPresent() && head)
			{
				// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
				headsetRotation = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), headsetCoordinates);

				if(useHierarchicalModel)
					head.rotation = transform.rotation * headsetRotation /*skeleton.head.rotation*/ *
					(jointInitialRotations.ContainsKey(head) ? jointInitialRotations[head] : Quaternion.identity);
				else
					head.localRotation = headsetRotation; //skeleton.head;
			}
			
			// Obtained new body tracking data. TODO test that Kinect 1 still works
			if(skeleton.isTracking || !hasBeenTracked /* && bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame */)
			{
				UpdateTransform(ref leftShoulder,	skeleton.leftShoulder, 	maxAngularChange, shoulderRotationOffset);
				UpdateTransform(ref rightShoulder,	skeleton.rightShoulder, maxAngularChange, shoulderRotationOffset);
				UpdateTransform(ref rightElbow, 	skeleton.rightElbow, 	maxAngularChange, elbowRotationOffset);
				UpdateTransform(ref leftElbow, 		skeleton.leftElbow, 	maxAngularChange, elbowRotationOffset);
				// HACK the multiplier: 2 * maxAngularChange
				UpdateTransform(ref leftHand, 		skeleton.leftHand,  2 * maxAngularChange, handRotationOffset);
				UpdateTransform(ref rightHand, 		skeleton.rightHand, 2 * maxAngularChange, handRotationOffset);

				UpdateTransform(ref leftHip, 		skeleton.leftHip, 		maxAngularChange, hipRotationOffset);
				UpdateTransform(ref rightHip, 		skeleton.rightHip, 		maxAngularChange, hipRotationOffset);
				UpdateTransform(ref leftKnee, 		skeleton.leftKnee, 		maxAngularChange, kneeRotationOffset);
				UpdateTransform(ref rightKnee, 		skeleton.rightKnee, 	maxAngularChange, kneeRotationOffset);
				
				UpdateTransform(ref leftFoot, 		skeleton.leftFoot, 		maxAngularChange, feetRotationOffset);
				UpdateTransform(ref rightFoot, 		skeleton.rightFoot, 	maxAngularChange, feetRotationOffset);
				
				if(!useHierarchicalModel)
				{
					if(!trackWrist)
					{
						if(leftHand && leftElbow) // *** OPTIHACK4 no axis minus in left handRotationOffset, check that this works
							leftHand.localRotation  = leftElbow.localRotation  * Quaternion.Euler(handRotationOffset);
						if(rightHand && rightElbow)
							rightHand.localRotation = rightElbow.localRotation * Quaternion.Euler(handRotationOffset);
					}
					if(!trackAnkle)
					{
						if(leftFoot && leftKnee) // *** OPTIHACK4 no axis minus in left feetRotationOffset, check that this works
							leftFoot.localRotation  = leftKnee.localRotation  * Quaternion.Euler(feetRotationOffset);
						if(rightFoot && rightKnee)
							rightFoot.localRotation = rightKnee.localRotation * Quaternion.Euler(feetRotationOffset);
					}
					// *** TODO bone scaling
				}
			
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
//					//				UpdateTransform (ref rightElbow, skeleton.rightHand);
//					//				UpdateTransform (ref leftElbow,  skeleton.leftHand);
//				}
	
				if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2)
				{
					leftHandStatus  = skeleton.leftHandStatus;
					rightHandStatus = skeleton.rightHandStatus;

					if(fistMaking && !externalFistTrigger)
						HandleFistMaking();

					if(kinect2Thumbs)
					{
						// UpdateFingerRotations() usually handles all finger rotations, but thumb rotations are handled below if kinect2Thumbs == true
						if(rightThumb)
							UpdateTransform(ref rightThumb, skeleton.rightThumb, maxAngularChange, thumbRotationOffset);
						if(leftThumb)
							UpdateTransform(ref leftThumb,  skeleton.leftThumb,  maxAngularChange, thumbRotationOffset);
					}
				}
				
				// There is some other trigger that determines fist curling (e.g. RUISButtonGestureRecognizer)
				if(fistMaking && externalFistTrigger)
					HandleFistMaking();
			}

			if(!fistMaking) // Tracked fingers are reflected by the avatar only if fistMaking is set to false
				UpdateFingerRotations();

			if(useHierarchicalModel)
			{
				if(scaleHierarchicalModelBones)
				{
					UpdateBoneScalings();

					torsoRotation = Quaternion.Slerp(torsoRotation, skeleton.torso.rotation, deltaTime * maxAngularVelocity);
					torsoDirection = torsoRotation * Vector3.down;

					ForceUpdatePosition(ref torso, skeleton.torso, 10, deltaTime);
					// *** OPTIHACK TODO commented below and moved it to the above ForceUpdatePosition() with modifications, check that everything works!
//					if(torso == root)
//						torso.position = transform.TransformPoint(- torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition)
//																  + skeleton.torso.rotation * pelvisOffset * torso.localScale.x);
//					else
//						torso.position = transform.TransformPoint(skeleton.torso.position - skeletonPosition
//																  - torsoDirection * (torsoOffset * torsoScale + adjustVerticalHipsPosition)
//																  + skeleton.torso.rotation * pelvisOffset * torso.localScale.x);
					// *** NOTE above how pelvisOffset is scaled with torso.localScale.x   <-- This assumes uniform scaling of torso

					// Obtained new body tracking data. TODO test that Kinect 1 still works
//					if(bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame)
					if(updateJointPositions)
					{
						float deltaT;
//						if(bodyTrackingDeviceID == RUISSkeletonManager.kinect2SensorID)
//							deltaT = skeletonManager.kinect2FrameDeltaT;
//						else
						deltaT = deltaTime;

						if(hmdMovesHead && RUISDisplayManager.IsHmdPresent())
						{
							// *** OPTIHACK5 TODO Added ConvertLocation() because it was missing. Any side-effects?? RUISDevice.OpenVR
							skeleton.head.position = 
								coordinateSystem.ConvertLocation(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head), headsetCoordinates);
							// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
						}

//						if(!independentTorsoSegmentsScaling)
						{
							ForceUpdatePosition(ref chest, 		   skeleton.chest, 	   	   11, deltaT);
							ForceUpdatePosition(ref neck, 		   skeleton.neck, 		    5, deltaT);
							ForceUpdatePosition(ref head, 		   skeleton.head, 		    4, deltaT);
							if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
							{
								if(customRightClavicle)
									ForceUpdatePosition(ref rightClavicle, skeleton.rightClavicle, 8, deltaT);
								if(customLeftClavicle)
									ForceUpdatePosition(ref leftClavicle,  skeleton.leftClavicle,  9, deltaT);
							}
						}
						// *** OPTIHACK3 above were added
						ForceUpdatePosition(ref rightShoulder, 	skeleton.rightShoulder, 0, deltaT);
						ForceUpdatePosition(ref leftShoulder, 	skeleton.leftShoulder,  1, deltaT);
						ForceUpdatePosition(ref rightHip, 		skeleton.rightHip, 	 	2, deltaT);
						ForceUpdatePosition(ref leftHip, 		skeleton.leftHip, 		3, deltaT);
						if(!limbsAreScaled)
						{
							ForceUpdatePosition(ref rightElbow, 	skeleton.rightElbow, 	14, deltaT);
							ForceUpdatePosition(ref leftElbow, 		skeleton.leftElbow,		15, deltaT);
							ForceUpdatePosition(ref rightKnee, 		skeleton.rightKnee, 	16, deltaT);
							ForceUpdatePosition(ref leftKnee, 		skeleton.leftKnee, 		17, deltaT);
							ForceUpdatePosition(ref rightHand, 		skeleton.rightHand, 	12, deltaT);
							ForceUpdatePosition(ref leftHand, 		skeleton.leftHand, 		13, deltaT);
							ForceUpdatePosition(ref rightFoot, 		skeleton.rightFoot, 	 6, deltaT);
							ForceUpdatePosition(ref leftFoot, 		skeleton.leftFoot, 	 	 7, deltaT);
						}
					}
				}
				else if(updateJointPositions)
				{
					if(hmdMovesHead && RUISDisplayManager.IsHmdPresent())
					{
						// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
						skeleton.head.position = 
								coordinateSystem.ConvertLocation(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head), headsetCoordinates);
					}
					ForceUpdatePosition(ref torso, 			skeleton.torso, 		10, deltaTime);
					ForceUpdatePosition(ref chest, 			skeleton.chest, 		11, deltaTime);
					ForceUpdatePosition(ref neck, 			skeleton.neck, 		 	 5, deltaTime);
					ForceUpdatePosition(ref head, 			skeleton.head, 		 	 4, deltaTime);
					ForceUpdatePosition(ref rightClavicle, 	skeleton.rightClavicle,	 8, deltaTime);
					ForceUpdatePosition(ref leftClavicle, 	skeleton.leftClavicle, 	 9, deltaTime);
					ForceUpdatePosition(ref rightShoulder, 	skeleton.rightShoulder,	 0, deltaTime);
					ForceUpdatePosition(ref leftShoulder, 	skeleton.leftShoulder, 	 1, deltaTime);
					ForceUpdatePosition(ref rightElbow, 	skeleton.rightElbow, 	14, deltaTime);
					ForceUpdatePosition(ref leftElbow, 		skeleton.leftElbow,  	15, deltaTime);
					ForceUpdatePosition(ref rightHand, 		skeleton.rightHand, 	12, deltaTime);
					ForceUpdatePosition(ref leftHand, 		skeleton.leftHand, 		13, deltaTime);
					ForceUpdatePosition(ref rightHip, 		skeleton.rightHip, 		 2, deltaTime);
					ForceUpdatePosition(ref leftHip, 		skeleton.leftHip, 		 3, deltaTime);
					ForceUpdatePosition(ref rightKnee, 		skeleton.rightKnee, 	16, deltaTime);
					ForceUpdatePosition(ref leftKnee, 		skeleton.leftKnee, 		17, deltaTime);
					ForceUpdatePosition(ref rightFoot, 		skeleton.rightFoot, 	 6, deltaTime);
					ForceUpdatePosition(ref leftFoot, 		skeleton.leftFoot, 		 7, deltaTime);

					if(leftKnee)
						leftKnee.localScale  = shinLengthRatio * jointInitialLocalScales[leftKnee];
					if(rightKnee)
						rightKnee.localScale = shinLengthRatio * jointInitialLocalScales[rightKnee];
					if(leftElbow)
						leftElbow.localScale  = forearmLengthRatio * jointInitialLocalScales[leftElbow];
					if(rightElbow)
						rightElbow.localScale = forearmLengthRatio * jointInitialLocalScales[rightElbow];
				}
			}

			if(updateRootPosition)
			{
//				Vector3 newRootPosition = skeleton.root.position;
//				measuredPos [0] = newRootPosition.x;
//				measuredPos [1] = newRootPosition.y;
//				measuredPos [2] = newRootPosition.z;
//				positionKalman.setR (deltaTime * positionNoiseCovariance);
//				positionKalman.predict ();
//				positionKalman.update (measuredPos);
//				pos = positionKalman.getState ();

				// Root speed scaling is applied here
				transform.localPosition = Vector3.Scale(skeletonPosition, rootSpeedScaling); // *** What if this is before updating limb transforms..?
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

					if(!skeletonManager || !skeleton.isTracking)
					{
//						skeletonPosition = psmove.localPosition - trackedDeviceYawRotation * characterController.psmoveOffset;
//						skeletonPosition.y = 0;
						tempVector = psmove.localPosition - trackedDeviceYawRotation * characterController.psmoveOffset;
						skeletonPosition.x = tempVector.x;
						skeletonPosition.z = tempVector.z;

						if(characterController.headRotatesBody)
							tempRotation = UpdateTransformWithTrackedDevice(ref root, moveYaw);
						else 
							tempRotation = Quaternion.identity;
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
							// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
							tempVector = coordinateSystem.ConvertLocation(coordinateSystem.GetHmdRawPosition(), headsetCoordinates);
							skeletonPosition.x = tempVector.x;
							skeletonPosition.z = tempVector.z;
							// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
							hmdYaw = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), headsetCoordinates).eulerAngles.y;
						}
					}
					else
					{
//						skeletonPosition = coordinateSystem.GetHmdRawPosition();
//						skeletonPosition.y = 0;
						tempVector = coordinateSystem.GetHmdRawPosition();
						skeletonPosition.x = tempVector.x;
						skeletonPosition.z = tempVector.z;
						hmdYaw = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head).eulerAngles.y;
					}
				}

				trackedDeviceYawRotation = Quaternion.Euler(0, hmdYaw, 0);

				if(characterController.headRotatesBody)
					tempRotation = UpdateTransformWithTrackedDevice(ref root, hmdYaw);
				else
					tempRotation = Quaternion.identity;
					
				if(updateRootPosition) 
					transform.localPosition = skeletonPosition + tempRotation*rootOffset;	
			}
		}
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

	private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet, float maxAngularChange, Vector3 rotOffset)
	{
		bool useParentRotation = false;

		if(transformToUpdate == null)
		{
			return;
		}
		
		if(updateJointPositions && !useHierarchicalModel && jointToGet.positionConfidence >= minimumConfidenceToUpdate)
		{
			// HACK TODO: in Kinect 1/2 skeleton.torso = skeleton.root, so lets use filtered version of that (==skeletonPosition)
			if(jointToGet.jointID == RUISSkeletonManager.Joint.Torso)
				transformToUpdate.localPosition = pelvisOffset;
			else
				transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
		}

		switch(jointToGet.jointID)
		{
			case RUISSkeletonManager.Joint.LeftClavicle: 
			case RUISSkeletonManager.Joint.LeftShoulder: 
			case RUISSkeletonManager.Joint.LeftElbow: 
			case RUISSkeletonManager.Joint.LeftHip: 
			case RUISSkeletonManager.Joint.LeftKnee: 
			case RUISSkeletonManager.Joint.LeftThumb: // *** OPTIHACK5 check that thumb offsets work with Kinect2 tracking
				rotOffset.Set(rotOffset.x, -rotOffset.y, -rotOffset.z);
				break;
			case RUISSkeletonManager.Joint.LeftHand: 
				if(!trackWrist)
				{
					useParentRotation = true;
					switch(boneLengthAxis)
					{
						case RUISAxis.X: rotOffset.Set(-rotOffset.x, -rotOffset.y,  rotOffset.z); break; // HACK do these work with all rigs?
						case RUISAxis.Y: rotOffset.Set( rotOffset.x, -rotOffset.y, -rotOffset.z); break;
						case RUISAxis.Z: rotOffset.Set(-rotOffset.x,  rotOffset.y, -rotOffset.z); break; // *** not tested with any "Z-rig"
					}
				}
				else
					rotOffset.Set(rotOffset.x, -rotOffset.y, -rotOffset.z);
				break;
			case RUISSkeletonManager.Joint.LeftFoot: 
				if(!trackAnkle)
				{
					useParentRotation = true;
					switch(boneLengthAxis)
					{
						case RUISAxis.X: rotOffset.Set(-rotOffset.x, -rotOffset.y,  rotOffset.z); break; // HACK do these work with all rigs?
						case RUISAxis.Y: rotOffset.Set( rotOffset.x, -rotOffset.y, -rotOffset.z); break;
						case RUISAxis.Z: rotOffset.Set(-rotOffset.x,  rotOffset.y, -rotOffset.z); break; // *** not tested with any "Z-rig"
					}
				}
				else
					rotOffset.Set(rotOffset.x, -rotOffset.y, -rotOffset.z);
				break;
			case RUISSkeletonManager.Joint.RightHand: 
				if(!trackWrist)
					useParentRotation = true;
				break;
			case RUISSkeletonManager.Joint.RightFoot:
				if(!trackAnkle)
					useParentRotation = true;
				break;
		}

		Quaternion rotationOffset = Quaternion.Euler(rotOffset);

		if(updateJointRotations && (jointToGet.rotationConfidence >= minimumConfidenceToUpdate || !hasBeenTracked))
		{
			if(useHierarchicalModel)
			{
				Quaternion newRotation;
				if(useParentRotation && transformToUpdate.parent) // *** OPTIHACK4 check that disabling trackWrist & trackAnkle still works
					newRotation = transformToUpdate.parent.rotation * rotationOffset;
				else
					newRotation = transform.rotation * jointToGet.rotation * rotationOffset *
					                                     (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
				transformToUpdate.rotation = Quaternion.RotateTowards(transformToUpdate.rotation, newRotation, maxAngularChange);
			}
			else
			{	// *** OPTIHACK4  check that this works and rotationOffset multiplication belongs to right side
				transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation,  jointToGet.rotation * rotationOffset, maxAngularChange);
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
//                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, deltaTime * maxAngularVelocity);
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
//              transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, deltaTime * maxAngularVelocity);
			}
		}
		return yaw;
	}

	float offsetScale;
	private Vector3 jointOffset = Vector3.zero; // *** OPTIHACK

	private void ForceUpdatePosition(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet, int jointID, float deltaT)
	{
		if(transformToUpdate == null)
			return;

//		if(jointID == 2 || jointID == 3) // HACK: for now saving performance by not filtering hips
//			transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);
//		else
		{
			if(filterPosition && jointID < fourJointsKalman.Length && !(filterHeadPositionOnly && jointID != 4)) // *** TODO set ÍDs in better manner
			{
				measuredPos[0] = jointToGet.position.x;
				measuredPos[1] = jointToGet.position.y;
				measuredPos[2] = jointToGet.position.z;

				fourJointsKalman[jointID].setR(deltaT * fourJointsNoiseCovariance);
				fourJointsKalman[jointID].predict();
				fourJointsKalman[jointID].update(measuredPos);
				pos = fourJointsKalman[jointID].getState();

				forcedJointPositions[jointID].Set((float)pos[0], (float)pos[1], (float)pos[2]);
			}
			else
				forcedJointPositions[jointID] = jointToGet.position;
			
			// return; invocation is here as well so that the above Kalman filter will be updated even with "bad" confidence values
			if(jointToGet.positionConfidence < minimumConfidenceToUpdate && hasBeenTracked)
				return;

			// *** OPTIHACK5 remove this and the commented block below
			jointOffset = Vector3.zero;

//			if(heightAffectsOffsets)
//			{
//				if(jointToGet.jointID != RUISSkeletonManager.Joint.Torso) // *** OPTIHACK4
//					offsetScale = transformToUpdate.parent.localScale.x; // *** TODO CHECK IF THIS IS BEST offsetScale or if torsoScale would be better
//																		 //     considering offset invariance between users of different height
//				else
//					offsetScale = 1; //offsetScale = torsoScale; // if uncommented, remove torso.localScale.x multiplier from below switch torso case
//			}
//			else
//				offsetScale = 1;
//				
//			// *** OPTIHACK4 change all skeleton.*.rotations preceding *Offset to *.rotation (e.g. chest.rotation) ? 
//			//		Not that simple... the jointOffsets need to be in the tracking/skeleton frame. 
//			//		Quaternion.Inverse(transform.rotation)*chest.rotation might suffice... Finally add jointoffsets to RUISKinectAndMecanimCombiner
//			switch(jointToGet.jointID) 
//			{
//				case RUISSkeletonManager.Joint.Torso: // *** OPTIHACK still using hacky Kinect1/2 pelvis offset adjustments. TODO: consider switching to use pelvisOffset
//					if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2 || bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
//						jointOffset =  skeleton.torso.rotation * pelvisOffset * torso.localScale.x - torsoDirection * torsoOffset * torsoScale;
//					else
//						jointOffset =  skeleton.torso.rotation * pelvisOffset;
//					break;
//				case RUISSkeletonManager.Joint.Chest:
//					jointOffset = skeleton.chest.rotation * chestOffset;
//					break;
//				case RUISSkeletonManager.Joint.Neck:
//					jointOffset = skeleton.neck.rotation * neckOffset;
//					offsetScale = 1; // OPTIHACK TODO ***
//					break;
//				case RUISSkeletonManager.Joint.Head:
//					if(hmdMovesHead && RUISDisplayManager.IsHmdPresent())
//					{
//						offsetScale = 1; // OPTIHACK *** CHECK THAT WORKS, torso.localScale.x is better
//						// *** OPTIHACK TODO this isn't right!  Quaternion.Inverse( coordinateSystem.ConvertRotation(...
//						jointOffset = headOffset + coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
//																					headsetCoordinates) * hmdLocalOffset; 
//						// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
//					}
//					else
//					{
//						offsetScale = 1; // OPTIHACK TODO ***
//						jointOffset = skeleton.head.rotation * headOffset;
//					}
//					break;
//				case RUISSkeletonManager.Joint.LeftClavicle:
//					jointOffset = skeleton.leftClavicle.rotation * clavicleOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightClavicle:
//					jointOffset.Set(-clavicleOffset.x, clavicleOffset.y, clavicleOffset.z);
//					jointOffset = skeleton.rightClavicle.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftShoulder:
//					jointOffset = skeleton.chest.rotation * shoulderOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightShoulder:
//					jointOffset.Set(-shoulderOffset.x, shoulderOffset.y, shoulderOffset.z);
//					jointOffset = skeleton.chest.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftElbow:
//					jointOffset = skeleton.leftShoulder.rotation * elbowOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightElbow:
//					jointOffset.Set(-elbowOffset.x, elbowOffset.y, elbowOffset.z);
//					jointOffset = skeleton.rightShoulder.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftHand:
//					jointOffset = skeleton.leftElbow.rotation * handOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightHand:
//					jointOffset.Set(-handOffset.x, handOffset.y, handOffset.z);
//					jointOffset = skeleton.rightElbow.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftHip:
//					jointOffset = skeleton.torso.rotation * hipOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightHip:
//					jointOffset.Set(-hipOffset.x, hipOffset.y, hipOffset.z);
//					jointOffset = skeleton.torso.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftKnee:
//					jointOffset = skeleton.leftHip.rotation * kneeOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightKnee:
//					jointOffset.Set(-kneeOffset.x, kneeOffset.y, kneeOffset.z);
//					jointOffset = skeleton.rightHip.rotation * jointOffset;
//					break;
//				case RUISSkeletonManager.Joint.LeftFoot:
//					jointOffset = skeleton.leftKnee.rotation * footOffset;
//					break;
//				case RUISSkeletonManager.Joint.RightFoot:
//					jointOffset.Set(-footOffset.x, footOffset.y, footOffset.z);
//					jointOffset = skeleton.rightKnee.rotation * jointOffset;
//					break;
//				default:
//					jointOffset.Set(0, 0, 0);
//					break;
//			}

			// If skeleton is not tracked, apply jointOffset anyway using initial position
			if(!hasBeenTracked)
			{
				// Below is hacky but seems to work
				forcedJointPositions[jointID].Set(1/initialLossyScale.x, 1/initialLossyScale.y, 1/initialLossyScale.z);
				forcedJointPositions[jointID] = Vector3.Scale(initialLocalPosition, transform.localScale) + Quaternion.Inverse(initialWorldRotation) 
													* Vector3.Scale(forcedJointPositions[jointID], jointInitialWorldPositions[transformToUpdate] - jointInitialWorldPositions[root]);
			}

			// *** NOTE the use of transformToUpdate.parent.localScale.x <-- assumes uniform scale from the parent (clavicle/torso)
			if(jointToGet.jointID == RUISSkeletonManager.Joint.Torso && torso == root)
				transformToUpdate.position = transform.TransformPoint(jointOffset * offsetScale);
			else
				transformToUpdate.position = transform.TransformPoint(forcedJointPositions[jointID] + jointOffset * offsetScale - skeletonPosition);
		}
//		transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);
	}

	void ApplyTranslationOffsets()
	{
		if(heightAffectsOffsets)
		{
//			if(jointToGet.jointID != RUISSkeletonManager.Joint.Torso) // *** OPTIHACK4
//				offsetScale = transformToUpdate.parent.localScale.x; // *** TODO CHECK IF THIS IS BEST offsetScale or if torsoScale would be better
//     		// considering offset invariance between users of different height
//				else
			offsetScale = 1; //offsetScale = torsoScale; // if uncommented, remove torso.localScale.x multiplier from below switch torso case
		}
		else
			offsetScale = 1;
		// *** OPTIHACK5 start using torsoScale (and set it to sensible values). Get rid of this if-else conditional (torso.localScale.x) and test results
		if(bodyTrackingDevice == BodyTrackingDeviceType.Kinect2 || bodyTrackingDevice == BodyTrackingDeviceType.Kinect1)
			jointOffset =  skeleton.torso.rotation * pelvisOffset * torso.localScale.x;
		else
			jointOffset =  skeleton.torso.rotation * pelvisOffset; // * offsetScale
		skeleton.torso.position			+= offsetScale * (jointOffset);

		skeleton.chest.position			+= offsetScale * (skeleton.chest.rotation * chestOffset);
		skeleton.neck.position			+= offsetScale * (skeleton.neck.rotation  * neckOffset);

		if(hmdMovesHead && RUISDisplayManager.IsHmdPresent())
		{
			offsetScale = 1; // OPTIHACK *** CHECK THAT WORKS, torso.localScale.x is better
			// *** OPTIHACK TODO this isn't right!  Quaternion.Inverse( coordinateSystem.ConvertRotation(...
			jointOffset = headOffset + coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
																		headsetCoordinates) * hmdLocalOffset; 
			// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
		}
		else
		{
			offsetScale = 1; // OPTIHACK TODO ***
			jointOffset = skeleton.neck.rotation * headOffset; // *** OPTIHACK6 changed skeleton.head.rotation to skeleton.neck.rotation <-- check that this works with optitrack
		}
		skeleton.head.position			+= offsetScale * (jointOffset);

		skeleton.leftClavicle.position	+= offsetScale * (skeleton.chest.rotation 		 * clavicleOffset);
		skeleton.leftShoulder.position	+= offsetScale * (skeleton.chest.rotation 		 * shoulderOffset);
		skeleton.leftElbow.position		+= offsetScale * (skeleton.leftShoulder.rotation * elbowOffset);
		skeleton.leftHand.position		+= offsetScale * (skeleton.leftElbow.rotation 	 * handOffset);
		skeleton.leftHip.position		+= offsetScale * (skeleton.torso.rotation 		 * hipOffset);
		skeleton.leftKnee.position		+= offsetScale * (skeleton.leftHip.rotation 	 * kneeOffset);
		skeleton.leftFoot.position		+= offsetScale * (skeleton.leftKnee.rotation 	 * footOffset);

		jointOffset.Set(-clavicleOffset.x, clavicleOffset.y, clavicleOffset.z);
		skeleton.rightClavicle.position	+= offsetScale * (skeleton.chest.rotation 		  * jointOffset);

		jointOffset.Set(-shoulderOffset.x, shoulderOffset.y, shoulderOffset.z);
		skeleton.rightShoulder.position	+= offsetScale * (skeleton.chest.rotation 		  * jointOffset);

		jointOffset.Set(-elbowOffset.x, elbowOffset.y, elbowOffset.z);
		skeleton.rightElbow.position	+= offsetScale * (skeleton.rightShoulder.rotation * jointOffset);

		jointOffset.Set(-handOffset.x, handOffset.y, handOffset.z);
		skeleton.rightHand.position		+= offsetScale * (skeleton.rightElbow.rotation 	  * jointOffset);

		jointOffset.Set(-hipOffset.x, hipOffset.y, hipOffset.z);
		skeleton.rightHip.position		+= offsetScale * (skeleton.torso.rotation 		  * jointOffset);

		jointOffset.Set(-kneeOffset.x, kneeOffset.y, kneeOffset.z);
		skeleton.rightKnee.position		+= offsetScale * (skeleton.rightHip.rotation 	  * jointOffset);

		jointOffset.Set(-footOffset.x, footOffset.y, footOffset.z);
		skeleton.rightFoot.position		+= offsetScale * (skeleton.rightKnee.rotation 	  * jointOffset);
	}

	private Vector3    headsetPosition;
	private Quaternion headsetRotation;
	private Vector3    newRootPosition;
	private Vector3 headToHeadsetVector;
//	private Vector3 pelvisToHeadVector;

	// Gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
	private void UpdateSkeletonPosition()
	{
		if(yawCorrectIMU)
		{
			if(skeleton.torso.rotationConfidence > minimumConfidenceToUpdate)
			{
				// *** OPTIHACK the below GetYawDriftCorrection invocations do not work if skeleton.torso.rotation was not updated with a "fresh" value
				// just before calling this function (right now this only concerns Kinect1/2 which shouldn't be used with yawCorrectIMU
				if(customHMDSource)
				{
					// GetYawDriftCorrection() sets rotationDrift and also returns it
					GetYawDriftCorrection(customHMDSource.rotation, skeleton.torso.rotation); 
				}
				else
				{
					if(RUISDisplayManager.IsHmdPresent()) // *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
						GetYawDriftCorrection(coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
																			   headsetCoordinates), skeleton.torso.rotation);
				}
			}

			skeleton.root.rotation 			= rotationDrift * skeleton.root.rotation;
			skeleton.torso.rotation  		= rotationDrift * skeleton.torso.rotation;
			skeleton.chest.rotation  		= rotationDrift * skeleton.chest.rotation;
			skeleton.neck.rotation  		= rotationDrift * skeleton.neck.rotation;
			skeleton.head.rotation  		= rotationDrift * skeleton.head.rotation;
			skeleton.leftClavicle.rotation  = rotationDrift * skeleton.leftClavicle.rotation;
			skeleton.leftShoulder.rotation  = rotationDrift * skeleton.leftShoulder.rotation;
			skeleton.leftElbow.rotation  	= rotationDrift * skeleton.leftElbow.rotation;
			skeleton.leftHand.rotation  	= rotationDrift * skeleton.leftHand.rotation;
			skeleton.rightClavicle.rotation = rotationDrift * skeleton.rightClavicle.rotation;
			skeleton.rightShoulder.rotation = rotationDrift * skeleton.rightShoulder.rotation;
			skeleton.rightElbow.rotation    = rotationDrift * skeleton.rightElbow.rotation;
			skeleton.rightHand.rotation  	= rotationDrift * skeleton.rightHand.rotation;
			skeleton.leftHip.rotation  	    = rotationDrift * skeleton.leftHip.rotation;
			skeleton.leftKnee.rotation  	= rotationDrift * skeleton.leftKnee.rotation;
			skeleton.leftFoot.rotation  	= rotationDrift * skeleton.leftFoot.rotation;
			skeleton.rightHip.rotation  	= rotationDrift * skeleton.rightHip.rotation;
			skeleton.rightKnee.rotation  	= rotationDrift * skeleton.rightKnee.rotation;
			skeleton.rightFoot.rotation  	= rotationDrift * skeleton.rightFoot.rotation;
			skeleton.leftThumb.rotation  	= rotationDrift * skeleton.leftThumb.rotation;
			skeleton.rightThumb.rotation 	= rotationDrift * skeleton.rightThumb.rotation;
		}

		if(headsetDragsBody)
		{
//			Vector3 headPelvisOffset = motionSuitHead.position - motionSuitPelvis.position + motionSuitPelvis.rotation * pelvisOffset;
//			motionSuitPelvis.position = HMDCameraEye.position - headPelvisOffset + HMDCameraEye.rotation * eyeOffset;

//			pelvisToHeadVector =  skeleton.head.position - skeleton.torso.position + skeleton.torso.rotation * rootOffset;
			
			if(customHMDSource)
			{
				headsetPosition = customHMDSource.localPosition; // ***
				headsetRotation = customHMDSource.localRotation; // ***
			}
			else
			{
				if(RUISDisplayManager.IsHmdPresent())
				{
					// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
					headsetPosition = coordinateSystem.ConvertLocation(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head), headsetCoordinates);
					headsetRotation = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), headsetCoordinates);
				}
				else // If HMD is not present, then the practical effects should limit to only applying headToPelvisVector and hmdLocalOffset
				{
					headsetPosition = skeleton.head.position;
					headsetRotation = skeleton.head.rotation;
				}
			}
			
				headToHeadsetVector = headsetPosition - (skeleton.root.position + rotationDrift * (skeleton.head.position - skeleton.root.position)) + headsetRotation * hmdLocalOffset;
		}
		else
			headToHeadsetVector = Vector3.zero;

		if(headsetDragsBody || yawCorrectIMU)
		{
			// Update positions with headToHeadsetVector, rotate around skeleton.root if yawCorrectIMU == true
			RotatePositionAroundPivot(ref skeleton.torso.position, 			skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.chest.position, 			skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.neck.position, 			skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.head.position, 			skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftClavicle.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftShoulder.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftElbow.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftHand.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightClavicle.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightShoulder.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightElbow.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightHand.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftHip.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftKnee.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftFoot.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightHip.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightKnee.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightFoot.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.leftThumb.position, 		skeleton.root.position, rotationDrift, headToHeadsetVector);
			RotatePositionAroundPivot(ref skeleton.rightThumb.position, 	skeleton.root.position, rotationDrift, headToHeadsetVector);

			// skeleton.root.position is last to update because otherwise it is used as a pivot and that would mess up the above calculations
			RotatePositionAroundPivot(ref skeleton.root.position, 			skeleton.root.position, rotationDrift, headToHeadsetVector);
		}

		if(filterPosition && !filterHeadPositionOnly)
		{
			newRootPosition = skeleton.root.position;

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
			skeletonPosition = skeleton.root.position;
			

		// If skeleton is not tracked, apply jointOffset anyway using initial position
		if(!hasBeenTracked)
		{
			// Setting this on every frame due to RUISKinectAndMecanimCombiner's recreation of this script
			skeletonPosition = initialLocalPosition;
		}

//		if(skeleton.root.positionConfidence >= minimumConfidenceToUpdate)
//        {
//			skeletonPosition = skeleton.root.position;
//        }
	}
	
	/// <summary>
	/// Saves the initial _world_ rotation, localPosition, and localScale
	/// </summary>
	/// <param name="bodyPart">Body part</param>
	private void SaveInitialTransform(Transform bodyPart)
	{
		if(bodyPart)
		{
			jointInitialRotations[bodyPart] 	 = GetInitialRotation(bodyPart);
			jointInitialWorldPositions[bodyPart] = bodyPart.position;
			jointInitialLocalScales[bodyPart]    = bodyPart.localScale;
			jointInitialLocalPositions[bodyPart] = bodyPart.localPosition;
		}
	}

	private float SaveInitialDistance(Transform rootTransform, Transform distanceTo)
	{
		if(!rootTransform || !distanceTo)
			return 0;
		Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
		float distance = Vector3.Distance(Vector3.Scale(rootTransform.position, scaler), Vector3.Scale(distanceTo.position, scaler));
		jointInitialDistances.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), distance);
		trackedBoneLengths.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), distance); // Initialized bone lengths
		return distance;
	}

	private Quaternion GetInitialRotation(Transform bodyPart)
	{
		return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
	}
			
	private float cumulativeScale = 1;
	private float cumulatedPelvisScale = 1;
	private float limbStartScale = 1;
	private float neckScale = 1;
	private float torsoMultiplier = 1;
	private Vector3 elementScale = Vector3.one;

	private void UpdateBoneScalings()
	{
		if(!ConfidenceGoodEnoughForScaling() && hasBeenTracked)
			return;

//		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK4 commented this
		{
			if(independentTorsoSegmentsScaling)
			{
				torsoMultiplier = torsoThickness;
			}
			else
			{
				if(skeleton.isTracking)
					torsoScale = UpdateTorsoScale(); // *** OPTIHACK3
				torsoMultiplier = torsoScale * torsoThickness;
			}
				
			cumulativeScale = UpdateUniformBoneScaling(torso, chest, skeleton.torso, skeleton.chest, torsoMultiplier * pelvisScaleAdjust, 1);

			cumulatedPelvisScale = cumulativeScale;
//			torsoScale = cumulativeScale; // *** OPTIHACK3 was this

			cumulativeScale = UpdateUniformBoneScaling(chest, neck, skeleton.chest, skeleton.neck, torsoMultiplier * chestScaleAdjust, cumulativeScale);
			
			if(!neckParentsShoulders)
				neckScale = cumulativeScale;

			cumulativeScale = UpdateUniformBoneScaling(neck, head, skeleton.neck, skeleton.head, torsoMultiplier * neckScaleAdjust, cumulativeScale);
			if(neckParentsShoulders)
				neckScale = cumulativeScale;

			if(head && neckScale != 0) // cumulativeScale contains the accumulated scale of head's parent
				head.localScale = (headScaleAdjust / cumulativeScale) * Vector3.one; 
		}
//		else // *** OPTIHACK4 commented this whole else clause
//		{
//			// *** TODO torsoScale is now assigned below and INSIDE the method. Get rid of hip/neck tweaks and reconsider how torsoScale is assigned
//			if(skeleton.isTracking)
//				torsoScale = UpdateTorsoScale();
//			torsoMultiplier = torsoScale * torsoThickness;
//			torso.localScale = torsoMultiplier * Vector3.one; // *** OPTIHACK3
//			limbStartScale = torsoScale; 
//			if(head && neckScale != 0)
//				head.localScale = (headScaleAdjust/torsoScale) * Vector3.one;
//		}

//		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK4 commented this
			limbStartScale = UpdateUniformBoneScaling(rightClavicle, rightShoulder, skeleton.rightClavicle, skeleton.rightShoulder, 
													  torsoMultiplier * clavicleScaleAdjust, neckScale);

		cumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeleton.rightShoulder, skeleton.rightElbow,  limbStartScale);
		cumulativeScale = UpdateBoneScaling(rightElbow,    rightHand,  skeleton.rightElbow,    skeleton.rightHand,  cumulativeScale);
		UpdateEndBoneScaling(rightHand, handScaleAdjust * Vector3.one, skeleton.rightHand, prevRightHandDelta, cumulativeScale);

//		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK4 commented this
			limbStartScale = UpdateUniformBoneScaling(leftClavicle, leftShoulder, skeleton.leftClavicle, skeleton.leftShoulder, 
													  torsoMultiplier * clavicleScaleAdjust, neckScale);

		cumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeleton.leftShoulder, skeleton.leftElbow,  limbStartScale);
		cumulativeScale = UpdateBoneScaling(leftElbow,    leftHand,  skeleton.leftElbow,    skeleton.leftHand,  cumulativeScale);
		UpdateEndBoneScaling(leftHand, handScaleAdjust * Vector3.one, skeleton.leftHand, prevLeftHandDelta, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(rightHip,  rightKnee, skeleton.rightHip,  skeleton.rightKnee, cumulatedPelvisScale);
		cumulativeScale = UpdateBoneScaling(rightKnee, rightFoot, skeleton.rightKnee, skeleton.rightFoot,      cumulativeScale);
		UpdateEndBoneScaling(rightFoot, footScaleAdjust * Vector3.one, skeleton.rightFoot, prevRightFootDelta, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(leftHip,  leftKnee, skeleton.leftHip,  skeleton.leftKnee, cumulatedPelvisScale);
		cumulativeScale = UpdateBoneScaling(leftKnee, leftFoot, skeleton.leftKnee, skeleton.leftFoot,      cumulativeScale);
		UpdateEndBoneScaling(leftFoot, footScaleAdjust * Vector3.one, skeleton.leftFoot, prevLeftFootDelta, cumulativeScale);
	}

	private float UpdateUniformBoneScaling( Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, 
		                                   RUISSkeletonManager.JointData comparisonBoneTracker, float adjustScale, float accumulatedScale		)
	{
		if(!boneToScale)
			return accumulatedScale;
			
		float newScale = 1;
		float modelBoneLength = 1;
		float extremityTweaker = 1;
		float playerBoneLength = 1;
		bool isMidsectionBone = false;

		switch(boneToScaleTracker.jointID)
		{
			case RUISSkeletonManager.Joint.LeftKnee:   extremityTweaker = shinLengthRatio; break;
			case RUISSkeletonManager.Joint.RightKnee:  extremityTweaker = shinLengthRatio; break;
			case RUISSkeletonManager.Joint.LeftElbow:  extremityTweaker = forearmLengthRatio; break;
			case RUISSkeletonManager.Joint.RightElbow: extremityTweaker = forearmLengthRatio; break;
			case RUISSkeletonManager.Joint.Torso: 	   isMidsectionBone = true; break;
			case RUISSkeletonManager.Joint.Chest: 	   isMidsectionBone = true; break;
			case RUISSkeletonManager.Joint.Neck: 	   isMidsectionBone = true; break;
			case RUISSkeletonManager.Joint.LeftClavicle:  isMidsectionBone = true; break;
			case RUISSkeletonManager.Joint.RightClavicle: isMidsectionBone = true; break;
		}

		if(independentTorsoSegmentsScaling || !isMidsectionBone)
		{
			if(comparisonBone)
			{
				modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];

				if(boneToScaleTracker.positionConfidence > 0 && comparisonBoneTracker.positionConfidence > 0)
				{
					playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
					trackedBoneLengths[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)] = playerBoneLength; // *** OPTIHACK4 check that works
				}
				else
					playerBoneLength = trackedBoneLengths[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];
						
				if(hasBeenTracked)
					newScale = playerBoneLength / modelBoneLength;
			}
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
		float playerBoneLength = 1;
		float newScale = 1;
		float thickness = 1;
//		float parentBoneThickness = 1;
		float extremityTweaker = 1;
		float skewedScaleTweak = 1;
		float thicknessU = 1;
		float thicknessV = 1;
		bool isLimbStart = false;
		bool isExtremityJoint = false;
		bool isEndBone = false;
		Vector3 previousScale = Vector3.one;
		Vector3 avatarBoneVector;

		if(comparisonBone)
		{
			modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];

			if(boneToScaleTracker.positionConfidence > 0 && comparisonBoneTracker.positionConfidence > 0)
			{
				playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
				trackedBoneLengths[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)] = playerBoneLength; // *** OPTIHACK4 check that works
			}
			else
				playerBoneLength = trackedBoneLengths[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];

			if(hasBeenTracked)
				newScale = playerBoneLength / modelBoneLength; //playerBoneLength / modelBoneLength / accumulatedScale;

			avatarBoneVector = boneToScale.position - comparisonBone.position;
		}
		else
		{
			switch(boneLengthAxis) // *** OPTIHACK4 check that works
			{
				case RUISAxis.X: avatarBoneVector = boneToScale.rotation * Vector3.right; 	break;
				case RUISAxis.Y: avatarBoneVector = boneToScale.rotation * Vector3.up;		break;
				case RUISAxis.Z: avatarBoneVector = boneToScale.rotation * Vector3.forward; break;
				default: avatarBoneVector = boneToScale.rotation * Vector3.forward; break;
			}
		}
		
		switch(boneToScaleTracker.jointID)
		{
			case RUISSkeletonManager.Joint.LeftHip:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				previousScale = prevLeftHip;
				isLimbStart = true;
				break;
			case RUISSkeletonManager.Joint.RightHip:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				previousScale = prevRightHip;
				isLimbStart = true;
				break;
			case RUISSkeletonManager.Joint.LeftShoulder:
				thickness = leftArmThickness;
				thicknessU = leftArmThickness;
				thicknessV = leftArmThickness;
				previousScale = prevLeftShoulder;
				isLimbStart = true;
				break;
			case RUISSkeletonManager.Joint.RightShoulder:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				previousScale = prevRightShoulder;
				isLimbStart = true;
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
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.RightHand:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevRightHandScale;
				isExtremityJoint = true;
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.LeftFoot:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevLeftFootScale;
				isExtremityJoint = true;
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.RightFoot:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevRightFootScale;
				isExtremityJoint = true;
				isEndBone = true;
				break;
		}
		

		if(scaleBoneLengthOnly && limbsAreScaled)
		{
			if(isExtremityJoint && boneToScale.parent)
			{
				Vector3 avatarParentBone = boneToScale.parent.position - boneToScale.position;
				Vector3 u, v, w; // *** TODO remove w
				switch(boneLengthAxis)
				{
					case RUISAxis.X: u = Vector3.up;      v = Vector3.forward; w = Vector3.right;   break;
					case RUISAxis.Y: u = Vector3.forward; v = Vector3.right;   w = Vector3.up;      break;
					case RUISAxis.Z: u = Vector3.right;   v = Vector3.up;      w = Vector3.forward; break;
					default: u = Vector3.up; v = Vector3.forward; w = Vector3.right; break;
				}
				// *** OPTIHACK2
//				float jointAngle = Vector3.Angle(boneVector, avatarParentBone);
//				float cosAngle = Mathf.Cos(Mathf.Deg2Rad * jointAngle);
//				float sinAngle = Mathf.Sin(Mathf.Deg2Rad * jointAngle);


				if(isEndBone) // Hand / Foot   *** OPTIHACK this is not used at the moment
				{
					int uAxis = FindClosestGlobalAxis(boneToScale.localRotation, u);
					int vAxis = FindClosestGlobalAxis(boneToScale.localRotation, v);
					int wAxis = FindClosestGlobalAxis(boneToScale.localRotation, w);

					axisScales[0] = boneToScale.parent.localScale.x;
					axisScales[1] = boneToScale.parent.localScale.y;
					axisScales[2] = boneToScale.parent.localScale.z;

//					skewedScaleTweak = CalculateScale(boneToScale.rotation * w, avatarParentBone, axisScales[wAxis], accumulatedScale);	
//					thicknessU = CalculateScale(boneToScale.rotation * u, avatarParentBone, axisScales[uAxis], accumulatedScale);
//					thicknessV = CalculateScale(boneToScale.rotation * v, avatarParentBone, axisScales[vAxis], accumulatedScale);

					skewedScaleTweak = CalculateScale(boneToScale.rotation * w, avatarParentBone, thickness, accumulatedScale);	
					thicknessU 		 = CalculateScale(boneToScale.rotation * u, avatarParentBone, thickness, accumulatedScale);
					thicknessV 		 = CalculateScale(boneToScale.rotation * v, avatarParentBone, thickness, accumulatedScale);						
//				if(boneToScaleTracker.jointID == RUISSkeletonManager.Joint.RightHand)
//						print(skewedScaleTweak + " " + thicknessU + " " + thicknessV + " " + boneToScale.lossyScale + " " + boneToScale.localScale);
				}
				else // Forearm / Lower Leg
				{	
//					skewedScaleTweak = extremityTweaker / (accumulatedScale * cosAngle * cosAngle + thickness * sinAngle * sinAngle);
					skewedScaleTweak = extremityTweaker * CalculateScale(avatarBoneVector, 			avatarParentBone, thickness, accumulatedScale);
					thicknessU 		 = thickness 		* CalculateScale(boneToScale.rotation * u,  avatarParentBone, thickness, accumulatedScale);
					thicknessV 		 = thickness 		* CalculateScale(boneToScale.rotation * v,  avatarParentBone, thickness, accumulatedScale);

					// Below is a bit of a hack (average of thickness and accumulatedScale). A proper solution would have two thickness axes
//					thickness = thickness / (0.5f*(thickness + accumulatedScale) * sinAngle * sinAngle + thickness * cosAngle * cosAngle);
				}
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
		{
			if(limbsAreScaled)
				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, extremityTweaker * 
															 (newScale / accumulatedScale) * Vector3.one, maxScaleFactor * deltaTime);
			else if(isLimbStart)
				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, extremityTweaker * 
															 (1 / accumulatedScale) * Vector3.one, maxScaleFactor * deltaTime);
		}


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

	private int[] 	axisLabels = new int[3];
	private float[] axisScales = new float[3];
	private Vector3 delta = Vector3.zero;
	private const float minEndBoneScale = 10000f * float.Epsilon; // HACK
	Vector3 signChanged;

	private void UpdateEndBoneScaling(Transform boneToScale, Vector3 boneScaleTarget, RUISSkeletonManager.JointData joint, Vector3 previousDelta, 
									  float accumulatedScale)
	{
		if(!boneToScale)
			return;
				
		Vector3 updatedScale = Vector3.one;

		if(scaleBoneLengthOnly && limbsAreScaled)
		{
			axisLabels[0] = FindClosestGlobalAxis(Quaternion.Inverse(boneToScale.rotation), Vector3.right	); // X
			axisLabels[1] = FindClosestGlobalAxis(Quaternion.Inverse(boneToScale.rotation), Vector3.up		); // Y
			axisLabels[2] = FindClosestGlobalAxis(Quaternion.Inverse(boneToScale.rotation), Vector3.forward	); // Z

			// Below only works if all elements of boneScaleTarget have the same value 
			axisScales[axisLabels[0]] = transform.lossyScale.x * boneScaleTarget.x / boneToScale.lossyScale.x;
			axisScales[axisLabels[1]] = transform.lossyScale.y * boneScaleTarget.y / boneToScale.lossyScale.y;
			axisScales[axisLabels[2]] = transform.lossyScale.z * boneScaleTarget.z / boneToScale.lossyScale.z;

			delta.Set(axisScales[axisLabels[0]] > 0 ? axisScales[axisLabels[0]] - 1 : 1 - axisScales[axisLabels[0]], 
					  axisScales[axisLabels[1]] > 0 ? axisScales[axisLabels[1]] - 1 : 1 - axisScales[axisLabels[1]], 
					  axisScales[axisLabels[2]] > 0 ? axisScales[axisLabels[2]] - 1 : 1 - axisScales[axisLabels[2]] );
			signChanged = Vector3.Scale(delta, previousDelta);
			signChanged.Set((signChanged.x < 0) ? 0.01f : 1, (signChanged.y < 0) ? 0.01f : 1, (signChanged.z < 0) ? 0.01f : 1); // HACK

			switch(joint.jointID)
			{
				case RUISSkeletonManager.Joint.LeftHand:   
					prevLeftHandDelta  = delta;
					break;
				case RUISSkeletonManager.Joint.RightHand: 
					prevRightHandDelta = delta;
					break;
				case RUISSkeletonManager.Joint.LeftFoot:  
					prevLeftFootDelta  = delta;
					break;
				case RUISSkeletonManager.Joint.RightFoot:   
					prevRightFootDelta = delta;
					break;
			}

			float descentSpeed = 10;

			// *** As is, RUISSkeletonController doesn't handle negative body part scales, but handles negative parent scale, if all of the axes are negative
			tempVector.Set( boneToScale.localScale.x + delta.x * descentSpeed * signChanged.x * Mathf.Pow(1 - Mathf.Abs(axisScales[axisLabels[0]]), 2), 
							boneToScale.localScale.y + delta.y * descentSpeed * signChanged.y * Mathf.Pow(1 - Mathf.Abs(axisScales[axisLabels[1]]), 2), 
							boneToScale.localScale.z + delta.z * descentSpeed * signChanged.z * Mathf.Pow(1 - Mathf.Abs(axisScales[axisLabels[2]]), 2) );
				
			// *** HACK
			float maxLossyScale = Mathf.Max(transform.lossyScale.x * boneScaleTarget.x, 
											Mathf.Max(transform.lossyScale.y * boneScaleTarget.y, transform.lossyScale.z * boneScaleTarget.z));
			float minLossyScale = Mathf.Min(axisScales[0], Mathf.Min(axisScales[1], axisScales[2]));
			if(minLossyScale > 0)
				minLossyScale = 0;
				
			updatedScale.Set(Mathf.Clamp(tempVector.x==0? minEndBoneScale : tempVector.x, minLossyScale, maxLossyScale),
							 Mathf.Clamp(tempVector.y==0? minEndBoneScale : tempVector.y, minLossyScale, maxLossyScale),
							 Mathf.Clamp(tempVector.z==0? minEndBoneScale : tempVector.z, minLossyScale, maxLossyScale) );
			updatedScale.Set(float.IsNaN(tempVector.x) ? boneToScale.localScale.x : tempVector.x,
							 float.IsNaN(tempVector.y) ? boneToScale.localScale.y : tempVector.y,
							 float.IsNaN(tempVector.z) ? boneToScale.localScale.z : tempVector.z );
//			print(transform.parent.name + " " + joint.jointID + " " + minLossyScale + " " + maxLossyScale + " " + (tempVector - boneToScale.localScale));
		}
		else
		{
			updatedScale = boneScaleTarget / accumulatedScale;
		}

		boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, updatedScale, 10 * deltaTime); // *** TODO: speed 10 might not be good
//		if(joint.jointID == RUISSkeletonManager.Joint.LeftFoot)
//			print(axisLabels[0] + " " + axisLabels[1] + " " + axisLabels[2] + " " + updatedScale + " " + boneToScale.lossyScale.x);

	}

	private int FindClosestGlobalAxis(Quaternion rotation, Vector3 globalAxis)
	{
			float minAngle;
			float sinAngle;
			int axisID = 0;
			minAngle = Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(rotation * globalAxis, Vector3.right));
			sinAngle = Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(rotation * globalAxis, Vector3.up));
			if(sinAngle < minAngle)
			{
				minAngle = sinAngle;
				axisID = 1;
			}
			sinAngle = Mathf.Sin(Mathf.Deg2Rad * Vector3.Angle(rotation * globalAxis, Vector3.forward));
			if(sinAngle < minAngle)
			{
				minAngle = sinAngle;
				axisID = 2;
			}
			return axisID;
	}

	private float UpdateTorsoScale()
	{
		//average hip to shoulder length and compare it to the one found in the model - scale accordingly
		//we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
		float modelLength = modelSpineLength;
//		float modelLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, leftHip)] +
//				jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2; // *** OPTIHACK was this before
//		float playerLength = (Vector3.Distance(skeleton.rightShoulder.position, skeleton.leftShoulder.position) +
//		                      Vector3.Distance(skeleton.rightHip.position,      skeleton.leftHip.position     )) / 2;
//		float playerLength = (Vector3.Distance( rightShoulder.position,  leftShoulder.position) + // *** THIS IS WRONG, SCALING APPLIES ON THESE TRANSFORMS
//		                      Vector3.Distance(      rightHip.position,       leftHip.position)  ) / 2;
//		float playerLength = (Vector3.Distance(forcedJointPositions[0], forcedJointPositions[1]) +
//		                      Vector3.Distance(skeleton.rightHip.position, skeleton.leftHip.position)) / 2; // *** OPTIHACK was this before
		float playerLength = GetTrackedSpineLength(); // For Kinect 1/2 this is same as below:
//		float playerLength = (Vector3.Distance( forcedJointPositions[0], forcedJointPositions[1]) +
//		                      Vector3.Distance( forcedJointPositions[2], forcedJointPositions[3])  ) / 2;
		
		float newScale = Mathf.Abs(playerLength / modelLength); // * (scaleBoneLengthOnly ? torsoThickness : 1);

		// *** HACK: Here we halve the maxScaleFactor because the torso is bigger than the limbs
		torsoScale = Mathf.Lerp(torsoScale, newScale, 0.5f * maxScaleFactor * deltaTime);

		// *** OPTIHACK3 from below uncommented part used to be: torso.localScale = torsoThickness * torsoScale * Vector3.one;
//		if(scaleBoneLengthOnly)
//		{
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
//			torso.localScale = torsoThickness * torsoScale * Vector3.one; // *** OPTIHACK
//		}
//		else
//			torso.localScale = torsoThickness * torsoScale * Vector3.one; // *** OPTIHACK
		return torsoScale;
	}

	private Quaternion FindFixingRotation(Vector3 fromJoint, Vector3 toJoint, Vector3 wantedDirection)
	{
		Vector3 boneVector = toJoint - fromJoint;
		return Quaternion.FromToRotation(boneVector, wantedDirection);
	}

	public float GetTrackedSpineLength()
	{
		float lengthSum = 0;
		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customSpineJointCount > 1)
		{
			for(int i = 1; i < customSpineJointCount; ++i) 
				lengthSum += Vector3.Distance(trackedSpineJoints[i - 1].position, trackedSpineJoints[i].position);
		}
		else
		{
			lengthSum = (Vector3.Distance( forcedJointPositions[0], forcedJointPositions[1] ) +
						 Vector3.Distance( forcedJointPositions[2], forcedJointPositions[3] )  ) / 2;
		}

		return lengthSum;
	}

	public bool ConfidenceGoodEnoughForScaling()
	{
		return !(skeleton.rightShoulder.positionConfidence	< minimumConfidenceToUpdate ||
				 skeleton.leftShoulder.positionConfidence	< minimumConfidenceToUpdate ||
				 skeleton.rightHip.positionConfidence		< minimumConfidenceToUpdate ||
				 skeleton.leftHip.positionConfidence		< minimumConfidenceToUpdate   );
	}

	private void HandleFistMaking()
	{
		bool closeHand;
		int inv = 1;
		float rotationSpeed = 10.0f; // Per second
		Quaternion clenchedRotationThumbTM_corrected  = Quaternion.identity;
		Quaternion clenchedRotationThumbMCP_corrected = Quaternion.identity;
		Quaternion clenchedRotationThumbIP_corrected  = Quaternion.identity;
		
		RUISSkeletonManager.Skeleton.handState currentLeftHandStatus = leftHandStatus;
		RUISSkeletonManager.Skeleton.handState currentRightHandStatus = rightHandStatus;

		if(externalFistTrigger)
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
		
		// Hands
		for(int i = 0; i < 2; i++)
		{
			if(i == 0) // Right hand
			{
				closeHand = (currentRightHandStatus == RUISSkeletonManager.Skeleton.handState.closed); // closeHand == should we make a fist?
				inv = -1;
			}
			else       // Left hand
			{
				closeHand = (currentLeftHandStatus == RUISSkeletonManager.Skeleton.handState.closed);  // closeHand == should we make a fist?
				inv = 1;
			}
			// Thumb rotation correction: these depend on your animation rig
			switch(boneLengthAxis)
			{
				case RUISAxis.X:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler( clenchedThumbAngleTM.x * inv,  clenchedThumbAngleTM.y * inv,  clenchedThumbAngleTM.z)
														 * Quaternion.Euler(thumbRotationOffset.x * inv,   thumbRotationOffset.y * inv,   thumbRotationOffset.z);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedThumbAngleMCP.x * inv, clenchedThumbAngleMCP.y * inv, clenchedThumbAngleMCP.z);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler( clenchedThumbAngleIP.x * inv,  clenchedThumbAngleIP.y * inv,  clenchedThumbAngleIP.z);
				break;
				case RUISAxis.Y:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler( clenchedThumbAngleTM.x,  clenchedThumbAngleTM.y * inv,  clenchedThumbAngleTM.z * inv)
														 * Quaternion.Euler(thumbRotationOffset.x,   thumbRotationOffset.y * inv,   thumbRotationOffset.z * inv);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedThumbAngleMCP.x, clenchedThumbAngleMCP.y * inv, clenchedThumbAngleMCP.z * inv);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler( clenchedThumbAngleIP.x,  clenchedThumbAngleIP.y * inv,  clenchedThumbAngleIP.z * inv);
					break;
				case RUISAxis.Z:
					clenchedRotationThumbTM_corrected  = Quaternion.Euler( clenchedThumbAngleTM.x * inv,  clenchedThumbAngleTM.y,  clenchedThumbAngleTM.z * inv)
														 * Quaternion.Euler(thumbRotationOffset.x * inv,   thumbRotationOffset.y,   thumbRotationOffset.z * inv);
					clenchedRotationThumbMCP_corrected = Quaternion.Euler(clenchedThumbAngleMCP.x * inv, clenchedThumbAngleMCP.y, clenchedThumbAngleMCP.z * inv);
					clenchedRotationThumbIP_corrected  = Quaternion.Euler( clenchedThumbAngleIP.x * inv,  clenchedThumbAngleIP.y,  clenchedThumbAngleIP.z * inv);
					break;
			}
			// *** OPTIHACK5 this probably should be removed: If left thumb rotations have been set separately 
			if(leftThumbHasIndependentRotations && i == 1) // i == 1 is left hand
			{
				clenchedRotationThumbTM_corrected 	= clenchedLeftThumbTM;
				clenchedRotationThumbMCP_corrected 	= clenchedLeftThumbMCP;
				clenchedRotationThumbIP_corrected 	= clenchedLeftThumbIP;
			}

			// Fingers
			for(int j = 0; j < 5; j++)
			{ 
				if(!closeHand && !(j == 4 && kinect2Thumbs)) // Not making a fist: Lets straighten this finger (unless it's a Kinect 2 -tracked thumb)
				{
					if(fingerTargets[i, j, 0])
						fingerTargets[i, j, 0].localRotation = Quaternion.Slerp(fingerTargets[i, j, 0].localRotation, initialFingerLocalRotations[i, j, 0], deltaTime * rotationSpeed);
					if(fingerTargets[i, j, 1])
						fingerTargets[i, j, 1].localRotation = Quaternion.Slerp(fingerTargets[i, j, 1].localRotation, initialFingerLocalRotations[i, j, 1], deltaTime * rotationSpeed);
					if(fingerTargets[i, j, 2])
						fingerTargets[i, j, 2].localRotation = Quaternion.Slerp(fingerTargets[i, j, 2].localRotation, initialFingerLocalRotations[i, j, 2], deltaTime * rotationSpeed);
				}
				else // Making a fist: Lets flex this finger
				{
					if(j != 4) // Fingers that are not thumbs
					{
						if(fingerTargets[i, j, 0])
							fingerTargets[i, j, 0].localRotation = Quaternion.Slerp(fingerTargets[i, j, 0].localRotation, Quaternion.Euler(clenchedFingerAngleMCP), deltaTime * rotationSpeed);
						if(fingerTargets[i, j, 1])
							fingerTargets[i, j, 1].localRotation = Quaternion.Slerp(fingerTargets[i, j, 1].localRotation, Quaternion.Euler(clenchedFingerAnglePIP), deltaTime * rotationSpeed);
						if(fingerTargets[i, j, 2])
							fingerTargets[i, j, 2].localRotation = Quaternion.Slerp(fingerTargets[i, j, 2].localRotation, Quaternion.Euler(clenchedFingerAngleDIP), deltaTime * rotationSpeed);
					}
					else if(!kinect2Thumbs) // Thumbs (if separate thumb  tracking is not enabled)
					{
						if(fingerTargets[i, j, 0])
							fingerTargets[i, j, 0].localRotation = Quaternion.Slerp(fingerTargets[i, j, 0].localRotation,  clenchedRotationThumbTM_corrected, deltaTime * rotationSpeed);
						if(fingerTargets[i, j, 1])
							fingerTargets[i, j, 1].localRotation = Quaternion.Slerp(fingerTargets[i, j, 1].localRotation, clenchedRotationThumbMCP_corrected, deltaTime * rotationSpeed);
						if(fingerTargets[i, j, 2])
							fingerTargets[i, j, 2].localRotation = Quaternion.Slerp(fingerTargets[i, j, 2].localRotation,  clenchedRotationThumbIP_corrected, deltaTime * rotationSpeed);
					}	
				}	
			}
		}
	}

	// *** TODO remove
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
					initialFingerLocalRotations[i, index, 0] = finger.localRotation;
					fingerTargets[i, index, 0] = finger;
					Transform[] nextFingerParts = finger.gameObject.GetComponentsInChildren<Transform>();
					foreach(Transform part1 in nextFingerParts)
					{
						if(part1.parent.transform.gameObject == finger.gameObject
						    && (part1.gameObject.name.Contains("finger") || part1.gameObject.name.Contains("Finger") || part1.gameObject.name.Contains("FINGER")))
						{
							// Second bone
							initialFingerLocalRotations[i, index, 1] = part1.localRotation;
							fingerTargets[i, index, 1] = part1;
							Transform[] nextFingerParts2 = finger.gameObject.GetComponentsInChildren<Transform>();
							foreach(Transform part2 in nextFingerParts2)
							{
								if(part2.parent.transform.gameObject == part1.gameObject
								    && (part2.gameObject.name.Contains("finger") || part2.gameObject.name.Contains("Finger") || part2.gameObject.name.Contains("FINGER")))
								{
									// Third bone
									initialFingerLocalRotations[i, index, 2] = part2.localRotation;
									fingerTargets[i, index, 2] = part2; 
								}
							}
						}
					}
				}
			}	
		}
	}

	private void FindAndInitializeFingers(Transform[,,] fingerArray, bool isTargetFingers)
	{	
		Transform handObject;

		for(int i = 0; i < 2; i++) // rightHand: i == 0, leftHand: i == 1
		{ 
			// Assign fingerArray with fingers (more accurately, their proximal phalanx transforms)
			if(isTargetFingers)
			{
				if(i == 0) // Right
				{
					fingerArray[i, 0, 0] = rightLittleF;
					fingerArray[i, 1, 0] = rightRingF;
					fingerArray[i, 2, 0] = rightMiddleF;
					fingerArray[i, 3, 0] = rightIndexF;
					fingerArray[i, 4, 0] = rightThumb;
				}
				else // Left
				{
					fingerArray[i, 0, 0] = leftLittleF;
					fingerArray[i, 1, 0] = leftRingF;
					fingerArray[i, 2, 0] = leftMiddleF;
					fingerArray[i, 3, 0] = leftIndexF;
					fingerArray[i, 4, 0] = leftThumb;
				}
			}
			else // Custom Finger Sources
			{
				if(i == 0) // Right
				{
					fingerArray[i, 0, 0] = customRightLittleF;
					fingerArray[i, 1, 0] = customRightRingF;
					fingerArray[i, 2, 0] = customRightMiddleF;
					fingerArray[i, 3, 0] = customRightIndexF;
					fingerArray[i, 4, 0] = customRightThumb;
				}
				else // Left
				{
					fingerArray[i, 0, 0] = customLeftLittleF;
					fingerArray[i, 1, 0] = customLeftRingF;
					fingerArray[i, 2, 0] = customLeftMiddleF;
					fingerArray[i, 3, 0] = customLeftIndexF;
					fingerArray[i, 4, 0] = customLeftThumb;
				}
			}
			
			List<int> unassignedFingers = new List<int>();

			// First fill the finger (phalanx) array with Transforms that have been assigned in the Editor (Finger Targets/Sources)
			for(int j = 0; j < 5; ++j)
			{
				if(fingerArray[i, j, 0])
				{
					// Proximal phalanx (1st one from the hand)
					if(isTargetFingers)
					{
						initialFingerWorldRotations[i, j, 0] = GetInitialRotation(fingerArray[i, j, 0]);
						initialFingerLocalRotations[i, j, 0] = fingerArray[i, j, 0].localRotation;
						hasFingerTarget[i, j, 0] = true;
					}
					else
					{
						hasFingerSource[i, j, 0] = true;
						noSourceFingers = false;
					}
					foreach(Transform middlePhalanx in fingerArray[i, j, 0].GetComponentsInChildren<Transform>(true))
					{
						if(middlePhalanx.parent.gameObject == fingerArray[i, j, 0].gameObject)
						{
							// Middle phalanx (2nd one from the hand)
							if(isTargetFingers)
							{
								initialFingerWorldRotations[i, j, 1] = GetInitialRotation(middlePhalanx);
								initialFingerLocalRotations[i, j, 1] = middlePhalanx.localRotation;
								hasFingerTarget[i, j, 1] = true;
							}
							else
								hasFingerSource[i, j, 1] = true;
							fingerArray[i, j, 1] = middlePhalanx;
							foreach(Transform distalPhalanx in middlePhalanx.GetComponentsInChildren<Transform>(true))
							{
								if(distalPhalanx.parent.gameObject == middlePhalanx.gameObject)
								{
									// Distal phalanx (3rd one from the hand)
									if(isTargetFingers)
									{
										initialFingerWorldRotations[i, j, 2] = GetInitialRotation(distalPhalanx);
										initialFingerLocalRotations[i, j, 2] = distalPhalanx.localRotation;
										hasFingerTarget[i, j, 2] = true;
									}
									else
										hasFingerSource[i, j, 2] = true;
									fingerArray[i, j, 2] = distalPhalanx; 
								}
							}
						}
					}
				}
				else if(isTargetFingers)
				{
					// This finger target Transform index has not been assigned in Editor
					unassignedFingers.Add(j);
				}
			}

			// If there were unassigned Finger Targets in the Editor, then try to fill them using child transforms from each hand
			if(isTargetFingers && unassignedFingers.Count > 0)
			{
				if(i == 0)
					handObject = rightHand;
				else
					handObject = leftHand;

				if(handObject == null)
					continue;

				Transform[] fingers;
				int foundFingerCount = 0;

				fingers = handObject.GetComponentsInChildren<Transform>();
				foreach(Transform finger in fingers)
				{
					int fingerIndex = unassignedFingers[foundFingerCount]; // Get index of next unassigned finger 

					if(finger.parent && finger.parent.gameObject == handObject.gameObject && ContainsFingerSubstring(finger.name))
					{
						bool isAlreadyAssigned = false;
						for(int j = 0; j < 5; ++j)
						{
							if(finger == fingerArray[i, j, 0])
							{
								// Make sure that the finger was not assigned in the previous loop
								isAlreadyAssigned = true;
								break;
							}
						}

						if(isAlreadyAssigned || fingerIndex > 5)
							continue;

						// Found an unassigned finger of an unknown identity (thumb/index/middle/ring/little finger?)
						foundFingerCount++;

						// Proximal phalanx (1st one from the hand)
						initialFingerLocalRotations[i, fingerIndex, 0] = finger.localRotation;
						fingerArray[i, fingerIndex, 0] = finger;
						foreach(Transform middlePhalanx in finger.GetComponentsInChildren<Transform>())
						{
							if(middlePhalanx.parent.gameObject == finger.gameObject && ContainsFingerSubstring(middlePhalanx.name))
							{
								// Middle phalanx (2nd one from the hand)
								initialFingerLocalRotations[i, fingerIndex, 1] = middlePhalanx.localRotation;
								fingerArray[i, fingerIndex, 1] = middlePhalanx;
								foreach(Transform distalPhalanx in finger.GetComponentsInChildren<Transform>())
								{
									if(distalPhalanx.parent.gameObject == middlePhalanx.gameObject && ContainsFingerSubstring(distalPhalanx.name))
									{
										// Distal phalanx (3rd one from the hand)
										initialFingerLocalRotations[i, fingerIndex, 2] = distalPhalanx.localRotation;
										fingerArray[i, fingerIndex, 2] = distalPhalanx; 
									}
								}
							}
						}

						if(foundFingerCount >= unassignedFingers.Count)
							break; // Only 5 fingers allowed!
					}
				}
			}
		}
	}

	private bool ContainsFingerSubstring(string str)
	{
		return (str.Contains("finger") || str.Contains("Finger") || str.Contains("FINGER"));
	}

	void UpdateFingerRotations()
	{
		if(noSourceFingers || !updateJointRotations)
			return;
		
		Quaternion rotationOffset;
		Quaternion newRotation;
		Vector3 rotOffset;
		float maxAngularDelta = Time.deltaTime * maxFingerAngularVelocity;

		for(int i = 0; i < hasFingerTarget.GetLength(0); ++i)
		{
			for(int j = 0; j < hasFingerTarget.GetLength(1); ++j)
			{
				if(j == 4 && kinect2Thumbs)
					continue;  // Kinect2 is used for thumb tracking
				for(int k = 0; k < hasFingerTarget.GetLength(2); ++k)
				{
					if(hasFingerTarget[i, j, k])
					{
						if(hasFingerSource[i, j, k])
						{
							if(fingerConfidence[i, j] >= minimumConfidenceToUpdate) // It is up to the developer to modify fingerConfidence[i, j]
							{
								switch(j)
								{
								case 4: rotOffset = thumbRotationOffset;   break; // Thumb
								case 3: rotOffset = indexFRotationOffset;  break; // Index finger
								case 2: rotOffset = middleFRotationOffset; break; // Middle finger
								case 1: rotOffset = ringFRotationOffset;   break; // Ring finger
								case 0: rotOffset = littleFRotationOffset; break; // Little finger
								default: rotOffset = Vector3.zero; 		   break; // Mutant fingers should not exist
								}
								if(i == 1) // Left hand
									rotOffset.Set(rotOffset.x, -rotOffset.y, -rotOffset.z);
								rotationOffset = Quaternion.Euler(rotOffset);

								// At the moment only hierarchical finger phalanx Transforms are supported
								newRotation = transform.rotation * fingerSources[i, j, k].rotation * rotationOffset * initialFingerWorldRotations[i, j, k];
								fingerTargets[i, j, k].rotation = Quaternion.RotateTowards(fingerTargets[i, j, k].rotation, newRotation, maxAngularDelta);
							}
						}
					}
				}
			}
		}
	}

	void CopySkeletonJointData(RUISSkeletonManager.Skeleton sourceSkeleton, RUISSkeletonManager.Skeleton targetSkeleton)
	{
		CopyJoint(sourceSkeleton.root,  targetSkeleton.root);
		CopyJoint(sourceSkeleton.torso, targetSkeleton.torso);
		CopyJoint(sourceSkeleton.chest, targetSkeleton.chest);
		CopyJoint(sourceSkeleton.neck,  targetSkeleton.neck);
		CopyJoint(sourceSkeleton.head,  targetSkeleton.head);
		CopyJoint(sourceSkeleton.leftClavicle, targetSkeleton.leftClavicle);
		CopyJoint(sourceSkeleton.leftShoulder, targetSkeleton.leftShoulder);
		CopyJoint(sourceSkeleton.leftElbow,    targetSkeleton.leftElbow);
		CopyJoint(sourceSkeleton.leftHand,     targetSkeleton.leftHand);
		CopyJoint(sourceSkeleton.leftThumb,    targetSkeleton.leftThumb);
		CopyJoint(sourceSkeleton.leftHip,      targetSkeleton.leftHip);
		CopyJoint(sourceSkeleton.leftKnee,     targetSkeleton.leftKnee);
		CopyJoint(sourceSkeleton.leftFoot,     targetSkeleton.leftFoot);
		CopyJoint(sourceSkeleton.rightClavicle, targetSkeleton.rightClavicle);
		CopyJoint(sourceSkeleton.rightShoulder, targetSkeleton.rightShoulder);
		CopyJoint(sourceSkeleton.rightElbow,    targetSkeleton.rightElbow);
		CopyJoint(sourceSkeleton.rightHand,     targetSkeleton.rightHand);
		CopyJoint(sourceSkeleton.rightThumb,    targetSkeleton.rightThumb);
		CopyJoint(sourceSkeleton.rightHip,      targetSkeleton.rightHip);
		CopyJoint(sourceSkeleton.rightKnee,     targetSkeleton.rightKnee);
		CopyJoint(sourceSkeleton.rightFoot,     targetSkeleton.rightFoot);

		targetSkeleton.leftHandStatus  = sourceSkeleton.leftHandStatus;
		targetSkeleton.rightHandStatus = sourceSkeleton.rightHandStatus;
	}

	void CopyJoint(RUISSkeletonManager.JointData copySource, RUISSkeletonManager.JointData copyTarget)
	{
		copyTarget.position = copySource.position;
		copyTarget.rotation = copySource.rotation;
		copyTarget.positionConfidence = copySource.positionConfidence;
		copyTarget.rotationConfidence = copySource.rotationConfidence;
	}

	private bool yawCorrectButtonPressed = false;
	System.Collections.IEnumerator CorrectYawImmediately()
	{
		yawCorrectButtonPressed = true;
		yield return new WaitForSeconds(0.2f);
		yawCorrectButtonPressed = false;
	}

	/// <summary>
	/// Gets the yaw drift correction rotation, if yawCorrectIMU is set to true
	/// </summary>
	/// <returns>The yaw drift correction.</returns>
	/// <param name="driftlessRotation">Driftless rotation.</param>
	/// <param name="driftingRotation">Drifting rotation.</param>
	public Quaternion GetYawDriftCorrection(Quaternion driftlessRotation, Quaternion driftingRotation) 
	{
		// drifting rotation transform.rotation = Quaternion.Inverse(parent.rotation) * child.rotation;

		if(Input.GetKeyDown(yawCorrectResetButton))
			StartCoroutine(CorrectYawImmediately());

//			if(driftingParent)
//				driftingRotation = Quaternion.Inverse(driftingParent.rotation) * driftingChild.rotation;
//			else
//				driftingRotation = driftingChild.rotation;

//			driftingForward  = driftingRotation * Quaternion.Euler(driftlessToDriftingOffset) * Vector3.forward;
//			driftlessForward = (driftlessIsLocalRotation ? driftlessTransform.localRotation : driftlessTransform.rotation) * Vector3.forward;
		driftingForward  = driftingRotation * Vector3.forward;
		driftlessForward = driftlessRotation * Vector3.forward;

//			#if UNITY_EDITOR
//			Debug.DrawRay(driftingChild.position, driftingForward);
//			Debug.DrawRay(driftingChild.position, 0.5f * (driftingRotation * Quaternion.Euler(driftlessToDriftingOffset) * Vector3.up));
//			#endif

		// HACK: Project forward vectors to XZ-plane. This is a problem if they are constantly parallel to Y-axis, e.g. HMD user looking directly up or down 
		driftingForward.Set(driftingForward.x, 0, driftingForward.z);
		driftlessForward.Set(driftlessForward.x, 0, driftlessForward.z);

		// HACK: If either forward vector is constantly parallel to Y-axis, no drift correction occurs. Occasionally this is OK, as the drift correction occurs gradually.
		if(driftingForward.magnitude > 0.01f && driftlessForward.magnitude > 0.01f) 
		{
			// HACK: Vector projection to XZ-plane ensures that the change in the below driftVector is continuous, 
			//		 as long as rotation change in driftingRotation and driftlessTransform is continuous. Otherwise 
			//		 more math is needed to ensure the continuity...
			driftVector = Quaternion.Euler(0, ((Vector3.Cross(driftingForward, driftlessForward).y < 0)?-1:1)
				                           		* Vector3.Angle(driftingForward, driftlessForward), 0) * Vector3.forward;

			if(yawCorrectButtonPressed)
				currentCorrectionVelocity = initCorrectionVelocity;
			else
				currentCorrectionVelocity = yawCorrectAngularVelocity;

			// 2D vector rotated by yaw difference has continuous components
			measuredDrift[0] = driftVector.x;
			measuredDrift[1] = driftVector.z;

			// Simple Kalman filtering
			filterDrift.setR(Time.deltaTime * driftNoiseCovariance);
			filterDrift.predict();
			filterDrift.update(measuredDrift);
			filteredDrift = filterDrift.getState();

			tempVector.Set((float)filteredDrift [0], 0, (float)filteredDrift [1]);
			rotationDrift = Quaternion.RotateTowards(rotationDrift, Quaternion.LookRotation(tempVector), currentCorrectionVelocity * Time.deltaTime);
//				if(correctionTarget)
//					correctionTarget.localRotation = filteredRotation;
		}
		return rotationDrift;
	}

	void RotatePositionAroundPivot(ref Vector3 position, Vector3 pivot, Quaternion rotation, Vector3 offset)
	{
		if(yawCorrectIMU)
			position = pivot + rotation * (position - pivot) + offset;
		else
			position += offset;
	}

	// If memory serves me correctly, this method doesn't work quite right
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
		
//		print(rollAngle + " " + rotationWithoutRoll.eulerAngles);
		
		return outputRotation;
	}
}

			
