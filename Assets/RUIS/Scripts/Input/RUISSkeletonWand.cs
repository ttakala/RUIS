using UnityEngine;
using System.Collections;

[AddComponentMenu("RUIS/Input/RUISSkeletonWand")]
public class RUISSkeletonWand : RUISWand {
    public Transform wandStartTransform;
    public Transform wandEndTransform;

    public RUISGestureRecognizer gestureRecognizer;

    public Color wandColor = Color.white;

	public void Update () {
        Vector3 wandPosition = wandEndTransform.position;
        Vector3 wandDirection = wandEndTransform.position - wandStartTransform.position;

        transform.position = wandPosition;
        transform.rotation = Quaternion.LookRotation(wandDirection);

        gestureRecognizer.GestureTriggered();
	}

    public override bool SelectionButtonWasPressed()
    {
        return gestureRecognizer.GestureTriggered();
    }

    public override bool SelectionButtonWasReleased()
    {
        return gestureRecognizer.GestureTriggered();
    }

    public override bool SelectionButtonIsDown()
    {
        return gestureRecognizer.GestureTriggered();
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }

    public override Color color { get { return wandColor; } }
}
