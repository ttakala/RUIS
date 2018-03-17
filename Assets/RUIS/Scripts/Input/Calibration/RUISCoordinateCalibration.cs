/*****************************************************************************

Content    :   Handles the calibration procedure between PS Move and Kinect
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSML;
using Kinect = Windows.Kinect;

public enum RUISDevice
{
	Kinect_1 = 0,
	Kinect_2 = 1,
	UnityXR = 2,
	OpenVR = 3,
	Custom_1 = int.MaxValue - 2,
	Custom_2 = int.MaxValue - 1,
	None = int.MaxValue
}

public enum RUISCalibrationPhase 
{
	Initial,
	Preparation,
	ReadyToCalibrate,
	Calibration,
	ShowResults,
	Invalid
}


public abstract class RUISCalibrationProcess 
{	
	RUISDevice inputDevice1, inputDevice2;

	public abstract string guiTextUpper { get; }
	public abstract string guiTextLower  { get; }
	
	abstract public RUISCalibrationPhase InitialPhase(float deltaTime);
	abstract public RUISCalibrationPhase PreparationPhase(float deltaTime);
	abstract public RUISCalibrationPhase ReadyToCalibratePhase(float deltaTime);
	abstract public RUISCalibrationPhase CalibrationPhase(float deltaTime);
	abstract public RUISCalibrationPhase ShowResultsPhase(float deltaTime);

	abstract public void PlaceSensorModels();

	public void updateDictionaries(Dictionary<string, Vector3> RUISCalibrationResultsInVector3, 
	                        Dictionary<string, Quaternion> RUISCalibrationResultsInQuaternion,
	                        Dictionary<string, Matrix4x4> RUISCalibrationResultsIn4x4Matrix,
	                        Vector3 translate, Quaternion rotation, Matrix4x4 pairwiseTransform,
	                        RUISDevice device1, RUISDevice device2)
	{
		RUISCalibrationResultsInVector3[device1.ToString() + "-" + device2.ToString()] = translate;
		RUISCalibrationResultsIn4x4Matrix[device1.ToString() + "-" + device2.ToString()] = pairwiseTransform;
		RUISCalibrationResultsInQuaternion[device1.ToString() + "-" + device2.ToString()] = rotation;		
		
		// Inverses
		RUISCalibrationResultsInVector3[device2.ToString() + "-" + device1.ToString()] = -translate;
		RUISCalibrationResultsIn4x4Matrix[device2.ToString() + "-" + device1.ToString()] = pairwiseTransform.inverse;
		RUISCalibrationResultsInQuaternion[device2.ToString() + "-" + device1.ToString()] = Quaternion.Inverse(rotation);		
	}
}

public class RUISCalibrationProcessSettings
{	// HACK to convey calibration settings to calibration.scene when entering it from another scene via RUISMenu. Consider using existing XML instead
	static public bool isCalibrating = false;
	static public string devicePair;
	static public int previousSceneId;
	static public bool enablePSMove;
	static public bool enableKinect;
	static public bool enableKinect2;
//	static public bool jumpGestureEnabled;
	static public bool enableRazerHydra;
	static public string PSMoveIP;
	static public int PSMovePort;
	static public float yawOffset;
	static public Vector3 positionOffset;
	static public RUISDevice originalMasterCoordinateSystem;
	static public bool enableCustomDevice1;
	static public bool enableCustomDevice2;
	static public string customDevice1Name;
	static public string customDevice2Name;
	static public RUISCoordinateSystem.DeviceCoordinateConversion customDevice1Conversion;
	static public RUISCoordinateSystem.DeviceCoordinateConversion customDevice2Conversion;

	public RUISDevice firstDevice;
	public RUISDevice secondDevice;
	public int numberOfSamplesToTake;
	public int numberOfSamplesPerSecond;
	public float sampleMinDistance;
	public GameObject device1SamplePrefab;
	public GameObject device2SamplePrefab;
	public GameObject floorPlane;
	public GameObject calibrationPhaseObjects; 
	public GameObject calibrationResultPhaseObjects;
	public string xmlFilename;
	public GameObject deviceModelObjects, depthViewObjects, iconObjects;
}

public class RUISCoordinateCalibration : MonoBehaviour 
{    
	private GUIText upperText, lowerText;
	
	public GameObject device1SamplePrefab;
	public GameObject device2SamplePrefab;

	private bool hmdCalibration = false;

	public RUISDevice firstDevice;
	public RUISDevice secondDevice;
	public int numberOfSamplesToTake = 50;
    public int samplesPerSecond = 1;
	public float sampleMinDistance = 0.3f;

	public Transform customDevice1Tracker;
	public Transform customDevice2Tracker;
	public Transform customDevice1FloorPoint;
	public Transform customDevice2FloorPoint;

	public GameObject calibrationSpherePrefab, calibrationCubePrefab, customDevice1Object, customDevice2Object, 
					floorPlane, calibrationPhaseObjects, calibrationResultPhaseObjects, depthViews, 
					deviceModels, icons;
	public string xmlFilename = "calibration.xml";

	public GUIText firstIconText;
	public GUIText secondIconText;
	public GUITexture firstIcon;
	public GUITexture secondIcon;

	public List<Texture2D> iconTextures = new List<Texture2D>();

	public RUISSkeletonController skeletonController;

	public RUISCalibrationProcess calibrationProcess;

	private Vector3 floorNormal;
	RUISCalibrationPhase currentPhase, nextPhase, lastPhase;
	
	RUISCalibrationProcessSettings calibrationProcessSettings;

	public RUISCoordinateSystem coordinateSystem;

	bool menuIsVisible = false;
	
	void Awake ()
	{
		Cursor.visible = true; // Incase cursor was hidden in previous scene
		
		// Check if calibration settings were chosen on previous scene
		if(RUISCalibrationProcessSettings.devicePair != null) 
		{
			numberOfSamplesToTake = 50;
			samplesPerSecond = 5;

			switch(RUISCalibrationProcessSettings.devicePair)
			{
				case "Kinect 1 - Kinect2":
					firstDevice  = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.Kinect_2;
					break;
				case "Kinect 1 - OpenVR (controller)": // *** HACK TODO hacky
					firstDevice  = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.OpenVR;
					break;
				case "Kinect 2 - OpenVR (controller)":
					firstDevice  = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.OpenVR;
					break;
				case "Kinect 1 - OpenVR (HMD)":
					firstDevice  = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.OpenVR;
					hmdCalibration = true;
					break;
				case "Kinect 2 - OpenVR (HMD)":
					firstDevice  = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.OpenVR;
					hmdCalibration = true;
					break;
				case "Kinect 1 - UnityXR (HMD)":
					firstDevice  = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.UnityXR;
					hmdCalibration = true;
					break;
				case "Kinect 2 - UnityXR (HMD)":
					firstDevice  = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.UnityXR;
					hmdCalibration = true;
					break;
				case "Kinect 1 floor data":
					firstDevice  = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.Kinect_1;
					break;
				case "Kinect 2 floor data":
					firstDevice  = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.Kinect_2;
					break;
				case "Custom 1 - Custom 2":
					firstDevice  = RUISDevice.Custom_1;
					secondDevice = RUISDevice.Custom_2;
					break;
				case "Custom 1 - OpenVR (controller)":
					firstDevice  = RUISDevice.OpenVR;
					secondDevice = RUISDevice.Custom_1;
					break;
				case "Custom 2 - OpenVR (controller)":
					firstDevice  = RUISDevice.OpenVR;
					secondDevice = RUISDevice.Custom_2;
					break;
				case "Custom 1 - Kinect 1":
					firstDevice  = RUISDevice.Custom_1;
					secondDevice = RUISDevice.Kinect_1;
					break;
				case "Custom 2 - Kinect 1":
					firstDevice  = RUISDevice.Custom_2;
					secondDevice = RUISDevice.Kinect_1;
					break;
				case "Custom 1 - Kinect 2":
					firstDevice  = RUISDevice.Custom_1;
					secondDevice = RUISDevice.Kinect_2;
					break;
				case "Custom 2 - Kinect 2":
					firstDevice  = RUISDevice.Custom_2;
					secondDevice = RUISDevice.Kinect_2;
					break;
//				case "Custom 1 - OpenVR (HMD)":
//					firstDevice  = RUISDevice.OpenVR;
//					secondDevice = RUISDevice.Custom_1;
//					hmdCalibration = true;
//					break;
//				case "Custom 2 - OpenVR (HMD)":
//					firstDevice  = RUISDevice.OpenVR;
//					secondDevice = RUISDevice.Custom_2;
//					hmdCalibration = true;
//				break;
				default:
					firstDevice  = RUISDevice.None;
					secondDevice = RUISDevice.None;
				break;
			}
		}
	
		// Init scene objects
		this.floorPlane = GameObject.Find ("Floor");
		this.calibrationPhaseObjects = GameObject.Find("CalibrationPhase");
		this.calibrationResultPhaseObjects = GameObject.Find("ResultPhase");
		this.depthViews = GameObject.Find ("Depth views");
		this.deviceModels = GameObject.Find ("Device models");
		this.icons = GameObject.Find ("Icons");
		
		upperText = GameObject.Find ("Upper Text").GetComponent<GUIText>();
		lowerText = GameObject.Find ("Lower Text").GetComponent<GUIText>();

		coordinateSystem  = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;

		// Pass variables and objects to calibrationProcess
		calibrationProcessSettings = new RUISCalibrationProcessSettings();
		calibrationProcessSettings.xmlFilename = xmlFilename;
		calibrationProcessSettings.numberOfSamplesToTake = numberOfSamplesToTake;
		calibrationProcessSettings.numberOfSamplesPerSecond = samplesPerSecond;
		calibrationProcessSettings.sampleMinDistance = this.sampleMinDistance;
		calibrationProcessSettings.firstDevice  = firstDevice;
		calibrationProcessSettings.secondDevice = secondDevice;
		calibrationProcessSettings.device1SamplePrefab = this.device1SamplePrefab;
		calibrationProcessSettings.device2SamplePrefab = this.device2SamplePrefab;
		calibrationProcessSettings.floorPlane = this.floorPlane;
		calibrationProcessSettings.calibrationPhaseObjects = this.calibrationPhaseObjects;
		calibrationProcessSettings.calibrationResultPhaseObjects = this.calibrationResultPhaseObjects;
		calibrationProcessSettings.deviceModelObjects = deviceModels;
		calibrationProcessSettings.depthViewObjects = depthViews;
		calibrationProcessSettings.iconObjects = icons;

		if(!coordinateSystem) 
		{
			Debug.LogError("Component " + typeof(RUISCoordinateSystem) + " not found in the calibration scene, cannot continue!");
			upperText.text = "Component " + typeof(RUISCoordinateSystem) + " not found in\n the calibration scene, cannot continue!";
			lowerText.text = upperText.text;
			this.enabled = false;
		}
	}

    void Start()
    {
		if(		( firstDevice == RUISDevice.Custom_1 && secondDevice == RUISDevice.OpenVR)
			||	(secondDevice == RUISDevice.Custom_1 &&  firstDevice == RUISDevice.OpenVR))
		{
			skeletonController.customConversionType = RUISSkeletonController.CustomConversionType.Custom_1;
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
//			if(hmdCalibration) // TODO: add RUISCustomDeviceToOpenVrHmdCalibrationProcess
				calibrationProcess = new RUISCustomDeviceCalibrationProcess(calibrationProcessSettings);
//			else
//				calibrationProcess = new RUISCustomDeviceToOpenVrHmdCalibrationProcess(calibrationProcessSettings);
		}
		else if(	( firstDevice == RUISDevice.Custom_2 && secondDevice == RUISDevice.OpenVR)
				 ||	(secondDevice == RUISDevice.Custom_2 &&  firstDevice == RUISDevice.OpenVR))
		{
			skeletonController.customConversionType = RUISSkeletonController.CustomConversionType.Custom_2;
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
			//			if(hmdCalibration) // TODO: add RUISCustomDeviceToOpenVrHmdCalibrationProcess
			calibrationProcess = new RUISCustomDeviceCalibrationProcess(calibrationProcessSettings);
			//			else
			//				calibrationProcess = new RUISCustomDeviceToOpenVrHmdCalibrationProcess(calibrationProcessSettings);
		}
		else if(	( firstDevice == RUISDevice.Custom_1 && secondDevice == RUISDevice.Custom_2)
				 || (secondDevice == RUISDevice.Custom_1 &&  firstDevice == RUISDevice.Custom_2))
		{
			skeletonController.customConversionType = RUISSkeletonController.CustomConversionType.Custom_1;
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
			calibrationProcess = new RUISCustomDeviceCalibrationProcess(calibrationProcessSettings);
		}
		else if(	( firstDevice == RUISDevice.Custom_1 && (secondDevice == RUISDevice.Kinect_1 || secondDevice == RUISDevice.Kinect_2))
				 || (secondDevice == RUISDevice.Custom_1 && ( firstDevice == RUISDevice.Kinect_1 ||  firstDevice == RUISDevice.Kinect_2)))
		{
			skeletonController.customConversionType = RUISSkeletonController.CustomConversionType.Custom_1;
			if(firstDevice == RUISDevice.Kinect_2 || secondDevice == RUISDevice.Kinect_2)
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			else
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			calibrationProcess = new RUISCustomDeviceCalibrationProcess(calibrationProcessSettings);
		}
		else if(	( firstDevice == RUISDevice.Custom_2 && (secondDevice == RUISDevice.Kinect_1 || secondDevice == RUISDevice.Kinect_2))
				 || (secondDevice == RUISDevice.Custom_2 && ( firstDevice == RUISDevice.Kinect_1 ||  firstDevice == RUISDevice.Kinect_2)))
		{
			skeletonController.customConversionType = RUISSkeletonController.CustomConversionType.Custom_2;
			if(firstDevice == RUISDevice.Kinect_2 || secondDevice == RUISDevice.Kinect_2)
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			else
				skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			calibrationProcess = new RUISCustomDeviceCalibrationProcess(calibrationProcessSettings);
		}
		else if(	( firstDevice == RUISDevice.Kinect_1 && secondDevice == RUISDevice.Kinect_2)
		   		 ||	(secondDevice == RUISDevice.Kinect_1 &&  firstDevice == RUISDevice.Kinect_2)) 
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinect2ToKinectCalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice  == RUISDevice.Kinect_2 && secondDevice == RUISDevice.OpenVR)
				||	(secondDevice == RUISDevice.Kinect_2 &&  firstDevice == RUISDevice.OpenVR )) 
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.OpenVR;
			if(hmdCalibration)
				calibrationProcess = new RUISKinect2ToHmdCalibrationProcess(calibrationProcessSettings);
			else
				calibrationProcess = new RUISKinectsToOpenVrControllerCalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice  == RUISDevice.Kinect_1 && secondDevice == RUISDevice.OpenVR)
				||	(secondDevice == RUISDevice.Kinect_1 &&  firstDevice == RUISDevice.OpenVR))
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.OpenVR;
			if(hmdCalibration)
				calibrationProcess = new RUISKinectToHmdCalibrationProcess(calibrationProcessSettings);
			else
				calibrationProcess = new RUISKinectsToOpenVrControllerCalibrationProcess(calibrationProcessSettings);
		}
		else if(hmdCalibration &&	(	( firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.UnityXR)
									 ||	(secondDevice == RUISDevice.Kinect_2  &&  firstDevice == RUISDevice.UnityXR)))
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.UnityXR;
			calibrationProcess = new RUISKinect2ToHmdCalibrationProcess(calibrationProcessSettings);
		}
		else if(hmdCalibration && 	(   ( firstDevice == RUISDevice.Kinect_1 && secondDevice == RUISDevice.UnityXR)
									 || (secondDevice == RUISDevice.Kinect_1 &&  firstDevice == RUISDevice.UnityXR))) 
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.UnityXR;
			calibrationProcess = new RUISKinectToHmdCalibrationProcess(calibrationProcessSettings);
		}
//		else if(hmdCalibration &&  (	(firstDevice == RUISDevice.UnityXR  && secondDevice == RUISDevice.OpenVR)
//									||	(secondDevice == RUISDevice.UnityXR && firstDevice == RUISDevice.OpenVR )))
//		{
//			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
//			coordinateSystem.rootDevice = RUISDevice.OpenVR;
//			calibrationProcess = new RUISPSMoveToOpenVrHmdCalibrationProcess(calibrationProcessSettings);
//		}
		else if(firstDevice == RUISDevice.Kinect_1  && secondDevice == RUISDevice.Kinect_1 ) 
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinectFloorDataCalibrationProcess(calibrationProcessSettings);
		}
		else if(firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.Kinect_2 ) 
		{
			skeletonController.BodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
			calibrationProcess = new RUISKinect2FloorDataCalibrationProcess(calibrationProcessSettings);
		}
		else
		{
			calibrationProcess = null;
		}	
    
    	if(calibrationProcess == null)
		{
			upperText.text = "";
			lowerText.text = "Selected calibration device combination\n(" + RUISCalibrationProcessSettings.devicePair + ") not yet supported.";
			
			foreach (Transform child in this.deviceModels.transform)
			{
				child.gameObject.SetActive(false);
			}
			
			foreach (Transform child in this.depthViews.transform)
			{
				child.gameObject.SetActive(false);
			}
			
			foreach (Transform child in this.icons.transform)
			{
				child.gameObject.SetActive(false);
			}
			
			this.calibrationResultPhaseObjects.SetActive(false);
			currentPhase = RUISCalibrationPhase.Invalid;
		}
		else 
		{
			currentPhase = RUISCalibrationPhase.Initial;
		}
		string devicePairName = firstDevice.ToString() + "-" + secondDevice.ToString();
		string devicePairName2 = secondDevice.ToString() + "-" + firstDevice.ToString();
		
		coordinateSystem.RUISCalibrationResultsIn4x4Matrix[devicePairName] = Matrix4x4.identity;
		coordinateSystem.RUISCalibrationResultsDistanceFromFloor[firstDevice] = 0.0f;
		coordinateSystem.RUISCalibrationResultsFloorPitchRotation[firstDevice] = Quaternion.identity;
		
		coordinateSystem.RUISCalibrationResultsIn4x4Matrix[devicePairName2] = Matrix4x4.identity;
		coordinateSystem.RUISCalibrationResultsDistanceFromFloor[secondDevice] = 0.0f;
		coordinateSystem.RUISCalibrationResultsFloorPitchRotation[secondDevice] = Quaternion.identity;
    }

	void LateUpdate ()
	{
		if(Input.GetKeyDown(KeyCode.Escape))
			menuIsVisible = !menuIsVisible;

		if(calibrationProcess != null) {
			upperText.text = calibrationProcess.guiTextUpper;
			lowerText.text = calibrationProcess.guiTextLower;	
		}

		if(currentPhase != RUISCalibrationPhase.Invalid)
		{
			switch(currentPhase)
			{
				case RUISCalibrationPhase.Initial: 
					currentPhase = calibrationProcess.InitialPhase(Time.deltaTime);		
				break;
					
				case RUISCalibrationPhase.Preparation: 
					currentPhase = calibrationProcess.PreparationPhase(Time.deltaTime);		
				break;
					
				case RUISCalibrationPhase.ReadyToCalibrate: 
					currentPhase = calibrationProcess.ReadyToCalibratePhase(Time.deltaTime);		
				break;
					
				case RUISCalibrationPhase.Calibration: 
					currentPhase = calibrationProcess.CalibrationPhase(Time.deltaTime);		
				break;
				
				case RUISCalibrationPhase.ShowResults: 
					currentPhase = calibrationProcess.ShowResultsPhase(Time.deltaTime);	

					if(coordinateSystem)
					{
						if(!coordinateSystem.applyToRootCoordinates) // Set values only once if applyToRootCoordinates == false
						{
							coordinateSystem.yawOffset      = RUISCalibrationProcessSettings.yawOffset;
							coordinateSystem.positionOffset = RUISCalibrationProcessSettings.positionOffset;
						}
						coordinateSystem.applyToRootCoordinates = true;
					}

					calibrationProcess.PlaceSensorModels();

//					if(ruisNGUIMenu != null) {
//						ruisNGUIMenu.calibrationReady = true;	
//						ruisNGUIMenu.menuIsVisible = true;
//					}
				break;
			}	
		}
	}

	void OnGUI()
	{
		if(currentPhase == RUISCalibrationPhase.ShowResults || menuIsVisible) 
		{
			GUILayout.Window(0, new Rect(50, 50, 150, 200), DrawWindow, "RUIS");
		}
	}


	void DrawWindow(int windowId)
	{	
		if(currentPhase == RUISCalibrationPhase.ShowResults) 
		{
			GUILayout.Label("Calibration finished.");
			GUILayout.Space(20);
			if(GUILayout.Button("Exit calibration"))
			{
				Destroy(this.gameObject);
				Application.LoadLevel(RUISCalibrationProcessSettings.previousSceneId);
			}
		}
		else 
		{
			GUILayout.Label("Calibration not finished yet.");
			GUILayout.Space(20);
			if(GUILayout.Button("Abort calibration"))
			{
				Destroy(this.gameObject);
				Application.LoadLevel(RUISCalibrationProcessSettings.previousSceneId);
			}
		}
	}
}




