using UnityEngine;
using System.Collections.Generic;

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
    public RUISCamera linkedCamera;
    public StereoType stereoType;

    private float aspectRatio;

    public void Awake()
    {
        if (!linkedCamera)
        {
            Debug.LogError("No camera attached to display: " + name);
        }
        else
        {
            linkedCamera.associatedDisplay = this;
        }

        if (isStereo && linkedCamera.GetComponent<RUISStereoCamera>() == null)
        {
            Debug.LogError("Display " + name + " marked as stereo but linked RUISCamera " + linkedCamera.name + " is mono. Switching to mono mode.");
            isStereo = false;
        }

        if (!isStereo && linkedCamera.GetComponent<RUISMonoCamera>() == null)
        {
            Debug.LogError("Display " + name + " marked as mono but linked RUISCamera " + linkedCamera.name + " is stereo. Switching to side by side stereo");
            isStereo = true;
            stereoType = StereoType.SideBySide;
        }

        aspectRatio = resolutionX / resolutionY;
    }

    public void SetupViewports(int xCoordinate, Vector2 totalRawResolution)
    {
        float relativeWidth = rawResolutionX / totalRawResolution.x;
        float relativeHeight = rawResolutionY / totalRawResolution.y;
        
        float relativeLeft = xCoordinate / totalRawResolution.x;
        float relativeBottom = 1.0f - relativeHeight;

        if (linkedCamera)
        {
            linkedCamera.SetupCameraViewports(relativeLeft, relativeBottom, relativeWidth, relativeHeight, aspectRatio);
            linkedCamera.associatedDisplay = this;
        }
        else
        {
            Debug.LogWarning("Please set up a RUISCamera for display: " + name);
        }
    }

    public Camera GetCameraForScreenPoint(Vector2 relativeScreenPoint)
    {
        if (relativeScreenPoint.x > rawResolutionX || relativeScreenPoint.y > rawResolutionY || relativeScreenPoint.y < 0) return null;

        if (!isStereo)
        {
            return linkedCamera.GetComponent<RUISMonoCamera>().linkedCamera;
        }
        else
        {
            if (stereoType == StereoType.SideBySide)
            {
                if (relativeScreenPoint.x < rawResolutionX / 2)
                {
                    return linkedCamera.GetComponent<RUISStereoCamera>().leftCamera;
                }
                else
                {
                    return linkedCamera.GetComponent<RUISStereoCamera>().rightCamera;
                }
            }
            else if (stereoType == StereoType.TopAndBottom)
            {
                if (relativeScreenPoint.y < rawResolutionY / 2)
                {
                    return linkedCamera.GetComponent<RUISStereoCamera>().rightCamera;
                }
                else
                {
                    return linkedCamera.GetComponent<RUISStereoCamera>().leftCamera;
                }
            }
            else
            {
                return linkedCamera.GetComponent<RUISStereoCamera>().leftCamera;
            }
        }
    }

    public void WorldPointToScreenPoints(Vector3 worldPoint, ref List<Vector2> screenPoints)
    {
        if (isStereo)
        {
            Vector3 leftCameraPoint = linkedCamera.GetComponent<RUISStereoCamera>().leftCamera.WorldToScreenPoint(worldPoint);
            leftCameraPoint.y = rawResolutionY - leftCameraPoint.y;
            screenPoints.Add(leftCameraPoint);
            Vector3 rightCameraPoint = linkedCamera.GetComponent<RUISStereoCamera>().rightCamera.WorldToScreenPoint(worldPoint);
            rightCameraPoint.y = rawResolutionY - rightCameraPoint.y;
            screenPoints.Add(rightCameraPoint);
        }
        else
        {
            Vector3 screenPoint = linkedCamera.GetComponent<RUISMonoCamera>().linkedCamera.WorldToScreenPoint(worldPoint);
            screenPoint.y = rawResolutionY - screenPoint.y;
            screenPoints.Add(screenPoint);
        }
    }
}
