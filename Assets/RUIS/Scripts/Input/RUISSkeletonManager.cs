/*****************************************************************************

Content    :   A class to manage Kinect/OpenNI skeleton data
Authors    :   Mikael Matveinen, Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. 
               All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class RUISSkeletonManager : MonoBehaviour 
{
	
    public enum Joint
    {
        Root,
        Head,
		Neck, // *** OPTIHACK
        Torso,
		Chest, // *** OPTIHACK
		LeftClavicle, // *** OPTIHACK
        LeftShoulder,
        LeftElbow,
		LeftHand,
		LeftThumb, // *** OPTIHACK
		LeftIndexFinger,
		LeftMiddleFinger,
		LeftRingFinger,
		LeftLittleFinger,
		RightClavicle, // *** OPTIHACK
        RightShoulder,
        RightElbow,
        RightHand,
		RightThumb, // *** OPTIHACK
		RightIndexFinger,
		RightMiddleFinger,
		RightRingFinger,
		RightLittleFinger,
        LeftHip,
        LeftKnee,
        LeftFoot,
        RightHip,
        RightKnee,
        RightFoot,
        None
    }

    public class JointData
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float positionConfidence = 0.0f;
        public float rotationConfidence = 0.0f;
		public Joint jointID = Joint.None;
		public float boneScale = 1; // playerLength / modelLength

		public JointData(Joint jointID)
		{
			this.jointID = jointID;
		}
    }

    public class Skeleton
    {
        public bool isTracking = false;
		public JointData root 		   = new JointData(Joint.Root);
		public JointData torso 		   = new JointData(Joint.Torso);
		public JointData chest 		   = new JointData(Joint.Chest);
		public JointData neck 		   = new JointData(Joint.Neck);
		public JointData head 		   = new JointData(Joint.Head);
		public JointData leftClavicle  = new JointData(Joint.LeftClavicle);
		public JointData leftShoulder  = new JointData(Joint.LeftShoulder);
		public JointData leftElbow 	   = new JointData(Joint.LeftElbow);
		public JointData leftHand 	   = new JointData(Joint.LeftHand);
		public JointData rightClavicle = new JointData(Joint.RightClavicle);
		public JointData rightShoulder = new JointData(Joint.RightShoulder);
		public JointData rightElbow    = new JointData(Joint.RightElbow);
		public JointData rightHand 	   = new JointData(Joint.RightHand);
		public JointData leftHip 	   = new JointData(Joint.LeftHip);
		public JointData leftKnee 	   = new JointData(Joint.LeftKnee);
		public JointData leftFoot 	   = new JointData(Joint.LeftFoot);
		public JointData rightHip 	   = new JointData(Joint.RightHip);
		public JointData rightKnee 	   = new JointData(Joint.RightKnee);
		public JointData rightFoot 	   = new JointData(Joint.RightFoot);

		public JointData leftThumb  = new JointData(Joint.LeftThumb);
		public JointData rightThumb = new JointData(Joint.RightThumb);

		// *** OPTIHACK4 (some of these are unused/unnecessary?) Kinect 2 joints, TODO: add Kinect 2 joint enumerations to Joint
		public JointData baseSpine = new JointData(Joint.None);
		public JointData midSpine = new JointData(Joint.None);
		public JointData shoulderSpine = new JointData(Joint.None);
		public JointData leftWrist = new JointData(Joint.None);
		public JointData rightWrist = new JointData(Joint.None);
		public JointData leftAnkle = new JointData(Joint.None);
		public JointData rightAnkle = new JointData(Joint.None);
		public JointData leftHandTip = new JointData(Joint.None);
		public JointData rightHandTip = new JointData(Joint.None);

		public handState rightHandStatus = handState.unknown;
		public handState leftHandStatus = handState.unknown;
		
		public enum handState
		{
			closed,
			open,
			pointing,
			unknown
		}

		// Offset Z rotation of the thumb. Default value is 45, but it might depend on your avatar rig.
		public float thumbZRotationOffset = 45;

		// HACK for filtering Kinect 2 arm rotations
		public bool filterRotations = false;
		public float rotationNoiseCovariance = 200;
		public KalmanFilteredRotation[] filterRot = new KalmanFilteredRotation[14];
		public Quaternion[] previousRotation = new Quaternion[14];
		
		public ulong trackingId = 0;
    }
 
//	// Kinect 1 position filtering parameters
//	[Range(0f, 1f)]
//	public float kinect1Smoothing;             // [0..1], lower values closer to raw data
//	[Range(0f, 1f)]
//	public float kinect1Correction;            // [0..1], lower values slower to correct towards the raw data
//	[Range(0f, 1f)]
//	public float kinect1Prediction;            // [0..n], the number of frames to predict into the future
//	[Range(0f, 1f)]
//	public float kinect1JitterRadius;          // The radius in meters for jitter reduction
//	[Range(0f, 1f)]
//	public float kinect1MaxDeviationRadius;    // The maximum radius in meters that filtered positions are allowed to deviate from raw data
//	
//	// Kinect 2 position filtering parameters
//	[Range(0f, 1f)]
//	public float kinect2Smoothing;            
//	[Range(0f, 1f)]
//	public float kinect2Correction;      
//	[Range(0f, 1f)]
//	public float kinect2Prediction;            
//	[Range(0f, 1f)]
//	public float kinect2JitterRadius;          
//	[Range(0f, 1f)]
//	public float kinect2MaxDeviationRadius;    
//	
//	// Generic motion tracker position filtering parameters
//	[Range(0f, 1f)]
//	public float genericSmoothing;           
//	[Range(0f, 1f)]
//	public float genericCorrection;            
//	[Range(0f, 1f)]
//	public float genericPrediction;           
//	[Range(0f, 1f)]
//	public float genericJitterRadius;         
//	[Range(0f, 1f)]
//	public float genericMaxDeviationRadius; 
//
//	public class FilterDoubleExponentialData
//	{
//		public Vector3 m_vRawPosition;
//		public Vector3 m_vFilteredPosition;
//		public Vector3 m_vTrend;
//		public int m_dwFrameCount;
//	}
//
//	Vector3[][][] m_pFilteredJoints;
//	FilterDoubleExponentialData[][][] m_pHistory;


	NIPlayerManager playerManager;
	RUISKinect2Data RUISKinect2Data;

	RUISCoordinateSystem coordinateSystem;
	RUISInputManager inputManager;

	public const int maxTrackedSkeletons = 6;
	public const int kinect1HardwareLimit = 4; // Kinect 1 legacy (RUISTrackerEditor)

	public Skeleton[,] skeletons = new Skeleton[3, maxTrackedSkeletons];
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();

	public const int kinect1SensorID = 0;
	public const int kinect2SensorID = 1;
	public const int customSensorID  = 2;
	
	public bool isNewKinect2Frame { get; private set; }
	public float timeSinceLastKinect2Frame { get; private set; }
	public float kinect2FrameDeltaT { get; private set; }

	private Vector3 lastPelvisPosition 			= Vector3.zero;
	private Vector3 lastRightShoulderPosition 	= Vector3.zero;
	private Vector3 lastLeftShoulderPosition 	= Vector3.zero;
	private Vector3 lastRightHipPosition 		= Vector3.zero;
	private Vector3 lastLeftHipPosition 		= Vector3.zero;
	private Quaternion lastPelvisRotation 			= Quaternion.identity;
	private Quaternion lastRightShoulderRotation 	= Quaternion.identity;
	private Quaternion lastLeftShoulderRotation 	= Quaternion.identity;
	private Quaternion lastRightHipRotation 		= Quaternion.identity;
	private Quaternion lastLeftHipRotation 			= Quaternion.identity;


	[Tooltip(  "Interpolate Kinect 1 skeletons' head position from the measured head position (0) to the measured neck position (1). This setting can be "
	         + "used to give better correspondence with Kinect 2 (so that both Kinects can be used to animate the same skeletons). Default value is 0.5.")]
	[Range(0f, 0.95f)]
	public float headOffsetKinect1 = 0.5f;

	[Tooltip(  "Interpolate Kinect 2 skeletons' head position from the measured head position (0) to the measured neck position (1). This setting can be "
	         + "used to give better correspondence with Kinect 1 (so that both Kinects can be used to animate the same skeletons). Default value is 0.5.")]
	[Range(0f, 0.95f)]
	public float headOffsetKinect2 = 0.5f;
	private Vector3 tempVector = Vector3.zero;

    void Awake()
    {
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;

		playerManager = GetComponent<NIPlayerManager>();
		RUISKinect2Data = GetComponent<RUISKinect2Data>();

		if (!inputManager.enableKinect) playerManager.enabled = false;
		if (!inputManager.enableKinect2) RUISKinect2Data.enabled = false;

        if (coordinateSystem == null)
        {
            coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
        }

		for (int x = 0; x < 3; x++) // Kinect 1, Kinect 2, Custom Tracker
		{
			for (int i = 0; i < maxTrackedSkeletons; i++) 
			{
				skeletons [x, i] = new Skeleton ();

				// Kalman filters for joint rotation filtering
				for(int k = 0; k < skeletons[x, i].filterRot.Length; ++k)
				{
					skeletons[x, i].filterRot[k] = new KalmanFilteredRotation();
					skeletons[x, i].filterRot[k].SkipIdenticalMeasurements = true;
					skeletons[x, i].filterRot[k].rotationNoiseCovariance = skeletons[x, i].rotationNoiseCovariance;
					skeletons[x, i].previousRotation[k] = Quaternion.identity;
				}
			}
		}
		
		isNewKinect2Frame = false;
		timeSinceLastKinect2Frame = 0;
    }
	
	// HACK for filtering Kinect 2 arm rotations
	//void filterJointRotation(ref Quaternion measuredRotation, ref Quaternion previousRotation, ref KalmanFilteredRotation kalmanFilter, float kalmanDeltaTime)
	void FilterJointRotation(ref Skeleton skeleton, ref JointData joint, int jointID, float kalmanDeltaTime)
	{
		float updateAngle = 0.01f;
		float angleThreshold = 15;
		float covarianceMultiplier = 1;
//		float rotateSpeed = 30;

//		if (jointID < 2) // Shoulders
//		{
//			joint.rotation = Quaternion.Slerp (skeleton.previousRotation [jointID], joint.rotation, rotateSpeed * kalmanDeltaTime );
//			return;
//		}

		if(jointID == 8 || jointID == 9) // Heuristic for leftHand, rightHand
			covarianceMultiplier = 2;
		
		if(jointID > 9 && jointID < 14) // Heuristic for leftHip, rightHip, leftKnee, rightKnee
			covarianceMultiplier = 3;
		
		updateAngle = Mathf.Abs(Quaternion.Angle(joint.rotation, skeleton.previousRotation[jointID])); // New measurement vs previous rotation
		skeleton.previousRotation[jointID] = joint.rotation; // This used to be after the below if-else clauses
		if(updateAngle < angleThreshold)
		{
//			joint.rotation = Quaternion.Slerp (skeleton.previousRotation [jointID], joint.rotation, rotateSpeed * kalmanDeltaTime );
			skeleton.filterRot[jointID].rotationNoiseCovariance = covarianceMultiplier*skeleton.rotationNoiseCovariance*(0.05f + 0.95f*(angleThreshold-updateAngle)/angleThreshold);
			joint.rotation = skeleton.filterRot[jointID].Update(joint.rotation, kalmanDeltaTime);
//			if(Time.time < 10)
//				Debug.Log(1);
//				joint.rotation = skeleton.filterRot[jointID].Update(joint.rotation, kalmanDeltaTime);
//			print(Time.time);
		}
		else 
		{
			skeleton.filterRot[jointID].rotationNoiseCovariance = covarianceMultiplier * 0.05f * skeleton.rotationNoiseCovariance;
			joint.rotation = skeleton.filterRot[jointID].Update(joint.rotation, kalmanDeltaTime);
		}
//		skeleton.previousRotation[jointID] = joint.rotation;
	}

	// TODO: More efficient rotation filtering, extend filtering to all joints (now just arms)
	public void FilterSkeletonRotations(int sensorID, int playerID, float kalmanDeltaTime)
	{							
		//if(skeletons[sensorID, playerID].leftShoulder.rotationConfidence >= 0.5f)
		{
			FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].leftShoulder, 0, kalmanDeltaTime);
		}
		//if(skeletons[sensorID, playerID].rightShoulder.rotationConfidence >= 0.5f)
		{
			FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].rightShoulder, 1, kalmanDeltaTime);
		}
		//if(skeletons[sensorID, playerID].leftElbow.rotationConfidence >= 0.5f)
		{
			FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].leftElbow, 2, kalmanDeltaTime);
		}
		//if(skeletons[sensorID, playerID].rightElbow.rotationConfidence >= 0.5f)
		{
			FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].rightElbow, 3, kalmanDeltaTime);
		}

//		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].root, 4, kalmanDeltaTime);
//
////		measuredRotation = skeletons[sensorID, playerID].root.rotation;
////		skeletons[sensorID, playerID].root.rotation = skeletons[sensorID, playerID].filterRot[4].Update(measuredRotation, kalmanDeltaTime);
//							
							// TODO uncomment
		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].torso, 5, kalmanDeltaTime);
//
////		measuredRotation = skeletons[sensorID, playerID].torso.rotation;
////		skeletons[sensorID, playerID].torso.rotation = skeletons[sensorID, playerID].filterRot[5].Update(measuredRotation, kalmanDeltaTime);
//							
							// TODO uncomment
//		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].chest, 6, kalmanDeltaTime);
//
////		measuredRotation = skeletons[sensorID, playerID].chest.rotation;
////		skeletons[sensorID, playerID].chest.rotation = skeletons[sensorID, playerID].filterRot[6].Update(measuredRotation, kalmanDeltaTime);
//							
							// TODO uncomment
//		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].shoulderSpine, 7, kalmanDeltaTime);
//
////		measuredRotation = skeletons[sensorID, playerID].shoulderSpine.rotation;
////		skeletons[sensorID, playerID].shoulderSpine.rotation = skeletons[sensorID, playerID].filterRot[7].Update(measuredRotation, kalmanDeltaTime);


		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].leftHand, 8, kalmanDeltaTime);

		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].rightHand, 9, kalmanDeltaTime);


		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].rightHip, 10, kalmanDeltaTime);

		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].leftHip, 11, kalmanDeltaTime);

		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].rightKnee, 12, kalmanDeltaTime);

		FilterJointRotation(ref skeletons[sensorID, playerID], ref skeletons[sensorID, playerID].leftKnee, 13, kalmanDeltaTime);

		// Kalman filtering is a slightly heavy operation, just average two latest thumb rotations // TODO uncomment
		//							Quaternion thumbRotation = skeletons[sensorID, playerID].leftThumb.rotation;
		//							skeletons[sensorID, playerID].leftThumb.rotation = Quaternion.Slerp(thumbRotation, 
		//							                                                                           skeletons[sensorID, playerID].previousRotation[10], 0.5f);
		//							skeletons[sensorID, playerID].previousRotation[10] = thumbRotation;
		//
		//							thumbRotation = skeletons[sensorID, playerID].rightThumb.rotation;
		//							skeletons[sensorID, playerID].rightThumb.rotation = Quaternion.Slerp(thumbRotation, 
		//							                                                                           skeletons[sensorID, playerID].previousRotation[11], 0.5f);
		//							skeletons[sensorID, playerID].previousRotation[11] = thumbRotation;
	}

	void Update() 
	{
		if(inputManager.enableKinect) 
		{
			for(int i = 0; i < playerManager.m_MaxNumberOfPlayers; i++) 
			{
				skeletons[kinect1SensorID, i].isTracking = playerManager.GetPlayer(i).Tracking;

				if(!skeletons[kinect1SensorID, i].isTracking)
					continue;

				SaveLastMajorJointPoses(skeletons[kinect1SensorID, i]); // These will be used in HasNewMocapData()

				UpdateKinectRootAndPelvisData(i);

				UpdateKinectJointData(OpenNI.SkeletonJoint.Torso, i, ref skeletons[kinect1SensorID, i].chest); // Kinect 1 "Torso" is at chest level
//				UpdateKinectJointData(OpenNI.SkeletonJoint.Waist, i, ref skeletons[kinect1SensorID, i].chest); // Waist gives out 0
				UpdateKinectJointData(OpenNI.SkeletonJoint.Neck, i, ref skeletons[kinect1SensorID, i].neck);
				UpdateKinectJointData(OpenNI.SkeletonJoint.Head, i, ref skeletons[kinect1SensorID, i].head);
//				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftCollar, i, ref skeletons[kinect1SensorID, i].leftClavicle); // LeftCollar gives out 0
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons[kinect1SensorID, i].leftShoulder);
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons[kinect1SensorID, i].leftElbow);
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftHand, i, ref skeletons[kinect1SensorID, i].leftHand);
//				UpdateKinectJointData(OpenNI.SkeletonJoint.RightCollar, i, ref skeletons[kinect1SensorID, i].rightClavicle); // RightCollar gives out 0
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons[kinect1SensorID, i].rightShoulder);
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightElbow, i, ref skeletons[kinect1SensorID, i].rightElbow);
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightHand, i, ref skeletons[kinect1SensorID, i].rightHand);
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftHip, i, ref skeletons[kinect1SensorID, i].leftHip);
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons[kinect1SensorID, i].leftKnee);
				UpdateKinectJointData(OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons[kinect1SensorID, i].leftFoot);
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightHip, i, ref skeletons[kinect1SensorID, i].rightHip);
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightKnee, i, ref skeletons[kinect1SensorID, i].rightKnee);
				UpdateKinectJointData(OpenNI.SkeletonJoint.RightFoot, i, ref skeletons[kinect1SensorID, i].rightFoot);

				// Calculate the pose of pelvis (between hips)
				if(skeletons[kinect1SensorID, i].rightHip.positionConfidence >= 0.5f && skeletons[kinect1SensorID, i].leftHip.positionConfidence >= 0.5f)
				{
					skeletons[kinect1SensorID, i].torso.position = 0.5f * (   skeletons[kinect1SensorID, i].rightHip.position
																			+ skeletons[kinect1SensorID, i].leftHip.position );
					skeletons[kinect1SensorID, i].torso.positionConfidence = Mathf.Min(skeletons[kinect1SensorID, i].rightHip.positionConfidence, 
					                                                                   skeletons[kinect1SensorID, i].leftHip.positionConfidence);
				}
				else
					skeletons[kinect1SensorID, i].torso.positionConfidence = 0;
				
				skeletons[kinect1SensorID, i].torso.rotation 			= skeletons[kinect1SensorID, i].root.rotation;
				skeletons[kinect1SensorID, i].torso.rotationConfidence 	= skeletons[kinect1SensorID, i].root.rotationConfidence;

				// Interpolate head position
				skeletons[kinect1SensorID, i].head.position =  skeletons[kinect1SensorID, i].head.position 
					+ headOffsetKinect1 * (skeletons[kinect1SensorID, i].neck.position - skeletons[kinect1SensorID, i].head.position);
				skeletons[kinect1SensorID, i].head.positionConfidence = Mathf.Min(skeletons[kinect1SensorID, i].head.positionConfidence, 
				                                                                  skeletons[kinect1SensorID, i].neck.positionConfidence );

				if(skeletons[kinect1SensorID, i].filterRotations && HasNewMocapData(skeletons[kinect1SensorID, i]))
				{
					FilterSkeletonRotations(kinect1SensorID, i, 0.033f); // Kinect 1 mocap update rate is 30 FPS
				}
			}
		}

		if(inputManager.enableKinect2) 
		{
			bool isNewFrame = false;
			Kinect.Body[] data = RUISKinect2Data.getData(out isNewFrame);
			isNewKinect2Frame = isNewFrame;

			if(data != null && isNewFrame) 
			{
				Vector3 relativePos;
				int playerID = 0;
				bool newBody = true;
				int i = 0;
				
				// Refresh skeleton tracking status
				for(int y = 0; y < skeletons.GetLength(1); y++) 
					skeletons[kinect2SensorID, y].isTracking = false; 
				
				foreach(var body in data) 
				{
					if(body.IsTracked) 
					{
						for(int y = 0; y < skeletons.GetLength(1); y++) 
						{
							if(skeletons[kinect2SensorID, y].trackingId == body.TrackingId)
							{
								skeletons[kinect2SensorID, y].isTracking = true;
							}
						}
					}
				}
				
				foreach(var body in data)
				{		
					if(i >= skeletons.GetLength(1)) 
						break;
					if (body == null) 
						continue;
					newBody = true;
					playerID = 0;
					
					// Check if trackingID has been assigned to certaint index before and use that index
					if(trackingIDtoIndex.ContainsKey(body.TrackingId) && body.IsTracked)
					{
						playerID = trackingIDtoIndex[body.TrackingId];
						newBody = false;
					} 
					
					if(body.IsTracked) 
					{
						if(newBody) 
						{
							// Find the first unused slot in skeletons array
							for(int y = 0; y < skeletons.GetLength(1); y++) 
							{
								if(!skeletons [kinect2SensorID, y].isTracking) 
								{
									playerID = y;
									break;
								}
							}
						}

						trackingIDtoIndex[body.TrackingId] = playerID;
						skeletons [kinect2SensorID, playerID].trackingId = body.TrackingId;

						// HACK: Kinect 2 can't track closer than 0.5 meters!
						if(body.Joints[Kinect.JointType.SpineMid].Position.Z < 0.5f)
						{
							skeletons [kinect2SensorID, playerID].isTracking = false;
						}

						// *** OPTIHACK4 this probably should be SpineBase??
						UpdateKinect2RootData(GetKinect2JointData(body.Joints[Kinect.JointType.SpineBase], body.JointOrientations[Kinect.JointType.SpineMid]), playerID);

						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.Head], body.JointOrientations[Kinect.JointType.Head]), playerID, ref skeletons [kinect2SensorID, playerID].head);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.Neck], body.JointOrientations[Kinect.JointType.Neck]), playerID, ref skeletons [kinect2SensorID, playerID].neck);
						// *** OPTIHACK4 consider using SpineBase here as well, at least for the position
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.SpineBase], body.JointOrientations[Kinect.JointType.SpineMid]), playerID, ref skeletons [kinect2SensorID, playerID].torso);

						// *** OPTIHACK4 This needs to be seriously checked! Is torsoOffsetKinect2 really needed? Does SpineBase have a rotation (that is similar to SpineMid)?
						// Kinect 2 SpineMid position adjusted to correspond to Kinect 1's torso position (so that hip-torso segment doesn't stretch):
						// Below gets the SpineBase position with SpineMid orientation (latter of which is discarded because only .position of the JointData is used)
//						tempVector = coordinateSystem.ConvertLocation(coordinateSystem.ConvertRawKinect2Location(
//																				GetKinect2JointData(body.Joints[Kinect.JointType.SpineBase], 
//								                    												body.JointOrientations[Kinect.JointType.SpineMid]).position), RUISDevice.Kinect_2);
//						// tempVector = torsoPosition + offset * (spineBasePosition - torsoPosition)
//						// torsoOffsetKinect2 interpolates between SpineMid and SpineBase position
//						tempVector = skeletons [kinect2SensorID, playerID].torso.position + torsoOffsetKinect2 * (tempVector - skeletons[kinect2SensorID, playerID].torso.position);
//						skeletons[kinect2SensorID, playerID].root.position  = tempVector; //  root.rotation was set by  UpdateKinect2RootData() to that of SpineMid
//						skeletons[kinect2SensorID, playerID].torso.position = tempVector; // torso.rotation was set by UpdateKinect2JointData() to that of SpineMid

						// Interpolate head position
						skeletons[kinect2SensorID, playerID].head.position =  skeletons[kinect2SensorID, playerID].head.position 
																			+ headOffsetKinect2 * (skeletons[kinect2SensorID, playerID].neck.position - skeletons[kinect2SensorID, playerID].head.position);
						skeletons[kinect2SensorID, playerID].head.positionConfidence = Mathf.Min(skeletons[kinect2SensorID, playerID].head.positionConfidence, 
						                                                                         skeletons[kinect2SensorID, playerID].neck.positionConfidence );

						// *** OPTIHACK4 skeletons [kinect2SensorID, playerID].midSpine <-- change to .chest
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), playerID, ref skeletons [kinect2SensorID, playerID].chest);
						skeletons[kinect2SensorID, playerID].chest.rotation = skeletons[kinect2SensorID, playerID].neck.rotation * Quaternion.Euler(0, 180, 0);

						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.SpineShoulder], body.JointOrientations[Kinect.JointType.SpineShoulder]), playerID, ref skeletons [kinect2SensorID, playerID].shoulderSpine);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ShoulderLeft], body.JointOrientations[Kinect.JointType.ShoulderLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftShoulder);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ShoulderRight], body.JointOrientations[Kinect.JointType.ShoulderRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightShoulder);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ElbowRight], body.JointOrientations[Kinect.JointType.ElbowRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightElbow);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ElbowLeft], body.JointOrientations[Kinect.JointType.ElbowLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftElbow);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.WristRight], body.JointOrientations[Kinect.JointType.HandRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHand);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.WristLeft], body.JointOrientations[Kinect.JointType.HandLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHand);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HipLeft], body.JointOrientations[Kinect.JointType.HipLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHip);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HipRight], body.JointOrientations[Kinect.JointType.HipRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHip);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HandTipRight], body.JointOrientations[Kinect.JointType.HandTipRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHandTip);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HandTipLeft], body.JointOrientations[Kinect.JointType.HandTipLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHandTip);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.KneeRight], body.JointOrientations[Kinect.JointType.KneeRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightKnee);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.KneeLeft], body.JointOrientations[Kinect.JointType.KneeLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftKnee);

						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.WristLeft], body.JointOrientations[Kinect.JointType.WristLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftWrist);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.WristRight], body.JointOrientations[Kinect.JointType.WristRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightWrist);
						
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.AnkleLeft], body.JointOrientations[Kinect.JointType.AnkleLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftAnkle);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.AnkleRight], body.JointOrientations[Kinect.JointType.AnkleRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightAnkle);
	
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HandLeft], body.JointOrientations[Kinect.JointType.HandLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHand);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.HandRight], body.JointOrientations[Kinect.JointType.HandRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHand);

						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ThumbLeft], body.JointOrientations[Kinect.JointType.ThumbLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftThumb);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.ThumbRight], body.JointOrientations[Kinect.JointType.ThumbRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightThumb);
						
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.FootLeft], body.JointOrientations[Kinect.JointType.FootLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftFoot);
						UpdateKinect2JointData(GetKinect2JointData(body.Joints[Kinect.JointType.FootRight], body.JointOrientations[Kinect.JointType.FootRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightFoot);

						/*
						 *	Rotation corrections
						 *  Map rotations to Kinect 1 space (which is currently assumed by every script that uses RUISSkeletonManager)
						 *  HACK: Below joints are accessed from top to bottom in hierarchy, because we modify some values before using them later:
						 *        E.g. shoulder rotations are modified first because they are used to calculate lower arm rotations (same with hands and thumbs).
						 *        Also funky reassignments: skeleton[...].leftHip.rotation = skeleton[...].leftKnee.rotation
						 *  TODO: These should be fixed to be less confusing and error-inducing, preferably already in above UpdateKinect2JointData() assignments
						 */

						// *** TODO: Check that avatar rotations match that of Kinect 2 demo with boxes as body parts. Not sure about shoulders, hands, and thumbs
						//           Upper arm 45 degree rotation ??
							
						// Head // *** OPTIHACK4 change .midSpine to .chest when it has been changed in the above UpdateKinect2JointData() call
						relativePos =  skeletons[kinect2SensorID, playerID].head.position -  skeletons[kinect2SensorID, playerID].neck.position;
						skeletons[kinect2SensorID, playerID].head.rotation = Quaternion.LookRotation(relativePos,
						                                                                             skeletons[kinect2SensorID, playerID].chest.rotation * (-Vector3.right)) * Quaternion.Euler(0, -90, -90);

						skeletons[kinect2SensorID, playerID].neck.rotation = skeletons[kinect2SensorID, playerID].neck.rotation * Quaternion.Euler(0, 180, 0);

						// Torso // *** OPTIHACK4 change .midSpine to .chest when it has been changed in the above UpdateKinect2JointData() call
						skeletons[kinect2SensorID, playerID].torso.rotation = skeletons[kinect2SensorID, playerID].chest.rotation;
						
						// Upper leg
						skeletons[kinect2SensorID, playerID].leftHip.rotation = skeletons[kinect2SensorID, playerID].leftKnee.rotation * Quaternion.Euler(180, -90, 0);
						skeletons[kinect2SensorID, playerID].rightHip.rotation = skeletons[kinect2SensorID, playerID].rightKnee.rotation * Quaternion.Euler(180, 90, 0);;
						
						// Lower leg // *** OPTIHACK4 change .midSpine to .chest when it has been changed in the above UpdateKinect2JointData() call
						relativePos = skeletons[kinect2SensorID, playerID].leftAnkle.position - skeletons[kinect2SensorID, playerID].leftKnee.position;
						skeletons[kinect2SensorID, playerID].leftKnee.rotation = Quaternion.LookRotation(relativePos,
						                                                                                 skeletons[kinect2SensorID, playerID].chest.rotation * (-Vector3.right)) * Quaternion.Euler(0, 90, -90);
						//						skeletons [kinect2SensorID, playerID].leftKnee.rotation = skeletons [kinect2SensorID, playerID].leftAnkle.rotation  * Quaternion.Euler(180, -90, 0);

						// *** OPTIHACK4 change .midSpine to .chest when it has been changed in the above UpdateKinect2JointData() call
						relativePos = skeletons[kinect2SensorID, playerID].rightAnkle.position - skeletons[kinect2SensorID, playerID].rightKnee.position;
						skeletons[kinect2SensorID, playerID].rightKnee.rotation = Quaternion.LookRotation(relativePos,
						                                                                                  skeletons[kinect2SensorID, playerID].chest.rotation * (-Vector3.right)) * Quaternion.Euler(0, 90, -90);
						//						skeletons [kinect2SensorID, playerID].rightKnee.rotation = skeletons [kinect2SensorID, playerID].rightAnkle.rotation  * Quaternion.Euler(180, 90, 0);;
						
						// Feet / Ankles
						relativePos = skeletons[kinect2SensorID, playerID].leftAnkle.position - skeletons[kinect2SensorID, playerID].leftFoot.position;
						skeletons[kinect2SensorID, playerID].leftFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0); 
						
						relativePos = skeletons[kinect2SensorID, playerID].rightAnkle.position - skeletons[kinect2SensorID, playerID].rightFoot.position;
						skeletons[kinect2SensorID, playerID].rightFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0);

						// Upper arm
						//relativePos =  skeletons [kinect2SensorID, playerID].leftElbow.position - skeletons [kinect2SensorID, playerID].leftShoulder.position;
						//skeletons [kinect2SensorID, playerID].leftShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons [kinect2SensorID, playerID].leftElbow.rotation * Vector3.right) * Quaternion.Euler(-45, 90, 0);
						skeletons[kinect2SensorID, playerID].leftShoulder.rotation = skeletons[kinect2SensorID, playerID].leftElbow.rotation * Quaternion.Euler(0, 45, -90);

						//relativePos = skeletons [kinect2SensorID, playerID].rightElbow.position - skeletons [kinect2SensorID, playerID].rightShoulder.position;
						//skeletons [kinect2SensorID, playerID].rightShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons [kinect2SensorID, playerID].rightElbow.rotation * Vector3.right) * Quaternion.Euler(135, 270, 0);
						skeletons[kinect2SensorID, playerID].rightShoulder.rotation = skeletons[kinect2SensorID, playerID].rightElbow.rotation * Quaternion.Euler(0, -45, 90);
						
						// Lower arm
						relativePos = skeletons[kinect2SensorID, playerID].leftElbow.position - skeletons[kinect2SensorID, playerID].leftWrist.position;
						skeletons[kinect2SensorID, playerID].leftElbow.rotation = Quaternion.LookRotation(relativePos, 
						                                             skeletons[kinect2SensorID, playerID].leftShoulder.rotation * Vector3.forward) * Quaternion.Euler(180, -90, 0); 
						//skeletons [kinect2SensorID, playerID].leftElbow.rotation = skeletons [kinect2SensorID, playerID].leftWrist.rotation  * Quaternion.Euler(0, 180, -90); 
						
						relativePos = skeletons[kinect2SensorID, playerID].rightElbow.position - skeletons[kinect2SensorID, playerID].rightWrist.position;
						skeletons[kinect2SensorID, playerID].rightElbow.rotation = Quaternion.LookRotation(relativePos,
						                                             skeletons[kinect2SensorID, playerID].rightShoulder.rotation * Vector3.forward) * Quaternion.Euler(180, 90, 0); 
						//skeletons [kinect2SensorID, playerID].rightElbow.rotation = skeletons [kinect2SensorID, playerID].rightWrist.rotation  * Quaternion.Euler(0, -180, 90); 
						
						// Hands
						skeletons[kinect2SensorID, playerID].leftHand.rotation = skeletons[kinect2SensorID, playerID].leftHand.rotation * Quaternion.Euler(0, 180, -90);
						skeletons[kinect2SensorID, playerID].rightHand.rotation = skeletons[kinect2SensorID, playerID].rightHand.rotation * Quaternion.Euler(0, 180, 90);

						// Thumbs
						relativePos = skeletons[kinect2SensorID, playerID].leftThumb.position - skeletons[kinect2SensorID, playerID].leftWrist.position;
						skeletons[kinect2SensorID, playerID].leftThumb.rotation = Quaternion.LookRotation(relativePos,
																						skeletons[kinect2SensorID, playerID].leftHand.rotation * Vector3.up)
																						* Quaternion.Euler(-90, 0, skeletons[kinect2SensorID, playerID].thumbZRotationOffset); 
						
						relativePos = skeletons[kinect2SensorID, playerID].rightThumb.position - skeletons[kinect2SensorID, playerID].rightWrist.position;
						skeletons[kinect2SensorID, playerID].rightThumb.rotation = Quaternion.LookRotation(relativePos,
						                                                                skeletons[kinect2SensorID, playerID].rightHand.rotation * Vector3.up)
																						* Quaternion.Euler(-90, 0, -skeletons[kinect2SensorID, playerID].thumbZRotationOffset); 

						// Fist curling
						switch(body.HandLeftState) 
						{
							case Kinect.HandState.Closed:
								skeletons[kinect2SensorID, playerID].leftHandStatus = RUISSkeletonManager.Skeleton.handState.closed;
							break;	
							case Kinect.HandState.Open:
								skeletons[kinect2SensorID, playerID].leftHandStatus = RUISSkeletonManager.Skeleton.handState.open;
							break;	
							case Kinect.HandState.Lasso:
								skeletons[kinect2SensorID, playerID].leftHandStatus = RUISSkeletonManager.Skeleton.handState.pointing;
							break;
							case Kinect.HandState.Unknown:
							case Kinect.HandState.NotTracked:
								skeletons[kinect2SensorID, playerID].leftHandStatus = RUISSkeletonManager.Skeleton.handState.unknown;
							break;
						}
						switch(body.HandRightState) 
						{
							case Kinect.HandState.Closed:
								skeletons[kinect2SensorID, playerID].rightHandStatus = RUISSkeletonManager.Skeleton.handState.closed;
								break;	
							case Kinect.HandState.Open:
								skeletons[kinect2SensorID, playerID].rightHandStatus = RUISSkeletonManager.Skeleton.handState.open;
								break;	
							case Kinect.HandState.Lasso:
								skeletons[kinect2SensorID, playerID].rightHandStatus = RUISSkeletonManager.Skeleton.handState.pointing;
								break;
							case Kinect.HandState.Unknown:
							case Kinect.HandState.NotTracked:
								skeletons[kinect2SensorID, playerID].rightHandStatus = RUISSkeletonManager.Skeleton.handState.unknown;
								break;
						}

						if(isNewKinect2Frame && skeletons[kinect2SensorID, playerID].filterRotations)
						{
							FilterSkeletonRotations(kinect2SensorID, playerID, 0.033f); // Kinect 2 mocap update rate is 30 FPS
						}
						
						i++;
					}
				}
			}
			
			if(isNewKinect2Frame)
			{
				kinect2FrameDeltaT = timeSinceLastKinect2Frame;
				timeSinceLastKinect2Frame = 0;
			}
			else
				timeSinceLastKinect2Frame += Time.deltaTime;
		}
	}

	/*
	 * 	Kinect 1 functions
	 */
    private void UpdateKinectRootAndPelvisData(int player)
    {
        OpenNI.SkeletonJointTransformation data;

        if(!playerManager.GetPlayer(player).GetSkeletonJoint(OpenNI.SkeletonJoint.Torso, out data))
        {
			skeletons[kinect1SensorID, player].root.positionConfidence = 0;
			skeletons[kinect1SensorID, player].root.rotationConfidence = 0;
            return;
        }

		Vector3 newRootPosition = coordinateSystem.ConvertLocation(coordinateSystem.ConvertRawKinectLocation(data.Position.Position), RUISDevice.Kinect_1);

		skeletons[kinect1SensorID, player].root.position = newRootPosition;
		skeletons[kinect1SensorID, player].root.positionConfidence = data.Position.Confidence;

		skeletons[kinect1SensorID, player].root.rotation = coordinateSystem.ConvertRotation(coordinateSystem.ConvertRawKinectRotation(data.Orientation), RUISDevice.Kinect_1);
		skeletons[kinect1SensorID, player].root.rotationConfidence = data.Orientation.Confidence;

    }

    private void UpdateKinectJointData(OpenNI.SkeletonJoint joint, int player, ref JointData jointData)
    {
        OpenNI.SkeletonJointTransformation data;

        if(!playerManager.GetPlayer(player).GetSkeletonJoint(joint, out data))
        {
			jointData.positionConfidence = 0;
			jointData.rotationConfidence = 0;
            return;
        }

		jointData.position = coordinateSystem.ConvertLocation (coordinateSystem.ConvertRawKinectLocation(data.Position.Position), RUISDevice.Kinect_1);
        jointData.positionConfidence = data.Position.Confidence;
		jointData.rotation = coordinateSystem.ConvertRotation (coordinateSystem.ConvertRawKinectRotation(data.Orientation), RUISDevice.Kinect_1);
        jointData.rotationConfidence = data.Orientation.Confidence;
    }

	/*
	 * 	Kinect 2 functions
	 */
	private void UpdateKinect2RootData(JointData torso, int player)
	{
		tempVector = coordinateSystem.ConvertLocation(coordinateSystem.ConvertRawKinect2Location(torso.position), RUISDevice.Kinect_2);
		skeletons[kinect2SensorID, player].root.position = tempVector;
		skeletons[kinect2SensorID, player].root.positionConfidence = torso.positionConfidence;
		skeletons[kinect2SensorID, player].root.rotation = coordinateSystem.ConvertRotation(coordinateSystem.ConvertRawKinect2Rotation(torso.rotation), RUISDevice.Kinect_2);
		skeletons[kinect2SensorID, player].root.rotationConfidence = torso.rotationConfidence;
	}

	private void UpdateKinect2JointData(JointData joint, int player, ref JointData jointData)
	{
		jointData.position = coordinateSystem.ConvertLocation(coordinateSystem.ConvertRawKinect2Location(joint.position), RUISDevice.Kinect_2);
		jointData.positionConfidence = joint.positionConfidence; 
		jointData.rotation = coordinateSystem.ConvertRotation(coordinateSystem.ConvertRawKinect2Rotation(joint.rotation), RUISDevice.Kinect_2);
		jointData.rotationConfidence = joint.rotationConfidence;
	}


	public JointData GetJointData(Joint joint, int playerID, int bodyTrackingDeviceID)
    {
		if(playerID >= skeletons.GetLength(1))
			return null;

        switch(joint)
        {
            case Joint.Root:
			return skeletons[bodyTrackingDeviceID, playerID].root;
			case Joint.Torso:
				return skeletons[bodyTrackingDeviceID, playerID].torso;
			case Joint.Chest: 											// *** OPTIHACK
				return skeletons[bodyTrackingDeviceID, playerID].chest; // *** OPTIHACK
			case Joint.Neck: 											// *** OPTIHACK
				return skeletons[bodyTrackingDeviceID, playerID].neck; 	// *** OPTIHACK
            case Joint.Head:
				return skeletons[bodyTrackingDeviceID, playerID].head;
			case Joint.LeftClavicle:  											// *** OPTIHACK
				return skeletons[bodyTrackingDeviceID, playerID].leftClavicle;  // *** OPTIHACK
            case Joint.LeftShoulder:
				return skeletons[bodyTrackingDeviceID, playerID].leftShoulder;
            case Joint.LeftElbow:
				return skeletons[bodyTrackingDeviceID, playerID].leftElbow;
            case Joint.LeftHand:
				return skeletons[bodyTrackingDeviceID, playerID].leftHand;
			case Joint.RightClavicle:  											// *** OPTIHACK
				return skeletons[bodyTrackingDeviceID, playerID].rightClavicle; // *** OPTIHACK
            case Joint.RightShoulder:
				return skeletons[bodyTrackingDeviceID, playerID].rightShoulder;
            case Joint.RightElbow:
				return skeletons[bodyTrackingDeviceID, playerID].rightElbow;
            case Joint.RightHand:
				return skeletons[bodyTrackingDeviceID, playerID].rightHand;
            case Joint.LeftHip:
				return skeletons[bodyTrackingDeviceID, playerID].leftHip;
            case Joint.LeftKnee:
				return skeletons[bodyTrackingDeviceID, playerID].leftKnee;
            case Joint.LeftFoot:
				return skeletons[bodyTrackingDeviceID, playerID].leftFoot;
            case Joint.RightHip:
				return skeletons[bodyTrackingDeviceID, playerID].rightHip;
            case Joint.RightKnee:
				return skeletons[bodyTrackingDeviceID, playerID].rightKnee;
            case Joint.RightFoot:
				return skeletons[bodyTrackingDeviceID, playerID].rightFoot;
            default:
                return null;
        }
    }


	JointData tempJoint = new JointData(Joint.None); // Temporary variable used to pass values, jointID can be none
	Quaternion tempRotation = Quaternion.identity;

	// TODO: Add method that return joints by ID, assign proper jointID in the first line: ... = new JointData(Joint.None);
	private JointData GetKinect2JointData(Kinect.Joint jointPosition, Kinect.JointOrientation jointRotation) 
	{
		tempVector.Set(jointPosition.Position.X, jointPosition.Position.Y, jointPosition.Position.Z);
		tempRotation.Set(jointRotation.Orientation.X,jointRotation.Orientation.Y,jointRotation.Orientation.Z,jointRotation.Orientation.W);
		tempJoint.rotation = tempRotation;
		tempJoint.position = tempVector;

		if(jointPosition.TrackingState == Kinect.TrackingState.Tracked)  
		{
			tempJoint.positionConfidence = 1.0f;
			tempJoint.rotationConfidence = 1.0f;
		}
		else if(jointPosition.TrackingState == Kinect.TrackingState.Inferred)  
		{
			tempJoint.positionConfidence = 0.5f;
			tempJoint.rotationConfidence = 0.5f;
		}
		else if(jointPosition.TrackingState == Kinect.TrackingState.NotTracked)  
		{
			tempJoint.positionConfidence = 0.0f;
			tempJoint.rotationConfidence = 0.0f;
		}
		else 
		{
			tempJoint.positionConfidence = 0.0f;
			tempJoint.rotationConfidence = 0.0f;
		}
		return tempJoint;
	}

	public void SaveLastMajorJointPoses(Skeleton sourceSkeleton)
	{
		lastPelvisPosition 			= sourceSkeleton.torso.position;
		lastRightShoulderPosition 	= sourceSkeleton.rightShoulder.position;
		lastLeftShoulderPosition 	= sourceSkeleton.leftShoulder.position;
		lastRightHipPosition 		= sourceSkeleton.rightHip.position;
		lastLeftHipPosition 		= sourceSkeleton.leftHip.position;

		lastPelvisRotation 			= sourceSkeleton.torso.rotation;
		lastRightShoulderRotation 	= sourceSkeleton.rightShoulder.rotation;
		lastLeftShoulderRotation 	= sourceSkeleton.leftShoulder.rotation;
		lastRightHipRotation 		= sourceSkeleton.rightHip.rotation;
		lastLeftHipRotation 		= sourceSkeleton.leftHip.rotation;
	}

	public bool HasNewMocapData(Skeleton sourceSkeleton)
	{
		return 	
		   lastPelvisPosition.x != sourceSkeleton.torso.position.x || lastPelvisPosition.y != sourceSkeleton.torso.position.y
		|| lastPelvisPosition.z != sourceSkeleton.torso.position.z

		|| lastRightShoulderPosition.x != sourceSkeleton.rightShoulder.position.x || lastRightShoulderPosition.y != sourceSkeleton.rightShoulder.position.y
		|| lastRightShoulderPosition.z != sourceSkeleton.rightShoulder.position.z

		|| lastLeftShoulderPosition.x != sourceSkeleton.leftShoulder.position.x	|| lastLeftShoulderPosition.y != sourceSkeleton.leftShoulder.position.y
		|| lastLeftShoulderPosition.z != sourceSkeleton.leftShoulder.position.z

		|| lastRightHipPosition.x != sourceSkeleton.rightHip.position.x || lastRightHipPosition.y != sourceSkeleton.rightHip.position.y
		|| lastRightHipPosition.z != sourceSkeleton.rightHip.position.z

		|| lastLeftHipPosition.x != sourceSkeleton.leftHip.position.x || lastLeftHipPosition.y != sourceSkeleton.leftHip.position.y
		|| lastLeftHipPosition.z != sourceSkeleton.leftHip.position.z

		|| lastPelvisRotation.x != sourceSkeleton.torso.rotation.x || lastPelvisRotation.y != sourceSkeleton.torso.rotation.y
		|| lastPelvisRotation.z != sourceSkeleton.torso.rotation.z || lastPelvisRotation.w != sourceSkeleton.torso.rotation.w

		|| lastRightShoulderRotation.x != sourceSkeleton.rightShoulder.rotation.x || lastRightShoulderRotation.y != sourceSkeleton.rightShoulder.rotation.y
		|| lastRightShoulderRotation.z != sourceSkeleton.rightShoulder.rotation.z || lastRightShoulderRotation.w != sourceSkeleton.rightShoulder.rotation.w

		|| lastLeftShoulderRotation.x != sourceSkeleton.leftShoulder.rotation.x || lastLeftShoulderRotation.y != sourceSkeleton.leftShoulder.rotation.y
		|| lastLeftShoulderRotation.z != sourceSkeleton.leftShoulder.rotation.z || lastLeftShoulderRotation.w != sourceSkeleton.leftShoulder.rotation.w

		|| lastRightHipRotation.x != sourceSkeleton.rightHip.rotation.x || lastRightHipRotation.y != sourceSkeleton.rightHip.rotation.y
		|| lastRightHipRotation.z != sourceSkeleton.rightHip.rotation.z || lastRightHipRotation.w != sourceSkeleton.rightHip.rotation.w

		|| lastLeftHipRotation.x != sourceSkeleton.leftHip.rotation.x || lastLeftHipRotation.y != sourceSkeleton.leftHip.rotation.y
		|| lastLeftHipRotation.z != sourceSkeleton.leftHip.rotation.z || lastLeftHipRotation.w != sourceSkeleton.leftHip.rotation.w;
	}
}
