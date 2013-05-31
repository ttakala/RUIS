using UnityEngine;
using System.Collections;

public class RUISRiftCamera : RUISCamera {
	void Awake () 
    {
        leftCamera = transform.FindChild("CameraLeft").GetComponent<Camera>();
        rightCamera = transform.FindChild("CameraRight").GetComponent<Camera>();

        if (!leftCamera || !rightCamera)
        {
            Debug.LogError("Could not find cameras for Oculus Rift.");
        }
	}

    void Start()
    {
    }

    void Update()
    {
    }

    void LateUpdate()
    {
    }
	
	override public void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight, float aspectRatio)
    {
        leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth / 2, relativeHeight);
        rightCamera.rect = new Rect(relativeLeft + relativeWidth / 2, relativeBottom, relativeWidth / 2, relativeHeight);
	}
}
