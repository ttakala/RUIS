using UnityEngine;
using System.Collections.Generic;

public class RUISHeadTracker : MonoBehaviour {
    public Transform positionFromTracker;
    public Transform rotationFromTracker;
    public Vector3 trackerPositionOffset;
    public Vector3 trackerRotationOffset;

    public Quaternion rotation
    {
        get;
        private set;
    }

    public Vector3 defaultPosition;
    public Vector3 defaultRotation;

    private Vector3 eyeCenterPosition;
    public Vector3 EyeCenterPosition
    {
        get
        {
            return eyeCenterPosition;
        }
    }

    void Awake()
    {
        //linkedCameras = new List<RUISCamera>(GetComponentsInChildren<RUISCamera>() as RUISCamera[]);
    }

	void Start () {
	}
	
	void LateUpdate () {
        if (positionFromTracker != null)
        {
            eyeCenterPosition = positionFromTracker.position + trackerPositionOffset;
        }
        else
        {
            eyeCenterPosition = defaultPosition;
        }

        if (rotationFromTracker != null)
        {
            rotation = rotationFromTracker.rotation * Quaternion.Euler(trackerRotationOffset);
        }
        else
        {
            rotation = Quaternion.Euler(defaultRotation);
        }
	}

    private Vector3 GetLeftEyePosition(float eyeSeparation)
    {
        return EyeCenterPosition - rotation * Vector3.right * eyeSeparation / 2;
    }

    // Returns the eye positions in slots {center, left, right}
    public Vector3[] GetEyePositions(float eyeSeparation)
    {
        Vector3 leftEye = GetLeftEyePosition(eyeSeparation);
        return new Vector3[] {
            EyeCenterPosition,
            leftEye,
            EyeCenterPosition + (EyeCenterPosition - leftEye)
        };
    }
}
