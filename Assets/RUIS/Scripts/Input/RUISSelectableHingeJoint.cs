﻿/*****************************************************************************

Content    :   Implements selection behavior for RUISWands
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSelectableHingeJoint : RUISSelectable {
	
	public float springForce = 10;
	private float originalAngluarDrag;
	private bool isSelected;
	private Vector3 originalHingeForward;
	
	private float targetAngleOnSelection;
	private float angleOnSelection;
	private Vector3 jointAxisInGlobalCoordinates;
	
	private JointSpring originalJointSpring;
	
	void Start() 
	{
		this.originalAngluarDrag = transform.rigidbody.angularDrag; 
		this.jointAxisInGlobalCoordinates = transform.TransformDirection(transform.hingeJoint.axis);
		Vector3 objectCenterProjectedOnPlane = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, hingeJoint.connectedAnchor, transform.position);
		
		this.originalHingeForward = hingeJoint.connectedAnchor - objectCenterProjectedOnPlane;
		if(this.originalHingeForward == Vector3.zero) {
			if(jointAxisInGlobalCoordinates.normalized == transform.forward) {
				this.originalHingeForward = transform.right;
			}
			else {
			 	this.originalHingeForward =  transform.forward;
			}
		}
		
		if(this.physicalSelection) hingeJoint.useSpring = true;
	}
	
	public override void OnSelection(RUISWandSelector selector)
	{
		this.selector = selector;
		this.isSelected = true;
		positionAtSelection = selector.selectionRayEnd;
		rotationAtSelection = transform.rotation;
		selectorPositionAtSelection = selector.transform.position;
		selectorRotationAtSelection = selector.transform.rotation;
		distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude; // Dont remove this, needed in the inherited class
		this.angleOnSelection = transform.hingeJoint.angle;
		this.targetAngleOnSelection = calculateAngleDifferenceFromOrig();
		
		this.originalJointSpring = transform.hingeJoint.spring;
	}
	
	public override void OnSelectionEnd()
	{
		if (rigidbody)
		{
			rigidbody.isKinematic = rigidbodyWasKinematic;
			if(continuousCollisionDetectionWhenSelected)
			{
				switchToOldCollisionMode = true;
			}
		}
		if(selectionMaterial != null)
			RemoveMaterialFromEverything();
		
		
		this.selector = null;
		this.isSelected = false;
		
		transform.hingeJoint.spring = this.originalJointSpring;
	}
	
	public void FixedUpdate()
	{
		this.UpdateTransform(true);
	}
	
	private float calculateAngleDifferenceFromOrig() {
		Vector3 jointAxisInGlobalCoordinates = transform.TransformDirection(transform.hingeJoint.axis);
		Vector3 newManipulationPoint = getManipulationPoint();
		Quaternion newManipulationRotation = getManipulationRotation();
		Vector3 projectedPoint = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, hingeJoint.connectedAnchor, newManipulationPoint);
		Vector3 fromHingeToProjectedPoint = hingeJoint.connectedAnchor - projectedPoint;
		float angleDirection = Vector3.Dot(Vector3.Cross(this.originalHingeForward, fromHingeToProjectedPoint).normalized, jointAxisInGlobalCoordinates);
		float angleDifferenceFromOrig = Vector3.Angle(this.originalHingeForward , fromHingeToProjectedPoint) * angleDirection;
		
		// For debug
		Debug.DrawLine(hingeJoint.connectedAnchor, projectedPoint, Color.blue);
		Debug.DrawLine(hingeJoint.connectedAnchor, newManipulationPoint, Color.red);
		Debug.DrawLine(hingeJoint.connectedAnchor, Vector3.Cross(this.originalHingeForward, fromHingeToProjectedPoint).normalized + hingeJoint.connectedAnchor, Color.cyan);
		DrawPlane(hingeJoint.connectedAnchor, jointAxisInGlobalCoordinates);	
		
		return angleDifferenceFromOrig;
		
	}
	
	protected override void UpdateTransform(bool safePhysics)
	{
		if (!isSelected) return;
		
		float angleDifferenceFromOrig = calculateAngleDifferenceFromOrig();
		
		
		if(this.physicalSelection) {
			JointSpring spr = hingeJoint.spring;
			spr.spring = springForce;
			spr.targetPosition = angleDifferenceFromOrig - targetAngleOnSelection + this.angleOnSelection;
			hingeJoint.spring = spr;
		}
		else {
			transform.RotateAround(transform.hingeJoint.connectedAnchor, this.jointAxisInGlobalCoordinates, angleDifferenceFromOrig - transform.hingeJoint.angle  - targetAngleOnSelection + this.angleOnSelection);
		}
		
	}
	
	// For debug : http://answers.unity3d.com/questions/467458/how-to-debug-drawing-plane.html
	public void DrawPlane(Vector3 position , Vector3 normal) {
		Vector3 v3;
		if (normal.normalized != Vector3.forward)
			v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
		else
			v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;;
		Vector3 corner0 = position + v3;
		Vector3 corner2 = position - v3;
		
		Quaternion q = Quaternion.AngleAxis(90.0f, normal);
		v3 = q * v3;
		Vector3 corner1 = position + v3;
		Vector3 corner3 = position - v3;
		Debug.DrawLine(corner0, corner2, Color.green);
		Debug.DrawLine(corner1, corner3, Color.green);
		Debug.DrawLine(corner0, corner1, Color.green);
		Debug.DrawLine(corner1, corner2, Color.green);
		Debug.DrawLine(corner2, corner3, Color.green);
		Debug.DrawLine(corner3, corner0, Color.green);
		//Debug.DrawRay(position, normal, Color.red);
	}
	
}
