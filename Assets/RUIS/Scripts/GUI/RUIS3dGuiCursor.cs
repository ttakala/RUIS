using UnityEngine;
using System.Collections;

public class RUIS3dGuiCursor : MonoBehaviour {
	
	private Collider guiPlane;
	private GameObject markerObject;
	//private Camera guiCamera;
	private UICamera[] cameras;
	private RUISCamera ruisCamera;
	private RUISMenuNGUI menuScript;
	private RUISDisplayManager ruisDisplayManager;
	private GameObject instancedCursor;
	
	private Vector3 mouseInputCoordinates;
	
	void Start() 
	{
		menuScript = this.GetComponent<RUISMenuNGUI>();
		this.guiPlane = this.transform.Find ("planeCollider").GetComponent<Collider>();
		ruisCamera = this.transform.parent.parent.GetComponent<RUISCamera>();
		ruisDisplayManager =  FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
		if(ruisDisplayManager.hideMouseOnPlay && menuScript.currentMenuState != RUISMenuNGUI.RUISMenuStates.calibration) Screen.showCursor = false;
		markerObject = ruisDisplayManager.menuCursorPrefab;
	}
	
	void Update() 
	{
		// If we are in calibration scene, disable 3d cursor
		if(this.transform.parent == null) 
		{ 
			this.enabled = false;
			return;
		}
		cameras = this.transform.parent.parent.GetComponentsInChildren<UICamera>();
		
		if(menuScript.menuIsVisible && !instancedCursor) 
		{
			instancedCursor = Instantiate(this.markerObject) as GameObject;
		}
		if(!menuScript.menuIsVisible && instancedCursor) 
		{
			Destroy (instancedCursor);
		}
		
		RaycastHit hit;	
		
		foreach(UICamera camera in cameras) 
		{
			/*
			if(!ruisCamera.associatedDisplay.isStereo
			   &&	(camera.gameObject.name == "CameraLeft"
			   ||	camera.gameObject.name == "CameraRight" 
			   || camera.gameObject.name == "guiCameraForRift"
			   ))  
			{
				camera.enabled = false;
				continue;
			}
			
			if(ruisCamera.associatedDisplay.isStereo 
				&& !ruisCamera.associatedDisplay.enableOculusRift  
				&& !(camera.gameObject.name == "CameraLeft"
				||	camera.gameObject.name == "CameraRight" 
				)) 
				{
					camera.enabled = false;
					continue;
				}
			if(ruisCamera.associatedDisplay.enableOculusRift 
				&& camera.gameObject.name != "guiCameraForRift")
				{
					camera.enabled = false;
					continue;
				} 
			*/
				
			if(ruisCamera.associatedDisplay.enableOculusRift) 
			{
				mouseInputCoordinates = Input.mousePosition * 2.5f;
			}
			else 
			{
				mouseInputCoordinates = Input.mousePosition; 	
			}	
			
			Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(mouseInputCoordinates);
			
			if(ruisCamera.associatedDisplay.isObliqueFrustum) {
				ray.origin = ruisCamera.associatedDisplay.displayCenterPosition;
			}
			
			if (guiPlane.Raycast(ray, out hit, 1000.0F))
			{ 
				
				instancedCursor.transform.position = hit.point;
				Debug.DrawLine(ray.origin, hit.point);
				break;
			}		
		}
	}
}
