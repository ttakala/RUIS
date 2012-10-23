using UnityEngine;
using System.Collections;

public class RUISMonoCamera : RUISCamera {
    public Camera camera;

    public void Awake()
    {
        camera = GetComponent<Camera>();
    }

	public new void Start () {
        base.Start();
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
        camera.rect = new Rect(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
    }

}
