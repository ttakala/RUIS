using UnityEngine;
using System.Collections;

public class RUISCamera : MonoBehaviour {
    [HideInInspector]
    public bool isHeadTracking;
    [HideInInspector]
    public bool isKeystoneCorrected;

    public Camera centerCamera; //the camera used for mono rendering
    public Camera leftCamera;
    public Camera rightCamera;

    public float eyeSeparation = 0.06f;
    public float zeroParallaxDistance = 0;

    public RUISDisplay associatedDisplay;

    private Rect normalizedScreenRect;
    private float aspectRatio;

    public bool isStereo { get { return associatedDisplay.isStereo; } }

    private bool oldStereoValue;
    private RUISDisplay.StereoType oldStereoTypeValue;

    RUISKeystoningConfiguration keystoningConfiguration;

    public void Awake()
    {
        keystoningConfiguration = GetComponent<RUISKeystoningConfiguration>();
    }

	public void Start () {
        if (!associatedDisplay)
        {
            Debug.LogError("Camera not associated to any display, disabling... " + name);
            gameObject.SetActiveRecursively(false);
            return;
        }

        centerCamera = camera;

        UpdateStereo();
        UpdateStereoType();

        if (!leftCamera || !rightCamera)
        {
            Debug.LogError("Cameras not set properly in RUISCamera: " + name);
        }

        SetupCameraTransforms();
	}
	
	public void Update () {
        centerCamera.ResetProjectionMatrix();
        leftCamera.ResetProjectionMatrix();
        rightCamera.ResetProjectionMatrix();

        if (oldStereoValue != associatedDisplay.isStereo)
        {
            UpdateStereo();
        }

        if (oldStereoTypeValue != associatedDisplay.stereoType)
        {
            UpdateStereoType();
        }

        if (isHeadTracking)
        {
            ApplyHeadTrackingDistortion();
        }

        if (isKeystoneCorrected)
        {
            ApplyKeystoneCorrection();
        }
	}

    public void SetupHeadTracking()
    {
        isHeadTracking = true;
    }

    public void SetupKeystoneCorrection()
    {
        isKeystoneCorrected = true;
    }

    private void ApplyHeadTrackingDistortion()
    {
    }

    private void ApplyKeystoneCorrection()
    {
        centerCamera.projectionMatrix *= keystoningConfiguration.centerCameraKeystoningSpec.GetMatrix();
        leftCamera.projectionMatrix *= keystoningConfiguration.leftCameraKeystoningSpec.GetMatrix();
        rightCamera.projectionMatrix *= keystoningConfiguration.rightCameraKeystoningSpec.GetMatrix();
    }

    public void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight, float aspectRatio)
    {
        normalizedScreenRect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
        this.aspectRatio = aspectRatio;

        centerCamera.rect = normalizedScreenRect;
        centerCamera.aspect = aspectRatio;

        if (associatedDisplay.stereoType == RUISDisplay.StereoType.SideBySide)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth / 2, relativeHeight);
            rightCamera.rect = new Rect(relativeLeft + relativeWidth / 2, relativeBottom, relativeWidth / 2, relativeHeight);
        }
        else if (associatedDisplay.stereoType == RUISDisplay.StereoType.TopAndBottom)
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom + relativeHeight / 2, relativeWidth, relativeHeight / 2);
            rightCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight / 2);
        }
        else
        {
            leftCamera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
            rightCamera.rect = new Rect(leftCamera.rect);
        }

        leftCamera.aspect = aspectRatio;
        rightCamera.aspect = aspectRatio;
    }

    public void SetupCameraTransforms()
    {
        float halfEyeSeparation = eyeSeparation / 2;
        leftCamera.transform.localPosition = new Vector3(-halfEyeSeparation, 0, 0);
        rightCamera.transform.localPosition = new Vector3(halfEyeSeparation, 0, 0);

        if (zeroParallaxDistance > 0)
        {
            float angle = Mathf.Acos(halfEyeSeparation / Mathf.Sqrt(Mathf.Pow(halfEyeSeparation, 2) + Mathf.Pow(zeroParallaxDistance, 2)));
            Vector3 rotation = new Vector3(0, angle, 0);
            rightCamera.transform.localRotation = Quaternion.Euler(-rotation);
            leftCamera.transform.localRotation = Quaternion.Euler(rotation);
        }
    }

    private void UpdateStereo()
    {
        if (associatedDisplay.isStereo)
        {
            centerCamera.enabled = false;
            leftCamera.enabled = true;
            rightCamera.enabled = true;
        }
        else
        {
            centerCamera.enabled = true;
            leftCamera.enabled = false;
            rightCamera.enabled = false;
        }

        oldStereoValue = associatedDisplay.isStereo;
    }

    private void UpdateStereoType()
    {
        SetupCameraViewports(normalizedScreenRect.xMin, normalizedScreenRect.yMin, normalizedScreenRect.width, normalizedScreenRect.height, aspectRatio);
        oldStereoTypeValue = associatedDisplay.stereoType;
    }
}
