using UnityEngine;
using System.Collections;

public class RUISKinectJointFollower : MonoBehaviour {
    public RUISSkeletonManager skeletonManager;
    public int playerId;

    public RUISSkeletonManager.Joint jointToFollow;

    public float minimumConfidenceToUpdate = 0.5f;

    public float positionSmoothing = 5.0f;
    public float rotationSmoothing = 5.0f;

	void Awake () {
        if (skeletonManager == null)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }
	}
	
	void Update () {
        if (!skeletonManager.skeletons[playerId].isTracking) return;

        RUISSkeletonManager.JointData jointData = skeletonManager.GetJointData(jointToFollow, playerId);
        if(jointData.positionConfidence > minimumConfidenceToUpdate)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, jointData.position, positionSmoothing * Time.deltaTime);
        }
        if(jointData.rotationConfidence > minimumConfidenceToUpdate)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, jointData.rotation, rotationSmoothing * Time.deltaTime);
        }
	}
}
