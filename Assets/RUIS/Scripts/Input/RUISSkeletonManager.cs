/*****************************************************************************

Content    :   A class to manage Kinect/OpenNI skeleton data
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class RUISSkeletonManager : MonoBehaviour {
    RUISCoordinateSystem coordinateSystem;

    public enum Joint
    {
        Root,
        Head,
        Torso,
        LeftShoulder,
        LeftElbow,
        LeftHand,
        RightShoulder,
        RightElbow,
        RightHand,
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
		public Kinect.TrackingState TrackingState = Kinect.TrackingState.NotTracked;
    }

    public class Skeleton
    {
        public bool isTracking = false;
        public JointData root = new JointData();
        public JointData head = new JointData();
        public JointData torso = new JointData();
        public JointData leftShoulder = new JointData();
        public JointData leftElbow = new JointData();
        public JointData leftHand = new JointData();
        public JointData rightShoulder = new JointData();
        public JointData rightElbow = new JointData();
        public JointData rightHand = new JointData();
        public JointData leftHip = new JointData();
        public JointData leftKnee = new JointData();
        public JointData leftFoot = new JointData();
        public JointData rightHip = new JointData();
        public JointData rightKnee = new JointData();
        public JointData rightFoot = new JointData();

		// Kinect 2 joints
		public JointData baseSpine = new JointData();
		public JointData midSpine = new JointData();
		public JointData shoulderSpine = new JointData();
		public JointData leftWrist = new JointData();
		public JointData rightWrist = new JointData();
		public JointData leftAnkle = new JointData();
		public JointData rightAnkle = new JointData();
		public JointData leftHandTip = new JointData();
		public JointData rightHandTip = new JointData();
		public JointData leftThumb = new JointData();
		public JointData rightThumb = new JointData();
		public JointData neck = new JointData();

		public bool rightHandClosed = false;
		public bool leftHandClosed = false;
    }


	NIPlayerManager playerManager;
	RUISInputManager inputManager;
	RUISKinect2Data RUISKinect2Data;

	public readonly int skeletonsHardwareLimit = 4;
	public Skeleton[,] skeletons = new Skeleton[2,4];
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    

    public Vector3 rootSpeedScaling = Vector3.one;

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

		for (int x = 0; x < 2; x++) {
					for (int i = 0; i < 4; i++) {
							skeletons [x, i] = new Skeleton ();
					}
			}
    }

    void Start()
    {
        
	}
	
	void Update () {

		if (inputManager.enableKinect) {
			for (int i = 0; i < playerManager.m_MaxNumberOfPlayers; i++) 
					{
						skeletons [0, i].isTracking = playerManager.GetPlayer (i).Tracking;

						if (!skeletons [0, i].isTracking)
								continue;

						UpdateRootData (i);
						UpdateJointData (OpenNI.SkeletonJoint.Head, i, ref skeletons [0, i].head);
						UpdateJointData (OpenNI.SkeletonJoint.Torso, i, ref skeletons [0, i].torso);
						UpdateJointData (OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons [0, i].leftShoulder);
						UpdateJointData (OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons [0, i].leftElbow);
						UpdateJointData (OpenNI.SkeletonJoint.LeftHand, i, ref skeletons [0, i].leftHand);
						UpdateJointData (OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons [0, i].rightShoulder);
						UpdateJointData (OpenNI.SkeletonJoint.RightElbow, i, ref skeletons [0, i].rightElbow);
						UpdateJointData (OpenNI.SkeletonJoint.RightHand, i, ref skeletons [0, i].rightHand);
						UpdateJointData (OpenNI.SkeletonJoint.LeftHip, i, ref skeletons [0, i].leftHip);
						UpdateJointData (OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons [0, i].leftKnee);
						UpdateJointData (OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons [0, i].leftFoot);
						UpdateJointData (OpenNI.SkeletonJoint.RightHip, i, ref skeletons [0, i].rightHip);
						UpdateJointData (OpenNI.SkeletonJoint.RightKnee, i, ref skeletons [0, i].rightKnee);
						UpdateJointData (OpenNI.SkeletonJoint.RightFoot, i, ref skeletons [0, i].rightFoot);

					}
			}

		if (inputManager.enableKinect2) {

			Kinect.Body[] data = RUISKinect2Data.getData ();

			if (data != null) {

				int i = 0;

				List<ulong> trackedIds = new List<ulong>();
				foreach(var body in data)
				{
					if (body == null)
					{
						continue;
					}
					
					if(body.IsTracked)
					{
						trackedIds.Add (body.TrackingId);
					}
				}
				
				List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
				
				// First delete untracked bodies
				foreach(ulong trackingId in knownIds)
				{
					if(!trackedIds.Contains(trackingId))
					{
						Destroy(_Bodies[trackingId]);
						_Bodies.Remove(trackingId);
					}
				}


				foreach(var body in data)
				{
					if(i > skeletons.Length - 1) continue;
					if (body == null) continue;
					float angleCorrection;



					if(body.IsTracked)
					{
						skeletons [1, i].isTracking = true;

						UpdateRootData2(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), i);

						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.Head], body.JointOrientations[Kinect.JointType.Head]), i, ref skeletons[1, i].head);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.Neck], body.JointOrientations[Kinect.JointType.Neck]), i, ref skeletons[1, i].neck);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), i, ref skeletons[1, i].torso);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), i, ref skeletons[1, i].midSpine);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.SpineShoulder], body.JointOrientations[Kinect.JointType.SpineShoulder]), i, ref skeletons[1, i].shoulderSpine);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ShoulderLeft], body.JointOrientations[Kinect.JointType.ShoulderLeft]), i, ref skeletons[1, i].leftShoulder);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ShoulderRight], body.JointOrientations[Kinect.JointType.ShoulderRight]), i, ref skeletons[1, i].rightShoulder);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ElbowRight], body.JointOrientations[Kinect.JointType.ElbowRight]), i, ref skeletons[1, i].rightElbow);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ElbowLeft], body.JointOrientations[Kinect.JointType.ElbowLeft]), i, ref skeletons[1, i].leftElbow);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandRight], body.JointOrientations[Kinect.JointType.HandRight]), i, ref skeletons[1, i].rightHand);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandLeft], body.JointOrientations[Kinect.JointType.HandLeft]), i, ref skeletons[1, i].leftHand);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HipLeft], body.JointOrientations[Kinect.JointType.HipLeft]), i, ref skeletons[1, i].leftHip);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HipRight], body.JointOrientations[Kinect.JointType.HipRight]), i, ref skeletons[1, i].rightHip);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandTipRight], body.JointOrientations[Kinect.JointType.HandTipRight]), i, ref skeletons[1, i].rightHandTip);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandTipLeft], body.JointOrientations[Kinect.JointType.HandTipLeft]), i, ref skeletons[1, i].leftHandTip);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.KneeRight], body.JointOrientations[Kinect.JointType.KneeRight]), i, ref skeletons[1, i].rightKnee);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.KneeLeft], body.JointOrientations[Kinect.JointType.KneeLeft]), i, ref skeletons[1, i].leftKnee);
						
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.WristLeft], body.JointOrientations[Kinect.JointType.WristLeft]), i, ref skeletons[1, i].leftWrist);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.WristRight], body.JointOrientations[Kinect.JointType.WristRight]), i, ref skeletons[1, i].rightWrist);
						
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.AnkleLeft], body.JointOrientations[Kinect.JointType.AnkleLeft]), i, ref skeletons[1, i].leftAnkle);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.AnkleRight], body.JointOrientations[Kinect.JointType.AnkleRight]), i, ref skeletons[1, i].rightAnkle);
	
				
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandLeft], body.JointOrientations[Kinect.JointType.HandLeft]), i, ref skeletons[1, i].leftHand);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.HandRight], body.JointOrientations[Kinect.JointType.HandRight]), i, ref skeletons[1, i].rightHand);

						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ThumbLeft], body.JointOrientations[Kinect.JointType.ThumbLeft]), i, ref skeletons[1, i].leftThumb);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.ThumbRight], body.JointOrientations[Kinect.JointType.ThumbRight]), i, ref skeletons[1, i].rightThumb);
						
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.FootLeft], body.JointOrientations[Kinect.JointType.FootLeft]), i, ref skeletons[1, i].leftFoot);
						UpdateJointData2(getKinect2JointData(body.Joints[Kinect.JointType.FootRight], body.JointOrientations[Kinect.JointType.FootRight]), i, ref skeletons[1, i].rightFoot);
						

						//body.Expressions
						//print(body.Expressions[Kinect.Expression.Happy]);
						Vector3 relativePos;
						/*
						 * 	Rotation corrections
						 */

						// Head
						relativePos =  skeletons[1, i].head.position -  skeletons[1, i].neck.position;
						skeletons[1, i].head.rotation = Quaternion.LookRotation(relativePos, Vector3.forward)  * Quaternion.Euler(-90, -90, -90);

						// Torso
						relativePos =  skeletons[1, i].midSpine.position - skeletons[1, i].shoulderSpine.position;
						skeletons[1, i].torso.rotation  = Quaternion.LookRotation(relativePos, skeletons[1, i].midSpine.rotation * Vector3.right) * Quaternion.Euler(0, 90, -90); // TODO: Bug when turning right

						// Upper arm
						relativePos =  skeletons[1, i].leftElbow.position - skeletons[1, i].leftShoulder.position;
						skeletons[1, i].leftShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons[1, i].leftElbow.rotation * Vector3.right) * Quaternion.Euler(-45, 90, 0);

						relativePos = skeletons[1, i].rightElbow.position - skeletons[1, i].rightShoulder.position;
						skeletons[1, i].rightShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons[1, i].rightElbow.rotation * Vector3.right) * Quaternion.Euler(135, 270, 0);

						// Lower arm
						relativePos = skeletons[1, i].leftElbow.position - skeletons[1, i].leftWrist.position;
						skeletons[1, i].leftElbow.rotation = Quaternion.LookRotation(relativePos)  * Quaternion.Euler(0, -90, 0); 
					
						relativePos = skeletons[1, i].rightElbow.position - skeletons[1, i].rightWrist.position;
						skeletons[1, i].rightElbow.rotation = Quaternion.LookRotation(relativePos)  * Quaternion.Euler(0, 90, 0); 
					
						// Upper leg
						skeletons[1, i].leftHip.rotation = skeletons[1, i].leftKnee.rotation * Quaternion.Euler(180, -90, 0);
						skeletons[1, i].rightHip.rotation = skeletons[1, i].rightKnee.rotation * Quaternion.Euler(180, 90, 0);;

						// Lower leg
						skeletons[1, i].leftKnee.rotation = skeletons[1, i].leftAnkle.rotation  * Quaternion.Euler(180, -90, 0);
						skeletons[1, i].rightKnee.rotation = skeletons[1, i].rightAnkle.rotation  * Quaternion.Euler(180, 90, 0);;

						// Hands
						skeletons[1, i].leftHand.rotation *= Quaternion.Euler(0, 180, -90);
						skeletons[1, i].rightHand.rotation *= Quaternion.Euler(0, 180, 90);

						// Fingers
						Vector3 angleFingers = new Vector3(0,0,-120);
						Vector3 angleThumb = new Vector3(0,0,40);

						skeletons[1, i].leftHandClosed = (body.HandLeftState == Kinect.HandState.Closed) ? true : false;
						skeletons[1, i].rightHandClosed = (body.HandRightState == Kinect.HandState.Closed) ? true : false;
						
						// Thumbs
						relativePos = skeletons[1, i].leftThumb.position - skeletons[1, i].leftHand.position;
						skeletons[1, i].leftThumb.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(-90, 0, 0); 
						
						relativePos = skeletons[1, i].rightThumb.position - skeletons[1, i].rightHand.position;
						skeletons[1, i].rightThumb.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(-90, 0, 0); 
						
						// Feets
						relativePos = skeletons[1, i].leftAnkle.position - skeletons[1, i].leftFoot.position;
						skeletons[1, i].leftFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0); 
						
						relativePos = skeletons[1, i].rightAnkle.position - skeletons[1, i].rightFoot.position;
						skeletons[1, i].rightFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0); 
						
						i++;

						// Debug kinect detecion area
						/*
						Vector3 origPosition = GameObject.Find ("z1Wall").transform.position;
						origPosition.y = 0;
						GameObject mechanim = GameObject.Find("Mecanim - Kinect 2");
						for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
						{
							if(body.Joints[jt].TrackingState == Kinect.TrackingState.Tracked) {

								GameObject x1Wall = GameObject.Find ("x1Wall");
								GameObject x2Wall = GameObject.Find ("x2Wall");
								GameObject z2Wall = GameObject.Find ("z2Wall");

								float currentX1 = x1Wall.transform.position.x - mechanim.transform.position.x;
								//float currentY1 = GameObject.Find ("y1Wall").transform.position.y - mechanim.transform.position.y;
								//float currentZ1 = GameObject.Find ("z1Wall").transform.position.z - mechanim.transform.position.z;

								float currentX2 = x2Wall.transform.position.x - mechanim.transform.position.x;
								//float currentY2 = GameObject.Find ("y2Wall").transform.position.y - mechanim.transform.position.y;
								float currentZ2 = z2Wall.transform.position.z - mechanim.transform.position.z;

								if(body.Joints[jt].Position.X > currentX1) x1Wall.transform.position = new Vector3(body.Joints[jt].Position.X + mechanim.transform.position.x,mechanim.transform.position.y,-body.Joints[jt].Position.Z  + mechanim.transform.position.z);
							//	if(body.Joints[jt].Position.Y > currentY1) GameObject.Find ("y1Wall").transform.position = new Vector3(mechanim.transform.position.x,body.Joints[jt].Position.Y + mechanim.transform.position.y,mechanim.transform.position.z);
							//	if(-body.Joints[jt].Position.Z > currentZ1) GameObject.Find ("z1Wall").transform.position = new Vector3(mechanim.transform.position.x,mechanim.transform.position.y,-body.Joints[jt].Position.Z + mechanim.transform.position.z);

								if(body.Joints[jt].Position.X < currentX2) x2Wall.transform.position = new Vector3(body.Joints[jt].Position.X + mechanim.transform.position.x,mechanim.transform.position.y,-body.Joints[jt].Position.Z + mechanim.transform.position.z);
							//	if(body.Joints[jt].Position.Y < currentY2) GameObject.Find ("y2Wall").transform.position = new Vector3(mechanim.transform.position.x,body.Joints[jt].Position.Y + mechanim.transform.position.y,mechanim.transform.position.z);
								if(-body.Joints[jt].Position.Z < currentZ2) z2Wall.transform.position = new Vector3(mechanim.transform.position.x,mechanim.transform.position.y,-body.Joints[jt].Position.Z + mechanim.transform.position.z);

								Vector3 leftPoint = new Vector3(x1Wall.transform.position.x, 0, x1Wall.transform.position.z);
								Vector3 rightPoint = new Vector3(x2Wall.transform.position.x, 0, x2Wall.transform.position.z);
								Vector3 leftVector = leftPoint - origPosition;
								Vector3 rightVector = rightPoint - origPosition;


								Quaternion angleLeft = Quaternion.LookRotation(leftVector, Vector3.up);
								Quaternion angleRight = Quaternion.LookRotation(rightVector, Vector3.up);

								x1Wall.transform.rotation = angleLeft;
								x2Wall.transform.rotation = angleRight;
							}
						}
						// end debug
						*/

					}
					else {
						skeletons [1, i].isTracking = false;
					}

				}
			}
		}
	}
	/*
	 * 	Kinect 1 functions
	 */
    private void UpdateRootData(int player)
    {
        OpenNI.SkeletonJointTransformation data;

        if (!playerManager.GetPlayer(player).GetSkeletonJoint(OpenNI.SkeletonJoint.Torso, out data))
        {
            return;
        }

		Vector3 newRootPosition = coordinateSystem.ConvertKinectPosition(data.Position.Position);
        newRootPosition = Vector3.Scale(newRootPosition, rootSpeedScaling);
        skeletons[0, player].root.position = newRootPosition;
		skeletons[0, player].root.positionConfidence = data.Position.Confidence;
		skeletons[0, player].root.rotation = coordinateSystem.ConvertKinectRotation(data.Orientation);
		skeletons[0, player].root.rotationConfidence = data.Orientation.Confidence;
    }

    private void UpdateJointData(OpenNI.SkeletonJoint joint, int player, ref JointData jointData)
    {
        OpenNI.SkeletonJointTransformation data;

        if (!playerManager.GetPlayer(player).GetSkeletonJoint(joint, out data))
        {
            return;
        }

		jointData.position = coordinateSystem.ConvertKinectPosition(data.Position.Position);
        jointData.positionConfidence = data.Position.Confidence;
        jointData.rotation = coordinateSystem.ConvertKinectRotation(data.Orientation);
        jointData.rotationConfidence = data.Orientation.Confidence;
    }
	/*
	 * 	Kinect 2 functions
	 */
	private void UpdateRootData2(JointData torso, int player)
	{
		Vector3 newRootPosition = coordinateSystem.ConvertKinectPosition2(torso.position);
		skeletons[1, player].root.position = newRootPosition;
		skeletons[1, player].root.positionConfidence = torso.positionConfidence;
		skeletons[1, player].root.rotation = coordinateSystem.ConvertKinectRotation2(torso.rotation);
		skeletons[1, player].root.rotationConfidence = torso.rotationConfidence;
	}
	private void UpdateJointData2(JointData joint, int player, ref JointData jointData)
	{
		jointData.position = coordinateSystem.ConvertKinectPosition2(joint.position);
		jointData.positionConfidence = joint.positionConfidence; 
		jointData.rotation = coordinateSystem.ConvertKinectRotation2(joint.rotation);
		jointData.rotationConfidence = joint.rotationConfidence;
	}


	public JointData GetJointData(Joint joint, int player, int kinectVersion)
    {
        //if (player >= playerManager.m_MaxNumberOfPlayers)
		if(player >= skeletons.Length)
			return null;

		int index = kinectVersion;
	
        switch (joint)
        {
            case Joint.Root:
                return skeletons[index, player].root;
            case Joint.Head:
				return skeletons[index, player].head;
            case Joint.Torso:
				return skeletons[index, player].torso;
            case Joint.LeftShoulder:
				return skeletons[index, player].leftShoulder;
            case Joint.LeftElbow:
				return skeletons[index, player].leftElbow;
            case Joint.LeftHand:
				return skeletons[index, player].leftHand;
            case Joint.RightShoulder:
				return skeletons[index, player].rightShoulder;
            case Joint.RightElbow:
				return skeletons[index, player].rightElbow;
            case Joint.RightHand:
				return skeletons[index, player].rightHand;
            case Joint.LeftHip:
				return skeletons[index, player].leftHip;
            case Joint.LeftKnee:
				return skeletons[index, player].leftKnee;
            case Joint.LeftFoot:
				return skeletons[index, player].leftFoot;
            case Joint.RightHip:
				return skeletons[index, player].rightHip;
            case Joint.RightKnee:
				return skeletons[index, player].rightKnee;
            case Joint.RightFoot:
				return skeletons[index, player].rightFoot;
            default:
                return null;
        }
    }

	public JointData getKinect2JointData(Kinect.Joint jointPosition, Kinect.JointOrientation jointRotation) {
		JointData jointData = new JointData();
		jointData.rotation = new Quaternion(jointRotation.Orientation.X,jointRotation.Orientation.Y,jointRotation.Orientation.Z,jointRotation.Orientation.W);
		jointData.position = new Vector3(jointPosition.Position.X, jointPosition.Position.Y, jointPosition.Position.Z);

		if(jointPosition.TrackingState == Kinect.TrackingState.Tracked)  {
			jointData.positionConfidence = 1.0f;
			jointData.rotationConfidence = 1.0f;
		}
		else if(jointPosition.TrackingState == Kinect.TrackingState.Inferred)  {
			jointData.positionConfidence = 0.5f;
			jointData.rotationConfidence = 0.5f;
		}
		else if(jointPosition.TrackingState == Kinect.TrackingState.NotTracked)  {
			jointData.positionConfidence = 0.0f;
			jointData.rotationConfidence = 0.0f;
		}
		else {
			jointData.positionConfidence = 0.0f;
			jointData.rotationConfidence = 0.0f;
		}
		jointData.TrackingState = jointPosition.TrackingState;



		return jointData;
	}
}
