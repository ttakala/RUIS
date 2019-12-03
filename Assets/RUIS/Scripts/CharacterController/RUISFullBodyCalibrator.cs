using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using CSML;

public class RUISFullBodyCalibrator : MonoBehaviour
{
	public int calibrationSampleCount = 40;

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

		private Vector3 lastLimbStartSample 	= Vector3.zero;
		private Vector3 lastLimbExtremitySample = Vector3.zero;

		private int collectedSamplesCount = 0;

		private Matrix RUMatrix;
		private Matrix deltaPos; // p_s - p_c

		private Matrix HUMatrix;
		private Matrix dotProduct; // e dot (p_h - p_s)

		Quaternion[] samplesR;
		Quaternion[] samplesU;
		Quaternion[] samplesH;

		Vector3[] samplesLimbStartPos; // p_s
		Vector3[] samplesExtremityPos; // p_h
		Vector3[] samplesDeltaPos;     // p_s - p_c

		float[,] samplesHDot;
		float[] samplesDotProduct;

		public void InitializeLimbCalibration(int sampleCount)
		{
			Quaternion[] samplesR = new Quaternion[sampleCount];
			Quaternion[] samplesU = new Quaternion[sampleCount];
			Quaternion[] samplesH = new Quaternion[sampleCount];

			Vector3[] samplesLimbStartPos 	= new Vector3[sampleCount]; // p_s
			Vector3[] samplesExtremityPos 	= new Vector3[sampleCount]; // p_h
			Vector3[] samplesDeltaPos 		= new Vector3[sampleCount]; // p_s - p_c

			float[,] samplesHDot = new float[sampleCount, 6];
			float[] samplesDotProduct = new float[sampleCount];

			collectedSamplesCount = 0;
		}

		public void TrySavingSample(TrackerPose limbBaseTracker, TrackerPose upperLimbTracker, TrackerPose extremityTracker)
		{
			if(!limbBaseTracker.trackerChild || !upperLimbTracker.trackerChild || !extremityTracker.trackerChild)
				return; // How to handle immobile tracker Transforms?
			
			if(collectedSamplesCount < samplesR.Length && false) // Success conditions
			{
				samplesR[collectedSamplesCount] =  limbBaseTracker.trackerChild.rotation;
				samplesU[collectedSamplesCount] = upperLimbTracker.trackerChild.rotation;
				samplesH[collectedSamplesCount] = extremityTracker.trackerChild.rotation;

				samplesLimbStartPos[collectedSamplesCount] = upperLimbTracker.trackerChild.parent.position; // Note parent
				samplesExtremityPos[collectedSamplesCount] = extremityTracker.trackerChild.parent.position; // Note parent
				samplesDeltaPos[collectedSamplesCount] =  upperLimbTracker.trackerChild.parent.position - limbBaseTracker.trackerChild.parent.position; // Note parent: p_s - p_c

				//samplesHDot[collectedSamplesCount][0] = ;
				//samplesHDot[collectedSamplesCount][1] = ;
				//samplesHDot[collectedSamplesCount][2] = ;
				//samplesHDot[collectedSamplesCount][3] = ;
				//samplesHDot[collectedSamplesCount][4] = ;
				//samplesHDot[collectedSamplesCount][5] = ;
				//samplesDotProduct[collectedSamplesCount] = ;

				++collectedSamplesCount;
			}
		}

		private void CalculateTransformation()
		{
			Quaternion tempRotation = Quaternion.identity;
			Vector3 tempVector = Vector3.zero;
			Matrix4x4 tempMatrix = Matrix4x4.identity;

			RUMatrix = Matrix.Zeros(3 * samplesR.Length, 6);
			deltaPos = Matrix.Zeros(3 * samplesR.Length, 1);

			HUMatrix 	= Matrix.Zeros(samplesH.Length, 6);
			dotProduct 	= Matrix.Zeros(samplesH.Length, 1);

			for(int i = 1; i <= samplesDeltaPos.Length; i++) 
			{
				tempMatrix = Matrix4x4.Rotate(samplesR[i - 1]);
				RUMatrix[3*i - 2, 1] = new Complex(tempMatrix.m00);
				RUMatrix[3*i - 2, 2] = new Complex(tempMatrix.m01);
				RUMatrix[3*i - 2, 3] = new Complex(tempMatrix.m02);
				RUMatrix[3*i - 1, 1] = new Complex(tempMatrix.m10);
				RUMatrix[3*i - 1, 2] = new Complex(tempMatrix.m11);
				RUMatrix[3*i - 1, 3] = new Complex(tempMatrix.m12);
				RUMatrix[3*i,	  1] = new Complex(tempMatrix.m20);
				RUMatrix[3*i,	  2] = new Complex(tempMatrix.m21);
				RUMatrix[3*i,	  3] = new Complex(tempMatrix.m22);

				tempMatrix = Matrix4x4.Rotate(samplesU[i - 1]);
				RUMatrix[3*i - 2, 4] = new Complex(tempMatrix.m00);
				RUMatrix[3*i - 2, 5] = new Complex(tempMatrix.m01);
				RUMatrix[3*i - 2, 6] = new Complex(tempMatrix.m02);
				RUMatrix[3*i - 1, 4] = new Complex(tempMatrix.m10);
				RUMatrix[3*i - 1, 5] = new Complex(tempMatrix.m11);
				RUMatrix[3*i - 1, 6] = new Complex(tempMatrix.m12);
				RUMatrix[3*i,	  4] = new Complex(tempMatrix.m20);
				RUMatrix[3*i,	  5] = new Complex(tempMatrix.m21);
				RUMatrix[3*i,	  6] = new Complex(tempMatrix.m22);

				deltaPos[3*i - 2, 1] = new Complex(samplesDeltaPos[i-1].x);
				deltaPos[3*i - 1, 1] = new Complex(samplesDeltaPos[i-1].y);
				deltaPos[3*i,     1] = new Complex(samplesDeltaPos[i-1].z);
			}

			for(int i = 1; i <= samplesDotProduct.Length; i++) 
			{
				HUMatrix[i, 1] = new Complex(samplesHDot[i - 1, 0]);
				HUMatrix[i, 2] = new Complex(samplesHDot[i - 1, 1]);
				HUMatrix[i, 3] = new Complex(samplesHDot[i - 1, 2]);
				HUMatrix[i, 4] = new Complex(samplesHDot[i - 1, 3]);
				HUMatrix[i, 5] = new Complex(samplesHDot[i - 1, 4]);
				HUMatrix[i, 6] = new Complex(samplesHDot[i - 1, 5]);

				dotProduct[i, 1] = new Complex(samplesDotProduct[i - 1]);
			}

			//perform a matrix solve Ax = B. We have to get transposes and inverses because openVrMatrix isn't square
			//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
			Matrix transformMatrixSolution = (RUMatrix.Transpose() * RUMatrix).Inverse() * RUMatrix.Transpose() * deltaPos;

			Matrix error = RUMatrix * transformMatrixSolution - deltaPos;

			transformMatrixSolution = transformMatrixSolution.Transpose();

			Debug.Log(transformMatrixSolution);
			Debug.Log(error);

			// Do something with transformMatrixSolution

			List<Vector3> orthogonalVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(MathUtil.MatrixToMatrix4x4(transformMatrixSolution)));
			//rotationMatrix = CreateRotationMatrix(orthogonalVectors);
			//Debug.Log(rotationMatrix);

