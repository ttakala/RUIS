using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSML;
using Kinect = Windows.Kinect;

public class RUISKinect2ToKinectCalibrationProcess : RUISCalibrationProcess {
	
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
	
	// Custom variables
	private NIPlayerManagerCOMSelection kinectSelection;
	private OpenNISettingsManager settingsManager;
	private OpenNI.SceneAnalyzer sceneAnalyzer;
	private List<Vector3> samples_Kinect1, samples_Kinect2;
	private int numberOfSamplesTaken, numberOfSamplesToTake, numberOfSamplesPerSecond;
	private float timeSinceLastSample, timeBetweenSamples, timeSinceScriptStart;
	public RUISCoordinateSystem coordinateSystem;
	public RUISInputManager inputManager;
	private bool kinectChecked = false, kinect2Checked = false, calibrationFinnished = false;
	List<GameObject> calibrationSpheres;
	private GameObject calibrationPhaseObjects, calibrationResultPhaseObjects, kinect1ModelObject, 
	kinect2ModelObject, floorPlane, calibrationSphere, calibrationCube, depthView,
	Kinect1Icon, Kinect2Icon, deviceModelObjects, depthViewObjects, iconObjects;
	
	private Vector3 lastPSMoveSample, lastKinect2Sample, lastKinectSample;
	private string xmlFilename;
	Quaternion kinect1PitchRotation = Quaternion.identity;
	Quaternion kinect2PitchRotation = Quaternion.identity;
	float kinect1DistanceFromFloor = 0;
	float kinect2DistanceFromFloor = 0;
	
	private Matrix4x4 rotationMatrix, transformMatrix;
	
	Kinect2SourceManager kinect2SourceManager;
	Kinect.Body[] bodyData; 
	
	private trackedBody[] trackingIDs = null; // Defined in RUISKinect2DepthView
	private Dictionary<ulong, int> trackingIDtoIndex = new Dictionary<ulong, int>();
	private int kinectTrackingIndex;
	private ulong kinectTrackingID;
	
