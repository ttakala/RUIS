/*****************************************************************************

Content    :   Class for locomotion of Kinect controlled character
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISCharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class RUISCharacterLocomotionControl : MonoBehaviour {
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

    public float jumpStrength = 10f;

    private RUISJumpGestureRecognizer jumpGesture;

	// TUUKKA
	PSMoveWrapper moveWrapper;

    bool shouldJump = false;

	void Awake () {
        characterController = GetComponent<RUISCharacterController>();
        jumpGesture = GetComponentInChildren<RUISJumpGestureRecognizer>();
		
		// TUUKKA
		moveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;
	}

    void Update()
    {
        if (Input.GetButtonDown("Jump") || JumpGestureTriggered())
        {
            shouldJump = true;
        }
    }
	
	void FixedUpdate () {
        //characterController.ApplyForceInCharacterDirection(translation);

        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		
		if(PSNaviControllerID < 0)
		{
			Debug.LogError("PSNaviControllerID was set to " + PSNaviControllerID
							+ " which is incorrect value: It must be positive!");
		}
		else if(moveWrapper && moveWrapper.isConnected)
		{
			if(PSNaviControllerID < moveWrapper.navConnected.Length)
			{
				if(moveWrapper.navConnected[PSNaviControllerID])
				{
					int horiz = moveWrapper.valueNavAnalogX[PSNaviControllerID];
					int verti = moveWrapper.valueNavAnalogY[PSNaviControllerID];
					float extraSpeed = ((float) moveWrapper.valueNavL2[PSNaviControllerID])/255f;
					if(Mathf.Abs(verti) > 20)
						targetVelocity += new Vector3(0, 0, -((float) verti)/128f*(1 + extraSpeed));
					//if(Mathf.Abs(horiz) > 20)
					//	targetVelocity += new Vector3(((float) horiz)/128f*(1 + extraSpeed), 0, 0);
                    if (Mathf.Abs(horiz) > 10)
                    {
                        characterController.RotateAroundCharacterPivot(new Vector3(0, 100 * ((float)horiz) / 128f * Time.fixedDeltaTime, 0));
                    }
				}
			}
			else
			{
				Debug.LogError("PSNaviControllerID was set to " + PSNaviControllerID
								+ " which is too big value: It must be below 7.");
			}
		}
		
        targetVelocity = characterController.TransformDirection(targetVelocity);
        targetVelocity *= speed;

        Vector3 velocity = rigidbody.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
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
