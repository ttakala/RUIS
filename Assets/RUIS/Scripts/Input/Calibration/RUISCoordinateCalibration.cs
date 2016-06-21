/*****************************************************************************

Content    :   Handles the calibration procedure between PS Move and Kinect
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSML;
using Kinect = Windows.Kinect;

public enum RUISDevice
{
	Kinect_1,
	Kinect_2,
	PS_Move,
	Oculus_DK2,
	Vive,
	Null
}
public enum RUISCalibrationPhase {
	Initial,
	Preparation,
	ReadyToCalibrate,
	Calibration,
	ShowResults,
	Invalid
}


public abstract class RUISCalibrationProcess {
	
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
{
	static public bool isCalibrating = false;
	static public string devicePair;
	static public int previousSceneId;
	static public bool enablePSMove;
	static public bool enableKinect;
	static public bool enableKinect2;
	static public bool jumpGestureEnabled;
	static public bool enableRazerHydra;
	static public string PSMoveIP;
	static public int PSMovePort;
	static public float yawOffset;
	static public Vector3 positionOffset;
	static public RUISDevice originalMasterCoordinateSystem;

	public int numberOfSamplesToTake;
	public int numberOfSamplesPerSecond;
	public GameObject calibrationSpherePrefab;
	public GameObject calibrationCubePrefab;
	public GameObject floorPlane;
	public GameObject calibrationPhaseObjects; 
	public GameObject calibrationResultPhaseObjects;
	public string xmlFilename;
	public GameObject deviceModelObjects, depthViewObjects, iconObjects;
}

public class RUISCoordinateCalibration : MonoBehaviour {
    
	private GUIText upperText, lowerText;
	
	public GameObject sampleCube;
	public GameObject sampleSphere;
	
	public RUISDevice firstDevice;
	public RUISDevice secondDevice;
	public int numberOfSamplesToTake = 50;
    public int samplesPerSecond = 1;
 	
	public GameObject calibrationSpherePrefab, calibrationCubePrefab, floorPlane, 
					calibrationPhaseObjects, calibrationResultPhaseObjects, depthViews, 
					deviceModels, icons;
	public string xmlFilename = "calibration.xml";
	
	private Vector3 floorNormal;
	public RUISCalibrationProcess calibrationProcess;
	RUISCalibrationPhase currentPhase, nextPhase, lastPhase;
	
	RUISCalibrationProcessSettings calibrationProcessSettings;
	
	RUISSkeletonController skeletonController;
	RUISCoordinateSystem coordinateSystem;


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
				case "Kinect - Kinect2":
					firstDevice = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.Kinect_2;
				break;
				case "Kinect - PSMove":
					firstDevice = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.PS_Move;
				break;
				case "Kinect 2 - PSMove":
					firstDevice = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.PS_Move;
				break;
				case "Kinect 2 - Oculus DK2":
					firstDevice = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.Oculus_DK2;
				break;
				case "Kinect - Oculus DK2":
					firstDevice = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.Oculus_DK2;
				break;
				case "Kinect 2 - Vive":
					firstDevice = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.Vive;
				break;
				case "Vive - Kinect 2":
					firstDevice = RUISDevice.Vive;
					secondDevice = RUISDevice.Kinect_2;
				break;
				case "PSMove - Oculus DK2":
					firstDevice = RUISDevice.PS_Move;
					secondDevice = RUISDevice.Oculus_DK2;
				break;
				case "Kinect floor data":
					firstDevice = RUISDevice.Kinect_1;
					secondDevice = RUISDevice.Kinect_1;
				break;
				case "Kinect 2 floor data":
					firstDevice = RUISDevice.Kinect_2;
					secondDevice = RUISDevice.Kinect_2;
				break;
				default:
					firstDevice = RUISDevice.Null;
					secondDevice = RUISDevice.Null;
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
		
		skeletonController = FindObjectOfType(typeof(RUISSkeletonController)) as RUISSkeletonController;
		coordinateSystem  = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;

		// Pass variables and objects to calibrationProcess
		calibrationProcessSettings = new RUISCalibrationProcessSettings();
		calibrationProcessSettings.xmlFilename = xmlFilename;
		calibrationProcessSettings.numberOfSamplesToTake = numberOfSamplesToTake;
		calibrationProcessSettings.numberOfSamplesPerSecond = samplesPerSecond;
		calibrationProcessSettings.calibrationCubePrefab = this.sampleCube;
		calibrationProcessSettings.calibrationSpherePrefab = this.sampleSphere;
		calibrationProcessSettings.calibrationPhaseObjects = this.calibrationPhaseObjects;
		calibrationProcessSettings.calibrationResultPhaseObjects = this.calibrationResultPhaseObjects;
		calibrationProcessSettings.deviceModelObjects = deviceModels;
		calibrationProcessSettings.depthViewObjects = depthViews;
		calibrationProcessSettings.iconObjects = icons;
	}

    void Start()
    {
		if(			(firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.Oculus_DK2)
		  		 ||	(secondDevice == RUISDevice.Kinect_2 && firstDevice == RUISDevice.Oculus_DK2 )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
			calibrationProcess = new RUISKinect2ToOculusDK2CalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice == RUISDevice.Kinect_1  && secondDevice == RUISDevice.Kinect_2)
		   		 ||	(secondDevice == RUISDevice.Kinect_1 && firstDevice == RUISDevice.Kinect_2 )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinect2ToKinectCalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice == RUISDevice.Kinect_1  && secondDevice == RUISDevice.PS_Move)
		  		 ||	(secondDevice == RUISDevice.Kinect_1 && firstDevice == RUISDevice.PS_Move )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinectToPSMoveCalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.PS_Move)
		       	 ||	(secondDevice == RUISDevice.Kinect_2 && firstDevice == RUISDevice.PS_Move )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
			calibrationProcess = new RUISKinect2ToPSMoveCalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice == RUISDevice.PS_Move  && secondDevice == RUISDevice.Oculus_DK2)
		        ||	(secondDevice == RUISDevice.PS_Move && firstDevice == RUISDevice.Oculus_DK2 )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.customSensorID;
			coordinateSystem.rootDevice = RUISDevice.Oculus_DK2;
			calibrationProcess = new RUISPSMoveToOculusDK2CalibrationProcess(calibrationProcessSettings);
		}
		else if(	(firstDevice == RUISDevice.Kinect_1  && secondDevice == RUISDevice.Oculus_DK2)
		   		||	(secondDevice == RUISDevice.Kinect_1 && firstDevice == RUISDevice.Oculus_DK2 )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinectToOculusDK2CalibrationProcess(calibrationProcessSettings);
		}
		else if(firstDevice == RUISDevice.Kinect_1  && secondDevice == RUISDevice.Kinect_1 ) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect1SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_1;
			calibrationProcess = new RUISKinectFloorDataCalibrationProcess(calibrationProcessSettings);
		}
		else if(firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.Kinect_2 ) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
			calibrationProcess = new RUISKinect2FloorDataCalibrationProcess(calibrationProcessSettings);
		}
		else if((firstDevice == RUISDevice.Kinect_2  && secondDevice == RUISDevice.Vive)
			||	(secondDevice == RUISDevice.Kinect_2 && firstDevice == RUISDevice.Vive )) 
		{
			skeletonController.bodyTrackingDeviceID = RUISSkeletonManager.kinect2SensorID;
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
			calibrationProcess = new RUISKinect2ToViveCalibrationProcess(calibrationProcessSettings);
		}
		
		else {
			calibrationProcess = null;
		}	
    
    	if(calibrationProcess == null) {
			upperText.text = "";
			lowerText.text = "Selected calibration device combination\n not yet supported.";
			
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
		else {
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

	void Update ()
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




