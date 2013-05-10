using UnityEngine;
using System.Collections.Generic;

public class RUISHeadTracker : MonoBehaviour {
    public Transform transformFromTracker;
    public Vector3 trackerPositionOffset;
    public Vector3 trackerRotationOffset;

    private Quaternion rotation = Quaternion.identity;

    public float eyeSeparation = 0.06f;

    public Vector3 defaultPosition;

    private Vector3 eyeCenterPosition;
    public Vector3 EyeCenterPosition
    {
        get
        {
            return eyeCenterPosition;
        }
    }
    public Vector3 LeftEyePosition
    {
        get
        {
            return eyeCenterPosition - rotation * Vector3.right * eyeSeparation;
        }
    }
    public Vector3 RightEyePosition
    {
        get
        {
            return eyeCenterPosition + rotation * Vector3.right * eyeSeparation;
        }
    }

    void Awake()
    {
        //linkedCameras = new List<RUISCamera>(GetComponentsInChildren<RUISCamera>() as RUISCamera[]);
    }

	void Start () {
	}
	
	void LateUpdate () {
        if (transformFromTracker != null)
        {
            eyeCenterPosition = transformFromTracker.position + trackerPositionOffset;
            rotation = transformFromTracker.rotation;
        }
        else
        {
            eyeCenterPosition = defaultPosition;
            rotation = Quaternion.identity;
        }
        /*foreach (RUISCamera camera in linkedCameras)
        {
            camera.transform.position = transform.position + linkedCameraPositions[camera] + transformFromTracker.position + trackerPositionOffset;
            camera.transform.rotation = transform.rotation * linkedCameraRotations[camera] * (transformFromTracker.rotation * Quaternion.Euler(trackerRotationOffset));
        }*/
	}

    void OnDrawGizmosSelected()
    {
        Color originalColor = Gizmos.color;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(LeftEyePosition, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(RightEyePosition, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(eyeCenterPosition - rotation * Vector3.up * 0.2f, eyeCenterPosition + rotation * Vector3.up * 0.2f);
        Gizmos.color = originalColor;
    }
}
