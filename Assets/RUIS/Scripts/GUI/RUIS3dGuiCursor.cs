/*****************************************************************************

Content    :   A manager for display configurations
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2015 Tuukka Takala, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

public class RUIS3dGuiCursor : MonoBehaviour {
	
	private Collider guiPlane;
	private GameObject markerObject;
	private UICamera uiCamera;
//	private UICamera[] cameras;
	private RUISCamera ruisCamera;
	private RUISMenuNGUI menuScript;
	private RUISDisplayManager ruisDisplayManager;
	private GameObject instancedCursor;
	
	private Vector3 mouseInputCoordinates;
	private bool wasVisible = false;
	Quaternion wallOrientation = Quaternion.identity;
	private Vector3 trackerPosition = Vector3.zero;

	private Vector3 originalLocalScale = Vector3.one;

	void Start() 
	{
		menuScript = this.GetComponent<RUISMenuNGUI>();
		if(menuScript == null)
			Debug.LogError( "Did not find " + typeof(RUISMenuNGUI) + " script!");

		this.guiPlane = this.transform.Find ("NGUIControls/planeCollider").GetComponent<Collider>();
		if(this.guiPlane == null)
			Debug.LogError( "Did not find RUIS Menu collider object, onto which mouse selection ray is projected!" );

		if(menuScript.transform.parent == null)
			Debug.LogError(  "The parent of GameObject '" + menuScript.name 
			               + " is null and RUIS Menu will not function. Something is wrong with 'RUIS NGUI Menu' prefab or you "
			               + "are misusing the " + typeof(RUIS3dGuiCursor) + " script.");
//		else if(menuScript.transform.parent.parent == null)
//			Debug.LogError(  "The grand-parent of GameObject '" + menuScript.name 
//			               + " is null and RUIS Menu will not function. Something is wrong with 'RUIS NGUI Menu' prefab or you "
//			               + "are misusing the " + typeof(RUIS3dGuiCursor) + " script.");
//		else
//			ruisCamera = menuScript.transform.parent.parent.GetComponent<RUISCamera>();
//
//		if(ruisCamera == null)
//			Debug.LogError(  typeof(RUIS3dGuiCursor) + " script did not find "  + typeof(RUISCamera) + " from the parent of "
//			               + menuScript.transform.name + "gameobject! RUIS Menu is unavailable.");
			               
		ruisDisplayManager =  FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

		if(ruisDisplayManager == null) 
		{ 
			this.enabled = false;
			Debug.LogError("Could not find " + typeof(RUISDisplayManager) + " script, RUIS Menu will not work!");
			return;
		}

		if(ruisDisplayManager.displays[ruisDisplayManager.guiDisplayChoice] && ruisDisplayManager.displays[ruisDisplayManager.guiDisplayChoice].linkedCamera)
			ruisCamera = ruisDisplayManager.displays[ruisDisplayManager.guiDisplayChoice].linkedCamera;
		else
			Debug.LogError("Could not find the " + typeof(RUISCamera) + " that is linked to the " + typeof(RUISDisplay) +  " with RUIS Menu!");

		if(ruisDisplayManager.hideMouseOnPlay && menuScript.currentMenuState != RUISMenuNGUI.RUISMenuStates.calibration) 
			Cursor.visible = false;
		markerObject = ruisDisplayManager.menuCursorPrefab;

		if(markerObject)
			originalLocalScale = this.markerObject.transform.localScale;
	}
	
	void LateUpdate() 
	{
		// If we are in calibration scene, disable 3d cursor
		if(this.transform.parent == null) 
		{ 
			this.enabled = false;
			return;
		}

		// TODO: instead of searching hierarchy on every frame, find the UICameras more efficiently
		uiCamera = ruisCamera.transform.GetComponentInChildren<UICamera>(); //menuScript.transform.parent.parent.GetComponentsInChildren<UICamera>();
		
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

		mouseInputCoordinates = Input.mousePosition;
		if(ruisCamera.centerCamera) 
		{
			if(instancedCursor)
				instancedCursor.transform.rotation = ruisCamera.centerCamera.transform.rotation;
		}
		else 
		{
			if(instancedCursor)
				instancedCursor.transform.rotation = ruisCamera.transform.rotation;
		}

		// HACK for MecanimBlendedCharacter: Keep cursor visible size even if character is scaled
		if(menuScript.transform.parent)
			instancedCursor.transform.localScale = originalLocalScale * Mathf.Max (menuScript.transform.parent.lossyScale.x, menuScript.transform.parent.lossyScale.y);

		RaycastHit hit;	

		if(uiCamera) 
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

			Ray ray;
			Camera rayCamera = uiCamera.GetComponent<Camera>();

			if(rayCamera)
			{
				if(UnityEngine.XR.XRSettings.enabled && rayCamera.stereoTargetEye != StereoTargetEyeMask.None) // if(ruisCamera.associatedDisplay != null && ruisCamera.associatedDisplay.isHmdDisplay)
				{
					// *** TODO remove this hack when Camera.ScreenPointToRay() works again
					ray = RUISDisplayManager.HMDScreenPointToRay(mouseInputCoordinates, rayCamera);
				} else
					ray = rayCamera.ScreenPointToRay(mouseInputCoordinates);
			} else
				ray = new Ray();

			if(ruisCamera.associatedDisplay != null && ruisCamera.associatedDisplay.isObliqueFrustum)
			{
				Quaternion outerRot = ruisCamera.transform.rotation;
				wallOrientation = Quaternion.LookRotation(-ruisCamera.associatedDisplay.DisplayNormal, ruisCamera.associatedDisplay.DisplayUp);

				// *** HACK why is this sign flip necessary to keep the menu at right place??
				outerRot = new Quaternion(outerRot.x, outerRot.y, -outerRot.z, outerRot.w);

				instancedCursor.transform.rotation = outerRot * wallOrientation;

				trackerPosition = outerRot * ruisCamera.KeystoningHeadTrackerPosition;
				ray.origin += trackerPosition - outerRot * (new Vector3(ruisDisplayManager.guiX, 
																		ruisDisplayManager.guiY, 
																		ruisDisplayManager.guiZ ));
//				ray.direction =  wallOrientation * ray.direction;
			}

			if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(LayerMask.LayerToName(ruisDisplayManager.menuLayer))))
			{ 
				if(instancedCursor)
				{
					instancedCursor.transform.position = hit.point;

					if(!wasVisible)
						instancedCursor.SetActive(true);
					wasVisible = true;
				}
				#if UNITY_EDITOR
				Debug.DrawLine(ray.origin, hit.point);
				#endif
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
