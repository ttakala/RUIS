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
		
		public ulong trackingId = 0;
    }


	NIPlayerManager playerManager;
	RUISInputManager inputManager;
	RUISKinect2Data RUISKinect2Data;

	public readonly int skeletonsHardwareLimit = 4;
	public Skeleton[,] skeletons = new Skeleton[2,6];
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();

    public Vector3 rootSpeedScaling = Vector3.one;

	private static int kinectSensorID = 0;
	private static int kinect2SensorID = 1;

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
					for (int i = 0; i < 6; i++) {
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
						skeletons [kinectSensorID, i].isTracking = playerManager.GetPlayer (i).Tracking;

						if (!skeletons [kinectSensorID, i].isTracking)
								continue;

						UpdateKinectRootData (i);
						UpdateKinectJointData (OpenNI.SkeletonJoint.Head, i, ref skeletons [kinectSensorID, i].head);
						UpdateKinectJointData (OpenNI.SkeletonJoint.Torso, i, ref skeletons [kinectSensorID, i].torso);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons [kinectSensorID, i].leftShoulder);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons [kinectSensorID, i].leftElbow);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftHand, i, ref skeletons [kinectSensorID, i].leftHand);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons [kinectSensorID, i].rightShoulder);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightElbow, i, ref skeletons [kinectSensorID, i].rightElbow);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightHand, i, ref skeletons [kinectSensorID, i].rightHand);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftHip, i, ref skeletons [kinectSensorID, i].leftHip);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons [kinectSensorID, i].leftKnee);
						UpdateKinectJointData (OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons [kinectSensorID, i].leftFoot);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightHip, i, ref skeletons [kinectSensorID, i].rightHip);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightKnee, i, ref skeletons [kinectSensorID, i].rightKnee);
						UpdateKinectJointData (OpenNI.SkeletonJoint.RightFoot, i, ref skeletons [kinectSensorID, i].rightFoot);
					}
			}

		if (inputManager.enableKinect2) {

			Kinect.Body[] data = RUISKinect2Data.getData ();
			
			
			
			foreach (KeyValuePair<ulong, int> item in trackingIDtoIndex)
			{
				print (item.Key + " : " + item.Value);
			}
			
			if (data != null) {
				
				Vector3 relativePos;
				float angleCorrection;
				int playerID;
				int x = 0;
				bool newBody = true;
				int i = 0;
				
				
				// Refresh skeleton tracking status
				for(int y = 0; y < skeletons.GetLength(1); y++) {
					skeletons [kinect2SensorID, y].isTracking = false; 
				}
				//for(int j=0; j<data.Length; ++j)
				//	data.
				
				foreach(var body in data) {
					if(body.IsTracked) {
						for(int y = 0; y < skeletons.GetLength(1); y++) {
							if(skeletons [kinect2SensorID, y].trackingId == body.TrackingId) {
								skeletons [kinect2SensorID, y].isTracking = true;
							}
						}
					}
				}
				
				
				foreach(var body in data) {	
					
					if(i > skeletons.GetLength(1) - 1) break;
					if (body == null) continue;
					newBody = true;
					playerID = 0;
					
					// Check if trackingID has been assigned to certaint index before and use that index
					if(trackingIDtoIndex.ContainsKey(body.TrackingId) && body.IsTracked) {
						playerID = trackingIDtoIndex[body.TrackingId];
						newBody = false;
					} 
					
					if(body.IsTracked) {
						if(newBody) {
							// Find the first unused slot in skeletons array
							for(int y = 0; y < skeletons.GetLength(1); y++) {
								if(!skeletons [kinect2SensorID, y].isTracking) {
									playerID = y;
									break;
								}
							}
						}
						
						// Fore debug
						if(playerID == 0) {
							GameObject.Find ("Kinect 2 text A").GetComponent<TextMesh>().text  = "-----------------" + body.TrackingId + " Index: " + playerID + " i: " + i;
						}
						if(playerID == 1) {
							GameObject.Find ("Kinect 2 text B").GetComponent<TextMesh>().text  = "-----------------" + body.TrackingId + " Index: " + playerID + " i: " + i;
						}
						// end debug
						
						trackingIDtoIndex[body.TrackingId] = playerID;
						skeletons [kinect2SensorID, playerID].trackingId = body.TrackingId;
						

						UpdateKinect2RootData(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), playerID);

						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.Head], body.JointOrientations[Kinect.JointType.Head]), playerID, ref skeletons [kinect2SensorID, playerID].head);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.Neck], body.JointOrientations[Kinect.JointType.Neck]), playerID, ref skeletons [kinect2SensorID, playerID].neck);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), playerID, ref skeletons [kinect2SensorID, playerID].torso);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.SpineMid], body.JointOrientations[Kinect.JointType.SpineMid]), playerID, ref skeletons [kinect2SensorID, playerID].midSpine);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.SpineShoulder], body.JointOrientations[Kinect.JointType.SpineShoulder]), playerID, ref skeletons [kinect2SensorID, playerID].shoulderSpine);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ShoulderLeft], body.JointOrientations[Kinect.JointType.ShoulderLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftShoulder);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ShoulderRight], body.JointOrientations[Kinect.JointType.ShoulderRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightShoulder);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ElbowRight], body.JointOrientations[Kinect.JointType.ElbowRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightElbow);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ElbowLeft], body.JointOrientations[Kinect.JointType.ElbowLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftElbow);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandRight], body.JointOrientations[Kinect.JointType.HandRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHand);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandLeft], body.JointOrientations[Kinect.JointType.HandLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHand);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HipLeft], body.JointOrientations[Kinect.JointType.HipLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHip);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HipRight], body.JointOrientations[Kinect.JointType.HipRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHip);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandTipRight], body.JointOrientations[Kinect.JointType.HandTipRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHandTip);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandTipLeft], body.JointOrientations[Kinect.JointType.HandTipLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHandTip);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.KneeRight], body.JointOrientations[Kinect.JointType.KneeRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightKnee);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.KneeLeft], body.JointOrientations[Kinect.JointType.KneeLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftKnee);
						
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.WristLeft], body.JointOrientations[Kinect.JointType.WristLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftWrist);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.WristRight], body.JointOrientations[Kinect.JointType.WristRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightWrist);
						
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.AnkleLeft], body.JointOrientations[Kinect.JointType.AnkleLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftAnkle);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.AnkleRight], body.JointOrientations[Kinect.JointType.AnkleRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightAnkle);
	
				
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandLeft], body.JointOrientations[Kinect.JointType.HandLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftHand);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.HandRight], body.JointOrientations[Kinect.JointType.HandRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightHand);

						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ThumbLeft], body.JointOrientations[Kinect.JointType.ThumbLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftThumb);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.ThumbRight], body.JointOrientations[Kinect.JointType.ThumbRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightThumb);
						
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.FootLeft], body.JointOrientations[Kinect.JointType.FootLeft]), playerID, ref skeletons [kinect2SensorID, playerID].leftFoot);
						UpdateKinect2JointData(getKinect2JointData(body.Joints[Kinect.JointType.FootRight], body.JointOrientations[Kinect.JointType.FootRight]), playerID, ref skeletons [kinect2SensorID, playerID].rightFoot);
						

						//body.Expressions
						//print(body.Expressions[Kinect.Expression.Happy]);
						
						/*
						 * 	Rotation corrections
						 */

						// Head
						relativePos =  skeletons [kinect2SensorID, playerID].head.position -  skeletons [kinect2SensorID, playerID].neck.position;
						skeletons [kinect2SensorID, playerID].head.rotation = Quaternion.LookRotation(relativePos, Vector3.forward)  * Quaternion.Euler(-90, -90, -90);

						// Torso
						relativePos =  skeletons [kinect2SensorID, playerID].midSpine.position - skeletons [kinect2SensorID, playerID].shoulderSpine.position;
						skeletons [kinect2SensorID, playerID].torso.rotation  = Quaternion.LookRotation(relativePos, skeletons [kinect2SensorID, playerID].midSpine.rotation * Vector3.right) * Quaternion.Euler(0, 90, -90); // TODO: Bug when turning right

						// Upper arm
						relativePos =  skeletons [kinect2SensorID, playerID].leftElbow.position - skeletons [kinect2SensorID, playerID].leftShoulder.position;
						skeletons [kinect2SensorID, playerID].leftShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons [kinect2SensorID, playerID].leftElbow.rotation * Vector3.right) * Quaternion.Euler(-45, 90, 0);

						relativePos = skeletons [kinect2SensorID, playerID].rightElbow.position - skeletons [kinect2SensorID, playerID].rightShoulder.position;
						skeletons [kinect2SensorID, playerID].rightShoulder.rotation = Quaternion.LookRotation(relativePos, skeletons [kinect2SensorID, playerID].rightElbow.rotation * Vector3.right) * Quaternion.Euler(135, 270, 0);

						// Lower arm
						relativePos = skeletons [kinect2SensorID, playerID].leftElbow.position - skeletons [kinect2SensorID, playerID].leftWrist.position;
						skeletons [kinect2SensorID, playerID].leftElbow.rotation = Quaternion.LookRotation(relativePos)  * Quaternion.Euler(0, -90, 0); 
					
						relativePos = skeletons [kinect2SensorID, playerID].rightElbow.position - skeletons [kinect2SensorID, playerID].rightWrist.position;
						skeletons [kinect2SensorID, playerID].rightElbow.rotation = Quaternion.LookRotation(relativePos)  * Quaternion.Euler(0, 90, 0); 
					
						// Upper leg
						skeletons [kinect2SensorID, playerID].leftHip.rotation = skeletons [kinect2SensorID, playerID].leftKnee.rotation * Quaternion.Euler(180, -90, 0);
						skeletons [kinect2SensorID, playerID].rightHip.rotation = skeletons [kinect2SensorID, playerID].rightKnee.rotation * Quaternion.Euler(180, 90, 0);;

						// Lower leg
						skeletons [kinect2SensorID, playerID].leftKnee.rotation = skeletons [kinect2SensorID, playerID].leftAnkle.rotation  * Quaternion.Euler(180, -90, 0);
						skeletons [kinect2SensorID, playerID].rightKnee.rotation = skeletons [kinect2SensorID, playerID].rightAnkle.rotation  * Quaternion.Euler(180, 90, 0);;

						// Hands
						skeletons [kinect2SensorID, playerID].leftHand.rotation *= Quaternion.Euler(0, 180, -90);
						skeletons [kinect2SensorID, playerID].rightHand.rotation *= Quaternion.Euler(0, 180, 90);

						// Fingers
						Vector3 angleFingers = new Vector3(0,0,-120);
						Vector3 angleThumb = new Vector3(0,0,40);

						skeletons [kinect2SensorID, playerID].leftHandClosed = (body.HandLeftState == Kinect.HandState.Closed) ? true : false;
						skeletons [kinect2SensorID, playerID].rightHandClosed = (body.HandRightState == Kinect.HandState.Closed) ? true : false;
						
						// Thumbs
						relativePos = skeletons [kinect2SensorID, playerID].leftThumb.position - skeletons [kinect2SensorID, playerID].leftHand.position;
						skeletons [kinect2SensorID, playerID].leftThumb.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(-90, 0, 0); 
						
						relativePos = skeletons [kinect2SensorID, playerID].rightThumb.position - skeletons [kinect2SensorID, playerID].rightHand.position;
						skeletons [kinect2SensorID, playerID].rightThumb.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(-90, 0, 0); 
						
						// Feets
						relativePos = skeletons [kinect2SensorID, playerID].leftAnkle.position - skeletons [kinect2SensorID, playerID].leftFoot.position;
						skeletons [kinect2SensorID, playerID].leftFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0); 
						
						relativePos = skeletons [kinect2SensorID, playerID].rightAnkle.position - skeletons [kinect2SensorID, playerID].rightFoot.position;
						skeletons [kinect2SensorID, playerID].rightFoot.rotation = Quaternion.LookRotation(relativePos) * Quaternion.Euler(0, 180, 0); 
						
						

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
						i++;
					}
				}
			}
		}
	}
	/*
	 * 	Kinect 1 functions
	 */
    private void UpdateKinectRootData(int player)
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

    private void UpdateKinectJointData(OpenNI.SkeletonJoint joint, int player, ref JointData jointData)
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
	private void UpdateKinect2RootData(JointData torso, int player)
	{
		Vector3 newRootPosition = coordinateSystem.ConvertKinectPosition2(torso.position);
		skeletons [kinect2SensorID, player].root.position = newRootPosition;
		skeletons [kinect2SensorID, player].root.positionConfidence = torso.positionConfidence;
		skeletons [kinect2SensorID, player].root.rotation = coordinateSystem.ConvertKinectRotation2(torso.rotation);
		skeletons [kinect2SensorID, player].root.rotationConfidence = torso.rotationConfidence;
	}
	private void UpdateKinect2JointData(JointData joint, int player, ref JointData jointData)
	{
		jointData.position = coordinateSystem.ConvertKinectPosition2(joint.position);
		jointData.positionConfidence = joint.positionConfidence; 
		jointData.rotation = coordinateSystem.ConvertKinectRotation2(joint.rotation);
		jointData.rotationConfidence = joint.rotationConfidence;
	}


	public JointData GetJointData(Joint joint, int playerID, int bodyTrackingDeviceID)
    {
        if(playerID >= skeletons.Length)
			return null;

        switch (joint)
        {
            case Joint.Root:
                return skeletons[bodyTrackingDeviceID, playerID].root;
            case Joint.Head:
				return skeletons[bodyTrackingDeviceID, playerID].head;
            case Joint.Torso:
				return skeletons[bodyTrackingDeviceID, playerID].torso;
            case Joint.LeftShoulder:
				return skeletons[bodyTrackingDeviceID, playerID].leftShoulder;
            case Joint.LeftElbow:
				return skeletons[bodyTrackingDeviceID, playerID].leftElbow;
            case Joint.LeftHand:
				return skeletons[bodyTrackingDeviceID, playerID].leftHand;
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
