using UnityEngine;
using System.Collections.Generic;

public class RUISPlainSkeletonController : MonoBehaviour {
    public Transform head;
    public Transform torso;
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightHand;
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightFoot;
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftHand;
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftFoot;

    public RUISSkeletonManager skeletonManager;

    public int playerId = 0;

    private Vector3 skeletonPosition = Vector3.zero;

    void Awake()
    {
        if (skeletonManager == null)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }
    }

	void LateUpdate () {
        if (skeletonManager != null && skeletonManager.skeletons[playerId] != null)
        {
            UpdateSkeletonPosition();

            UpdateTransform(ref head, skeletonManager.skeletons[playerId].head);
            UpdateTransform(ref torso, skeletonManager.skeletons[playerId].torso);
            UpdateTransform(ref leftShoulder, skeletonManager.skeletons[playerId].leftShoulder);
            UpdateTransform(ref leftElbow, skeletonManager.skeletons[playerId].leftElbow);
            UpdateTransform(ref leftHand, skeletonManager.skeletons[playerId].leftHand);
            UpdateTransform(ref rightShoulder, skeletonManager.skeletons[playerId].rightShoulder);
            UpdateTransform(ref rightElbow, skeletonManager.skeletons[playerId].rightElbow);
            UpdateTransform(ref rightHand, skeletonManager.skeletons[playerId].rightHand);
            UpdateTransform(ref leftHip, skeletonManager.skeletons[playerId].leftHip);
            UpdateTransform(ref leftKnee, skeletonManager.skeletons[playerId].leftKnee);
            UpdateTransform(ref leftFoot, skeletonManager.skeletons[playerId].leftFoot);
            UpdateTransform(ref rightHip, skeletonManager.skeletons[playerId].rightHip);
            UpdateTransform(ref rightKnee, skeletonManager.skeletons[playerId].rightKnee);
            UpdateTransform(ref rightFoot, skeletonManager.skeletons[playerId].rightFoot);


            if (leftHand != null)
            {
                leftHand.localRotation = leftElbow.localRotation;
            }

            if (rightHand != null)
            {
                rightHand.localRotation = rightElbow.localRotation;
            }

            transform.position = skeletonPosition;
        }
	}

    private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        if (jointToGet.positionConfidence >= 0.5f)
        {
            transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
        }

        if (jointToGet.rotationConfidence >= 0.5f)
        {
            transformToUpdate.localRotation = jointToGet.rotation;
        }
    }

    //gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
    private void UpdateSkeletonPosition()
    {
        if (skeletonManager.skeletons[playerId].torso.positionConfidence <= 0.5f)
        {
            skeletonPosition = skeletonManager.skeletons[playerId].torso.position;
        }
    }
}
