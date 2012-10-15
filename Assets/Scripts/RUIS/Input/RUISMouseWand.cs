using UnityEngine;
using System.Collections;

public class RUISMouseWand : RUISWand {
    public bool mouseButtonPressed = false;
    public bool mouseButtonReleased = false;

    public void Update()
    {
        Ray wandRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        transform.position = wandRay.origin;
        transform.rotation = Quaternion.LookRotation(wandRay.direction);

        mouseButtonPressed = Input.GetMouseButtonDown(0);
        mouseButtonReleased = Input.GetMouseButtonUp(0);
    }

    public override bool SelectionButtonWasPressed()
    {
        return mouseButtonPressed;
    }

    public override bool SelectionButtonWasReleased()
    {
        return mouseButtonReleased;
    }

    public override Vector3 GetVelocity()
    {
        throw new System.NotImplementedException();
    }

    public override Vector3 GetAngularVelocity()
    {
        throw new System.NotImplementedException();
    }
}
