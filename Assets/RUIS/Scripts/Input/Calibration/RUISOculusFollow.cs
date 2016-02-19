using UnityEngine;
using System.Collections;

public class RUISOculusFollow : MonoBehaviour 
{
	RUISCoordinateSystem coordinateSystem;
	
	void Start() 
	{
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
	}
	
	void Update () 
	{
//				if(RUISOVRManager.ovrHmd != null) //06to08

		if(UnityEngine.VR.VRDevice.isPresent)
		{
			Vector3 tempSample = Vector3.zero;
			
//			Ovr.Posef headpose = RUISOVRManager.ovrHmd.GetTrackingState().HeadPose.ThePose;
//			float px =  headpose.Position.x;
//			float py =  headpose.Position.y;
//			float pz = -headpose.Position.z; // This needs to be negated TODO: might change with future OVR version
//			
//			tempSample = new Vector3(px, py, pz);
//			
//			tempSample = coordinateSystem.ConvertRawOculusDK2Location(tempSample);

			// HACK TODO if this doesn't work for major HMDs, add wrapper (also for rotation)
			// HACK TODO tempSample components might need negation or other hackery
			tempSample = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head); //06to08

			Vector3 convertedLocation = coordinateSystem.ConvertLocation(tempSample, RUISDevice.Oculus_DK2); 
			this.transform.localPosition = convertedLocation;
			this.transform.localRotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head); // HACK TODO check that this works

//			if(OVRManager.capiHmd != null)
//			{
//				try
//				{
//					this.transform.localRotation = OVRManager.capiHmd.GetTrackingState().HeadPose.ThePose.Orientation.ToQuaternion();
//				}
//				catch(System.Exception e)
//				{
//					Debug.LogError(e.Message);
//				}
//			}
		}
	}
}
