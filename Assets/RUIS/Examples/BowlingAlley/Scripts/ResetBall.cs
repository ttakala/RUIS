/*****************************************************************************

Content    :   Functionality to reset the ball position
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2016 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class ResetBall : MonoBehaviour {
//    public RUISPSMoveWand controller;
	public RUISOpenVrWand controller;
    public Transform ballResetSpot;

    private bool shouldResetBall = true;

    void FixedUpdate()
    {
		if(shouldResetBall || (controller && controller.MenuButtonWasPressed)) //controller.moveButtonWasPressed)
        {
            transform.position = ballResetSpot.transform.position;
            transform.rotation = ballResetSpot.transform.rotation;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            shouldResetBall = false;
        }
    }

    void OnTriggerEnter(Collider collider)
    {
        shouldResetBall = true;
    }
}
