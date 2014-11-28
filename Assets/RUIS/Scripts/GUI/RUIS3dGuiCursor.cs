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
	private bool wasVisible = false;
	Quaternion wallOrientation = Quaternion.identity;
	private Vector4 translateColumn = Vector4.zero;
	private Vector3 trackerPosition = Vector3.zero;

	void Start() 
	{
		menuScript = this.GetComponent<RUISMenuNGUI>();
		this.guiPlane = this.transform.Find ("planeCollider").GetComponent<Collider>();
		if(this.guiPlane == null)
			Debug.LogError( "Did not find RUISMenu collider object, onto which mouse selection ray is projected!" );

		ruisCamera = this.transform.parent.parent.GetComponent<RUISCamera>();
		if(this.transform.parent == null)
		{
			Debug.LogError(  "The parent of GameObject '" + name 
			               + "is null and RUIS Menu will not function. Something is wrong with 'RUIS NGUI Menu' prefab or you "
			               + "are misusing the " + typeof(RUIS3dGuiCursor) + " script.");
		}
		else if(this.transform.parent.parent == null)
			Debug.LogError(  "The grand parent of GameObject '" + name 
			               + "is null and RUIS Menu will not function. Something is wrong with 'RUIS NGUI Menu' prefab or you "
			               + "are misusing the " + typeof(RUIS3dGuiCursor) + " script.");

		ruisDisplayManager =  FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

		if(ruisDisplayManager == null) 
		{ 
			this.enabled = false;
			Debug.LogError("Could not find " + typeof(RUISDisplayManager) + " script, RUIS menu will not work!");
			return;
		}
		if(ruisDisplayManager.hideMouseOnPlay && menuScript.currentMenuState != RUISMenuNGUI.RUISMenuStates.calibration) 
			Screen.showCursor = false;
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

		if(!menuScript.menuIsVisible)
			return;

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
				mouseInputCoordinates = 2.5f * Input.mousePosition;
//				mouseInputCoordinates.x = Input.mousePosition.x;
//				mouseInputCoordinates.y = Input.mousePosition.y;
//				mouseInputCoordinates.z = Input.mousePosition.z;
			}
			else 
			{
				mouseInputCoordinates = Input.mousePosition; 	
			}	
			
			Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(mouseInputCoordinates);
			
			instancedCursor.transform.rotation = ruisCamera.transform.rotation;

			if(ruisCamera.associatedDisplay.isObliqueFrustum)
			{
				wallOrientation = Quaternion.LookRotation(-ruisCamera.associatedDisplay.DisplayNormal, ruisCamera.associatedDisplay.DisplayUp);
				
				instancedCursor.transform.rotation = instancedCursor.transform.rotation * wallOrientation;

				//ray.origin = ruisCamera.associatedDisplay.displayCenterPosition;
				//translateColumn = ruisCamera.centerCamera.projectionMatrix.GetColumn(3);
				trackerPosition.Set(translateColumn.x, translateColumn.y, translateColumn.z);
				trackerPosition = ruisCamera.transform.position + ruisCamera.transform.rotation * ruisCamera.KeystoningHeadTrackerPosition;
				ray.origin += trackerPosition;
				ray.direction = wallOrientation * ray.direction;

			}

			if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(ruisDisplayManager.menuLayerName)))
			{ 
				if(instancedCursor)
				{
					instancedCursor.transform.position = hit.point;

					if(!wasVisible)
						instancedCursor.SetActive(true);
					wasVisible = true;
				}
				Debug.DrawLine(ray.origin, hit.point);
				break;
			}
			else
			{
				if(wasVisible)
					instancedCursor.SetActive(false);
				wasVisible = false;
			}
		}
	}
}
