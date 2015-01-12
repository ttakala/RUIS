using UnityEngine;
using System.Collections;
using Ovr;

public class RUISOculusFollow : MonoBehaviour 
{
	RUISCoordinateSystem coordinateSystem;
	
	void Start() 
	{
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
	}
	
	void Update () 
	{
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
			if(OVRManager.capiHmd != null)
			{
				TrackingState state = OVRManager.capiHmd.GetTrackingState();
				try
				{
					this.transform.localRotation = OVRManager.capiHmd.GetTrackingState().HeadPose.ThePose.Orientation.ToQuaternion();
				}
				catch(System.Exception e)
				{
					Debug.LogError(e.Message);
				}
			}
			// Scene doesn't have a real OVRManager (only some DLLs are loaded), that's why below doesn't work
//			this.transform.localRotation = coordinateSystem.GetOculusRiftOrientation();
		}
	}
}
