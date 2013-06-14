using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RUISCharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class KeyboardCharacterControls : MonoBehaviour {
    RigidbodyFPSController fpsController;
    RUISCharacterController characterController;
    RUISCharacterStabilizingCollider stabilizingCollider;

    public float rotationScaler = 60.0f;

    public float speed = 2.0f;
    public float gravity = 10.0f;
    public float maxVelocityChange = 10.0f;
    //public bool canJump = true;
    //public float jumpHeight = 2.0f;
    //private bool grounded = false;

	void Start () {
        characterController = GetComponent<RUISCharacterController>();
        stabilizingCollider = GetComponentInChildren<RUISCharacterStabilizingCollider>();
	}
	
	void FixedUpdate () {
        //characterController.ApplyForceInCharacterDirection(translation);

        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
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

        //if(
	}
}
