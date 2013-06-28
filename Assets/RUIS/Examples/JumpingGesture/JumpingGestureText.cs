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
