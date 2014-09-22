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
    private Quaternion deviceToRootRotation = Quaternion.identity;
    
	public float yawOffset = 0;
	
	public Vector3 floorNormal = Vector3.up; // TODO: This needs to be assigned everywhere where floorPitchRotation is assigned
	public Quaternion floorPitchRotation = Quaternion.identity;
	private float distanceFromFloor = 0;
	
    public bool applyToRootCoordinates = true;
    public bool setKinectOriginToFloor = false;

    public Vector3 positionOffset = Vector3.zero;
    
	public Dictionary<string, Vector3> RUISCalibrationResultsInVector3 = new Dictionary<string, Vector3>();
	public Dictionary<string, Quaternion> RUISCalibrationResultsInQuaternion = new Dictionary<string, Quaternion>();
	public Dictionary<string, Matrix4x4> RUISCalibrationResultsIn4x4Matrix = new Dictionary<string, Matrix4x4>();
	
	public Dictionary<RUISDevice, Quaternion> RUISCalibrationResultsFloorPitchRotation = new Dictionary<RUISDevice, Quaternion>();
	public Dictionary<RUISDevice, float> RUISCalibrationResultsDistanceFromFloor = new Dictionary<RUISDevice, float>();
	
	public RUISDevice rootDevice; 
	
	OVRCameraController ovrCameraController;
	
	void Awake() {
		ovrCameraController = MonoBehaviour.FindObjectOfType(typeof(OVRCameraController)) as OVRCameraController;
	
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
				Matrix4x4 moveToKinectTransform = new Matrix4x4();
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
						moveToKinectTransform.SetColumn(3, new Vector4(x, y, z, 1.0f));
			
						XmlNode rotationElement = groupElement.SelectSingleNode("rotate");
						
						moveToKinectTransform.m00 = float.Parse(rotationElement.Attributes["r00"].Value);
						moveToKinectTransform.m01 = float.Parse(rotationElement.Attributes["r01"].Value);
						moveToKinectTransform.m02 = float.Parse(rotationElement.Attributes["r02"].Value);
						moveToKinectTransform.m10 = float.Parse(rotationElement.Attributes["r10"].Value);
						moveToKinectTransform.m11 = float.Parse(rotationElement.Attributes["r11"].Value);
						moveToKinectTransform.m12 = float.Parse(rotationElement.Attributes["r12"].Value);
						moveToKinectTransform.m20 = float.Parse(rotationElement.Attributes["r20"].Value);
						moveToKinectTransform.m21 = float.Parse(rotationElement.Attributes["r21"].Value);
						moveToKinectTransform.m22 = float.Parse(rotationElement.Attributes["r22"].Value);
						
						List<Vector3> rotationVectors = MathUtil.Orthonormalize(MathUtil.ExtractRotationVectors(moveToKinectTransform));
						Matrix4x4 rotationMatrix = new Matrix4x4();
						rotationMatrix.SetColumn(0, rotationVectors[0]);
						rotationMatrix.SetColumn(1, rotationVectors[1]);
						rotationMatrix.SetColumn(2, rotationVectors[2]);
						
						quaternion = MathUtil.QuaternionFromMatrix(rotationMatrix); 
						
						RUISCalibrationResultsInVector3[groupElement.Name] = vector3;
						RUISCalibrationResultsIn4x4Matrix[groupElement.Name] = moveToKinectTransform;
						RUISCalibrationResultsInQuaternion[groupElement.Name] = quaternion;		
						
						// Inverses
						string[] parts = groupElement.Name.Split('-');
						string inverseName = parts[1] + "-" + parts[0];
						RUISCalibrationResultsInVector3[inverseName] = -vector3;
						RUISCalibrationResultsIn4x4Matrix[inverseName] = moveToKinectTransform.inverse;
						RUISCalibrationResultsInQuaternion[inverseName] = Quaternion.Inverse(quaternion);		
					}
				}
				
				if(node.Name == "FloorData") 
				{
					foreach (XmlNode groupElement in node.ChildNodes)	
					{
						foreach (XmlNode element in groupElement.ChildNodes)	
						{
							switch(element.Name) 
							{
							case "floorNormal":
								float xValue = float.Parse(element.Attributes["x"].Value);
								float yValue = float.Parse(element.Attributes["y"].Value);
								float zValue = float.Parse(element.Attributes["z"].Value);
								floorPitchRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(xValue, yValue, zValue));
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

    public void SaveTransformData(string filename, RUISDevice device1, RUISDevice device2)
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

	public void SaveFloorData(string filename, RUISDevice device, Quaternion normal, float distance)
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
		Vector3 normalVector = normal * Vector3.up;
		kinectFloorNormalElement.SetAttribute("x", normalVector.x.ToString());
		kinectFloorNormalElement.SetAttribute("y", normalVector.y.ToString());
		kinectFloorNormalElement.SetAttribute("z", normalVector.z.ToString());
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
   
	public void SetFloorNormal(Vector3 newFloorNormal)
    {
		Quaternion kinectFloorRotator = Quaternion.identity;
		kinectFloorRotator.SetFromToRotation(Vector3.up, newFloorNormal);
		floorPitchRotation = kinectFloorRotator;
    }

    public void ResetFloorNormal()
    {
		floorPitchRotation = Quaternion.identity;
    }

    public void SetDistanceFromFloor(float distance)
    {
        distanceFromFloor = distance;
    }
	
	public float GetDistanceFromFloor()
	{
		return distanceFromFloor;
	}

    public void ResetDistanceFromFloor()
    {
        distanceFromFloor = 0;
    }

	public void SetDeviceToRootTransforms(Matrix4x4 rotationMatrix, Matrix4x4 transformMatrix) 
	{
		deviceToRootTransform = transformMatrix;
		deviceToRootRotation = MathUtil.QuaternionFromMatrix(rotationMatrix);
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
	
	// TUUKKA:
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
	
	public Vector3 ConvertRawOculusDK2Location(Vector3 position) {
		Vector3 newPosition = Vector3.zero;
		newPosition.x = position.x;
		newPosition.y = position.y;
		newPosition.z = -(position.z + 1);
		
		return newPosition;
	}
	
	public Vector3 ConvertLocation(Vector3 inputLocation, RUISDevice device) {

		Vector3 outputLocation = inputLocation;
		string devicePairString = device.ToString() + "-" + rootDevice.ToString();

		if (applyToRootCoordinates && rootDevice != device) 
		{
			outputLocation = RUISCalibrationResultsIn4x4Matrix[devicePairString].MultiplyPoint3x4(outputLocation);
		}
		
		outputLocation = Quaternion.Euler(0, yawOffset, 0) * RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputLocation;
		
		if (setKinectOriginToFloor)
		{
			outputLocation.y += RUISCalibrationResultsDistanceFromFloor[rootDevice];
		}
		
		outputLocation += positionOffset;
		
		return outputLocation;
	}
	
	
	public Quaternion ConvertRotation(Quaternion inputRotation, RUISDevice device) {
		
		Quaternion outputRotation = inputRotation;
		
		if (applyToRootCoordinates && rootDevice != device) {
			string devicePairString = device.ToString() + "-" + rootDevice.ToString();
			outputRotation = RUISCalibrationResultsInQuaternion[devicePairString] * outputRotation;
		}
		
		outputRotation = Quaternion.Euler(0, yawOffset, 0) * RUISCalibrationResultsFloorPitchRotation[rootDevice] * outputRotation;
		
		return outputRotation;
	}
	
}

