using UnityEngine;
using System.Collections;


[AddComponentMenu("RUIS/Input/RUISPSMoveController")]
public class RUISPSMoveWand : RUISWand {
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


        if (whereToCopyColor != null)
        {
            foreach (Material mat in whereToCopyColor.materials)
            {
                mat.color = color;
            }
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

    public bool squareButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.SQUARE); } }
    public bool crossButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.CROSS); } }
    public bool circleButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.CIRCLE); } }
    public bool triangleButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.TRIANGLE); } }
    public bool moveButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.MOVE); } }
    public bool triggerButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.T); } }
    public bool startButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.START); } }
    public bool selectButtonWasPressed { get { return psMoveWrapper.WasPressed(controllerId, PSMoveWrapper.SELECT); } }

    public bool squareButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.SQUARE); } }
    public bool crossButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.CROSS); } }
    public bool circleButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.CIRCLE); } }
    public bool triangleButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.TRIANGLE); } }
    public bool moveButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.MOVE); } }
    public bool triggerButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.T); } }
    public bool startButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.START); } }
    public bool selectButtonWasReleased { get { return psMoveWrapper.WasReleased(controllerId, PSMoveWrapper.SELECT); } }

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
