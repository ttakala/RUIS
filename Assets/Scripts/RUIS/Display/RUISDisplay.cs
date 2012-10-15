using UnityEngine;
using System.Collections;

public class RUISDisplay : MonoBehaviour {
    public enum StereoType
    {
        SideBySide,
        TopAndBottom,
        Alternate
    }

    public int resolutionX;
    public int resolutionY;

    public bool useDoubleTheSpace = false;

    public int rawResolutionX
    {
        get
        {
            if (isStereo && stereoType == StereoType.SideBySide && useDoubleTheSpace)
            {
                return 2 * resolutionX;
            }
            else
            {
                return resolutionX;
            }
        }
    }

    public int rawResolutionY
    {
        get
        {
            if (isStereo && stereoType == StereoType.TopAndBottom && useDoubleTheSpace)
            {
                return 2 * resolutionY;
            }
            else
            {
                return resolutionY;
            }
        }
    }

    public bool isStereo;
    public bool isHMD; //head-mounted display
    public RUISCamera camera;
    public StereoType stereoType;

    public void Awake()
    {
        if (!camera)
        {
            Debug.LogError("No camera attached to display: " + name);
        }
        else
        {
            camera.associatedDisplay = this;
        }
    }

    public void SetupViewports(int xCoordinate, Vector2 totalRawResolution)
    {
        float relativeWidth = rawResolutionX / totalRawResolution.x;
        float relativeHeight = rawResolutionY / totalRawResolution.y;
        
        float relativeLeft = xCoordinate / totalRawResolution.x;
        float relativeBottom = 1.0f - relativeHeight;

        if (camera)
        {
            camera.SetupCameraViewports(relativeLeft, relativeBottom, relativeWidth, relativeHeight);
        }
        else
        {
            Debug.LogWarning("Please set up a RUISCamera for display: " + name);
        }
    }
}
