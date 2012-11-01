using UnityEngine;
using System.Collections;


[AddComponentMenu("RUIS/Input/RUISPSMoveController")]
public class RUISPSMoveController : RUISWand {
    public enum SelectionButton
    {
        Trigger,
        Cross,
        Circle,
        Square,
        Triangle,
        Move,
        Start,
        Select,
        None
    }

    public SelectionButton selectionButton;

    private static PSMoveWrapper psMoveWrapper;
    
    public int controllerId;

    private Vector3 positionUpdate;
    private Vector3 rotationUpdate;

    public RUISCoordinateSystem coordinateSystem;

    public bool copyColor = false;
    public Renderer whereToCopyColor;

	public void Awake ()
    {
        if (psMoveWrapper == null)
        {
            psMoveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;
            if (!psMoveWrapper)
            {
                Debug.LogError("Could not find PSMoveWrapper");
            }
        }

        if (coordinateSystem == null)
        {
            coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
            if (!coordinateSystem)
            {
                Debug.LogError("Could not find coordinate system!");
            }
        }
	}
	
	void Update ()
    {
        squareButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.SQUARE);
        crossButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.CROSS);
        circleButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.CIRCLE);
        triangleButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.TRIANGLE);
        moveButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.MOVE);
        triggerButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.T);
        startButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.START);
        selectButtonWasPressed = psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.SELECT);

        squareButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.SQUARE);
        crossButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.CROSS);
        circleButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.CIRCLE);
        triangleButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.TRIANGLE);
        moveButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.MOVE);
        triggerButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.T);
        startButtonWasReleased= psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.START);
        selectButtonWasReleased = psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.SELECT);
        
        if (rigidbody)
        {
            rigidbody.MovePosition(position);
            rigidbody.MoveRotation(qOrientation);
        }
        else
        {
            transform.position = position;
            transform.rotation = qOrientation;
        }


        if (copyColor && whereToCopyColor)
        {
            whereToCopyColor.material.color = color;
        }
    }

    public void RumbleOn(int strength)
    {
        psMoveWrapper.SetRumble(controllerId, strength);
    }

    public void RumbleOff() 
    {
        psMoveWrapper.SetRumble(controllerId, 0);
    }

    public void Rumble(int strength, float duration)
    {
        StartCoroutine(DoRumble(strength, duration));
    }

    private IEnumerator DoRumble(int strength, float duration)
    {
        psMoveWrapper.SetRumble(controllerId, strength);
        yield return new WaitForSeconds(duration);
        RumbleOff();
    }

    public void SetColor(Color color)
    {
        psMoveWrapper.SetColorAndTrack(controllerId, color);
    }

    public override bool SelectionButtonWasPressed()
    {
        switch (selectionButton)
        {
            case SelectionButton.Trigger:
                return triggerButtonWasPressed;
            case SelectionButton.Cross:
                return crossButtonWasPressed;
            case SelectionButton.Circle:
                return circleButtonWasPressed;
            case SelectionButton.Square:
                return squareButtonWasPressed;
            case SelectionButton.Triangle:
                return triangleButtonWasPressed;
            case SelectionButton.Move:
                return moveButtonWasPressed;
            case SelectionButton.Start:
                return startButtonWasPressed;
            case SelectionButton.Select:
                return selectButtonWasPressed;
            default:
                return false;
        }
    }

    public override bool SelectionButtonWasReleased()
    {
        switch (selectionButton)
        {
            case SelectionButton.Trigger:
                return triggerButtonWasReleased;
            case SelectionButton.Cross:
                return crossButtonWasReleased;
            case SelectionButton.Circle:
                return circleButtonWasReleased;
            case SelectionButton.Square:
                return squareButtonWasReleased;
            case SelectionButton.Triangle:
                return triangleButtonWasReleased;
            case SelectionButton.Move:
                return moveButtonWasReleased;
            case SelectionButton.Start:
                return startButtonWasReleased;
            case SelectionButton.Select:
                return selectButtonWasReleased;
            default:
                return false;
        }
    }

    public override bool SelectionButtonIsDown()
    {
        switch (selectionButton)
        {
            case SelectionButton.Trigger:
                return triggerValue > 0.99f;
            case SelectionButton.Cross:
                return crossButtonDown;
            case SelectionButton.Circle:
                return circleButtonDown;
            case SelectionButton.Square:
                return squareButtonDown;
            case SelectionButton.Triangle:
                return triangleButtonDown;
            case SelectionButton.Move:
                return moveButtonDown;
            case SelectionButton.Start:
                return startButtonDown;
            case SelectionButton.Select:
                return selectButtonDown;
            default:
                return false;
        }
    }

    public Vector3 position
    {
        get
        {
            return TransformPosition(psMoveWrapper.position[controllerId]);
        }
    }

    public Vector3 velocity
    {
        get
        {
            return TransformPosition(psMoveWrapper.velocity[controllerId]);
        }
    }
    public Vector3 acceleration
    {
        get
        {
            return TransformPosition(psMoveWrapper.acceleration[controllerId]);
        }
    }

    public Vector3 orientation
    {
        get
        {
            return qOrientation.eulerAngles;
        }
    }
    public Quaternion qOrientation
    {
        get
        {
            return coordinateSystem.ConvertMoveRotation(psMoveWrapper.qOrientation[controllerId]);
        }
    }
    public Vector3 angularVelocity
    {
        get
        {
            return coordinateSystem.ConvertMoveAngularVelocity(psMoveWrapper.angularVelocity[controllerId]);
        }
    }
    public Vector3 angularAcceleration
    {
        get
        {
            return coordinateSystem.ConvertMoveAngularVelocity(psMoveWrapper.angularAcceleration[controllerId]);
        }
    }

    public Vector3 handlePosition
    {
        get
        {
            return TransformPosition(psMoveWrapper.handlePosition[controllerId]);
        }
    }
    public Vector3 handleVelocity
    {
        get
        {
            return TransformPosition(psMoveWrapper.handleVelocity[controllerId]);
        }
    }
    public Vector3 handleAcceleration
    {
        get
        {
            return TransformPosition(psMoveWrapper.handleAcceleration[controllerId]);
        }
    }

    public bool squareButtonDown { get { return psMoveWrapper.isButtonSelect[controllerId]; } }
    public bool crossButtonDown { get { return psMoveWrapper.isButtonCross[controllerId]; } }
    public bool circleButtonDown { get { return psMoveWrapper.isButtonCircle[controllerId]; } }
    public bool triangleButtonDown { get { return psMoveWrapper.isButtonTriangle[controllerId]; } }
    public bool moveButtonDown { get { return psMoveWrapper.isButtonMove[controllerId]; } }
    public bool startButtonDown { get { return psMoveWrapper.isButtonStart[controllerId]; } }
    public bool selectButtonDown { get { return psMoveWrapper.isButtonSelect[controllerId]; } }

    public bool squareButtonWasPressed { get; private set; }
    public bool crossButtonWasPressed { get; private set; }
    public bool circleButtonWasPressed { get; private set; }
    public bool triangleButtonWasPressed { get; private set; }
    public bool moveButtonWasPressed { get; private set; }
    public bool triggerButtonWasPressed { get; private set; }
    public bool startButtonWasPressed { get; private set; }
    public bool selectButtonWasPressed { get; private set; }

    public bool squareButtonWasReleased { get; private set; }
    public bool crossButtonWasReleased { get; private set; }
    public bool circleButtonWasReleased { get; private set; }
    public bool triangleButtonWasReleased { get; private set; }
    public bool moveButtonWasReleased { get; private set; }
    public bool triggerButtonWasReleased { get; private set; }
    public bool startButtonWasReleased { get; private set; }
    public bool selectButtonWasReleased { get; private set; }

    public override Color color { get { return psMoveWrapper.sphereColor[controllerId]; } set { SetColor(value); } }

    public float triggerValue { get { return psMoveWrapper.valueT[controllerId] / 255.0f; } }

    private Vector3 TransformPosition(Vector3 value)
    {
        return coordinateSystem.ConvertMovePosition(value);
    }

    public override Vector3 GetAngularVelocity()
    {
        return angularVelocity;
    }
}
