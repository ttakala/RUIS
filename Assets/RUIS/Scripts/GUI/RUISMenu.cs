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

	// Use this for initialization
	void Start () {
        displayManager = GetComponent<RUISDisplayManager>();
        inputManager = GetComponent<RUISInputManager>();
	}

    void Update()
    {
        if (Input.GetButtonDown("RUISMenu"))
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
        GUI.DragWindow();
        switch (currentWindow)
        {
            case mainWindow:
                if (GUILayout.Button("Configure keystoning"))
                {
                    currentWindow = keystoningWindow;
                }

                if (GUILayout.Button("PS Move"))
                {

                }

                if (GUILayout.Button("Kinect"))
                {

                }
                break;
            case keystoningWindow:
                if (GUILayout.Button("Activate keystone correction"))
                {

                }
                if(GUILayout.Button("Back")){
                    currentWindow = mainWindow;
                }
                break;
        }
    }
}
