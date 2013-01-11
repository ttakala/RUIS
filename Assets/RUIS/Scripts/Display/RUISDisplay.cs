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

    public Camera GetCameraForScreenPoint(Vector2 screenPoint)
    {
        if (isStereo)
        {
            RUISStereoCamera stereoCam = linkedCamera.GetComponent<RUISStereoCamera>();
            if (stereoCam.leftCamera.pixelRect.Contains(screenPoint))
            {
                return stereoCam.leftCamera;
            }
            else if (stereoCam.rightCamera.pixelRect.Contains(screenPoint))
            {
                return stereoCam.rightCamera;
            }
            else return null;
        }
        else
        {
            RUISMonoCamera monoCam = linkedCamera.GetComponent<RUISMonoCamera>();
            if(monoCam.linkedCamera.pixelRect.Contains(screenPoint)){
                return monoCam.linkedCamera;
            } 
            else return null;
        }
    }

    public void WorldPointToScreenPoints(Vector3 worldPoint, ref List<RUISDisplayManager.ScreenPoint> screenPoints)
    {
        if (isStereo)
        {
            RUISDisplayManager.ScreenPoint leftCameraPoint = new RUISDisplayManager.ScreenPoint();
            leftCameraPoint.camera = linkedCamera.GetComponent<RUISStereoCamera>().leftCamera;
            leftCameraPoint.coordinates = leftCameraPoint.camera.WorldToScreenPoint(worldPoint);
            screenPoints.Add(leftCameraPoint);

            RUISDisplayManager.ScreenPoint rightCameraPoint = new RUISDisplayManager.ScreenPoint();
            rightCameraPoint.camera = linkedCamera.GetComponent<RUISStereoCamera>().rightCamera;
            rightCameraPoint.coordinates = rightCameraPoint.camera.WorldToScreenPoint(worldPoint);
            screenPoints.Add(rightCameraPoint);
        }
        else
        {
            RUISDisplayManager.ScreenPoint screenPoint = new RUISDisplayManager.ScreenPoint();
            screenPoint.camera = linkedCamera.GetComponent<RUISMonoCamera>().linkedCamera;
            screenPoint.coordinates = screenPoint.camera.WorldToScreenPoint(worldPoint);
            screenPoints.Add(screenPoint);
        }
    }
}
