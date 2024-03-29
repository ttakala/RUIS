/*****************************************************************************

Content    :   Handles the calibration procedure between Custom_1/2 and OpenVR devices
Authors    :   Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala.
               All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using CSML;
using Valve.VR;

public class RUISCustomDeviceCalibrationProcess : RUISCalibrationProcess
{	
	public string getUpperText() 
	{
		return this.guiTextUpperLocal;
	}
	
	public string getLowerText() 
	{
		return this.guiTextLowerLocal;
	}

	public string guiTextUpperLocal = "", guiTextLowerLocal = "";

	// Abstract class variables
	public override string guiTextUpper { get{return getUpperText();} }
	public override string guiTextLower { get{return getLowerText();} }

	// first* and second* variables always refer to the calibration pair "slot number"
	private RUISDevice firstInputDevice, secondInputDevice;
	bool firstDeviceError, secondDeviceError;
	private string firstInputName = "OpenVR controller"; // TODO: both devices can be custom
	private string secondInputName = "Custom_1";

	Quaternion device1PitchRotation = Quaternion.identity;
	float device1DistanceFromFloor = 0;
	Vector3 device1FloorNormal = Vector3.up;
	Quaternion device2PitchRotation = Quaternion.identity;
	float device2DistanceFromFloor = 0;
	Vector3 device2FloorNormal = Vector3.up;

	// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
	RUISCoordinateSystem.DeviceCoordinateConversion device1Conversion, device2Conversion;

	bool showMovementAlert = false;
	float lastMovementAlertTime = 0;

	// Custom variables
	private List<Vector3> samplesCustom, samplesOpenVr;
	private int numberOfSamplesTaken, numberOfSamplesToTake, numberOfSamplesPerSecond;
	private float timeSinceLastSample, timeBetweenSamples, timeSinceScriptStart = 0;
	public RUISInputManager inputManager;
	private bool openVrChecked = false, calibrationFinished = false;
	List<GameObject> calibrationSpheres;
	private GameObject calibrationPhaseObjects, calibrationResultPhaseObjects, 
	customOriginObject, floorPlane, depthView, kinectModelObject,
	firstDeviceIcon, customIcon, deviceModelObjects, depthViewObjects, iconObjects;

	private NIPlayerManagerCOMSelection kinectSelection;
	private OpenNISettingsManager settingsManager;
	private OpenNI.SceneAnalyzer sceneAnalyzer;

	Kinect2SourceManager kinect2SourceManager;
	Kinect.Body[] bodyData; 

	private trackedBody[] trackingIDs = null; // Defined in RUISKinect2DepthView
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();
	private ulong kinectTrackingID;

	Quaternion kinectPitchRotation = Quaternion.identity;
	float kinectDistanceFromFloor = 0;
	Vector3 kinectFloorNormal = Vector3.up;

	private Vector3 lastCustomSample, lastOpenVrSample;
	private string xmlFilename;
	
	private Matrix4x4 rotationMatrix, transformMatrix;

	RUISOpenVrPrefabContainer openVrPrefabContainer;
	SteamVR_TrackedObject[] trackedOpenVRObjects;
	int openVrControllerIndex = 0;
	Transform openVrControllerTransform;
	Vector3 translateAtTuneStart = Vector3.zero;
	Vector3 controllerPositionAtTuneStart = Vector3.zero;

	RUISCoordinateCalibration calibration;
	RUISCalibrationProcessSettings calibrationSettings;

	// *** TODO: Input conversion (also for floor normal and point)
	public RUISCustomDeviceCalibrationProcess(RUISCalibrationProcessSettings calibrationSettings) 
	{
		inputManager 	 = MonoBehaviour.FindObjectOfType<RUISInputManager>();
		calibration 	 = MonoBehaviour.FindObjectOfType<RUISCoordinateCalibration>();

		if(!inputManager) 
		{
			Debug.LogError ("Component " + typeof(RUISInputManager) + " not found in the calibration scene, cannot continue!");
			return;
		}
		if(!calibration) 
		{
			Debug.LogError ("Component " + typeof(RUISCoordinateCalibration) + " not found in the calibration scene, cannot continue!");
			return;
		}

		this.deviceModelObjects = calibrationSettings.deviceModelObjects;
		this.depthViewObjects = calibrationSettings.depthViewObjects;
		this.iconObjects = calibrationSettings.iconObjects;
		this.floorPlane = calibrationSettings.floorPlane; //GameObject.Find ("Floor");

		
		bool calibratingOpenVR = false;
		if(calibrationSettings.firstDevice == RUISDevice.OpenVR || calibrationSettings.secondDevice == RUISDevice.OpenVR)
		{
			firstInputDevice = RUISDevice.OpenVR;
			if(calibration.firstIconText)
			{
				calibration.firstIconText.text = RUISDevice.OpenVR.ToString();
			}
			calibratingOpenVR = true;
		}

		bool calibratingKinect = false;
		if(calibrationSettings.firstDevice == RUISDevice.Kinect_1 || calibrationSettings.secondDevice == RUISDevice.Kinect_1)
		{
			firstInputDevice = RUISDevice.Kinect_1;
			if(calibration.firstIconText)
			{
				calibration.firstIconText.text = "Kinect 1";
			}
			firstInputName = "Kinect 1";
			this.depthView = GameObject.Find("KinectDepthView");
			calibratingKinect = true;
		}
		if(calibrationSettings.firstDevice == RUISDevice.Kinect_2 || calibrationSettings.secondDevice == RUISDevice.Kinect_2)
		{
			firstInputDevice = RUISDevice.Kinect_2;
			if(calibration.firstIconText)
			{
				calibration.firstIconText.text = "Kinect 2";
			}
			firstInputName = "Kinect 2";
			this.depthView = GameObject.Find("Kinect2DepthView");
			calibratingKinect = true;
		}

		if(calibrationSettings.firstDevice == RUISDevice.Custom_1 || calibrationSettings.secondDevice == RUISDevice.Custom_1)
		{
			// Note: device1* variables always refer to RUISDevice.Custom_1, whereas first*/second* variables only refer to the calibration "slot"
			if(calibratingOpenVR || calibratingKinect)
				secondInputDevice = RUISDevice.Custom_1;
			else
				firstInputDevice = RUISDevice.Custom_1;

			if(RUISCalibrationProcessSettings.customDevice1Conversion != null)
				device1Conversion = RUISCalibrationProcessSettings.customDevice1Conversion;
			else
				device1Conversion = inputManager.customDevice1Conversion;

			string deviceName;
			if(string.IsNullOrEmpty(RUISCalibrationProcessSettings.customDevice1Name))
				deviceName = inputManager.customDevice1Name;
			else
				deviceName = RUISCalibrationProcessSettings.customDevice1Name;
			
			if(firstInputDevice == RUISDevice.Custom_1)
			{
				if(calibration.firstIconText)
				{
					calibration.firstIconText.text = string.IsNullOrEmpty(deviceName)?RUISDevice.Custom_1.ToString():deviceName;
				}
			}
			else
			{
				if(calibration.secondIconText)
				{
					calibration.secondIconText.text = string.IsNullOrEmpty(deviceName)?RUISDevice.Custom_1.ToString():deviceName;
				}
			}
				
			// There could be 2 custom-devices
			if(calibratingOpenVR || calibratingKinect)
				secondInputName = RUISDevice.Custom_1 + (string.IsNullOrEmpty(deviceName)?"":(" (" + deviceName + ")"));
			else
				firstInputName  = RUISDevice.Custom_1 + (string.IsNullOrEmpty(deviceName)?"":(" (" + deviceName + ")"));
		}

		if(calibrationSettings.firstDevice == RUISDevice.Custom_2 || calibrationSettings.secondDevice == RUISDevice.Custom_2)
		{
			// Note: device2* variables always refer to RUISDevice.Custom_2, whereas first*/second* variables only refer to the calibration "slot"
			secondInputDevice = RUISDevice.Custom_2;

			if(RUISCalibrationProcessSettings.customDevice2Conversion != null)
				device2Conversion = RUISCalibrationProcessSettings.customDevice2Conversion;
			else
				device2Conversion = inputManager.customDevice2Conversion;
			
			string deviceName;
			if(string.IsNullOrEmpty(RUISCalibrationProcessSettings.customDevice2Name))
				deviceName = inputManager.customDevice2Name;
			else
				deviceName = RUISCalibrationProcessSettings.customDevice2Name;

			if(calibration.secondIconText)
			{
				calibration.secondIconText.text = string.IsNullOrEmpty(deviceName)?RUISDevice.Custom_2.ToString():deviceName;
			}

			secondInputName = RUISDevice.Custom_2 + (string.IsNullOrEmpty(deviceName)?"":(" (" + deviceName + ")"));
		}

		if(firstInputDevice != RUISDevice.Custom_1 && secondInputDevice != RUISDevice.Custom_1 && secondInputDevice != RUISDevice.Custom_2)
			Debug.LogError("Variable calibrationSettings.firstDevice is " + calibrationSettings.firstDevice + ", and " 
							+ "calibrationSettings.secondDevice is " + calibrationSettings.secondDevice + ". Expected one of them to be " 
							+ RUISDevice.Custom_1 + " or " + RUISDevice.Custom_2);

		this.numberOfSamplesToTake = calibrationSettings.numberOfSamplesToTake;
		this.numberOfSamplesPerSecond = calibrationSettings.numberOfSamplesPerSecond;

		if(firstInputDevice == RUISDevice.OpenVR)
			SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);

