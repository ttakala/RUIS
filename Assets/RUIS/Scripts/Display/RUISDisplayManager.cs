using UnityEngine;
using System.Collections.Generic;

public class RUISDisplayManager : MonoBehaviour {
    public List<RUISDisplay> displays;
    public GameObject stereoCamera;
    public Camera monoCamera;
    public int totalResolutionX = 0;
    public int totalResolutionY = 0;
    public int totalRawResolutionX = 0;
    public int totalRawResolutionY = 0;

    public bool fullScreen;

    private float editorHeightScaler = 1;
    private float editorWidthScaler = 1;

	void Start () {
        UpdateTotalResolution();

        if (Application.isEditor)
        {
            CalculateEditorResolutions();
        }

        InitDisplays();
	}
	
	void Update () {
	}

    public void InitDisplays()
    {
        UpdateTotalResolution();

        int currentResolutionX = 0;
        foreach (RUISDisplay display in displays)
        {
            display.SetupViewports(currentResolutionX, new Vector2(totalRawResolutionX, totalRawResolutionY));
            currentResolutionX += display.rawResolutionX;
        }

        Screen.SetResolution(totalRawResolutionX, totalRawResolutionY, fullScreen);
    }

    public void UpdateTotalResolution()
    {
        totalResolutionX = 0;
        totalResolutionY = 0;
        totalRawResolutionX = 0;
        totalRawResolutionY = 0;

        foreach (RUISDisplay display in displays)
        {
            totalResolutionX += display.resolutionX;
            totalResolutionY = Mathf.Max(totalResolutionY, display.resolutionY);

            totalRawResolutionX += display.rawResolutionX;
            totalRawResolutionY = Mathf.Max(totalRawResolutionY, display.rawResolutionY);
        }
    }

    public Ray ScreenPointToRay(Vector2 screenPoint)
    {
        int currentResolutionX = 0;
        foreach (RUISDisplay display in displays)
        {
            
            if (currentResolutionX + display.rawResolutionX >= screenPoint.x)
            {
                Camera camera = display.GetCameraForScreenPoint(new Vector2(screenPoint.x - currentResolutionX, screenPoint.y));
                
                if (camera)
                {
                    return camera.ScreenPointToRay(screenPoint);
                }
                else
                {
                    break;
                }
            }

            currentResolutionX += display.rawResolutionX;
        }

        return new Ray(Vector3.zero, Vector3.zero);
    }

    private void CalculateEditorResolutions()
    {
        int trueWidth = Screen.width;
        int trueHeight = Screen.height;

        editorWidthScaler = (float)trueWidth / totalRawResolutionX;
        editorHeightScaler = (float)trueHeight / totalRawResolutionY;

        foreach (RUISDisplay display in displays)
        {
            display.resolutionX = (int)(display.resolutionX * editorWidthScaler);
            display.resolutionY = (int)(display.resolutionY * editorHeightScaler);
        }
    }
}
