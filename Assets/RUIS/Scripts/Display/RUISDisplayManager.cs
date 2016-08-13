/*****************************************************************************

Content    :   A manager for display configurations
Authors    :   Mikael Matveinen, Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2016 Tuukka Takala, Mikael Matveinen, Heikki Heiskanen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

//using Ovr;

public class RUISDisplayManager : MonoBehaviour
{
	public List<RUISDisplay> displays;
	public GameObject stereoCamera;
	public Camera monoCamera;
	public int totalResolutionX = 0;
	public int totalResolutionY = 0;
	public int totalRawResolutionX = 0;
	public int totalRawResolutionY = 0;

	//    public bool allowResolutionDialog;

	public GameObject ruisMenuPrefab;
	public GameObject menuCursorPrefab;
	public int menuLayer = 0;
	public int guiDisplayChoice = 0;
	
	public float guiX;
	public float guiY;
	public float guiZ;
	
	public float guiScaleX = 1;
	public float guiScaleY = 1;
	public bool hideMouseOnPlay = false;

	private bool hasHeadMountedDisplay = false;

	public class ScreenPoint
	{
		public Vector2 coordinates;
		public Camera camera;
	}

	void Start()
	{

		CalculateTotalResolution();

		if(Application.isEditor)
		{
			UpdateResolutionsOnTheFly();
		}
		
		hasHeadMountedDisplay = HasHeadMountedDisplay();


		UpdateDisplays();

		DisableUnlinkedCameras();


		LoadDisplaysFromXML();

		// Second substitution because displays might have been updated via XML etc.
		hasHeadMountedDisplay = HasHeadMountedDisplay();

		InitRUISMenu(ruisMenuPrefab, guiDisplayChoice);
		
		
	}
	
	void Update()
	{
		if(Application.isEditor && (Screen.width != totalRawResolutionX || Screen.height != totalRawResolutionY))
		{
			UpdateResolutionsOnTheFly();
			UpdateDisplays();
		}
	}

	public void UpdateDisplays()
	{
		CalculateTotalResolution();

		int currentResolutionX = 0;
		foreach(RUISDisplay display in displays)
		{
			display.SetupViewports(currentResolutionX, new Vector2(totalRawResolutionX, totalRawResolutionY));
			currentResolutionX += display.rawResolutionX;
		}

		if(displays.Count > 1 || (displays.Count == 1 /* && !allowResolutionDialog */))
		{
			if(!hasHeadMountedDisplay)
			{
				Screen.SetResolution(totalRawResolutionX, totalRawResolutionY, false);
			}
		}
	}

	public bool HasHeadMountedDisplay()
	{
		foreach(RUISDisplay display in displays)
		{
			if(display.linkedCamera && display.isHmdDisplay)
			{
				return true;
			}
		}
		return false;
	}

	public void CalculateTotalResolution()
	{
		totalResolutionX = 0;
		totalResolutionY = 0;
		totalRawResolutionX = 0;
		totalRawResolutionY = 0;

		foreach(RUISDisplay display in displays)
		{
			totalResolutionX += display.resolutionX;
			totalResolutionY = Mathf.Max(totalResolutionY, display.resolutionY);

			totalRawResolutionX += display.rawResolutionX;
			totalRawResolutionY = Mathf.Max(totalRawResolutionY, display.rawResolutionY);
		}
	}

	// *** TODO remove this hack when Camera.ScreenPointToRay() works again
	static public Ray HMDScreenPointToRay(Vector2 screenPoint, Camera cam)
	{
		Ray ray;

		if(!cam)
			return new Ray();

		float widthFactor = 1;
		float heightFactor = 1;
		float xOffset = 0;
		float yOffset = 0;
		float windowAspect = (float) Screen.width/Screen.height;
		float eyeTextureAspect = (float) UnityEngine.VR.VRSettings.eyeTextureWidth / UnityEngine.VR.VRSettings.eyeTextureHeight;
		if(windowAspect < eyeTextureAspect)
		{
			widthFactor = windowAspect / eyeTextureAspect;
			xOffset = 0.5f*(eyeTextureAspect - windowAspect)/eyeTextureAspect;
		} else
		{
			heightFactor = eyeTextureAspect / windowAspect;
			yOffset = 0.5f*(windowAspect - eyeTextureAspect)/windowAspect;
		}
		ray = new Ray(	cam.transform.TransformPoint(Vector3.right * (-cam.stereoSeparation)), 
				cam.cameraToWorldMatrix.MultiplyPoint(new Vector3(	2*( widthFactor*screenPoint.x/Screen.width  - 0.5f + xOffset), 
											2*(heightFactor*screenPoint.y/Screen.height - 0.5f + yOffset), -1)) - cam.transform.position);

//		Vector3 rayDirection = cam.projectionMatrix.inverse.MultiplyPoint( 
//			new Vector3(	2 * (widthFactor * screenPoint.x / Screen.width - 0.5f + xOffset), 
//				2 * (heightFactor * screenPoint.y / Screen.height - 0.5f + yOffset), 1));
//		rayDirection = new Vector3(rayDirection.x, rayDirection.y, -rayDirection.z).normalized;
//		ray = new Ray(cam.transform.TransformPoint(Vector3.right * (-cam.stereoSeparation)), cam.transform.TransformDirection(rayDirection));

		return ray;
	}

	public Ray ScreenPointToRay(Vector2 screenPoint)
	{
		RUISDisplay display = GetDisplayForScreenPoint(screenPoint);

		if(display)
		{
			Camera camera = display.GetCameraForScreenPoint(screenPoint);
			
			if(camera)
			{   
				if(display.isHmdDisplay)
				{
					// *** TODO remove this hack when Camera.ScreenPointToRay() works again
					return HMDScreenPointToRay(screenPoint, camera);
				} 
				else
					return camera.ScreenPointToRay(screenPoint);
			}
		}
         
		return new Ray(Vector3.zero, Vector3.zero);
	}

	public List<ScreenPoint> WorldPointToScreenPoints(Vector3 worldPoint)
	{
		List<ScreenPoint> screenPoints = new List<ScreenPoint>();

		foreach(RUISDisplay display in displays)
		{
			display.WorldPointToScreenPoints(worldPoint, ref screenPoints);
		}

		return screenPoints;
	}

	public RUISDisplay GetDisplayForScreenPoint(Vector2 screenPoint/*, ref Vector2 relativeScreenPoint*/)
	{
		//relativeScreenPoint = Vector2.zero;

		int currentResolutionX = 0;
		foreach(RUISDisplay display in displays)
		{

			if(currentResolutionX + display.rawResolutionX >= screenPoint.x)
			{
				//relativeScreenPoint = new Vector2(screenPoint.x - currentResolutionX, totalRawResolutionY - screenPoint.y);
				return display;
			}

			currentResolutionX += display.rawResolutionX;
		}

		if(displays != null && displays[0])
			return displays[0];
		else
			return null;
	}
	/*
    public Camera GetCameraForScreenPoint(Vector2 screenPoint)
    {
        Vector2 relativeScreenPoint = Vector2.zero;
        RUISDisplay display = GetDisplayForScreenPoint(screenPoint);
        //Debug.Log(relativeScreenPoint);
        if (display)
            return display.GetCameraForScreenPoint(relativeScreenPoint, totalRawResolutionY);
        else
            return null;
    }*/

	private void UpdateResolutionsOnTheFly()
	{
		int trueWidth = Screen.width;
		int trueHeight = Screen.height;

		float widthScaler = (float)trueWidth / totalRawResolutionX;
		float heightScaler = (float)trueHeight / totalRawResolutionY;

		foreach(RUISDisplay display in displays)
		{
			display.resolutionX = (int)(display.resolutionX * widthScaler);
			display.resolutionY = (int)(display.resolutionY * heightScaler);
		}
	}

	private void DisableUnlinkedCameras()
	{
		RUISCamera[] allCameras = FindObjectsOfType(typeof(RUISCamera)) as RUISCamera[];

		foreach(RUISCamera ruisCamera in allCameras)
		{
			if(ruisCamera.associatedDisplay == null)
			{
				Debug.LogWarning("Disabling RUISCamera '" + ruisCamera.name + "' because it isn't linked into a RUISDisplay.");
				ruisCamera.gameObject.SetActive(false);
			}
		}
	}

	public void LoadDisplaysFromXML(bool refresh = false)
	{
		foreach(RUISDisplay display in displays)
		{
			display.LoadFromXML();
		}

		if(refresh)
		{
			UpdateDisplays();
		}
	}

	public void SaveDisplaysToXML()
	{
		foreach(RUISDisplay display in displays)
		{
			display.SaveToXML();
		}
	}

	public RUISDisplay GetHmdDisplay()
	{
		foreach(RUISDisplay display in displays)
		{
			if(display.linkedCamera && display.isHmdDisplay)
			{
				return display;
			}
		}

		return null;
	}

	private void InitRUISMenu(GameObject ruisMenuPrefab, int guiDisplayChoice)
	{
		if(ruisMenuPrefab == null)
			return;
		
		// HACK: displays is a list and accessing components by index might break if we modify the list in run-time
		if(displays.Count <= guiDisplayChoice)
		{
			Debug.LogError("displays.Count is too small: " + displays.Count + ", because guiDisplayChoice == " + guiDisplayChoice
			+ ". Fix the guiDisplayChoice implementation so that it conforms to the displays variable (dynamic List<>).");
			return;
		}

		if(displays[guiDisplayChoice] == null
		   || displays[guiDisplayChoice].GetComponent<RUISDisplay>() == null
		   || displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera == null)
		{
			return;
		}
		
		GameObject ruisMenu = Instantiate(ruisMenuPrefab) as GameObject;
		if(ruisMenu == null)
			return;
	
		if(menuLayer == -1)
			Debug.LogError("Could not find layer '" + LayerMask.LayerToName(menuLayer) + "', the RUIS menu cursor will not work without this layer! "
			+ "The prefab '" + ruisMenuPrefab.name + "' and its children should be on this layer.");

		if(!displays[guiDisplayChoice].GetComponent<RUISDisplay>().isStereo /* && !displays[guiDisplayChoice].GetComponent<RUISDisplay>().enableOculusRift */)
		{
			if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.centerCamera)
			{

				displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.centerCamera.gameObject.AddComponent<UICamera>();
			} else
				Debug.LogError("The " + typeof(RUISDisplay) + " that was assigned with 'RUIS Menu Prefab' in " + typeof(RUISDisplayManager) + " has an 'Attached Camera' "
				+ " whose centerCamera is null for some reason! Can't create UICamera.");
		} else
		{
			if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.rightCamera)
				displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.rightCamera.gameObject.AddComponent<UICamera>();
			else
				Debug.LogError("The " + typeof(RUISDisplay) + " that was assigned with 'RUIS Menu Prefab' in " + typeof(RUISDisplayManager) + " has an 'Attached Camera' "
				+ " whose rightCamera is null for some reason! Can't create UICamera.");
			if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.leftCamera)
				displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.leftCamera.gameObject.AddComponent<UICamera>();
			else
				Debug.LogError("The " + typeof(RUISDisplay) + " that was assigned with 'RUIS Menu Prefab' in " + typeof(RUISDisplayManager) + " has an 'Attached Camera' "
				+ " whose leftCamera is null for some reason! Can't create UICamera.");
		}

		UICamera[] NGUIcameras = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.GetComponentsInChildren<UICamera>();

		foreach(UICamera camera in NGUIcameras)
		{
			camera.eventReceiverMask = LayerMask.GetMask(LayerMask.LayerToName(menuLayer));
		}

		string primaryMenuParent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.centerCameraName;
		string secondaryMenuParent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.rightCameraName;
		string tertiaryMenuParent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.leftCameraName;
		if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.centerCamera)
		{

			ruisMenu.transform.parent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.centerCamera.transform;
		} else
		{
			if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.rightCamera)
				ruisMenu.transform.parent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.rightCamera.transform;
			else
			{
				if(displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.leftCamera)
					ruisMenu.transform.parent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.leftCamera.transform;
				else
				{
//					Debug.LogError(  "Could not find any of the following gameObjects under " 
//					               + displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.gameObject.name
//					               + ": " + primaryMenuParent + ", " + secondaryMenuParent + ", " + tertiaryMenuParent + ". RUIS Menu will be parented "
//						+ "directly under " + displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.gameObject.name + ".");
//					ruisMenu.transform.parent = displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.transform;
					Debug.LogError("Can't parent RUIS Menu Prefab under the correct Transform because the " + typeof(RUISCamera) + " in gameObject "
					+ displays[guiDisplayChoice].GetComponent<RUISDisplay>().linkedCamera.gameObject + " has null value for centerCamera, "
					+ "leftCamera, and rightCamera.");
				}
			}
		}
		
		ruisMenu.transform.localRotation = Quaternion.identity;
		ruisMenu.transform.localPosition = new Vector3(guiX, guiY, guiZ);

		if(ruisMenu.GetComponent<RUISMenuNGUI>())
			ruisMenu.GetComponent<RUISMenuNGUI>().Hide3DGUI();
	}

	#if !UNITY_EDITOR
	private static bool isOpenVrAccessible = false;
	private static bool failedToAccessOpenVr = false;
	#endif

	public static bool IsHmdPresent()
	{
		// If Unity thinks yes, then we will go with that
		if(UnityEngine.VR.VRDevice.isPresent) 
			return true;

		// Otherwise lets ask a second opinion from OpenVR
		#if UNITY_EDITOR
		return Valve.VR.OpenVR.IsHmdPresent();
		#else
		if(isOpenVrAccessible) 
			return Valve.VR.OpenVR.IsHmdPresent();
		else
		{
			bool isOpenVrHmdPresent = false;
			if(!failedToAccessOpenVr)
			{
				try
				{
					isOpenVrHmdPresent = Valve.VR.OpenVR.IsHmdPresent();
					isOpenVrAccessible = true;
				}
				catch
				{
					failedToAccessOpenVr = true;
				}
			}

			return isOpenVrHmdPresent;
		}
		#endif
	}

	#if !UNITY_EDITOR
	private static bool isSteamVrAccessible = false;
	private static bool failedToAccessSteamVr = false;
	#endif

	public static string GetHmdModel()
	{
		if(!RUISDisplayManager.IsHmdPresent())
			return "no_HMD";
		
		string hmdModel = UnityEngine.VR.VRDevice.model;

		// Lets ask OpenVR if Unity does not recognice the HMD name
		if(hmdModel == null || hmdModel == "")
		{
			#if UNITY_EDITOR
			return SteamVR.instance.hmd_ModelNumber;
			#else
			if(isSteamVrAccessible)
			{
				if(SteamVR.instance != null)
					return SteamVR.instance.hmd_ModelNumber;
			}
			else
			{
				if(!failedToAccessSteamVr)
				{
					try
					{
						if(SteamVR.instance != null)
							hmdModel = SteamVR.instance.hmd_ModelNumber;
						isSteamVrAccessible = true;
					} 
					catch
					{
						failedToAccessSteamVr = true;
					}
				}
			}
			#endif

		}
		else
			return hmdModel;

		if(hmdModel == null || hmdModel == "")
			return "unknown HMD";
		
		return hmdModel;
	}

	// *** HACK TODO need to check if the found HMD is really position tracked
	public static bool IsHmdPositionTrackable()
	{
		//		if(OVRManager.capiHmd != null)
		//		{
		//			Ovr.HmdType ovrHmdVersion = OVRManager.capiHmd.GetDesc().Type; //06to08
		//			
		//			if(    (OVRManager.capiHmd.GetTrackingState().StatusFlags & (uint)StatusBits.HmdConnected) != 0 // !isplay.isPresent
		//				&& (ovrHmdVersion == HmdType.DK2 || ovrHmdVersion == HmdType.Other)) // Rift is DK2+     //06to08
		//				return true;
		//		}
		//		return false;

		return RUISDisplayManager.IsHmdPresent();
	}
}
