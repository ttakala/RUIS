using UnityEngine;
using System.Collections;

public class CameraTiltTextUpdater : MonoBehaviour {
    PSMoveWrapper psMoveWrapper;

	void Awake () {
        psMoveWrapper = FindObjectOfType(typeof(PSMoveWrapper)) as PSMoveWrapper;
        psMoveWrapper.CameraFrameResume();
	}
	
	void Update () {
        if (psMoveWrapper.state != null)
        {
            guiText.text = string.Format("PSMove camera pitch angle: {0}", Mathf.Rad2Deg * psMoveWrapper.state.gemStates[0].camera_pitch_angle);
        }
	}
}