/*****************************************************************************

Content    :   Implements selection behavior for RUISWands
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSelectableJoint : RUISSelectable {
	
	public Transform grabPoint;
	public Transform hingePoint;
	private float originalAngluarDrag;
	private Vector3 originalRotationVector;
	
	void Start() 
	{
		this.originalAngluarDrag = transform.rigidbody.angularDrag;
		
		Vector3 grabPointPosition = grabPoint.position;
		Vector3 hingePointPosition = hingePoint.position;
		this.originalRotationVector =  hingePointPosition - grabPointPosition;
	}
	
//	public override void OnSelection(RUISWandSelector selector)
//	{
//		this.selector = selector;
//		this.isSelected = true;
//		positionAtSelection = transform.position;
//		rotationAtSelection = transform.rotation;
//		selectorPositionAtSelection = selector.transform.position;
//		selectorRotationAtSelection = selector.transform.rotation;
//		distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude;
//	}
	
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
	}
	
	public void FixedUpdate()
	{
		print ("here1");
		this.UpdateTransform(true);
	}
	
	protected override void UpdateTransform(bool safePhysics)
	{
		if (!isSelected) return;
		
		Vector3 newManipulationPoint = getManipulationPoint();
		Quaternion newManipulationRotation = getManipulationRotation();
		
		Vector3 grabPointPosition = grabPoint.position;
		Vector3 hingePointPosition = hingePoint.position;
	
		Vector3 fromHingeToManipulationPoint = hingePointPosition - newManipulationPoint; 
		Vector3 fromHingeToSelectPoint = hingePointPosition - grabPointPosition;
	
		// Calculate projected point	
		
		Vector3 jointAxisInGlobalCoordinates = transform.TransformVector(transform.hingeJoint.axis);
		Vector3 axisCrossProduct = Vector3.Cross(jointAxisInGlobalCoordinates, fromHingeToSelectPoint);
		
		Vector3 normalVector = Vector3.Cross(axisCrossProduct, fromHingeToSelectPoint).normalized;
		Vector3 projectedPoint = MathUtil.ProjectPointOnPlane(normalVector, hingePointPosition, newManipulationPoint);
		
		Vector3 fromHingeToProjectedPoint = hingePointPosition - projectedPoint; 
		
		Vector3 directionCrossProduct = Vector3.Cross (fromHingeToSelectPoint, fromHingeToProjectedPoint);
		
		print (Vector3.Dot(jointAxisInGlobalCoordinates, directionCrossProduct));
		
		float angelDifference = Vector3.Angle(fromHingeToSelectPoint, fromHingeToProjectedPoint);
		Debug.DrawLine(hingePointPosition, projectedPoint, Color.blue);
		Debug.DrawLine(hingePointPosition, newManipulationPoint, Color.red);
		Debug.DrawLine(hingePointPosition, grabPointPosition, Color.green);
		
		Debug.DrawLine(hingePointPosition, directionCrossProduct + hingePointPosition, Color.cyan);
			
		DrawPlane(hingePointPosition, normalVector);	
			
	}
	
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
