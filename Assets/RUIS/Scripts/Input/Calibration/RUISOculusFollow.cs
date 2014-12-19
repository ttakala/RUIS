using UnityEngine;
using System.Collections;

public class RUISOculusFollow : MonoBehaviour {
	RUISCoordinateSystem coordinateSystem;
	
	void Start() {
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
	}
	
	void Update () {
		if(RUISOVRManager.ovrHmd != null)
		{
			Ovr.Posef headpose = RUISOVRManager.ovrHmd.GetTrackingState().HeadPose.ThePose;
			float px = headpose.Position.x;
			float py = headpose.Position.y;
			float pz = headpose.Position.z;
			
			Vector3 tempSample = new Vector3(px, py, pz);
			tempSample = coordinateSystem.ConvertRawOculusDK2Location(tempSample);
			Vector3 convertedLocation = coordinateSystem.ConvertLocation(tempSample, RUISDevice.Oculus_DK2); 
			
			this.transform.localPosition = convertedLocation;
			
			Quaternion convertedRotation = coordinateSystem.ConvertRotation(Quaternion.identity, RUISDevice.Oculus_DK2);
			// TODO: Test below transform.localRotation
			transform.localRotation = Quaternion.Euler(new Vector3(0, convertedRotation.eulerAngles.y, 0));
		}
	}
}
