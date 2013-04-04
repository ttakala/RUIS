using UnityEngine;
using System.Collections;

[AddComponentMenu("RUIS/GUI/RUISMenu")]
public class RUISMenu : MonoBehaviour {
    private enum MenuState
    {
        Main,
        Calibrating,
        EditingDisplays
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

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);

        try
        {
            Input.GetButtonDown("RUISMenu");
        }
        catch (UnityException)
        {
            ruisMenuButtonDefined = false;
        }

        RUISInputManager inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
        enablePSMove = inputManager.enablePSMove;
        psMoveIP = inputManager.PSMoveIP;
        psMovePort = inputManager.PSMovePort;
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
    }

    void DrawWindow(int windowId)
    {
        switch(menuState){
            case MenuState.Main:
                if (GUILayout.Button("Calibrate Coordinate System"))
                {
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
                break;
            case MenuState.Calibrating:
                if(GUILayout.Button("End Calibration")){
                    Destroy(this.gameObject);

                    Application.LoadLevel(previousSceneId);
                }
                break;
            case MenuState.EditingDisplays:
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
