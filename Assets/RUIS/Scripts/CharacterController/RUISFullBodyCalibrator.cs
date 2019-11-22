using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class RUISFullBodyCalibrator : MonoBehaviour
{

	[Serializable]
	public class TrackerPose
	{
		public Transform trackerChild;
		public bool inferPose = false;
		public Transform targetJoint;
	}

	public TrackerPose head;
	public TrackerPose neck;
	public TrackerPose chest;
	public TrackerPose pelvis;
	public TrackerPose rightClavicle;
	public TrackerPose leftClavicle;
	public TrackerPose rightShoulder;
	public TrackerPose leftShoulder;
	public TrackerPose rightElbow;
	public TrackerPose leftElbow;
	public TrackerPose rightHand;
	public TrackerPose leftHand;
	public TrackerPose rightHip;
	public TrackerPose leftHip;
	public TrackerPose rightKnee;
	public TrackerPose leftKnee;
	public TrackerPose rightFoot;
	public TrackerPose leftFoot;

	enum CalibrationState {NotCalibrating, TPose, CranePose};
	CalibrationState calibrationState = CalibrationState.NotCalibrating;

	// Reference to the actions
	public SteamVR_Action_Boolean rightStartButton;
	public SteamVR_Action_Boolean leftStartButton;

	// References to the controllers
	public SteamVR_Input_Sources rightController;
	public SteamVR_Input_Sources leftController;

	bool rightMenuDown = false;
	bool leftMenuDown  = false;

	public void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		if(fromSource == rightController)
		{
			rightMenuDown = false;
//			Debug.Log("Right menu is up");
		}
		else if(fromSource == leftController)
		{
			leftMenuDown = false;
//			Debug.Log("Left menu is up");
		}
	}

	public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		if(fromSource == rightController)
		{
			rightMenuDown = true;
			if(leftMenuDown)
				NextStateDown();
//			Debug.Log("Right menu is down");
		}
		else if(fromSource == leftController)
		{
			leftMenuDown = true;
			if(rightMenuDown)
				NextStateDown();
//			Debug.Log("Left menu is down");
		}
	}

	public void NextStateDown()
	{
		switch(calibrationState)
		{
			case CalibrationState.NotCalibrating:
				calibrationState = CalibrationState.TPose;
				Debug.Log("Started Vive Tracker Full Body Calibration. Make a T-pose, and press down Menu buttons on both controllers at the same time to enter to the next calibration phase.");
				break;
			case CalibrationState.TPose:
				calibrationState = CalibrationState.CranePose;
				Debug.Log("Make a Crane-pose, and press down Menu buttons on both controllers at the same time to enter to the next calibration phase.");
				break;
			case CalibrationState.CranePose:
				calibrationState = CalibrationState.NotCalibrating;
				Debug.Log("Finished Vive Tracker Full Body Calibration, and saved rotation and position offsets to tracker child Transforms.");
				break;
		}
	}

	// Use this for initialization
	void Start()
	{
		leftStartButton.AddOnStateUpListener(	TriggerUp, 		leftController);
		leftStartButton.AddOnStateDownListener(	TriggerDown, 	leftController);
		rightStartButton.AddOnStateUpListener(	TriggerUp, 		rightController);
		rightStartButton.AddOnStateDownListener(TriggerDown, 	rightController);
	}

	// Update is called once per frame
	void Update()
	{
//		print(calibrationState);
	}
}