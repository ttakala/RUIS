/*****************************************************************************

Content    :   Functionality to send RUISCharacterLocomotionControl and RUISCharacterController info forward to a Mecanim Animator
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/


using UnityEngine;
using System.Collections;

public class RUISCharacterAnimationController : MonoBehaviour {
    public Animator animator;
    public RUISCharacterLocomotionControl locomotionControl;
    public RUISCharacterController characterController;
    public RUISKinectAndMecanimCombiner animationCombiner;

	void Update () {
        if (!animator) return;
        animator.SetBool("Grounded", characterController.grounded);
        animator.SetFloat("ForwardSpeed", locomotionControl.forwardSpeed);
        animator.SetFloat("StrafeSpeed", locomotionControl.strafeSpeed);
        animator.SetFloat("Direction", locomotionControl.direction);
        animator.SetBool("Jump", locomotionControl.jump);

        if (characterController.grounded)
        {
            float maxOfForwardOrStrafe = Mathf.Max(Mathf.Abs(locomotionControl.forwardSpeed), Mathf.Abs(locomotionControl.strafeSpeed));
            animationCombiner.leftLegBlendWeight = Mathf.Clamp01(maxOfForwardOrStrafe);
            animationCombiner.rightLegBlendWeight = Mathf.Clamp01(maxOfForwardOrStrafe);
        }
        else
        {
            animationCombiner.leftLegBlendWeight = 1;
            animationCombiner.rightLegBlendWeight = 1;
        }
	}
}
