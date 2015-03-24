/*****************************************************************************

Content    :   Implements selection behavior for RUISWands
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSelectableHingeJoint : RUISSelectable {
	
	private float springForce = 10; // Hidden until physical selection works
	
	public bool resetTargetOnRelease;
	private Vector3 hingeForward;
	private Vector3 hingeForwardOnStart;
	private Vector3 connectedBodyForwardOnStart;
	
	private float targetAngleOnSelection;
	private float angleOnSelection;
	private Vector3 hingeForwardOnSelection;
	
		
	private int hingeForwardType;
	private bool useGravityOriginalValue;
	private Vector3 jointAxisInGlobalCoordinates;
	private JointSpring originalJointSpring;
	private Vector3 connectedAnchor;
	
	public float outputAngle;
	
	void Start() 
	{
		updateHingePositionAndAxis();
		updateHingeForward();
		this.hingeForwardOnStart = this.hingeForward;
		
		if(GetComponent<HingeJoint>().connectedBody) 
			this.connectedBodyForwardOnStart = GetComponent<HingeJoint>().connectedBody.transform.forward;
		
		if(this.physicalSelection) GetComponent<HingeJoint>().useSpring = true;
	}
	
	private void updateHingeForward()
	{
		Vector3 objectCenterProjectedOnPlane = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, this.connectedAnchor, transform.position);
		this.hingeForward = this.connectedAnchor - objectCenterProjectedOnPlane;
		if(this.hingeForward == Vector3.zero) {
			if(jointAxisInGlobalCoordinates.normalized == transform.forward) {
				this.hingeForward = transform.right;
				this.hingeForwardType = 1;
			}
			else {
				this.hingeForward =  transform.forward;
				this.hingeForwardType = 2;
			}
		}
		else 
		{
			this.hingeForwardType = 0;
		}
	}
	
	
	
	public override void OnSelection(RUISWandSelector selector)
	{
		updateHingePositionAndAxis();
		updateHingeForward();
		
		this.selector = selector;
		positionAtSelection = selector.selectionRayEnd;
		rotationAtSelection = transform.rotation;
		selectorPositionAtSelection = selector.transform.position;
		selectorRotationAtSelection = selector.transform.rotation;
		distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude; // Dont remove this, needed in the inherited class
		
		this.hingeForwardOnSelection = this.hingeForward;
		
		this.angleOnSelection = getHingeJointAngle();
		this.targetAngleOnSelection = getAngleDifferenceFromHingeForwardOnStart();
		
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
	
	private float getAngleDifferenceFromHingeForwardOnStart() {
		Quaternion connectedBodyRotationCorrection = Quaternion.identity;
		if(GetComponent<HingeJoint>().connectedBody) 
			connectedBodyRotationCorrection = GetComponent<HingeJoint>().connectedBody.transform.rotation; 
		return -calculateAngleDifferenceFromFoward(connectedBodyRotationCorrection * this.hingeForwardOnStart);
	}
	
	public override void OnSelectionEnd()
	{
		transform.GetComponent<Rigidbody>().useGravity = this.useGravityOriginalValue;
		
		if(selectionMaterial != null)
			RemoveMaterialFromEverything();
		
		this.selector = null;
		
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
			spr.targetPosition = getHingeJointAngle();
			GetComponent<HingeJoint>().spring = spr;
		}
		else 
		{
			transform.GetComponent<HingeJoint>().spring = this.originalJointSpring;
		}
	}
	
	public new void FixedUpdate()
	{
		this.UpdateTransform(true);
	}
	
	private void updateHingePositionAndAxis() {
		if(GetComponent<HingeJoint>().connectedBody) 
			this.connectedAnchor = GetComponent<HingeJoint>().connectedBody.transform.TransformPoint(GetComponent<HingeJoint>().connectedAnchor);
		else
			this.connectedAnchor = GetComponent<HingeJoint>().connectedAnchor;
		this.jointAxisInGlobalCoordinates = transform.TransformDirection(transform.GetComponent<HingeJoint>().axis);
	}
	
	
	private float calculateAngleBetweenVectors(Vector3 vectorA, Vector3 vectorB, Vector3 planeNormal)
	{
		float angleDirection = Vector3.Dot(Vector3.Cross(vectorA, vectorB).normalized, jointAxisInGlobalCoordinates);
		float angleDifference = Vector3.Angle(vectorA , vectorB) * angleDirection;
		return angleDifference;
	}
	
	private float calculateAngleDifferenceFromFoward(Vector3 forwardVector) {
		Vector3 jointAxisInGlobalCoordinates = transform.TransformDirection(transform.GetComponent<HingeJoint>().axis);
		Vector3 newManipulationPoint = getManipulationPoint();
		Vector3 projectedPoint = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, this.connectedAnchor, newManipulationPoint);
		Vector3 fromHingeToProjectedPoint = this.connectedAnchor - projectedPoint;
		
		float angleDifferenceFromForward = calculateAngleBetweenVectors(forwardVector, fromHingeToProjectedPoint, jointAxisInGlobalCoordinates);
		
		// For debug
		Debug.DrawLine(this.connectedAnchor, projectedPoint, Color.blue);
		Debug.DrawLine(this.connectedAnchor, newManipulationPoint, Color.red);
		Debug.DrawLine(this.connectedAnchor, Vector3.Cross(forwardVector, fromHingeToProjectedPoint).normalized + this.connectedAnchor, Color.cyan);
		DrawPlane(this.connectedAnchor, jointAxisInGlobalCoordinates);	
		
		return angleDifferenceFromForward;
		
	}
	
	private float getHingeJointAngle() {
		updateHingeForward();
		Quaternion connectedBodyRotationCorrection = Quaternion.identity;
		if(GetComponent<HingeJoint>().connectedBody) 
			connectedBodyRotationCorrection = GetComponent<HingeJoint>().connectedBody.transform.rotation; 
		float hingeAngle = calculateAngleBetweenVectors(this.hingeForward, connectedBodyRotationCorrection * this.hingeForwardOnStart, jointAxisInGlobalCoordinates);
		return hingeAngle;
	}
	
	
	protected override void UpdateTransform(bool safePhysics)
	{
		updateHingePositionAndAxis();
		updateHingeForward();
		this.outputAngle = getHingeJointAngle();
		
		if (!isSelected) return;
		updateHingePositionAndAxis();
		updateHingeForward();
		
		float hingeJointAngle = getHingeJointAngle();	
		float angleDifferenceFromStart = getAngleDifferenceFromHingeForwardOnStart();
		
		if(this.physicalSelection) {
			// http://forum.unity3d.com/threads/assign-hingejoint-spring-targetposition-in-c.105427/
			JointSpring spr = GetComponent<HingeJoint>().spring;
			spr.spring = springForce;
			spr.targetPosition = angleDifferenceFromStart - (targetAngleOnSelection + this.angleOnSelection);
			GetComponent<HingeJoint>().spring = spr;
		}
		else {
			transform.RotateAround(
				transform.position, 
				this.jointAxisInGlobalCoordinates, 
				(hingeJointAngle - angleDifferenceFromStart)
				- 
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
