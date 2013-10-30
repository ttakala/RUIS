/*****************************************************************************

Content    :   Class for locomotion of Kinect controlled character
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISCharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class RUISCharacterLocomotion : MonoBehaviour
{
    RUISCharacterController characterController;
	RUISInputManager inputManager;
	
    public KeyCode turnRightKey = KeyCode.E;
    public KeyCode turnLeftKey = KeyCode.Q;

    public float rotationScaler = 60.0f;
	private float turnMagnitude = 0;

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
	public float jumpSpeedEffect = 0;
	public float aerialMobility = 0.1f;
	
    private Vector3 velocity;
    private Vector3 velocityChange;
	private bool airborne = false;
	private bool grounded = true;
	private bool colliding = true;
	private Vector3 accumulatedAirboneVelocity = new Vector3(0, 0, 0);
	private Vector3 jumpTimeVelocity = new Vector3(0, 0, 0);
	
	
	float extraSpeed = 0;
	private float timeSinceJump = 0;

    private RUISJumpGestureRecognizer jumpGesture;

	public float forwardSpeed { get; private set; }
    public float strafeSpeed { get; private set; }
    public float direction { get; private set; }
    public bool jump { get; private set; }
	
    // TUUKKA
	public bool useRazerHydra = true;
	public SixenseHands razerHydraID = SixenseHands.RIGHT;
	SixenseInput.Controller razerController;
	
    PSMoveWrapper moveWrapper;

    bool shouldJump = false;

    private float animationBlendStrength = 10.0f;

    void Awake()
    {
        characterController = GetComponent<RUISCharacterController>();
        jumpGesture = GetComponentInChildren<RUISJumpGestureRecognizer>();

        moveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;

        try
        {
            Input.GetAxis("Sprint");
        }
        catch (UnityException)
        {
            Debug.LogWarning("Sprint Axis not set");
        }

        try
        {
            Input.GetAxis("Right Analog Stick");
        }
        catch (UnityException)
        {
            Debug.LogWarning("Right Analog Stick Axis not set");
        }

    }
	
	void Start()
	{
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		
		if(useRazerHydra && inputManager && !inputManager.enableRazerHydra)
		{
			useRazerHydra = false;
			Debug.LogWarning(	"Your settings indicate that you want to use Razer Hydra for "
							 +	"character locomotion controls, but you have disabled Razer "
							 +	"Hydra from RUIS InputManager.");
		}
		
		if(usePSNavigationController && inputManager && !inputManager.enablePSMove)
		{
			usePSNavigationController = false;
			Debug.LogWarning(	"Your settings indicate that you want to use PS Navigation "
							 +	"controller for character locomotion controls, but you have "
							 +	"disabled PS Move from RUIS InputManager.");
		}
		
	}

    void Update()
    {
        jump = false;

        if(characterController == null || !characterController.grounded)
			return;
        
        if ((Input.GetButtonDown("Jump") || JumpGestureTriggered()))
        {
            shouldJump = true;
        }
		
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
					{
                    	shouldJump = true;
					}
                }
            }
		}

        if (shouldJump)
        {
            jump = true;
        }
    }

    void FixedUpdate()
    {
        //characterController.ApplyForceInCharacterDirection(translation);

        direction = 0;
		turnMagnitude = 0;
		
		if(characterController != null)
		{
			grounded  = characterController.grounded;
			colliding = characterController.colliding;
		}
		
		if(grounded || colliding)
		{
			airborne = false;
			accumulatedAirboneVelocity = Vector3.zero;
		}
		else
		{
			timeSinceJump += Time.fixedDeltaTime;
			if(!airborne)
			{
				jumpTimeVelocity = rigidbody.velocity;
				timeSinceJump = 0;
			}
			airborne = true;
		}
			
        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        {
            try
            {
                extraSpeed = Input.GetAxis("Sprint");
                if (!airborne)
                    targetVelocity *= 1 + extraSpeed;
            }
            catch (UnityException) { }
        }

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
					if(!airborne)
	                    extraSpeed = ((float)moveWrapper.valueNavL2[PSNaviControllerID]) / 255f;
					else
						extraSpeed = 0;
                    if (Mathf.Abs(verti) > 20)
                        targetVelocity += new Vector3(0, 0, -((float)verti) / 128f * (1 + extraSpeed));

                    if (strafeInsteadTurning)
                    {
                        if (Mathf.Abs(horiz) > 20)
                            targetVelocity += new Vector3(((float)horiz) / 128f * (1 + extraSpeed), 0, 0);
                    }
                    else
                    {
                        if (Mathf.Abs(horiz) > 10)
                        {
							turnMagnitude += ((float)horiz) / 128f;
                        }
                    }
					
					if(moveWrapper.isNavButtonCross[PSNaviControllerID])
						turnMagnitude -= 1;
					if(moveWrapper.isNavButtonCircle[PSNaviControllerID])
						turnMagnitude += 1;
                }
            }
            else
            {
                Debug.LogError("PSNaviControllerID was set to " + PSNaviControllerID
                                + " which is too big value: It must be below 7.");
            }
        }

        // TUUKKA
        if (useRazerHydra) // Check if moving with Razer Hydra controller
        {
            razerController = SixenseInput.GetController(razerHydraID);
            if (razerController != null && razerController.Enabled)
            {
				if(!airborne)
					if(razerController.GetButton(SixenseButtons.JOYSTICK))
		                extraSpeed = 1; //razerController.Trigger;
				else
					extraSpeed = 0;
				
                if (Mathf.Abs(razerController.JoystickY) > 0.15f)
                    targetVelocity += new Vector3(0, 0, razerController.JoystickY * (1 + extraSpeed));

                if (strafeInsteadTurning)
                {
                    if (Mathf.Abs(razerController.JoystickX) > 0.15f)
                        targetVelocity += new Vector3(razerController.JoystickX * (1 + extraSpeed), 0, 0);
				}
                else
                {
                    if (Mathf.Abs(razerController.JoystickX) > 0.075f)
                    {
						turnMagnitude += razerController.JoystickX;
                    }
                }
				
				if(razerController.GetButton(SixenseButtons.THREE))
					turnMagnitude -= 1;
				if(razerController.GetButton(SixenseButtons.FOUR))
					turnMagnitude += 1;
            }
        }
		
		// Limit of two comes from [0,1] + extraSpeed
		targetVelocity = Vector3.ClampMagnitude(targetVelocity, 2);

        forwardSpeed = Mathf.Lerp(forwardSpeed, targetVelocity.z, Time.deltaTime * animationBlendStrength);
        strafeSpeed = Mathf.Lerp(strafeSpeed, targetVelocity.x, Time.deltaTime * animationBlendStrength);
		
        targetVelocity = characterController.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        velocity = rigidbody.velocity;
        velocityChange = (targetVelocity - velocity);

        velocityChange.y = 0;
        velocityChange = Vector3.ClampMagnitude(velocityChange, Time.deltaTime * maxVelocityChange);
		
		if(!airborne)
		{
        	rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
		}
		else
		{
			// Below is very hacky ***
			accumulatedAirboneVelocity += aerialMobility*targetVelocity;
			Vector3 temp = new Vector3(jumpTimeVelocity.x, 0, jumpTimeVelocity.z);
			velocityChange = temp.normalized*Vector3.Dot(accumulatedAirboneVelocity, temp.normalized);
			if(Vector3.Dot(velocityChange, temp) > 0)
			{
				accumulatedAirboneVelocity -= velocityChange;
				accumulatedAirboneVelocity = Vector3.ClampMagnitude(accumulatedAirboneVelocity, speed);
			}
			else
			{
				accumulatedAirboneVelocity = Vector3.ClampMagnitude(accumulatedAirboneVelocity, speed);
				
				jumpTimeVelocity *= Mathf.Clamp01(1-0.1f*timeSinceJump);
				
			}
			
			velocityChange = jumpTimeVelocity + accumulatedAirboneVelocity - velocity;
        	velocityChange.y = 0;
        	velocityChange = Vector3.ClampMagnitude(velocityChange, Time.deltaTime * maxVelocityChange);
			
			rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
		}

        //rigidbody.AddForce(new Vector3(0, -gravity * rigidbody.mass, 0));

        try
        {
            turnMagnitude += Input.GetAxis("Right Analog Stick");
        }
        catch (UnityException) { }

		if(Input.GetKey(turnLeftKey))
            turnMagnitude -= 1;
        if(Input.GetKey(turnRightKey))
            turnMagnitude += 1;
			
		if(turnMagnitude != 0)
			characterController.RotateAroundCharacterPivot(new Vector3(0, turnMagnitude * rotationScaler * Time.fixedDeltaTime, 0));
		
        if (shouldJump)
        {
            rigidbody.AddForce(new Vector3(0, Mathf.Sqrt((1 + 0.5f*Mathf.Abs(forwardSpeed)*jumpSpeedEffect) * jumpStrength) 
																			* rigidbody.mass, 0), ForceMode.Impulse);
			if(characterController)
				characterController.lastJumpTime = Time.fixedTime;
			
            shouldJump = false;
        }
		
    }

    bool JumpGestureTriggered()
    {
        if (jumpGesture == null) return false;

        return jumpGesture.GestureTriggered();
    }
}
