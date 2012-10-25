using UnityEngine;
using System.Collections;

public class RUISMenu : MonoBehaviour {
    private const int mainWindow = 0;
    private const int keystoningWindow = 1;
    private int currentWindow = mainWindow;
    private Rect windowRect = new Rect(50, 50, 200, 200);

    RUISDisplayManager displayManager;
    RUISInputManager inputManager;

    bool isShowing = false;

    bool ruisMenuButtonDefined = true;

	// Use this for initialization
	void Start () {
        displayManager = GetComponent<RUISDisplayManager>();
        inputManager = GetComponent<RUISInputManager>();

        try
        {
            Input.GetButtonDown("RUISMenu");
        }
        catch (UnityException)
        {
            ruisMenuButtonDefined = false;
        }
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
        Debug.Log("baa");

        GUI.DragWindow();
        switch (currentWindow)
        {
            case mainWindow:
                if (GUILayout.Button("Calibrate"))
                {
                    Debug.Log("Should load calibration..");
                    Application.LoadLevel("calibration");
                    Debug.Log("Didn't load calibration..");
                }
                break;
        }
    }
}
