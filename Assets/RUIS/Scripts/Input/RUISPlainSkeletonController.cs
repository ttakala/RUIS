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

    public bool updateRootPosition = true;
    public bool updateJointPositions = true;
    public bool updateJointRotations = true;

    public bool useHierarchicalModel = false;

    public float minimumConfidenceToUpdate = 0.5f;

    public float rotationDampening = 15f;

    private Dictionary<Transform, Quaternion> jointInitialRotations;

    public Vector3 rootSpeedScaling = Vector3.one;

    void Awake()
    {
        if (skeletonManager == null)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }

        jointInitialRotations = new Dictionary<Transform, Quaternion>();
    }

    void Start()
    {
        SaveInitialRotation(head);
        SaveInitialRotation(torso);
        SaveInitialRotation(rightShoulder);
        SaveInitialRotation(rightElbow);
        SaveInitialRotation(rightHand);
        SaveInitialRotation(leftShoulder);
        SaveInitialRotation(leftElbow);
        SaveInitialRotation(leftHand);
        SaveInitialRotation(rightHip);
        SaveInitialRotation(rightKnee);
        SaveInitialRotation(rightFoot);
        SaveInitialRotation(leftHip);
        SaveInitialRotation(leftKnee);
        SaveInitialRotation(leftFoot);
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

            if (updateRootPosition)
            {
                transform.position = Vector3.Scale(skeletonPosition, rootSpeedScaling);
            }
        }
	}

    private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        if (updateJointPositions && jointToGet.positionConfidence >= minimumConfidenceToUpdate)
        {
            transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
        }

        if (updateJointRotations && jointToGet.rotationConfidence >= minimumConfidenceToUpdate)
        {
            if (useHierarchicalModel)
            {
                Quaternion newRotation = transform.rotation * jointToGet.rotation * 
                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);

                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDampening);
            }
            else
            {
                transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDampening);
            }
        }
    }

    //gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
    private void UpdateSkeletonPosition()
    {
        if (skeletonManager.skeletons[playerId].torso.positionConfidence >= minimumConfidenceToUpdate)
        {
            skeletonPosition = skeletonManager.skeletons[playerId].torso.position;
        }
    }

    private void SaveInitialRotation(Transform bodyPart)
    {
        if(bodyPart)
            jointInitialRotations[bodyPart] = GetInitialRotation(bodyPart);
    }

    private Quaternion GetInitialRotation(Transform bodyPart)
    {
        return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
    }
}
