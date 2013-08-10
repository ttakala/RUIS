/*****************************************************************************

Content    :   Comprehensive 6DOF tracker class with yaw drift correction
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISHeadTracker : MonoBehaviour 
{
	
	// Beginning of members that are needed by RUISCamera's oblique frustum creation
	
	/** Default tracker position before tracking is initialized  */
	public Vector3 defaultPosition = new Vector3(0,0,0);
    Vector3 eyeCenterPosition = new Vector3(0,0,0);
	
	/** This field is needed if this tracker is used as a head tracker. */
    public Vector3 EyeCenterPosition 
    {
        get
        {
            return eyeCenterPosition;
        }
    }
	
	// End of members that are needed by RUISCamera's oblique frustum creation
	
	
	RUISInputManager inputManager;
    public RUISSkeletonManager skeletonManager;
	
	// Three enums are needed if we support more devices in the future...
	// Some devices provide only location or rotation. Some devices have
	// drifting rotations (e.g. Oculus Rift)  and cannot be used as a compass.
	public enum HeadPositionSource
	{
	    Kinect = 0,
	    PSMove = 1,
	    RazerHydra = 2,
		InputTransform = 3,
		None = 4
	};
	
	public enum HeadRotationSource
	{
	    Kinect = 0,
	    PSMove = 1,
	    RazerHydra = 2,
		InputTransform = 3,
		None = 4
	};

    public enum CompassSource
    {
        Kinect = 0,
        PSMove = 1,
        RazerHydra = 2,
        InputTransform = 3,
        None = 4
    };
	
	public enum RazerHydraBase
	{
	    Kinect = 0,
	    InputTransform = 1
	};
	
	public HeadPositionSource headPositionInput = HeadPositionSource.Kinect;
	
	public HeadRotationSource headRotationInput = HeadRotationSource.Kinect;
	
	public RazerHydraBase mobileRazerBase = RazerHydraBase.Kinect;
	
	public bool pickRotationSource = false;
	
	/// <summary>
	/// (Filtered) rotation of tracker WITHOUT yaw drift correction 
	/// </summary>
	public Quaternion rawRotation {get; private set;}
	/// <summary>
	/// (Filtered) rotation of tracker that includes yaw drift correction 
	/// </summary>
	public Quaternion localRotation {get; private set;}
	/// <summary>
	/// (Filtered) position of tracker
	/// </summary>
	public Vector3 localPosition {get; private set;}
	
	public int oculusID = 0;
	OVRCameraController oculusCamController;
	public bool useOculusRiftRotation = false;
	public KeyCode resetKey;
	
    public int positionPlayerID = 0;
    public int rotationPlayerID = 0;
	public RUISSkeletonManager.Joint positionJoint = RUISSkeletonManager.Joint.Head;
	public RUISSkeletonManager.Joint rotationJoint = RUISSkeletonManager.Joint.Torso; // Most stable
	private RUISSkeletonManager.JointData jointData;
	
    public int positionPSMoveID = 0;
    public int rotationPSMoveID = 0;
	private RUISPSMoveWand posePSMove;
	
	private Vector3	sensitivity = new Vector3( 0.001f, 0.001f, 0.001f );
	public SixenseHands	positionRazerID = SixenseHands.LEFT;
	public SixenseHands	rotationRazerID = SixenseHands.LEFT;
	SixenseInput.Controller poseRazer;
	public bool isRazerBaseMobile = false;
	public Vector3 hydraBasePosition {get; private set;}
	public Quaternion hydraBaseRotation {get; private set;}
	private Vector3 hydraTempVector = new Vector3(0, 0, 0);
	private Quaternion hydraTempRotation = Quaternion.identity;
	public Vector3 hydraAtRotationTrackerOffset = new Vector3(90, 0, 0);
	public int hydraBaseKinectPlayerID = 0;
	public RUISSkeletonManager.Joint hydraBaseJoint = RUISSkeletonManager.Joint.Torso;
	public Transform hydraBaseInput;
	public Vector3 hydraBasePositionOffsetKinect  = new Vector3(0, -0.3f, 0.1f);
	public Vector3 hydraBaseRotationOffsetKinect  = new Vector3(90, 0, 0);
	public bool inferBaseRotationFromRotationTrackerKinect = true;
	public bool inferBaseRotationFromRotationTrackerTransform = false;
	private KalmanFilter hydraBaseFilterPos;
	//private KalmanFilter hydraBaseFilterRot;
	private KalmanFilteredRotation hydraBaseKalmanRot = new KalmanFilteredRotation();
	public bool filterHydraBasePose = true;
	public bool filterHydraBasePoseKinect = true;
	public bool filterHydraBasePoseTransform = true;
	public float hydraBasePositionCovariance = 100;
	public float hydraBaseRotationCovariance = 1000;
	public float hydraBasePositionCovarianceKinect = 100;
	public float hydraBaseRotationCovarianceKinect = 1000;
	public float hydraBasePositionCovarianceTransform = 100;
	public float hydraBaseRotationCovarianceTransform = 1000;
	
	public Transform positionInput;
	public Transform rotationInput;
	
	// Position offsets are in the input source coordinate system, therefore
	// user needs to know each controllers' local coordinate system
	public Vector3 positionOffsetKinect = new Vector3(0, 0, 0);
	public Vector3 positionOffsetPSMove = new Vector3(0, 0.12f, 0);
	public Vector3 positionOffsetHydra  = new Vector3(-0.1f, -0.05f, 0);
	
	// Rift has its own methods for rotation offsetting
	public Vector3 rotationOffsetKinect = new Vector3(0, 0, 0);
	public Vector3 rotationOffsetPSMove = new Vector3(0, 0, 0);
	public Vector3 rotationOffsetHydra  = new Vector3(90, 0, 0);
	
	public bool filterPosition = false;
	public bool filterPositionKinect = true;
	public bool filterPositionPSMove = false;
	public bool filterPositionHydra = false;
	public bool filterPositionTransform = false;
	private KalmanFilter filterPos;
	private double[] measuredPos = {0, 0, 0};
	private Vector3 measuredHeadPosition = new Vector3(0, 0, 0);
	private double[] filteredPos = {0, 0, 0};
	public float positionNoiseCovariance = 500;
	public float positionNoiseCovarianceKinect    = 500;
	public float positionNoiseCovariancePSMove    = 1;
	public float positionNoiseCovarianceHydra     = 1;
	public float positionNoiseCovarianceTransform = 500;
	
	public bool filterRotation = false;
	public bool filterRotationKinect = true;
	public bool filterRotationPSMove = false;
	public bool filterRotationHydra = false;
	public bool filterRotationTransform = false;
	private KalmanFilteredRotation filterRot = new KalmanFilteredRotation();
	private Quaternion measuredHeadRotation = new Quaternion(0, 0, 0, 1);
