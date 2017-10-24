/*****************************************************************************

Content    :   Handles the calibration procedure between CustomDevice1 and OpenVR
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
using Valve.VR;

public class RUISCustomDevice1ToOpenVrControllerCalibrationProcess : RUISCalibrationProcess {
	
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
	private string customDevice1Name = "Custom Device 1"; //###

	bool showMovementAlert = false;
	float lastMovementAlertTime = 0;

	// Custom variables
	private List<Vector3> samplesCustom1, samplesVive;
	private int numberOfSamplesTaken, numberOfSamplesToTake, numberOfSamplesPerSecond;
	private float timeSinceLastSample, timeBetweenSamples, timeSinceScriptStart = 0;
	public RUISCoordinateSystem coordinateSystem;
	public RUISInputManager inputManager;
	private bool viveChecked = false, calibrationFinnished = false;
	List<GameObject> calibrationSpheres;
	private GameObject calibrationPhaseObjects, calibrationResultPhaseObjects, viveCameraObject, 
	custom1ModelObject, floorPlane, /*depthView,*/
	viveIcon, custom1Icon, deviceModelObjects, depthViewObjects, iconObjects;
	
	private Vector3 lastCustom1Sample, lastViveSample;
	private string xmlFilename;
	
	private Matrix4x4 rotationMatrix, transformMatrix;

	Quaternion custom1PitchRotation = Quaternion.identity;
	float custom1DistanceFromFloor = 0;
	Vector3 custom1FloorNormal = Vector3.up;

	bool device1Error, device2Error;

	RUISVivePrefabContainer vivePrefabContainer;
	SteamVR_TrackedObject[] trackedOpenVRObjects;
	int viveControllerIndex = 0;
	Transform viveControllerTransform;
	Vector3 translateAtTuneStart = Vector3.zero;
	Vector3 controllerPositionAtTuneStart = Vector3.zero;

	RUISCalibrationProcessSettings calibrationSettings;

	public RUISCustomDevice1ToOpenVrControllerCalibrationProcess(RUISCalibrationProcessSettings calibrationSettings) {
		
		this.inputDevice1 = RUISDevice.OpenVR;
		this.inputDevice2 = RUISDevice.CustomDevice1;
		
		this.numberOfSamplesToTake = calibrationSettings.numberOfSamplesToTake;
		this.numberOfSamplesPerSecond = calibrationSettings.numberOfSamplesPerSecond;

//		SteamVR_Utils.Event.Listen("device_connected", OnDeviceConnected);
		SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
		
		inputManager = MonoBehaviour.FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;

		if(RUISCalibrationProcessSettings.originalMasterCoordinateSystem == RUISDevice.CustomDevice1)
			coordinateSystem.rootDevice = RUISDevice.CustomDevice1;
		else
			coordinateSystem.rootDevice = RUISDevice.OpenVR;

		vivePrefabContainer = Component.FindObjectOfType<RUISVivePrefabContainer>();
		if(vivePrefabContainer)
		{
			if(vivePrefabContainer.viveCameraRigPrefab)
			{
				vivePrefabContainer.instantiatedViveCameraRig = GameObject.Instantiate(vivePrefabContainer.viveCameraRigPrefab);
				Camera[] rigCams = vivePrefabContainer.instantiatedViveCameraRig.GetComponentsInChildren<Camera>();
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
				Debug.LogError("The viveCameraRigPrefab field in " + typeof(RUISVivePrefabContainer) + " is null, and calibration will not work!");
		}
		else
		{
			Debug.LogError("Could not locate " + typeof(RUISVivePrefabContainer) + " component in this scene, and calibration will not work!");
		}

		this.timeSinceScriptStart = 0;
		this.timeBetweenSamples = 1 / (float)numberOfSamplesPerSecond;
		
		// Limit sample rate
		if(this.timeBetweenSamples < 0.1f) {
			this.timeBetweenSamples = 0.1f;
		}
		
		calibrationSpheres = new List<GameObject>();
		
		samplesCustom1 = new List<Vector3>();
		samplesVive = new List<Vector3>();

		this.calibrationSettings = calibrationSettings;

		this.calibrationPhaseObjects = calibrationSettings.calibrationPhaseObjects;
		this.calibrationResultPhaseObjects = calibrationSettings.calibrationResultPhaseObjects;
		
		this.deviceModelObjects = calibrationSettings.deviceModelObjects;
		this.depthViewObjects = calibrationSettings.depthViewObjects;
		this.iconObjects = calibrationSettings.iconObjects;
		this.floorPlane = calibrationSettings.floorPlane; //GameObject.Find ("Floor");
		
		if(GameObject.Find ("PSMoveWand") != null)
			GameObject.Find ("PSMoveWand").SetActive(false);
		if(GameObject.Find ("HmdModel")) // "Was OculusRift"
			GameObject.Find ("HmdModel").SetActive(false);
		
		// Models
		this.viveCameraObject = GameObject.Find ("HmdCamera"); // ###
		this.custom1ModelObject = GameObject.Find ("Kinect2Camera"); // ###

		// Depth view
//		this.depthView = GameObject.Find ("Kinect2DepthView"); // ***
		
		// Icons
		this.viveIcon = GameObject.Find ("Hmd Icon"); // ###
		this.custom1Icon = GameObject.Find ("Kinect2 Icon"); // ###

		if(this.viveIcon && this.viveIcon.GetComponent<GUITexture>())
			this.viveIcon.GetComponent<GUITexture>().pixelInset = new Rect(5.1f, 10.0f, 70.0f, 70.0f);
		
		foreach (Transform child in this.deviceModelObjects.transform)
		{
			child.gameObject.SetActive(false);
		}
		
		foreach (Transform child in this.depthViewObjects.transform)
		{
			child.gameObject.SetActive(false);
		}
		
		foreach (Transform child in this.iconObjects.transform)
		{
			child.gameObject.SetActive(false);
		}
		
		if(this.viveCameraObject)
			this.viveCameraObject.SetActive(false);
		if(this.custom1ModelObject)
			this.custom1ModelObject.SetActive(true);
		if(this.viveIcon)
			this.viveIcon.SetActive(true);
		if(this.custom1Icon)
			this.custom1Icon.SetActive(true);
		if(this.calibrationPhaseObjects)
			this.calibrationPhaseObjects.SetActive(true);
		if(this.calibrationResultPhaseObjects)
			this.calibrationResultPhaseObjects.SetActive(false);
//		if(this.depthView) // ***
//			this.depthView.SetActive(true);
		this.xmlFilename = calibrationSettings.xmlFilename;
	}
	
	
	public override RUISCalibrationPhase InitialPhase(float deltaTime) 
	{
		timeSinceScriptStart += deltaTime;
		
		if(timeSinceScriptStart < 2)
		{
			this.guiTextLowerLocal = "Calibration of " + customDevice1Name + " and OpenVR Tracking\n\n Starting up...";
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

		// Wait to connect to custom1device
//		if(timeSinceScriptStart < 5)
//		{
//			this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Please wait...";
//			return RUISCalibrationPhase.Initial;
//		}

		// Execute once only
		if(calibrationSettings.customDevice1Tracker)
		{
			lastCustom1Sample = new Vector3(0, 0, 0);
			return RUISCalibrationPhase.Preparation;
		}
		else
		{
			this.guiTextLowerLocal = "Error: 'CustomDevice1 Tracked Pose' field in\n" 
									+ typeof(RUISCoordinateCalibration) + " component is null!";
		}
		
		return RUISCalibrationPhase.Invalid; // Loop should not get this far
	}
	
	
	public override RUISCalibrationPhase PreparationPhase(float deltaTime)
	{
		this.guiTextLowerLocal = "Hold " + openVRDeviceName + " controller and CustomDevice1 together.";

		if(vivePrefabContainer && vivePrefabContainer.instantiatedViveCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedViveCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

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

		// Has a duration of timeSinceLastSample passed?
		if(timeSinceLastSample < Mathf.Clamp(timeBetweenSamples, float.Epsilon, 1))
		{
			timeSinceLastSample += deltaTime;
		}
		else
		{
			timeSinceLastSample = 0;
			// Check if customDevice1Tracker position is moving
			if(lastCustom1Sample != calibrationSettings.customDevice1Tracker.position)
				return RUISCalibrationPhase.ReadyToCalibrate;
			else
				this.guiTextUpperLocal = "No motion detected in 'CustomDevice1 Tracked Pose' of\n" + typeof(RUISCoordinateCalibration) 
										+ " component. Its position should correspond to\nthe tracked CustomDevice1.";
			lastCustom1Sample = calibrationSettings.customDevice1Tracker.position;
		}

		return RUISCalibrationPhase.Preparation;
	}
	
	
	public override RUISCalibrationPhase ReadyToCalibratePhase(float deltaTime) 
	{
		this.guiTextLowerLocal = "Hold " + openVRDeviceName + " controller and CustomDevice1 together."
								+"\nPress the trigger button to start calibrating.";


		if(vivePrefabContainer && vivePrefabContainer.instantiatedViveCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedViveCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

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
					lastCustom1Sample = new Vector3(0, 0, 0);
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
//			this.depthView.SetActive(false); // ***
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
				Vector3 cubePosition = transformMatrix.inverse.MultiplyPoint3x4(samplesCustom1[i]);
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
			if(followTransform && viveControllerTransform)
				followTransform.transformToFollow = viveControllerTransform;

			this.guiTextUpperLocal = string.Format(	   "Calibration finished!\n\nTotal Error: {0:0.####}\nMean: {1:0.####}\n\nHold down " 
													+  openVRDeviceName + " touchpad button\nto drag and finetune the results.\n",
			                                       totalErrorDistance, averageError);

			calibrationFinnished = true;                                  
		}

		// Fine tune translation with Vive controller
		if(vivePrefabContainer && vivePrefabContainer.instantiatedViveCameraRig)
			trackedOpenVRObjects = vivePrefabContainer.instantiatedViveCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

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
				coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.CustomDevice1, custom1FloorNormal, custom1DistanceFromFloor);
			}
		}

		return RUISCalibrationPhase.ShowResults;
	}

	public override void PlaceSensorModels()
	{
		custom1ModelObject.transform.rotation = coordinateSystem.ConvertRotation(Quaternion.identity, RUISDevice.CustomDevice1);
		custom1ModelObject.transform.position = coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.CustomDevice1);

		if(coordinateSystem.rootDevice == RUISDevice.CustomDevice1)
		{
			if(vivePrefabContainer && vivePrefabContainer.instantiatedViveCameraRig)
			{
				vivePrefabContainer.instantiatedViveCameraRig.transform.localRotation = coordinateSystem.GetHmdCoordinateSystemYaw(RUISDevice.OpenVR);
				vivePrefabContainer.instantiatedViveCameraRig.transform.localScale    = coordinateSystem.ExtractLocalScale(RUISDevice.OpenVR);
				vivePrefabContainer.instantiatedViveCameraRig.transform.localPosition = coordinateSystem.ConvertLocation(Vector3.zero, RUISDevice.OpenVR);
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
		if(timeSinceLastSample < timeBetweenSamples) 
			return;
		timeSinceLastSample = 0;
		
		
		Vector3 viveSample    = getSample (this.inputDevice1);
		Vector3 custom1Sample = getSample (this.inputDevice2);

		if(device1Error || device2Error)
		{
			showMovementAlert = false;
			return;
		}

		// Check that we didn't just start
		if(custom1Sample == Vector3.zero || viveSample == Vector3.zero) 
		{
			return;
		}

		// Check that we are not taking samples too frequently
		if(   Vector3.Distance(custom1Sample, lastCustom1Sample) < calibrationSettings.sampleMinDistance
		   || Vector3.Distance(viveSample, lastViveSample)       < calibrationSettings.sampleMinDistance)
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

		lastCustom1Sample = custom1Sample;
		lastViveSample    = viveSample;

//		Debug.Log(viveSample + " " + custom1Sample);
		samplesCustom1.Add(custom1Sample);
		samplesVive.Add(viveSample);
		calibrationSpheres.Add(MonoBehaviour.Instantiate(calibrationSettings.device2SamplePrefab, viveSample, Quaternion.identity) as GameObject);
		numberOfSamplesTaken++;
	} 
	
	
	private Vector3 getSample(RUISDevice device) 
	{
		Vector3 sample = new Vector3(0,0,0);

		if(device == RUISDevice.CustomDevice1) 
		{
			sample = calibrationSettings.customDevice1Tracker.position;
			device1Error = false;
		}
		if(device == RUISDevice.OpenVR)
		{
			if(vivePrefabContainer && vivePrefabContainer.instantiatedViveCameraRig)
				trackedOpenVRObjects = vivePrefabContainer.instantiatedViveCameraRig.GetComponentsInChildren<SteamVR_TrackedObject>();

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
		if (samplesCustom1.Count != numberOfSamplesTaken || samplesVive.Count != numberOfSamplesTaken)
		{
			Debug.LogError("Mismatch in sample list lengths!");
		}
		
		Matrix viveMatrix;
		Matrix custom1Matrix;
		
		viveMatrix = Matrix.Zeros (samplesVive.Count, 4);
		custom1Matrix = Matrix.Zeros (samplesCustom1.Count, 3);
		
		for (int i = 1; i <= samplesVive.Count; i++) {
			viveMatrix [i, 1] = new Complex (samplesVive [i - 1].x);
			viveMatrix [i, 2] = new Complex (samplesVive [i - 1].y);
			viveMatrix [i, 3] = new Complex (samplesVive [i - 1].z);
			viveMatrix [i, 4] = new Complex (1.0f);
		}
		for (int i = 1; i <= samplesCustom1.Count; i++) {
			custom1Matrix [i, 1] = new Complex (samplesCustom1 [i - 1].x);
			custom1Matrix [i, 2] = new Complex (samplesCustom1 [i - 1].y);
			custom1Matrix [i, 3] = new Complex (samplesCustom1 [i - 1].z);
		}
		
		//perform a matrix solve Ax = B. We have to get transposes and inverses because viveMatrix isn't square
		//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
		Matrix transformMatrixSolution = (viveMatrix.Transpose() * viveMatrix).Inverse() * viveMatrix.Transpose() * custom1Matrix;
		
		Matrix error = viveMatrix * transformMatrixSolution - custom1Matrix;
		
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
		coordinateSystem.SaveTransformDataToXML(xmlFilename, RUISDevice.OpenVR,  RUISDevice.CustomDevice1); 
		coordinateSystem.SaveFloorData(xmlFilename, RUISDevice.CustomDevice1, custom1FloorNormal, custom1DistanceFromFloor);


		Vector3 translate = new Vector3(transformMatrix[0, 3], transformMatrix[1, 3], transformMatrix[2, 3]);
		updateDictionaries(	coordinateSystem.RUISCalibrationResultsInVector3, 
		                   	coordinateSystem.RUISCalibrationResultsInQuaternion,
		                   	coordinateSystem.RUISCalibrationResultsIn4x4Matrix,
							translate, rotationQuaternion, transformMatrix,
							RUISDevice.OpenVR, RUISDevice.CustomDevice1);
		                   
		coordinateSystem.RUISCalibrationResultsDistanceFromFloor[RUISDevice.CustomDevice1] = custom1DistanceFromFloor;
		coordinateSystem.RUISCalibrationResultsFloorPitchRotation[RUISDevice.CustomDevice1] = custom1PitchRotation; 
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
	private void UpdateFloorNormalAndDistance()
	{
		coordinateSystem.ResetFloorNormal(RUISDevice.CustomDevice1);
		if(calibrationSettings.customDevice1FloorPoint)
		{
			Vector3 newFloorNormal   = calibrationSettings.customDevice1FloorPoint.up.normalized;
			Vector3 newFloorPosition = calibrationSettings.customDevice1FloorPoint.position;

			//Project the position of the customDevice1 origin onto the floor
			//http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
			//http://en.wikipedia.org/wiki/Plane_(geometry)
			float d = newFloorNormal.x * newFloorPosition.x + newFloorNormal.y * newFloorPosition.y + newFloorNormal.z * newFloorPosition.z;
			Vector3 closestFloorPointToCustom1Device = new Vector3(newFloorNormal.x, newFloorNormal.y, newFloorNormal.z);
			closestFloorPointToCustom1Device = (closestFloorPointToCustom1Device * d) / closestFloorPointToCustom1Device.sqrMagnitude;

			Quaternion custom1FloorRotator = Quaternion.FromToRotation(newFloorNormal, Vector3.up);

			//transform the point from custom1device's coordinate system rotation to Unity's rotation
			closestFloorPointToCustom1Device = custom1FloorRotator * closestFloorPointToCustom1Device;

			if(float.IsNaN(closestFloorPointToCustom1Device.magnitude))
				closestFloorPointToCustom1Device = Vector3.zero;
			if(newFloorNormal.sqrMagnitude < 0.1f)
				newFloorNormal = Vector3.up;

			custom1DistanceFromFloor = closestFloorPointToCustom1Device.magnitude;
			custom1FloorNormal = newFloorNormal;
			
			custom1PitchRotation = Quaternion.Inverse(custom1FloorRotator);
		}

		coordinateSystem.SetDistanceFromFloor(custom1DistanceFromFloor, RUISDevice.CustomDevice1);
		coordinateSystem.SetFloorNormal(custom1FloorNormal, RUISDevice.CustomDevice1);
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

	~RUISCustomDevice1ToOpenVrControllerCalibrationProcess() // HACK TODO does this work in all cases, calibration finish/abort?
	{
//		SteamVR_Utils.Event.Remove("device_connected", OnDeviceConnected);
		SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
	}
}
