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

					if(body.IsTracked)
					{
						skeletons [1, i].isTracking = true;

						for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
						{	
							JointData tempJoint = new JointData();
							Vector3 relativePos;
							Quaternion newrotation;
							Kinect.Joint p = body.Joints[jt];
							Kinect.JointOrientation o = body.JointOrientations[jt];
							tempJoint.rotation = new Quaternion(o.Orientation.X,o.Orientation.Y,o.Orientation.Z,o.Orientation.W);
							tempJoint.position = new Vector3(p.Position.X, p.Position.Y, p.Position.Z);
							tempJoint.positionConfidence = tempJoint.positionConfidence;
							tempJoint.rotationConfidence = tempJoint.rotationConfidence;

							tempJoint.TrackingState = p.TrackingState;


							if(p.TrackingState == Kinect.TrackingState.Tracked)  {
								tempJoint.positionConfidence = 1.0f;
								tempJoint.rotationConfidence = 1.0f;
							}
							if(p.TrackingState == Kinect.TrackingState.Inferred)  {
								tempJoint.positionConfidence = 0.5f;
								tempJoint.rotationConfidence = 0.5f;
							}
							if(p.TrackingState == Kinect.TrackingState.NotTracked)  {
								tempJoint.positionConfidence = 0.0f;
								tempJoint.rotationConfidence = 0.0f;
							}

							//print (jt.ToString() + " : " + tempJoint.rotation);
						
							switch(jt.ToString()) {
								
								case "Head":UpdateJointData2(tempJoint, i, ref skeletons[1, i].head);break;
								case "SpineMid":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.back, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].torso);
									tempJoint.position.z = -tempJoint.position.z;
									UpdateRootData2(tempJoint, i);
									break;
								case "ShoulderLeft":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[1, i].leftElbow.position;
									newrotation = Quaternion.LookRotation(relativePos);
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.right);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftShoulder);
									break;
								case "ShoulderRight":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[1, i].rightElbow.position;
									newrotation = Quaternion.LookRotation(relativePos);
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.left);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightShoulder);
									break;
								case "WristLeft":
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftWrist);
									relativePos = skeletons[1, i].leftHandTip.position -  coordinateSystem.ConvertKinectPosition2(tempJoint.position);
									newrotation = Quaternion.LookRotation(relativePos, Vector3.right);
									//UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftHand);
									break;
								case "WristRight":
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightWrist);
									relativePos = skeletons[1, i].leftHandTip.position -  coordinateSystem.ConvertKinectPosition2(tempJoint.position);
									newrotation = Quaternion.LookRotation(relativePos);
									//UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightHand);
									break;
								case "ElbowLeft":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[1, i].leftWrist.position;
									newrotation = Quaternion.LookRotation(relativePos);
									tempJoint.rotation = newrotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftElbow);
									break;
								case "ElbowRight":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[1, i].rightWrist.position;
									newrotation = Quaternion.LookRotation(relativePos);
									tempJoint.rotation = newrotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightElbow);
									break;
								case "HandLeft":
									tempJoint.rotation = skeletons[1, i].leftHand.rotation; // Only take position, rotation is handled at Wrist rotation
									//UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftHand);
									break;
								case "HandRight":
									tempJoint.rotation = skeletons[1, i].rightHand.rotation; // Only take position, rotation is handled at Wrist rotation
									//UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightHand);
									break;
								case "HandTipLeft":
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftHandTip);
									break;
								case "HandTipRight":
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightHandTip);
									break;
								case "HipLeft":
									//UpdateJointData2(tempJoint, i, ref skeletons[i].leftHip);
									break;
								case "HipRight":
									//UpdateJointData2(tempJoint, i, ref skeletons[i].rightHip);
									break;
								case "KneeLeft":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.back, Vector3.back);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftHip);
									break;
								case "KneeRight":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.forward, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightHip);
									break;
								case "FootLeft":UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftFoot);break;
								case "FootRight":UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightFoot);break;
								case "AnkleRight":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].rightKnee);
									break;
								case "AnkleLeft":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[1, i].leftKnee);
									break; 
							}
						}
						i++;
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


    public JointData GetJointData(Joint joint, int player)
    {
        //if (player >= playerManager.m_MaxNumberOfPlayers)
		if(player >= skeletons.Length)
			return null;

		int index = 0;
		if (inputManager.enableKinect) index = 0;
		if (inputManager.enableKinect2) index = 1;

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
}
