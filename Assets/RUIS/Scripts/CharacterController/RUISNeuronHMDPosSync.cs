/*****************************************************************************

The original author of this file is kkostenkov (Kirill).
Minor modifications have been made by Tuukka Takala.

******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RUISNeuronHMDPosSync : MonoBehaviour {
	public Transform viveCamEye;
	public Transform neuronHead;
	public Transform neuronPelvis;

	[Tooltip("Translation offset that can be used to adjust the head position with relation to the eye cameras.")]
	public Vector3 eyeOffset = Vector3.zero;

	void LateUpdate () {
		Vector3 headPelvisOffset = neuronHead.position - neuronPelvis.position;
		neuronPelvis.position = viveCamEye.position - headPelvisOffset + viveCamEye.rotation * eyeOffset;

	}
}