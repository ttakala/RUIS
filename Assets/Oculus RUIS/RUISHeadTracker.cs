/*****************************************************************************

Content    :   Comprehensive head tracking class with yaw drift correction
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISHeadTracker : MonoBehaviour 
{
	
	// Beginning of members that are needed by RUISCamera's oblique frustum creation
	public Vector3 defaultPosition = new Vector3(0,0,0);
    Vector3 eyeCenterPosition = new Vector3(0,0,0);
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
	
	public enum HeadPositionSource
	{
	    Kinect = 0,
	    PSMove = 1,
	    RazerHydra = 2,
		InputTransform = 3
	};
	
	public enum HeadRotationSource
	{
	    Kinect = 0,
	    PSMove = 1,
	    RazerHydra = 2,
		OculusRift = 3,
		InputTransform = 4
	};
	
	public enum RiftMagnetometer
	{
	    Off = 0,
	    ManualCalibration = 1,
		AutomaticCalibration = 2
	};
	
	public HeadPositionSource headPositionInput = HeadPositionSource.Kinect;
	
	public HeadRotationSource headRotationInput = HeadRotationSource.Kinect;
	
	public RiftMagnetometer riftMagnetometerMode = RiftMagnetometer.Off;
	
	public int oculusID = 0;
	OVRCameraController oculusCamController;
	
    public int positionPlayerID = 0;
    public int rotationPlayerID = 0;
	public RUISSkeletonManager.Joint positionJoint = RUISSkeletonManager.Joint.Head;
	public RUISSkeletonManager.Joint rotationJoint = RUISSkeletonManager.Joint.Torso; // Most stable
	private RUISSkeletonManager.JointData jointData;
	
    public int positionPSMoveID = 0;
    public int rotationPSMoveID = 0;
	private RUISPSMoveWand posePSMove;
	
	public Transform positionInput;
	public Transform rotationInput;
	
	// Position offsets are in the input source coordinate system, therefore
	// user needs to know each controllers' local coordinate system
	public Vector3 positionOffsetKinect = new Vector3(0, 0, 0);
	public Vector3 positionOffsetPSMove = new Vector3(0, 0, 0);
	public Vector3 positionOffsetHydra  = new Vector3(0, 0, 0);
	
	// Rift has its own methods for rotation offsetting
	public Vector3 rotationOffsetKinect = new Vector3(0, 0, 0);
	public Vector3 rotationOffsetPSMove = new Vector3(0, 0, 0);
	public Vector3 rotationOffsetHydra  = new Vector3(0, 0, 0);
	
	public bool filterPosition = true;
	private KalmanFilter filterPos;
	private double[] measuredPos = {0, 0, 0};
	private Vector3 measuredHeadPosition = new Vector3(0, 0, 0);
	private double[] filteredPos = {0, 0, 0};
	public float positionNoiseCovariance = 500;
	
	public bool filterRotation = false;
	private KalmanFilter filterRot;
	private double[] measuredRot = {0, 0, 0, 1};
	private Quaternion measuredHeadRotation = new Quaternion(0, 0, 0, 1);
	private double[] filteredRot = {0, 0, 0};
	public float rotationNoiseCovariance = 500;
	private Quaternion tempLocalRotation = new Quaternion(0, 0, 0, 1);
	
	// Beginning of Yaw Drift Corrector members
	public bool externalDriftCorrection = true;
	
	public enum CompassSource
	{
	    Kinect = 0,
	    PSMove = 1,
		InputTransform = 2
	};
	
	public CompassSource compass = CompassSource.PSMove;
	
	public int compassPlayerID = 0;
	public RUISSkeletonManager.Joint compassJoint = RUISSkeletonManager.Joint.Torso;
	public bool correctOnlyWhenFacingForward = true;
	private RUISSkeletonManager.JointData compassData;
	
	public int compassPSMoveID = 0;
	private RUISPSMoveWand compassPSMove;
	
	public Transform compassTransform;
	
	public float driftCorrectionRate = 0.1f;
	
	private Quaternion driftingRot = new Quaternion(0, 0, 0, 1);
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
	
	//public bool filterInFixedUpdate = false;
	
	public float driftNoiseCovariance = 3000;
	
	public GameObject driftingDirectionVisualizer;
	public GameObject compassDirectionVisualizer;
	public GameObject correctedDirectionVisualizer;
	public Transform driftVisualizerPosition;
	// End of Yaw Drift Corrector members
	
    void Awake()
    {
		filterPos = new KalmanFilter();
		filterPos.initialize(3,3);
		filterRot = new KalmanFilter();
		filterRot.initialize(4,4);
		
		// Yaw Drift Corrector invocations in Awake()
		filterDrift = new KalmanFilter();
		filterDrift.initialize(2,2);
		
		transform.localPosition = defaultPosition;
		eyeCenterPosition = defaultPosition;
		measuredHeadPosition = defaultPosition;
    }
		
	void Start()
    {
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		if(		!inputManager
			&&  (	headPositionInput == HeadPositionSource.PSMove
			     || headRotationInput == HeadRotationSource.PSMove
			     || (externalDriftCorrection && compass == CompassSource.PSMove)))
			Debug.LogError("RUISInputManager script is missing from this scene!");
		
        if (!skeletonManager)
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		if(		!skeletonManager
			&&  (	headPositionInput == HeadPositionSource.Kinect
			     || headRotationInput == HeadRotationSource.Kinect
			     || (externalDriftCorrection && compass == CompassSource.Kinect)))
			Debug.LogError("RUISSkeletonManager script is missing from this scene!");
		
		if(headPositionInput == HeadPositionSource.InputTransform && !positionInput)
			Debug.LogError("headPositionInput is none, you need to set it from the inspector!");
		
		if(headRotationInput == HeadRotationSource.InputTransform && !rotationInput)
			Debug.LogError("headRotationInput is none, you need to set it from the inspector!");
		
		oculusCamController = FindObjectOfType(typeof(OVRCameraController)) as OVRCameraController;
		if(headRotationInput == HeadRotationSource.OculusRift && !oculusCamController)
			Debug.LogError("OVRCameraController script is missing from this scene!");
	}
		
	void Update () 
	{
		switch(headPositionInput) 
		{
			case HeadPositionSource.Kinect:
		        if (   skeletonManager 
					&& skeletonManager.skeletons[positionPlayerID].torso.positionConfidence >= 1)
		        {
					jointData = skeletonManager.GetJointData(positionJoint, positionPlayerID);
					// Most stable joint:
					if(skeletonManager.skeletons[positionPlayerID].torso.positionConfidence >= 1)
						measuredHeadPosition = jointData.position // Fix for Kinect2: below takes rotation from torso
							+ skeletonManager.skeletons[positionPlayerID].torso.rotation * positionOffsetKinect;
		        }
				break;
			case HeadPositionSource.PSMove:
		        if (inputManager)
		        {
					posePSMove = inputManager.GetMoveWand(positionPSMoveID);
					if(posePSMove)
						measuredHeadPosition = posePSMove.position + posePSMove.qOrientation * positionOffsetPSMove;
				}
				break;
			case HeadPositionSource.RazerHydra:
				// TODO
				// measuredHeadPosition = hydraPosition + hydraRotation * positionOffsetHydra;
				break;
			case HeadPositionSource.InputTransform:
				if(positionInput)
					measuredHeadPosition = positionInput.position;
				break;
		}
		
        if (filterPosition)
        {
			measuredPos[0] = measuredHeadPosition.x;
			measuredPos[1] = measuredHeadPosition.y;
			measuredPos[2] = measuredHeadPosition.z;
			filterPos.setR(Time.deltaTime * positionNoiseCovariance);
		    filterPos.predict();
		    filterPos.update(measuredPos);
			filteredPos = filterPos.getState();
			transform.localPosition = new Vector3((float) filteredPos[0], (float) filteredPos[1], (float) filteredPos[2]);
        }
		else 
			transform.localPosition = measuredHeadPosition;
		
		switch(headRotationInput) 
		{
			case HeadRotationSource.OculusRift:
		        // In this case rotation is applied by OVRCameraController which should be parented
				// under this GameObject
				break;
			case HeadRotationSource.Kinect:
		        if (   skeletonManager 
					&& skeletonManager.skeletons[rotationPlayerID].torso.rotationConfidence >= 1)
		        {
					jointData = skeletonManager.GetJointData(rotationJoint, rotationPlayerID);
					// Most stable joint:
					if(skeletonManager.skeletons[rotationPlayerID].torso.rotationConfidence >= 1)
						measuredHeadRotation = // Fix for Kinect2: below takes rotation from torso
							skeletonManager.skeletons[rotationPlayerID].torso.rotation 
														* Quaternion.Euler(rotationOffsetKinect);
		        }
				break;
			case HeadRotationSource.PSMove:
		        if (inputManager)
		        {
					posePSMove = inputManager.GetMoveWand(rotationPSMoveID);
					if(posePSMove)
						measuredHeadRotation = posePSMove.qOrientation 
														* Quaternion.Euler(rotationOffsetPSMove);
				}
				break;
			case HeadRotationSource.RazerHydra:
				// TODO
				// measuredHeadRotation = hydraRotation * Quaternion.Euler(rotationOffsetHydra);
				break;
			case HeadRotationSource.InputTransform:
				if(rotationInput)
					measuredHeadRotation = rotationInput.rotation;
				break;
		}
		
        if (filterRotation)
        {
			measuredRot[0] = measuredHeadRotation.x;
			measuredRot[1] = measuredHeadRotation.y;
			measuredRot[2] = measuredHeadRotation.z;
			measuredRot[3] = measuredHeadRotation.w;
			filterRot.setR(Time.deltaTime * rotationNoiseCovariance);
		    filterRot.predict();
		    filterRot.update(measuredRot);
			filteredRot = filterRot.getState();
			tempLocalRotation = new Quaternion((float) filteredRot[0], (float) filteredRot[1], 
													 (float) filteredRot[2], (float) filteredRot[3] );
        }
		else 
			tempLocalRotation = measuredHeadRotation;
		
		
		if(	   !externalDriftCorrection // Kinect and PSMove do not need drift correction
			|| headRotationInput == HeadRotationSource.Kinect 
			|| headRotationInput == HeadRotationSource.PSMove)
			transform.localRotation = tempLocalRotation;
		else
		{
			// Yaw Drift Corrector invocations in Update()
			//if(!filterInFixedUpdate) 
			//{
			doYawFiltering(tempLocalRotation, Time.deltaTime);
			transform.localRotation = 
				Quaternion.Euler(new Vector3(driftingEuler.x, 
											 (360 + driftingEuler.y 
												  - finalYawDifference.eulerAngles.y)%360, 
											 driftingEuler.z)							  );
			//}
		}
	}
	
	void FixedUpdate() 
	{
		// Yaw Drift Corrector invocations in FixedUpdate()
		// Kinect and PSMove do not need drift correction
		//if(	   filterInFixedUpdate && externalDriftCorrection 
		//	&& headRotationInput != HeadRotationSource.Kinect 
		//	&& headRotationInput != HeadRotationSource.PSMove)
		//{
		//	doYawFiltering(tempLocalRotation, Time.deltaTime);
		//	transform.localRotation = 
		//		Quaternion.Euler(new Vector3(driftingEuler.x, 
		//									 (360 + driftingEuler.y 
		//										  - finalYawDifference.eulerAngles.y)%360, 
		//									 driftingEuler.z)							  );
		//}
	}
	
	void LateUpdate () 
	{
		// Beginning of invocations that are needed by RUISCamera's oblique frustum creation
        eyeCenterPosition = transform.localPosition;
		// End of invocations that are needed by RUISCamera's oblique frustum creation
	}
	
	public void ResetOrientation()
	{
		switch(headRotationInput) 
		{
			case HeadRotationSource.OculusRift:
				OVRDevice.ResetOrientation(0);
				break;
			case HeadRotationSource.Kinect:
		        if (skeletonManager)
		        {
					jointData = skeletonManager.GetJointData(rotationJoint, rotationPlayerID);
					// Torso is most stable joint
					rotationOffsetKinect = // Fix for Kinect2: below takes rotation from torso
						Quaternion.Inverse(
							skeletonManager.skeletons[rotationPlayerID].torso.rotation).eulerAngles;
		        }
				break;
			case HeadRotationSource.PSMove:
		        if (inputManager)
		        {
					posePSMove = inputManager.GetMoveWand(rotationPSMoveID);
					if(posePSMove)
						rotationOffsetPSMove = Quaternion.Inverse(posePSMove.qOrientation).eulerAngles;
				}
				break;
			case HeadRotationSource.RazerHydra:
				// TODO
				// rotationOffsetHydra = Quaternion.Inverse(hydraRotation).eulerAngles;
				break;
		}
	}
	
	// Beginning of methods that are needed by RUISCamera's oblique frustum creation
	private Vector3 GetLeftEyePosition(float eyeSeparation)
    { 
        return EyeCenterPosition - transform.localRotation * Vector3.right * eyeSeparation / 2;
    }

    // Returns the eye positions in slots {center, left, right}
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
	private void doYawFiltering(Quaternion driftingOrientation, float deltaT)
	{
		// If the Rift is HeadRotationSource, its rotation is not stored in measuredHeadRotation
		// in the Update() like with the other sources
		if(headRotationInput == HeadRotationSource.OculusRift)
		{
			if(OVRDevice.IsSensorPresent(oculusID))
			{
				OVRDevice.GetOrientation(oculusID, ref driftingRot);
				if(oculusCamController) 
				{
					// In the future OVR SDK oculusCamController will have oculusID?
					oculusCamController.SetYRotation(-finalYawDifference.eulerAngles.y);
				}
			}
		}
		else
			driftingRot = driftingOrientation;
		
		if(driftingDirectionVisualizer != null)
			driftingDirectionVisualizer.transform.rotation = driftingRot;
		
		driftingEuler = driftingRot.eulerAngles;
		
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
						updateDifferenceKalman( compassData.rotation.eulerAngles, 
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
						updateDifferenceKalman( compassPSMove.qOrientation.eulerAngles, 
												driftingEuler, deltaT 				 );
					}
				}
				break;
			case CompassSource.InputTransform:
				if(compassTransform != null)
					updateDifferenceKalman( compassTransform.rotation.eulerAngles, 
											driftingEuler, deltaT 				 );
				break;
		}
		
		float normalizedT = Mathf.Clamp01(deltaT * driftCorrectionRate);
		if(normalizedT != 0)
			finalYawDifference = Quaternion.Lerp(finalYawDifference, filteredYawDifference, 
											  normalizedT );
		
		if(correctedDirectionVisualizer != null)
			correctedDirectionVisualizer.transform.rotation = Quaternion.Euler(
											new Vector3(driftingEuler.x, 
														(360 + driftingEuler.y 
															 - finalYawDifference.eulerAngles.y)%360, 
														driftingEuler.z));
		//driftingRotation*Quaternion.Inverse(finalDifference);
		if(correctedDirectionVisualizer != null && driftVisualizerPosition != null)
			correctedDirectionVisualizer.transform.position = driftVisualizerPosition.position;
	}
	
	private void updateDifferenceKalman(Vector3 compassEuler, Vector3 driftingEuler, float deltaT)
	{
		driftingYaw = Quaternion.Euler(new Vector3(0, driftingEuler.y, 0));
		compassYaw  = Quaternion.Euler(new Vector3(0, compassEuler.y, 0));
		
		// If Kinect is used for drift correction, it can be set to apply correction only when
		// skeleton is facing the sensor
		if(compass != CompassSource.Kinect || (	  !correctOnlyWhenFacingForward 
											   || (compassYaw*Vector3.forward).z >= 0))
		{
			if(compassDirectionVisualizer != null)
			{
	            compassDirectionVisualizer.transform.rotation = compassYaw;
				if(driftVisualizerPosition != null)
					compassDirectionVisualizer.transform.position = driftVisualizerPosition.position;
			}
			if(driftingDirectionVisualizer != null && driftVisualizerPosition != null)
				driftingDirectionVisualizer.transform.position   = driftVisualizerPosition.position;
		
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
