/*****************************************************************************

Content    :   Class for correcting sensor's yaw drift with Kinect or PS Move
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class YawDriftCorrector : MonoBehaviour {

	public enum CompassSource{
	    Kinect = 0,
	    PSMove = 1
	};
	
	public enum DriftingRotation{
	    OculusRift = 0,
	    InputTransform = 1
	};
	
	public DriftingRotation driftingSensor = DriftingRotation.OculusRift;
	
	public int oculusID = 0;
	OVRCameraController oculusCamController;
	public Transform inputTransform;
	
	
	public CompassSource    compass = CompassSource.PSMove;
	
	public int kinectSkeletonID = 0;
	private RUISSkeletonManager skeletonManager;
	public RUISSkeletonManager.Joint compassJoint = RUISSkeletonManager.Joint.Torso;
	public bool correctOnlyWhenFacingForward = true;
	private RUISSkeletonManager.JointData compassData;
	
	public int PSMoveID = 0;
	private RUISPSMoveWand compassMove;
	
	
	
	RUISInputManager inputManager;
	
	private Quaternion driftingRot = new Quaternion(0, 0, 0, 1);
	private Quaternion rotationDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion filteredYawDifference = new Quaternion(0, 0, 0, 1);
	private Quaternion finalDifference = new Quaternion(0, 0, 0, 1);
	
	private Vector3 driftingEuler;
	private Vector3 compassEuler;
	private Vector3 yawDirection;
	private Quaternion driftingYaw;
	private Quaternion compassYaw;
	
	private KalmanFilter filterRot;
	
	private double[] measuredVector = {0, 0};
	
	private double[] filteredVector = {0, 0};
	
	public bool filterInFixedUpdate = false;
	
	public float filterNoiseCovariance = 3000;
	
	public GameObject inputDirectionVisualizer;
	public GameObject compassDirectionVisualizer;
	public GameObject outputDirectionVisualizer;
	public Transform visualizerPosition;
	
    public void Awake()
    {
		filterRot = new KalmanFilter();
		filterRot.initialize(2,2);
		
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		oculusCamController = FindObjectOfType(typeof(OVRCameraController)) as OVRCameraController;
	}
	
	
    public void Start()
    {
		switch(compass) 
		{
			case CompassSource.Kinect:
		        if (!skeletonManager)
		        {
		            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		        }
				break;
			case CompassSource.PSMove:
			
				break;
		}
		
	}
	
	// Update is called once per frame
	void Update() 
	{
		/*
		// Frame rate slow down simulation :-)
		int iter = Random.Range(100000, 100000000);
		int result = 0;
		for(int i=0; i<iter; ++i)
		{
			result = (iter + 980179)%(iter + 123020211);
		}
		*/
		
		if(!filterInFixedUpdate) 
		{
			doYawFiltering(Time.deltaTime);
		}
	}
	
	void FixedUpdate() 
	{
		if(filterInFixedUpdate) 
		{
			doYawFiltering(Time.fixedDeltaTime);
		}
	}
	
	private void doYawFiltering(float deltaT)
	{
			
		switch(driftingSensor) 
		{
			case DriftingRotation.OculusRift:
				if(OVRDevice.IsSensorPresent(oculusID))
				{
					OVRDevice.GetOrientation(oculusID, ref driftingRot);
					inputDirectionVisualizer.transform.rotation = driftingRot;
					if(oculusCamController)
					{
						oculusCamController.SetYRotation(-finalDifference.eulerAngles.y);
					}
				}
				break;
			case DriftingRotation.InputTransform:

				if(inputTransform) // if(inputManager)
				{
					driftingRot = inputTransform.rotation; //inputManager.GetMoveWand(2).qOrientation;
					inputDirectionVisualizer.transform.rotation = driftingRot;
				}
				break;
		}
		
		driftingEuler = driftingRot.eulerAngles;
		
		switch(compass) 
		{
			case CompassSource.Kinect:
		        if (!skeletonManager || !skeletonManager.skeletons[kinectSkeletonID].isTracking)
		        {
		            break;
		        }
				else 
				{
					compassData = skeletonManager.GetJointData(compassJoint, kinectSkeletonID);
				
					// First check for high confidence value
		            if (compassData != null && compassData.rotationConfidence >= 1.0f) 
					{
						updateDifferenceKalman( compassData.rotation.eulerAngles, 
												driftingEuler, deltaT );
		            }
				}
				break;
			case CompassSource.PSMove:
				
		        if (inputManager)
		        {
					compassMove = inputManager.GetMoveWand(PSMoveID);
					if(compassMove)
					{
						updateDifferenceKalman( compassMove.qOrientation.eulerAngles, 
												driftingRot.eulerAngles, deltaT );
					}
				}
				break;
		}
		
		float normalizedT = Mathf.Clamp01(deltaT * 0.1f);
		if(normalizedT != 0)
			finalDifference = Quaternion.Lerp(finalDifference, filteredYawDifference, 
											  normalizedT );
		
		outputDirectionVisualizer.transform.rotation = Quaternion.Euler(
											new Vector3(driftingEuler.x, 
														(360 + driftingEuler.y 
															 - finalDifference.eulerAngles.y)%360, 
														driftingEuler.z));
		//driftingRotation*Quaternion.Inverse(finalDifference);
		outputDirectionVisualizer.transform.position = visualizerPosition.position;
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
            compassDirectionVisualizer.transform.rotation = compassYaw;
			compassDirectionVisualizer.transform.position = visualizerPosition.position;
			inputDirectionVisualizer.transform.position   = visualizerPosition.position;
		
			// Yaw gets unstable when pitch is near poles so disregard those cases
			if(	  (driftingEuler.x < 60 || driftingEuler.x > 300)
			   && ( compassEuler.x < 60 ||  compassEuler.x > 300)) 
			{
				rotationDifference = driftingYaw * Quaternion.Inverse(compassYaw);
				yawDirection = rotationDifference*Vector3.forward;
			
				// 2D vector rotated by yaw difference has continuous components
				measuredVector[0] = yawDirection.x;
				measuredVector[1] = yawDirection.z;
				
				filterRot.setR(deltaT * filterNoiseCovariance);
			    filterRot.predict();
			    filterRot.update(measuredVector);
				filteredVector = filterRot.getState();
				filteredYawDifference = 
								Quaternion.LookRotation(new Vector3((float) filteredVector[0], 0, 
																	(float) filteredVector[1])   );
			}
		}
	}
	
}
