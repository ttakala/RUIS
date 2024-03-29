/*****************************************************************************

Content    :   The main class used for coordinate system transforms between different input systems and Unity
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2018 Tuukka Takala, Mikael Matveinen.
               All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using Kinect = Windows.Kinect;

//using Ovr;

public class RUISCoordinateSystem : MonoBehaviour
{
	[System.Serializable]
	public class DeviceCoordinateConversion
	{
		[Tooltip("Multiplier for the raw position input values from the tracked Custom device. Keep this value as 1 if one unit is one meter.")]
		public float unitScale = 1;
		[Tooltip("Negate the raw X-position values (i.e. flip X-axis) for the tracked Custom device. In most cases you leave this disabled.")]
		public bool xPosNegate = false;
		[Tooltip("Negate the raw Y-position values (i.e. flip Y-axis) for the tracked Custom device. In most cases you leave this disabled.")]
		public bool yPosNegate = false;
		[Tooltip("Negate the raw Z-position values (i.e. flip Z-axis) for the tracked Custom device. In most cases you leave this disabled.")]
		public bool zPosNegate = false;
		[Space]
		[Tooltip("Inverse the raw rotation quaternion from the tracked Custom device. In most cases you leave this disabled.")]
		public bool rotationInverse = false;
		[Tooltip("Negate the raw rotation quaternion's X-component value for the tracked Custom device. In most cases you leave this disabled.")]
		public bool xRotNegate = false;
		[Tooltip("Negate the raw rotation quaternion's Y-component value for the tracked Custom device. In most cases you leave this disabled.")]
		public bool yRotNegate = false;
		[Tooltip("Negate the raw rotation quaternion's Z-component value for the tracked Custom device. In most cases you leave this disabled.")]
		public bool zRotNegate = false;
		[Tooltip("Negate the raw rotation quaternion's W-component value for the tracked Custom device. In most cases you leave this disabled.")]
		public bool wRotNegate = false;
	};

	public string coordinateXmlFile = "calibration.xml";
	public TextAsset coordinateSchema;
	public bool loadFromXML = true;

	public const float kinectToUnityScale = 0.001f;
	public const float moveToUnityScale = 0.1f;
    
	private Matrix4x4 deviceToRootTransform = Matrix4x4.identity;
    
	public float yawOffset = 0;
	
	public bool applyToRootCoordinates = true;
	public bool switchToAvailableDevice = true;
	public bool setKinectOriginToFloor = false;

	public Vector3 positionOffset = Vector3.zero;
    
	public Dictionary<string, Vector3> RUISCalibrationResultsInVector3 = new Dictionary<string, Vector3>();
	public Dictionary<string, Quaternion> RUISCalibrationResultsInQuaternion = new Dictionary<string, Quaternion>();
	public Dictionary<string, Matrix4x4> RUISCalibrationResultsIn4x4Matrix = new Dictionary<string, Matrix4x4>();
	
	public Dictionary<RUISDevice, Quaternion> RUISCalibrationResultsFloorPitchRotation = new Dictionary<RUISDevice, Quaternion>();
	public Dictionary<RUISDevice, float> RUISCalibrationResultsDistanceFromFloor = new Dictionary<RUISDevice, float>();
	
	public RUISDevice rootDevice;

	RUISInputManager inputManager;

	void Awake()
	{
		string[] names = System.Enum.GetNames(typeof(RUISDevice));
		foreach(string device in names)
		{
			RUISDevice device1Enum = (RUISDevice)System.Enum.Parse(typeof(RUISDevice), device, true);
			RUISCalibrationResultsFloorPitchRotation[device1Enum] = Quaternion.identity;
			RUISCalibrationResultsDistanceFromFloor[device1Enum] = 0.0f;
			foreach(string device2 in names)
			{
				if(device != device2)
				{
					string devicePairString = device + "-" + device2;
					RUISCalibrationResultsInVector3[devicePairString] = new Vector3(0, 0, 0);
					RUISCalibrationResultsInQuaternion[devicePairString] = Quaternion.identity;
					RUISCalibrationResultsIn4x4Matrix[devicePairString] = Matrix4x4.identity;
				}
			}
		}
		
		if(loadFromXML)
		{
			if(!LoadMultiXML(coordinateXmlFile))
			{
				createExampleXML(coordinateXmlFile);
			}
		}

		inputManager = FindObjectOfType<RUISInputManager>();
		if(switchToAvailableDevice && inputManager)
		{
			bool needToSwitch = false;
			RUISDevice previousDevice = rootDevice;

			switch(rootDevice)
			{
			case RUISDevice.Kinect_1:
				if(!inputManager.enableKinect)
					needToSwitch = true;
				break;
			case RUISDevice.Kinect_2:
				if(!inputManager.enableKinect2)
					needToSwitch = true;
				break;
			case RUISDevice.UnityXR:
				if(!RUISDisplayManager.IsHmdPresent())
					needToSwitch = true;
				break;
			case RUISDevice.OpenVR: // If OpenVR can't accessed AND a HMD can't be detected
				if(!RUISDisplayManager.IsOpenVrAccessible() && !RUISDisplayManager.IsHmdPresent())
					needToSwitch = true;
				break;
			}

			if(needToSwitch)
			{
				// Try to determine if Kinect2 can be used (because this method is run before RUISInputManager can disable enableKinect2)
				bool kinect2FoundBySystem = false;
				if(inputManager.enableKinect2)
				{
					try
					{
						Kinect2SourceManager kinect2SourceManager = FindObjectOfType(typeof(Kinect2SourceManager)) as Kinect2SourceManager;
						if(kinect2SourceManager != null && kinect2SourceManager.GetSensor() != null && kinect2SourceManager.GetSensor().IsOpen)
						{
							// IsOpen seems to return false mostly if Kinect 2 drivers are not installed?
							//					Debug.Log("Kinect 2 was detected by the system.");
							kinect2FoundBySystem = true;
						}
					}
					catch
					{}
				}

				if(RUISDisplayManager.IsOpenVrAccessible() && RUISDisplayManager.IsHmdPositionTrackable())
					rootDevice = RUISDevice.OpenVR;
				else if(inputManager.enableKinect2 && kinect2FoundBySystem)
					rootDevice = RUISDevice.Kinect_2;
				else if(inputManager.enableKinect)
					rootDevice = RUISDevice.Kinect_1;
				else if(RUISDisplayManager.IsHmdPositionTrackable()) // *** TODO this is already above, distinguish these two cases
					rootDevice = RUISDevice.UnityXR;

				if(rootDevice != previousDevice)
				{
					if(previousDevice == RUISDevice.OpenVR)
						Debug.LogWarning("Switched 'Master Coordinate System Sensor' from " + previousDevice + " to " + rootDevice + " "
						+ "because OpenVR could not be accessed! Is SteamVR installed?");
					else
						Debug.LogWarning("Switched 'Master Coordinate System Sensor' from " + previousDevice + " to " + rootDevice + " "
						+ "because the former was not enabled in " + typeof(RUISInputManager) + " while the latter was!");
				}
			}
		}
	}

	private void createExampleXML(string filename)
	{
		Vector3 exampleFloorNormal = new Vector3(0, 1, 0);
		float exampleDistanceFromFloor = 0.0f;
		
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
		XmlNode calibrationMatrixElement = xmlDoc.CreateElement("ns2", "RUISPairwiseCalibration", "http://ruisystem.net/RUISPairwiseCalibration");
		xmlDoc.AppendChild(calibrationMatrixElement);
		
		XmlElement groupElementTransforms = xmlDoc.CreateElement("Transforms");
		XmlElement groupElementFloorData = xmlDoc.CreateElement("FloorData");
		
		calibrationMatrixElement.AppendChild(groupElementTransforms);
		calibrationMatrixElement.AppendChild(groupElementFloorData);
		
		
		XmlElement transformWrapperElement = xmlDoc.CreateElement("OpenVR-Kinect_2");
		groupElementTransforms.AppendChild(transformWrapperElement);
		
		XmlElement translateElement = xmlDoc.CreateElement("translate");
		translateElement.SetAttribute("x", deviceToRootTransform[0, 3].ToString());
		translateElement.SetAttribute("y", deviceToRootTransform[1, 3].ToString());
		translateElement.SetAttribute("z", deviceToRootTransform[2, 3].ToString());
		
		transformWrapperElement.AppendChild(translateElement);
		
		XmlElement rotateElement = xmlDoc.CreateElement("rotate");
		rotateElement.SetAttribute("r00", deviceToRootTransform[0, 0].ToString());
		rotateElement.SetAttribute("r01", deviceToRootTransform[0, 1].ToString());
		rotateElement.SetAttribute("r02", deviceToRootTransform[0, 2].ToString());
		rotateElement.SetAttribute("r10", deviceToRootTransform[1, 0].ToString());
		rotateElement.SetAttribute("r11", deviceToRootTransform[1, 1].ToString());
		rotateElement.SetAttribute("r12", deviceToRootTransform[1, 2].ToString());
		rotateElement.SetAttribute("r20", deviceToRootTransform[2, 0].ToString());
		rotateElement.SetAttribute("r21", deviceToRootTransform[2, 1].ToString());
		rotateElement.SetAttribute("r22", deviceToRootTransform[2, 2].ToString());
		
		transformWrapperElement.AppendChild(rotateElement);
		
		
		
		XmlElement floorDataWrapperElement = xmlDoc.CreateElement("Kinect_2");
		groupElementFloorData.AppendChild(floorDataWrapperElement);
		
		XmlElement kinectFloorNormalElement = xmlDoc.CreateElement("floorNormal");
		kinectFloorNormalElement.SetAttribute("x", exampleFloorNormal.x.ToString());
		kinectFloorNormalElement.SetAttribute("y", exampleFloorNormal.y.ToString());
		kinectFloorNormalElement.SetAttribute("z", exampleFloorNormal.z.ToString());
		
		floorDataWrapperElement.AppendChild(kinectFloorNormalElement);
		
		XmlElement kinectDistanceFromFloorElement = xmlDoc.CreateElement("distanceFromFloor");
		kinectDistanceFromFloorElement.SetAttribute("value", exampleDistanceFromFloor.ToString());
		
		floorDataWrapperElement.AppendChild(kinectDistanceFromFloorElement);
		
		FileStream xmlFileStream = File.Open(filename, FileMode.Create);
		StreamWriter streamWriter = new StreamWriter(xmlFileStream);
		xmlDoc.Save(streamWriter);
		streamWriter.Flush();
		streamWriter.Close();
		xmlFileStream.Close();
		
	}

	public bool LoadMultiXML(string filename)
	{
		
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, coordinateSchema);
		if(xmlDoc != null)
		{
			foreach(XmlNode node in xmlDoc.DocumentElement.ChildNodes)
			{	
				Vector3 vector3 = new Vector3(0, 0, 0);
				Matrix4x4 device1ToDevice2Transform = new Matrix4x4();
				Quaternion quaternion = new Quaternion();
				
				if(node.Name == "Transforms")
				{
					foreach(XmlNode groupElement in node.ChildNodes)
					{
						XmlNode translationElement = groupElement.SelectSingleNode("translate");
						float x = float.Parse(translationElement.Attributes["x"].Value);
						float y = float.Parse(translationElement.Attributes["y"].Value);
						float z = float.Parse(translationElement.Attributes["z"].Value);
						vector3 = new Vector3(x, y, z);
						device1ToDevice2Transform.SetColumn(3, new Vector4(x, y, z, 1.0f));
			
						XmlNode rotationElement = groupElement.SelectSingleNode("rotate");
						
						device1ToDevice2Transform.m00 = float.Parse(rotationElement.Attributes["r00"].Value);
						device1ToDevice2Transform.m01 = float.Parse(rotationElement.Attributes["r01"].Value);
						device1ToDevice2Transform.m02 = float.Parse(rotationElement.Attributes["r02"].Value);
						device1ToDevice2Transform.m10 = float.Parse(rotationElement.Attributes["r10"].Value);
						device1ToDevice2Transform.m11 = float.Parse(rotationElement.Attributes["r11"].Value);
						device1ToDevice2Transform.m12 = float.Parse(rotationElement.Attributes["r12"].Value);
						device1ToDevice2Transform.m20 = float.Parse(rotationElement.Attributes["r20"].Value);
						device1ToDevice2Transform.m21 = float.Parse(rotationElement.Attributes["r21"].Value);
						device1ToDevice2Transform.m22 = float.Parse(rotationElement.Attributes["r22"].Value);
						
						List<Vector3> rotationVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(device1ToDevice2Transform));
						Matrix4x4 rotationMatrix = new Matrix4x4();
						rotationMatrix.SetColumn(0, rotationVectors[0]);
						rotationMatrix.SetColumn(1, rotationVectors[1]);
						rotationMatrix.SetColumn(2, rotationVectors[2]);
						
						quaternion = MathUtil.QuaternionFromMatrix(rotationMatrix); 
						
						RUISCalibrationResultsInVector3[groupElement.Name] = vector3;
						RUISCalibrationResultsIn4x4Matrix[groupElement.Name] = device1ToDevice2Transform;
						RUISCalibrationResultsInQuaternion[groupElement.Name] = quaternion;		
						
						// Inverses
						string[] parts = groupElement.Name.Split('-');
						string inverseName = parts[1] + "-" + parts[0];
						RUISCalibrationResultsInVector3[inverseName] = -vector3;
						RUISCalibrationResultsIn4x4Matrix[inverseName] = device1ToDevice2Transform.inverse;
						RUISCalibrationResultsInQuaternion[inverseName] = Quaternion.Inverse(quaternion);		
					}
				}
				
				if(node.Name == "FloorData")
				{
					foreach(XmlNode groupElement in node.ChildNodes)
					{
						Quaternion floorPitchRotation = Quaternion.identity;
						float distanceFromFloor = 0;
						foreach(XmlNode element in groupElement.ChildNodes)
						{
							switch(element.Name)
							{
							case "floorNormal":
								float xValue = float.Parse(element.Attributes["x"].Value);
								float yValue = float.Parse(element.Attributes["y"].Value);
								float zValue = float.Parse(element.Attributes["z"].Value);
								floorPitchRotation = Quaternion.Inverse(Quaternion.FromToRotation(new Vector3(xValue, yValue, zValue), Vector3.up));
								break;
								
							case "distanceFromFloor":
								distanceFromFloor = float.Parse(element.Attributes["value"].Value);
								break;		
							}
						}	
						
						RUISCalibrationResultsFloorPitchRotation[(RUISDevice)System.Enum.Parse(typeof(RUISDevice), groupElement.Name, true)] = floorPitchRotation;
						RUISCalibrationResultsDistanceFromFloor[(RUISDevice)System.Enum.Parse(typeof(RUISDevice), groupElement.Name, true)] = distanceFromFloor;		
					}
				}
				
			}
			
		}
		else
			return false;	
		/*
		// For debug
		foreach (string key in RUISCalibrationResultsInVector3.Keys)
		{
			print("Key: " + key + ": Value: " + RUISCalibrationResultsInVector3[key].ToString());
		}
		foreach (string key in RUISCalibrationResultsIn4x4Matrix.Keys)
		{
			print("Key: " + key + ": Value: " + RUISCalibrationResultsIn4x4Matrix[key].ToString());
		}
		foreach (string key in RUISCalibrationResultsInVector3.Keys)
		{
			print("Key: " + key + ": Value: " + RUISCalibrationResultsInQuaternion[key].ToString());
		}
		foreach (RUISDevice key in RUISCalibrationResultsFloorNormal.Keys)
		{
			print("Key: " + key.ToString() + ": Value: " + RUISCalibrationResultsFloorNormal[key].ToString());
		}
		foreach (RUISDevice key in RUISCalibrationResultsDistanceFromFloor.Keys)
		{
			print("Key: " + key.ToString() + ": Value: " + RUISCalibrationResultsDistanceFromFloor[key].ToString());
		}
		*/
		return true;
	}

	public void SaveTransformDataToXML(string filename, RUISDevice device1, RUISDevice device2)
	{	
		string wrapperElementName = device1.ToString() + "-" + device2.ToString();
	
		XmlNode calibrationMatrixElement;
		
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, coordinateSchema);
		XmlNode groupElement;
		
		if(xmlDoc != null)
		{
			calibrationMatrixElement = xmlDoc.DocumentElement;
			groupElement = calibrationMatrixElement.SelectSingleNode("Transforms");
			if(groupElement == null)
				groupElement = xmlDoc.CreateElement("Transforms");
		}
		else
		{
			xmlDoc = new XmlDocument();
			xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			calibrationMatrixElement = xmlDoc.CreateElement("ns2", "RUISClibrationMatrix", "http://ruisystem.net/m2k");
			xmlDoc.AppendChild(calibrationMatrixElement);
			groupElement = xmlDoc.CreateElement("Transforms");
		}
		calibrationMatrixElement.AppendChild(groupElement);
		
		XmlElement wrapperElement = xmlDoc.CreateElement(wrapperElementName);
		groupElement.AppendChild(wrapperElement);
		
		XmlElement translateElement = xmlDoc.CreateElement("translate");
		translateElement.SetAttribute("x", deviceToRootTransform[0, 3].ToString());
		translateElement.SetAttribute("y", deviceToRootTransform[1, 3].ToString());
		translateElement.SetAttribute("z", deviceToRootTransform[2, 3].ToString());
		
		wrapperElement.AppendChild(translateElement);
		
		XmlElement rotateElement = xmlDoc.CreateElement("rotate");
		rotateElement.SetAttribute("r00", deviceToRootTransform[0, 0].ToString());
		rotateElement.SetAttribute("r01", deviceToRootTransform[0, 1].ToString());
		rotateElement.SetAttribute("r02", deviceToRootTransform[0, 2].ToString());
		rotateElement.SetAttribute("r10", deviceToRootTransform[1, 0].ToString());
		rotateElement.SetAttribute("r11", deviceToRootTransform[1, 1].ToString());
		rotateElement.SetAttribute("r12", deviceToRootTransform[1, 2].ToString());
		rotateElement.SetAttribute("r20", deviceToRootTransform[2, 0].ToString());
		rotateElement.SetAttribute("r21", deviceToRootTransform[2, 1].ToString());
		rotateElement.SetAttribute("r22", deviceToRootTransform[2, 2].ToString());
		
		wrapperElement.AppendChild(rotateElement);
		
		XmlNode groupNode = xmlDoc.DocumentElement.SelectSingleNode("Transforms");
		XmlNode testNode = groupNode.SelectSingleNode(wrapperElementName);
		// Element not found
		if(testNode == null)
		{ 	
			groupNode.AppendChild(wrapperElement);
		}
		else
		{
			// Element already exists
			var oldElem = testNode;
			groupNode.ReplaceChild(wrapperElement, oldElem);
		
		}
		
		FileStream xmlFileStream = File.Open(filename, FileMode.Create);
		StreamWriter streamWriter = new StreamWriter(xmlFileStream);
		xmlDoc.Save(streamWriter);
		streamWriter.Flush();
		streamWriter.Close();
		xmlFileStream.Close();
	}

	public void SaveFloorData(string filename, RUISDevice device, Vector3 normal, float distance)
	{	
		string wrapperElementName = device.ToString();
	
		XmlNode calibrationMatrixElement;
		
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, coordinateSchema);
		XmlNode groupElement;
		
		if(xmlDoc != null)
		{
			calibrationMatrixElement = xmlDoc.DocumentElement;
			groupElement = calibrationMatrixElement.SelectSingleNode("FloorData");
			if(groupElement == null)
				groupElement = xmlDoc.CreateElement("FloorData");
		}
		else
		{
			xmlDoc = new XmlDocument();
			xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			calibrationMatrixElement = xmlDoc.CreateElement("ns2", "RUISClibrationMatrix", "http://ruisystem.net/m2k");
			xmlDoc.AppendChild(calibrationMatrixElement);
			groupElement = xmlDoc.CreateElement("FloorData");
		}
		calibrationMatrixElement.AppendChild(groupElement);
		
		XmlElement wrapperElement = xmlDoc.CreateElement(wrapperElementName);
		groupElement.AppendChild(wrapperElement);
		
		XmlElement deviceFloorNormalElement = xmlDoc.CreateElement("floorNormal");
		
		deviceFloorNormalElement.SetAttribute("x", normal.x.ToString());
		deviceFloorNormalElement.SetAttribute("y", normal.y.ToString());
		deviceFloorNormalElement.SetAttribute("z", normal.z.ToString());
		wrapperElement.AppendChild(deviceFloorNormalElement);
		
		XmlElement deviceDistanceFromFloorElement = xmlDoc.CreateElement("distanceFromFloor");
		deviceDistanceFromFloorElement.SetAttribute("value", distance.ToString());
		
		wrapperElement.AppendChild(deviceDistanceFromFloorElement);
		
		XmlNode groupNode = xmlDoc.DocumentElement.SelectSingleNode("FloorData");
		XmlNode testNode = groupNode.SelectSingleNode(wrapperElementName);
		// Element not found
		if(testNode == null)
		{ 	
			groupNode.AppendChild(wrapperElement);
		}
		else
		{
			// Element already exists
			var oldElem = testNode;
			groupNode.ReplaceChild(wrapperElement, oldElem);
			
		}
		
		FileStream xmlFileStream = File.Open(filename, FileMode.Create);
		StreamWriter streamWriter = new StreamWriter(xmlFileStream);
		xmlDoc.Save(streamWriter);
		streamWriter.Flush();
		streamWriter.Close();
		xmlFileStream.Close();
	}

	public void SetFloorNormal(Vector3 newFloorNormal, RUISDevice floorDetectingDevice)
	{
    	
		Quaternion kinectFloorRotator = Quaternion.identity;
		kinectFloorRotator.SetFromToRotation(newFloorNormal, Vector3.up);
		kinectFloorRotator = Quaternion.Inverse(kinectFloorRotator);
		
		switch(floorDetectingDevice)
		{
		case RUISDevice.Kinect_1:
			RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_1] = kinectFloorRotator;
			break;
		case RUISDevice.Kinect_2:
			RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] = kinectFloorRotator;
			break;
		default:
			Debug.LogWarning("Currently floor normal detection with " + floorDetectingDevice.ToString() + " is not supported!");
			break;
		}
	}

	public void ResetFloorNormal(RUISDevice floorDetectingDevice)
	{
		RUISCalibrationResultsFloorPitchRotation[floorDetectingDevice] = Quaternion.identity;
	}

	public void SetDistanceFromFloor(float distance, RUISDevice floorDetectingDevice)
	{
		switch(floorDetectingDevice)
		{
		case RUISDevice.Kinect_1:
			RUISCalibrationResultsDistanceFromFloor[RUISDevice.Kinect_1] = distance;
			break;
		case RUISDevice.Kinect_2:
			RUISCalibrationResultsDistanceFromFloor[RUISDevice.Kinect_2] = distance;
			break;
		default:
			Debug.LogWarning("Currently floor distance detection with " + floorDetectingDevice.ToString() + " is not supported!");
			break;
		}
	}

	public void ResetDistanceFromFloor(RUISDevice floorDetectingDevice)
	{
		RUISCalibrationResultsDistanceFromFloor[floorDetectingDevice] = 0;
	}
	
	// TODO: Implement below for all devices
	//	public float GetDistanceFromFloor(RUISDevice device)
	//	{
	//		return distanceFromFloor;
	//	}

	// TODO: Pass the transform matrix as an argument to SaveTransformDataToXML(), delete deviceToRootTransform parameter
	public void SetDeviceToRootTransforms(Matrix4x4 transformMatrix)
	{
		deviceToRootTransform = transformMatrix;
	}
	
	
	/*
	*	Device specific functions
	*/

	/*
	*	PSMove
	*/
	public Vector3 ConvertRawPSMoveLocation(Vector3 position)
	{
		//flip the z coordinate to get into unity's coordinate system
		Vector3 newPosition = new Vector3(position.x, position.y, -position.z);

		newPosition *= moveToUnityScale;

		return newPosition;
	}

	public Quaternion ConvertRawPSMoveRotation(Quaternion rotation)
	{
		Quaternion newRotation = rotation;
		
		//this turns the quaternion into the correct direction
		newRotation.x = -newRotation.x;
		newRotation.y = -newRotation.y;
		
		return newRotation;
	}
	
	/*
	*	Kinect 1
	*/
	public Vector3 ConvertRawKinectLocation(OpenNI.Point3D position)
	{
		//we have to flip the z axis to get into unity's coordinate system
		Vector3 newPosition = Vector3.zero;
		newPosition.x = position.X;
		newPosition.y = position.Y;
		newPosition.z = -position.Z;
		
		newPosition = kinectToUnityScale * newPosition;
		
		return newPosition;
	}

	public Quaternion ConvertRawKinectRotation(OpenNI.SkeletonJointOrientation rotation)
	{
		Vector3 up = new Vector3(rotation.Y1, rotation.Y2, rotation.Y3);
		Vector3 forward = new Vector3(rotation.Z1, rotation.Z2, rotation.Z3);

		if(up == Vector3.zero || forward == Vector3.zero)
			return Quaternion.identity;

		Quaternion newRotation = Quaternion.LookRotation(forward, up);

		newRotation.x = -newRotation.x;
		newRotation.y = -newRotation.y;

		return newRotation;
	}
    
	/*
	 * 	Kinect 2
	 */
	public Vector3 ConvertRawKinect2Location(Vector3 position)
	{
		Vector3 newPosition = Vector3.zero;
		newPosition.x = position.x;
		newPosition.y = position.y;
		newPosition.z = -position.z;
		
		return newPosition;
	}

	public Quaternion ConvertRawKinect2Rotation(Quaternion rotation)
	{
		Quaternion newRotation = rotation;
		newRotation.x = -rotation.x;
		newRotation.y = -rotation.y;
		
		return newRotation;
	}
	
	/*
	 * 	Head-mounted display wrapper methods
	 */
	/// <summary>
	/// Get head-mounted display position in Unity coordinate system
	/// </summary>
	public Vector3 GetHmdRawPosition()
	{
		//		if(OVRManager.display != null) //06to08
		//		{
		//			return ConvertRawOculusDK2Location(OVRManager.display.GetHeadPose().position); //06to08
		//		}
		//		else
		//			return Vector3.zero;
		return UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head); //06to08
	}

	/// <summary>
	/// Get head-mounted display rotation in Unity coordinate system
	/// </summary>
	public Quaternion GetHmdRawRotation()
	{
//		if(OVRManager.display != null)
//		{
//			return OVRManager.display.GetHeadPose().orientation; //06to08
//		}
//		else 
//			return Quaternion.identity;

		// HACK TODO check that this works
		return UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head); // HACK TODO if this doesn't work for major HMDs, add wrapper
	}

	/// <summary>
	/// Head-mounted display coordinate system's orientation around master coordinate system's Y-axis
	/// </summary>
	public Quaternion GetHmdCoordinateSystemYaw(RUISDevice device)
	{
		Quaternion convertedRotation = ConvertRotation(Quaternion.identity, device);

		Vector3 projected = convertedRotation * Vector3.forward;
		projected.Set(projected.x, 0, projected.z);

		// Make sure that the projection is not pointing too much up
		if(projected.sqrMagnitude > 0.001)
			return Quaternion.LookRotation(projected);
		else // *** HACK TODO The HMD coordinate system view axis is parallel with the master coordinate system's Y-axis!
			return Quaternion.identity;
	}

	/// <summary>
	/// Get head-mounted display rotation in master coordinate system.
	/// This is different from ConvertRotation(GetHmdRawRotation(), RUISDevice.OpenVR), because it does not
	/// include pitch and roll difference between OpenVR frame and the master coordinate system frame,
	/// which would in several cases tilt the world when viewed from the head-mounted display.
	/// </summary>
	public Quaternion GetHmdOrientationInMasterFrame()
	{
		// *** HACK TODO this is the only HMD wrapper method that is currently hard-coded to use OpenVR
		return GetHmdCoordinateSystemYaw(RUISDevice.OpenVR) * GetHmdRawRotation();
	}

	static public Vector3 ConvertRawLocation(Vector3 rawLocation, DeviceCoordinateConversion conversion)
	{
		if(conversion != null)
		{
			rawLocation.Set((conversion.xPosNegate?-1:1)*rawLocation.x, 
							(conversion.yPosNegate?-1:1)*rawLocation.y, 
							(conversion.zPosNegate?-1:1)*rawLocation.z );
			return conversion.unitScale*rawLocation;
		}
		else
			return rawLocation;
	}

	static public Quaternion ConvertRawRotation(Quaternion rawRotation, DeviceCoordinateConversion conversion)
	{
		if(conversion != null)
		{
			if(conversion.rotationInverse)
				rawRotation = Quaternion.Inverse(rawRotation);
			rawRotation.Set((conversion.xRotNegate?-1:1)*rawRotation.x, 
							(conversion.yRotNegate?-1:1)*rawRotation.y, 
							(conversion.zRotNegate?-1:1)*rawRotation.z, 
							(conversion.wRotNegate?-1:1)*rawRotation.w );
			return rawRotation;
		}
		else
			return rawRotation;
	}

	/*
	 * 	Oculus Rift - Lots of obsolete functions, bits of which might be needed in the future if Oculus Unity Utilities support will be added
	 */
	// Oculus positional tracking camera's coordinate system origin in Unity coordinate system
