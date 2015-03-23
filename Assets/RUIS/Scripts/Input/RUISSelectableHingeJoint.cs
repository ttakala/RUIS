/*****************************************************************************

Content    :   Implements selection behavior for RUISWands
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSelectableHingeJoint : RUISSelectable {
	
	public float springForce = 10;
	public bool resetTargetOnRelease;
	private float originalAngluarDrag;
	private bool isSelected;
	private Vector3 originalHingeForward;
	private int hingeForwardType;
	private bool useGravityOriginalValue;
	
	private float targetAngleOnSelection;
	private float angleOnSelection;
	private Vector3 jointAxisInGlobalCoordinates;
	
	private JointSpring originalJointSpring;
	
	void Start() 
	{
		this.originalAngluarDrag = transform.GetComponent<Rigidbody>().angularDrag; 
		this.jointAxisInGlobalCoordinates = transform.TransformDirection(transform.GetComponent<HingeJoint>().axis);
		Vector3 objectCenterProjectedOnPlane = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, GetComponent<HingeJoint>().connectedAnchor, transform.position);;
		
		this.originalHingeForward = GetComponent<HingeJoint>().connectedAnchor - objectCenterProjectedOnPlane;
		if(this.originalHingeForward == Vector3.zero) {
			if(jointAxisInGlobalCoordinates.normalized == transform.forward) {
				this.originalHingeForward = transform.right;
				this.hingeForwardType = 1;
			}
			else {
			 	this.originalHingeForward =  transform.forward;
				this.hingeForwardType = 2;
			}
		}
		else 
		{
			this.hingeForwardType = 0;
		}
		
		if(this.physicalSelection) GetComponent<HingeJoint>().useSpring = true;
	}
	
	private float getHingeJointAngle() {
		
		Vector3 currentHingeJointForwardVector = Vector3.forward;
		
		switch(this.hingeForwardType) {
			case 0: 
				Vector3 objectCenterProjectedOnPlane = MathUtil.ProjectPointOnPlane(this.jointAxisInGlobalCoordinates, GetComponent<HingeJoint>().connectedAnchor, transform.position);
				currentHingeJointForwardVector = GetComponent<HingeJoint>().connectedAnchor - objectCenterProjectedOnPlane;
				break;
			case 1:
				currentHingeJointForwardVector = transform.right;
				break;
			case 2:
				currentHingeJointForwardVector = transform.forward;
				break;
		}
		
		float hingeAngleDirection = Vector3.Dot(Vector3.Cross(this.originalHingeForward, currentHingeJointForwardVector).normalized, jointAxisInGlobalCoordinates);
		float hingeJointAngle = Vector3.Angle(currentHingeJointForwardVector, this.originalHingeForward) * hingeAngleDirection;
		return hingeJointAngle;
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
		this.angleOnSelection = getHingeJointAngle();
		this.targetAngleOnSelection = calculateAngleDifferenceFromOrig();
		
		this.originalJointSpring = transform.GetComponent<HingeJoint>().spring;
		
		if(!physicalSelection) {
			JointSpring spr = GetComponent<HingeJoint>().spring;
			spr.spring = 0;
			spr.targetPosition = this.originalJointSpring.targetPosition;
			spr.damper = this.originalJointSpring.damper;
			GetComponent<HingeJoint>().spring = spr;
			this.useGravityOriginalValue = transform.GetComponent<Rigidbody>().useGravity;
			transform.GetComponent<Rigidbody>().useGravity = false;	
		}
	}
	
	public override void OnSelectionEnd()
	{
		
		transform.GetComponent<Rigidbody>().useGravity = this.useGravityOriginalValue;
		
		if(selectionMaterial != null)
			RemoveMaterialFromEverything();
		
		this.selector = null;
		this.isSelected = false;
		
		if(!physicalSelection) {
			transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
			//transform.rigidbody.Sleep();
		}
		
		if(resetTargetOnRelease) 
		{
			JointSpring spr = GetComponent<HingeJoint>().spring;
			spr.spring = this.originalJointSpring.spring;
			spr.damper = this.originalJointSpring.damper;
			spr.targetPosition = -getHingeJointAngle();
			GetComponent<HingeJoint>().spring = spr;
		}
		else 
		{
			transform.GetComponent<HingeJoint>().spring = this.originalJointSpring;
		}
	}
	
	public void FixedUpdate()
	{
		this.UpdateTransform(true);
	}
	
	private float calculateAngleDifferenceFromOrig() {
		Vector3 jointAxisInGlobalCoordinates = transform.TransformDirection(transform.GetComponent<HingeJoint>().axis);
		Vector3 newManipulationPoint = getManipulationPoint();
		Quaternion newManipulationRotation = getManipulationRotation();
		Vector3 projectedPoint = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, GetComponent<HingeJoint>().connectedAnchor, newManipulationPoint);
		Vector3 fromHingeToProjectedPoint = GetComponent<HingeJoint>().connectedAnchor - projectedPoint;
		float angleDirection = Vector3.Dot(Vector3.Cross(this.originalHingeForward, fromHingeToProjectedPoint).normalized, jointAxisInGlobalCoordinates);
		float angleDifferenceFromOrig = Vector3.Angle(this.originalHingeForward , fromHingeToProjectedPoint) * angleDirection;
		
		// For debug
		Debug.DrawLine(GetComponent<HingeJoint>().connectedAnchor, projectedPoint, Color.blue);
		Debug.DrawLine(GetComponent<HingeJoint>().connectedAnchor, newManipulationPoint, Color.red);
		Debug.DrawLine(GetComponent<HingeJoint>().connectedAnchor, Vector3.Cross(this.originalHingeForward, fromHingeToProjectedPoint).normalized + GetComponent<HingeJoint>().connectedAnchor, Color.cyan);
		DrawPlane(GetComponent<HingeJoint>().connectedAnchor, jointAxisInGlobalCoordinates);	
		
		return angleDifferenceFromOrig;
		
	}
	
	protected override void UpdateTransform(bool safePhysics)
	{
		if (!isSelected) return;
		
		float angleDifferenceFromOrig = calculateAngleDifferenceFromOrig();
		float hingeJointAngle = getHingeJointAngle();
		
		if(this.physicalSelection) {
			// http://forum.unity3d.com/threads/assign-hingejoint-spring-targetposition-in-c.105427/
			JointSpring spr = GetComponent<HingeJoint>().spring;
			spr.spring = springForce;
			spr.targetPosition = angleDifferenceFromOrig - (targetAngleOnSelection + this.angleOnSelection);
			GetComponent<HingeJoint>().spring = spr;
		}
		else {
			       
			transform.RotateAround(
				transform.position, 
				this.jointAxisInGlobalCoordinates, 
				(angleDifferenceFromOrig - hingeJointAngle)
				+ 
				(this.angleOnSelection - targetAngleOnSelection) // Prevent "angle snap" on selection
				);
			
		}
		
	}
	
	
	public override Vector3 getManipulationPoint()
	{
		switch (selector.positionSelectionGrabType)
		{
			case RUISWandSelector.SelectionGrabType.SnapToWand:
				return selector.transform.position;
			case RUISWandSelector.SelectionGrabType.RelativeToWand:
				Vector3 selectorPositionChange = selector.transform.position - selectorPositionAtSelection;
				return positionAtSelection + selectorPositionChange;
			case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
				float clampDistance = distanceFromSelectionRayOrigin;
				if (clampToCertainDistance) clampDistance = distanceToClampTo;
				return selector.selectionRay.origin + clampDistance * selector.selectionRay.direction;
			case RUISWandSelector.SelectionGrabType.DoNotGrab:
				return transform.position;
		}
		return transform.position;
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
