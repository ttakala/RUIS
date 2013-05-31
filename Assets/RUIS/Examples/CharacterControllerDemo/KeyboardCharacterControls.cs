using UnityEngine;
using System.Collections;

public class KeyboardCharacterControls : MonoBehaviour {
    RUISCharacterController characterController;

    public float forwardSpeed = 3.0f;
    public float strafeSpeed = 2.0f;

    public float rotationScaler = 60.0f;

	void Start () {
        characterController = GetComponent<RUISCharacterController>();
	}
	
	void FixedUpdate () {
        Vector3 translation = Input.GetAxis("Horizontal") * Vector3.right * strafeSpeed * Time.fixedDeltaTime + Input.GetAxis("Vertical") * Vector3.forward * forwardSpeed * Time.fixedDeltaTime;
        characterController.TranslateInCharacterCoordinates(translation);

        if (Input.GetKey(KeyCode.Q))
        {
            characterController.RotateAroundCharacterPivot(new Vector3(0, -rotationScaler * Time.fixedDeltaTime, 0));
        }
        else if (Input.GetKey(KeyCode.E))
        {
            characterController.RotateAroundCharacterPivot(new Vector3(0, rotationScaler * Time.fixedDeltaTime, 0));
        }
	}
}