//		if(RUISCalibrationProcessSettings.originalMasterCoordinateSystem == secondInputDevice) // Custom_1/2
//			calibration.coordinateSystem.rootDevice = secondInputDevice; // Custom_1/2
//		else
//			calibration.coordinateSystem.rootDevice = firstInputDevice; // OpenVR / Kinect_1 / Kinect_2
		calibration.coordinateSystem.rootDevice = firstInputDevice;

		if(firstInputDevice == RUISDevice.OpenVR)
		{
			openVrPrefabContainer = MonoBehaviour.FindObjectOfType<RUISOpenVrPrefabContainer>();
			if(openVrPrefabContainer)
			{
				if(openVrPrefabContainer.openVrCameraRigPrefab)
				{
					openVrPrefabContainer.instantiatedOpenVrCameraRig = GameObject.Instantiate(openVrPrefabContainer.openVrCameraRigPrefab);
					Camera[] rigCams = openVrPrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<Camera>();
					if(rigCams != null)
					{
						foreach(Camera cam in rigCams)
						{
							// *** TODO HACK Ugly fix, not 100% sure if in the future all the perspective cameras in the ViveCameraRig work well with below code
							if(!cam.orthographic)
								cam.nearClipPlane = 0.10f; // TODO: switch back to 0.15f;
						}
					}
				}
				else
					Debug.LogError("The viveCameraRigPrefab field in " + typeof(RUISOpenVrPrefabContainer) 
								 + " is null, and calibration will not work!");
			}
			else
			{
				Debug.LogError("Could not locate " + typeof(RUISOpenVrPrefabContainer) 
							 + " component in this scene, and calibration will not work!");
			}
		}

		this.timeSinceScriptStart = 0;
		this.timeBetweenSamples = 1.0f / (float) numberOfSamplesPerSecond;
		
		// Limit sample rate
		if(this.timeBetweenSamples < 0.1f)
		{
			this.timeBetweenSamples = 0.1f;
		}
		
		calibrationSpheres = new List<GameObject>();
		
		samplesCustom = new List<Vector3>();
		samplesOpenVr = new List<Vector3>();

		this.calibrationSettings = calibrationSettings;

		this.calibrationPhaseObjects = calibrationSettings.calibrationPhaseObjects;
		this.calibrationResultPhaseObjects = calibrationSettings.calibrationResultPhaseObjects;
		
		if(GameObject.Find("PSMoveWand") != null)
			GameObject.Find("PSMoveWand").SetActive(false);
		if(GameObject.Find("HmdModel"))
			GameObject.Find("HmdModel").SetActive(false);
		
		// Models
		this.customOriginObject = GameObject.Find("HmdCamera"); // ### TODO Custom1/2 origin GameObject
		
		// Icons
		switch(firstInputDevice)
		{
			case RUISDevice.OpenVR: this.firstDeviceIcon = GameObject.Find("PS Move Icon"); break; // TODO replace with OpenVR controller icon
			case RUISDevice.Kinect_1: this.firstDeviceIcon = GameObject.Find("Kinect Icon"); break;
			case RUISDevice.Kinect_2: this.firstDeviceIcon = GameObject.Find("Kinect2 Icon"); break;
			case RUISDevice.Custom_1: this.firstDeviceIcon = GameObject.Find("1st Icon Background"); break;
		}

		if(firstInputDevice == RUISDevice.Kinect_1 || firstInputDevice == RUISDevice.Kinect_2)
			this.kinectModelObject = GameObject.Find("Kinect2Camera");

		this.customIcon = GameObject.Find("Hmd Icon"); // ###

		if(this.firstDeviceIcon && this.firstDeviceIcon.GetComponent<GUITexture>())
			this.firstDeviceIcon.GetComponent<GUITexture>().pixelInset = new Rect(5.1f, 10.0f, 70.0f, 70.0f);

		foreach (Transform child in this.deviceModelObjects.transform)
			child.gameObject.SetActive(false);
		foreach (Transform child in this.depthViewObjects.transform)
			child.gameObject.SetActive(false);
		foreach (Transform child in this.iconObjects.transform)
			child.gameObject.SetActive(false);

		if((firstInputDevice == RUISDevice.Custom_1 || secondInputDevice == RUISDevice.Custom_1) && calibration.customDevice1Object)
			calibration.customDevice1Object.SetActive(true);
		if(secondInputDevice == RUISDevice.Custom_2 && calibration.customDevice2Object)
			calibration.customDevice2Object.SetActive(true);
		if(this.customOriginObject != null)
			this.customOriginObject.SetActive(true);
		if(this.firstDeviceIcon)
			this.firstDeviceIcon.SetActive(true);
		if(this.customIcon)
			this.customIcon.SetActive(true);
		if(calibration.firstIconText)
			calibration.firstIconText.gameObject.SetActive(true);
		if(calibration.secondIconText)
			calibration.secondIconText.gameObject.SetActive(true);
		if(this.calibrationPhaseObjects)
			this.calibrationPhaseObjects.SetActive(true);
		if(this.calibrationResultPhaseObjects)
			this.calibrationResultPhaseObjects.SetActive(false);
		if(this.depthView)
			this.depthView.SetActive(true);
		if(this.kinectModelObject)
			this.kinectModelObject.SetActive(true);
		this.xmlFilename = calibrationSettings.xmlFilename;

		if(calibratingKinect)
		{
			kinect2SourceManager = MonoBehaviour.FindObjectOfType(typeof(Kinect2SourceManager)) as Kinect2SourceManager;
			kinectSelection = MonoBehaviour.FindObjectOfType(typeof(NIPlayerManagerCOMSelection)) as NIPlayerManagerCOMSelection;
			settingsManager = MonoBehaviour.FindObjectOfType(typeof(OpenNISettingsManager)) as OpenNISettingsManager;
		}
	}
	
	
	public override RUISCalibrationPhase InitialPhase(float deltaTime) 
	{
		timeSinceScriptStart += deltaTime;

		if(!inputManager) 
		{
			this.guiTextLowerLocal = "Component " + typeof(RUISInputManager) + " not found in\n the calibration scene, cannot continue!";
			return RUISCalibrationPhase.Invalid;
		}
		if(!calibration) 
		{
			this.guiTextLowerLocal = "Component " + typeof(RUISCoordinateCalibration) + " not found in\n the calibration scene, cannot continue!";
			return RUISCalibrationPhase.Invalid;
		}

		PlaceCustomTrackedDevices();

		if(timeSinceScriptStart < 2)
		{
			this.guiTextLowerLocal = "Calibration of '" + firstInputName + "'\nand " + secondInputName + "\n\n Starting up...";
			return RUISCalibrationPhase.Initial;
		}

		// Execute once only
		if(!openVrChecked)
		{
			openVrChecked = true;

			if(firstInputDevice == RUISDevice.OpenVR)
			{
				try
				{
					if(SteamVR.instance != null)
						firstInputName = SteamVR.instance.hmd_ModelNumber;
					if(firstInputName.Contains("Vive"))
						firstInputName = "Vive controller";
					else if(firstInputName.Contains("Oculus"))
						firstInputName = "Touch controller";
					else
						firstInputName = "OpenVR controller";
					
					if(!Valve.VR.OpenVR.IsHmdPresent()) // *** TODO HACK Valve API
					{
						this.guiTextLowerLocal = "Head-mounted display is not detected!\nYou might not be able to access the " 
												+ firstInputName + "s.";
						Debug.LogError(  "Head-mounted display is not detected! This could be an indication of a bigger problem and you might "
										+ "not be able to access the " + firstInputName + "s.");
					}
				} catch
				{
					this.guiTextLowerLocal = "Failed to access " + firstInputName + ". \n\n Error: OpenVR not found! Is SteamVR installed?";
					return RUISCalibrationPhase.Invalid;
				}
			}
		}

		if(timeSinceScriptStart < 4)
		{
			if(firstInputDevice == RUISDevice.OpenVR)
				this.guiTextLowerLocal = "Detected OpenVR.";
			else
				this.guiTextLowerLocal = "Detecting " + firstInputName + ".";
			return RUISCalibrationPhase.Initial;
		}

		// Wait to connect to customDevice
//		if(timeSinceScriptStart < 5)
//		{
//			this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Please wait...";
//			return RUISCalibrationPhase.Initial;
//		}

		lastCustomSample = new Vector3(0, 0, 0);
		bool customObjectsExist = false;
		string missingObjectError = " Tracked Pose\"\n field in " + typeof(RUISCoordinateCalibration) + "\ncomponent is null!";

		// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
		if(firstInputDevice != RUISDevice.Custom_2 && secondInputDevice == RUISDevice.Custom_1)
		{
			if(calibration.customDevice1Tracker)
			{
				customObjectsExist = true;
				lastCustomSample = calibration.customDevice1Tracker.position;
			}
			else
				missingObjectError = "Error: \"" + RUISDevice.Custom_1 + missingObjectError;
		}
		else if(firstInputDevice != RUISDevice.Custom_1 && secondInputDevice == RUISDevice.Custom_2)
		{
			if(calibration.customDevice2Tracker)
			{
				customObjectsExist = true;
				lastCustomSample = calibration.customDevice2Tracker.position;
			}
			else
				missingObjectError = "Error: \"" + RUISDevice.Custom_2 + missingObjectError;
		}
		else if(firstInputDevice == RUISDevice.Custom_1 && secondInputDevice == RUISDevice.Custom_2)
		{
			if(calibration.customDevice1Tracker)
			{
				if(calibration.customDevice2Tracker)
				{
					customObjectsExist = true;
					lastCustomSample = calibration.customDevice2Tracker.position;
				}
				else
					missingObjectError = "Error: \"" + RUISDevice.Custom_2 + missingObjectError;
			}
			else
				missingObjectError = "Error: \"" + RUISDevice.Custom_1 + missingObjectError;
		}
			
		// Rest of this method will be executed once only, because the execution will jump to Invalid or Preparation phase
		if(!customObjectsExist)
		{
			this.guiTextLowerLocal = missingObjectError;
			Debug.LogError(missingObjectError);
			return RUISCalibrationPhase.Invalid;
		}

		if(firstInputDevice == RUISDevice.Kinect_1)
		{
			bool kinectSetupError = false;
			if(settingsManager == null) 
			{
				this.guiTextLowerLocal = "Kinect error: Missing \nOpenNISettingsManager script";
				Debug.LogError("Kinect error: Missing OpenNISettingsManager script");
				return RUISCalibrationPhase.Invalid;
			}
			else if(settingsManager.UserGenrator == null) 
				kinectSetupError = true;
			else if(!settingsManager.UserGenrator.Valid) 
				kinectSetupError = true;
			else 
			{
				try
				{
					if(settingsManager != null && settingsManager.CurrentContext != null && settingsManager.CurrentContext.BasicContext != null)
						sceneAnalyzer = new OpenNI.SceneAnalyzer(settingsManager.CurrentContext.BasicContext);
					else
						kinectSetupError = true;
					if(sceneAnalyzer != null)
						sceneAnalyzer.StartGenerating();
					else
						kinectSetupError = true;
				}
				catch(System.Exception e)
				{
					Debug.LogError(e.Message);
					this.guiTextLowerLocal = "Kinect error: Could not start OpenNI";
					return RUISCalibrationPhase.Invalid;
				}
			}

			if(kinectSetupError)
			{
				this.guiTextLowerLocal = "Kinect error: Could not start OpenNI";
				return RUISCalibrationPhase.Invalid;
			}
		}

		if(firstInputDevice == RUISDevice.Kinect_2)
		{
			if(		kinect2SourceManager.GetSensor() == null || !kinect2SourceManager.GetSensor().IsOpen 
				|| !kinect2SourceManager.GetSensor().IsAvailable										)
			{
				this.guiTextLowerLocal = "Kinect error: Sensor is unavailable.";
				return RUISCalibrationPhase.Invalid;
			}
		}

		if(customObjectsExist)
			return RUISCalibrationPhase.Preparation;
		
		return RUISCalibrationPhase.Invalid; // Loop should not get this far
	}
	
	
	public override RUISCalibrationPhase PreparationPhase(float deltaTime)
	{
		if(firstInputDevice == RUISDevice.Kinect_1 || firstInputDevice == RUISDevice.Kinect_2)
			this.guiTextLowerLocal = "Step in front of Kinect. \nTake " + secondInputName + " into your right hand.";
		else
			this.guiTextLowerLocal = "Hold " + firstInputName + " and \n" + secondInputName + " together.";

		PlaceCustomTrackedDevices();

		if(firstInputDevice == RUISDevice.OpenVR)
		{
			if(openVrPrefabContainer && openVrPrefabContainer.instantiatedOpenVrCameraRig)
				trackedOpenVRObjects = openVrPrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

			if(trackedOpenVRObjects != null && trackedOpenVRObjects.Length != 0)
			{
				foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
					if(trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1)
						this.guiTextUpperLocal = firstInputName + " not detected.";
					else if(this.guiTextUpperLocal.Contains(" not detected"))
						this.guiTextUpperLocal = "";
			}
			else
				this.guiTextUpperLocal = firstInputName + " not detected.";
		}

		bool kinectIsReady = false;
		if(firstInputDevice == RUISDevice.Kinect_1 && kinectSelection.GetNumberOfSelectedPlayers() >= 1)
			kinectIsReady = true;
		if(firstInputDevice == RUISDevice.Kinect_2)
		{
			UpdateBodyData();
			kinectTrackingID = 0;

			for(int a = 0; a < trackingIDs.Length; a++) 
			{
				if(trackingIDs[a].isTracking) 
				{
					kinectTrackingID = trackingIDs[a].trackingId;
				}
			}

			if(kinectTrackingID != 0)
				kinectIsReady = true;
		}

		// Has a duration of timeSinceLastSample passed?
		if(timeSinceLastSample < Mathf.Clamp(timeBetweenSamples, float.Epsilon, 1))
		{
			timeSinceLastSample += deltaTime;
		}
		else
		{
			timeSinceLastSample = 0;
			// Check if customDevice1/2Tracker position is moving
			// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
			Vector3 customDeviceTrackerPosition = Vector3.zero;
			if(secondInputDevice == RUISDevice.Custom_1)
				customDeviceTrackerPosition = calibration.customDevice1Tracker.position;
			if(secondInputDevice == RUISDevice.Custom_2)
				customDeviceTrackerPosition = calibration.customDevice2Tracker.position;

			if(lastCustomSample != customDeviceTrackerPosition)
			{
				if((firstInputDevice != RUISDevice.Kinect_1 && firstInputDevice != RUISDevice.Kinect_2) || kinectIsReady)
					return RUISCalibrationPhase.ReadyToCalibrate;
			}
			else if(	firstInputDevice != RUISDevice.OpenVR 
					|| (	trackedOpenVRObjects != null && trackedOpenVRObjects.Length > 0
						&& (trackedOpenVRObjects.Length != 1 || trackedOpenVRObjects[0].index != SteamVR_TrackedObject.EIndex.Hmd)))
				this.guiTextUpperLocal = "No motion detected in\n'" + secondInputDevice + " Tracked Pose' field of\n" 
										+ typeof(RUISCoordinateCalibration) + " component.\nIts position should correspond to\nthe tracked "
										+ secondInputName + " device.";
			lastCustomSample = customDeviceTrackerPosition;
		}

		return RUISCalibrationPhase.Preparation;
	}
	
	
	public override RUISCalibrationPhase ReadyToCalibratePhase(float deltaTime) 
	{
		if(firstInputDevice == RUISDevice.OpenVR)
			this.guiTextLowerLocal = "Hold " + firstInputName + " and " + secondInputName + " together."
									+"\nPress the trigger button to start calibrating.";
		else // Custom 1 / Custom 2 / Kinect calibration
		{
			lastOpenVrSample = new Vector3(0, 0, 0);
			lastCustomSample = new Vector3(0, 0, 0);
			return RUISCalibrationPhase.Calibration;
		}
			
		PlaceCustomTrackedDevices();

		if(firstInputDevice == RUISDevice.OpenVR)
		{
			if(openVrPrefabContainer && openVrPrefabContainer.instantiatedOpenVrCameraRig)
				trackedOpenVRObjects = openVrPrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

			if(trackedOpenVRObjects != null && trackedOpenVRObjects.Length != 0)
			{
				foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
				{
//					Debug.Log(trackedOpenVRObject.index + " " + ((int) trackedOpenVRObject.index));
					if(false /*   SteamVR_Controller.Input((int)trackedOpenVRObject.index).connected			// *** UPDATED TO STEAMVR2.5
						&& SteamVR_Controller.Input((int)trackedOpenVRObject.index).GetHairTrigger() */)		// *** UPDATED TO STEAMVR2.5
					{
						openVrControllerIndex = (int)trackedOpenVRObject.index;
						openVrControllerTransform = trackedOpenVRObject.transform;
						lastOpenVrSample = new Vector3(0, 0, 0);
						lastCustomSample = new Vector3(0, 0, 0);
						return RUISCalibrationPhase.Calibration;
					}

					if(trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1)
						this.guiTextUpperLocal = firstInputName + " not detected.";
					else if(this.guiTextUpperLocal.Contains(" not detected"))
						this.guiTextUpperLocal = "";
				}
			}
			else
			{
				this.guiTextUpperLocal = firstInputName + " not detected.";
			}
		}

		return RUISCalibrationPhase.ReadyToCalibrate;
	}

	public override RUISCalibrationPhase CalibrationPhase(float deltaTime) 
	{
		if(firstInputDevice == RUISDevice.Kinect_1 || firstInputDevice == RUISDevice.Kinect_2)
			this.guiTextLowerLocal = string.Format(   "Calibrating... {0}/{1} samples taken.\n\n"
													+ "Keep the " + secondInputDevice + " in your right\n"
													+ "hand and make wide, calm motions with it.\n"
													+ "Have both sensors see it.", numberOfSamplesTaken, numberOfSamplesToTake);
		else
			this.guiTextLowerLocal = string.Format(  "Calibrating... {0}/{1} samples taken.\n\n"
			                                       + "Keep the " + firstInputName + " right next to the " + secondInputDevice
			                                       + "\ntracking target, and make wide, calm motions with them.\n"
			                                       + "Have both sensors see them.", numberOfSamplesTaken, numberOfSamplesToTake);

		PlaceCustomTrackedDevices();

		TakeSample(deltaTime);
		
		if(numberOfSamplesTaken >= numberOfSamplesToTake) 
		{
			timeSinceScriptStart = 0;
			this.calibrationPhaseObjects.SetActive(false);
			this.calibrationResultPhaseObjects.SetActive(true);
			if(this.depthView)
				this.depthView.SetActive(false);
			return RUISCalibrationPhase.ShowResults;
		}
		else 
		{ 
			return RUISCalibrationPhase.Calibration;
		}
	}

	public override RUISCalibrationPhase ShowResultsPhase(float deltaTime) 
	{
		if(!calibrationFinished) 
		{
			float totalErrorDistance, averageError;
			CalculateTransformation();
			
			float distance = 0;
			Vector3 error = Vector3.zero;
			List<float> errorMagnitudes = new List<float>();
			for (int i = 0; i < calibrationSpheres.Count; i++)
			{
				GameObject sphere = calibrationSpheres[i];
//				Vector3 cubePosition =  transformMatrix.MultiplyPoint3x4(samples_Kinect2[i]);
				Vector3 cubePosition = transformMatrix.inverse.MultiplyPoint3x4(samplesCustom[i]);
				GameObject cube = MonoBehaviour.Instantiate(calibrationSettings.device1SamplePrefab, cubePosition, Quaternion.identity) as GameObject;
				cube.GetComponent<RUISSampleDifferenceVisualizer>().device2SamplePrefab = sphere;
				
				
				distance += Vector3.Distance(sphere.transform.position, cubePosition);
				errorMagnitudes.Add(distance);
				error += cubePosition - sphere.transform.position;
				
				sphere.transform.parent = calibrationResultPhaseObjects.transform;
				cube.transform.parent = calibrationResultPhaseObjects.transform;
			}
			
			totalErrorDistance = distance;
			averageError = distance / calibrationSpheres.Count;
			
			calibrationResultPhaseObjects.SetActive(true);

			FixedFollowTransform followTransform = Component.FindObjectOfType<FixedFollowTransform>();
			if(followTransform && openVrControllerTransform)
				followTransform.transformToFollow = openVrControllerTransform;

			string openVRAdjustInfo = "";
			if(firstInputDevice == RUISDevice.OpenVR)
				openVRAdjustInfo = "Hold down " +  firstInputName + " touchpad\nbutton to drag and finetune the results.";
			this.guiTextUpperLocal = string.Format("Calibration finished!\nTotal Error: {0:0.####}\nMean: {1:0.####}\n\n" + openVRAdjustInfo,
			                                       totalErrorDistance, averageError);

			calibrationFinished = true;                                  
		}

		PlaceCustomTrackedDevices();

		// Fine tune translation with OpenVR controller
		if(firstInputDevice == RUISDevice.OpenVR)
		{
			// *** UPDATED TO STEAMVR2.5 this whole block is commented
			/*
			if(openVrPrefabContainer && openVrPrefabContainer.instantiatedOpenVrCameraRig)
				trackedOpenVRObjects = openVrPrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

			if(SteamVR_Controller.Input(openVrControllerIndex).connected) 
			{
				var pose = new SteamVR_Utils.RigidTransform(SteamVR_Controller.Input(openVrControllerIndex).GetPose().mDeviceToAbsoluteTracking);

				if(SteamVR_Controller.Input(openVrControllerIndex).hasTracking)
				{
					if(SteamVR_Controller.Input(openVrControllerIndex).GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
					{
						controllerPositionAtTuneStart = pose.pos;
						translateAtTuneStart = new Vector3(transformMatrix[0, 3], transformMatrix[1, 3], transformMatrix[2, 3]);
					}

					if(SteamVR_Controller.Input(openVrControllerIndex).GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad))
					{
						Quaternion rotationQuaternion = MathUtil.QuaternionFromMatrix(rotationMatrix);
						Vector3 translate = translateAtTuneStart + rotationQuaternion * (controllerPositionAtTuneStart - pose.pos);

						transformMatrix.SetColumn(3, new Vector4(translate.x, translate.y, translate.z, transformMatrix.m33));

						updateDictionaries(	calibration.coordinateSystem.RUISCalibrationResultsInVector3, 
											calibration.coordinateSystem.RUISCalibrationResultsInQuaternion,
											calibration.coordinateSystem.RUISCalibrationResultsIn4x4Matrix,
											translate, rotationQuaternion, transformMatrix,
											firstInputDevice, secondInputDevice);
					}
				}

				if(SteamVR_Controller.Input(openVrControllerIndex).GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
				{
					calibration.coordinateSystem.SetDeviceToRootTransforms(transformMatrix);
					calibration.coordinateSystem.SaveTransformDataToXML(xmlFilename, firstInputDevice, secondInputDevice); 
	//				calibration.coordinateSystem.SaveFloorData(xmlFilename, inputDevice2, device1FloorNormal, device1DistanceFromFloor);
				}
			}
			*/
		}

		return RUISCalibrationPhase.ShowResults;
	}

	private Vector3 tempPosition;
	private Quaternion tempRotation;
	public void PlaceCustomTrackedDevices()
	{
		// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
		if(		(firstInputDevice == RUISDevice.Custom_1 || secondInputDevice == RUISDevice.Custom_1) 
			&& 	calibration.customDevice1Object && calibration.customDevice1Tracker) 
		{
			tempPosition = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice1Tracker.position, device1Conversion);
			tempRotation = RUISCoordinateSystem.ConvertRawRotation(calibration.customDevice1Tracker.rotation, device1Conversion);
			if(calibrationFinished)
			{
				calibration.customDevice1Object.transform.position = calibration.coordinateSystem.ConvertLocation(tempPosition, RUISDevice.Custom_1);
				calibration.customDevice1Object.transform.rotation = calibration.coordinateSystem.ConvertRotation(tempRotation, RUISDevice.Custom_1);
			}
			else
			{
				calibration.customDevice1Object.transform.position = tempPosition;
				calibration.customDevice1Object.transform.rotation = tempRotation;
			}
		}
		if(		secondInputDevice == RUISDevice.Custom_2
			&& 	calibration.customDevice2Object && calibration.customDevice2Tracker) 
		{
			tempPosition = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice2Tracker.position, device2Conversion);
			tempRotation = RUISCoordinateSystem.ConvertRawRotation(calibration.customDevice2Tracker.rotation, device2Conversion);
			if(calibrationFinished)
			{
				calibration.customDevice2Object.transform.position = calibration.coordinateSystem.ConvertLocation(tempPosition, RUISDevice.Custom_2);
				calibration.customDevice2Object.transform.rotation = calibration.coordinateSystem.ConvertRotation(tempRotation, RUISDevice.Custom_2);
			}
			else
			{
				calibration.customDevice2Object.transform.position = tempPosition;
				calibration.customDevice2Object.transform.rotation = tempRotation;
			}
		}
	}

	// This is invoked from RUISCoordinateCalibration.cs
	public override void PlaceSensorModels()
	{
		if(calibration.coordinateSystem.rootDevice == secondInputDevice) // Custom_1/2
		{
			if(firstInputDevice == RUISDevice.OpenVR && openVrPrefabContainer && openVrPrefabContainer.instantiatedOpenVrCameraRig)
			{
				openVrPrefabContainer.instantiatedOpenVrCameraRig.transform.localRotation = 
																		calibration.coordinateSystem.GetHmdCoordinateSystemYaw(RUISDevice.OpenVR);
				openVrPrefabContainer.instantiatedOpenVrCameraRig.transform.localScale    = 
																		calibration.coordinateSystem.ExtractLocalScale(RUISDevice.OpenVR);
				openVrPrefabContainer.instantiatedOpenVrCameraRig.transform.localPosition = 
																		calibration.coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.OpenVR);
			}
		}

		if(kinectModelObject && (firstInputDevice == RUISDevice.Kinect_1 || firstInputDevice == RUISDevice.Kinect_2))
		{
			kinectModelObject.transform.rotation = calibration.coordinateSystem.ConvertRotation(Quaternion.identity, firstInputDevice); 
			kinectModelObject.transform.position = calibration.coordinateSystem.ConvertLocation(Vector3.zero, firstInputDevice); // Kinect1/2
		}

		// TODO Now this uses distance from floor and "sensor pitch".
		if(customOriginObject)
		{
			customOriginObject.transform.rotation = 
											calibration.coordinateSystem.ConvertRotation(Quaternion.identity, secondInputDevice); // Custom 1 / 2
			customOriginObject.transform.position = 
											calibration.coordinateSystem.ConvertLocation(Vector3.zero, secondInputDevice); // Custom 1 / 2
		}

		if(this.floorPlane)
			this.floorPlane.transform.position = new Vector3(0, 0, 0);
	}
	
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) 
	{
		// Source: http://answers.unity3d.com/questions/11363/converting-matrix4x4-to-quaternion-vector3.html
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2;
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2;
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2;
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2;
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
	
	// Custom functionsRUISCalibrationPhase.Stopped
	private void TakeSample(float deltaTime)
	{
		timeSinceLastSample += deltaTime;
		if(timeSinceLastSample < timeBetweenSamples) 
			return;
		timeSinceLastSample = 0;
		
		
		Vector3 openVrSample = GetSample(this.firstInputDevice);
		Vector3 customSample = GetSample(this.secondInputDevice); // Custom_1/2

		if(firstDeviceError)
		{
			showMovementAlert = false;
			return;
		}

		// Check that we didn't just start
		if(customSample == Vector3.zero || openVrSample == Vector3.zero) 
		{
			return;
		}

		// Check that we are not taking samples too frequently
		if(   Vector3.Distance(customSample, lastCustomSample) < calibrationSettings.sampleMinDistance
		   || Vector3.Distance(openVrSample, lastOpenVrSample) < calibrationSettings.sampleMinDistance)
		{
			if(!showMovementAlert && Time.time - lastMovementAlertTime > 3)
			{
				lastMovementAlertTime = Time.time;
				showMovementAlert = true;
			}
			if(showMovementAlert)
				this.guiTextUpperLocal = "Not enough device movement.";
			return;
		}
		else
		{
			lastMovementAlertTime = Time.time;
			if(showMovementAlert)
				showMovementAlert = false;
			this.guiTextUpperLocal = "";
		}

		lastCustomSample = customSample;
		lastOpenVrSample = openVrSample;

//		Debug.Log(openVrSample + " " + customSample);
		samplesCustom.Add(customSample);
		samplesOpenVr.Add(openVrSample);
		calibrationSpheres.Add(MonoBehaviour.Instantiate(calibrationSettings.device2SamplePrefab, openVrSample, Quaternion.identity) as GameObject);
		numberOfSamplesTaken++;
	} 
	
	
	private Vector3 GetSample(RUISDevice device) 
	{
		Vector3 sample = new Vector3(0,0,0);

		// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
		if(device == RUISDevice.Custom_1) 
		{
			sample = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice1Tracker.position, device1Conversion);

			if(secondInputDevice == RUISDevice.Custom_1)
				secondDeviceError = false;
			else 
				firstDeviceError = false;
		}
		if(device == RUISDevice.Custom_2) 
		{
			sample = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice2Tracker.position, device2Conversion);
			
			secondDeviceError = false;
		}
		if(device == RUISDevice.OpenVR)
		{
			if(openVrPrefabContainer && openVrPrefabContainer.instantiatedOpenVrCameraRig)
				trackedOpenVRObjects = openVrPrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

			if(trackedOpenVRObjects != null && trackedOpenVRObjects.Length != 0)
			{
				foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
				{
					if(openVrControllerIndex == (int)trackedOpenVRObject.index)
					{
						if( false /*   SteamVR_Controller.Input(openVrControllerIndex).connected	// *** UPDATED TO STEAMVR2.5
							&& SteamVR_Controller.Input(openVrControllerIndex).valid				// *** UPDATED TO STEAMVR2.5
							&& SteamVR_Controller.Input(openVrControllerIndex).hasTracking */)		// *** UPDATED TO STEAMVR2.5
						{
							sample = trackedOpenVRObject.transform.localPosition;
							firstDeviceError = false;

							if(this.guiTextUpperLocal.Contains(" not tracked"))
								this.guiTextUpperLocal = "";
							break;
						}
						else
						{
							firstDeviceError = true;
							this.guiTextUpperLocal = firstInputName + " not tracked.";
						}
					}

					if(trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1) 
					{
						firstDeviceError = true;
						this.guiTextUpperLocal = firstInputName + " not detected.";
					}
					else if(this.guiTextUpperLocal.Contains(" not detected"))
						this.guiTextUpperLocal = "";
				}
			}
			else
			{
				firstDeviceError = true;
				this.guiTextUpperLocal = firstInputName + " not detected.";
			}

//			if(    SteamVR_Controller.Input(viveControllerIndex).valid
//				&& SteamVR_Controller.Input(viveControllerIndex).connected
//				&& SteamVR_Controller.Input(viveControllerIndex).hasTracking)
//			{
//				var pose = new SteamVR_Utils.RigidTransform(SteamVR_Controller.Input(viveControllerIndex).GetPose().mDeviceToAbsoluteTracking);
//
//				sample = pose.pos;
//				device2Error = false;
//			}
//			else
//			{
//				device2Error = true;
//				this.guiTextUpperLocal = openVRDeviceName + " not detected.";
//			}
		}
		if(device == RUISDevice.Kinect_2) 
		{
			UpdateBodyData();
			Kinect.Body[] data = kinect2SourceManager.GetBodyData();
			bool trackedBodyFound = false;
			int foundBodies = 0;
			foreach(var body in data) 
			{
				foundBodies++;
				if(body.IsTracked)
				{
					if(trackingIDtoIndex[body.TrackingId] == 0)
					{
						trackedBodyFound = true;
						if(body.Joints[Kinect.JointType.HandRight].TrackingState == Kinect.TrackingState.Tracked) 
						{
							sample = new Vector3(body.Joints[Kinect.JointType.HandRight].Position.X,
								body.Joints[Kinect.JointType.HandRight].Position.Y,
								body.Joints[Kinect.JointType.HandRight].Position.Z );
							sample = calibration.coordinateSystem.ConvertRawKinect2Location(sample);
							firstDeviceError = false;
							if(!secondDeviceError)
								this.guiTextUpperLocal = "";
						}
					}
				}
			}
			if(!trackedBodyFound && foundBodies > 1) 
			{
				firstDeviceError = true;
				if(!secondDeviceError)
					this.guiTextUpperLocal = "Step out of the Kinect's\nview and come back.";
			}
		}
		if(device == RUISDevice.Kinect_1) 
		{
			if(kinectSelection.GetNumberOfSelectedPlayers() > 0)
			{
				OpenNI.SkeletonJointPosition jointPosition;
				bool success = kinectSelection.GetPlayer(0).GetSkeletonJointPosition(OpenNI.SkeletonJoint.RightHand, out jointPosition);
				if(success && jointPosition.Confidence >= 0.5) 
				{ 
					sample = calibration.coordinateSystem.ConvertRawKinectLocation(jointPosition.Position);
				}
				firstDeviceError = false;
				if(!secondDeviceError) 
					this.guiTextUpperLocal = "";
			}
			else
			{
				if(!secondDeviceError)
					this.guiTextUpperLocal = "Step in front of Kinect. \nHold " + secondInputName + " in your right hand.";
				firstDeviceError = true;
			}
		}

		return sample;
	}
	
	private void CalculateTransformation()
	{
		if (samplesCustom.Count != numberOfSamplesTaken || samplesOpenVr.Count != numberOfSamplesTaken)
		{
			Debug.LogError("Mismatch in sample list lengths!");
		}
		
		Matrix openVrMatrix;
		Matrix customMatrix;
		
		openVrMatrix = Matrix.Zeros (samplesOpenVr.Count, 4);
		customMatrix = Matrix.Zeros (samplesCustom.Count, 3);
		
		for (int i = 1; i <= samplesOpenVr.Count; i++) {
			openVrMatrix [i, 1] = new Complex (samplesOpenVr [i - 1].x);
			openVrMatrix [i, 2] = new Complex (samplesOpenVr [i - 1].y);
			openVrMatrix [i, 3] = new Complex (samplesOpenVr [i - 1].z);
			openVrMatrix [i, 4] = new Complex (1.0f);
		}
		for (int i = 1; i <= samplesCustom.Count; i++) {
			customMatrix [i, 1] = new Complex (samplesCustom [i - 1].x);
			customMatrix [i, 2] = new Complex (samplesCustom [i - 1].y);
			customMatrix [i, 3] = new Complex (samplesCustom [i - 1].z);
		}
		
		//perform a matrix solve Ax = B. We have to get transposes and inverses because openVrMatrix isn't square
		//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
		Matrix transformMatrixSolution = (openVrMatrix.Transpose() * openVrMatrix).Inverse() * openVrMatrix.Transpose() * customMatrix;
		
		Matrix error = openVrMatrix * transformMatrixSolution - customMatrix;
		
		transformMatrixSolution = transformMatrixSolution.Transpose();
		
		Debug.Log(transformMatrixSolution);
		Debug.Log(error);
		
		List<Vector3> orthogonalVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(MathUtil.MatrixToMatrix4x4(transformMatrixSolution)));
		rotationMatrix = CreateRotationMatrix(orthogonalVectors);
		Debug.Log(rotationMatrix);
		
		transformMatrix = MathUtil.MatrixToMatrix4x4(transformMatrixSolution);//CreateTransformMatrix(transformMatrixSolution);
		Debug.Log(transformMatrix);

		if(firstInputDevice == RUISDevice.Custom_1 || secondInputDevice == RUISDevice.Custom_1)
			UpdateFloorNormalAndDistance(RUISDevice.Custom_1);
		if(secondInputDevice == RUISDevice.Custom_2)
			UpdateFloorNormalAndDistance(RUISDevice.Custom_2);
		if(firstInputDevice == RUISDevice.Kinect_1)
			UpdateFloorNormalAndDistance(RUISDevice.Kinect_1);
		if(firstInputDevice == RUISDevice.Kinect_2)
			UpdateFloorNormalAndDistance(RUISDevice.Kinect_2);

		Quaternion rotationQuaternion = MathUtil.QuaternionFromMatrix(rotationMatrix);

		calibration.coordinateSystem.SetDeviceToRootTransforms(transformMatrix);
		calibration.coordinateSystem.SaveTransformDataToXML(xmlFilename, firstInputDevice, secondInputDevice); // OpenVR/Custom_1, Custom_1/2

		Vector3 translate = new Vector3(transformMatrix[0, 3], transformMatrix[1, 3], transformMatrix[2, 3]);
		updateDictionaries(	calibration.coordinateSystem.RUISCalibrationResultsInVector3, 
		                   	calibration.coordinateSystem.RUISCalibrationResultsInQuaternion,
		                   	calibration.coordinateSystem.RUISCalibrationResultsIn4x4Matrix,
							translate, rotationQuaternion, transformMatrix,
							firstInputDevice, secondInputDevice); // OpenVR/Custom_1, Custom_1/2 [3 permutations]

		if(firstInputDevice == RUISDevice.Custom_1 || secondInputDevice == RUISDevice.Custom_1)
		{
			calibration.coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.Custom_1, device1FloorNormal, device1DistanceFromFloor);
			calibration.coordinateSystem.RUISCalibrationResultsDistanceFromFloor[RUISDevice.Custom_1]  = device1DistanceFromFloor;
			calibration.coordinateSystem.RUISCalibrationResultsFloorPitchRotation[RUISDevice.Custom_1] = device1PitchRotation;
		}
		if(secondInputDevice == RUISDevice.Custom_2)
		{
			calibration.coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.Custom_2, device2FloorNormal, device2DistanceFromFloor);
			calibration.coordinateSystem.RUISCalibrationResultsDistanceFromFloor[RUISDevice.Custom_2]  = device2DistanceFromFloor;
			calibration.coordinateSystem.RUISCalibrationResultsFloorPitchRotation[RUISDevice.Custom_2] = device2PitchRotation;
		}
		if(firstInputDevice == RUISDevice.Kinect_1 || firstInputDevice == RUISDevice.Kinect_2)
		{
			calibration.coordinateSystem.SaveFloorData(xmlFilename, firstInputDevice, kinectFloorNormal, kinectDistanceFromFloor);
			calibration.coordinateSystem.RUISCalibrationResultsDistanceFromFloor[firstInputDevice]  = kinectDistanceFromFloor;
			calibration.coordinateSystem.RUISCalibrationResultsFloorPitchRotation[firstInputDevice] = kinectPitchRotation;
		}
	}

	private static Matrix4x4 CreateRotationMatrix(List<Vector3> vectors)
	{
		Matrix4x4 result = new Matrix4x4();
		result.SetColumn(0, new Vector4(vectors[0].x, vectors[0].y, vectors[0].z, 0));
		result.SetColumn(1, new Vector4(vectors[1].x, vectors[1].y, vectors[1].z, 0));
		result.SetColumn(2, new Vector4(vectors[2].x, vectors[2].y, vectors[2].z, 0));
		
		result[3, 3] = 1.0f;
		
		return result;
	}
	
	private static Matrix4x4 CreateTransformMatrix(Matrix transformMatrix)
	{
		Matrix4x4 result = new Matrix4x4();
		
		result.SetRow(0, new Vector4((float)transformMatrix[1, 1].Re, (float)transformMatrix[1, 2].Re, (float)transformMatrix[1, 3].Re, (float)transformMatrix[4, 1].Re));
		result.SetRow(1, new Vector4((float)transformMatrix[2, 1].Re, (float)transformMatrix[2, 2].Re, (float)transformMatrix[2, 3].Re, (float)transformMatrix[4, 2].Re));
		result.SetRow(2, new Vector4((float)transformMatrix[3, 1].Re, (float)transformMatrix[3, 2].Re, (float)transformMatrix[3, 3].Re, (float)transformMatrix[4, 3].Re));
		
		result.m33 = 1.0f;
		
		return result;
	}

	// *** ### TODO: Test that this works
	private void UpdateFloorNormalAndDistance(RUISDevice device)
	{
		Transform customDeviceFloorPoint = null;

		// device1* and device2* variables always refer to RUISDevice.Custom_1 and RUISDevice.Custom_2, respectively
		calibration.coordinateSystem.ResetFloorNormal(device); // CustomDevice1/2
		if(device == RUISDevice.Custom_1 && calibration.customDevice1FloorPoint)
		{
			customDeviceFloorPoint = calibration.customDevice1FloorPoint;
		}
		if(device == RUISDevice.Custom_2 && calibration.customDevice2FloorPoint)
		{
			customDeviceFloorPoint = calibration.customDevice2FloorPoint;
		}
		if(customDeviceFloorPoint && (device == RUISDevice.Custom_1 || device == RUISDevice.Custom_2))
		{
			Vector3 newFloorNormal   = customDeviceFloorPoint.up.normalized;
			Vector3 newFloorPosition = customDeviceFloorPoint.position;

			if(device == RUISDevice.Custom_1)
			{
				newFloorPosition = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice1FloorPoint.position, device1Conversion);
				newFloorNormal   = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice1FloorPoint.up,		 device1Conversion);
			}
			if(device == RUISDevice.Custom_2)
			{
				newFloorPosition = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice2FloorPoint.position, device2Conversion);
				newFloorNormal   = RUISCoordinateSystem.ConvertRawLocation(calibration.customDevice2FloorPoint.up,		 device2Conversion);
			}

			//Project the position of the customDevice origin onto the floor
			//http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
			//http://en.wikipedia.org/wiki/Plane_(geometry)
			float d = newFloorNormal.x * newFloorPosition.x + newFloorNormal.y * newFloorPosition.y + newFloorNormal.z * newFloorPosition.z;
			Vector3 closestFloorPointToCustomDevice = new Vector3(newFloorNormal.x, newFloorNormal.y, newFloorNormal.z);
			closestFloorPointToCustomDevice = (closestFloorPointToCustomDevice * d) / closestFloorPointToCustomDevice.sqrMagnitude;

			Quaternion customFloorRotator = Quaternion.FromToRotation(newFloorNormal, Vector3.up);

			//transform the point from customDevice's coordinate system rotation to Unity's rotation
			closestFloorPointToCustomDevice = customFloorRotator * closestFloorPointToCustomDevice;

			if(float.IsNaN(closestFloorPointToCustomDevice.magnitude))
				closestFloorPointToCustomDevice = Vector3.zero;
			if(newFloorNormal.sqrMagnitude < 0.1f)
				newFloorNormal = Vector3.up;

			if(device == RUISDevice.Custom_1)
			{
				device1DistanceFromFloor = closestFloorPointToCustomDevice.magnitude;
				device1FloorNormal		 = newFloorNormal;
				device1PitchRotation	 = Quaternion.Inverse(customFloorRotator);
			}
			if(device == RUISDevice.Custom_2)
			{
				device2DistanceFromFloor = closestFloorPointToCustomDevice.magnitude;
				device2FloorNormal		  = newFloorNormal;
				device2PitchRotation	  = Quaternion.Inverse(customFloorRotator);
			}
		}

		Quaternion kinectFloorRotator = Quaternion.identity;
		if(device == RUISDevice.Kinect_1)
		{
			OpenNI.Plane3D floor;

			try{
				floor = sceneAnalyzer.Floor;
			}
			catch(System.Exception e)
			{
				Debug.LogError(e.TargetSite + ": Failed to get OpenNI.SceneAnalyzer.Floor.");
				return;
			}

			kinectFloorNormal= new Vector3(floor.Normal.X, floor.Normal.Y, floor.Normal.Z);
			kinectFloorNormal.Normalize();

			if(kinectFloorNormal.sqrMagnitude < 0.1f)
				kinectFloorNormal = Vector3.up;

			Vector3 floorPoint = new Vector3(floor.Point.X, floor.Point.Y, floor.Point.Z);
			kinectDistanceFromFloor = ClosestDistanceFromFloor(kinectFloorNormal, floorPoint, RUISCoordinateSystem.kinectToUnityScale);
		}

		if(device == RUISDevice.Kinect_2)
		{
			Windows.Kinect.Vector4 kinect2FloorPlane = kinect2SourceManager.GetFlootClipPlane();
			kinectFloorNormal = new Vector3(kinect2FloorPlane.X, kinect2FloorPlane.Y, kinect2FloorPlane.Z);

			kinectFloorNormal.Normalize();

			if(kinectFloorNormal.sqrMagnitude < 0.1f)
				kinectFloorNormal = Vector3.up;

			kinectDistanceFromFloor = kinect2FloorPlane.W / Mathf.Sqrt(kinectFloorNormal.sqrMagnitude);
		}

		if(float.IsNaN(kinectDistanceFromFloor))
			kinectDistanceFromFloor = 0;
		kinectFloorRotator = Quaternion.FromToRotation(kinectFloorNormal, Vector3.up);
		kinectPitchRotation = Quaternion.Inverse(kinectFloorRotator);

		if(device == RUISDevice.Kinect_1 || device == RUISDevice.Kinect_2)
		{
			calibration.coordinateSystem.SetDistanceFromFloor(kinectDistanceFromFloor, device);
			calibration.coordinateSystem.SetFloorNormal(kinectFloorNormal, device);
			Debug.Log("Saved " + device + " floor normal " + kinectFloorNormal + " and floor distance (" + kinectDistanceFromFloor + ")");
		}

		if(device == RUISDevice.Custom_1)
		{
			calibration.coordinateSystem.SetDistanceFromFloor(device1DistanceFromFloor, RUISDevice.Custom_1);
			calibration.coordinateSystem.SetFloorNormal(device1FloorNormal, RUISDevice.Custom_1);
			Debug.Log("Saved " + device + " floor normal " + device1FloorNormal + " and floor distance (" + device1DistanceFromFloor + ")");
		}
		if(device == RUISDevice.Custom_2)
		{
			calibration.coordinateSystem.SetDistanceFromFloor(device2DistanceFromFloor, RUISDevice.Custom_2);
			calibration.coordinateSystem.SetFloorNormal(device2FloorNormal, RUISDevice.Custom_2);
			Debug.Log("Saved " + device + " floor normal " + device2FloorNormal + " and floor distance (" + device2DistanceFromFloor + ")");
		}
	}

	private float ClosestDistanceFromFloor(Vector3 floorNormal, Vector3 floorPoint, float scaling) 
	{
		float closestDistanceFromFloor = 0;

		floorNormal = floorNormal.normalized;
		Vector3 newFloorPosition = (new Vector3(floorPoint.x, floorPoint.y, floorPoint.z)) * scaling; 
		//Project the position of the kinect camera onto the floor
		//http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
		//http://en.wikipedia.org/wiki/Plane_(geometry)
		float d = floorNormal.x * newFloorPosition.x + floorNormal.y * newFloorPosition.y + floorNormal.z * newFloorPosition.z;
		Vector3 closestFloorPoint = new Vector3(floorNormal.x, floorNormal.y, floorNormal.z);
		closestFloorPoint = (closestFloorPoint * d) / closestFloorPoint.sqrMagnitude;
		//transform the point from Kinect's coordinate system rotation to Unity's rotation
		closestDistanceFromFloor = closestFloorPoint.magnitude;

		return closestDistanceFromFloor;
	}

	private void UpdateBodyData() 
	{
		bodyData = kinect2SourceManager.GetBodyData();

		if(bodyData != null) 
		{
			// Update tracking ID array
			for(int y = 0; y < trackingIDs.Length; y++) 
			{
				trackingIDs[y].isTracking = false; 
				trackingIDs[y].index = -1;
			}

			// Check tracking status and assing old indexes
			var arrayIndex = 0;
			foreach(var body in bodyData) 
			{
				if(body.IsTracked) 
				{
					for(int y = 0; y < trackingIDs.Length; y++) 
					{
						if(trackingIDs[y].trackingId == body.TrackingId) { // Body found in tracking IDs array
							trackingIDs[y].isTracking = true;			   // Reset as tracked
							trackingIDs[y].kinect2ArrayIndex = arrayIndex; // Set current kinect2 array index

							if(trackingIDtoIndex.ContainsKey(body.TrackingId)) { // If key added to trackingIDtoIndex array earlier...
								trackingIDs[y].index = trackingIDtoIndex[body.TrackingId]; // Set old index
							}
						}
					}

				}


				arrayIndex++;
			}

			// Add new bodies
			arrayIndex = 0;
			foreach(var body in bodyData) 
			{
				if(body.IsTracked) 
				{
					if(!trackingIDtoIndex.ContainsKey(body.TrackingId)) { // A new body
						for(int y = 0; y < trackingIDs.Length; y++) 
						{
							if(!trackingIDs[y].isTracking) {			// Find an array slot that does not have a tracked body
								trackingIDs[y].index = y;				// Set index to trackingIDs array index
								trackingIDs[y].trackingId = body.TrackingId;	
								trackingIDtoIndex[body.TrackingId] = y;		// Add tracking id to trackingIDtoIndex array
								trackingIDs[y].kinect2ArrayIndex = arrayIndex;
								trackingIDs[y].isTracking = true;
								break;
							}
						}	
					}
				}	
				arrayIndex++;	
			}
		}
	}

	public Valve.VR.VRControllerState_t controllerState;

	// TODO: Remove
	private void OnDeviceConnected(int deviceId, bool isConnected)
	{
		int openVrControllerIndex = 3;
//		var index = (int)args[0];

//		if (index == leftIndex)
//		{
//			if (left != null)
//				left.SetActive(false);
//
//			leftIndex = -1;
//		}
//
//		if (index == rightIndex)
//		{
//			if (right != null)
//				right.SetActive(false);
//
//			rightIndex = -1;
//		}
//
//		if (unassigned.Remove(index) && unassigned.Count == 0)
//			StopAllCoroutines();
//
//		var vr = SteamVR.instance;
//		if (vr.hmd.GetTrackedDeviceClass((uint)index) == ETrackedDeviceClass.Controller)
//		{
//			var connected = (bool)args[1];
//			if (connected)
//			{
//				unassigned.Add(index);
//				if (unassigned.Count == 1)
//					StartCoroutine(FindControllers());
//			}
//		}

	}

	~RUISCustomDeviceCalibrationProcess() // HACK TODO does this work in all cases, calibration finish/abort?
	{
//		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
		if(firstInputDevice == RUISDevice.OpenVR)
			SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
	}
}