//	private KalmanFilter filterRot;
//	private double[] measuredRot = {0, 0, 0, 1};
//	private double[] filteredRot = {0, 0, 0};
	public float rotationNoiseCovariance = 500;
	public float rotationNoiseCovarianceKinect    = 500;
	public float rotationNoiseCovariancePSMove    = 1;
	public float rotationNoiseCovarianceHydra     = 1;
	public float rotationNoiseCovarianceTransform = 500;
	private Quaternion tempLocalRotation = new Quaternion(0, 0, 0, 1);
	
	// Beginning of Yaw Drift Corrector members
	public bool externalDriftCorrection = true;
	
	public CompassSource compass = CompassSource.PSMove;
	
	public bool compassIsPositionTracker = true;
	
	public int compassPlayerID = 0;
	public RUISSkeletonManager.Joint compassJoint = RUISSkeletonManager.Joint.Torso;
	public bool correctOnlyWhenFacingForward = true;
	private RUISSkeletonManager.JointData compassData;
	
	public int compassPSMoveID = 0;
	private RUISPSMoveWand compassPSMove;
	
	public SixenseHands	compassRazerID = SixenseHands.LEFT;
	SixenseInput.Controller compassRazer;
	
	public Transform compassTransform;
	
	public Vector3 compassRotationOffsetKinect = new Vector3(0, 0, 0);
	public Vector3 compassRotationOffsetPSMove = new Vector3(0, 0, 0);
	public Vector3 compassRotationOffsetHydra  = new Vector3(90, 0, 0);
	
	public float driftCorrectionRateKinect    = 0.08f;
	public float driftCorrectionRatePSMove    = 0.1f;
	public float driftCorrectionRateHydra     = 0.1f;
	public float driftCorrectionRateTransform = 0.1f;
	
	private Quaternion rotationDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion filteredYawDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion finalYawDifference = new Quaternion(0, 0, 0, 1);
	
	private Vector3 driftingEuler;
	private Vector3 compassEuler;
	private Vector3 yawDifferenceDirection;
	private Quaternion driftingYaw;
	private Quaternion compassYaw;
	
	private KalmanFilter filterDrift;
	
	private double[] measuredDrift = {0, 0};
	
	private double[] filteredDrift = {0, 0};
	
	public bool filterInFixedUpdate = true;
	
	public float driftNoiseCovariance = 1000;
	
	public bool enableVisualizers = false;
	public GameObject driftingDirectionVisualizer;
	public GameObject compassDirectionVisualizer;
	public GameObject correctedDirectionVisualizer;
	public Transform driftVisualizerPosition;
	// End of Yaw Drift Corrector members
	
    void Awake()
    {
		localPosition = Vector3.zero;
		localRotation = Quaternion.identity;
		rawRotation = Quaternion.identity;
		
		filterPos = new KalmanFilter();
		filterPos.initialize(3,3);
		filterPos.skipIdenticalMeasurements = true;
//		filterRot = new KalmanFilter();
//		filterRot.initialize(4,4);
		
		// Mobile Razer Hydra base filtering
		hydraBaseFilterPos = new KalmanFilter();
		hydraBaseFilterPos.initialize(3,3);
		hydraBaseFilterPos.skipIdenticalMeasurements = true;
//		hydraBaseFilterRot = new KalmanFilter();
//		hydraBaseFilterRot.initialize(4,4);
		
		filterRot.skipIdenticalMeasurements = true;
		
		// Yaw Drift Corrector invocations in Awake()
		filterDrift = new KalmanFilter();
		filterDrift.initialize(2,2);
		
		transform.localPosition = defaultPosition;
		eyeCenterPosition = defaultPosition;
		measuredHeadPosition = defaultPosition;
		
		hydraBasePosition = new Vector3(0, 0, 0);
		hydraBaseRotation = Quaternion.identity;
		
		oculusCamController = gameObject.GetComponentInChildren(typeof(OVRCameraController)) as OVRCameraController;
		
		if(oculusCamController)
		{
			useOculusRiftRotation = true;
		}
		else
		{
			useOculusRiftRotation = false;
		}
		//oculusCamController = FindObjectOfType(typeof(OVRCameraController)) as OVRCameraController;
		//if(headRotationInput == HeadRotationSource.OculusRift && !oculusCamController)
		//	Debug.LogError("OVRCameraController script is missing from this scene!");
		
		filterPosition = false;
    }
		
	void Start()
    {
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		if(		!inputManager
			&&  (	headPositionInput == HeadPositionSource.PSMove
			     || headRotationInput == HeadRotationSource.PSMove
			     || (externalDriftCorrection && compass == CompassSource.PSMove)))
			Debug.LogError("RUISInputManager script is missing from this scene!");
		
		if(inputManager && !inputManager.enablePSMove)
		{
			if(headPositionInput == HeadPositionSource.PSMove)
				Debug.LogError(	"Your settings indicate that you want to use PS Move for position "
								 +	"tracking, but you have not enabled it from InputManager.");
			if(headRotationInput == HeadRotationSource.PSMove)
				Debug.LogError(	"Your settings indicate that you want to use PS Move for rotation "
								 +	"tracking, but you have not enabled it from InputManager.");
			if(externalDriftCorrection && compass == CompassSource.PSMove)
				Debug.LogError(	"Your settings indicate that you want to use PS Move for yaw drift "
								 +	"correction, but you have not enabled it from InputManager.");
		}
			
        if (!skeletonManager)
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		if(		!skeletonManager
			&&  (	headPositionInput == HeadPositionSource.Kinect
			     || (headRotationInput == HeadRotationSource.Kinect && !useOculusRiftRotation)
			     || (externalDriftCorrection && compass == CompassSource.Kinect)))
		{
			Debug.LogError("RUISSkeletonManager script is missing from this scene!");
		}
		
		if(inputManager && !inputManager.enableRazerHydra)
		{
			if(headPositionInput == HeadPositionSource.RazerHydra)
				Debug.LogError(		"Your settings indicate that you want to use Razer Hydra for "
								+	"position tracking, but you have disabled Razer Hydra from RUIS "
								+	"InputManager.");
			if(headRotationInput == HeadRotationSource.RazerHydra)
				Debug.LogError(		"Your settings indicate that you want to use Razer Hydra for "
								+	"rotation tracking, but you have disabled Razer Hydra from RUIS "
								+	"InputManager.");
			if(externalDriftCorrection && compass == CompassSource.RazerHydra)
				Debug.LogError(		"Your settings indicate that you want to use Razer Hydra for "
								+	"yaw drift correction, but you have disabled Razer Hydra from RUIS "
								+	"InputManager.");
		}
		
		if(headPositionInput == HeadPositionSource.InputTransform && !positionInput)
			Debug.LogError("Position tracker's Input Transform is none, you need to set it in Unity inspector!");
		
		if(headRotationInput == HeadRotationSource.InputTransform && !rotationInput && !useOculusRiftRotation)
			Debug.LogError("Rotation tracker's Input Transform is none, you need to set it in Unity inspector!");
		
		if(headPositionInput == HeadPositionSource.Kinect && positionJoint == RUISSkeletonManager.Joint.None)
			Debug.LogError(	 "Your settings indicate that you want to track position with a "
						   + "Kinect joint, but you have left its value to None in Unity inspector!");
		
		if(		headRotationInput == HeadRotationSource.Kinect && rotationJoint == RUISSkeletonManager.Joint.None
			&&	!useOculusRiftRotation																			 )
			Debug.LogError(	 "Your settings indicate that you want to track rotation with a "
						   + "Kinect joint, but you have left its value to None in Unity inspector!");
		
		if(		externalDriftCorrection && compass == CompassSource.Kinect && compassJoint == RUISSkeletonManager.Joint.None
			&&  !compassIsPositionTracker && (useOculusRiftRotation || headRotationInput == HeadRotationSource.InputTransform))
			Debug.LogError(	 "Your settings indicate that you want to do yaw drift correction with a "
						   + "Kinect joint, but you have left its value to None in Unity inspector!");
		
		if(		externalDriftCorrection && compass == CompassSource.InputTransform && !compassTransform 
			&&  !compassIsPositionTracker && (useOculusRiftRotation || headRotationInput == HeadRotationSource.InputTransform))
			Debug.LogError("Yaw drift corrector's Input Transform is none, you need to set it in Unity inspector!");
		
		if(externalDriftCorrection && compassIsPositionTracker && headPositionInput == HeadPositionSource.None)
			Debug.LogError(		"Position Tracker is set to None, but in 'Yaw Drift Correction' you have enabled "
							+	"'Use Position Tracker'!");
		
		if(isRazerBaseMobile && (	headPositionInput == HeadPositionSource.RazerHydra
								 || headRotationInput == HeadRotationSource.RazerHydra
								 || compass == CompassSource.RazerHydra				  ))
		{
			if(mobileRazerBase == RazerHydraBase.InputTransform && hydraBaseInput == null)
				Debug.LogError(	 "Your settings indicate that you want to track Razer Hydra base station with a "
							   + "custom Input Transform, but you have left its value to None in Unity inspector!");
			if(mobileRazerBase == RazerHydraBase.Kinect && hydraBaseJoint == RUISSkeletonManager.Joint.None)
				Debug.LogError(	 "Your settings indicate that you want to track Razer Hydra base station with a "
							   + "Kinect joint, but you have left its value to None in Unity inspector!");
		}
		
		if(oculusCamController && Application.isEditor)
			Debug.Log("OVRCameraController script detected in a child object of this " + gameObject.name
					+ " object. Using Oculus Rift as a Rotation Tracker. You can access other rotation "
					+ "trackers when you remove the OVRCameraController component from the child object(s).");
		
//		if(useOculusRiftRotation && inputManager)
//		{
//			if(		(inputManager.enableKinect 		&& headPositionInput == HeadPositionSource.Kinect)
//				||	(inputManager.enableRazerHydra 	&& headPositionInput == HeadPositionSource.RazerHydra)
//				||	(inputManager.enablePSMove 		&& headPositionInput == HeadPositionSource.PSMove)
//				||	headPositionInput == HeadPositionSource.InputTransform								  )
//			{
//				oculusCamController.SetNeckPosition(Vector3.zero);
//				oculusCamController.SetEyeCenterPosition(Vector3.zero);
//				Debug.Log(	"Head position tracker found, setting NeckPosition and EyeCenterPosition to zero from "
//						  + "OVRCameraController.");
//			}
//		}
		
	}
		
	void Update () 
	{
		if(!filterInFixedUpdate)
			updateTracker(Time.deltaTime);
	}
	
	void FixedUpdate() 
	{
		if(filterInFixedUpdate)
			updateTracker(Time.fixedDeltaTime);
	}
	
	void LateUpdate () 
	{
		// Beginning of invocations that are needed by RUISCamera's oblique frustum creation
        eyeCenterPosition = transform.localPosition;
		// End of invocations that are needed by RUISCamera's oblique frustum creation
	}
	
	private void updateTracker(float deltaT)
	{

		// Lets reduce the amount of required if clauses by setting the following:
		if(!externalDriftCorrection)
			compass = CompassSource.None;
		if(useOculusRiftRotation)
			headRotationInput = HeadRotationSource.None;
		else if(	headRotationInput == HeadRotationSource.Kinect
				||  headRotationInput == HeadRotationSource.PSMove
				||  headRotationInput == HeadRotationSource.RazerHydra
				||  headRotationInput == HeadRotationSource.None	  )
			compass = CompassSource.None; // The above rotation sources do not need yaw drift correction
		if(		headPositionInput != HeadPositionSource.RazerHydra
			 && headRotationInput != HeadRotationSource.RazerHydra
			 && compass != CompassSource.RazerHydra					)
		{
			isRazerBaseMobile = false; // If Razer Hydra is not used as a source then this can be false
		}
		
		// Reset view if necessary
		bool checkRazer = false;
		bool checkPSMove = false;
		
		// Reset view: Is PS Move used for tracking?
		if (inputManager)
        {
			if(headPositionInput == HeadPositionSource.PSMove)
			{
				posePSMove = inputManager.GetMoveWand(positionPSMoveID);
				checkPSMove = true;
			}
			else if(compass == CompassSource.PSMove)
			{
				posePSMove = inputManager.GetMoveWand(compassPSMoveID);
				checkPSMove = true;
			}
			else if(headRotationInput == HeadRotationSource.PSMove)
			{
				posePSMove = inputManager.GetMoveWand(rotationPSMoveID);
				checkPSMove = true;
			}
		}
		
		// Reset view: Is Razer Hydra used for tracking?
		if(headPositionInput == HeadPositionSource.RazerHydra)
		{
			poseRazer = SixenseInput.GetController(positionRazerID);
			checkRazer = true;
		}
		else if(compass == CompassSource.RazerHydra)
		{
			poseRazer = SixenseInput.GetController(compassRazerID);
			checkRazer = true;
		}
		else if(headRotationInput == HeadRotationSource.RazerHydra)
		{
			poseRazer = SixenseInput.GetController(rotationRazerID);
			checkRazer = true;
		}
		
		// Reset view: Check if reset view button was pressed
		if(checkPSMove && posePSMove != null)
		{
			if(posePSMove.moveButtonWasPressed)
				ResetOrientation();
		}
		if(checkRazer && poseRazer != null && poseRazer.Enabled)
		{
			if(		poseRazer.GetButton(SixenseButtons.BUMPER)
				&&  poseRazer.GetButtonDown(SixenseButtons.START) )
				ResetOrientation();
		}
		if(Input.GetKeyDown(resetKey))
			ResetOrientation();
		
		/* If we are using Razer Hydra and it's attached to a moving object (i.e. the user), 
		   lets calculate the position and rotation of the base station */
		if(isRazerBaseMobile) // In the beginning of the method we coupled this to tracker sources
		{
			// Adjust hydraBasePositionOffset and hydraBaseRotationOffset if BUMPER button is down
			if(headPositionInput == HeadPositionSource.RazerHydra)
				poseRazer = SixenseInput.GetController(positionRazerID);
			else if(headRotationInput == HeadRotationSource.RazerHydra)
				poseRazer = SixenseInput.GetController(rotationRazerID);
			else if(compass == CompassSource.RazerHydra)
				poseRazer = SixenseInput.GetController(compassRazerID);
			if(poseRazer != null && poseRazer.Enabled && poseRazer.GetButton(SixenseButtons.BUMPER))
			{
				if(Mathf.Abs(poseRazer.JoystickX) > 0.1f)
				{
					if(mobileRazerBase == RazerHydraBase.Kinect)
						hydraBasePositionOffsetKinect.x    += 0.5f*deltaT*poseRazer.JoystickX;
				}
				if(Mathf.Abs(poseRazer.JoystickY) > 0.1f)
				{
					if(mobileRazerBase == RazerHydraBase.Kinect)
						hydraBasePositionOffsetKinect.y    += 0.5f*deltaT*poseRazer.JoystickY;
				}
				if(poseRazer.GetButton(SixenseButtons.THREE))
				{
					if(mobileRazerBase == RazerHydraBase.Kinect)
						hydraBaseRotationOffsetKinect.x += 60*deltaT;
				}
				if(poseRazer.GetButton(SixenseButtons.ONE))
				{
					if(mobileRazerBase == RazerHydraBase.Kinect)
						hydraBaseRotationOffsetKinect.x -= 60*deltaT;
				}
			}
			
			switch(mobileRazerBase)
			{
				case RazerHydraBase.Kinect:
					if (skeletonManager)
				        {
							jointData = skeletonManager.GetJointData(hydraBaseJoint, hydraBaseKinectPlayerID);
							if(		skeletonManager.skeletons[hydraBaseKinectPlayerID].isTracking
								&&  jointData != null)
							{
								filterHydraBasePose = filterHydraBasePoseKinect;
								hydraBasePositionCovariance = hydraBasePositionCovarianceKinect 
																+ Mathf.Clamp01(1.0f - jointData.positionConfidence)*2000;
								hydraBaseRotationCovariance = hydraBaseRotationCovarianceKinect;

								if(		inferBaseRotationFromRotationTrackerKinect 
									&&  headRotationInput != HeadRotationSource.RazerHydra)
								{
									// Assuming that poseRazer is attached to Rotation Tracker
									if(poseRazer  != null && poseRazer.Enabled)
									{
										// Offset-adjusted Razer Hydra rotation in offset-adjusted base station coordinate
										// system: Rotation from base to Razer Hydra
										tempLocalRotation = Quaternion.Euler(hydraBaseRotationOffsetKinect)
															* poseRazer.Rotation
															* Quaternion.Inverse(Quaternion.Euler(hydraAtRotationTrackerOffset));
								
										// Subtract above rotation from Rotation Tracker's rotation (drift corrected, if enabled)
										hydraTempRotation = localRotation * Quaternion.Inverse(tempLocalRotation);
								
										// Get yaw rotation of above, result is the base station's yaw in Unity world coordinates
										hydraTempRotation = Quaternion.Euler(0, hydraTempRotation.eulerAngles.y, 0) ;
								
										// hydraTempVector will become hydraBasePosition after filtering
										hydraTempVector = jointData.position + hydraTempRotation * hydraBasePositionOffsetKinect;
								
										// Apply base station offset to hydraTempRotation, that will become hydraBaseRotation
										hydraTempRotation = hydraTempRotation * Quaternion.Euler(hydraBaseRotationOffsetKinect);
									}
								}
								else
								{
									hydraTempVector   = jointData.position + jointData.rotation * hydraBasePositionOffsetKinect;
									hydraTempRotation = jointData.rotation * Quaternion.Euler(hydraBaseRotationOffsetKinect);
									hydraBaseRotationCovariance += Mathf.Clamp01(1.0f - jointData.rotationConfidence)*2000;
								}
						
							}
				        }
					break;
				case RazerHydraBase.InputTransform:
					if(hydraBaseInput)
					{
						filterHydraBasePose = filterHydraBasePoseTransform;
						hydraBasePositionCovariance = hydraBasePositionCovarianceTransform;
						hydraBaseRotationCovariance = hydraBaseRotationCovarianceTransform;
					
						if(		inferBaseRotationFromRotationTrackerTransform 
							&&  headRotationInput != HeadRotationSource.RazerHydra)
						{
							// Assuming that poseRazer is attached to Rotation Tracker
							if(poseRazer  != null && poseRazer.Enabled)
							{
								// Offset-adjusted Razer Hydra rotation in base station coordinate
								// system: Rotation from base to Razer Hydra
								tempLocalRotation =   poseRazer.Rotation
													* Quaternion.Inverse(Quaternion.Euler(hydraAtRotationTrackerOffset));
						
								// Subtract above rotation from Rotation Tracker's rotation (drift corrected, if enabled)
								hydraTempRotation = localRotation * Quaternion.Inverse(tempLocalRotation);
						
								// Get yaw rotation of above, result is the base station's yaw in Unity world coordinates
								hydraTempRotation = Quaternion.Euler(0, hydraTempRotation.eulerAngles.y, 0) ;
						
								// hydraTempVector will become hydraBasePosition after filtering
								hydraTempVector = hydraBaseInput.position;
							}
						}
						else
						{
							hydraTempVector   = hydraBaseInput.position;
							hydraTempRotation = hydraBaseInput.rotation;
						}
					}
					break;
				default:
					filterHydraBasePose = false;
					break;
			}								
			if(filterHydraBasePose)
			{
				measuredPos[0] = hydraTempVector.x;
				measuredPos[1] = hydraTempVector.y;
				measuredPos[2] = hydraTempVector.z;
				hydraBaseFilterPos.setR(deltaT * hydraBasePositionCovariance);
			    hydraBaseFilterPos.predict();
			    hydraBaseFilterPos.update(measuredPos);
				filteredPos = hydraBaseFilterPos.getState();
				hydraBasePosition = new Vector3(  (float) filteredPos[0], 
												  (float) filteredPos[1],
												  (float) filteredPos[2] );
			}
			else
				hydraBasePosition = hydraTempVector;
//			float normalizedT = Mathf.Clamp01(deltaT * 5);
//			if(normalizedT != 0)
//				hydraBasePosition = Vector3.Lerp(hydraBasePosition, hydraTempVector, normalizedT );
			
			if(filterHydraBasePose)
			{
//				measuredRot[0] = hydraTempRotation.x;
//				measuredRot[1] = hydraTempRotation.y;
//				measuredRot[2] = hydraTempRotation.z;
//				measuredRot[3] = hydraTempRotation.w;
//				hydraBaseFilterRot.setR(deltaT * hydraBaseRotationCovariance);
//			    hydraBaseFilterRot.predict();
//			    hydraBaseFilterRot.update(measuredRot);
//				filteredRot = hydraBaseFilterRot.getState();
//				hydraBaseRotation = new Quaternion( (float) filteredRot[0], (float) filteredRot[1], 
//													(float) filteredRot[2], (float) filteredRot[3] );
				
				hydraBaseKalmanRot.rotationNoiseCovariance = hydraBaseRotationCovariance;
				hydraBaseRotation = hydraBaseKalmanRot.Update(hydraTempRotation, deltaT);
			}
			else
				hydraBaseRotation = hydraTempRotation;		
//			normalizedT = Mathf.Clamp01(deltaT * 5);
//			if(normalizedT != 0)
//				hydraBaseRotation = Quaternion.Lerp(hydraBaseRotation, hydraTempRotation, normalizedT);
		}
		else
		{
			hydraBasePosition = new Vector3(0, 0, 0);
			hydraBaseRotation = Quaternion.identity;
		}
		
		switch(headPositionInput) 
		{
			case HeadPositionSource.Kinect:
		        if (   skeletonManager 
					&& skeletonManager.skeletons[positionPlayerID].torso.positionConfidence >= 1) // Most stable joint is torso
		        {
					filterPosition = filterPositionKinect;
					positionNoiseCovariance = positionNoiseCovarianceKinect;
					jointData = skeletonManager.GetJointData(positionJoint, positionPlayerID);
					if(jointData != null)
						measuredHeadPosition = jointData.position // Fix for Kinect2: below takes rotation from torso
							- skeletonManager.skeletons[positionPlayerID].torso.rotation 
											* Quaternion.Inverse(Quaternion.Euler(rotationOffsetKinect)) * positionOffsetKinect;
		        }
				break;
			case HeadPositionSource.PSMove:
		        if (inputManager)
		        {
					posePSMove = inputManager.GetMoveWand(positionPSMoveID);
					if(posePSMove)
					{
						filterPosition = filterPositionPSMove;
						positionNoiseCovariance = positionNoiseCovariancePSMove;
						measuredHeadPosition = posePSMove.position 
										- posePSMove.qOrientation * Quaternion.Inverse(Quaternion.Euler(rotationOffsetPSMove)) 
																										* positionOffsetPSMove;
					}
				}
				break;
			case HeadPositionSource.RazerHydra:
				poseRazer = SixenseInput.GetController(positionRazerID);
				if(poseRazer != null && poseRazer.Enabled)
				{
					filterPosition = filterPositionHydra;
					positionNoiseCovariance = positionNoiseCovarianceHydra;
					measuredHeadPosition = new Vector3( poseRazer.Position.x * sensitivity.x,
														poseRazer.Position.y * sensitivity.y,
														poseRazer.Position.z * sensitivity.z  ) 
											- poseRazer.Rotation * Quaternion.Inverse(Quaternion.Euler(rotationOffsetHydra))
																										* positionOffsetHydra;
					if(isRazerBaseMobile)
						measuredHeadPosition = hydraBasePosition + hydraBaseRotation*measuredHeadPosition;
				}
				break;
			case HeadPositionSource.InputTransform:
				if(positionInput)
				{
					filterPosition = filterPositionTransform;
					positionNoiseCovariance = positionNoiseCovarianceTransform;
					measuredHeadPosition = positionInput.position;
				}
				break;
			case HeadPositionSource.None:
				filterPosition = false;
				break;
		}
		
        if (filterPosition)
        {
			measuredPos[0] = measuredHeadPosition.x;
			measuredPos[1] = measuredHeadPosition.y;
			measuredPos[2] = measuredHeadPosition.z;
			filterPos.setR(deltaT * positionNoiseCovariance);
		    filterPos.predict();
		    filterPos.update(measuredPos);
			filteredPos = filterPos.getState();
			localPosition = new Vector3((float) filteredPos[0], (float) filteredPos[1], (float) filteredPos[2]);
			transform.localPosition = localPosition;
        }
		else
		{
			//if((localPosition - measuredHeadPosition).magnitude > 0.3f)
			//	Debug.LogError("aa " + (localPosition - measuredHeadPosition).magnitude + "locR " 
			///           + localRotation + "bPos " + hydraBasePosition+ "bRot " + hydraBaseRotation);
			//else print ("ok " + (localPosition - measuredHeadPosition).magnitude + "locR "
			//            + localRotation + "bPos " + hydraBasePosition+ "bRot " + hydraBaseRotation);
			localPosition = measuredHeadPosition;
			transform.localPosition = measuredHeadPosition;
		}
		
		// Determine whether rotation source is Oculus Rift or some other device
		if(useOculusRiftRotation)
		{
			if(OVRDevice.IsSensorPresent(oculusID))
			{
				if(!OVRDevice.GetOrientation(oculusID, ref tempLocalRotation))
					tempLocalRotation = Quaternion.identity;
				
			}
		}
		else
		{
			switch(headRotationInput) 
			{
				//case HeadRotationSource.OculusRift:
			        // In this case rotation is applied by OVRCameraController which should be parented
					// under this GameObject
				//	break;
				case HeadRotationSource.Kinect:
			        if (   skeletonManager 
						&& skeletonManager.skeletons[rotationPlayerID].torso.rotationConfidence >= 1)
			        {
						filterRotation = filterRotationKinect;
						rotationNoiseCovariance = rotationNoiseCovarianceKinect;
						jointData = skeletonManager.GetJointData(rotationJoint, rotationPlayerID);
						// Most stable joint:
						if(jointData != null && jointData.rotationConfidence >= 1)
							measuredHeadRotation = jointData.rotation * Quaternion.Inverse(Quaternion.Euler(rotationOffsetKinect));
			        }
					break;
				case HeadRotationSource.PSMove:
			        if (inputManager)
			        {
						posePSMove = inputManager.GetMoveWand(rotationPSMoveID);
						if(posePSMove)
						{
							filterRotation = filterRotationPSMove;
							rotationNoiseCovariance = rotationNoiseCovariancePSMove;
							measuredHeadRotation = posePSMove.qOrientation * Quaternion.Inverse(Quaternion.Euler(rotationOffsetPSMove));
						}
					}
					break;
				case HeadRotationSource.RazerHydra:
					poseRazer = SixenseInput.GetController(rotationRazerID);
					if(poseRazer  != null && poseRazer.Enabled)
					{
						filterRotation = filterRotationHydra;
						rotationNoiseCovariance = rotationNoiseCovarianceHydra;
						measuredHeadRotation = poseRazer.Rotation * Quaternion.Inverse(Quaternion.Euler(rotationOffsetHydra));
						if(isRazerBaseMobile)
							measuredHeadRotation = hydraBaseRotation * measuredHeadRotation;
					}
					break;
				case HeadRotationSource.InputTransform:
					if(rotationInput)
					{
						filterRotation = filterRotationTransform;
						rotationNoiseCovariance = rotationNoiseCovarianceTransform;
						measuredHeadRotation = rotationInput.rotation;
					}
					break;
				case HeadRotationSource.None:
					filterRotation = false;
					break;
			}
			
	        if (filterRotation)
	        {
//				measuredRot[0] = measuredHeadRotation.x;
//				measuredRot[1] = measuredHeadRotation.y;
//				measuredRot[2] = measuredHeadRotation.z;
//				measuredRot[3] = measuredHeadRotation.w;
//				filterRot.setR(deltaT * rotationNoiseCovariance);
//			    filterRot.predict();
//			    filterRot.update(measuredRot);
//				filteredRot = filterRot.getState();
//				tempLocalRotation = new Quaternion(	(float) filteredRot[0], (float) filteredRot[1], 
//													(float) filteredRot[2], (float) filteredRot[3] );
				
				filterRot.rotationNoiseCovariance = rotationNoiseCovariance;
				tempLocalRotation = filterRot.Update(measuredHeadRotation, deltaT);
	        }
			else 
				tempLocalRotation = measuredHeadRotation;
		}
		
		rawRotation = tempLocalRotation;
		
		// Do yaw drift correction for rotation source if that option is enabled and necessary
		if(	   !externalDriftCorrection
			|| compass == CompassSource.None )
		{
			localRotation = rawRotation;
			transform.localRotation = rawRotation;
		}
		else
		{
			localRotation = driftCorrectedRotation(tempLocalRotation, deltaT);
			transform.localRotation = localRotation;
		}
	}
	
	/// <summary>
	/// Resets Oculus Rift orientation and yaw drift correction
	/// </summary>
	public void ResetOrientation()
	{
		if(externalDriftCorrection && compass != CompassSource.PSMove) 
			filterDrift.reset(); // Reset yaw filter correction to zero
		
		if(useOculusRiftRotation)
		{
			finalYawDifference = Quaternion.identity;
			OVRDevice.ResetOrientation(oculusID);
		}
	}
	
	/// <summary>
	/// Sets the Rotation Tracker's rotation offset (euler angles) to the source 
	/// rotation's current value. The resulting rotation offset will be correct
	/// if the tracked object (e.g. head) is oriented along Unity world coordinates,
	/// i.e. the tracked object is "looking" into +Z-direction while its "top" is
	/// pointing into +Y-direction.
	/// </summary>
	public Vector3 CalibrateRotationOffset()
	{
		filterRot.Reset();
		switch(headRotationInput)
		{
			case HeadRotationSource.Kinect:
		        if (skeletonManager)
		        {
					jointData = skeletonManager.GetJointData(rotationJoint, rotationPlayerID);
					if(jointData != null)
					{
						rotationOffsetKinect = jointData.rotation.eulerAngles;
						return rotationOffsetKinect;
					}
		        }
				break;
			case HeadRotationSource.PSMove:
		        if (inputManager)
		        {
					posePSMove = inputManager.GetMoveWand(rotationPSMoveID);
					if(posePSMove)
					{
						rotationOffsetPSMove = posePSMove.qOrientation.eulerAngles;
						return rotationOffsetPSMove;
					}
				}
				break;
			case HeadRotationSource.RazerHydra:
				poseRazer = SixenseInput.GetController(rotationRazerID);
				if(poseRazer != null && poseRazer.Enabled)
				{
					rotationOffsetHydra = poseRazer.Rotation.eulerAngles;
					return rotationOffsetHydra;
				}
				break;
		}
		return Vector3.zero;
	}
		
	// Beginning of methods that are needed by RUISCamera's oblique frustum creation
	private Vector3 GetLeftEyePosition(float eyeSeparation)
    { 
        return EyeCenterPosition - transform.localRotation * Vector3.right * eyeSeparation / 2;
    }

	/// <summary>
	/// This method is needed if this tracker is used as a head tracker.
	///	Returns the eye positions as a Vector3 list: [center, left, right]
	/// </summary>
    public Vector3[] GetEyePositions(float eyeSeparation)
    {
        Vector3 leftEye = GetLeftEyePosition(eyeSeparation);
        return new Vector3[] {
            EyeCenterPosition,
            leftEye,
            EyeCenterPosition + (EyeCenterPosition - leftEye)
        };
    }
	// End of methods that are needed by RUISCamera's oblique frustum creation
	
	// Beginning of Yaw Drift Corrector methods
	private Quaternion driftCorrectedRotation(Quaternion driftedRotation, float deltaT)
	{
		doYawFiltering(driftedRotation, deltaT);
		// driftingEuler and finalYawDifference are private members set in doYawFiltering()
		return Quaternion.Euler(new Vector3( driftingEuler.x, 
											 (360 + driftingEuler.y 
												  - finalYawDifference.eulerAngles.y)%360, 
											 driftingEuler.z)							  );
	}
	
	
	private void doYawFiltering(Quaternion driftingOrientation, float deltaT)
	{
		// If the Rift is HeadRotationSource, we need to apply the yaw correction to it
		if(useOculusRiftRotation)
		{
			if(OVRDevice.IsSensorPresent(oculusID))
			{
				if(oculusCamController) 
				{
					// In the future OVR SDK oculusCamController will have oculusID?
					oculusCamController.SetYRotation(-finalYawDifference.eulerAngles.y);
				}
			}
		}
		
		driftingEuler = driftingOrientation.eulerAngles;
		
		// You can set compassIsPositionTracker to true in a script and it will work as
		// expected, but if you return it to false, it doesn't remember what the compass
		// was before setting it to true
//		if(compassIsPositionTracker)
//		{
//			if(headPositionInput == HeadPositionSource.None)
//				return; // Don't do yaw drift correction in this case
//			
//			switch(headPositionInput) 
//			{
//				case HeadPositionSource.Kinect:
//					compass = CompassSource.Kinect; 
//					compassPlayerID = positionPlayerID;
//					compassJoint = positionJoint;
//					break;
//				case HeadPositionSource.PSMove:
//					compass = CompassSource.PSMove;
//					compassPSMoveID = positionPSMoveID;
//					break;
//				case HeadPositionSource.RazerHydra:
//					compass = CompassSource.RazerHydra; 
//					compassRazerID = positionRazerID;
//					break;
//				case HeadPositionSource.InputTransform:
//					compass = CompassSource.InputTransform;
//					compassTransform = positionInput;
//					break;
//			}
//		}
		
		float driftCorrectionRate = 0.1f;
		switch(compass) 
		{
			case CompassSource.Kinect:
		        if (!skeletonManager || !skeletonManager.skeletons[compassPlayerID].isTracking)
		        {
		            break;
		        }
				else 
				{
					compassData = skeletonManager.GetJointData(compassJoint, compassPlayerID);
				
					// First check for high confidence value
		            if (compassData != null && compassData.rotationConfidence >= 1.0f) 
					{
						driftCorrectionRate = driftCorrectionRateKinect;
						updateDifferenceKalman( (compassData.rotation 
													* Quaternion.Inverse(Quaternion.Euler(compassRotationOffsetKinect))).eulerAngles, 
												driftingEuler, deltaT 			 );
		            }
				}
				break;
			
			case CompassSource.PSMove:
		        if (inputManager)
		        {
					compassPSMove = inputManager.GetMoveWand(compassPSMoveID);
					if(compassPSMove)
					{
						driftCorrectionRate = driftCorrectionRatePSMove;
						updateDifferenceKalman( (compassPSMove.qOrientation 
													* Quaternion.Inverse(Quaternion.Euler(compassRotationOffsetPSMove))).eulerAngles, 
												driftingEuler, deltaT 				 );
					}
				}
				break;
			
			case CompassSource.RazerHydra:
				compassRazer = SixenseInput.GetController(compassRazerID);
				if(compassRazer != null && compassRazer.Enabled)
				{
					driftCorrectionRate = driftCorrectionRateHydra;
					if(isRazerBaseMobile)
						updateDifferenceKalman((hydraBaseRotation * compassRazer.Rotation 
													* Quaternion.Inverse(Quaternion.Euler(compassRotationOffsetHydra))).eulerAngles,
												driftingEuler, deltaT 				 				 	);
					else
						updateDifferenceKalman( (compassRazer.Rotation 
													* Quaternion.Inverse(Quaternion.Euler(compassRotationOffsetHydra))).eulerAngles,
												driftingEuler, deltaT 				 );
				}
				break;
			
			case CompassSource.InputTransform:
				if(compassTransform != null)
				{
					driftCorrectionRate = driftCorrectionRateTransform;
					updateDifferenceKalman( compassTransform.rotation.eulerAngles, 
											driftingEuler, deltaT 				 );
				}
				break;
		}
		
		float normalizedT = Mathf.Clamp01(deltaT * driftCorrectionRate);
		if(normalizedT != 0)
			finalYawDifference = Quaternion.Lerp(finalYawDifference, filteredYawDifference, 
											  normalizedT );
		// TODO: REMOVE THIS ***
//		if(finalYawDifference.x*finalYawDifference.x + finalYawDifference.y*finalYawDifference.y
//			+ finalYawDifference.z*finalYawDifference.z + finalYawDifference.w*finalYawDifference.w < 0.3)
//			Debug.LogError("LERP:ing quaternions was a bad idea: " + finalYawDifference);
		
		if(enableVisualizers)
		{
			if(driftingDirectionVisualizer != null)
				driftingDirectionVisualizer.transform.rotation = driftingOrientation;
			if(correctedDirectionVisualizer != null)
				correctedDirectionVisualizer.transform.rotation = Quaternion.Euler(
												new Vector3(driftingEuler.x, 
															(360 + driftingEuler.y 
																 - finalYawDifference.eulerAngles.y)%360, 
															driftingEuler.z));
			if(driftVisualizerPosition != null)
			{
				if(driftingDirectionVisualizer != null)
					driftingDirectionVisualizer.transform.position = driftVisualizerPosition.position;
				if(compassDirectionVisualizer != null)
					compassDirectionVisualizer.transform.position = driftVisualizerPosition.position;
				if(correctedDirectionVisualizer != null)
					correctedDirectionVisualizer.transform.position = driftVisualizerPosition.position;
			}
		}
	}
	
	private void updateDifferenceKalman(Vector3 compassEuler, Vector3 driftingEuler, float deltaT)
	{
		driftingYaw = Quaternion.Euler(0, driftingEuler.y, 0);
		compassYaw  = Quaternion.Euler(0, compassEuler.y, 0);
		
		// If Kinect is used for drift correction, it can be set to apply correction only when
		// skeleton is facing the sensor
		if(compass != CompassSource.Kinect || (	  !correctOnlyWhenFacingForward 
											   || (compassYaw*Vector3.forward).z >= 0))
		{
			if(enableVisualizers)
			{
				if(compassDirectionVisualizer != null)
				{
		            compassDirectionVisualizer.transform.rotation = compassYaw;
				}
			}
		
			// Yaw gets unstable when pitch is near poles so disregard those cases
			if(	  (driftingEuler.x < 60 || driftingEuler.x > 300)
			   && ( compassEuler.x < 60 ||  compassEuler.x > 300)) 
			{
				rotationDifference = driftingYaw * Quaternion.Inverse(compassYaw);
				yawDifferenceDirection = rotationDifference*Vector3.forward;
			
				// 2D vector rotated by yaw difference has continuous components
				measuredDrift[0] = yawDifferenceDirection.x;
				measuredDrift[1] = yawDifferenceDirection.z;
				
				filterDrift.setR(deltaT * driftNoiseCovariance);
			    filterDrift.predict();
			    filterDrift.update(measuredDrift);
				filteredDrift = filterDrift.getState();
				filteredYawDifference = 
								Quaternion.LookRotation(new Vector3((float) filteredDrift[0], 0, 
																	(float) filteredDrift[1])   );
			}
		}
	}
	// End of Yaw Drift Corrector methods
		
}
