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
		public Vector3 manualPositionOffset = Vector3.zero;
	}
		
	public TrackerPose pelvis;
	public TrackerPose chest;
	public TrackerPose neck;
	public TrackerPose head;
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

	enum CalibrationState
	{
		NotCalibrating,
		GetReady,
		StorePose};

	CalibrationState calibrationState = CalibrationState.NotCalibrating;

	// Reference to the actions
	public SteamVR_Action_Boolean rightStartButton;
	public SteamVR_Action_Boolean leftStartButton;

	// References to the controllers
	public SteamVR_Input_Sources rightController;
	public SteamVR_Input_Sources leftController;

	bool rightMenuDown = false;
	bool leftMenuDown = false;

	Vector3 vectorX = Vector3.zero;
	Vector3 vectorY = Vector3.zero;
	Quaternion tempRotation = Quaternion.identity;

	public void TriggerUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		if(fromSource == rightController)
		{
			rightMenuDown = false;
			//            Debug.Log("Right menu is up");
		}
		else if(fromSource == leftController)
			{
				leftMenuDown = false;
				//            Debug.Log("Left menu is up");
			}
	}

	public void TriggerDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
	{
		if(fromSource == rightController)
		{
			rightMenuDown = true;
			if(leftMenuDown)
				NextStateDown();
			//            Debug.Log("Right menu is down");
		}
		else if(fromSource == leftController)
			{
				leftMenuDown = true;
				if(rightMenuDown)
					NextStateDown();
				//            Debug.Log("Left menu is down");
			}
	}

	public void NextStateDown()
	{
		switch(calibrationState)
		{
			case CalibrationState.NotCalibrating:
				bool failedRequirements = false;
				if(rightHand == null || !rightHand.trackerChild)
				{
					Debug.LogError(rightHand.trackerChild.name + "rightHand.trackerChild must be assigned!");
					failedRequirements = true;
				}
				if(leftHand == null || !leftHand.trackerChild)
				{
					Debug.LogError(rightHand.trackerChild.name + "leftHand.trackerChild must be assigned!");
					failedRequirements = true;
				}
				// TODO: same null check for all the required inputs

				if(!failedRequirements)
				{
					calibrationState = CalibrationState.GetReady;
					Debug.Log("Started Vive Tracker Full Body Calibration. Make a hands up pose with elbows and shoulders on the same horizontal line, forearms "
					+ "pointing straight up, palms facing forward, legs straight, and toes pointing forward. Then press down Joystick (" + rightStartButton.GetShortName()
					+ ") buttons on both controllers at the same time to enter to the next calibration phase.");
				}
				break;
			case CalibrationState.GetReady:
				calibrationState = CalibrationState.StorePose;

				vectorX = 0.5f * (rightHand.trackerChild.position - leftHand.trackerChild.position); // Hand middlepoint
				vectorY = vectorX - 2 * Vector3.down; // Two meters below hand middlepoint
				chest.manualPositionOffset = ProjectPointToLineSegment(chest.trackerChild.position, vectorX, vectorY) - chest.trackerChild.position; // offset = chest projected to spine - chest tracker position
				pelvis.manualPositionOffset = ProjectPointToLineSegment(pelvis.trackerChild.position, vectorX, vectorY) - pelvis.trackerChild.position; // offset = pelvis projected to spine - pelvis tracker position

				vectorX = (rightHand.trackerChild.position - leftHand.trackerChild.position).normalized; // Left to right normalized vector
				vectorY = Vector3.Cross(vectorX, Vector3.down); // Forward vector
				tempRotation = Quaternion.LookRotation(vectorY, Vector3.up);

				ResetTrackerOffsetsFromTPose(pelvis, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(chest, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(head, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(rightShoulder, Quaternion.LookRotation(Vector3.up, Vector3.back));
				ResetTrackerOffsetsFromTPose(leftShoulder, Quaternion.LookRotation(Vector3.up, Vector3.back));
				ResetTrackerOffsetsFromTPose(rightHand, Quaternion.LookRotation(Vector3.right, Vector3.back));
				ResetTrackerOffsetsFromTPose(leftHand, Quaternion.LookRotation(Vector3.right, Vector3.back));
				ResetTrackerOffsetsFromTPose(rightHip, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(leftHip, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(rightFoot, Quaternion.identity);
				ResetTrackerOffsetsFromTPose(leftFoot, Quaternion.identity);

				ResetTrackerOffsetsFromTPose(neck, tempRotation);
				ResetTrackerOffsetsFromTPose(rightClavicle, tempRotation);
				ResetTrackerOffsetsFromTPose(leftClavicle, tempRotation);
				ResetTrackerOffsetsFromTPose(rightElbow, tempRotation);
				ResetTrackerOffsetsFromTPose(leftElbow, tempRotation);
				ResetTrackerOffsetsFromTPose(rightKnee, tempRotation);
				ResetTrackerOffsetsFromTPose(leftKnee, tempRotation);

				Debug.Log("Make a Crane-pose, and press down Menu buttons on both controllers at the same time to enter to the next calibration phase.");
				break;
			case CalibrationState.StorePose:
				calibrationState = CalibrationState.NotCalibrating;
				Debug.Log("Finished Vive Tracker Full Body Calibration, and saved rotation and position offsets to tracker child Transforms.");
				break;
		}
	}

	void ResetTrackerOffsetsFromTPose(TrackerPose tracker, Quaternion offset)
	{
		tracker.trackerChild.localRotation = Quaternion.Inverse(tracker.trackerChild.parent.rotation) * offset;
		// tracker.trackerChild.localRotation = tracker.trackerChild.parent.rotation.inverse * bodyOrientation;
		tracker.trackerChild.localPosition = tracker.manualPositionOffset;
	}

	Vector3 ProjectPointToLineSegment(Vector3 p, Vector3 a, Vector3 b)
	{
		// A + dot(AP,AB) / dot(AB,AB) * AB
		return a + (Vector3.Dot(p - a, b - a) / Vector3.Dot(b - a, b - a)) * (b - a);
	}

	// Use this for initialization
	void Start()
	{
		leftStartButton.AddOnStateUpListener(TriggerUp, leftController);
		leftStartButton.AddOnStateDownListener(TriggerDown, leftController);
		rightStartButton.AddOnStateUpListener(TriggerUp, rightController);
		rightStartButton.AddOnStateDownListener(TriggerDown, rightController);
	}

	void DebugDrawRotation(TrackerPose tracker)
	{
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position 
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.forward), Color.blue);
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position 
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.up), Color.green);
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position 
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.right), Color.red);
	}

	// Update is called once per frame
	void Update()
	{
		//        print(calibrationState);
	}
}
