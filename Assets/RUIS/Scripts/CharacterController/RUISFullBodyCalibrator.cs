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

	[Serializable]
	public class Limb
	{
		public float upperLimbBoneLength = 0.5f;
		public float lowerLimbBoneLength = 0.5f;
		[Tooltip("Vector from upper arm / thigh tracker to shoulder / hip joint in local coordinates.")]
		public Vector3 limbStartJointOffset = Vector3.zero;
		[Tooltip("Vector from upper arm / thigh tracker to elbow / knee joint in local coordinates.")]
		public Vector3 limbMiddleJointOffset = Vector3.zero;
		[Tooltip("Vector from hand / foot tracker to wrist / ankle joint in local coordinates.")]
		public Vector3 limbEndJointOffset = Vector3.zero;

		// Vector from chest / pelvis tracker to shoulder / hip joint in local coordinates
		private Vector3 bodyToLimbStartOffset = Vector3.zero;

		private bool isRightLimb = true;

		public void SetAsLeftLimb()
		{
			isRightLimb = false;
		}
	}

	[Header("Limbs")]
	public Limb rightArm;
	public Limb leftArm;
	public Limb rightLeg;
	public Limb leftLeg;

	[Header("Trackers")]
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
	Quaternion firstPoseBodyRotation = Quaternion.identity;

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
				if(!rightHand.trackerChild)
				{
					Debug.LogError(rightHand.trackerChild.name + "rightHand.trackerChild must be assigned!");
					failedRequirements = true;
				}
				if(!leftHand.trackerChild)
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

				vectorX = (rightHand.trackerChild.parent.position - leftHand.trackerChild.parent.position).normalized; // Left to right normalized vector
				vectorY = Vector3.Cross(vectorX, Vector3.down); // Forward vector
				firstPoseBodyRotation = Quaternion.LookRotation(vectorY, Vector3.up);

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

				ResetTrackerOffsetsFromTPose(neck, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(rightClavicle, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(leftClavicle, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(rightElbow, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(leftElbow, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(rightKnee, firstPoseBodyRotation);
				ResetTrackerOffsetsFromTPose(leftKnee, firstPoseBodyRotation);

				vectorX = 0.5f * (rightHand.trackerChild.parent.position - leftHand.trackerChild.parent.position); // Hand middlepoint
				vectorY = vectorX - 2 * Vector3.down; // Two meters below hand middlepoint
				chest.trackerChild.localPosition = chest.trackerChild.InverseTransformPoint(ProjectPointToLineSegment(chest.trackerChild.parent.position, vectorX, vectorY)); // offset = chest projected to spine
				pelvis.trackerChild.localPosition = pelvis.trackerChild.InverseTransformPoint(ProjectPointToLineSegment(pelvis.trackerChild.parent.position, vectorX, vectorY)); // offset = pelvis projected to spine

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
	}

	Vector3 ProjectPointToLineSegment(Vector3 p, Vector3 a, Vector3 b)
	{
		// A + dot(AP,AB) / dot(AB,AB) * AB
		return a + (Vector3.Dot(p - a, b - a) / Vector3.Dot(b - a, b - a)) * (b - a);
	}

	void Awake()
	{
		// This might not be the best way to do this
		leftArm.SetAsLeftLimb();
		leftLeg.SetAsLeftLimb();
	}

	// Use this for initialization
	void Start()
	{
		leftStartButton.AddOnStateUpListener(TriggerUp, leftController);
		leftStartButton.AddOnStateDownListener(TriggerDown, leftController);
		rightStartButton.AddOnStateUpListener(TriggerUp, rightController);
		rightStartButton.AddOnStateDownListener(TriggerDown, rightController);
	}

	void DebugDrawTrackerPose(TrackerPose tracker)
	{
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.forward), Color.blue);
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.up), Color.green);
		Debug.DrawLine(tracker.trackerChild.position, tracker.trackerChild.position
			+ 0.1f * (tracker.trackerChild.rotation * Vector3.right), Color.red);
	}

	void DebugDrawTrackedLimb(Limb limb, TrackerPose upperLimb, TrackerPose lowerLimb, TrackerPose extremity, bool isLeg, bool isRightLimb)
	{
		Vector3 boneDirection = Vector3.down; // Upper and lower legs
		Vector3 extremityDirection = Vector3.forward; // Feet

		if(!isLeg)
		{
			if(isRightLimb)
			{
				boneDirection = Vector3.right;
				extremityDirection = Vector3.right;
			}
			else
			{
				boneDirection = Vector3.left;
				extremityDirection = Vector3.left;
			}
		}

		if(upperLimb.trackerChild)
			Debug.DrawLine(upperLimb.trackerChild.position, upperLimb.trackerChild.position
				+ limb.upperLimbBoneLength * (upperLimb.trackerChild.rotation * boneDirection), Color.cyan);
		if(lowerLimb.trackerChild)
			Debug.DrawLine(lowerLimb.trackerChild.position, lowerLimb.trackerChild.position
				+ limb.lowerLimbBoneLength * (lowerLimb.trackerChild.rotation * boneDirection), Color.yellow);
		if(extremity.trackerChild)
			Debug.DrawLine(extremity.trackerChild.position, extremity.trackerChild.position
				+ 0.1f * (extremity.trackerChild.rotation * extremityDirection), Color.magenta);
	}

	// Update is called once per frame
	void Update()
	{
		DebugDrawTrackedLimb(rightArm, 	rightShoulder, 	rightElbow, rightHand, 	isLeg: false, 	isRightLimb: true);
		DebugDrawTrackedLimb(leftArm, 	leftShoulder, 	leftElbow, 	leftHand, 	isLeg: false, 	isRightLimb: false);
		DebugDrawTrackedLimb(rightLeg, 	rightHip, 		rightKnee, 	rightFoot, 	isLeg: true, 	isRightLimb: true);
		DebugDrawTrackedLimb(leftLeg, 	leftHip, 		leftKnee, 	leftHand, 	isLeg: true, 	isRightLimb: false);

		DebugDrawTrackerPose(pelvis);
		DebugDrawTrackerPose(chest);
		DebugDrawTrackerPose(neck);
		DebugDrawTrackerPose(head);
		DebugDrawTrackerPose(rightClavicle);
		DebugDrawTrackerPose(leftClavicle);
		DebugDrawTrackerPose(rightShoulder);
		DebugDrawTrackerPose(leftShoulder);
		DebugDrawTrackerPose(rightElbow);
		DebugDrawTrackerPose(leftElbow);
		DebugDrawTrackerPose(rightHand);
		DebugDrawTrackerPose(leftHand);
		DebugDrawTrackerPose(rightHip);
		DebugDrawTrackerPose(leftHip);
		DebugDrawTrackerPose(rightKnee);
		DebugDrawTrackerPose(leftKnee);
		DebugDrawTrackerPose(rightFoot);
		DebugDrawTrackerPose(leftFoot);
	}
}