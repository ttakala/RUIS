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

	// Use this for initialization
	void Start () {
        InitDisplays();
	}
	
	// Update is called once per frame
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
}