	public RUISKinect2ToKinectCalibrationProcess(RUISCalibrationProcessSettings calibrationSettings) {
		
		this.inputDevice1 = RUISDevice.Kinect_2;
		this.inputDevice2 = RUISDevice.Kinect_1;
		
		this.numberOfSamplesToTake = calibrationSettings.numberOfSamplesToTake;
		this.numberOfSamplesPerSecond = calibrationSettings.numberOfSamplesPerSecond;
		
		trackingIDs = new trackedBody[6]; 
		for(int y = 0; y < trackingIDs.Length; y++) {
			trackingIDs[y] = new trackedBody(-1, false, 1);
		}
		
		kinectSelection = MonoBehaviour.FindObjectOfType(typeof(NIPlayerManagerCOMSelection)) as NIPlayerManagerCOMSelection;
		settingsManager = MonoBehaviour.FindObjectOfType(typeof(OpenNISettingsManager)) as OpenNISettingsManager;
		inputManager = MonoBehaviour.FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		coordinateSystem = MonoBehaviour.FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
		kinect2SourceManager = MonoBehaviour.FindObjectOfType(typeof(Kinect2SourceManager)) as Kinect2SourceManager;
		
		this.timeSinceScriptStart = 0;
		this.timeBetweenSamples = 1 / (float)numberOfSamplesPerSecond;
		
		// Limit sample rate
		if(this.timeBetweenSamples < 0.1f) {
			this.timeBetweenSamples = 0.1f;
		}
		
		calibrationSpheres = new List<GameObject>();
		
		samples_Kinect1 = new List<Vector3>();
		samples_Kinect2 = new List<Vector3>();
		
		this.calibrationCube = calibrationSettings.calibrationCubePrefab;
		this.calibrationSphere = calibrationSettings.calibrationSpherePrefab;
		this.calibrationPhaseObjects = calibrationSettings.calibrationPhaseObjects;
		this.calibrationResultPhaseObjects = calibrationSettings.calibrationResultPhaseObjects;
		
		this.deviceModelObjects = calibrationSettings.deviceModelObjects;
		this.depthViewObjects = calibrationSettings.depthViewObjects;
		this.iconObjects = calibrationSettings.iconObjects;
		
		if(GameObject.Find ("PSMoveWand") != null) GameObject.Find ("PSMoveWand").SetActive(false);
		
		// Models
		this.kinect1ModelObject = GameObject.Find ("KinectCamera");
		this.kinect2ModelObject = GameObject.Find ("Kinect2Camera");
		
		// Depth view
		this.depthView = GameObject.Find ("KinectDepthView");
		
		// Icons
		this.Kinect1Icon = GameObject.Find ("Kinect Icon");
		this.Kinect2Icon = GameObject.Find ("Kinect2 Icon");

		this.Kinect1Icon.GetComponent<GUITexture>().pixelInset = new Rect(5.1f, 10.0f, 70.0f, 70.0f);
		
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
		
		this.kinect1ModelObject.SetActive(true);
		this.kinect2ModelObject.SetActive(true);
		this.Kinect1Icon.SetActive(true);
		this.Kinect2Icon.SetActive(true);
		this.calibrationPhaseObjects.SetActive(true);
		this.calibrationResultPhaseObjects.SetActive(false);
		this.depthView.SetActive(true);
		this.xmlFilename = calibrationSettings.xmlFilename;
	}
	
	
	public override RUISCalibrationPhase InitialPhase(float deltaTime) {
		
		timeSinceScriptStart += deltaTime;
		
		if(timeSinceScriptStart < 3) {
			this.guiTextLowerLocal = "Calibration of Kinect 1 and Kinect 2\n\n Starting up...";
			return RUISCalibrationPhase.Initial;
		}
		
		if(timeSinceScriptStart < 4) {
			this.guiTextLowerLocal = "Connecting to Kinect 1. \n\n Please wait...";
			return RUISCalibrationPhase.Initial;
		}
		 
		if(!kinectChecked && timeSinceScriptStart > 4) {
			if (settingsManager == null) {
				this.guiTextLowerLocal = "Connecting to Kinect 1. \n\n Error: Could not start OpenNI";
				return RUISCalibrationPhase.Invalid;
			}
			else if(settingsManager.UserGenrator == null) {
				this.guiTextLowerLocal = "Connecting to Kinect 1. \n\n Error: Could not start OpenNI";
				return RUISCalibrationPhase.Invalid;
			}
			else if(!settingsManager.UserGenrator.Valid) {
				this.guiTextLowerLocal = "Connecting to Kinect 1. \n\n Error: Could not start OpenNI";
				return RUISCalibrationPhase.Invalid;
			}
			else {
				sceneAnalyzer = new OpenNI.SceneAnalyzer((MonoBehaviour.FindObjectOfType(typeof(OpenNISettingsManager)) as OpenNISettingsManager).CurrentContext.BasicContext);
				sceneAnalyzer.StartGenerating();
			}
			kinectChecked = true;	
		}	
		
		if(timeSinceScriptStart < 5) {
			this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Please wait...";
			return RUISCalibrationPhase.Initial;
		}
		
		if(!kinect2Checked && timeSinceScriptStart > 5) {
			kinect2Checked = true;	
			if (!kinect2SourceManager.GetSensor().IsOpen || !kinect2SourceManager.GetSensor().IsAvailable) {
				this.guiTextLowerLocal = "Connecting to Kinect 2. \n\n Error: Could not connect to Kinect 2.";
				return RUISCalibrationPhase.Invalid;
			}
			else {
				return RUISCalibrationPhase.Preparation;
			}
			
		}	
		
		return RUISCalibrationPhase.Invalid; // Loop should not get this far
	}
	
	
	public override RUISCalibrationPhase PreparationPhase(float deltaTime) {
		this.guiTextLowerLocal = "Step in front of the camera.";
		updateBodyData();
		kinectTrackingID = 0;
		
		for(int a = 0; a < trackingIDs.Length; a++) {
			if(trackingIDs[a].isTracking) {
				kinectTrackingID = trackingIDs[a].trackingId;
				kinectTrackingIndex = trackingIDs[a].index;
			}
		}
		
		if(kinectTrackingID != 0) return RUISCalibrationPhase.ReadyToCalibrate;
		else return RUISCalibrationPhase.Preparation;
		
	}
	
	
	public override RUISCalibrationPhase ReadyToCalibratePhase(float deltaTime) {
			return RUISCalibrationPhase.Calibration;
	}
	
	
	public override RUISCalibrationPhase CalibrationPhase(float deltaTime) {
		
		this.guiTextLowerLocal = string.Format("Calibrating... {0}/{1} samples taken.", numberOfSamplesTaken, numberOfSamplesToTake);
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
				Vector3 cubePosition =  transformMatrix.MultiplyPoint3x4(samples_Kinect2[i]);
				GameObject cube = MonoBehaviour.Instantiate(calibrationCube, cubePosition, Quaternion.identity) as GameObject;
				cube.GetComponent<RUISSampleDifferenceVisualizer>().kinectCalibrationSphere = sphere;
				
				
				distance += Vector3.Distance(sphere.transform.position, cubePosition);
				errorMagnitudes.Add(distance);
				error += cubePosition - sphere.transform.position;
				
				sphere.transform.parent = calibrationResultPhaseObjects.transform;
				cube.transform.parent = calibrationResultPhaseObjects.transform;
			}
			
