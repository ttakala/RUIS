using UnityEngine;
using System.Collections.Generic;

public class RUISHeadTracker : MonoBehaviour {
    public Transform transformFromTracker;
    public Vector3 trackerPositionOffset;
    public Vector3 trackerRotationOffset;

    List<RUISCamera> linkedCameras = new List<RUISCamera>();
    //we have to record the original relative camera positions (compared to the head tracker position)
    //in order to be able to keep them once we start moving
    Dictionary<RUISCamera, Vector3> linkedCameraPositions = new Dictionary<RUISCamera, Vector3>();
    Dictionary<RUISCamera, Quaternion> linkedCameraRotations = new Dictionary<RUISCamera, Quaternion>();

    void Awake()
    {
        linkedCameras = new List<RUISCamera>(GetComponentsInChildren<RUISCamera>() as RUISCamera[]);
    }

	void Start () {
        foreach (RUISCamera camera in linkedCameras)
        {
            camera.SetupHeadTracking();
            linkedCameraPositions.Add(camera, camera.transform.position);
            linkedCameraRotations.Add(camera, camera.transform.rotation);
        }
	}
	
	void LateUpdate () {
        foreach (RUISCamera camera in linkedCameras)
        {
            camera.transform.position = transform.position + linkedCameraPositions[camera] + transformFromTracker.position + trackerPositionOffset;
            camera.transform.rotation = transform.rotation * linkedCameraRotations[camera] * (transformFromTracker.rotation * Quaternion.Euler(trackerRotationOffset));
        }
	}
}
