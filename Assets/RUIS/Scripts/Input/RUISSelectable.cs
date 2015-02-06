/*****************************************************************************

Content    :   The functionality for selectable objects, just add this to an object along with a collider to make it selectable
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSelectable")]
public class RUISSelectable : MonoBehaviour {

	protected bool rigidbodyWasKinematic;
	protected RUISWandSelector selector;
	public bool isSelected { get { return selector != null; } }
	public bool physicalSelection = false;

	protected Vector3 positionAtSelection;
	protected Quaternion rotationAtSelection;
	protected Vector3 selectorPositionAtSelection;
	protected Quaternion selectorRotationAtSelection;
	protected float distanceFromSelectionRayOrigin;
    public float DistanceFromSelectionRayOrigin
    {
        get
        {
            return distanceFromSelectionRayOrigin;
        }
    }

    public bool clampToCertainDistance = false;
    public float distanceToClampTo = 1.0f;
    
    //for highlights
    public Material highlightMaterial;
    public Material selectionMaterial;

    public bool maintainMomentumAfterRelease = true;
	
	public bool continuousCollisionDetectionWhenSelected = true;
	protected CollisionDetectionMode oldCollisionMode;
	protected bool switchToOldCollisionMode = false;
	protected bool switchToContinuousCollisionMode = false;

	protected Vector3 latestVelocity = Vector3.zero;
	protected Vector3 lastPosition = Vector3.zero;

//    private List<Vector3> velocityBuffer;
	
	protected KalmanFilter positionKalman;
	protected double[] measuredPos = {0, 0, 0};
	protected double[] pos = {0, 0, 0};
	protected float positionNoiseCovariance = 1000;
	Vector3 filteredVelocity = Vector3.zero;

    protected bool transformHasBeenUpdated = false;

    public void Awake()
    {
//        velocityBuffer = new List<Vector3>();
		if(rigidbody)
			oldCollisionMode = rigidbody.collisionDetectionMode;
			
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3,3);
		positionKalman.skipIdenticalMeasurements = true;
    }

	// TODO: Ideally there would not be any calls to Update() and FixedUpdate(), so that CPU resources are spared
    public void Update()
    {
        if (transformHasBeenUpdated)
        {
            latestVelocity = (transform.position - lastPosition) 
								/ Mathf.Max(Time.deltaTime, Time.fixedDeltaTime);
            lastPosition = transform.position;

			if(isSelected)
			{
				updateVelocity(positionNoiseCovariance, Time.deltaTime);
			}
//            velocityBuffer.Add(latestVelocity);
//            LimitBufferSize(velocityBuffer, 2);

            transformHasBeenUpdated = false;
        }
    }

	protected void updateVelocity(float noiseCovariance, float deltaTime)
	{
		measuredPos[0] = latestVelocity.x;
		measuredPos[1] = latestVelocity.y;
		measuredPos[2] = latestVelocity.z;
		positionKalman.setR(deltaTime * noiseCovariance);
		positionKalman.predict();
		positionKalman.update(measuredPos);
		pos = positionKalman.getState();
		filteredVelocity.x = (float) pos[0];
		filteredVelocity.y = (float) pos[1];
		filteredVelocity.z = (float) pos[2];
	}

    public void FixedUpdate()
    {
		if(switchToContinuousCollisionMode)
		{
			oldCollisionMode = rigidbody.collisionDetectionMode;
			rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			switchToContinuousCollisionMode = false;
		}
		if(switchToOldCollisionMode)
		{
			rigidbody.collisionDetectionMode = oldCollisionMode;
			switchToOldCollisionMode = false;
		}
        UpdateTransform(true);
        transformHasBeenUpdated = true;
    }

    public virtual void OnSelection(RUISWandSelector selector)
    {
        this.selector = selector;

		// "Reset" filtered velocity by temporarily using position noise covariance of 10
		updateVelocity(10, Time.deltaTime);

        positionAtSelection = transform.position;
        rotationAtSelection = transform.rotation;
        selectorPositionAtSelection = selector.transform.position;
        selectorRotationAtSelection = selector.transform.rotation;
        distanceFromSelectionRayOrigin = (positionAtSelection - selector.selectionRay.origin).magnitude;

        lastPosition = transform.position;

        if (rigidbody)
        {
			if(continuousCollisionDetectionWhenSelected)
			{
				switchToContinuousCollisionMode = true;
			}
            rigidbodyWasKinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = true;
        }

        if (selectionMaterial != null)
            AddMaterialToEverything(selectionMaterial);

        UpdateTransform(false);
    }


    public virtual void OnSelectionEnd()
    {
        if (rigidbody)
        {
            rigidbody.isKinematic = rigidbodyWasKinematic;
			if(continuousCollisionDetectionWhenSelected)
			{
				switchToOldCollisionMode = true;
			}
        }

        if (maintainMomentumAfterRelease && rigidbody && !rigidbody.isKinematic)
        {
//            rigidbody.AddForce(AverageBufferContent(velocityBuffer), ForceMode.VelocityChange);

			rigidbody.AddForce(filteredVelocity, ForceMode.VelocityChange);
			if(selector) // Put this if-clause here just in case because once received NullReferenceException
			{
				if(selector.transform.parent)
				{
					rigidbody.AddTorque(selector.transform.parent.TransformDirection(
										Mathf.Deg2Rad * selector.angularVelocity), ForceMode.VelocityChange);
				}
	            else
					rigidbody.AddTorque(Mathf.Deg2Rad * selector.angularVelocity, ForceMode.VelocityChange);
			}
        }

        if(selectionMaterial != null)
            RemoveMaterialFromEverything();

        this.selector = null;
    }

    public virtual void OnSelectionHighlight()
    {
        if(highlightMaterial != null)
            AddMaterialToEverything(highlightMaterial);
    }

    public virtual void OnSelectionHighlightEnd()
    {
        if(highlightMaterial != null)
            RemoveMaterialFromEverything();
    }

    protected virtual void UpdateTransform(bool safePhysics)
    {	
        if (!isSelected) return;

		Vector3 newManipulationPoint = getManipulationPoint();
		Quaternion newManipulationRotation = getManipulationRotation();

        if (rigidbody && safePhysics)
        {
            rigidbody.MovePosition(newManipulationPoint);
            rigidbody.MoveRotation(newManipulationRotation);
        }
        else
        {
            transform.position = newManipulationPoint;
            transform.rotation = newManipulationRotation;
        }
    }


	protected void LimitBufferSize(List<Vector3> buffer, int maxSize)
    {
        while (buffer.Count >= maxSize)
        {
            buffer.RemoveAt(0);
        }
    }
    
    public Vector3 getManipulationPoint()
    {
		switch (selector.positionSelectionGrabType)
		{
		case RUISWandSelector.SelectionGrabType.SnapToWand:
			return selector.transform.position;
			break;
		case RUISWandSelector.SelectionGrabType.RelativeToWand:
			Vector3 selectorPositionChange = selector.transform.position - selectorPositionAtSelection;
			return positionAtSelection + selectorPositionChange;
			break;
		case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
			float clampDistance = distanceFromSelectionRayOrigin;
			if (clampToCertainDistance) clampDistance = distanceToClampTo;
			return selector.selectionRay.origin + clampDistance * selector.selectionRay.direction;
			break;
		case RUISWandSelector.SelectionGrabType.DoNotGrab:
			return transform.position;
			break;
		}
		return transform.position;
    }
    
	
	public Quaternion getManipulationRotation()
	{
		switch (selector.rotationSelectionGrabType)
		{
		case RUISWandSelector.SelectionGrabType.SnapToWand:
			return selector.transform.rotation;
			break;
		case RUISWandSelector.SelectionGrabType.RelativeToWand:
			return rotationAtSelection;
			// Tuukka: 
			Quaternion selectorRotationChange = Quaternion.Inverse(selectorRotationAtSelection) * rotationAtSelection;
			return selector.transform.rotation * selectorRotationChange;
			break;
		case RUISWandSelector.SelectionGrabType.AlongSelectionRay:
			return Quaternion.LookRotation(selector.selectionRay.direction);
			break;
		case RUISWandSelector.SelectionGrabType.DoNotGrab:
			return transform.rotation;
			break;
		}
		return transform.rotation;
	}

//    private Vector3 AverageBufferContent(List<Vector3> buffer)
//    {
//        if (buffer.Count == 0) return Vector3.zero;
//
//        Vector3 averagedContent = new Vector3();
//        foreach (Vector3 v in buffer)
//        {
//            averagedContent += v;
//        }
//
//        averagedContent = averagedContent / buffer.Count;
//
//        return averagedContent;
//    }

	protected void AddMaterial(Material m, Renderer r)
    {
        if (	m == null || r == null || r.GetType() == typeof(ParticleRenderer) 
			||  r.GetType() == typeof(ParticleSystemRenderer))
			return;

        Material[] newMaterials = new Material[r.materials.Length + 1];
        for (int i = 0; i < r.materials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }

        newMaterials[newMaterials.Length - 1] = m;
        r.materials = newMaterials;
    }

	protected void RemoveMaterial(Renderer r)
    {
        if (	r == null || r.GetType() == typeof(ParticleRenderer) 
			||  r.GetType() == typeof(ParticleSystemRenderer) || r.materials.Length == 0)
			return;

        Material[] newMaterials = new Material[r.materials.Length - 1];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = r.materials[i];
        }
        r.materials = newMaterials;
    }

	protected void AddMaterialToEverything(Material m)
    {
        AddMaterial(m, renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            AddMaterial(m, childRenderer);
        }
    }

	protected void RemoveMaterialFromEverything()
    {
        RemoveMaterial(renderer);

        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
        {
            RemoveMaterial(childRenderer);
        }
    }
}
