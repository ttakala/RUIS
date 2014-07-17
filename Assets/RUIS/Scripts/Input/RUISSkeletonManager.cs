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

	public bool choice1 = true;
	public bool choice2 = false;
	public bool choice3 = false;
	public bool choice4 = false;
	
	public bool choice5 = false;
	public bool choice6 = false;
	public bool choice7 = false;
	public bool choice8 = false;
	public bool choice9 = false;
	public bool choice10 = false;
	public bool choice11 = false;
	public bool choice12 = false;
	
	public bool choice13 = false;
	public bool choice14 = false;
	public bool choice15 = false;
	public bool choice16 = false;
	
	public bool choice17 = false;
	public bool choice18 = false;
	public bool choice19 = false;
	public bool choice20 = false;
	
	public bool choice21 = false;
	public bool choice22 = false;
	public bool choice23 = false;
	public bool choice24 = false;
	
	public bool choice25 = false;
	public bool choice26 = false;
	public bool choice27 = false;
	public bool choice28 = false;
	
	public bool choice29 = false;
	public bool choice30 = false;
	public bool choice31 = false;
	public bool choice32 = false;
	
	public bool choice33 = false;
	public bool choice34 = false;
	public bool choice35 = false;
	public bool choice36 = false;

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
	public Skeleton[] skeletons = new Skeleton[4];
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    

    public Vector3 rootSpeedScaling = Vector3.one;

    void Awake()
    {
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;

		if (inputManager.enableKinect) playerManager = GetComponent<NIPlayerManager>();
		if(inputManager.enableKinect2) RUISKinect2Data = GetComponent<RUISKinect2Data>();

        if (coordinateSystem == null)
        {
            coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
        }

        for (int i = 0; i < skeletons.Length; i++)
        {
            skeletons[i] = new Skeleton();
        }
    }

    void Start()
    {
        
	}
	
	void Update () {

		if (inputManager.enableKinect) {
			for (int i = 0; i < playerManager.m_MaxNumberOfPlayers; i++) 
					{
						skeletons [i].isTracking = playerManager.GetPlayer (i).Tracking;

						if (!skeletons [i].isTracking)
								continue;

						UpdateRootData (i);
						UpdateJointData (OpenNI.SkeletonJoint.Head, i, ref skeletons [i].head);
						UpdateJointData (OpenNI.SkeletonJoint.Torso, i, ref skeletons [i].torso);
						UpdateJointData (OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons [i].leftShoulder);
						UpdateJointData (OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons [i].leftElbow);
						UpdateJointData (OpenNI.SkeletonJoint.LeftHand, i, ref skeletons [i].leftHand);
						UpdateJointData (OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons [i].rightShoulder);
						UpdateJointData (OpenNI.SkeletonJoint.RightElbow, i, ref skeletons [i].rightElbow);
						UpdateJointData (OpenNI.SkeletonJoint.RightHand, i, ref skeletons [i].rightHand);
						UpdateJointData (OpenNI.SkeletonJoint.LeftHip, i, ref skeletons [i].leftHip);
						UpdateJointData (OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons [i].leftKnee);
						UpdateJointData (OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons [i].leftFoot);
						UpdateJointData (OpenNI.SkeletonJoint.RightHip, i, ref skeletons [i].rightHip);
						UpdateJointData (OpenNI.SkeletonJoint.RightKnee, i, ref skeletons [i].rightKnee);
						UpdateJointData (OpenNI.SkeletonJoint.RightFoot, i, ref skeletons [i].rightFoot);

					}
			}
		else if (inputManager.enableKinect2) {

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
						skeletons [i].isTracking = true;

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
							//print (jt.ToString() + " : " + tempJoint.rotation);
						
							switch(jt.ToString()) {
								
								case "Head":UpdateJointData2(tempJoint, i, ref skeletons[i].head);break;
								case "SpineMid":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.back, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].torso);
									tempJoint.position.z = -tempJoint.position.z;
									UpdateRootData2(tempJoint, i);
									break;
								case "ShoulderLeft":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[i].leftElbow.position;
									newrotation = Quaternion.LookRotation(relativePos);
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.right);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftShoulder);
									break;
								case "ShoulderRight":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[i].rightElbow.position;
									newrotation = Quaternion.LookRotation(relativePos);
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.left);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightShoulder);
									break;
								case "WristLeft":
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftWrist);
									relativePos = skeletons[i].leftHandTip.position -  coordinateSystem.ConvertKinectPosition2(tempJoint.position);
									newrotation = Quaternion.LookRotation(relativePos, Vector3.right);
									//UpdateJointData2(tempJoint, i, ref skeletons[i].leftHand);
									break;
								case "WristRight":
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightWrist);
									relativePos = skeletons[i].leftHandTip.position -  coordinateSystem.ConvertKinectPosition2(tempJoint.position);
									newrotation = Quaternion.LookRotation(relativePos);
									//UpdateJointData2(tempJoint, i, ref skeletons[i].rightHand);
									break;
								case "ElbowLeft":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[i].leftWrist.position;
									newrotation = Quaternion.LookRotation(relativePos);
									tempJoint.rotation = newrotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftElbow);
									break;
								case "ElbowRight":
									relativePos = coordinateSystem.ConvertKinectPosition2(tempJoint.position) - skeletons[i].rightWrist.position;
									newrotation = Quaternion.LookRotation(relativePos);
									tempJoint.rotation = newrotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.up);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightElbow);
									break;
								case "HandLeft":
									tempJoint.rotation = skeletons[i].leftHand.rotation; // Only take position, rotation is handled at Wrist rotation
									//UpdateJointData2(tempJoint, i, ref skeletons[i].leftHand);
									break;
								case "HandRight":
									tempJoint.rotation = skeletons[i].rightHand.rotation; // Only take position, rotation is handled at Wrist rotation
									//UpdateJointData2(tempJoint, i, ref skeletons[i].rightHand);
									break;
								case "HandTipLeft":
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftHandTip);
									break;
								case "HandTipRight":
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightHandTip);
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
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftHip);
									break;
								case "KneeRight":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.forward, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightHip);
									break;
								case "FootLeft":UpdateJointData2(tempJoint, i, ref skeletons[i].leftFoot);break;
								case "FootRight":UpdateJointData2(tempJoint, i, ref skeletons[i].rightFoot);break;
								case "AnkleRight":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.right, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].rightKnee);
									break;
								case "AnkleLeft":
									newrotation = tempJoint.rotation;
									newrotation = newrotation * Quaternion.LookRotation(Vector3.left, Vector3.down);
									tempJoint.rotation = newrotation;
									UpdateJointData2(tempJoint, i, ref skeletons[i].leftKnee);
									break; 
							}
						}
						i++;
					}
					else {
						skeletons [i].isTracking = false;
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
        skeletons[player].root.position = newRootPosition;
        skeletons[player].root.positionConfidence = data.Position.Confidence;
        skeletons[player].root.rotation = coordinateSystem.ConvertKinectRotation(data.Orientation);
        skeletons[player].root.rotationConfidence = data.Orientation.Confidence;
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
		skeletons[player].root.position = newRootPosition;
		skeletons [player].root.positionConfidence = 1; //torso.positionConfidence;
		skeletons[player].root.rotation = coordinateSystem.ConvertKinectRotation2(torso.rotation);
		skeletons [player].root.rotationConfidence = 1; //torso.rotationConfidence;
	}
	private void UpdateJointData2(JointData joint, int player, ref JointData jointData)
	{
		jointData.position = coordinateSystem.ConvertKinectPosition2(joint.position);
		jointData.positionConfidence = 1; //joint.positionConfidence; 
		jointData.rotation = coordinateSystem.ConvertKinectRotation2(joint.rotation);
		jointData.rotationConfidence = 1; //joint.rotationConfidence;
	}


    public JointData GetJointData(Joint joint, int player)
    {
        //if (player >= playerManager.m_MaxNumberOfPlayers)
		if(player >= skeletons.Length)
			return null;

        switch (joint)
        {
            case Joint.Root:
                return skeletons[player].root;
            case Joint.Head:
                return skeletons[player].head;
            case Joint.Torso:
                return skeletons[player].torso;
            case Joint.LeftShoulder:
                return skeletons[player].leftShoulder;
            case Joint.LeftElbow:
                return skeletons[player].leftElbow;
            case Joint.LeftHand:
                return skeletons[player].leftHand;
            case Joint.RightShoulder:
                return skeletons[player].rightShoulder;
            case Joint.RightElbow:
                return skeletons[player].rightElbow;
            case Joint.RightHand:
                return skeletons[player].rightHand;
            case Joint.LeftHip:
                return skeletons[player].leftHip;
            case Joint.LeftKnee:
                return skeletons[player].leftKnee;
            case Joint.LeftFoot:
                return skeletons[player].leftFoot;
            case Joint.RightHip:
                return skeletons[player].rightHip;
            case Joint.RightKnee:
                return skeletons[player].rightKnee;
            case Joint.RightFoot:
                return skeletons[player].rightFoot;
            default:
                return null;
        }
    }
}
