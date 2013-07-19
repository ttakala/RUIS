/*****************************************************************************

Content    :   Class for locomotion of Kinect controlled character
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISCharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class RUISCharacterLocomotionControl : MonoBehaviour
{
    RUISCharacterController characterController;

    public KeyCode turnRightKey = KeyCode.E;
    public KeyCode turnLeftKey = KeyCode.Q;

    public float rotationScaler = 60.0f;

    public float speed = 2.0f;
    public float gravity = 10.0f; // Is this the best place to apply gravity to RUISCharacterController?
    public float maxVelocityChange = 10.0f;
    //public bool canJump = true;
    //public float jumpHeight = 2.0f;
    //private bool grounded = false;

    public bool usePSNavigationController = true;
    public int PSNaviControllerID = 0;
	public bool strafeInsteadTurning = false;

    public float jumpStrength = 10f;

    private RUISJumpGestureRecognizer jumpGesture;

	public float forwardSpeed { get; private set; }
    public float strafeSpeed { get; private set; }
    public float direction { get; private set; }
	
    // TUUKKA
	public bool useRazerHydra = true;
	public SixenseHands razerHydraID = SixenseHands.RIGHT;
	SixenseInput.Controller razerController;
	
    PSMoveWrapper moveWrapper;

    bool shouldJump = false;

    void Awake()
    {
        characterController = GetComponent<RUISCharacterController>();
        jumpGesture = GetComponentInChildren<RUISJumpGestureRecognizer>();

        // TUUKKA
		if(useRazerHydra && Object.FindObjectOfType(typeof(SixenseInput)) == null)
			Debug.LogError(		"Your settings indicate that you want to use Razer Hydra for "
							+	"character locomotion controls, but your scene is missing "
							+	"SixenseInput script.");
		
		
        moveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;
    }

    void Update()
    {
        if (characterController.grounded && (Input.GetButtonDown("Jump") || JumpGestureTriggered()))
        {
            shouldJump = true;
        }
		
        // TUUKKA
		if(useRazerHydra)
		{
			razerController = SixenseInput.GetController(razerHydraID);
			if(razerController != null && razerController.Enabled)
			{
				if(razerController.GetButtonDown(SixenseButtons.BUMPER))
					shouldJump = true;
			}
		}
		
		// Check if jumping with PS Move Navigation controller
		if (usePSNavigationController && moveWrapper && moveWrapper.isConnected)
        {
            if (PSNaviControllerID < moveWrapper.navConnected.Length && PSNaviControllerID >= 0)
            {
                if (moveWrapper.navConnected[PSNaviControllerID])
                {
                    if(moveWrapper.WasPressed(PSNaviControllerID, "NavL1"))
                    	shouldJump = true;
					
                }
            }
		}
    }

    void FixedUpdate()
    {
        //characterController.ApplyForceInCharacterDirection(translation);

		direction = 0;
		
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		// Check if moving with PS Move Navigation controller
        if (PSNaviControllerID < 0)
        {
            Debug.LogError("PSNaviControllerID was set to " + PSNaviControllerID
                            + " which is incorrect value: It must be positive!");
        }
        else if (usePSNavigationController && moveWrapper && moveWrapper.isConnected)
        {
            if (PSNaviControllerID < moveWrapper.navConnected.Length)
            {
                if (moveWrapper.navConnected[PSNaviControllerID])
                {
                    int horiz = moveWrapper.valueNavAnalogX[PSNaviControllerID];
                    int verti = moveWrapper.valueNavAnalogY[PSNaviControllerID];
                    float extraSpeed = ((float)moveWrapper.valueNavL2[PSNaviControllerID]) / 255f;
                    if (Mathf.Abs(verti) > 20)
                        targetVelocity += new Vector3(0, 0, -((float)verti) / 128f * (1 + extraSpeed));
                    //if(Mathf.Abs(horiz) > 20)
                    //	targetVelocity += new Vector3(((float) horiz)/128f*(1 + extraSpeed), 0, 0);
					
					if(strafeInsteadTurning)
					{
	                    if (Mathf.Abs(horiz) > 20)
	                        targetVelocity += new Vector3(((float)horiz) / 128f * (1 + extraSpeed), 0, 0);
					}
					else
					{
	                    if (Mathf.Abs(horiz) > 10)
	                    {
							if (horiz > 0)
                                direction = 1;
                            else
                                direction = -1;
	                        characterController.RotateAroundCharacterPivot(new Vector3(0, 100 * ((float)horiz) / 128f 
																							  * Time.fixedDeltaTime, 0));
	                    }
					}
                }
            }
            else
            {
                Debug.LogError("PSNaviControllerID was set to " + PSNaviControllerID
                                + " which is too big value: It must be below 7.");
            }
        }
		
        // TUUKKA
		if(useRazerHydra) // Check if moving with Razer Hydra controller
		{
			razerController = SixenseInput.GetController(razerHydraID);
			if(razerController != null && razerController.Enabled)
			{
                    float extraSpeed = razerController.Trigger; 
                    if (Mathf.Abs(razerController.JoystickY) > 0.15f)
                        targetVelocity += new Vector3(0, 0, razerController.JoystickY * (1 + extraSpeed));
					
					if(strafeInsteadTurning)
					{
	                    if (Mathf.Abs(razerController.JoystickX) > 0.15f)
	                        targetVelocity += new Vector3(razerController.JoystickX * (1 + extraSpeed), 0, 0);
					}
					else
					{
	                    if (Mathf.Abs(razerController.JoystickX) > 0.075f)
	                    {
	                        characterController.RotateAroundCharacterPivot(new Vector3(0, 100 * razerController.JoystickX 
																							  * Time.fixedDeltaTime, 0));
	                    }
					}
			}
		}

		forwardSpeed = targetVelocity.z;
        strafeSpeed = targetVelocity.x;
		
        targetVelocity = characterController.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        Vector3 velocity = rigidbody.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
		
        velocityChange.y = 0;
		velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);
        //velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        //velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        //velocityChange.y = 0;
		
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        rigidbody.AddForce(new Vector3(0, -gravity * rigidbody.mass, 0));


        if (Input.GetKey(KeyCode.Q))
        {
            characterController.RotateAroundCharacterPivot(new Vector3(0, -rotationScaler * Time.fixedDeltaTime, 0));

        }
        else if (Input.GetKey(KeyCode.E))
        {
            characterController.RotateAroundCharacterPivot(new Vector3(0, rotationScaler * Time.fixedDeltaTime, 0));
        }


        if (shouldJump)
        {
            rigidbody.AddForce(new Vector3(0, jumpStrength * rigidbody.mass, 0), ForceMode.Impulse);
            shouldJump = false;
        }
    }

    bool JumpGestureTriggered()
    {
        if (jumpGesture == null) return false;

        return jumpGesture.GestureTriggered();
    }
}
