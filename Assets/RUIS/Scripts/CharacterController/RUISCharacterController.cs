/*****************************************************************************

Content    :   A Script to handle controlling a rigidbody character using Kinect and some traditional input method
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISCharacterController : MonoBehaviour
{
    public enum CharacterPivotType
    {
        KinectHip,
        KinectHead,
        KinectCOM,
        MoveController
    }

    public CharacterPivotType characterPivotType = CharacterPivotType.KinectCOM;

    public int kinectPlayerId;
    public int moveControllerId;
    public Transform characterPivot;

    public bool ignorePitchAndRoll = true;

    private RUISInputManager inputManager;
    private RUISSkeletonManager skeletonManager;

    public LayerMask groundLayers;
    public float groundedErrorTweaker = 0.05f;

    public bool grounded { get; private set; }

    private RUISCharacterStabilizingCollider stabilizingCollider;

    void Awake()
    {
        inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        stabilizingCollider = GetComponentInChildren<RUISCharacterStabilizingCollider>();
    }

    void Update()
    {
        Vector3 raycastPosition = stabilizingCollider ? stabilizingCollider.transform.position : transform.position;

        float distanceToRaycast = stabilizingCollider ? stabilizingCollider.colliderHeight / 2 : 1.5f;
        distanceToRaycast += groundedErrorTweaker;

        grounded = Physics.Raycast(raycastPosition, -transform.up, distanceToRaycast, groundLayers.value);
    }

    public void RotateAroundCharacterPivot(Vector3 eulerRotation)
    {
        Vector3 pivotPosition = GetPivotPositionInTrackerCoordinates();
        if (pivotPosition == Vector3.zero)
        {
            rigidbody.MoveRotation(Quaternion.Euler(eulerRotation) * transform.rotation);
            return;
        }

        pivotPosition = transform.TransformPoint(pivotPosition);
        Debug.Log("pivotPosition: " + pivotPosition);
        //Debug.DrawLine(pivotPosition, transform.position, Color.blue);

        Vector3 positionDiff = pivotPosition - transform.position;
        //Debug.Log("old: " + positionDiff);
        //positionDiff.y = 0;
        Debug.DrawLine(pivotPosition - positionDiff, pivotPosition, Color.red);

        positionDiff = Quaternion.Euler(eulerRotation) * positionDiff;
        //Debug.DrawLine(transform.position, pivotPosition, Color.red);
        Debug.DrawLine(pivotPosition - positionDiff, pivotPosition, Color.green);

        //Debug.Log("new: " + positionDiff);
        Vector3 newPosition = pivotPosition - positionDiff;
        Debug.DrawLine(transform.position, newPosition, Color.yellow);
        rigidbody.MovePosition(newPosition);
        rigidbody.MoveRotation(Quaternion.Euler(eulerRotation) * transform.rotation);
    }

    public Vector3 TransformDirection(Vector3 directionInCharacterCoordinates)
    {
        Vector3 characterForward = Vector3.forward;

        switch (characterPivotType)
        {
            case CharacterPivotType.KinectHip:
                characterForward = skeletonManager.skeletons[kinectPlayerId].leftHip.rotation * Vector3.forward;
                break;
            case CharacterPivotType.KinectHead:
                characterForward = skeletonManager.skeletons[kinectPlayerId].head.rotation * Vector3.forward;
                break;
            case CharacterPivotType.KinectCOM:
                characterForward = skeletonManager.skeletons[kinectPlayerId].torso.rotation * Vector3.forward;
                break;
            case CharacterPivotType.MoveController:
                characterForward = inputManager.GetMoveWand(moveControllerId).qOrientation * Vector3.forward;
                break;
        }

        if (ignorePitchAndRoll)
        {
            characterForward.y = 0;
            characterForward.Normalize();
        }

        characterForward = transform.TransformDirection(characterForward);


        return Quaternion.LookRotation(characterForward, transform.up) * directionInCharacterCoordinates;
    }

    private Vector3 GetPivotPositionInTrackerCoordinates()
    {
        switch (characterPivotType)
        {
            case CharacterPivotType.KinectHip:
                return skeletonManager.skeletons[kinectPlayerId].leftHip.position;
            case CharacterPivotType.KinectHead:
                return skeletonManager.skeletons[kinectPlayerId].head.position;
            case CharacterPivotType.KinectCOM:
                return skeletonManager.skeletons[kinectPlayerId].torso.position;
            case CharacterPivotType.MoveController:
                return inputManager.GetMoveWand(moveControllerId).handlePosition;
        }

        return Vector3.zero;
    }

    public void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;
        Vector3 pivotPosition = GetPivotPositionInTrackerCoordinates();
        pivotPosition = transform.TransformPoint(pivotPosition);

        Gizmos.DrawLine(transform.position, pivotPosition);
    }
}
