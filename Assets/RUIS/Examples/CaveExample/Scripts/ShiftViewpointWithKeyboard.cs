/*****************************************************************************

Content    :   A class to move the transform around using the keyboard
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class ShiftViewpointWithKeyboard : MonoBehaviour {
    public float movementScaler = 1;
	public float rotationScaler = 180f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * movementScaler);
        transform.Translate(transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * movementScaler);
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(transform.up * Time.deltaTime * movementScaler);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(-transform.up * Time.deltaTime * movementScaler);
        }
		transform.Rotate (transform.up * (Input.GetKey (KeyCode.Z) ? -1 : 0) * Time.deltaTime * rotationScaler);
		transform.Rotate (transform.up * (Input.GetKey (KeyCode.C) ? 1 : 0) * Time.deltaTime * rotationScaler);
	}
	
}
