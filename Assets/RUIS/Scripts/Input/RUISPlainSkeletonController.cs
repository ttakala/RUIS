/*****************************************************************************

Content    :   Functionality to control a skeleton using Kinect
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISPlainSkeletonController")]
public class RUISPlainSkeletonController : MonoBehaviour
{
    public Transform root;
    public Transform head;
    public Transform neck;
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
    public bool scaleHierarchicalModelBones = true;
    public float maxScaleFactor = 0.01f;

    public float minimumConfidenceToUpdate = 0.5f;

    public float rotationDamping = 15f;

    private Dictionary<Transform, Quaternion> jointInitialRotations;
    private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;

    [HideInInspector]
    public float torsoOffset = 0.0f;
    [HideInInspector]
    public float torsoScale = 1.0f;

    public float neckHeightTweaker = 0.0f;
    public float forearmLengthRatio = 1.0f;
    private Vector3 neckOriginalLocalPosition;

    void Awake()
    {
        if (skeletonManager == null)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }

        jointInitialRotations = new Dictionary<Transform, Quaternion>();
        jointInitialDistances = new Dictionary<KeyValuePair<Transform, Transform>, float>();
    }

    void Start()
    {
        if (useHierarchicalModel)
        {
            rightShoulder.rotation = FindFixingRotation(rightShoulder.position, rightElbow.position, transform.right) * rightShoulder.rotation;
            leftShoulder.rotation = FindFixingRotation(leftShoulder.position, leftElbow.position, -transform.right) * leftShoulder.rotation;
            rightHip.rotation = FindFixingRotation(rightHip.position, rightFoot.position, -transform.up) * rightHip.rotation;
            leftHip.rotation = FindFixingRotation(leftHip.position, leftFoot.position, -transform.up) * leftHip.rotation;

            Vector3 assumedRootPos = (rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4;
            Vector3 realRootPos = torso.position;
            torsoOffset = (realRootPos - assumedRootPos).y;

            if (neck)
            {
                neckOriginalLocalPosition = neck.localPosition;
            }
        }

        SaveInitialRotation(root);
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

        SaveInitialDistance(rightShoulder, rightElbow);
        SaveInitialDistance(rightElbow, rightHand);
        SaveInitialDistance(leftShoulder, leftElbow);
        SaveInitialDistance(leftElbow, leftHand);

        SaveInitialDistance(rightHip, rightKnee);
        SaveInitialDistance(rightKnee, rightFoot);
        SaveInitialDistance(leftHip, leftKnee);
        SaveInitialDistance(leftKnee, leftFoot);

        SaveInitialDistance(torso, head);

        SaveInitialDistance(rightShoulder, leftShoulder);
        SaveInitialDistance(rightHip, leftHip);
    }




    void LateUpdate()
    {
        if (skeletonManager != null && skeletonManager.skeletons[playerId] != null && skeletonManager.skeletons[playerId].isTracking)
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

            if (!useHierarchicalModel)
            {
                if (leftHand != null)
                {
                    leftHand.localRotation = leftElbow.localRotation;
                }

                if (rightHand != null)
                {
                    rightHand.localRotation = rightElbow.localRotation;
                }
            }
            else
            {
                if (scaleHierarchicalModelBones)
                {
                    UpdateBoneScalings();

                    Vector3 torsoDirection = skeletonManager.skeletons[playerId].torso.rotation * Vector3.down;
                    torso.position = transform.TransformPoint(skeletonManager.skeletons[playerId].torso.position - skeletonPosition - torsoDirection * torsoOffset * torsoScale);
                    
                    ForceUpdatePosition(ref rightShoulder, skeletonManager.skeletons[playerId].rightShoulder);
                    ForceUpdatePosition(ref leftShoulder, skeletonManager.skeletons[playerId].leftShoulder);
                    ForceUpdatePosition(ref rightHip, skeletonManager.skeletons[playerId].rightHip);
                    ForceUpdatePosition(ref leftHip, skeletonManager.skeletons[playerId].leftHip);
                }
            }

            if (updateRootPosition)
            {
                Vector3 newRootPosition = skeletonManager.skeletons[playerId].root.position;
                transform.localPosition = newRootPosition;
            }

        }


        TweakNeckHeight();
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
                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDamping);
            }
            else
            {
                transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDamping);
            }
        }
    }

    private void ForceUpdatePosition(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);//transform.position + transform.rotation * (jointToGet.position - skeletonPosition);
    }

    //gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
    private void UpdateSkeletonPosition()
    {
        if (skeletonManager.skeletons[playerId].root.positionConfidence >= minimumConfidenceToUpdate)
        {
            skeletonPosition = skeletonManager.skeletons[playerId].root.position;
        }
    }

    private void SaveInitialRotation(Transform bodyPart)
    {
        if (bodyPart)
            jointInitialRotations[bodyPart] = GetInitialRotation(bodyPart);
    }

    private void SaveInitialDistance(Transform rootTransform, Transform distanceTo)
    {
        jointInitialDistances.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), Vector3.Distance(rootTransform.position, distanceTo.position));
    }

    private Quaternion GetInitialRotation(Transform bodyPart)
    {
        return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
    }

    private void UpdateBoneScalings()
    {
        torsoScale = UpdateTorsoScale();

        {
            float rightArmCumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeletonManager.skeletons[playerId].rightShoulder, skeletonManager.skeletons[playerId].rightElbow, torsoScale);
            UpdateBoneScaling(rightElbow, rightHand, skeletonManager.skeletons[playerId].rightElbow, skeletonManager.skeletons[playerId].rightHand, rightArmCumulativeScale / forearmLengthRatio);
        }

        {
            float leftArmCumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeletonManager.skeletons[playerId].leftShoulder, skeletonManager.skeletons[playerId].leftElbow, torsoScale);
            UpdateBoneScaling(leftElbow, leftHand, skeletonManager.skeletons[playerId].leftElbow, skeletonManager.skeletons[playerId].leftHand, leftArmCumulativeScale / forearmLengthRatio);
        }

        {
            float rightLegCumulativeScale = UpdateBoneScaling(rightHip, rightKnee, skeletonManager.skeletons[playerId].rightHip, skeletonManager.skeletons[playerId].rightKnee, torsoScale);
            UpdateBoneScaling(rightKnee, rightFoot, skeletonManager.skeletons[playerId].rightKnee, skeletonManager.skeletons[playerId].rightFoot, rightLegCumulativeScale);
        }

        {
            float leftLegCumulativeScale = UpdateBoneScaling(leftHip, leftKnee, skeletonManager.skeletons[playerId].leftHip, skeletonManager.skeletons[playerId].leftKnee, torsoScale);
            UpdateBoneScaling(leftKnee, leftFoot, skeletonManager.skeletons[playerId].leftKnee, skeletonManager.skeletons[playerId].leftFoot, leftLegCumulativeScale);
        }
    }

    private float UpdateBoneScaling(Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, RUISSkeletonManager.JointData comparisonBoneTracker, float cumulativeScale)
    {
        float modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];
        float playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
        float newScale = playerBoneLength / modelBoneLength / cumulativeScale;

        boneToScale.localScale = Vector3.Lerp(boneToScale.localScale, new Vector3(newScale, newScale, newScale), maxScaleFactor * Time.deltaTime);

        return boneToScale.localScale.x;
    }

    private float UpdateTorsoScale()
    {
        //average hip to shoulder length and compare it to the one found in the model - scale accordingly
        //we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
        float modelLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, leftHip)] +
                            jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2;
        float playerLength = (Vector3.Distance(skeletonManager.skeletons[playerId].rightShoulder.position, skeletonManager.skeletons[playerId].leftShoulder.position) +
                                Vector3.Distance(skeletonManager.skeletons[playerId].rightHip.position, skeletonManager.skeletons[playerId].leftHip.position)) / 2;

        float newScale = playerLength / modelLength;
        torso.localScale = new Vector3(newScale, newScale, newScale);
        return newScale;
    }

    private Quaternion FindFixingRotation(Vector3 fromJoint, Vector3 toJoint, Vector3 wantedDirection)
    {
        Vector3 boneVector = toJoint - fromJoint;
        return Quaternion.FromToRotation(boneVector, wantedDirection);
    }

    private void TweakNeckHeight()
    {
        if (!neck) return;
        neck.localPosition = neckOriginalLocalPosition - neck.InverseTransformDirection(Vector3.up) * neckHeightTweaker;
    }
}
