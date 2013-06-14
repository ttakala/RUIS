using UnityEngine;
using System.Collections;

public class RUISCharacterController : MonoBehaviour {
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

    public bool ignorePitchAndRollInTranslation = true;

    private RUISInputManager inputManager;
    private RUISSkeletonManager skeletonManager;

	void Awake () {
        inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
	}
	
	void Update () {
	
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

        if (ignorePitchAndRollInTranslation)
        {
            characterForward.y = 0;
            characterForward.Normalize();
        }

        characterForward = transform.TransformDirection(characterForward);


        return Quaternion.FromToRotation(Vector3.forward, characterForward) * directionInCharacterCoordinates;
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

    public void OnDrawGizmosSelected(){
        if (!Application.isPlaying)
            return;
        Vector3 pivotPosition = GetPivotPositionInTrackerCoordinates();
        pivotPosition = transform.TransformPoint(pivotPosition);

        Gizmos.DrawLine(transform.position, pivotPosition);
    }
}