			totalErrorDistance = distance;
			averageError = distance / calibrationSpheres.Count;
			
			calibrationResultPhaseObjects.SetActive(true);
			kinect1ModelObject.transform.position = transformMatrix.MultiplyPoint3x4(kinect1ModelObject.transform.position);
			
			this.guiTextUpperLocal = string.Format("Calibration finished!\n\nTotal Error: {0:0.####}\nMean: {1:0.####}\n",
			                                       totalErrorDistance, averageError);
			
			calibrationFinnished = true;                                  
		}
		return RUISCalibrationPhase.ShowResults;
	}
	
	
	// Custom functionsRUISCalibrationPhase.Stopped
	private void TakeSample(float deltaTime)
	{
		timeSinceLastSample += deltaTime;
		if(timeSinceLastSample < timeBetweenSamples) return;
		timeSinceLastSample = 0;
		
		
		Vector3 Kinect2_sample = getSample (this.inputDevice1);
		Vector3 Kinect1_sample = getSample (this.inputDevice2);
		
//		if(Kinect2_sample == null || Kinect1_sample == null) return; // No data from device
		if (Kinect2_sample == Vector3.zero || Kinect1_sample == Vector3.zero) //Data not valid
		{
			return;
		}
		
		samples_Kinect1.Add(Kinect1_sample);
		samples_Kinect2.Add(Kinect2_sample);
		calibrationSpheres.Add(MonoBehaviour.Instantiate(calibrationSphere, Kinect1_sample, Quaternion.identity) as GameObject);
		numberOfSamplesTaken++;
	} 
	
	
	private Vector3 getSample(RUISDevice device) {
		Vector3 sample = new Vector3(0,0,0);
		Vector3 tempSample;
		
		if(device == RUISDevice.Kinect_2) {
			Kinect.Body[] data = kinect2SourceManager.GetBodyData();
			foreach(var body in data) {
				if(body.IsTracked && body.Joints[Kinect.JointType.HandRight].TrackingState == Kinect.TrackingState.Tracked) {
					tempSample = new Vector3(body.Joints[Kinect.JointType.HandRight].Position.X,
					                         body.Joints[Kinect.JointType.HandRight].Position.Y,
					                         body.Joints[Kinect.JointType.HandRight].Position.Z);
					tempSample = coordinateSystem.ConvertRawKinect2Location(tempSample);
					if(Vector3.Distance(tempSample, lastKinect2Sample) > 0.1) {
						sample = tempSample;
						lastKinect2Sample = sample;
						this.guiTextUpperLocal = "";
					}
					else {
						this.guiTextUpperLocal = "Not enough hand movement.";
					}
				}
			}
			
		}
		if(device == RUISDevice.Kinect_1) {
			OpenNI.SkeletonJointPosition jointPosition;
			bool success = kinectSelection.GetPlayer(0).GetSkeletonJointPosition(OpenNI.SkeletonJoint.RightHand, out jointPosition);
			if(success && jointPosition.Confidence >= 0.5) { 
				tempSample = coordinateSystem.ConvertRawKinectLocation(jointPosition.Position);
				if(Vector3.Distance(tempSample, lastKinectSample) > 0.1) {
					sample = tempSample;
					lastKinectSample = sample;
					this.guiTextUpperLocal = "";
				}
				else {
					this.guiTextUpperLocal = "Not enough hand movement.";
				}
			}
		}
		return sample;
		
		
	}
	
	private void CalculateTransformation()
	{
		if (samples_Kinect1.Count != numberOfSamplesTaken || samples_Kinect2.Count != numberOfSamplesTaken)
		{
			Debug.LogError("Mismatch in sample list lengths!");
		}
		
		Matrix kinect2Matrix;
		Matrix kinect1Matrix;
		
		kinect2Matrix = Matrix.Zeros (samples_Kinect2.Count, 4);
		kinect1Matrix = Matrix.Zeros (samples_Kinect1.Count, 3);
		
		for (int i = 1; i <= samples_Kinect2.Count; i++) {
			kinect2Matrix [i, 1] = new Complex (samples_Kinect2 [i - 1].x);
			kinect2Matrix [i, 2] = new Complex (samples_Kinect2 [i - 1].y);
			kinect2Matrix [i, 3] = new Complex (samples_Kinect2 [i - 1].z);
			kinect2Matrix [i, 4] = new Complex (1.0f);
		}
		for (int i = 1; i <= samples_Kinect1.Count; i++) {
			kinect1Matrix [i, 1] = new Complex (samples_Kinect1 [i - 1].x);
			kinect1Matrix [i, 2] = new Complex (samples_Kinect1 [i - 1].y);
			kinect1Matrix [i, 3] = new Complex (samples_Kinect1 [i - 1].z);
		}
		
		//perform a matrix solve Ax = B. We have to get transposes and inverses because moveMatrix isn't square
		//the solution is the same with (A^T)Ax = (A^T)B -> x = ((A^T)A)'(A^T)B
		Matrix transformMatrixSolution = (kinect2Matrix.Transpose() * kinect2Matrix).Inverse() * kinect2Matrix.Transpose() * kinect1Matrix;
		
		Matrix error = kinect2Matrix * transformMatrixSolution - kinect1Matrix;
		
		transformMatrixSolution = transformMatrixSolution.Transpose();
		
		Debug.Log(transformMatrixSolution);
		Debug.Log(error);
		
		List<Vector3> orthogonalVectors = MathUtil.Orthonormalize(
			MathUtil.ExtractRotationVectors(
			MathUtil.MatrixToMatrix4x4(transformMatrixSolution)
			)
			);
		rotationMatrix = CreateRotationMatrix(orthogonalVectors);
		Debug.Log(rotationMatrix);
		
		transformMatrix = MathUtil.MatrixToMatrix4x4(transformMatrixSolution);//CreateTransformMatrix(transformMatrixSolution);
		Debug.Log(transformMatrix);
		
		// TODO: Set floor normal for Kinect 1 and Kinect 2
		UpdateFloorNormal();
		
		coordinateSystem.SetDeviceToRootTransforms(rotationMatrix, transformMatrix);
		coordinateSystem.SaveTransformData(xmlFilename,RUISDevice.Kinect_2, RUISDevice.Kinect_1); 
		coordinateSystem.SaveFloorData(xmlFilename,RUISDevice.Kinect_1, kinect1PitchRotation, kinect1DistanceFromFloor);
		coordinateSystem.SaveFloorData(xmlFilename,RUISDevice.Kinect_2, kinect2PitchRotation, kinect2DistanceFromFloor);
		                               
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
	
	private void UpdateFloorNormal()
	{
		// TODO: For Kinect 1 and Kinect 2
		
		coordinateSystem.ResetFloorNormal();
		
		OpenNI.Plane3D floor = sceneAnalyzer.Floor;
		Vector3 oneFloorNormal = new Vector3(floor.Normal.X, floor.Normal.Y, floor.Normal.Z).normalized; // Kinect 1 floor normal
		Vector3 newFloorPosition = (new Vector3(floor.Point.X, floor.Point.Y, floor.Point.Z))*RUISCoordinateSystem.kinectToUnityScale; 
		
		//Project the position of the kinect camera onto the floor
		//http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
		//http://en.wikipedia.org/wiki/Plane_(geometry)
		float d = oneFloorNormal.x * newFloorPosition.x + oneFloorNormal.y * newFloorPosition.y + oneFloorNormal.z * newFloorPosition.z;
		Vector3 closestFloorPointToKinect1 = new Vector3(oneFloorNormal.x, oneFloorNormal.y, oneFloorNormal.z);
		closestFloorPointToKinect1 = (closestFloorPointToKinect1 * d) / closestFloorPointToKinect1.sqrMagnitude;
		
		//transform the point from Kinect's coordinate system rotation to Unity's rotation
		closestFloorPointToKinect1 = Quaternion.FromToRotation(oneFloorNormal, Vector3.up)  * closestFloorPointToKinect1;
		floorPlane.transform.position = closestFloorPointToKinect1;
		
		//show the tilt of the kinect camera on the kinect model
		//kinectModelObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, newFloorNormal);
		
		// TODO: Calculate Vector3 twoFloorNormal and closestFloorPointToKinect2
//		coordinateSystem.SetFloorNormal(twoFloorNormal);
//		coordinateSystem.SetDistanceFromFloor(closestFloorPointToKinect2.magnitude);
		
		Quaternion kinectFloorRotator = Quaternion.identity;
		kinectFloorRotator.SetFromToRotation(Vector3.up, oneFloorNormal);
		kinect1PitchRotation = kinectFloorRotator;
		
		kinectFloorRotator = Quaternion.identity;
//		kinectFloorRotator.SetFromToRotation(Vector3.up, twoFloorNormal); // TODO: Uncomment
		kinect2PitchRotation = kinectFloorRotator;
		
		kinect1DistanceFromFloor = closestFloorPointToKinect1.magnitude;
		//kinect2DistanceFromFloor = closestFloorPointToKinect2.magnitude;
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
}
