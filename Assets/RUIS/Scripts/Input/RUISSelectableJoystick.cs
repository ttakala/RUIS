/*****************************************************************************

Content    :   Implements selection behavior for RUISWands
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUISSelectableJoystick : RUISSelectable {
	
	private bool isSelected;
	public float springForce = 10;
	private ConfigurableJoint configurableJoint;
	private Vector3 jointAxisInGlobalCoordinates;
	private Quaternion initialRotation; 
	private Quaternion rotationOnSelectionStart;
	private Vector3 originalHingeForward;
	
	private JointDrive originalJointDriveX, originalJointDriveYZ;
	
	void Start() 
	{
		
		this.configurableJoint = this.gameObject.GetComponent(typeof(ConfigurableJoint)) as ConfigurableJoint;
		this.jointAxisInGlobalCoordinates = transform.TransformDirection(Vector3.Cross(this.configurableJoint.axis, this.configurableJoint.secondaryAxis));
		this.initialRotation = this.configurableJoint.transform.localRotation;
		
		Vector3 objectCenterProjectedOnPlane = MathUtil.ProjectPointOnPlane(jointAxisInGlobalCoordinates, this.configurableJoint.connectedAnchor, transform.position);
		
		
		this.originalHingeForward = hingeJoint.connectedAnchor - objectCenterProjectedOnPlane;
		if(this.originalHingeForward == Vector3.zero) {
			if(jointAxisInGlobalCoordinates.normalized == transform.forward) {
				this.originalHingeForward = transform.right;
			}
			else {
				this.originalHingeForward =  transform.forward;
			}
		}
		
		
	}
	
	public override void OnSelection(RUISWandSelector selector)
	{
		this.selector = selector;
		this.isSelected = true;
		// Transform information
		positionAtSelection = selector.selectionRayEnd;
		rotationAtSelection = transform.rotation;
		// Selector information
		selectorPositionAtSelection = selector.transform.position;
		selectorRotationAtSelection = selector.transform.rotation;
		distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude; // Dont remove this, needed in the inherited class
		
		
		this.rotationOnSelectionStart = this.configurableJoint.transform.localRotation;
		
		this.originalJointDriveX = this.configurableJoint.angularXDrive;
		this.originalJointDriveYZ = this.configurableJoint.angularYZDrive;
		
		JointDrive jd = new JointDrive();
		jd.mode = JointDriveMode.Position;
		jd.positionSpring = springForce;
		jd.maximumForce = 999;
		
		this.configurableJoint.angularXDrive = jd;
		this.configurableJoint.angularYZDrive = jd;
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
		
		this.configurableJoint.angularXDrive = this.originalJointDriveX;
		this.configurableJoint.angularYZDrive = this.originalJointDriveYZ;
	}
	
	public void FixedUpdate()
	{
		this.UpdateTransform(true);
	}
	
	
	protected override void UpdateTransform(bool safePhysics)
	{
		Vector3 newManipulationPoint = getManipulationPoint();
		Vector3 projectedPoint = MathUtil.ProjectPointOnPlane(this.jointAxisInGlobalCoordinates, this.configurableJoint.connectedAnchor, newManipulationPoint);
		Vector3 fromHingeToProjectedPoint = this.configurableJoint.connectedAnchor - projectedPoint;
		
		// https://gist.github.com/mstevenson/4958837
		Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, (newManipulationPoint - this.configurableJoint.connectedAnchor).normalized);
		this.configurableJoint.SetTargetRotationLocal (targetRotation * Quaternion.LookRotation(Vector3.down), this.initialRotation);
		
		Debug.DrawLine(this.configurableJoint.connectedAnchor, projectedPoint, Color.blue);
		Debug.DrawLine(this.configurableJoint.connectedAnchor, newManipulationPoint, Color.red);
		DrawPlane(this.configurableJoint.connectedAnchor, jointAxisInGlobalCoordinates);
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
