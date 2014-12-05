using UnityEngine;
using System.Collections;

public class RUISOVRManagerDestroyer : MonoBehaviour 
{
	void Awake () 
	{
		int count = 0;
		RUISCamera[] cameras = FindObjectsOfType<RUISCamera>();
		RUISDisplay[] displays = FindObjectsOfType(typeof(RUISDisplay)) as RUISDisplay[];
		
		
		foreach(RUISDisplay display in displays)
		{
			if(display.linkedCamera != null && !display.enableOculusRift)
			{
				display.linkedCamera.GetComponent<OVRManager>().enabled = false;
				display.linkedCamera.GetComponent<OVRCameraRig>().enabled = false;
			}
		}
		
		foreach(RUISCamera camera in cameras)
		{
			bool foundLink = false;
			foreach(RUISDisplay display in displays)
			{
				if(camera == display.linkedCamera)  
					foundLink = true;
					
			}
			if(!foundLink)
			{
				camera.GetComponent<OVRManager>().enabled = false;
				camera.GetComponent<OVRCameraRig>().enabled = false;
			}
		}
	}
}
