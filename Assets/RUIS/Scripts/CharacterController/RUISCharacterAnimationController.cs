/*****************************************************************************

Content    :   Functionality to send RUISCharacterLocomotionControl and RUISCharacterController info forward to a Mecanim Animator
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/


using UnityEngine;
using System.Collections;

public class RUISCharacterAnimationController : MonoBehaviour {
    private Animator animator;
    public RUISCharacterLocomotionControl locomotionControl;
    public RUISCharacterController characterController;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

	void Update () {
        animator.SetBool("Grounded", characterController.grounded);
        animator.SetFloat("ForwardSpeed", locomotionControl.forwardSpeed);
        animator.SetFloat("StrafeSpeed", locomotionControl.strafeSpeed);
        animator.SetFloat("Direction", locomotionControl.direction);
	}
}
