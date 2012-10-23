using UnityEngine;
using System.Collections;

public class RUISSkeletonWand : RUISWand {
    public Transform wandStartTransform;
    public Transform wandEndTransform;

    //simulate grab with mouse button for now
    public bool mouseButtonPressed = false;
    public bool mouseButtonReleased = false;

	public void Update () {
        Vector3 wandPosition = wandEndTransform.position;
        Vector3 wandDirection = wandEndTransform.position - wandStartTransform.position;

        transform.position = wandPosition;
        transform.rotation = Quaternion.LookRotation(wandDirection);

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

    public override Vector3 GetAngularVelocity()
    {
        throw new System.NotImplementedException();
    }
}
