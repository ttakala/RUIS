using UnityEngine;
using System.Collections;

public class RUISSkeletonManager : MonoBehaviour {
    RUISCoordinateSystem coordinateSystem;

    public class JointData
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float positionConfidence = 0.0f;
        public float rotationConfidence = 0.0f;
    }

    public class Skeleton
    {
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
    }

    NIPlayerManager playerManager;

    public Skeleton[] skeletons = new Skeleton[4];

    void Awake()
    {
        playerManager = GetComponent<NIPlayerManager>();

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
        for (int i = 0; i < playerManager.m_MaxNumberOfPlayers; i++)
        {
            UpdateJointData(OpenNI.SkeletonJoint.Head, i, ref skeletons[i].head);
            UpdateJointData(OpenNI.SkeletonJoint.Torso, i, ref skeletons[i].torso);
            UpdateJointData(OpenNI.SkeletonJoint.LeftShoulder, i, ref skeletons[i].leftShoulder);
            UpdateJointData(OpenNI.SkeletonJoint.LeftElbow, i, ref skeletons[i].leftElbow);
            UpdateJointData(OpenNI.SkeletonJoint.LeftHand, i, ref skeletons[i].leftHand);
            UpdateJointData(OpenNI.SkeletonJoint.RightShoulder, i, ref skeletons[i].rightShoulder);
            UpdateJointData(OpenNI.SkeletonJoint.RightElbow, i, ref skeletons[i].rightElbow);
            UpdateJointData(OpenNI.SkeletonJoint.RightHand, i, ref skeletons[i].rightHand);
            UpdateJointData(OpenNI.SkeletonJoint.LeftHip, i, ref skeletons[i].leftHip);
            UpdateJointData(OpenNI.SkeletonJoint.LeftKnee, i, ref skeletons[i].leftKnee);
            UpdateJointData(OpenNI.SkeletonJoint.LeftFoot, i, ref skeletons[i].leftFoot);
            UpdateJointData(OpenNI.SkeletonJoint.RightHip, i, ref skeletons[i].rightHip);
            UpdateJointData(OpenNI.SkeletonJoint.RightKnee, i, ref skeletons[i].rightKnee);
            UpdateJointData(OpenNI.SkeletonJoint.RightFoot, i, ref skeletons[i].rightFoot);
        }
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
}
