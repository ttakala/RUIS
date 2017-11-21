/*****************************************************************************

Content    :   Handles the calibration procedure between Kinect 2 and OpenVR
Authors    :   Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSML;
using Kinect = Windows.Kinect;
using Valve.VR;

public class RUISKinect2ToOpenVrControllerCalibrationProcess : RUISCalibrationProcess 
{
	public string getUpperText() {
		return this.guiTextUpperLocal;
	}
	
	public string getLowerText() {
		return this.guiTextLowerLocal;
	}
	
	// Abstract class variables
	private RUISDevice inputDevice1, inputDevice2;
	public string guiTextUpperLocal, guiTextLowerLocal;
	public bool useScreen1, useScreen2;
	
	public override string guiTextUpper { get{return getUpperText();} }
	public override string guiTextLower { get{return getLowerText();} }

	private string openVRDeviceName = "OpenVR";

	bool showMovementAlert = false;
	float lastMovementAlertTime = 0;

	// Custom variables
	private List<Vector3> samples_Kinect2, samples_Vive;
	private int numberOfSamplesTaken, numberOfSamplesToTake, numberOfSamplesPerSecond;
	private float timeSinceLastSample, timeBetweenSamples, timeSinceScriptStart = 0;
	public RUISCoordinateSystem coordinateSystem;
	public RUISInputManager inputManager;
	private bool viveChecked = false, kinect2Checked = false, calibrationFinnished = false;
	List<GameObject> calibrationSpheres;
	private GameObject calibrationPhaseObjects, calibrationResultPhaseObjects, viveCameraObject, 
	kinect2ModelObject, floorPlane, calibrationSphere, calibrationCube, depthView,
	deviceModelObjects, depthViewObjects, iconObjects;
	
	private Vector3 lastKinect2Sample, lastViveSample;
	private string xmlFilename;
	
	private Matrix4x4 rotationMatrix, transformMatrix;
	
	Kinect2SourceManager kinect2SourceManager;
	Kinect.Body[] bodyData; 
	
	private trackedBody[] trackingIDs = null; // Defined in RUISKinect2DepthView
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();
	private int kinectTrackingIndex;
	private ulong kinectTrackingID;
	
	Quaternion kinect2PitchRotation = Quaternion.identity;
	float kinect2DistanceFromFloor = 0;
	Vector3 kinect2FloorNormal = Vector3.up;
//	RUISOVRManager ruisOvrManager;

	bool device1Error, device2Error;

	RUISOpenVrPrefabContainer vivePrefabContainer;
	SteamVR_TrackedObject[] trackedOpenVRObjects;
	int viveControllerIndex = 0;
	Transform viveControllerTransform;
	Vector3 translateAtTuneStart = Vector3.zero;
	Vector3 controllerPositionAtTuneStart = Vector3.zero;

	RUISCoordinateCalibration calibration;
	RUISCalibrationProcessSettings calibrationSettings;

	public RUISKinect2ToOpenVrControllerCalibrationProcess(RUISCalibrationProcessSettings calibrationSettings) 
	{
		calibration = MonoBehaviour.FindObjectOfType<RUISCoordinateCalibration>();

		if(!calibration) 
		{
			Debug.LogError ("Component " + typeof(RUISCoordinateCalibration) + " not found in the calibration scene, cannot continue!");
			return;
		}

		this.inputDevice1 = RUISDevice.OpenVR;
		this.inputDevice2 = RUISDevice.Kinect_2;
		
		this.numberOfSamplesToTake = calibrationSettings.numberOfSamplesToTake;
		this.numberOfSamplesPerSecond = calibrationSettings.numberOfSamplesPerSecond;

//		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
		SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);

		trackingIDs = new trackedBody[6]; 
		for(int y = 0; y < trackingIDs.Length; y++) {
			trackingIDs[y] = new trackedBody(-1, false, 1);
		}
		
		inputManager = MonoBehaviour.FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
		kinect2SourceManager = MonoBehaviour.FindObjectOfType(typeof(Kinect2SourceManager)) as Kinect2SourceManager;

		if(RUISCalibrationProcessSettings.originalMasterCoordinateSystem == RUISDevice.Kinect_2)
			coordinateSystem.rootDevice = RUISDevice.Kinect_2;
		else
			coordinateSystem.rootDevice = RUISDevice.OpenVR;

		vivePrefabContainer = Component.FindObjectOfType<RUISOpenVrPrefabContainer>();
		if(vivePrefabContainer)
		{
			if(vivePrefabContainer.openVrCameraRigPrefab)
			{
				vivePrefabContainer.instantiatedOpenVrCameraRig = GameObject.Instantiate(vivePrefabContainer.openVrCameraRigPrefab);
				Camera[] rigCams = vivePrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<Camera>();
				if(rigCams != null)
				{
					foreach(Camera cam in rigCams)
					{
						// *** TODO HACK Ugly fix, not 100% sure if in the future all the perspective cameras in the ViveCameraRig work well with below code
						if(!cam.orthographic)
							cam.nearClipPlane = 0.15f;
					}
				}
			}
			else
				Debug.LogError("The viveCameraRigPrefab field in " + typeof(RUISOpenVrPrefabContainer) + " is null, and calibration will not work!");
		}
		else
		{
			Debug.LogError("Could not locate " + typeof(RUISOpenVrPrefabContainer) + " component in this scene, and calibration will not work!");
		}

		this.timeSinceScriptStart = 0;
		this.timeBetweenSamples = 1 / (float)numberOfSamplesPerSecond;
		
		// Limit sample rate
		if(this.timeBetweenSamples < 0.1f) {
			this.timeBetweenSamples = 0.1f;
		}
		
		calibrationSpheres = new List<GameObject>();
		
		samples_Kinect2 = new List<Vector3>();
		samples_Vive = new List<Vector3>();

		this.calibrationSettings = calibrationSettings;
		this.calibrationCube = calibrationSettings.device1SamplePrefab;
		this.calibrationSphere = calibrationSettings.device2SamplePrefab;
		this.calibrationPhaseObjects = calibrationSettings.calibrationPhaseObjects;
		this.calibrationResultPhaseObjects = calibrationSettings.calibrationResultPhaseObjects;
		
		this.deviceModelObjects = calibrationSettings.deviceModelObjects;
		this.depthViewObjects = calibrationSettings.depthViewObjects;
		this.iconObjects = calibrationSettings.iconObjects;
		
		if(GameObject.Find ("PSMoveWand") != null)
			GameObject.Find ("PSMoveWand").SetActive(false);
		if(GameObject.Find ("HmdModel")) // "Was OculusRift"
			GameObject.Find ("HmdModel").SetActive(false);
		
		// Models
		this.viveCameraObject = GameObject.Find ("HmdCamera"); // Was "OculusDK2Camera"
		this.kinect2ModelObject = GameObject.Find ("Kinect2Camera");

		// Depth view
		this.depthView = GameObject.Find ("Kinect2DepthView");
		
		this.floorPlane = GameObject.Find ("Floor");
		
		foreach (Transform child in this.deviceModelObjects.transform)
			child.gameObject.SetActive(false);
		foreach (Transform child in this.depthViewObjects.transform)
			child.gameObject.SetActive(false);
		foreach (Transform child in this.iconObjects.transform)
			child.gameObject.SetActive(false);
		
		if(calibration.iconTextures != null)
		{
			foreach(Texture2D iconTexture in calibration.iconTextures)
			{
				if(iconTexture.name == "oculus_camera_icon") // TODO replace with OpenVR controller icon
				{
					if(calibration.firstIcon)
					{
						calibration.firstIcon.gameObject.SetActive(true);
						calibration.firstIcon.texture = iconTexture;
					}
				}
				if(iconTexture.name == "kinect2_icon")
				{
					if(calibration.secondIcon)
					{
						calibration.secondIcon.gameObject.SetActive(true);
						calibration.secondIcon.texture = iconTexture;
					}
				}
			}
		}

		if(calibration.firstIconText)
		{
			calibration.firstIconText.gameObject.SetActive(true);
			calibration.firstIconText.text = RUISDevice.OpenVR.ToString();
		}
		if(calibration.secondIconText)
		{
			calibration.secondIconText.gameObject.SetActive(true);
			calibration.secondIconText.text = RUISDevice.Kinect_2.ToString();
		}

		if(this.viveCameraObject)
			this.viveCameraObject.SetActive(false);
		if(this.kinect2ModelObject)
			this.kinect2ModelObject.SetActive(true);
		if(this.calibrationPhaseObjects)
			this.calibrationPhaseObjects.SetActive(true);
		if(this.calibrationResultPhaseObjects)
			this.calibrationResultPhaseObjects.SetActive(false);
		if(this.depthView)
			this.depthView.SetActive(true);
		this.xmlFilename = calibrationSettings.xmlFilename;
	}
	
	
	public override RUISCalibrationPhase InitialPhase(float deltaTime) 
	{
		timeSinceScriptStart += deltaTime;
		
		if(timeSinceScriptStart < 2)
		{
			this.guiTextLowerLocal = "Calibration of Kinect 2 and OpenVR Tracking\n\n Starting up...";
			return RUISCalibrationPhase.Initial;
		}

		// Execute once only
		if(!viveChecked)
		{
			viveChecked = true;

			try
			{
				if(SteamVR.instance != null)
					openVRDeviceName = SteamVR.instance.hmd_ModelNumber;
				if(openVRDeviceName.Contains("Vive"))
					openVRDeviceName = "Vive";
				else if(openVRDeviceName.Contains("Oculus"))
					openVRDeviceName = "Oculus Touch";
				else
					openVRDeviceName = "OpenVR";
				
				if(!Valve.VR.OpenVR.IsHmdPresent()) // *** TODO HACK Valve API
				{
					this.guiTextLowerLocal = "Head-mounted display is not detected!\nYou might not be able to access the " + openVRDeviceName + " controllers.";
					Debug.LogError(  "Head-mounted display is not detected! This could be an indication of a bigger problem and you might not be able to access the "
								   + openVRDeviceName + " controllers.");
				}
			} catch
			{
				this.guiTextLowerLocal = "Failed to access " + openVRDeviceName + ". \n\n Error: OpenVR not found! Is SteamVR installed?";
				return RUISCalibrationPhase.Invalid;
			}
		}

		if(timeSinceScriptStart < 4)
		{
			this.guiTextLowerLocal = "Detected " + openVRDeviceName + ".";
			return RUISCalibrationPhase.Initial;
		}
		
		if(timeSinceScriptStart < 5)
		{
			this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Please wait...";
			return RUISCalibrationPhase.Initial;
		}

		// Execute once only
		if(!kinect2Checked)
		{
			kinect2Checked = true;	
			if (kinect2SourceManager.GetSensor() == null || !kinect2SourceManager.GetSensor().IsOpen || !kinect2SourceManager.GetSensor().IsAvailable)
			{
				this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Error: Could not connect to Kinect 2.";
				return RUISCalibrationPhase.Invalid;
			}
			else
			{
				return RUISCalibrationPhase.Preparation;
			}
		}	
		
		return RUISCalibrationPhase.Invalid; // Loop should not get this far
	}
	
	
	public override RUISCalibrationPhase PreparationPhase(float deltaTime)
	{
		this.guiTextLowerLocal = "Step in front of Kinect. \nTake a " + openVRDeviceName + " controller into your right hand.";
		updateBodyData();
		kinectTrackingID = 0;
		
		for(int a = 0; a < trackingIDs.Length; a++) {
			if(trackingIDs[a].isTracking) {
				kinectTrackingID = trackingIDs[a].trackingId;
				kinectTrackingIndex = trackingIDs[a].index;
			}
		}


		if(vivePrefabContainer && vivePrefabContainer.instantiatedOpenVrCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

		if(trackedOpenVRObjects != null)
		{
			foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
			{

				if(trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1)
					this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
			}
		}
		else
		{
			this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
		}

		if(kinectTrackingID != 0)
		{
			return RUISCalibrationPhase.ReadyToCalibrate;
		}

		return RUISCalibrationPhase.Preparation;
	}
	
	
	public override RUISCalibrationPhase ReadyToCalibratePhase(float deltaTime) 
	{
		this.guiTextLowerLocal = "Hold " + openVRDeviceName + " controller in your right hand. \nPress the trigger button to start calibrating.";

		updateBodyData();

		if (!trackingIDs[kinectTrackingIndex].isTracking) {
			return RUISCalibrationPhase.Preparation;
		}

		if(vivePrefabContainer && vivePrefabContainer.instantiatedOpenVrCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

		if(trackedOpenVRObjects != null)
		{
			foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
			{
//				Debug.Log(trackedOpenVRObject.index + " " + ((int) trackedOpenVRObject.index));
				if(   SteamVR_Controller.Input((int)trackedOpenVRObject.index).connected
				   && SteamVR_Controller.Input((int)trackedOpenVRObject.index).GetHairTrigger())
				{
					viveControllerIndex = (int)trackedOpenVRObject.index;
					viveControllerTransform = trackedOpenVRObject.transform;
					lastViveSample = new Vector3(0, 0, 0);
					lastKinect2Sample = new Vector3(0, 0, 0);
					return RUISCalibrationPhase.Calibration;
				}

				if(trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1)
					this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
			}
		}
		else
		{
			this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
		}

		return RUISCalibrationPhase.ReadyToCalibrate;
	}
	
	
	public override RUISCalibrationPhase CalibrationPhase(float deltaTime) {
		
		this.guiTextLowerLocal = string.Format(  "Calibrating... {0}/{1} samples taken.\n\n"
											   + "Keep the " + openVRDeviceName + " controller in your right\n"
											   + "hand and make wide, calm motions with it.\n"
		                                       + "Have both sensors see it.", numberOfSamplesTaken, numberOfSamplesToTake);
		TakeSample(deltaTime);
		
		if(numberOfSamplesTaken >= numberOfSamplesToTake) 
		{
			timeSinceScriptStart = 0;
			this.calibrationPhaseObjects.SetActive(false);
			this.calibrationResultPhaseObjects.SetActive(true);
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
		if(!calibrationFinnished) 
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
				Vector3 cubePosition = transformMatrix.inverse.MultiplyPoint3x4(samples_Kinect2[i]);
				GameObject cube = MonoBehaviour.Instantiate(calibrationCube, cubePosition, Quaternion.identity) as GameObject;
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
			if(followTransform && viveControllerTransform)
				followTransform.transformToFollow = viveControllerTransform;

			this.guiTextUpperLocal = string.Format(	   "Calibration finished!\n\nTotal Error: {0:0.####}\nMean: {1:0.####}\n\nHold down " 
													+  openVRDeviceName + " touchpad button\nto drag and finetune the results.\n",
			                                       totalErrorDistance, averageError);

			calibrationFinnished = true;                                  
		}

		// Fine tune translation with Vive controller
		if(vivePrefabContainer && vivePrefabContainer.instantiatedOpenVrCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

		if(    SteamVR_Controller.Input(viveControllerIndex).connected )
		{
			var pose = new SteamVR_Utils.RigidTransform(SteamVR_Controller.Input(viveControllerIndex).GetPose().mDeviceToAbsoluteTracking);

			if(SteamVR_Controller.Input(viveControllerIndex).hasTracking)
			{
				if(SteamVR_Controller.Input(viveControllerIndex).GetPressDown(EVRButtonId.k_EButton_SteamVR_Touchpad))
				{
					controllerPositionAtTuneStart = pose.pos;
					translateAtTuneStart = new Vector3(transformMatrix[0, 3], transformMatrix[1, 3], transformMatrix[2, 3]);
				}

				if(SteamVR_Controller.Input(viveControllerIndex).GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad))
				{
					Quaternion rotationQuaternion = MathUtil.QuaternionFromMatrix(rotationMatrix);
					Vector3 translate = translateAtTuneStart + rotationQuaternion * (controllerPositionAtTuneStart - pose.pos);

					transformMatrix.SetColumn(3, new Vector4(translate.x, translate.y, translate.z, transformMatrix.m33));

					updateDictionaries(coordinateSystem.RUISCalibrationResultsInVector3, 
						coordinateSystem.RUISCalibrationResultsInQuaternion,
						coordinateSystem.RUISCalibrationResultsIn4x4Matrix,
						translate, rotationQuaternion, transformMatrix,
						this.inputDevice1, this.inputDevice2);
				}
			}

			if(SteamVR_Controller.Input(viveControllerIndex).GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
			{
				coordinateSystem.SetDeviceToRootTransforms(transformMatrix);
				coordinateSystem.SaveTransformDataToXML(xmlFilename, this.inputDevice1, this.inputDevice2); 
				coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.Kinect_2, kinect2FloorNormal, kinect2DistanceFromFloor);
			}
		}

		return RUISCalibrationPhase.ShowResults;
	}

	public override void PlaceSensorModels()
	{

		//		kinect2ModelObject.transform.rotation = kinect2PitchRotation;
		//		kinect2ModelObject.transform.localPosition = new Vector3(0, kinect2DistanceFromFloor, 0);

		kinect2ModelObject.transform.rotation = coordinateSystem.ConvertRotation(Quaternion.identity, RUISDevice.Kinect_2);
		kinect2ModelObject.transform.position = coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.Kinect_2);

		//		viveCameraObject.transform.position = coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.Vive);
		//		viveCameraObject.transform.rotation = coordinateSystem.ConvertRotation(Quaternion.identity, RUISDevice.Vive);

		if(coordinateSystem.rootDevice == RUISDevice.Kinect_2)
		{
			if(vivePrefabContainer && vivePrefabContainer.instantiatedOpenVrCameraRig)
			{
				vivePrefabContainer.instantiatedOpenVrCameraRig.transform.localRotation = coordinateSystem.GetHmdCoordinateSystemYaw(RUISDevice.OpenVR);
				vivePrefabContainer.instantiatedOpenVrCameraRig.transform.localScale    = coordinateSystem.ExtractLocalScale(RUISDevice.OpenVR);
				vivePrefabContainer.instantiatedOpenVrCameraRig.transform.localPosition = coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.OpenVR);
			}
		}

		if(this.floorPlane)
			this.floorPlane.transform.position = new Vector3(0, 0, 0);
	}
	
	public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
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
		if(timeSinceLastSample < timeBetweenSamples) return;
		timeSinceLastSample = 0;
		
		
		Vector3 vive_sample    = getSample (this.inputDevice1);
		Vector3 kinect2_sample = getSample (this.inputDevice2);

		if(device1Error || device2Error)
		{
			showMovementAlert = false;
			return;
		}

		// Check that we didn't just start
		if(kinect2_sample == Vector3.zero || vive_sample == Vector3.zero) 
		{
			return;
		}

		// Check that we are not taking samples too frequently
		if(   Vector3.Distance(kinect2_sample, lastKinect2Sample) < calibrationSettings.sampleMinDistance
		   || Vector3.Distance(vive_sample, lastViveSample)       < calibrationSettings.sampleMinDistance)
		{
			if(!showMovementAlert && Time.time - lastMovementAlertTime > 3)
			{
				lastMovementAlertTime = Time.time;
				showMovementAlert = true;
			}
			if(showMovementAlert)
				this.guiTextUpperLocal = "Not enough hand movement.";
			return;
		}
		else
		{
			lastMovementAlertTime = Time.time;
			if(showMovementAlert)
				showMovementAlert = false;
			this.guiTextUpperLocal = "";
		}

		lastKinect2Sample = kinect2_sample;
		lastViveSample    = vive_sample;

//		Debug.Log(vive_sample + " " + kinect2_sample);
		samples_Kinect2.Add(kinect2_sample);
		samples_Vive.Add(vive_sample);
		calibrationSpheres.Add(MonoBehaviour.Instantiate(calibrationSphere, vive_sample, Quaternion.identity) as GameObject);
		numberOfSamplesTaken++;
	} 
	
	
	private Vector3 getSample(RUISDevice device) 
	{
		Vector3 sample = new Vector3(0,0,0);
		updateBodyData();
		if(device == RUISDevice.Kinect_2) 
		{
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
							sample = coordinateSystem.ConvertRawKinect2Location(sample);
							device1Error = false;
						}
					}
				}
				
			}
			if(!trackedBodyFound && foundBodies > 1) 
			{
				device1Error = true;
				this.guiTextUpperLocal = "Step out of the Kinect's\nview and come back.";
			}
		}
		if(device == RUISDevice.OpenVR)
		{
			if(vivePrefabContainer && vivePrefabContainer.instantiatedOpenVrCameraRig)
				trackedOpenVRObjects = vivePrefabContainer.instantiatedOpenVrCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

			if(trackedOpenVRObjects != null)
			{
				foreach(SteamVR_TrackedObject trackedOpenVRObject in trackedOpenVRObjects)
				{
					if(viveControllerIndex == (int)trackedOpenVRObject.index)
					{
						if(   SteamVR_Controller.Input(viveControllerIndex).connected
						   && SteamVR_Controller.Input(viveControllerIndex).valid
						   && SteamVR_Controller.Input(viveControllerIndex).hasTracking)
						{
							sample = trackedOpenVRObject.transform.localPosition;
							device2Error = false;

							break;
						}
						else
						{
							device2Error = true;
							this.guiTextUpperLocal = openVRDeviceName + " controller not tracked.";
						}
					}

					if (trackedOpenVRObject.index == SteamVR_TrackedObject.EIndex.Hmd && trackedOpenVRObjects.Length == 1) 
					{
						device2Error = true;
						this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
					}
				}
			}
			else
			{
				device2Error = true;
				this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
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
//				this.guiTextUpperLocal = openVRDeviceName + " controller not detected.";
//			}
		}
		
		return sample;
		
		
	}
	
	private void CalculateTransformation()
	{
		if (samples_Kinect2.Count != numberOfSamplesTaken || samples_Vive.Count != numberOfSamplesTaken)
		{
			Debug.LogError("Mismatch in sample list lengths!");
		}
		
		Matrix viveMatrix;
		Matrix kinect2Matrix;
		
		viveMatrix = Matrix.Zeros (samples_Vive.Count, 4);
		kinect2Matrix = Matrix.Zeros (samples_Kinect2.Count, 3);
		
		for (int i = 1; i <= samples_Vive.Count; i++) {
			viveMatrix [i, 1] = new Complex (samples_Vive [i - 1].x);
			viveMatrix [i, 2] = new Complex (samples_Vive [i - 1].y);
			viveMatrix [i, 3] = new Complex (samples_Vive [i - 1].z);
			viveMatrix [i, 4] = new Complex (1.0f);
		}
		for (int i = 1; i <= samples_Kinect2.Count; i++) {
			kinect2Matrix [i, 1] = new Complex (samples_Kinect2 [i - 1].x);
			kinect2Matrix [i, 2] = new Complex (samples_Kinect2 [i - 1].y);
			kinect2Matrix [i, 3] = new Complex (samples_Kinect2 [i - 1].z);
		}
		
		//perform a matrix solve Ax = B. We have to get transposes and inverses because viveMatrix isn't square
		//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
		Matrix transformMatrixSolution = (viveMatrix.Transpose() * viveMatrix).Inverse() * viveMatrix.Transpose() * kinect2Matrix;
		
		Matrix error = viveMatrix * transformMatrixSolution - kinect2Matrix;
		
		transformMatrixSolution = transformMatrixSolution.Transpose();
		
		Debug.Log(transformMatrixSolution);
		Debug.Log(error);
		
		List<Vector3> orthogonalVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(MathUtil.MatrixToMatrix4x4(transformMatrixSolution)));
		rotationMatrix = CreateRotationMatrix(orthogonalVectors);
		Debug.Log(rotationMatrix);
		
		transformMatrix = MathUtil.MatrixToMatrix4x4(transformMatrixSolution);//CreateTransformMatrix(transformMatrixSolution);
		Debug.Log(transformMatrix);

		UpdateFloorNormalAndDistance(); 

		Quaternion rotationQuaternion = MathUtil.QuaternionFromMatrix(rotationMatrix);

		coordinateSystem.SetDeviceToRootTransforms(transformMatrix);
		coordinateSystem.SaveTransformDataToXML(xmlFilename, RUISDevice.OpenVR,  RUISDevice.Kinect_2); 
		coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.Kinect_2, kinect2FloorNormal, kinect2DistanceFromFloor);


		Vector3 translate = new Vector3(transformMatrix[0, 3], transformMatrix[1, 3], transformMatrix[2, 3]);
		updateDictionaries(	coordinateSystem.RUISCalibrationResultsInVector3, 
		                   	coordinateSystem.RUISCalibrationResultsInQuaternion,
		                   	coordinateSystem.RUISCalibrationResultsIn4x4Matrix,
							translate, rotationQuaternion, transformMatrix,
							RUISDevice.OpenVR, RUISDevice.Kinect_2);
		                   
		coordinateSystem.RUISCalibrationResultsDistanceFromFloor[RUISDevice.Kinect_2] = kinect2DistanceFromFloor;
		coordinateSystem.RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] = kinect2PitchRotation; 
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
	
	private void UpdateFloorNormalAndDistance()
	{
		coordinateSystem.ResetFloorNormal(RUISDevice.Kinect_2);
		
		Windows.Kinect.Vector4 kinect2FloorPlane = kinect2SourceManager.GetFlootClipPlane();
		kinect2FloorNormal = new Vector3(kinect2FloorPlane.X, kinect2FloorPlane.Y, kinect2FloorPlane.Z);
		kinect2FloorNormal.Normalize();

		if(kinect2FloorNormal.sqrMagnitude < 0.1f)
			kinect2FloorNormal = Vector3.up;

		kinect2DistanceFromFloor = kinect2FloorPlane.W / Mathf.Sqrt(kinect2FloorNormal.sqrMagnitude);
		
		if(float.IsNaN(kinect2DistanceFromFloor))
			kinect2DistanceFromFloor = 0;

		Quaternion kinect2FloorRotator = Quaternion.FromToRotation(kinect2FloorNormal, Vector3.up); 
		
		kinect2PitchRotation = Quaternion.Inverse (kinect2FloorRotator);

		coordinateSystem.SetDistanceFromFloor(kinect2DistanceFromFloor, RUISDevice.Kinect_2);
		coordinateSystem.SetFloorNormal(kinect2FloorNormal, RUISDevice.Kinect_2);
	}
	
	private void updateBodyData() {
		
		bodyData = kinect2SourceManager.GetBodyData();
		
		if(bodyData != null) {
			// Update tracking ID array
			for(int y = 0; y < trackingIDs.Length; y++) {
				trackingIDs[y].isTracking = false; 
				trackingIDs[y].index = -1;
			}
			
			// Check tracking status and assing old indexes
			var arrayIndex = 0;
			foreach(var body in bodyData) {
				
				if(body.IsTracked) {
					for(int y = 0; y < trackingIDs.Length; y++) {
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
			foreach(var body in bodyData) {
				if(body.IsTracked) {
					if(!trackingIDtoIndex.ContainsKey(body.TrackingId)) { // A new body
						for(int y = 0; y < trackingIDs.Length; y++) {
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

	private void OnDeviceConnected(int deviceId, bool isConnected)
	{
		int htcViveControllerIndex = 3;
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

		if(SteamVR_Controller.Input(htcViveControllerIndex).valid)
		{


		}

	}

	~RUISKinect2ToOpenVrControllerCalibrationProcess() // HACK TODO does this work in all cases, calibration finish/abort?
	{
//		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
		SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
	}
}