//	public Vector3 GetOculusCameraOriginRaw()
//	{
////		if (OVRManager.capiHmd != null) 
////		{
////			Vector3 currentOvrCameraPose = OVRManager.capiHmd.GetTrackingState().CameraPose.Position.ToVector3 (); //06to08
////			
////			return Quaternion.Inverse(GetOculusCameraOrientationRaw())*currentOvrCameraPose;
////		} else //06to08
//		return Vector3.zero; // HACK remove this method
//	}

	// Oculus positional tracking camera's coordinate system origin in master coordinate system
//	public Vector3 GetOculusCameraOrigin()
//	{
//		return ConvertLocation(GetOculusCameraOriginRaw(), RUISDevice.Oculus_DK2);
//	}
	
	// Oculus positional tracking camera's coordinate system orientation in Unity coordinates
//	public Quaternion GetOculusCameraOrientationRaw()
//	{
////		if (OVRManager.capiHmd != null) 
////		{
////			return OVRManager.capiHmd.GetTrackingState().CameraPose.Orientation.ToQuaternion(); //06to08
////		} else
//		return Quaternion.identity; // HACK remove this method
//	}

//	public Vector3 ConvertRawOculusDK2Location(Vector3 position)
//	{
////		Vector3 currentcameraPosition = Vector3.zero;
////		if (OVRManager.capiHmd != null)
////			currentcameraPosition = OVRManager.capiHmd.GetTrackingState().CameraPose.Position.ToVector3(); //06to08
////		return Quaternion.Inverse(GetOculusCameraOrientationRaw())*(position - currentcameraPosition); //06to08
//
//		return UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head); // HACK TODO if this doesn't work for major HMDs, add wrapper
//	}

	/// <summary>
	/// Convert velocity or angular velocity obtained with a certain device to master coordinate system, apply yaw offset, and apply Kinect pitch correction
	/// </summary>
	public Vector3 ConvertVelocity(Vector3 velocity, RUISDevice device)
	{
		Vector3 newVelocity = velocity;

		if(device == RUISDevice.None)
			return velocity;
		
		if(applyToRootCoordinates && rootDevice != device)
		{
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			newVelocity = RUISCalibrationResultsInQuaternion[devicePairString] * newVelocity;
		}

		// Apply floor pitch rotation (which is identity to anything else than Kinect 1/2)
		if(applyToRootCoordinates || device == rootDevice)
			newVelocity = RUISCalibrationResultsFloorPitchRotation[rootDevice] * newVelocity;
		else
		{
			if(device == RUISDevice.Kinect_2)
				newVelocity = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] * newVelocity;
			else if(device == RUISDevice.Kinect_1)
				newVelocity = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_1] * newVelocity;
		}

		newVelocity = Quaternion.Euler(0, yawOffset, 0) * newVelocity;

		return newVelocity;
	}

	/// <summary>
	/// Convert location obtained with a certain device to master coordinate system, apply position offset, and set Kinect origin to floor if applicable
	/// </summary>
	public Vector3 ConvertLocation(Vector3 inputLocation, RUISDevice device)
	{
		Vector3 outputLocation = inputLocation;

		if(device == RUISDevice.None)
			return inputLocation;
		
		// Transform location into master coordinate system
		if(applyToRootCoordinates && rootDevice != device)
		{
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			outputLocation = RUISCalibrationResultsIn4x4Matrix[devicePairString].MultiplyPoint3x4(outputLocation);
//			outputLocation = RUISCalibrationResultsInQuaternion[devicePairString] * outputLocation;
		}

		// Apply yaw offset and floor pitch rotation (which is identity to anything else than Kinect 1/2)
		if(applyToRootCoordinates || device == rootDevice)
			outputLocation = Quaternion.Euler(0, yawOffset, 0) * RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputLocation;
		else
		{
			if(device == RUISDevice.Kinect_2)
				outputLocation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] * outputLocation;
			else if(device == RUISDevice.Kinect_1)
				outputLocation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_1] * outputLocation;
		}

		// Set Kinect 1/2 origin to floor
		if(setKinectOriginToFloor)
		{
			if(applyToRootCoordinates || device == rootDevice)
				outputLocation.y += RUISCalibrationResultsDistanceFromFloor[rootDevice];
			else
			{
				if(device == RUISDevice.Kinect_2)
					outputLocation.y += RUISCalibrationResultsDistanceFromFloor[RUISDevice.Kinect_2];
				else if(device == RUISDevice.Kinect_1)
					outputLocation.y += RUISCalibrationResultsDistanceFromFloor[RUISDevice.Kinect_1];
			}
		}

		// Position offset
		if(applyToRootCoordinates || device == rootDevice)
			outputLocation += positionOffset;
		
		return outputLocation;
	}

	/// <summary>
	/// Convert rotation obtained with a certain device to master coordinate system, apply yaw offset, and apply Kinect pitch correction
	/// </summary>
	public Quaternion ConvertRotation(Quaternion inputRotation, RUISDevice device)
	{
		Quaternion outputRotation = inputRotation;

		if(device == RUISDevice.None)
			return inputRotation;

		if(applyToRootCoordinates && rootDevice != device)
		{
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			outputRotation = RUISCalibrationResultsInQuaternion[devicePairString] * outputRotation;
		}

		// Apply floor pitch rotation (which is identity to anything else than Kinect 1/2)
		if(applyToRootCoordinates || device == rootDevice)
			outputRotation = RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputRotation;
		else
		{
			if(device == RUISDevice.Kinect_2)
				outputRotation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] * outputRotation;
			else if(device == RUISDevice.Kinect_1)
				outputRotation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_1] * outputRotation;
		}
		
		// Apply yaw offset
		if(applyToRootCoordinates || device == rootDevice)
			outputRotation = Quaternion.Euler(0, yawOffset, 0) * outputRotation;

		return outputRotation;
	}

	/// <summary>
	/// Returns an approximation of Vector3 localScale (calculated from RUISCalibrationResultsIn4x4Matrix[devicePairString]),
	/// which can be used to compensate the scale difference between the master coordinate system frame and the argument device frame.
	/// </summary>
	public Vector3 ExtractLocalScale(RUISDevice device)
	{
		if(applyToRootCoordinates && rootDevice != device && device != RUISDevice.None)
		{
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			Matrix4x4 matrix = RUISCalibrationResultsIn4x4Matrix[devicePairString];

			// Extract new local scale
			return new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		}
		else
			return Vector3.one;
	}
	
}

