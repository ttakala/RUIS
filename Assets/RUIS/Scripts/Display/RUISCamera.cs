using UnityEngine;
using System.Collections;

public abstract class RUISCamera : MonoBehaviour {
    public bool isHeadTracking { get; private set; }
    public bool isKeystoneCorrected { get; private set; }
    [HideInInspector]
    public RUISDisplay associatedDisplay;

	public void Start () {
	}
	
	public void Update () {
        if (isHeadTracking)
        {
            ApplyHeadTrackingDistortion();
        }

        if (isKeystoneCorrected)
        {
            ApplyKeystoneCorrection();
        }
	}

    public virtual void SetupHeadTracking()
    {
        isHeadTracking = true;
    }

    public virtual void SetupKeystoneCorrection()
    {
        isKeystoneCorrected = true;
    }

    protected abstract void ApplyHeadTrackingDistortion();

    protected abstract void ApplyKeystoneCorrection();

    public abstract void SetupCameraViewports(float relativeLeft, float relativeBottom, float relativeWidth, float relativeHeight);
}
