/*****************************************************************************

Content    :   The main class used for coordinate system transforms between different input systems and Unity
Authors    :   Tuukka Takala, Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using Kinect = Windows.Kinect;

public class RUISCoordinateSystem : MonoBehaviour
{
    public string coordinateXmlFile = "calibration.xml";
    public TextAsset coordinateSchema;
    public bool loadFromXML = true;

    public const float kinectToUnityScale = 0.001f;
    public const float moveToUnityScale = 0.1f;
    
	private Matrix4x4 deviceToRootTransform = Matrix4x4.identity;
    
	public float yawOffset = 0;
	
    public bool applyToRootCoordinates = true;
    public bool setKinectOriginToFloor = false;

    public Vector3 positionOffset = Vector3.zero;
    
	public Dictionary<string, Vector3> RUISCalibrationResultsInVector3 = new Dictionary<string, Vector3>();
	public Dictionary<string, Quaternion> RUISCalibrationResultsInQuaternion = new Dictionary<string, Quaternion>();
	public Dictionary<string, Matrix4x4> RUISCalibrationResultsIn4x4Matrix = new Dictionary<string, Matrix4x4>();
	
	public Dictionary<RUISDevice, Quaternion> RUISCalibrationResultsFloorPitchRotation = new Dictionary<RUISDevice, Quaternion>();
	public Dictionary<RUISDevice, float> RUISCalibrationResultsDistanceFromFloor = new Dictionary<RUISDevice, float>();
	
	public RUISDevice rootDevice; 
	
	void Awake() 
	{
		string[] names = System.Enum.GetNames( typeof( RUISDevice ) );
		foreach(string device in names) {
			RUISDevice device1Enum = (RUISDevice) System.Enum.Parse( typeof(RUISDevice), device, true );
			RUISCalibrationResultsFloorPitchRotation[device1Enum] = Quaternion.identity;
			RUISCalibrationResultsDistanceFromFloor[device1Enum] = 0.0f;
			foreach(string device2 in names) {
				if(device != device2) {
					string devicePairString = device + "-" + device2;
					RUISCalibrationResultsInVector3[devicePairString] = new Vector3(0,0,0);
					RUISCalibrationResultsInQuaternion[devicePairString] = Quaternion.identity;
					RUISCalibrationResultsIn4x4Matrix[devicePairString] = Matrix4x4.identity;
				}
			}
		}
		
		if (loadFromXML)
		{
			if(!LoadMultiXML(coordinateXmlFile)) {
				createExampleXML(coordinateXmlFile);
			}
		}
	}

	private void createExampleXML(string filename) {
		
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
		
		
		XmlElement transformWrapperElement = xmlDoc.CreateElement("PS_Move-Kinect_1");
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
		
		
		
		XmlElement floorDataWrapperElement = xmlDoc.CreateElement("Kinect_1");
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

	public bool LoadMultiXML(string filename) {
		
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, coordinateSchema);
		if(xmlDoc != null) 
		{
			foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
			{	
				Vector3 vector3 = new Vector3(0, 0, 0);
				Matrix4x4 device1ToDevice2Transform = new Matrix4x4();
				Quaternion quaternion = new Quaternion();
				
				if(node.Name == "Transforms") 
				{
					foreach (XmlNode groupElement in node.ChildNodes)	
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
					foreach (XmlNode groupElement in node.ChildNodes)	
					{
						Quaternion floorPitchRotation = Quaternion.identity;
						float distanceFromFloor = 0;
						foreach (XmlNode element in groupElement.ChildNodes)	
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
						
						RUISCalibrationResultsFloorPitchRotation[(RUISDevice) System.Enum.Parse( typeof(RUISDevice), groupElement.Name, true )] = floorPitchRotation;
						RUISCalibrationResultsDistanceFromFloor[(RUISDevice) System.Enum.Parse( typeof(RUISDevice), groupElement.Name, true )] = distanceFromFloor;		
					}
				}
				
			}
			
		}
		else return false;	
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
		
		if(xmlDoc != null) {
			calibrationMatrixElement = 	xmlDoc.DocumentElement;
			groupElement = calibrationMatrixElement.SelectSingleNode("Transforms");
			if(groupElement == null) groupElement = xmlDoc.CreateElement("Transforms");
			}
		else {
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
		if(testNode == null) { 	
			groupNode.AppendChild(wrapperElement);
		}
		else {// Element already exists
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
		
		if(xmlDoc != null) {
			calibrationMatrixElement = 	xmlDoc.DocumentElement;
			groupElement = calibrationMatrixElement.SelectSingleNode("FloorData");
			if(groupElement == null) groupElement = xmlDoc.CreateElement("FloorData");
		}
		else {
			xmlDoc = new XmlDocument();
			xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
			calibrationMatrixElement = xmlDoc.CreateElement("ns2", "RUISClibrationMatrix", "http://ruisystem.net/m2k");
			xmlDoc.AppendChild(calibrationMatrixElement);
			groupElement = xmlDoc.CreateElement("FloorData");
		}
		calibrationMatrixElement.AppendChild(groupElement);
		
		XmlElement wrapperElement = xmlDoc.CreateElement(wrapperElementName);
		groupElement.AppendChild(wrapperElement);
		
		XmlElement kinectFloorNormalElement = xmlDoc.CreateElement("floorNormal");
		
		kinectFloorNormalElement.SetAttribute("x", normal.x.ToString());
		kinectFloorNormalElement.SetAttribute("y", normal.y.ToString());
		kinectFloorNormalElement.SetAttribute("z", normal.z.ToString());
		wrapperElement.AppendChild(kinectFloorNormalElement);
		
		XmlElement kinectDistanceFromFloorElement = xmlDoc.CreateElement("distanceFromFloor");
		kinectDistanceFromFloorElement.SetAttribute("value", distance.ToString());
		
		wrapperElement.AppendChild(kinectDistanceFromFloorElement);
		
		XmlNode groupNode = xmlDoc.DocumentElement.SelectSingleNode("FloorData");
		XmlNode testNode = groupNode.SelectSingleNode(wrapperElementName);
		// Element not found
		if(testNode == null) { 	
			groupNode.AppendChild(wrapperElement);
		}
		else {// Element already exists
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
				Debug.LogError("Currently floor detection with " + floorDetectingDevice.ToString() + " is not supported!");
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
				Debug.LogError("Currently floor detection with " + floorDetectingDevice.ToString() + " is not supported!");
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

	public Vector3 ConvertMoveVelocity(Vector3 velocity)
    {
        //flip the z coordinate to get into unity's coordinate system
        Vector3 newVelocity = new Vector3(velocity.x, velocity.y, -velocity.z);
		
		string devicePairString = RUISDevice.PS_Move.ToString() + "-" + rootDevice.ToString();
		if (applyToRootCoordinates && rootDevice != RUISDevice.PS_Move) {
			newVelocity = RUISCalibrationResultsIn4x4Matrix[devicePairString].MultiplyPoint3x4(newVelocity);
		}

        newVelocity *= moveToUnityScale;
		newVelocity = Quaternion.Euler(0, yawOffset, 0) * newVelocity;

        return newVelocity;
    }

	public Vector3 ConvertMoveAngularVelocity(Vector3 angularVelocity)
    {
        Vector3 newVelocity = angularVelocity;
        newVelocity.x = -newVelocity.x;
        newVelocity.y = -newVelocity.y;
        
		string devicePairString = RUISDevice.PS_Move.ToString() + "-" + rootDevice.ToString();
		
		if (applyToRootCoordinates && rootDevice != RUISDevice.PS_Move) {
			newVelocity = RUISCalibrationResultsIn4x4Matrix[devicePairString].MultiplyPoint3x4(newVelocity);
        }
        newVelocity = Quaternion.Euler(0, yawOffset, 0) * newVelocity;
        return newVelocity;
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

        if (up == Vector3.zero || forward == Vector3.zero) return Quaternion.identity;

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
		Quaternion newRotation  = rotation;
		newRotation.x = -rotation.x;
		newRotation.y = -rotation.y;
		newRotation.z = rotation.z;
		newRotation.w = rotation.w;
		
		return newRotation;
	}
	
	/*
	 * 	Oculus Rift
	 */
	public Vector3 ConvertRawOculusDK2Location(Vector3 position)
	{
		Vector3 currentcameraPosition = OVRManager.capiHmd.GetTrackingState().CameraPose.Position.ToVector3();
		
		// TODO: Try combinations of this: position = OVRManager.capiHmd.GetTrackingState().CameraPose.Orientation * position
		
		Vector3 newPosition = Vector3.zero;
		newPosition.x =  position.x - currentcameraPosition.x;
		newPosition.y =  position.y - currentcameraPosition.y;
		newPosition.z = -(position.z + currentcameraPosition.z); // TODO: This probably depends on some DK2 setting, that can be affected with some kind of ResetCoordinateSystem() etc.
		
		//newPosition = newPosition - currentcameraPosition;
		
		return newPosition;
	}
	
	// Get Oculus Rift rotation in master coordinate system
	public Quaternion GetOculusRiftOrientation()
	{
		OVRPose rightEye;
		if(OVRManager.display != null)
		{
			rightEye = OVRManager.display.GetEyePose(OVREye.Right);
			return OculusCameraYRotation() * rightEye.orientation;
		}
		else 
			return OculusCameraYRotation();
	}

	// Oculus positional tracking camera's coordinate system origin in master coordinate system
	public Vector3 OculusCameraOrigin()
	{
		if (OVRManager.capiHmd != null) 
		{
			Vector3 currentOvrCameraPose = OVRManager.capiHmd.GetTrackingState().CameraPose.Position.ToVector3 ();
			currentOvrCameraPose.z = -currentOvrCameraPose.z; // TODO: Tuukka commented this (was unclear), test if it was ok to comment
			// TODO: Tuukka tried adding minus and commenting the above currentOvrCameraPose.z = - ...  This didn't work
			
			currentOvrCameraPose = ConvertLocation(currentOvrCameraPose, RUISDevice.Oculus_DK2); 
			
			return currentOvrCameraPose;
		} else
			return Vector3.zero;
	}
	
	// Oculus positional tracking camera's orientation around Y-axis in master coordinate system
	public Quaternion OculusCameraYRotation()
	{
		// ovrManager.SetYRotation(convertedRotation.eulerAngles.y);
		Quaternion convertedRotation = ConvertRotation(Quaternion.identity, RUISDevice.Oculus_DK2);
		
		// TODO: Tuukka tried adding 180. This was probably not right. Find correct solution.
		return Quaternion.Euler(new Vector3 (0, convertedRotation.eulerAngles.y, 0));
	}

	
	/*
	 * 	Convert locations obtained with a certain device to master coordinate system, apply position offset, and set Kinect origin to floor if applicable
	 */
	public Vector3 ConvertLocation(Vector3 inputLocation, RUISDevice device)
	{
		Vector3 outputLocation = inputLocation;
		string devicePairString = device.ToString() + "-" + rootDevice.ToString();

		// Transform location into master coordinate system
		if (applyToRootCoordinates && rootDevice != device) 
		{
			outputLocation = RUISCalibrationResultsIn4x4Matrix[devicePairString].MultiplyPoint3x4(outputLocation);
		}

		// Apply yaw offset
		outputLocation = Quaternion.Euler(0, yawOffset, 0) * RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputLocation;

		// Set Kinect 1/2 origin to floor
		if (setKinectOriginToFloor)
		{
			if (applyToRootCoordinates)
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
		if (applyToRootCoordinates || device == rootDevice)
			outputLocation += positionOffset;
		
		return outputLocation;
	}
	
	/*
	 * 	Convert rotations obtained with a certain device to master coordinate system, apply yaw offset, and apply Kinect pitch correction
	 */
	public Quaternion ConvertRotation(Quaternion inputRotation, RUISDevice device)
	{
		Quaternion outputRotation = inputRotation;
		
		if (applyToRootCoordinates && rootDevice != device) {
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			outputRotation = RUISCalibrationResultsInQuaternion[devicePairString] * outputRotation;
		}
		
//		outputRotation = Quaternion.Euler(0, yawOffset, 0) * RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputRotation;

		if (applyToRootCoordinates)
			outputRotation = RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputRotation;
		else
		{
			if(device == RUISDevice.Kinect_2)
				outputRotation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_2] * outputRotation;
			else if(device == RUISDevice.Kinect_1)
				outputRotation = RUISCalibrationResultsFloorPitchRotation[RUISDevice.Kinect_1] * outputRotation;
		}
		
		// Apply yaw offset
		if (applyToRootCoordinates || device == rootDevice)
			outputRotation = Quaternion.Euler(0, yawOffset, 0) * outputRotation;

		return outputRotation;
	}
	
}

