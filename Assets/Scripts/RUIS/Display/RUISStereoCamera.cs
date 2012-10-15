using UnityEngine;
using System.Collections;

public class RUISStereoCamera : RUISCamera {
    public float eyeSeparation = 0.075f;
    public float zeroParallaxDistance = 0;
    public Camera left;
    public Camera right;

	// Use this for initialization
	public new void Start () {
        base.Start();
        SetupCameraTransforms();	
	}

    public void SetupCameraTransforms()
    {
        float halfEyeSeparation = eyeSeparation / 2;
        left.transform.localPosition = new Vector3(-halfEyeSeparation, 0, 0);
        right.transform.localPosition = new Vector3(halfEyeSeparation, 0, 0);

        if (zeroParallaxDistance > 0)
        {
            float angle = Mathf.Acos(halfEyeSeparation / Mathf.Sqrt(Mathf.Pow(halfEyeSeparation, 2) + Mathf.Pow(zeroParallaxDistance, 2)));
            Vector3 rotation = new Vector3(0, angle, 0);
            right.transform.localRotation = Quaternion.Euler(-rotation);
            left.transform.localRotation = Quaternion.Euler(rotation);
        }
    }

    protected override void ApplyHeadTrackingDistortion()
    {
        Debug.LogWarning("Head tracking distortion not yet implemented");
    }

    protected override void ApplyKeystoneCorrection()
    {
        Debug.LogWarning("Keystone correction not yet implemented");
    }

    public override void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight)
    {
        if (associatedDisplay.stereoType == RUISDisplay.StereoType.SideBySide)
        {
            left.rect = new Rect(relativeLeft, relativeBottom, relativeWidth / 2, relativeHeight);
            right.rect = new Rect(relativeLeft + relativeWidth / 2, relativeBottom, relativeWidth / 2, relativeHeight);
        }
        else if (associatedDisplay.stereoType == RUISDisplay.StereoType.TopAndBottom)
        {
            left.rect = new Rect(relativeLeft, relativeBottom + relativeHeight / 2, relativeWidth, relativeHeight / 2);
            right.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight / 2);
        }
        else
        {
            left.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
            right.rect = new Rect(left.rect);
        }
    }
}
