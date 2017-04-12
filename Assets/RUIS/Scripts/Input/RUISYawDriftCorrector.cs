/*****************************************************************************

Content    :   Applies a yaw drift correction to the GameObject Transform
Authors    :   Tuukka Takala
Copyright  :   Copyright 2017 Tuukka Takala. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RUISYawDriftCorrector : MonoBehaviour {

	[Tooltip(  "Leave this field as none if the drifting rotation is stored in the driftingChild's world rotation variable. "
			 + "If the drifting rotation is stored in the driftingChild's localRotation variable, then set this field as the "
			 + "immediate parent of the driftingChild. If driftingChild is a (grand) child of correctionTarget, then "
			 + "set this field as one of the (grand) parents of driftingChild, and the drifting rotation will be calculated as "
			 + "the rotation difference between driftingParent and driftingChild, preventing a drift correction feedback loop.")]
	public Transform driftingParent;

	[Tooltip(  "Transform that contains the drifting rotation. The parent coordinate system of this drifting rotation is "
			 + "defined with driftingParent.")]
	public Transform driftingChild;

	[Tooltip(  "The \"compass\" rotation that has no yaw drift, but otherwise is roughly a (possibly offset) version of "
			 + "driftingChild's rotation.")]
	public Transform driftlessTransform;

	[Tooltip(  "Use this check box to indicate whether the driftless rotation in driftlessTransform is stored in localRotation "
			 + "or world rotation variable.")]
	public bool driftlessIsLocalRotation = true;

	[Tooltip(  "Rotation offset in Euler angles (degrees) between the drifting rotation and the driftlessTransform's rotation. "
			 + "These angles should be non-zero only if there is a fixed offset between the two rotations.")]
	
	public Vector3 driftlessToDriftingOffset = Vector3.zero;
	[Tooltip(  "The Transform where the drift correction should be applied. Commonly this is a (grand) parent of the driftingChild, "
			 + "in which case you must also define the driftingParent, otherwise there will be a drift correction feedback loop.")]
	public Transform correctionTarget;

	[Tooltip("The rate at which drift correction is applied (degrees per second).")]
	public float driftCorrectionVelocity = 2;

	[Tooltip(  "The button that can be used to immediately apply the drift correction, if the driftingChild and the driftlessTransform "
			 + "were misaligned upon running the Awake() of this component (e.g. during loading a scene).")]
	public KeyCode resetCorrectionButton = KeyCode.Space;

	KalmanFilter filterDrift;
	double[] measuredDrift = {0, 0};
	double[] filteredDrift = {0, 0};
	float driftNoiseCovariance = 5000;

	[HideInInspector]
	public Quaternion filteredRotation = Quaternion.identity;
	Quaternion driftingRotation;
	Vector3 yawDifferenceDirection = Vector3.zero;
	Vector3 driftingForward;
	Vector3 driftlessForward;
	Vector3 driftVector;
	float initCorrectionVelocity = 36000;
	float currentCorrectionVelocity;

	// Use this for initialization
	void Awake () {
		filterDrift = new KalmanFilter();
		filterDrift.initialize(2,2);

		currentCorrectionVelocity = driftCorrectionVelocity;

		if (!driftingChild) {
			Debug.LogError (  "Field " + driftingChild.name + " must be defined! It should contain the rotation that has yaw drift. Disabling " 
							+ typeof(RUISYawDriftCorrector));
			this.enabled = false;
			return;
		}
		if (!driftingChild) {
			Debug.LogError (  "Field " + driftlessTransform.name + " must be defined! It should contain the \"compass\" rotation that has no yaw drift. "
							+ "Disabling " + typeof(RUISYawDriftCorrector));
			this.enabled = false;
			return;
		}

		StartCoroutine("CorrectImmediately");
	}
	
	// Update is called once per frame
	void LateUpdate () {
		// drifting rotation transform.rotation = Quaternion.Inverse (parent.rotation) * child.rotation;

		if(Input.GetKeyDown(resetCorrectionButton))
			StartCoroutine("CorrectImmediately");

		if (driftingParent)
			driftingRotation = Quaternion.Inverse (driftingParent.rotation) * driftingChild.rotation;
		else
			driftingRotation = driftingChild.rotation;

		driftingForward  = driftingRotation * Quaternion.Euler(driftlessToDriftingOffset) * Vector3.forward;
		driftlessForward = (driftlessIsLocalRotation ? driftlessTransform.localRotation : driftlessTransform.rotation) * Vector3.forward;

		#if UNITY_EDITOR
		Debug.DrawRay(driftingChild.position, driftingForward);
		Debug.DrawRay(driftingChild.position, 0.5f * (driftingRotation * Quaternion.Euler(driftlessToDriftingOffset) * Vector3.up));
		#endif

		// HACK: Project forward vectors to XZ-plane. This is a problem if they are constantly parallel to Y-axis, e.g. HMD user looking directly up or down 
		driftingForward.Set (driftingForward.x, 0, driftingForward.z);
		driftlessForward.Set (driftlessForward.x, 0, driftlessForward.z);

		// HACK: If either forward vector is constantly parallel to Y-axis, no drift correction occurs. Occasionally this is OK, as the drift correction occurs gradually.
		if (driftingForward.magnitude > 0.01f && driftlessForward.magnitude > 0.01f) {

			// HACK: Vector projection to XZ-plane ensures that the change in the below driftVector is continuous, 
			//		 as long as rotation change in driftingRotation and driftlessTransform is continuous. Otherwise 
			//		 more math is needed to ensure the continuity...
			driftVector = Quaternion.Euler(0, -Vector3.Angle (driftingForward, driftlessForward), 0) * Vector3.forward;

			// 2D vector rotated by yaw difference has continuous components
			measuredDrift [0] = driftVector.x;
			measuredDrift [1] = driftVector.z;

			// Simple Kalman filtering
			filterDrift.setR(Time.deltaTime * driftNoiseCovariance);
			filterDrift.predict();
			filterDrift.update (measuredDrift);
			filteredDrift = filterDrift.getState();

			filteredRotation = Quaternion.RotateTowards (filteredRotation, 
														 Quaternion.LookRotation (new Vector3 ((float)filteredDrift [0], 0, (float)filteredDrift [1])), 
														 currentCorrectionVelocity * Time.deltaTime);

			if(correctionTarget)
				correctionTarget.localRotation = filteredRotation;
		}
	}

	IEnumerator CorrectImmediately()
	{
		currentCorrectionVelocity = initCorrectionVelocity;
		yield return new WaitForSeconds(0.2f);
		currentCorrectionVelocity = driftCorrectionVelocity;
	}
}