			//perform a matrix solve Ax = B. We have to get transposes and inverses because openVrMatrix isn't square
			//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
			transformMatrixSolution = (HUMatrix.Transpose() * HUMatrix).Inverse() * HUMatrix.Transpose() * dotProduct;

			error = HUMatrix * transformMatrixSolution - dotProduct;

			transformMatrixSolution = transformMatrixSolution.Transpose();

			Debug.Log(transformMatrixSolution);
			Debug.Log(error);

			// Do something with transformMatrixSolution

			collectedSamplesCount = 0;
		}

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
		StorePose
	};

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

				vectorX = (rightHand.trackerChild.parent.position - leftHand.trackerChild.parent.position).normalized; // Note parent: Left to right normalized vector
				vectorY = Vector3.Cross(vectorX, Vector3.down); // Forward vector
				firstPoseBodyRotation = Quaternion.LookRotation(vectorY, Vector3.up);

				SetTrackerRotationsFromTPose(pelvis, Quaternion.identity);
				SetTrackerRotationsFromTPose(chest, Quaternion.identity);
				SetTrackerRotationsFromTPose(head, Quaternion.identity);
				SetTrackerRotationsFromTPose(rightShoulder, Quaternion.LookRotation(Vector3.up, Vector3.back));
				SetTrackerRotationsFromTPose(leftShoulder, Quaternion.LookRotation(Vector3.up, Vector3.back));
				SetTrackerRotationsFromTPose(rightHand, Quaternion.LookRotation(Vector3.right, Vector3.back));
				SetTrackerRotationsFromTPose(leftHand, Quaternion.LookRotation(Vector3.right, Vector3.back));
				SetTrackerRotationsFromTPose(rightHip, Quaternion.identity);
				SetTrackerRotationsFromTPose(leftHip, Quaternion.identity);
				SetTrackerRotationsFromTPose(rightFoot, Quaternion.identity);
				SetTrackerRotationsFromTPose(leftFoot, Quaternion.identity);

				SetTrackerRotationsFromTPose(neck, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(rightClavicle, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(leftClavicle, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(rightElbow, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(leftElbow, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(rightKnee, firstPoseBodyRotation);
				SetTrackerRotationsFromTPose(leftKnee, firstPoseBodyRotation);

				vectorX = 0.5f * (rightHand.trackerChild.parent.position + leftHand.trackerChild.parent.position); // Note parent: Hand middlepoint
				vectorY = vectorX - 2 * Vector3.down; // Two meters below hand middlepoint
				// Note parent:
				chest.trackerChild.localPosition 	= -chest.trackerChild.InverseTransformPoint(ProjectPointToLineSegment(chest.trackerChild.parent.position, vectorX, vectorY)); // offset = chest projected to spine
				// Note parent:
				pelvis.trackerChild.localPosition 	= -pelvis.trackerChild.InverseTransformPoint(ProjectPointToLineSegment(pelvis.trackerChild.parent.position, vectorX, vectorY)); // offset = pelvis projected to spine

				Debug.Log("Make a Crane-pose, and press down Menu buttons on both controllers at the same time to enter to the next calibration phase.");
				break;
			case CalibrationState.StorePose:
				calibrationState = CalibrationState.NotCalibrating;
				Debug.Log("Finished Vive Tracker Full Body Calibration, and saved rotation and position offsets to tracker child Transforms.");
				break;
		}
	}

	void SetTrackerRotationsFromTPose(TrackerPose tracker, Quaternion offset)
	{
		// Note parent:
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