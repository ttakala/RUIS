using UnityEngine;
using System.Collections;

public class RUISMouseWand : RUISWand {
    bool mouseButtonPressed = false;
    bool mouseButtonReleased = false;
    bool mouseButtonDown = false;

    RUISDisplayManager displayManager;

    public void Awake()
    {
        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

        if (!displayManager)
        {
            Debug.LogError("RUISMouseWand requires a RUISDisplayManager in the scene!");
        }
    }

    public void Update()
    {
        Ray wandRay = displayManager.ScreenPointToRay(Input.mousePosition);
        if (wandRay.direction != Vector3.zero)
        {
            transform.position = wandRay.origin;
            transform.rotation = Quaternion.LookRotation(wandRay.direction);
        }

        mouseButtonPressed = Input.GetMouseButtonDown(0);
        mouseButtonReleased = Input.GetMouseButtonUp(0);
        mouseButtonDown = Input.GetMouseButton(0);
    }

    public override bool SelectionButtonWasPressed()
    {
        return mouseButtonPressed;
    }

    public override bool SelectionButtonWasReleased()
    {
        return mouseButtonReleased;
    }

    public override bool SelectionButtonIsDown()
    {
        return mouseButtonDown;
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }
}
