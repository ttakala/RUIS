using UnityEngine;
using System.Collections;

[AddComponentMenu("RUIS/GUI/RUISMenu")]
public class RUISMenu : MonoBehaviour {
    private const int mainWindow = 0;
    private int currentWindow = mainWindow;
    private Rect windowRect = new Rect(50, 50, 200, 200);

    bool isShowing = false;

    bool ruisMenuButtonDefined = true;

    bool isCalibrating = false;
    int previousSceneId = -1;

    [HideInInspector] public bool enablePSMove;
    [HideInInspector] public string psMoveIP;
    [HideInInspector] public int psMovePort;

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
            currentWindow = mainWindow;
        }
    }

    void OnGUI()
    {
        if (!isShowing) return;

        windowRect = GUILayout.Window(currentWindow, windowRect, DrawWindow, "RUIS");
    }

    void DrawWindow(int windowId)
    {
        if (!isCalibrating)
        {
            switch (currentWindow)
            {
                case mainWindow:
                    if (GUILayout.Button("Calibrate"))
                    {
                        Debug.Log("Loading calibration screen.");

                        gameObject.transform.parent = null;

                        previousSceneId = Application.loadedLevel;

                        isCalibrating = true;

                        isShowing = false;

                        Application.LoadLevel("calibration");
                    }
                    break;
            }
        }
        else
        {
            if(GUILayout.Button("End Calibration")){
                Destroy(this);

                Application.LoadLevel(previousSceneId);

                isCalibrating = false;
            }
        }
        GUI.DragWindow();
    }
}
