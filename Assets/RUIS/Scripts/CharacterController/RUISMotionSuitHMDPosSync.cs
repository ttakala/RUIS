/*****************************************************************************

The original author of this file is kkostenkov (Kirill).
Minor modifications have been made by Tuukka Takala.

******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RUISMotionSuitHMDPosSync : MonoBehaviour {
	public Transform HMDCameraEye;
	public Transform motionSuitHead;
	public Transform motionSuitPelvis;

	[Tooltip("Translation offset that can be used to adjust the head position with relation to the eye cameras.")]
	public Vector3 eyeOffset = Vector3.zero;

	[Tooltip("Translation offset that can be used to adjust the pelvis position with relation to the floor for example.")]
	public Vector3 pelvisOffset = Vector3.zero;

	void LateUpdate () {
		Vector3 headPelvisOffset = motionSuitHead.position - motionSuitPelvis.position + motionSuitPelvis.rotation * pelvisOffset;
		motionSuitPelvis.position = HMDCameraEye.position - headPelvisOffset + HMDCameraEye.rotation * eyeOffset;

	}
}