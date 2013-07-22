/*****************************************************************************

Content    :   The menu shown when pressing F2 (default)
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

[AddComponentMenu("RUIS/GUI/RUISMenu")]
public class RUISMenu : MonoBehaviour {
    private enum MenuState
    {
        Main,
        Calibrating,
        EditingDisplays,
        EditingInputConfiguration
    }

    private MenuState menuState = MenuState.Main;

    private int currentWindow = 0;
    private Rect windowRect = new Rect(50, 50, 200, 200);

    bool isShowing = false;

    bool ruisMenuButtonDefined = true;

    int previousSceneId = -1;

    [HideInInspector] public bool enablePSMove;
    [HideInInspector] public string psMoveIP;
    [HideInInspector] public int psMovePort;

    bool isEditingKeystones = false;

    RUISInputManager inputManager;

    public bool oculusRiftMenu = false;
    RUISDisplayManager displayManager;
    RUISDisplay riftDisplay;

	// Use this for initialization
	void Start () {

        try
        {
            Input.GetButtonDown("RUISMenu");
        }
        catch (UnityException)
        {
            ruisMenuButtonDefined = false;
        }

        inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        enablePSMove = inputManager.enablePSMove;
        psMoveIP = inputManager.PSMoveIP;
        psMovePort = inputManager.PSMovePort;


        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
        riftDisplay = displayManager.GetOculusRiftDisplay();
        Debug.Log(riftDisplay.linkedCamera.leftCamera.name);
        if (oculusRiftMenu)
        {
            Debug.Log("pixelrect " + (riftDisplay.linkedCamera.leftCamera.pixelRect.x + riftDisplay.resolutionX / 4 - 100));
            Debug.Log("riftDisplay.resolutionX / 2: " + (riftDisplay.resolutionX / 4));
            windowRect = new Rect(riftDisplay.linkedCamera.leftCamera.pixelRect.x + riftDisplay.resolutionX / 4 - 200, riftDisplay.resolutionY / 2 - 100, 200, 200);
                        //new Rect(220, 400, 200, 200);
        }
	}

    void Update()
    {
        if ((!ruisMenuButtonDefined && Input.GetKeyDown(KeyCode.F2)) || (ruisMenuButtonDefined && Input.GetButtonDown("RUISMenu")))
        {
            isShowing = !isShowing;
        }
    }

    void OnGUI()
    {
        if (!isShowing) return;

        windowRect = GUILayout.Window(currentWindow, windowRect, DrawWindow, "RUIS");

        if (oculusRiftMenu)
        {
            if (riftDisplay)
            {
                float offset = riftDisplay.resolutionX / 2;
                Rect temp = GUILayout.Window(currentWindow + 1, new Rect(windowRect.x + offset, windowRect.y, windowRect.width, windowRect.height), DrawWindow, "RUIS");
                windowRect = new Rect(temp.x - offset, temp.y, temp.width, temp.height);
            }
        }
    }

    void DrawWindow(int windowId)
    {
        switch(menuState){
            case MenuState.Main:
                if (GUILayout.Button("Calibrate Coordinate System"))
                {
                    DontDestroyOnLoad(this);

                    Debug.Log("Loading calibration screen.");

                    gameObject.transform.parent = null;

                    previousSceneId = Application.loadedLevel;

                    menuState = MenuState.Calibrating;

                    isShowing = false;

                    Application.LoadLevel("calibration");
                }
                if (GUILayout.Button("Display Configuration"))
                {
                    SwitchKeystoneEditingState();
                    menuState = MenuState.EditingDisplays;
                }
                if (GUILayout.Button("Input Configuration"))
                {
                    menuState = MenuState.EditingInputConfiguration;
                }
				if(GUILayout.Button ("Resize Screen")){
					(FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).UpdateDisplays();
				}
                if (GUILayout.Button("Quit Application"))
                {
                    Application.Quit();
                }
                break;
            case MenuState.Calibrating:
                if(GUILayout.Button("End Calibration")){
                    Destroy(this.gameObject);

                    Application.LoadLevel(previousSceneId);
                }
                break;
            case MenuState.EditingDisplays:
                if (GUILayout.Button("Reset Keystoning"))
                {
                    foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
                    {
                        keystoningConfiguration.ResetConfiguration();
                    }
                }
                if(GUILayout.Button("Save Configurations")){
                    (FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).SaveDisplaysToXML();
                }
                if(GUILayout.Button("Load Old Configurations")){
                    (FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).LoadDisplaysFromXML();
                }
                if (GUILayout.Button("End Display Editing"))
                {
                    SwitchKeystoneEditingState();
                    menuState = MenuState.Main;
                }
                break;
            case MenuState.EditingInputConfiguration:
                string togglePSMoveText = inputManager.enablePSMove ? "Disable PS Move" : "Enable PS Move";
                if (GUILayout.Button(togglePSMoveText))
                {
                    inputManager.enablePSMove = !inputManager.enablePSMove;
                }
                string toggleKinectText = inputManager.enableKinect ? "Disable Kinect" : "Enable Kinect";
                if (GUILayout.Button(toggleKinectText))
                {
                    inputManager.enableKinect = !inputManager.enableKinect;
                }
                string toggleRazerHydraText = inputManager.enablePSMove ? "Disable Razer Hydra" : "Enable Razer Hydra";
                if (GUILayout.Button(toggleRazerHydraText))
                {
                    inputManager.enablePSMove = !inputManager.enablePSMove;
                }

                if (GUILayout.Button("Save Configuration & Restart Scene"))
                {
                    inputManager.Export(inputManager.filename);
                    Application.LoadLevel(Application.loadedLevel);
                }
                if (GUILayout.Button("End Input Configuration Editing"))
                {
                    menuState = MenuState.Main;
                }
                break;
        }

        GUI.DragWindow();
    }

    private void SwitchKeystoneEditingState()
    {
        foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
        {
            if (isEditingKeystones)
            {
                keystoningConfiguration.EndEditing();
            }
            else
            {
                keystoningConfiguration.StartEditing();
            }
        }

        isEditingKeystones = !isEditingKeystones;
    }

    private void ToggleKeystoneGridState()
    {
        foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
        {
            keystoningConfiguration.drawKeystoningGrid = !keystoningConfiguration.drawKeystoningGrid;
        }
    }
}
