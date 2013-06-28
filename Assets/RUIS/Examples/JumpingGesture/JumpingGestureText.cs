/*****************************************************************************

Content    :   Visualization text for RUISJumpGestureRecognizer
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System;

public class JumpingGestureText : MonoBehaviour {
    public RUISJumpGestureRecognizer gestureRecognizer;
    public RUISPointTracker pointTracker;
    public TextMesh textMesh;

	void Start () {
	
	}
	
	void Update () {
        textMesh.text = Enum.GetName(typeof(RUISJumpGestureRecognizer.State), gestureRecognizer.currentState) + '\n';
            textMesh.text += gestureRecognizer.leftFootHeight + " " + gestureRecognizer.rightFootHeight + '\n';
        //else
        //{
            textMesh.text += pointTracker.averageVelocity.y;
        //}
	}
}
