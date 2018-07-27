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

	public Transform customParent;

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
	public float leftHandScaleAdjust  = 1;
	public float rightHandScaleAdjust = 1;
//	public float hipScaleAdjust 	 = 1; // TODO is this needed?
//	public float kneeScaleAdjust     = 1; // TODO is this needed?
	public float footScaleAdjust 	 = 1; // left and right separately
	public float leftFootScaleAdjust 	 = 1;
	public float rightFootScaleAdjust 	 = 1;

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
	public bool scalingNeck = false;
	public bool scalingClavicles = false;
	public bool heightAffectsOffsets = false; // TODO

	public bool forceChestPosition = true;
	public bool forceNeckPosition  = false;
	public bool forceHeadPosition  = true;
	public bool forceClaviclePosition  = false;

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

	public enum AvatarColliderType
	{
		CapsuleCollider,
		BoxCollider
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

					if(Application.isPlaying)
					{
						minimumConfidenceToUpdate = 0.5f;
						kinect2Thumbs = false;

						// When it comes to GenericMotionTracker, it's up to the developer to change isTracking values in realtime
						StartCoroutine(DelayedCustomTrackingStart(customTrackingDelay));

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
	public float customTrackingDelay = 1;

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
	public RUISAxis torsoBoneLengthAxis  = RUISAxis.X;
	public RUISAxis armBoneLengthAxis    = RUISAxis.X;
	public RUISAxis legBoneLengthAxis    = RUISAxis.X;
	public RUISAxis fingerBoneLengthAxis = RUISAxis.X;
	public float maxScaleFactor = 0.5f; // *** Set to 0.01 when decoupling of thickness & scale adjust is done
	public bool limbsAreScaled = true;

	public float torsoThickness = 1;
	public float rightArmThickness = 1;
	public float rightUpperArmThickness = 1;
	public float rightForearmThickness 	= 1;
	public float leftArmThickness = 1;
	public float leftUpperArmThickness 	= 1;
	public float leftForearmThickness 	= 1;
	public float rightLegThickness = 1;
	public float rightThighThickness 	= 1;
	public float rightShinThickness 	= 1;
	public float leftLegThickness = 1;
	public float leftThighThickness 	= 1;
	public float leftShinThickness 		= 1;

	public float minimumConfidenceToUpdate = 0.5f;
	public float maxAngularVelocity = 360.0f;
	public float maxFingerAngularVelocity = 720.0f;

	// Constrained between [0, -180] in Unity Editor script
	public float handRollAngleMinimum = -180; 

	// Constrained between [0,  180] in Unity Editor script
	public float handRollAngleMaximum = 180; 

	public bool hmdRotatesHead = false;
	public bool hmdMovesHead = false;
	public bool neckInterpolate = false;
	public bool followHmdPosition { get; private set; }
	public Vector3 hmdLocalOffset = Vector3.zero;
	public float neckInterpolateBlend = 0.8f;

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

//	private int followMoveID = 0;
//	private RUISPSMoveWand psmove;

//	private Vector3 torsoDirection = Vector3.down;
//	private Quaternion torsoRotation = Quaternion.identity;

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
	public int customMocapFrameRate = 30;
	public float customMocapUpdateInterval = 0.033f;

	private Dictionary<Transform, Quaternion> jointInitialRotations;
	private Dictionary<Transform, Vector3> jointInitialWorldPositions;
	private Dictionary<Transform, Vector3> jointInitialLocalPositions;
	private Dictionary<Transform, Vector3> jointInitialLocalScales;
	private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;
	private Dictionary<KeyValuePair<Transform, Transform>, float> trackedBoneLengths;
	private Dictionary<RUISSkeletonManager.Joint, float> automaticBoneScales;
	private Dictionary<RUISSkeletonManager.Joint, float> automaticBoneOffsets;
	private Dictionary<RUISSkeletonManager.Joint, Quaternion> intendedRotations;

	private Vector3 initialLocalPosition = Vector3.zero;
//	private Vector3 initialWorldPosition = Vector3.zero;
	private Vector3 initialLossyScale = Vector3.one;
	private Quaternion initialWorldRotation = Quaternion.identity;

	private float modelSpineLength = 0;
	private int customSpineJointCount = 0;
	private Transform[] trackedSpineJoints = new Transform[4];
	// *** OPTIHACK6 this is not used for anything yet
//	private RUISSkeletonManager.Joint highestSpineJoint = RUISSkeletonManager.Joint.RightShoulder; // Here RightShoulder refers to shoulders' midpoint

//	private float torsoOffset = 0.0f;

	private float torsoScale = 1.0f;

	public float forearmLengthRatio = 1.0f;
	public float shinLengthRatio = 1.0f;

	private Vector3 prevRightShoulderScale;
	private Vector3 prevRightForearmScale;
	private Vector3 prevRightHandScale;
	private Vector3 prevRightHipScale;
	private Vector3 prevRightShinScale;
	private Vector3 prevRightFootScale;
	private Vector3 prevLeftShoulderScale;
	private Vector3 prevLeftForearmScale;
	private Vector3 prevLeftHandScale;
	private Vector3 prevLeftHipScale;
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

	public AvatarColliderType avatarCollider = AvatarColliderType.CapsuleCollider;
	public float colliderRadius = 0.04f;
	public float colliderLengthOffset = 0;
	public bool createFingerColliders = true;
	public bool pelvisHasBoxCollider  = true;
	public bool chestHasBoxCollider   = true;
	public bool headHasBoxCollider    = true;
	public bool handHasBoxCollider    = true;
	public bool footHasBoxCollider    = true;
	public bool fingerHasBoxCollider  = true;

	public bool keepPlayModeChanges = true; // This is only for the custom inspector to use

	#if UNITY_EDITOR
	void Reset()
	{
		string consoleReport = "";
		string shortReport  = "";
		if(AutoAssignJointTargetsFromAvatar(out shortReport, out consoleReport))
			Debug.Log(consoleReport);
		else
			Debug.LogWarning(consoleReport);
	}
	#endif

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
		intendedRotations		   = new Dictionary<RUISSkeletonManager.Joint, Quaternion>();
		
		positionKalman = new KalmanFilter();
		positionKalman.Initialize(3, 3);

		for(int i = 0; i < fourJointsKalman.Length; ++i)
		{
			fourJointsKalman[i] = new KalmanFilter();
			fourJointsKalman[i].Initialize(3, 3);
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
		filterDrift.Initialize(2,2);
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
			BodyTrackingDeviceID = RUISSkeletonManager.customSensorID; // This setter sets bunch of values, including skeleton.isTracking
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

//			Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
//			Vector3 assumedRootPos = Vector3.Scale((rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4, scaler); 
															
//			Vector3 realRootPos = Vector3.Scale(torso.position, scaler);

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


		// Find bone length axes for torso, arms, legs, and fingers
		SetAndReportLengthAxes(errorInsteadOfWarning: true, calledInReset: false);

		// Below if-cluster calculates the model spine lenght as the combined bone lengths from pelvis to neck.
		// NOTE: Since the exact location of pelvis and neck varies between models, this is not a good method (see below).
		// Those parts of code where neck-head bone contributed to spine length have been commented
		if(chest)
		{
			modelSpineLength += SaveInitialDistance(torso, chest); // *** OPTIHACK
			if(neck)
				modelSpineLength += SaveInitialDistance(chest, neck); // *** OPTIHACK
			else if(head)
				/*modelSpineLength += */SaveInitialDistance(chest, head);
		}
		if(neck)
		{
//			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customNeck)
//				highestSpineJoint = RUISSkeletonManager.Joint.Neck;
			if(head)
				/*modelSpineLength += */SaveInitialDistance(neck, head); // *** OPTIHACK
			if(leftClavicle)
				SaveInitialDistance(neck, leftClavicle);
			if(rightClavicle)
				SaveInitialDistance(neck, rightClavicle);
			if(!chest)
				modelSpineLength += SaveInitialDistance(torso, neck); // *** OPTIHACK
		}
		if(head)
		{
//			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customHead)
//				highestSpineJoint = RUISSkeletonManager.Joint.Head;
			if(!chest && !neck)
				/*modelSpineLength += */SaveInitialDistance(torso, head);
		}

		// NOTE: By having the below if-clause commented, the modelSpineLength that was calculated above is overridden: 
		// Currently the model spine lenght is the distance between shoulder midpoint and hip midpoint
//		if(bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker || (!neck /*!head*/))
		{

			Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);

			// Calculate the distance between shoulder midpoint and hip midpoint 
			modelSpineLength = Vector3.Scale(0.5f * (       rightHip.position +      leftHip.position 
			                                         - rightShoulder.position - leftShoulder.position), scaler).magnitude;
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
			prevRightShoulderScale = rightShoulder.localScale;

		if(rightElbow)
			prevRightForearmScale = rightElbow.localScale;

		if(rightHand)
			prevRightHandScale = rightHand.localScale;

		if(rightHip)
			prevRightHipScale = rightHip.localScale;

		if(rightKnee)
			prevRightShinScale = rightKnee.localScale;

		if(rightFoot)
			prevRightFootScale = rightFoot.localScale;

		if(leftShoulder)
			prevLeftShoulderScale = leftShoulder.localScale;
		
		if(leftElbow)
			prevLeftForearmScale = leftElbow.localScale;

		if(leftHand)
			prevLeftHandScale = leftHand.localScale;
		
		if(leftHip)
			prevLeftHipScale = leftHip.localScale;

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
//			if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && (inputManager.enableKinect || inputManager.enableKinect2))
//				hasBeenTracked = true;

//			if(gameObject.transform.parent != null)
//			{
//				characterController = gameObject.transform.parent.GetComponent<RUISCharacterController>();
//				if(characterController != null)
//				{
//					if(characterController.characterPivotType == RUISCharacterController.CharacterPivotType.MoveController
//					   &&	inputManager.enablePSMove)
//					{
//						followMoveController = true;
//						followMoveID = characterController.moveControllerId;
////						if(		 gameObject.GetComponent<RUISKinectAndMecanimCombiner>() == null 
////							||	!gameObject.GetComponent<RUISKinectAndMecanimCombiner>().enabled )
//						Debug.LogWarning("Using PS Move controller #" + characterController.moveControllerId + " as a source "
//						+	"for avatar root position of " + gameObject.name + ", because PS Move is enabled"
//						+	"and the PS Move controller has been assigned as a "
//						+	"Character Pivot in " + gameObject.name + "'s parent GameObject");
//					}
//
//					if(   !inputManager.enableKinect && !inputManager.enableKinect2 && !followMoveController
//					   && bodyTrackingDevice != BodyTrackingDeviceType.GenericMotionTracker)
//					{
//						if(RUISDisplayManager.IsHmdPresent())
//						{
//							followHmdPosition = true;
//							Debug.LogWarning("Using " + RUISDisplayManager.GetHmdModel() + " as a Character Pivot for " + gameObject.name
//							+ ", because Kinects are disabled and " + RUISDisplayManager.GetHmdModel() + " was detected.");
//						}
//					}
//				}
//			}
		}

		skeletonPosition = transform.localPosition;
		initialLocalPosition = transform.localPosition;
//		initialWorldPosition = transform.position;
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
		customMocapUpdateInterval = 1.0f / ((float) customMocapFrameRate);

		if(neck && leftShoulder && rightShoulder)
			neckParentsShoulders = leftShoulder.IsChildOf(neck) || rightShoulder.IsChildOf(neck);

		// *** OPTIHACK6 following is not true because of trackedSpineJoints: "... you can leave the below Custom Source fields empty." (add neck estimation)
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
		// Lefts not add head joint because it's position can vary so much between different mocap systems (so can neck though)
//		if(customHead)
//		{
//			trackedSpineJoints[customSpineJointCount] = customHead;
//			++customSpineJointCount;
//		}
	}

	void LateUpdate()
	{
		deltaTime = Time.deltaTime; //1.0f / vr.hmd_DisplayFrequency;

		// Update skeleton based on data fetched from skeletonManager
		if(		skeletonManager != null && skeletonManager.skeletons[BodyTrackingDeviceID, playerId] != null
		    /*&&  skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking */)
		{
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

				// Rotations get filtered on every frame if there is fresh data according to HasNewMocapData(), regardless of customMocapUpdateInterval
				if(	   skeletonManager.skeletons[BodyTrackingDeviceID, playerId].filterRotations 
					&& skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking
					&& skeletonManager.HasNewMocapData(skeletonManager.skeletons[BodyTrackingDeviceID, playerId]))
				{
					skeletonManager.SaveLastMajorJointPoses(skeletonManager.skeletons[BodyTrackingDeviceID, playerId]);
					skeletonManager.FilterSkeletonRotations(BodyTrackingDeviceID, playerId, customMocapUpdateInterval);
				}
			}
			else // Kinect is used; copy the .isTracking value that has been set by it
				skeleton.isTracking = skeletonManager.skeletons[BodyTrackingDeviceID, playerId].isTracking;

			// If HMD is used as a source for head pose, set head position and rotation confidence accordingly
			if(RUISDisplayManager.IsHmdPresent())
			{
				if(hmdMovesHead && !headsetDragsBody)
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.positionConfidence = 1;
				if(hmdRotatesHead)
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotationConfidence = 1;
			}
			else
			{
				if(hmdMovesHead && !headsetDragsBody)
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.positionConfidence = 0;
				if(hmdRotatesHead)
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotationConfidence = 0;
			}

			// If interpolation is used as a source for neck pose, set neck position and rotation confidence accordingly 
			if(neckInterpolate)
			{
				// Neck pose interpolation depends on a bunch of poses
				if(		skeleton.isTracking 
				   && 	skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest.positionConfidence >= minimumConfidenceToUpdate
				   &&	skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.positionConfidence  >= minimumConfidenceToUpdate)
				{
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck.positionConfidence = 1;
					if(skeletonManager.skeletons[BodyTrackingDeviceID, playerId].chest.rotationConfidence >= minimumConfidenceToUpdate)
						skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck.rotationConfidence = 1;
					else
						skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck.rotationConfidence = 0;
				}
				else
				{
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck.positionConfidence = 0;
					skeletonManager.skeletons[BodyTrackingDeviceID, playerId].neck.rotationConfidence = 0;
				}
			}

			if(skeleton.isTracking || !hasBeenTracked)
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

			// Get head position from HMD if that option is enabled
			if(hmdMovesHead && !headsetDragsBody && RUISDisplayManager.IsHmdPresent())
			{
				// *** OPTIHACK5 TODO Added ConvertLocation() because it was missing. Any side-effects?? RUISDevice.OpenVR
				skeleton.head.position = 
								coordinateSystem.ConvertLocation(UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head), headsetCoordinates);
				// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
			}

			// Interpolate neck pose if that option is enabled
			if(neckInterpolate && skeleton.neck.positionConfidence >= minimumConfidenceToUpdate) // skeleton.neck.positionConfidence was calculated above
			{
				skeleton.neck.position = Vector3.Lerp(skeleton.chest.position, skeleton.head.position, neckInterpolateBlend);
				if(skeleton.chest.rotationConfidence >= minimumConfidenceToUpdate)
					skeleton.neck.rotation = skeleton.chest.rotation * Quaternion.FromToRotation(skeleton.chest.rotation * Vector3.up, 
																								 skeleton.head.position - skeleton.neck.position);
			}

			ApplyInertialSuitCorrections(); // Updates all skeleton.<joint> rotations and positions if yawCorrectIMU (or headsetDragsBody) is enabled

			if(skeleton.isTracking || !hasBeenTracked || headsetDragsBody)
				UpdateSkeletonPosition(); // Updates skeletonPosition variable

			// *** OPTIHACK6 check that this new location for this code still works
			if(hmdRotatesHead && RUISDisplayManager.IsHmdPresent())
			{
				// *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
				skeleton.head.rotation = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
																														headsetCoordinates);
			}
				
			if(skeleton.isTracking || !hasBeenTracked  /* && bodyTrackingDeviceID != RUISSkeletonManager.kinect2SensorID || skeletonManager.isNewKinect2Frame */)
			{
				ApplyTranslationOffsets(); // *** OPTIHACK5 TODO skeleton.torso.position and rotation values are not used if skeleton.isTracking is false!!!!

				UpdateTransform(ref torso, skeleton.torso, maxAngularChange, pelvisRotationOffset);
				UpdateTransform(ref chest, skeleton.chest, maxAngularChange, chestRotationOffset);
				UpdateTransform(ref neck,  skeleton.neck,  maxAngularChange, neckRotationOffset);
				UpdateTransform(ref head,  skeleton.head,  maxAngularChange, headRotationOffset);

				if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
				{
					UpdateTransform(ref leftClavicle, skeleton.leftClavicle, maxAngularChange, clavicleRotationOffset); 
					UpdateTransform(ref rightClavicle, skeleton.rightClavicle, maxAngularChange, clavicleRotationOffset);
				}

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
							leftFoot.localRotation  = Quaternion.Euler(feetRotationOffset) * leftKnee.localRotation;
						if(rightFoot && rightKnee)
							rightFoot.localRotation = Quaternion.Euler(feetRotationOffset) * rightKnee.localRotation;
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

			if(useHierarchicalModel && (skeleton.isTracking || !hasBeenTracked))
			{
				if(scaleHierarchicalModelBones)
				{
					UpdateBoneScalings();
					
//					torsoRotation = Quaternion.Slerp(torsoRotation, skeleton.torso.rotation, deltaTime * maxAngularVelocity);
//					torsoDirection = torsoRotation * Vector3.down;

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

						if(forceChestPosition)
							ForceUpdatePosition(ref chest, 		   skeleton.chest, 	   	   11, deltaT);
						if(forceNeckPosition || neckInterpolate)
							ForceUpdatePosition(ref neck, 		   skeleton.neck, 		    5, deltaT);
						if(forceHeadPosition || (hmdMovesHead && RUISDisplayManager.IsHmdPresent()))
							ForceUpdatePosition(ref head, 		   skeleton.head, 		    4, deltaT);
						if(forceClaviclePosition && bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
						{
							if(customRightClavicle)
								ForceUpdatePosition(ref rightClavicle, skeleton.rightClavicle, 8, deltaT);
							if(customLeftClavicle)
								ForceUpdatePosition(ref leftClavicle,  skeleton.leftClavicle,  9, deltaT);
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
				else
				{
					if(updateJointPositions)
					{
						ForceUpdatePosition(	ref torso, 		skeleton.torso, 		10, deltaTime);

						if(forceChestPosition)
							ForceUpdatePosition(ref chest, 		skeleton.chest, 		11, deltaTime);
						if(forceNeckPosition || neckInterpolate)
							ForceUpdatePosition(ref neck, 		skeleton.neck, 		 	 5, deltaTime);
						if(forceHeadPosition || (hmdMovesHead && RUISDisplayManager.IsHmdPresent()))
							ForceUpdatePosition(ref head, 		skeleton.head, 		 	 4, deltaTime);
						if(forceClaviclePosition && bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker)
						{
							if(customRightClavicle)
								ForceUpdatePosition(ref rightClavicle, 	skeleton.rightClavicle,	 8, deltaTime);
							if(customLeftClavicle)
								ForceUpdatePosition(ref leftClavicle, 	skeleton.leftClavicle, 	 9, deltaTime);
						}
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
					}

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

			if(updateRootPosition && (skeleton.isTracking || !hasBeenTracked))
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

//		tempVector = transform.TransformPoint(skeleton.leftHand.position - skeleton.leftElbow.position);
//		Debug.DrawLine(leftElbow.position, leftElbow.position + tempVector);
//		Debug.DrawLine(leftElbow.position + 0.01f * Vector3.up, leftHand.position + 0.01f * Vector3.up, Color.magenta);

//		tempVector = transform.TransformPoint(skeleton.leftHand.rotation * (-Vector3.right));
//		Debug.DrawLine(leftElbow.position, leftElbow.position + tempVector);
//		Debug.DrawLine(leftElbow.position + 0.01f * Vector3.up, leftElbow.position - leftHand.rotation * Vector3.right + 0.01f * Vector3.up, Color.magenta);
		
//		if(characterController)
//		{
//			// If character controller pivot is PS Move
//			if(followMoveController && inputManager)
//			{
//				psmove = inputManager.GetMoveWand(followMoveID);
//				if(psmove)
//				{
//					float moveYaw = psmove.localRotation.eulerAngles.y;
//					trackedDeviceYawRotation = Quaternion.Euler(0, moveYaw, 0);
//
//					if(!skeletonManager || !skeleton.isTracking)
//					{
//						tempVector = psmove.localPosition - trackedDeviceYawRotation * characterController.psmoveOffset;
//						skeletonPosition.x = tempVector.x;
//						skeletonPosition.z = tempVector.z;
//
//						if(characterController.headRotatesBody)
//							tempRotation = UpdateTransformWithTrackedDevice(ref root, moveYaw);
//						else 
//							tempRotation = Quaternion.identity;
//
//						if(updateRootPosition)
//							transform.localPosition = skeletonPosition + tempRotation*rootOffset;
//					}
//				}
//			}
//
//			if(followHmdPosition)
//			{
//				float hmdYaw = 0;
//				if(coordinateSystem)
//				{
//					if(coordinateSystem.applyToRootCoordinates)
//					{
//						// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
//						tempVector = coordinateSystem.ConvertLocation(coordinateSystem.GetHmdRawPosition(), headsetCoordinates);
//						skeletonPosition.x = tempVector.x;
//						skeletonPosition.z = tempVector.z;
//						// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
//						hmdYaw = coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), headsetCoordinates).eulerAngles.y;
//					}
//					else
//					{
//						tempVector = coordinateSystem.GetHmdRawPosition();
//						skeletonPosition.x = tempVector.x;
//						skeletonPosition.z = tempVector.z;
//						hmdYaw = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head).eulerAngles.y;
//					}
//				}
//
//				trackedDeviceYawRotation = Quaternion.Euler(0, hmdYaw, 0);
//
//				if(characterController.headRotatesBody)
//					tempRotation = UpdateTransformWithTrackedDevice(ref root, hmdYaw);
//				else
//					tempRotation = Quaternion.identity;
//					
//				if(updateRootPosition) 
//					transform.localPosition = skeletonPosition + tempRotation*rootOffset;	
//			}
//		}
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
		else
		{
			jointToSet.positionConfidence = 0;
			jointToSet.rotationConfidence = 0;
		}
	}

	private Quaternion previousRotation, newRotation, rotationOffset;

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
					switch(armBoneLengthAxis)
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
					switch(legBoneLengthAxis)
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

		rotationOffset = Quaternion.Euler(rotOffset);

		if(updateJointRotations && (jointToGet.rotationConfidence >= minimumConfidenceToUpdate || !hasBeenTracked))
		{
			if(useHierarchicalModel)
			{
				if(useParentRotation && transformToUpdate.parent)
					newRotation = transformToUpdate.parent.rotation * rotationOffset;
				else
					newRotation = transform.rotation * jointToGet.rotation * rotationOffset *
					                   (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
				
				intendedRotations[jointToGet.jointID] = Quaternion.Inverse(transformToUpdate.parent.rotation) * newRotation; // *** OPTIHACK6 don't use .parent

				if(!scaleHierarchicalModelBones || !limbsAreScaled || !scaleBoneLengthOnly)
					transformToUpdate.rotation = Quaternion.RotateTowards(transformToUpdate.rotation, newRotation, maxAngularChange); // *** original
				else
				{
					previousRotation = transformToUpdate.rotation;
					transformToUpdate.rotation = 
						Quaternion.RotateTowards(previousRotation, ScaleCorrectedRotation(ref transformToUpdate, jointToGet, newRotation), maxAngularChange);
				}
			}
			else
			{	// *** OPTIHACK4  check that this works and rotationOffset multiplication belongs to right side
				if(useParentRotation && transformToUpdate.parent)
					transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation, transformToUpdate.parent.rotation * rotationOffset, maxAngularChange);
				else
					transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation, jointToGet.rotation * rotationOffset, maxAngularChange);
			}
		}
	}

//	Transform childTransform;
//	RUISSkeletonManager.JointData childJoint;

	private Quaternion ScaleCorrectedRotation(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet, Quaternion newRotation)
	{
		bool isEndBone = false;
		switch(jointToGet.jointID)
		{
			case RUISSkeletonManager.Joint.LeftElbow:
//				childTransform = leftHand;
//				childJoint = skeleton.leftHand;
				break;
			case RUISSkeletonManager.Joint.LeftHand:
//				childTransform = null;
//				childJoint = skeleton.leftHand;
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.LeftKnee:
//				childTransform = leftFoot;
//				childJoint = skeleton.leftFoot;
				break;
			case RUISSkeletonManager.Joint.LeftFoot:
//				childTransform = null;
//				childJoint = skeleton.leftFoot;
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.RightElbow:
//				childTransform = rightHand;
//				childJoint = skeleton.rightHand;
				break;
			case RUISSkeletonManager.Joint.RightHand:
//				childTransform = null;
//				childJoint = skeleton.rightHand;
				isEndBone = true;
				break;
			case RUISSkeletonManager.Joint.RightKnee:
//				childTransform = rightFoot;
//				childJoint = skeleton.rightFoot;
				break;
			case RUISSkeletonManager.Joint.RightFoot:
//				childTransform = null;
//				childJoint = skeleton.rightFoot;
				isEndBone = true;
				break;
			default: return newRotation;
		}
		
//		// Inverted localScale
//		tempVector.Set(1/transformToUpdate.parent.localScale.x, 1/transformToUpdate.parent.localScale.y, 1/transformToUpdate.parent.localScale.z);
//		
//		// Calculate intended child bone direction in the transformToUpdate frame, and scale it with the inverted localScale: the result is the scale-corrected 
//		// localRotation direction
//		tempVector = Vector3.Scale(tempVector, 
//								   Quaternion.Inverse(transformToUpdate.parent.rotation) * transform.TransformPoint(childJoint.position - jointToGet.position));
//		
//		// Calculate the rotation difference between the localRotation direction and its scale-corrected version, and use that to multiply newRotation
//		return Quaternion.FromToRotation(transform.TransformPoint(childJoint.position - jointToGet.position), 
//										 transformToUpdate.parent.rotation * tempVector						 ) * newRotation;

		// Below results in flattened lower arm when elbow is rotated by (0, 90, 270), but above is not perfect either
		// Solution: in bone scaling, set localScales using calculated angles instead of transform angles
//		Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(transformToUpdate.parent.rotation) * newRotation, Vector3.one);
//		if(jointToGet.jointID == RUISSkeletonManager.Joint.LeftElbow)
//		{
//			tempVector = transformToUpdate.parent.localScale;
//			Debug.DrawLine(leftElbow.position, leftElbow.position 
//												+ transformToUpdate.parent.rotation * Vector3.Scale(tempVector, rotationMatrix.GetColumn(2)), Color.blue);
//			Debug.DrawLine(leftElbow.position, leftElbow.position 
//												+ transformToUpdate.parent.rotation * Vector3.Scale(tempVector, rotationMatrix.GetColumn(1)), Color.green);
//			Debug.DrawLine(leftElbow.position, leftElbow.position 
//												+ transformToUpdate.parent.rotation * Vector3.Scale(tempVector, rotationMatrix.GetColumn(0)), Color.red);
//		}

		// *** OPTIHACK7 the below scale correction does not work perfectly if parent has extreme non-uniform scaling (regardless if parent/child is 
		//				upper/lower arm or lower arm/hand): the rotations get "magnified" in some directions still
		if(isEndBone) // *** OPTIHACK6 don't use .parent and .parent.parent
			return transformToUpdate.parent.rotation * GetScaleCorrectedRotation(Quaternion.Inverse(transformToUpdate.parent.rotation) 
					* transformToUpdate.parent.parent.rotation * GetScaleCorrectedRotation(Quaternion.Inverse(transformToUpdate.parent.parent.rotation) 
						* newRotation, transformToUpdate.parent.parent.localScale), transformToUpdate.parent.localScale);
		else
			return transformToUpdate.parent.rotation 
					* GetScaleCorrectedRotation(Quaternion.Inverse(transformToUpdate.parent.rotation) * newRotation, transformToUpdate.parent.localScale);
	}

	private Quaternion GetScaleCorrectedRotation(Quaternion rotation, Vector3 parentScale)
	{
		// Stopped using Matrix4x4.TRS(), it gives errors: "Quaternion To Matrix conversion failed because input Quaternion is invalid"
//		Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
//		tempVector = parentScale;
//		rotationMatrix.SetRow(0, new Vector4(rotationMatrix.m00 * tempVector.x, rotationMatrix.m01 * tempVector.x, rotationMatrix.m02 * tempVector.x, 0));
//		rotationMatrix.SetRow(1, new Vector4(rotationMatrix.m10 * tempVector.y, rotationMatrix.m11 * tempVector.y, rotationMatrix.m12 * tempVector.y, 0));
//		rotationMatrix.SetRow(2, new Vector4(rotationMatrix.m20 * tempVector.z, rotationMatrix.m21 * tempVector.z, rotationMatrix.m22 * tempVector.z, 0)); 
//		rotationMatrix.SetRow(3, Vector4.zero);
//		return Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));

		return Quaternion.LookRotation(Vector3.Scale(rotation * Vector3.forward, parentScale), Vector3.Scale(rotation * Vector3.up, parentScale));
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
				newRotation = transform.rotation * yaw *
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

				fourJointsKalman[jointID].SetR(deltaT * fourJointsNoiseCovariance);
				fourJointsKalman[jointID].Predict();
				fourJointsKalman[jointID].Update(measuredPos);
				pos = fourJointsKalman[jointID].GetState();

				forcedJointPositions[jointID].Set((float)pos[0], (float)pos[1], (float)pos[2]);
			}
			else
				forcedJointPositions[jointID] = jointToGet.position;
			
			// return; invocation is here as well so that the above Kalman filter will be updated even with "bad" confidence values
			if(jointToGet.positionConfidence < minimumConfidenceToUpdate && hasBeenTracked)
				return;

			// *** OPTIHACK5 remove these and the commented block below
			jointOffset = Vector3.zero;
			offsetScale = 0;

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
			// *** OPTIHACK7 commented if-else here, any adverse effects on Kinect1/2 or other avatar besides muscle dude where torso == root?
//			if(jointToGet.jointID == RUISSkeletonManager.Joint.Torso && torso == root)
//				transformToUpdate.position = transform.TransformPoint( torsoOffset);
//			else
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

		// *** OPTIHACK6 Consider removing this if-else and apply head offset normally like other joints?
		//     Currently if hmdMovesHead is enabled, its offset is applied along HMD pose even if hmdRotatesHead is not enabled!
		if(hmdMovesHead && RUISDisplayManager.IsHmdPresent())
		{	
			// *** OPTIHACK TODO this isn't right!  Quaternion.Inverse( coordinateSystem.ConvertRotation(...
			jointOffset = skeleton.neck.rotation * headOffset + coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
																								 headsetCoordinates) * hmdLocalOffset; 
			// *** OPTIHACK5 CustomHMDSource and coordinate conversion case...
		}
		else
		{
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

	private void ApplyInertialSuitCorrections()
	{
		if(yawCorrectIMU)
		{
			if(skeleton.torso.rotationConfidence >= minimumConfidenceToUpdate)
			{
				// *** OPTIHACK the below GetYawDriftCorrection invocations do not work if skeleton.torso.rotation was not updated with a "fresh" value
				// just before calling this function (right now this only concerns Kinect1/2 which shouldn't be used with yawCorrectIMU
				if(customHMDSource)
				{
					// GetYawDriftCorrection() sets rotationDrift and also returns it
					GetYawDriftCorrection(customHMDSource.rotation, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotation); 
				}
				else
				{
					if(RUISDisplayManager.IsHmdPresent()) // *** OPTIHACK5 TODO CustomHMDSource and coordinate conversion case...
						GetYawDriftCorrection(coordinateSystem.ConvertRotation(UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head), 
																headsetCoordinates), skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotation);
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

			// Assign tempVector with skeleton.head.position that has been pivoted around skeleton.root.position by the angle of rotationDrift
			tempVector = skeleton.head.position;
			RotatePositionAroundPivot(ref tempVector, skeleton.root.position, rotationDrift, Vector3.zero);
			headToHeadsetVector = headsetPosition - tempVector + headsetRotation * hmdLocalOffset;
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

//		#if UNITY_EDITOR
//		Debug.DrawRay(skeleton.head.position, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotation * Vector3.forward, Color.blue);
//		Debug.DrawRay(skeleton.head.position, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotation * Vector3.up, Color.green);
//		Debug.DrawRay(skeleton.head.position, skeletonManager.skeletons[BodyTrackingDeviceID, playerId].head.rotation * Vector3.right, Color.red);
//		Debug.DrawRay(skeleton.head.position, skeleton.head.rotation * Vector3.forward, Color.cyan);
//		Debug.DrawRay(skeleton.head.position, skeleton.head.rotation * Vector3.up, Color.yellow);
//		Debug.DrawRay(skeleton.head.position, skeleton.head.rotation * Vector3.right, Color.magenta);
//		#endif
	}

	// Gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
	private void UpdateSkeletonPosition()
	{
		if(filterPosition && !filterHeadPositionOnly)
		{
			newRootPosition = skeleton.root.position;

			measuredPos[0] = newRootPosition.x;
			measuredPos[1] = newRootPosition.y;
			measuredPos[2] = newRootPosition.z;
			positionKalman.SetR(deltaTime * positionNoiseCovariance); // HACK doesn't take into account Kinect's own update deltaT
			positionKalman.Predict();
			positionKalman.Update(measuredPos);
			pos = positionKalman.GetState();

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
	private float clavicleParentScale = 1;
	private float torsoMultiplier = 1;
//	private Vector3 elementScale = Vector3.one;

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
				clavicleParentScale = cumulativeScale;

			cumulativeScale = UpdateUniformBoneScaling(neck, head, skeleton.neck, skeleton.head, torsoMultiplier * neckScaleAdjust, cumulativeScale);
			if(neckParentsShoulders)
				clavicleParentScale = cumulativeScale;

			if(head && cumulativeScale != 0) // cumulativeScale contains the accumulated scale of head's parent
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
													  torsoMultiplier * clavicleScaleAdjust, clavicleParentScale);

		cumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeleton.rightShoulder, skeleton.rightElbow,  limbStartScale);
		cumulativeScale = UpdateBoneScaling(rightElbow,    rightHand,  skeleton.rightElbow,    skeleton.rightHand,  cumulativeScale);
		UpdateEndBoneScaling(rightHand, rightHandScaleAdjust * Vector3.one, skeleton.rightHand, prevRightHandDelta, cumulativeScale);

//		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker) // *** OPTIHACK4 commented this
			limbStartScale = UpdateUniformBoneScaling(leftClavicle, leftShoulder, skeleton.leftClavicle, skeleton.leftShoulder, 
													  torsoMultiplier * clavicleScaleAdjust, clavicleParentScale);

		cumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeleton.leftShoulder, skeleton.leftElbow,  limbStartScale);
		cumulativeScale = UpdateBoneScaling(leftElbow,    leftHand,  skeleton.leftElbow,    skeleton.leftHand,  cumulativeScale);
		UpdateEndBoneScaling(leftHand, leftHandScaleAdjust * Vector3.one, skeleton.leftHand, prevLeftHandDelta, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(rightHip,  rightKnee, skeleton.rightHip,  skeleton.rightKnee, cumulatedPelvisScale);
		cumulativeScale = UpdateBoneScaling(rightKnee, rightFoot, skeleton.rightKnee, skeleton.rightFoot,      cumulativeScale);
		UpdateEndBoneScaling(rightFoot, rightFootScaleAdjust * Vector3.one, skeleton.rightFoot, prevRightFootDelta, cumulativeScale);

		cumulativeScale = UpdateBoneScaling(leftHip,  leftKnee, skeleton.leftHip,  skeleton.leftKnee, cumulatedPelvisScale);
		cumulativeScale = UpdateBoneScaling(leftKnee, leftFoot, skeleton.leftKnee, skeleton.leftFoot,      cumulativeScale);
		UpdateEndBoneScaling(leftFoot, leftFootScaleAdjust * Vector3.one, skeleton.leftFoot, prevLeftFootDelta, cumulativeScale);

	}

	private float UpdateUniformBoneScaling(Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, 
		                                   RUISSkeletonManager.JointData comparisonBoneTracker, float adjustScale, float accumulatedScale		)
	{
		if(!boneToScale)
			return accumulatedScale;
			
		float newScale = 1;
		float modelBoneLength = 1;
		float extremityTweaker = 1;
		float playerBoneLength = 1;
		float initialBoneScale = 1;
		bool isScaledBone = true;

		// Below switch statement assumes that UpdateUniformBoneScaling() is only used for torso segments!
		switch(torsoBoneLengthAxis)
		{
			case RUISAxis.X: initialBoneScale = jointInitialLocalScales[boneToScale].x; break;
			case RUISAxis.Y: initialBoneScale = jointInitialLocalScales[boneToScale].y; break;
			case RUISAxis.Z: initialBoneScale = jointInitialLocalScales[boneToScale].z; break;
		}

		switch(boneToScaleTracker.jointID)
		{
//			case RUISSkeletonManager.Joint.LeftKnee:   extremityTweaker = shinLengthRatio; break;
//			case RUISSkeletonManager.Joint.RightKnee:  extremityTweaker = shinLengthRatio; break;
//			case RUISSkeletonManager.Joint.LeftElbow:  extremityTweaker = forearmLengthRatio; break;
//			case RUISSkeletonManager.Joint.RightElbow: extremityTweaker = forearmLengthRatio; break;
			case RUISSkeletonManager.Joint.Torso:			isScaledBone = independentTorsoSegmentsScaling; break;
			case RUISSkeletonManager.Joint.Chest:			isScaledBone = independentTorsoSegmentsScaling; break;
			case RUISSkeletonManager.Joint.Neck:			isScaledBone = independentTorsoSegmentsScaling && scalingNeck; break;
			case RUISSkeletonManager.Joint.LeftClavicle:	isScaledBone = independentTorsoSegmentsScaling && scalingClavicles; break;
			case RUISSkeletonManager.Joint.RightClavicle:	isScaledBone = independentTorsoSegmentsScaling && scalingClavicles; break;
		}

		if(isScaledBone)
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
				{
					newScale = playerBoneLength / modelBoneLength;
					boneToScaleTracker.boneScale = Mathf.MoveTowards(boneToScaleTracker.boneScale, newScale, maxScaleFactor * deltaTime / initialBoneScale); //
				}
			}
		}

		boneToScale.localScale = adjustScale * extremityTweaker * (boneToScaleTracker.boneScale / accumulatedScale) * jointInitialLocalScales[boneToScale];
//		boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, adjustScale * extremityTweaker * (newScale / accumulatedScale) * Vector3.one,
//			 										 maxScaleFactor * deltaTime);

		switch(torsoBoneLengthAxis)
		{
			case RUISAxis.X: return accumulatedScale * boneToScale.localScale.x / jointInitialLocalScales[boneToScale].x;
			case RUISAxis.Y: return accumulatedScale * boneToScale.localScale.y / jointInitialLocalScales[boneToScale].y;
			case RUISAxis.Z: return accumulatedScale * boneToScale.localScale.z / jointInitialLocalScales[boneToScale].z;
		}
		return accumulatedScale * boneToScale.localScale.x / jointInitialLocalScales[boneToScale].x;
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
		float parentBoneThickness = 1;
		float extremityTweaker = 1;
		float skewedScaleTweak = 1;
		float thicknessU = 1;
		float thicknessV = 1;
		float initialBoneScale = 1;
		bool isLimbStart = false;
		bool isLimbMiddle = false;
		bool isLimbEnd = false;
		Vector3 previousScale = Vector3.one;
		RUISAxis lengthAxis = armBoneLengthAxis;
//		Vector3 avatarBoneVector;

		switch(lengthAxis)
		{
			case RUISAxis.X: initialBoneScale = jointInitialLocalScales[boneToScale].x; break;
			case RUISAxis.Y: initialBoneScale = jointInitialLocalScales[boneToScale].y; break;
			case RUISAxis.Z: initialBoneScale = jointInitialLocalScales[boneToScale].z; break;
		}
			
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
			{
				newScale = playerBoneLength / modelBoneLength;
				boneToScaleTracker.boneScale = Mathf.MoveTowards(boneToScaleTracker.boneScale, newScale, maxScaleFactor * deltaTime / initialBoneScale);
			}

//			avatarBoneVector = boneToScale.position - comparisonBone.position;
		}
//		else
//		{
//			switch(boneLengthAxis) // *** OPTIHACK4 check that works
//			{
//				case RUISAxis.X: avatarBoneVector = boneToScale.rotation * Vector3.right; 	break;
//				case RUISAxis.Y: avatarBoneVector = boneToScale.rotation * Vector3.up;		break;
//				case RUISAxis.Z: avatarBoneVector = boneToScale.rotation * Vector3.forward; break;
//				default: avatarBoneVector = boneToScale.rotation * Vector3.forward; break;
//			}
//		}
		
		switch(boneToScaleTracker.jointID)
		{
			case RUISSkeletonManager.Joint.LeftHip:
				thickness = leftLegThickness * leftThighThickness;
				thicknessU = leftLegThickness * leftThighThickness;
				thicknessV = leftLegThickness * leftThighThickness;
				previousScale = prevLeftHipScale;
				isLimbStart = true;
				lengthAxis = legBoneLengthAxis;
				break;
			case RUISSkeletonManager.Joint.RightHip:
				thickness = rightLegThickness * rightThighThickness;
				thicknessU = rightLegThickness * rightThighThickness;
				thicknessV = rightLegThickness * rightThighThickness;
				previousScale = prevRightHipScale;
				isLimbStart = true;
				lengthAxis = legBoneLengthAxis;
				break;
			case RUISSkeletonManager.Joint.LeftShoulder:
				thickness = leftArmThickness * leftUpperArmThickness;
				thicknessU = leftArmThickness * leftUpperArmThickness;
				thicknessV = leftArmThickness * leftUpperArmThickness;
				previousScale = prevLeftShoulderScale;
				isLimbStart = true;
				break;
			case RUISSkeletonManager.Joint.RightShoulder:
				thickness = rightArmThickness * rightUpperArmThickness;
				thicknessU = rightArmThickness * rightUpperArmThickness;
				thicknessV = rightArmThickness * rightUpperArmThickness;
				previousScale = prevRightShoulderScale;
				isLimbStart = true;
				break;
			case RUISSkeletonManager.Joint.LeftKnee:
				thickness = leftLegThickness * leftShinThickness;
				thicknessU = leftLegThickness * leftShinThickness;
				thicknessV = leftLegThickness * leftShinThickness;
				parentBoneThickness = leftLegThickness * leftThighThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevLeftShinScale;
				isLimbMiddle = true;
				lengthAxis = legBoneLengthAxis;
				break;
			case RUISSkeletonManager.Joint.RightKnee:
				thickness = rightLegThickness * rightShinThickness;
				thicknessU = rightLegThickness * rightShinThickness;
				thicknessV = rightLegThickness * rightShinThickness;
				parentBoneThickness = rightLegThickness * rightThighThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevRightShinScale;
				isLimbMiddle = true;
				lengthAxis = legBoneLengthAxis;
				break;
			case RUISSkeletonManager.Joint.LeftElbow:
				thickness = leftArmThickness * leftForearmThickness;
				thicknessU = leftArmThickness * leftForearmThickness;
				thicknessV = leftArmThickness * leftForearmThickness;
				parentBoneThickness = leftArmThickness * leftUpperArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevLeftForearmScale;
				isLimbMiddle = true;
				break;
			case RUISSkeletonManager.Joint.RightElbow:
				thickness = rightArmThickness * rightForearmThickness;
				thicknessU = rightArmThickness * rightForearmThickness;
				thicknessV = rightArmThickness * rightForearmThickness;
				parentBoneThickness = rightArmThickness * rightUpperArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevRightForearmScale;
				isLimbMiddle = true;
				break;
			case RUISSkeletonManager.Joint.LeftHand:
				thickness = leftArmThickness;
				thicknessU = leftArmThickness;
				thicknessV = leftArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevLeftHandScale;
				isLimbEnd = true;
				break;
			case RUISSkeletonManager.Joint.RightHand:
				thickness = rightArmThickness;
				thicknessU = rightArmThickness;
				thicknessV = rightArmThickness;
				extremityTweaker = forearmLengthRatio;
				previousScale = prevRightHandScale;
				isLimbEnd = true;
				break;
			case RUISSkeletonManager.Joint.LeftFoot:
				thickness = leftLegThickness;
				thicknessU = leftLegThickness;
				thicknessV = leftLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevLeftFootScale;
				lengthAxis = legBoneLengthAxis;
				isLimbEnd = true;
				break;
			case RUISSkeletonManager.Joint.RightFoot:
				thickness = rightLegThickness;
				thicknessU = rightLegThickness;
				thicknessV = rightLegThickness;
				extremityTweaker = shinLengthRatio;
				previousScale = prevRightFootScale;
				lengthAxis = legBoneLengthAxis;
				isLimbEnd = true;
				break;
		}
		

		if(scaleBoneLengthOnly && limbsAreScaled)
		{
			if(isLimbMiddle && boneToScale.parent)
			{
				Vector3 avatarParentBone = boneToScale.parent.position - boneToScale.position; // *** TODO shouldn't use boneToScale.parent?
				Vector3 u, v, w; // *** TODO remove w
				switch(lengthAxis)
				{
					case RUISAxis.X: u = Vector3.up;      v = Vector3.forward; w = Vector3.right;   break;
					case RUISAxis.Y: u = Vector3.forward; v = Vector3.right;   w = Vector3.up;      break;
					case RUISAxis.Z: u = Vector3.right;   v = Vector3.up;      w = Vector3.forward; break;
					default: u = Vector3.up; v = Vector3.forward; w = Vector3.right; break;
				}

				// Forearm or Shin
				// Replace boneToScale.rotation with a rotation that is not scale corrected
//				skewedScaleTweak = extremityTweaker * CalculateScale(boneToScale.rotation * w, avatarParentBone, parentBoneThickness, accumulatedScale);
//				thicknessU 		 = thickness 		* CalculateScale(boneToScale.rotation * u, avatarParentBone, parentBoneThickness, accumulatedScale);
//				thicknessV 		 = thickness 		* CalculateScale(boneToScale.rotation * v, avatarParentBone, parentBoneThickness, accumulatedScale);
				// *** remove these unused code sections above

				switch(lengthAxis)
				{
					case RUISAxis.X:
						tempVector.Set(   accumulatedScale, parentBoneThickness, parentBoneThickness);
						break;
					case RUISAxis.Y:
						tempVector.Set(parentBoneThickness,    accumulatedScale, parentBoneThickness);
						break;
					case RUISAxis.Z:
						tempVector.Set(parentBoneThickness, parentBoneThickness,    accumulatedScale);
						break;
				}

				skewedScaleTweak 	= extremityTweaker 	/ Vector3.Scale(boneToScale.localRotation * w, tempVector).magnitude;
				thicknessU 			= thickness 		/ Vector3.Scale(boneToScale.localRotation * u, tempVector).magnitude;
				thicknessV 			= thickness 		/ Vector3.Scale(boneToScale.localRotation * v, tempVector).magnitude;

				// Below is a bit of a hack (average of thickness and accumulatedScale). A proper solution would have two thickness axes
//				thickness = thickness / (0.5f*(thickness + accumulatedScale) * sinAngle * sinAngle + thickness * cosAngle * cosAngle);

			}
			else
			{
				skewedScaleTweak = extremityTweaker / accumulatedScale;
				thickness  /= accumulatedScale;
				thicknessU /= accumulatedScale;
				thicknessV /= accumulatedScale;
			}

			// Scaling with jointInitialLocalScales makes other initial localScale values (in the target rig) besides 1 work
			switch(lengthAxis) // Calculating untweaked scales 
			{
				case RUISAxis.X:
					tempVector.Set(skewedScaleTweak * boneToScaleTracker.boneScale, thicknessU, thicknessV);
					break;
				case RUISAxis.Y:
					tempVector.Set(thicknessV, skewedScaleTweak * boneToScaleTracker.boneScale, thicknessU);
					break;
				case RUISAxis.Z:
					tempVector.Set(thicknessU, thicknessV, skewedScaleTweak * boneToScaleTracker.boneScale);
					break;
				/*
				case RUISAxis.X:
					boneToScale.localScale = new Vector3(skewedScaleTweak * Mathf.MoveTowards(previousScale.x, newScale, maxScaleFactor * deltaTime), thicknessU, thicknessV);
					break;
				case RUISAxis.Y:
					boneToScale.localScale = new Vector3(thicknessV, skewedScaleTweak * Mathf.MoveTowards(previousScale.y, newScale, maxScaleFactor * deltaTime), thicknessU);
					break;
				case RUISAxis.Z:
					boneToScale.localScale = new Vector3(thicknessU, thicknessV, skewedScaleTweak * Mathf.MoveTowards(previousScale.z, newScale, maxScaleFactor * deltaTime));
					break;
				*/
			}

			boneToScale.localScale = Vector3.Scale(jointInitialLocalScales[boneToScale], tempVector);

			// *** OPTIHACK8 these can probably be removed
			// Save untweaked scales
			switch(boneToScaleTracker.jointID)
			{
				case RUISSkeletonManager.Joint.LeftHip:
					prevLeftHipScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftKnee:  
					prevLeftShinScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftFoot:  
					prevLeftFootScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftShoulder:
					prevLeftShoulderScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftElbow:   
					prevLeftForearmScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.LeftHand:   
					prevLeftHandScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightShoulder:
					prevRightShoulderScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightElbow: 
					prevRightForearmScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightHand: 
					prevRightHandScale = newScale * Vector3.one;
					break;
				case RUISSkeletonManager.Joint.RightHip:
					prevRightHipScale = newScale * Vector3.one;
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
				boneToScale.localScale = extremityTweaker * (boneToScaleTracker.boneScale / accumulatedScale) * Vector3.one;
			else if(isLimbStart)
				boneToScale.localScale = extremityTweaker * (initialBoneScale / accumulatedScale) * Vector3.one;
//			if(limbsAreScaled)
//				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, extremityTweaker * 
//															 (newScale / accumulatedScale) * Vector3.one, maxScaleFactor * deltaTime);
//			else if(isLimbStart) // *** OPTIHACK8 below assumes that starting localScale is 1, which might not be the case
//				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, extremityTweaker * 
//															 (1 / accumulatedScale) * Vector3.one, maxScaleFactor * deltaTime);
		}


//			if(boneToScaleTracker.jointID == RUISSkeletonManager.Joint.RightElbow)
//				print(skewedScaleTweak + " " + thicknessU + " " + thicknessV + " " + newScale + " " + boneToScale.localScale);

		// Division by jointInitialLocalScales makes other initial localScale values (in the target rig) besides 1 work
		switch(lengthAxis)
		{
			case RUISAxis.X: return accumulatedScale * boneToScale.localScale.x / jointInitialLocalScales[boneToScale].x;
			case RUISAxis.Y: return accumulatedScale * boneToScale.localScale.y / jointInitialLocalScales[boneToScale].y;
			case RUISAxis.Z: return accumulatedScale * boneToScale.localScale.z / jointInitialLocalScales[boneToScale].z;
		}
		return accumulatedScale * boneToScale.localScale.x / jointInitialLocalScales[boneToScale].x;
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


		//
//		switch(boneLengthAxis)
//		{
//		case RUISAxis.X:
//			tempVector.Set(   accumulatedScale, parentBoneThickness, parentBoneThickness);
//			break;
//		case RUISAxis.Y:
//			tempVector.Set(parentBoneThickness,    accumulatedScale, parentBoneThickness);
//			break;
//		case RUISAxis.Z:
//			tempVector.Set(parentBoneThickness, parentBoneThickness,    accumulatedScale);
//			break;
//		}
//		skewedScaleTweak 	= extremityTweaker 	/ Vector3.Scale(boneToScale.localRotation * w, tempVector).magnitude;
//		thicknessU 			= thickness 		/ Vector3.Scale(boneToScale.localRotation * u, tempVector).magnitude;
//		thicknessV 			= thickness 		/ Vector3.Scale(boneToScale.localRotation * v, tempVector).magnitude;
		//

//		if(joint.jointID == RUISSkeletonManager.Joint.LeftHand)
//		{
//			Debug.DrawRay(boneToScale.parent.position, boneToScale.parent.localScale.z * (boneToScale.parent.parent.rotation * boneToScale.parent.localRotation * Vector3.forward), Color.blue);
//			Debug.DrawRay(boneToScale.parent.position, boneToScale.parent.localScale.y * (boneToScale.parent.parent.rotation * boneToScale.parent.localRotation * Vector3.up), Color.green);
//			Debug.DrawRay(boneToScale.parent.position, boneToScale.parent.localScale.x * (boneToScale.parent.parent.rotation * boneToScale.parent.localRotation * Vector3.right), Color.red);
//			Debug.DrawRay(boneToScale.parent.parent.position, boneToScale.parent.parent.localScale.z * (boneToScale.parent.parent.parent.rotation * boneToScale.parent.parent.localRotation * Vector3.forward), Color.blue);
//			Debug.DrawRay(boneToScale.parent.parent.position, boneToScale.parent.parent.localScale.y * (boneToScale.parent.parent.parent.rotation * boneToScale.parent.parent.localRotation * Vector3.up), Color.green);
//			Debug.DrawRay(boneToScale.parent.parent.position, boneToScale.parent.parent.localScale.x * (boneToScale.parent.parent.parent.rotation * boneToScale.parent.parent.localRotation * Vector3.right), Color.red);
//		}

// *** COMMENT BLOCK START
//		tempVector = boneToScale.parent.localScale;
//		delta = boneToScale.parent.parent.localScale;
////		tempVector.Set(1/boneToScale.parent.localScale.x, 1/boneToScale.parent.localScale.y, 1/boneToScale.parent.localScale.z);
////		delta.Set(1/boneToScale.parent.parent.localScale.x, 1/boneToScale.parent.parent.localScale.y, 1/boneToScale.parent.parent.localScale.z);
//
////		tempVector = tempVector / transform.localScale.x;
////		delta = delta / transform.localScale.x;
//
////		tempVector = (boneToScale.localRotation) * tempVector;
////		tempVector.Set(Mathf.Abs(tempVector.x), Mathf.Abs(tempVector.y), Mathf.Abs(tempVector.z));
////		tempVector.Set(1/Mathf.Abs(tempVector.x), 1/Mathf.Abs(tempVector.y), 1/Mathf.Abs(tempVector.z));
//		delta = (boneToScale.parent.localRotation) * delta;
//		delta.Set(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
////		delta.Set(1/Mathf.Abs(delta.x), 1/Mathf.Abs(delta.y), 1/Mathf.Abs(delta.z));
//
//		delta = (boneToScale.localRotation) * delta;
//		delta.Set(1/Mathf.Abs(delta.x), 1/Mathf.Abs(delta.y), 1/Mathf.Abs(delta.z));
//		delta.Set(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
//		tempVector = Vector3.Scale(delta, tempVector);
////		tempVector.Set(1/tempVector.x, 1/tempVector.y, 1/tempVector.z);
//
//		tempVector = (boneToScale.localRotation) * tempVector;
//		tempVector.Set(Mathf.Abs(tempVector.x), Mathf.Abs(tempVector.y), Mathf.Abs(tempVector.z));
//		tempVector.Set(1/tempVector.x, 1/tempVector.y, 1/tempVector.z);
////		tempVector = Vector3.Scale(delta, tempVector);
////		tempVector.Set(1/tempVector.x, 1/tempVector.y, 1/tempVector.z);
////		tempVector = Vector3.Scale(delta, tempVector);
//
//		delta.Set(	Vector3.Scale(boneToScale.parent.localRotation * Vector3.right, 	boneToScale.parent.parent.localScale).magnitude,
//					Vector3.Scale(boneToScale.parent.localRotation * Vector3.up, 		boneToScale.parent.parent.localScale).magnitude,
//					Vector3.Scale(boneToScale.parent.localRotation * Vector3.forward, 	boneToScale.parent.parent.localScale).magnitude);
//
////		delta = (boneToScale.parent.localRotation) * delta;
////		delta.Set(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
////		delta = Vector3.Scale(boneToScale.parent.localScale, delta);
//		tempVector.Set( 1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.right), 	Vector3.Scale(boneToScale.parent.localScale, delta)).magnitude,
//						1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.up), 		Vector3.Scale(boneToScale.parent.localScale, delta)).magnitude,
//						1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.forward),	Vector3.Scale(boneToScale.parent.localScale, delta)).magnitude );
////		tempVector.Set( 1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.right), 	boneToScale.parent.localScale).magnitude,
////						1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.up), 		boneToScale.parent.localScale).magnitude,
////						1 / Vector3.Scale(  (boneToScale.localRotation * Vector3.forward),	boneToScale.parent.localScale).magnitude );
//
////		delta.Set(1/Mathf.Abs(delta.x), 1/Mathf.Abs(delta.y), 1/Mathf.Abs(delta.z));
////		tempVector = Vector3.Scale(delta, tempVector);
////		delta = Quaternion.Inverse( boneToScale.parent.localRotation) * boneToScale.parent.parent.localScale;
////		delta.Set(Mathf.Abs(delta.x), Mathf.Abs(delta.y), Mathf.Abs(delta.z));
////		delta.Set(1/Mathf.Abs(delta.x), 1/Mathf.Abs(delta.y), 1/Mathf.Abs(delta.z));
////		tempVector = Vector3.Scale(delta, tempVector);
////		tempVector.Set(Mathf.Abs(tempVector.x), Mathf.Abs(tempVector.y), Mathf.Abs(tempVector.z));
////		tempVector = ( 1 / tempVector.magnitude) * tempVector;
////		boneToScale.localScale = tempVector;
// *** COMMENT BLOCK END

		tempVector = Vector3.MoveTowards(boneToScale.localScale, updatedScale, 10 * deltaTime); // *** TODO: speed 10 might not be good
		if(!float.IsNaN(tempVector.x) && !float.IsNaN(tempVector.y) && !float.IsNaN(tempVector.z))
			boneToScale.localScale = tempVector;

// *** COMMENT BLOCK START
//		if(joint.jointID == RUISSkeletonManager.Joint.LeftHand)
//		{
//
////			print(new Vector3(boneToScale.localScale.x / tempVector.x, boneToScale.localScale.y / tempVector.y, boneToScale.localScale.z / tempVector.z)
////				+ " " + boneToScale.localScale + " " + boneToScale.parent.localScale);
////			print("("+boneToScale.parent.parent.localScale.x + " " + boneToScale.parent.parent.localScale.y + " " + boneToScale.parent.parent.localScale.z + ") ("
////				+ boneToScale.parent.localScale.x + " " + boneToScale.parent.localScale.y + " " + boneToScale.parent.localScale.z + ")");
//			Debug.DrawRay(boneToScale.position, boneToScale.localScale.z * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.forward), Color.blue);
//			Debug.DrawRay(boneToScale.position, boneToScale.localScale.y * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.up), Color.green);
//			Debug.DrawRay(boneToScale.position, boneToScale.localScale.x * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.right), Color.red);
//
//			Debug.DrawRay(boneToScale.position + 0.1f*Vector3.up, tempVector.z * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.forward), Color.cyan);
//			Debug.DrawRay(boneToScale.position + 0.1f*Vector3.up, tempVector.y * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.up), Color.yellow);
//			Debug.DrawRay(boneToScale.position + 0.1f*Vector3.up, tempVector.x * (boneToScale.parent.rotation * boneToScale.localRotation * Vector3.right), Color.magenta);
//		}

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

	private int FindClosestLocalAxis(Transform pose, Vector3 globalAxis)
	{
		int axisID = 0;
		float maxDot = Mathf.Abs(Vector3.Dot(pose.right, globalAxis));
		float maxCandidate = Mathf.Abs(Vector3.Dot(pose.up, globalAxis));
		if(maxCandidate > maxDot)
		{
			axisID = 1;
			maxDot = maxCandidate;
		}
		maxCandidate = Mathf.Abs(Vector3.Dot(pose.forward, globalAxis));
		if(maxCandidate > maxDot)
			axisID = 2;
		return axisID;
	}

	private float UpdateTorsoScale()
	{
		//average hip to shoulder length and compare it to the one found in the model - scale accordingly
		//we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
		float modelLength = modelSpineLength; // The most stable metric (?): distance between (model) shoulder midpoint and (model) hip midpoint

		float playerLength = GetTrackedSpineLength(); // Should be same metric as above, but with player/user measurements
		
		float newScale = Mathf.Abs(playerLength / modelLength); // * (scaleBoneLengthOnly ? torsoThickness : 1);

		// *** HACK: Here we halve the maxScaleFactor because the torso is bigger than the limbs
		torsoScale = Mathf.MoveTowards(torsoScale, newScale, 0.5f * maxScaleFactor * deltaTime);

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

		// *** OPTIHACK7 commented below so that spineLength is same for all devices
//		if(bodyTrackingDevice == BodyTrackingDeviceType.GenericMotionTracker && customSpineJointCount > 1)
//		{
//			for(int i = 1; i < customSpineJointCount; ++i) 
//				lengthSum += Vector3.Distance(trackedSpineJoints[i - 1].position, trackedSpineJoints[i].position);
//		}
//		else
		{
			// *** OPTIHACK7 this is affected by offsets!
			// Distance between shoulders' midpoint and hips' midpoint (multiplier 0.5 taken outside), indices ([0], [1], ...) must be correct!
			lengthSum = 0.5f * Vector3.Distance((forcedJointPositions[0] + forcedJointPositions[1]),
			             						(forcedJointPositions[2] + forcedJointPositions[3]));
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
			switch(fingerBoneLengthAxis)
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

								if(yawCorrectIMU)
									tempRotation = rotationDrift;
								else
									tempRotation = Quaternion.identity;
										
								// At the moment only hierarchical finger phalanx Transforms are supported
								newRotation = transform.rotation * tempRotation * fingerSources[i, j, k].rotation * rotationOffset * initialFingerWorldRotations[i, j, k];
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

	System.Collections.IEnumerator DelayedCustomTrackingStart(float delay)
	{
		yield return new WaitForSeconds(delay);
		if(skeletonManager)
			skeletonManager.skeletons[this._bodyTrackingDeviceID, playerId].isTracking = true;
		if(skeleton != null)
			skeleton.isTracking = true;
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

		// HACK: Project forward vectors to XZ-plane. This is a problem if they are constantly parallel to Y-axis, e.g. HMD user looking directly up or down 
		driftingForward.Set(driftingForward.x, 0, driftingForward.z);
		driftlessForward.Set(driftlessForward.x, 0, driftlessForward.z);

		// *** OPTIHACK7 TODO: If either forward vector is constantly parallel to Y-axis, no drift correction occurs or it could be in the wrong direction. fix this 
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
			filterDrift.SetR(Time.deltaTime * driftNoiseCovariance);
			filterDrift.Predict();
			filterDrift.Update(measuredDrift);
			filteredDrift = filterDrift.GetState();

			tempVector.Set((float) filteredDrift[0], 0, (float) filteredDrift[1]);
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

	int GetLengthAxis(Transform startJoint, Transform endJoint)
	{
		if(startJoint && endJoint)
			return FindClosestLocalAxis(startJoint, endJoint.position - startJoint.position);
		return -1;
	}

	int CommonLengthAxis(List<KeyValuePair<RUISSkeletonManager.Joint, int>> candidateList, string bodySegmentGroupName, out string errorMessage)
	{
		errorMessage = "";
		if(candidateList != null && candidateList.Count > 0)
		{
			candidateList.RemoveAll(item => item.Value == -1);
			string axes = "";
			bool notConsistent = false;
			int lengthAxis = candidateList[0].Value;
			foreach(KeyValuePair<RUISSkeletonManager.Joint, int> candidate in candidateList)
			{
				if(candidate.Value != lengthAxis)
					notConsistent = true;
				axes += candidate.Key.ToString() + " (" + ((candidate.Value==0)?"X":((candidate.Value==1)?"Y":"Z")) + "-axis), ";
			}
			if(axes.Length > 1)
				axes = axes.Substring(0, axes.Length - 2);
//			Debug.LogError(axes);
			if(notConsistent)
				errorMessage = bodySegmentGroupName + " bones have inconsistent length axes: " + axes + ".\n";
			else
				return lengthAxis;
		}
		return -1;
	}

	void SetAndReportLengthAxes(bool errorInsteadOfWarning, bool calledInReset)
	{
		System.Action<string> reportingMethod = Debug.LogWarning;
		if(errorInsteadOfWarning)
			reportingMethod = Debug.LogError;
		int commonLengthAxis = -1;
		string consistencyError = "";
		string reactToErrorAdvice = "\"Length Only\"-option in " + typeof(RUISSkeletonController) + ". Alternatively, you can make the avatar bone "
					+ "length axes consistent by modifying their pivot orientations in a 3D animation software and re-importing the avatar to Unity.";
		if(calledInReset)
			reactToErrorAdvice = name + ": Do not enable the " + reactToErrorAdvice;
		else
			reactToErrorAdvice = name + ": Disable the " + reactToErrorAdvice;
		List<KeyValuePair<RUISSkeletonManager.Joint, int>> boneGroup = new List<KeyValuePair<RUISSkeletonManager.Joint, int>>();

		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.Torso, GetLengthAxis(torso, chest)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.Chest, GetLengthAxis(chest, neck)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.Neck, 	GetLengthAxis(neck, head)));
		commonLengthAxis = CommonLengthAxis(boneGroup, "Torso", out consistencyError);

		if(commonLengthAxis < 0)
		{
//			if(consistencyError.Length > 0)
//			{
//				reportingMethod(consistencyError + reactToErrorAdvice); // At the moment torso bone length consistency doesn't matter
//			}
		}
		else
			torsoBoneLengthAxis = (commonLengthAxis==0)?RUISAxis.X:((commonLengthAxis==1)?RUISAxis.Y:RUISAxis.Z);
		boneGroup.Clear();
		consistencyError = "";

		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightShoulder, GetLengthAxis(rightShoulder, rightElbow)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftShoulder, 	GetLengthAxis(leftShoulder, leftElbow)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightElbow, 	GetLengthAxis(rightElbow, rightHand)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftElbow, 	GetLengthAxis(leftElbow, leftHand)));
		commonLengthAxis = CommonLengthAxis(boneGroup, "Arm", out consistencyError);

		if(commonLengthAxis < 0)
		{
			if(consistencyError.Length > 0)
				reportingMethod(consistencyError + reactToErrorAdvice);
		}
		else
			armBoneLengthAxis = (commonLengthAxis==0)?RUISAxis.X:((commonLengthAxis==1)?RUISAxis.Y:RUISAxis.Z);
		boneGroup.Clear();
		consistencyError = "";

		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightHip,  GetLengthAxis(rightHip, rightKnee)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftHip, 	GetLengthAxis(leftHip, leftKnee)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightKnee, GetLengthAxis(rightKnee, rightFoot)));
		boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftKnee, 	GetLengthAxis(leftKnee, leftFoot)));
		commonLengthAxis = CommonLengthAxis(boneGroup, "Leg", out consistencyError);

		if(commonLengthAxis < 0)
		{
			if(consistencyError.Length > 0)
				reportingMethod(consistencyError + reactToErrorAdvice);
		}
		else
			legBoneLengthAxis = (commonLengthAxis==0)?RUISAxis.X:((commonLengthAxis==1)?RUISAxis.Y:RUISAxis.Z);
		boneGroup.Clear();
		consistencyError = "";

		// Finger bone axis consistency is relevant if fistMaking == true
		if(fistMaking)
		{
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightLittleFinger, GetLengthAxis(rightLittleF, fingerTargets[0, 0, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftLittleFinger, 	GetLengthAxis(leftLittleF, 	fingerTargets[1, 0, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightRingFinger,  	GetLengthAxis(rightRingF, 	fingerTargets[0, 1, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftRingFinger, 	GetLengthAxis(leftRingF, 	fingerTargets[1, 1, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightMiddleFinger, GetLengthAxis(rightMiddleF, fingerTargets[0, 2, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftMiddleFinger, 	GetLengthAxis(leftMiddleF, 	fingerTargets[1, 2, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightIndexFinger,  GetLengthAxis(rightIndexF, 	fingerTargets[0, 3, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftIndexFinger, 	GetLengthAxis(leftIndexF, 	fingerTargets[1, 3, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.RightThumb, 		GetLengthAxis(rightThumb, 	fingerTargets[0, 4, 1])));
			boneGroup.Add(new KeyValuePair<RUISSkeletonManager.Joint, int>(RUISSkeletonManager.Joint.LeftThumb, 		GetLengthAxis(leftThumb, 	fingerTargets[1, 4, 1])));
			commonLengthAxis = CommonLengthAxis(boneGroup, "Finger", out consistencyError);

			if(commonLengthAxis < 0)
			{
				if(consistencyError.Length > 0)
				{
					reactToErrorAdvice =  "\"Fist Clench Animation\"-option in \"Finger Targets\"-section of " + typeof(RUISSkeletonController)
										+ ". Alternatively, you can make the avatar finger bone length axes consistent by modifying their "
										+ "pivot orientations in a 3D animation software and re-importing the avatar to Unity.";
					if(calledInReset)
						reactToErrorAdvice = name + ": Do not enable the " + reactToErrorAdvice;
					else
						reactToErrorAdvice = name + ": Disable the " + reactToErrorAdvice;
					reportingMethod(consistencyError + reactToErrorAdvice);
				}
			}
			else
				fingerBoneLengthAxis = (commonLengthAxis==0)?RUISAxis.X:((commonLengthAxis==1)?RUISAxis.Y:RUISAxis.Z);
			boneGroup.Clear();
			consistencyError = "";
		}
	}

	public bool AutoAssignJointTargetsFromAvatar(out string shortReport, out string longReport)
	{
		shortReport = "";
		longReport  = "";
		string missedBones = "";
		Animator animator = GetComponentInChildren<Animator>();
		if(animator)
		{
			root 			= animator.GetBoneTransform(HumanBodyBones.LastBone); //
			torso 			= animator.GetBoneTransform(HumanBodyBones.Hips);
			chest 			= animator.GetBoneTransform(HumanBodyBones.Spine);
			neck 			= animator.GetBoneTransform(HumanBodyBones.Neck); //
			head 			= animator.GetBoneTransform(HumanBodyBones.Head);
			leftClavicle 	= animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
			leftShoulder 	= animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			leftElbow 		= animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			leftHand 		= animator.GetBoneTransform(HumanBodyBones.LeftHand);
			rightClavicle 	= animator.GetBoneTransform(HumanBodyBones.RightShoulder);
			rightShoulder 	= animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			rightElbow 		= animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			rightHand 		= animator.GetBoneTransform(HumanBodyBones.RightHand);
			leftHip 		= animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			leftKnee 		= animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			leftFoot 		= animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			rightHip 		= animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			rightKnee 		= animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			rightFoot 		= animator.GetBoneTransform(HumanBodyBones.RightFoot);
			leftThumb 		= animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
			leftIndexF		= animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
			leftMiddleF		= animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
			leftRingF		= animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
			leftLittleF		= animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
			rightThumb		= animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
			rightIndexF		= animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
			rightMiddleF	= animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
			rightRingF		= animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
			rightLittleF	= animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);


			if(		root && ((Transform) root).IsChildOf(animator.transform)
				&& 	animator.transform != ((Transform) root))
			{
				// Iterate until rootBone is direct child of animator.transform
				Transform child = (Transform) root;
				for(int i=0; i<100; ++i)
				{
					if(!child.parent)
						continue;
					if(child.parent == animator.transform)
					{
						root = child;
						break;
					}
					if(i==99)
					{
						root = null;
						break;
					}
					child = child.parent;
				}
			}

			if(!neck && head)
				neck = ((Transform) head).parent;

			if(!root)
				missedBones += "Root, ";
			if(!torso)
				missedBones += "Pelvis, ";
			if(!chest)
				missedBones += "Chest, ";
			if(!neck)
				missedBones += "Neck, ";
			if(!head)
				missedBones += "Head, ";
			if(!leftClavicle)
				missedBones += "Left Clavicle, ";
			if(!leftShoulder)
				missedBones += "Left Shoulder, ";
			if(!leftElbow)
				missedBones += "Left Elbow, ";
			if(!leftHand)
				missedBones += "Left Hand, ";
			if(!rightClavicle)
				missedBones += "Right Clavicle, ";
			if(!rightShoulder)
				missedBones += "Right Shoulder, ";
			if(!rightElbow)
				missedBones += "Right Elbow, ";
			if(!rightHand)
				missedBones += "Right Hand, ";
			if(!leftHip)
				missedBones += "Left Hip, ";
			if(!leftKnee)
				missedBones += "Left Knee, ";
			if(!leftFoot)
				missedBones += "Left Foot, ";
			if(!leftThumb)
				missedBones += "Left Thumb, ";
			if(!leftIndexF)
				missedBones += "Left Index Finger CMC, ";
			if(!leftMiddleF)
				missedBones += "Left Middle Finger CMC, ";
			if(!leftRingF)
				missedBones += "Left Ring Finger CMC, ";
			if(!leftLittleF)
				missedBones += "Left Little Finger CMC, ";
			if(!rightThumb)
				missedBones += "Right Thumb, ";
			if(!rightIndexF)
				missedBones += "Right Index Finger CMC, ";
			if(!rightMiddleF)
				missedBones += "Right Middle Finger CMC, ";
			if(!rightRingF)
				missedBones += "Right Ring Finger CMC, ";
			if(!rightLittleF)
				missedBones += "Right Little Finger CMC, ";

			if(!string.IsNullOrEmpty(missedBones))
			{
				if(missedBones.Length > 1)
					missedBones = missedBones.Substring(0, missedBones.Length - 2);
				longReport =  typeof(RUISSkeletonController) + " obtained some Avatar Target Transforms from Animator component of '"  
							+ animator.gameObject.name + "' GameObject. The following Transforms were NOT obtained: " + missedBones
							+ ". Please check that the automatically obtained Transforms correspond to the Target labels by "
							+ "clicking the Target Transform fields. All previously assigned Target Transforms were replaced.";

				shortReport =   "Obtained some Target Transforms but not all (see Console for details). Please check that they "
							  + "correspond to the Target labels by clicking the below Target Transform fields.";
			}
			else
			{
				longReport =  typeof(RUISSkeletonController) + " obtained all Avatar Target Transforms from Animator component of '"
							+ animator.gameObject.name + "' GameObject. Please check that the automatically obtained Transforms "
							+ "correspond to the Target labels by clicking the Target Transform fields.";

				shortReport =   "Obtained all Target Transforms. Please check that they correspond to the Target labels by "
							  + "clicking the below Target Transform fields.";
			}
			return true;
		}
		else
		{
			longReport =  typeof(RUISSkeletonController) + " failed to obtain Avatar Target Transforms: Could not find an Animator " 
						+ "component in " + name + " or its children. You must assign the Target Transforms manually.";
			shortReport = "Failed to obtain Avatar Target Transforms";
		}
		return false;
	}

	public float pelvisDepthMult 	= 2.0f;
	public float pelvisWidthMult 	= 1.2f;
	public float pelvisLengthMult 	= 1.0f;
	public float chestDepthMult 	= 2.0f;
	public float chestWidthMult 	= 1.0f;
	public float chestLengthMult 	= 1.0f;
	public float neckRadiusMult 	= 1.0f;
	public float headDepthMult 		= 2.0f;
	public float headWidthMult 		= 1.5f;
	public float headLengthMult 	= 1.0f;
	public float shoulderRadiusMult = 1.1f;
	public float elbowRadiusMult 	= 1.0f;
	public float handDepthMult 		= 0.5f;
	public float handWidthMult 		= 1.0f;
	public float handLengthMult 	= 1.0f;
	public float fingerRadiusMult   = 1.0f;
	public float fingerLengthMult   = 1.2f;
	public float fingerTaperValue   = 0.9f;
	public float thumbRadiusMult    = 1.5f;
	public float thumbLengthMult    = 1.2f;
	public float thumbTaperValue    = 0.7f;
	public float thighRadiusMult 	= 1.5f;
	public float shinRadiusMult 	= 1.0f;
	public float footDepthMult 		= 0.7f;
	public float footWidthMult 		= 1.2f;
	public float footLengthMult 	= 1.0f;

	/// <summary>
	/// Adds colliders to body segments in Edit or Play Mode. The avatar should be in T- or A-pose.
	/// </summary>
	/// <param name="colliderType">Collider type</param>
	/// <param name="span">Body segment radius (meters)</param>
	/// <param name="lengthOffset">Length offset (meters) that extends Neck, Upper Arm, Forearm, Thigh, and Shin Colliders.</param>
	/// <param name="fingerColliders">Create finger colliders if true</param>
	/// <param name="pelvisHasBox">If true, colliderType is overriden, and Pelvis will have Box Collider. Default value is false.</param>
	/// <param name="chestHasBox">If true, colliderType is overriden, and Chest will have Box Collider. Default value is false.</param>
	/// <param name="headHasBox">If true, colliderType is overriden, and Head will have Box Collider. Default value is false.</param>
	/// <param name="handHasBox">If true, colliderType is overriden, and Hands will have Box Colliders. Default value is false.</param>
	/// <param name="footHasBox">If true, colliderType is overriden, and Feet will have Box Colliders. Default value is false.</param>
	public void AddCollidersToBodySegments( AvatarColliderType colliderType, float span, float lengthOffset, bool fingerColliders, 
											bool pelvisHasBox = default(bool), bool chestHasBox = default(bool), bool headHasBox = default(bool), 
											bool handHasBox = default(bool), bool footHasBox = default(bool), bool fingerHasBox = default(bool)   )
	{
		if(!torso || !head || !leftShoulder || !rightShoulder)
		{
			string missingBones = "";
			if(!torso)
				missingBones += "Pelvis, ";
			if(!head)
				missingBones += "Head, ";
			if(!leftShoulder)
				missingBones += "Left Shoulder, ";
			if(!rightShoulder)
				missingBones += "Right Shoulder, ";
			if(missingBones.Length > 2)
				missingBones = missingBones.Substring(0, missingBones.Length - 2);
			Debug.LogError(   "Failed to add Colliders to body segments, because the following "
							+ "\"Avatar Target Transforms\" are not assigned: " + missingBones + ".");
			return;
		}
		AvatarColliderType pelvisType = pelvisHasBox? AvatarColliderType.BoxCollider : colliderType;
		AvatarColliderType chestType  =  chestHasBox? AvatarColliderType.BoxCollider : colliderType;
		AvatarColliderType headType   =   headHasBox? AvatarColliderType.BoxCollider : colliderType;
		AvatarColliderType handType   =   handHasBox? AvatarColliderType.BoxCollider : colliderType;
		AvatarColliderType footType   =   footHasBox? AvatarColliderType.BoxCollider : colliderType;
		AvatarColliderType fingerType = fingerHasBox? AvatarColliderType.BoxCollider : colliderType;

		Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
		Vector3 depthAxis = Vector3.Cross(head.position - torso.position, rightShoulder.position - leftShoulder.position);

		float shoulderWidth = Vector3.Distance(rightShoulder.position, leftShoulder.position);
		if(chest)
		{
			shoulderWidth = chest.transform.TransformVector(Vector3.Scale(
								chest.transform.InverseTransformVector(rightShoulder.position - leftShoulder.position), 
								new Vector3(1/chest.transform.lossyScale.x, 1/chest.transform.lossyScale.y, 1/chest.transform.lossyScale.z))).magnitude;
		}
		Vector3 handAxis, footAxis, headAxis;
		Vector3 shoulderAxis = rightShoulder.position - leftShoulder.position;

		float pelvisWidth = 0.8f * shoulderWidth;
		if(leftHip && rightHip)
		{
			pelvisWidth = Vector3.Distance(rightHip.position, leftHip.position);
			if(torso)
			{
				pelvisWidth = torso.transform.TransformVector(Vector3.Scale(torso.transform.InverseTransformVector(rightHip.position - leftHip.position), 
								new Vector3(1/torso.transform.lossyScale.x, 1/torso.transform.lossyScale.y, 1/torso.transform.lossyScale.z))).magnitude;
			}
				
		}

		CreateCollider(torso, 		chest,   pelvisType, depthAxis, 0.5f * pelvisWidthMult *  pelvisWidth, pelvisDepthMult * span, 0, pelvisLengthMult);
		CreateCollider(chest, 		 neck,    chestType, depthAxis, 0.5f * chestWidthMult * shoulderWidth,  chestDepthMult * span, 0, chestLengthMult );
		CreateCollider(neck, 		 head, colliderType, depthAxis, 		   neckRadiusMult * span, 	    neckRadiusMult * span, lengthOffset, 1    );

		CreateCollider(rightShoulder, rightElbow, colliderType, depthAxis, shoulderRadiusMult * span, shoulderRadiusMult * span, lengthOffset, 1);
		CreateCollider(rightElbow,     rightHand, colliderType, depthAxis,    elbowRadiusMult * span,    elbowRadiusMult * span, lengthOffset, 1);
		CreateCollider(leftShoulder,   leftElbow, colliderType, depthAxis, shoulderRadiusMult * span, shoulderRadiusMult * span, lengthOffset, 1);
		CreateCollider(leftElbow, 		leftHand, colliderType, depthAxis,    elbowRadiusMult * span,    elbowRadiusMult * span, lengthOffset, 1);

		CreateCollider(rightHip,  	   rightKnee, colliderType, depthAxis,    thighRadiusMult * span,    thighRadiusMult * span, lengthOffset, 1);
		CreateCollider(rightKnee, 	   rightFoot, colliderType, depthAxis,     shinRadiusMult * span,     shinRadiusMult * span, lengthOffset, 1);
		CreateCollider(leftHip,   		leftKnee, colliderType, depthAxis,    thighRadiusMult * span,    thighRadiusMult * span, lengthOffset, 1);
		CreateCollider(leftKnee,  		leftFoot, colliderType, depthAxis,     shinRadiusMult * span,     shinRadiusMult * span, lengthOffset, 1);

		if(neck)
			headAxis = 0.3f * (head.position - torso.position).magnitude * (head.position - neck.position).normalized;
		else
			headAxis = 0.3f * (head.position - torso.position);
		CreateCollider(head, null, headType, depthAxis, headWidthMult * span, headDepthMult * span, 0, headLengthMult, headAxis);


		if(rightFoot && rightFoot.childCount > 0)
			footAxis = (rightFoot.GetChild(0).position - rightFoot.position);
		else // If foot doesn't have child transforms, optional lengthDirection will get direction from depthAxis and length from 0.5 * shoulderWidth
			footAxis = 0.5f * shoulderWidth * depthAxis.normalized;
		CreateCollider(rightFoot, null,	footType, Vector3.Cross(footAxis, shoulderAxis), footWidthMult * span, footDepthMult * span, 0, footLengthMult, footAxis);

		if(leftFoot && leftFoot.childCount > 0)
			footAxis =  (leftFoot.GetChild(0).position  - leftFoot.position);
		else
			footAxis = 0.5f * shoulderWidth * depthAxis.normalized;
		CreateCollider(leftFoot,  null, footType, Vector3.Cross(footAxis, shoulderAxis), footWidthMult * span, footDepthMult * span, 0, footLengthMult, footAxis);


		handAxis = 0.1f * Vector3.right; // Default right hand length axis
		if(rightHand && rightElbow)
		{
			if(rightMiddleF)
				handAxis = (rightMiddleF.position - rightHand.position).magnitude * (rightHand.position - rightElbow.position).normalized;
			else
				handAxis = 0.3f * (rightHand.position - rightElbow.position);
			CreateCollider(rightHand, null, handType, Vector3.Cross(handAxis, depthAxis), handWidthMult * span, handDepthMult * span, 0, handLengthMult, handAxis);
		}
		else
			Debug.LogWarning("Could not create Collider for Right Hand: Either Right Elbow or Right Hand Target Transform is not assigned.");

		Vector3 thumbLengthAxis;
		float thumbRadius  =  thumbRadiusMult * 0.24f * handWidthMult * span;
		float fingerRadius = fingerRadiusMult * 0.24f * handWidthMult * span;

		// Assign colliders to finger phalanges of right hand
		if(fingerColliders)
		{
			thumbLengthAxis = handAxis.magnitude * Vector3.Cross(depthAxis, handAxis).normalized;
			if(rightThumb)
				IterateFingerColliders(2, rightThumb,   fingerType,  thumbRadius,  thumbLengthMult, depthAxis, thumbLengthAxis, thumbTaperValue);
			if(rightIndexF)
				IterateFingerColliders(2, rightIndexF,  fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(rightMiddleF)
				IterateFingerColliders(2, rightMiddleF, fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(rightRingF)
				IterateFingerColliders(2, rightRingF,   fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(rightLittleF)
				IterateFingerColliders(2, rightLittleF, fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
		}


		handAxis = -0.1f * Vector3.right; // Default left hand length axis
		if(leftHand && leftElbow)
		{
			if(leftMiddleF)
				handAxis = (leftMiddleF.position - leftHand.position).magnitude * (leftHand.position - leftElbow.position).normalized;
			else
				handAxis = 0.3f * (leftHand.position - leftElbow.position);
			CreateCollider(leftHand, null, handType, Vector3.Cross(handAxis, depthAxis), handWidthMult * span, handDepthMult * span, 0, handLengthMult, handAxis);
		}
		else
			Debug.LogWarning("Could not create Collider for Left Hand: Either Left Elbow or Left Hand Target Transform is not assigned.");

		// Assign colliders to finger phalanges of left hand
		if(fingerColliders)
		{
			thumbLengthAxis = handAxis.magnitude * Vector3.Cross(handAxis, depthAxis).normalized;
			if(leftThumb)
				IterateFingerColliders(2, leftThumb,   fingerType,  thumbRadius,  thumbLengthMult, depthAxis, thumbLengthAxis, thumbTaperValue);
			if(leftIndexF)
				IterateFingerColliders(2, leftIndexF,  fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(leftMiddleF)
				IterateFingerColliders(2, leftMiddleF, fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(leftRingF)
				IterateFingerColliders(2, leftRingF,   fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
			if(leftLittleF)
				IterateFingerColliders(2, leftLittleF, fingerType, fingerRadius, fingerLengthMult, depthAxis, handAxis, fingerTaperValue);
		}
	}

	/// <summary>
	/// Creates a Collider for the startJoint Transform.
	/// </summary>
	/// <param name="startJoint">Bone start joint.</param>
	/// <param name="endJoint">Bone end joint, can be null.</param>
	/// <param name="colliderType">Collider type.</param>
	/// <param name="depthDirection">Depth direction.</param>
	/// <param name="width">Width (meters)</param>
	/// <param name="depth">Depth (meters)</param>
	/// <param name="lengthOffset">Length offset (meters)</param>
	/// <param name="lengthMultiplier">Length multiplier.</param>
	/// <param name="lengthVector">Bone direction and length if endJoint is null, default value is (0,0,0).</param>
	public void CreateCollider( Transform startJoint, Transform endJoint, AvatarColliderType colliderType, Vector3 depthDirection, 
								float width, float depth, float lengthOffset, float lengthMultiplier, Vector3 lengthVector = default(Vector3))
	{
		if(!startJoint)
			return;

		Vector3 lengthAxis = startJoint.right;
		Vector3 boneCenter = Vector3.zero;
		Vector3 boundingBox = Vector3.one;
		int lengthAxisId = 0; 
		int depthAxisId  = 1; 
		int widthAxisId  = 2; 
		float boneLength = 0.5f;

		Vector3 scaler = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
//		Vector3 cumulativeScale = new Vector3(	transform.lossyScale.x / startJoint.lossyScale.x, transform.lossyScale.y / startJoint.lossyScale.y, 
//												transform.lossyScale.z / startJoint.lossyScale.z													);
//		Vector3 startInvLossyScale = new Vector3(1 / startJoint.lossyScale.x, 1 / startJoint.lossyScale.y, 1 / startJoint.lossyScale.z);
		
		if(endJoint)
		{
//			boneLength = lengthMultiplier * Vector3.Scale(transform.InverseTransformVector(endJoint.position - startJoint.position), scaler).magnitude 
//				+ lengthOffset;
			boneLength = lengthMultiplier * (endJoint.position - startJoint.position).magnitude + lengthOffset;
			boneCenter = 0.5f * (endJoint.position + startJoint.position);
			lengthAxis = endJoint.position - startJoint.position;
		}
		else
		{
//			boneLength = lengthMultiplier * Vector3.Scale(transform.InverseTransformVector(lengthVector), scaler).magnitude + lengthOffset;
			boneLength = lengthMultiplier * lengthVector.magnitude + lengthOffset;
			boneCenter = startJoint.position + 0.5f * lengthVector;
			lengthAxis = lengthVector;
		}

//		Vector3 widthAxis  = Vector3.Cross(lengthAxis, depthDirection);

		// Apply scaling that affects the lengthAxis direction if they are non-uniform
		lengthAxis = transform.TransformVector(Vector3.Scale(transform.InverseTransformVector(lengthAxis), scaler));

		lengthAxisId = FindClosestLocalAxis(startJoint, lengthAxis);
		depthAxisId  = FindClosestLocalAxis(startJoint, depthDirection);

		if(depthAxisId == lengthAxisId)
			depthAxisId = (lengthAxisId + 1) % 3;

		while(widthAxisId == depthAxisId || widthAxisId == lengthAxisId)
			widthAxisId = (widthAxisId + 1) % 3;

		boundingBox[lengthAxisId] = boneLength / startJoint.lossyScale[lengthAxisId];
		boundingBox[depthAxisId]  = 2 * depth; // / startJoint.lossyScale[depthAxisId];
		boundingBox[widthAxisId]  = 2 * width; // / startJoint.lossyScale[widthAxisId];

		Collider collider = null;
		Collider[] allColliders = startJoint.GetComponents<Collider>();
		List<Collider> destroyColliders = new List<Collider>();
		if(allColliders != null)
		{
			// Remove all existing capsule and box colliders, save for one
			foreach(Collider candidateCollider in allColliders)
			{
				switch(colliderType)
				{
					case AvatarColliderType.BoxCollider:
						if(candidateCollider)
						{
							if(candidateCollider as CapsuleCollider != null)
								destroyColliders.Add(candidateCollider);
							else if(candidateCollider as BoxCollider != null)
							{
								if(collider)
									destroyColliders.Add(candidateCollider);
								else
									collider = candidateCollider;
							}
						}
						break;
					case AvatarColliderType.CapsuleCollider:
						if(candidateCollider)
						{
							if(candidateCollider as BoxCollider != null)
								destroyColliders.Add(candidateCollider);
							else if(candidateCollider as CapsuleCollider != null)
							{
								if(collider)
									destroyColliders.Add(candidateCollider);
								else
									collider = candidateCollider;
							}
						}
						break;
				}
			}
			foreach(Collider destroyCollider in destroyColliders)
			{
				if(Application.isEditor)
					DestroyImmediate(destroyCollider);
				else
					Destroy(destroyCollider);
			}
		}

		// Calculate Box Collider size / Capsule Collider radius in localScale
		// Below doesn't take into account cumulative parent scales
		//Vector3 boxSize = Vector3.Scale(boundingBox, 
		//								  new Vector3(1f/startJoint.localScale.x, 1f/startJoint.localScale.y, 1f/startJoint.localScale.z));
		Vector3 boxSize = boundingBox;
//		Vector3 inverseScale = startJoint.InverseTransformVector(Vector3.one);
//		inverseScale.Set(1f/inverseScale.x, 1f/inverseScale.y, 1f/inverseScale.z);
		boxSize.Set(Mathf.Max(Mathf.Abs(boxSize.x), float.Epsilon), Mathf.Max(Mathf.Abs(boxSize.y), float.Epsilon), 
					Mathf.Max(Mathf.Abs(boxSize.z), float.Epsilon));
//		boxSize = Vector3.Scale(inverseScale, boxSize); // This should take into account cumulative parent scales

		// Adjust parameters
		switch(colliderType)
		{
			case AvatarColliderType.BoxCollider:
				if(!collider)
				{
#if UNITY_EDITOR
					collider = UnityEditor.Undo.AddComponent<BoxCollider>(startJoint.gameObject);
#elif
					collider = startJoint.gameObject.AddComponent<BoxCollider>();
#endif
				}
				BoxCollider box = collider as BoxCollider;
				box.center = startJoint.InverseTransformPoint(boneCenter); 
				box.size = boxSize;
				break;
			case AvatarColliderType.CapsuleCollider:
				if(!collider)
				{
#if UNITY_EDITOR
					collider = UnityEditor.Undo.AddComponent<CapsuleCollider>(startJoint.gameObject);
#elif
					collider = startJoint.gameObject.AddComponent<CapsuleCollider>();
#endif
				}
				CapsuleCollider capsule = collider as CapsuleCollider;
				capsule.direction = lengthAxisId;
				capsule.height = boxSize[lengthAxisId]; //boneLength; // Doesn't take into account cumulative parent scales	
				capsule.center = startJoint.InverseTransformPoint(boneCenter);
				capsule.radius = 0.25f * (boxSize[depthAxisId] + boxSize[widthAxisId]); // boundingBox had 2 * width and 2 * depth, take average
				// Below doesn't take into account cumulative parent scales	
				//				if(lengthAxisId == 0)
				//					capsule.radius = Mathf.Max(Mathf.Abs(width / startJoint.localScale.y), float.Epsilon);
				//				else
				//					capsule.radius = Mathf.Max(Mathf.Abs(width / startJoint.localScale.x), float.Epsilon);
				break;
		}

		return;
	}

	/// <summary>
	/// Creates Colliders for a finger when argument startJoint points to the finger root Transform
	/// </summary>
	/// <param name="iteration">Iteration, this value should be 2 (number of phalanges - 1) when startJoint points to the finger root.</param>
	/// <param name="startJoint">Start joint.</param>
	/// <param name="colliderType">Collider type.</param>
	/// <param name="radius">Radius.</param>
	/// <param name="lengthMultiplier">Length multiplier.</param>
	/// <param name="depthDirection">Depth direction.</param>
	/// <param name="lengthVector">Length vector.</param>
	/// <param name="taperValue">Taper value; Collider radius multiplier for each successive finger phalanx iteration</param>
	public void IterateFingerColliders(int iteration, Transform startJoint, AvatarColliderType colliderType, float radius, 
									   float lengthMultiplier, Vector3 depthDirection, Vector3 lengthVector, float taperValue)
	{
		Vector3 newLengthDirection;
		if(startJoint.childCount > 0)
		{
			Transform endJoint = startJoint.GetChild(0);
			newLengthDirection = lengthVector.magnitude * (endJoint.position - startJoint.position).normalized;
			CreateCollider(startJoint, endJoint, colliderType, depthDirection, radius, radius, 0, lengthMultiplier);
			if(iteration > 0)
				IterateFingerColliders(	iteration - 1, endJoint, colliderType, taperValue * radius, lengthMultiplier, 
										depthDirection, newLengthDirection, taperValue								 );
		}
		else
		{
			// Phalanx Collider length if endJoint is not found: firstly 1.0 (iteration==2), secondly 0.5 (iteration==1), thirdly 0.25 (iteration==0)
			newLengthDirection = Mathf.Min(1f, Mathf.Pow(2, iteration - 2)) * lengthVector;
			CreateCollider(startJoint, null, colliderType, depthDirection, radius, radius, 0, lengthMultiplier, newLengthDirection);
		}
	}
		
	// TODO: REMOVE: If memory serves me correctly, this method doesn't work quite right
	private Quaternion LimitZRotation(Quaternion inputRotation, float rollMinimum, float rollMaximum)
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

			
