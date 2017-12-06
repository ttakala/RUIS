using UnityEngine;
using System.Collections;
using System.Xml;

public class XmlImportExport {
	public static bool ImportInputManager(RUISInputManager inputManager, RUISCoordinateSystem coordinateSystem, string filename, TextAsset xmlSchema)
	{
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, xmlSchema);
		if (xmlDoc == null)
		{
			return false;
		}
		
		XmlNode psMoveNode = xmlDoc.GetElementsByTagName("PSMoveSettings").Item(0);
		inputManager.enablePSMove = bool.Parse(psMoveNode.SelectSingleNode("enabled").Attributes["value"].Value);
		inputManager.PSMoveIP = psMoveNode.SelectSingleNode("ip").Attributes["value"].Value;
		inputManager.PSMovePort = int.Parse(psMoveNode.SelectSingleNode("port").Attributes["value"].Value);
		inputManager.connectToPSMoveOnStartup = bool.Parse(psMoveNode.SelectSingleNode("autoConnect").Attributes["value"].Value);
		inputManager.enableMoveCalibrationDuringPlay = bool.Parse(psMoveNode.SelectSingleNode("enableInGameCalibration").Attributes["value"].Value);
		inputManager.amountOfPSMoveControllers = int.Parse(psMoveNode.SelectSingleNode("maxControllers").Attributes["value"].Value);
		
		XmlNode kinectNode = xmlDoc.GetElementsByTagName("KinectSettings").Item(0);
		inputManager.enableKinect = bool.Parse(kinectNode.SelectSingleNode("enabled").Attributes["value"].Value);
		inputManager.maxNumberOfKinectPlayers = int.Parse(kinectNode.SelectSingleNode("maxPlayers").Attributes["value"].Value);
		inputManager.kinectFloorDetection = bool.Parse(kinectNode.SelectSingleNode("floorDetection").Attributes["value"].Value);
//		inputManager.jumpGestureEnabled = bool.Parse(kinectNode.SelectSingleNode("jumpGestureEnabled").Attributes["value"].Value);
		
		XmlNode kinect2Node = xmlDoc.GetElementsByTagName("Kinect2Settings").Item(0);
		inputManager.enableKinect2 = bool.Parse(kinect2Node.SelectSingleNode("enabled").Attributes["value"].Value);
		
		XmlNode razerNode = xmlDoc.GetElementsByTagName("RazerSettings").Item(0);
		inputManager.enableRazerHydra = bool.Parse(razerNode.SelectSingleNode("enabled").Attributes["value"].Value);

		// CustomDevice1
		XmlNode custom1Node = xmlDoc.GetElementsByTagName("Custom1Settings").Item(0);
		if(custom1Node != null)
		{
			inputManager.enableCustomDevice1 = bool.Parse(custom1Node.SelectSingleNode("enabled").Attributes["value"].Value);
			inputManager.customDevice1Name = custom1Node.SelectSingleNode("name").Attributes["value"].Value;
			if(custom1Node.SelectSingleNode("positionConversion") != null)
			{
				XmlNode positionConversionElement = custom1Node.SelectSingleNode("positionConversion");
				if(positionConversionElement != null)
				{
					inputManager.customDevice1Conversion.unitScale = float.Parse(positionConversionElement.Attributes["unitScale"].Value);
					inputManager.customDevice1Conversion.xPosNegate = bool.Parse(positionConversionElement.Attributes["xNegate"].Value);
					inputManager.customDevice1Conversion.yPosNegate = bool.Parse(positionConversionElement.Attributes["yNegate"].Value);
					inputManager.customDevice1Conversion.zPosNegate = bool.Parse(positionConversionElement.Attributes["zNegate"].Value);
				}
				XmlNode rotationConversionElement = custom1Node.SelectSingleNode("rotationConversion");
				if(rotationConversionElement != null)
				{
					inputManager.customDevice1Conversion.rotationInverse = bool.Parse(rotationConversionElement.Attributes["invert"].Value);
					inputManager.customDevice1Conversion.xRotNegate = bool.Parse(rotationConversionElement.Attributes["xNegate"].Value);
					inputManager.customDevice1Conversion.yRotNegate = bool.Parse(rotationConversionElement.Attributes["yNegate"].Value);
					inputManager.customDevice1Conversion.zRotNegate = bool.Parse(rotationConversionElement.Attributes["zNegate"].Value);
					inputManager.customDevice1Conversion.wRotNegate = bool.Parse(rotationConversionElement.Attributes["wNegate"].Value);
				}
			}
		}
		// CustomDevice2
		XmlNode custom2Node = xmlDoc.GetElementsByTagName("Custom2Settings").Item(0);
		if(custom2Node != null)
		{
			inputManager.enableCustomDevice2 = bool.Parse(custom2Node.SelectSingleNode("enabled").Attributes["value"].Value);
			inputManager.customDevice2Name = custom2Node.SelectSingleNode("name").Attributes["value"].Value;
			if(custom2Node.SelectSingleNode("positionConversion") != null)
			{
				XmlNode positionConversionElement = custom2Node.SelectSingleNode("positionConversion");
				if(positionConversionElement != null)
				{
					inputManager.customDevice2Conversion.unitScale = float.Parse(positionConversionElement.Attributes["unitScale"].Value);
					inputManager.customDevice2Conversion.xPosNegate = bool.Parse(positionConversionElement.Attributes["xNegate"].Value);
					inputManager.customDevice2Conversion.yPosNegate = bool.Parse(positionConversionElement.Attributes["yNegate"].Value);
					inputManager.customDevice2Conversion.zPosNegate = bool.Parse(positionConversionElement.Attributes["zNegate"].Value);
				}
				XmlNode rotationConversionElement = custom2Node.SelectSingleNode("rotationConversion");
				if(rotationConversionElement != null)
				{
					inputManager.customDevice2Conversion.rotationInverse = bool.Parse(rotationConversionElement.Attributes["invert"].Value);
					inputManager.customDevice2Conversion.xRotNegate = bool.Parse(rotationConversionElement.Attributes["xNegate"].Value);
					inputManager.customDevice2Conversion.yRotNegate = bool.Parse(rotationConversionElement.Attributes["yNegate"].Value);
					inputManager.customDevice2Conversion.zRotNegate = bool.Parse(rotationConversionElement.Attributes["zNegate"].Value);
					inputManager.customDevice2Conversion.wRotNegate = bool.Parse(rotationConversionElement.Attributes["wNegate"].Value);
				}
			}
		}

//		XmlNode riftDriftNode = xmlDoc.GetElementsByTagName("OculusDriftSettings").Item(0);
//		string magnetometerMode = riftDriftNode.SelectSingleNode("magnetometerDriftCorrection").Attributes["value"].Value;
//		inputManager.riftMagnetometerMode = (RUISInputManager.RiftMagnetometer)System.Enum.Parse(typeof(RUISInputManager.RiftMagnetometer), magnetometerMode);
//		inputManager.kinectDriftCorrectionPreferred = bool.Parse(riftDriftNode.SelectSingleNode("kinectDriftCorrectionIfAvailable").Attributes["value"].Value);

		XmlNode coordinateSystemNode = xmlDoc.GetElementsByTagName("CoordinateSystemSettings").Item(0);
		if(coordinateSystemNode != null)
		{
			if(coordinateSystemNode.SelectSingleNode("useMasterCoordinateSystem") != null)
				coordinateSystem.applyToRootCoordinates = bool.Parse(coordinateSystemNode.SelectSingleNode("useMasterCoordinateSystem").Attributes["value"].Value);
			if(coordinateSystemNode.SelectSingleNode("masterCoordinateSystemSensor") != null)
			{
				string masterDevice = coordinateSystemNode.SelectSingleNode("masterCoordinateSystemSensor").Attributes["value"].Value;
				coordinateSystem.rootDevice = (RUISDevice)System.Enum.Parse(typeof(RUISDevice), masterDevice);
			}
			if(coordinateSystemNode.SelectSingleNode("switchMasterToAvailableSensor") != null)
				coordinateSystem.switchToAvailableDevice = bool.Parse(coordinateSystemNode.SelectSingleNode("switchMasterToAvailableSensor").Attributes["value"].Value);
			if(coordinateSystemNode.SelectSingleNode("setKinectOriginToFloor") != null)
				coordinateSystem.setKinectOriginToFloor  = bool.Parse(coordinateSystemNode.SelectSingleNode("setKinectOriginToFloor").Attributes["value"].Value);
			if(coordinateSystemNode.SelectSingleNode("coordinateSystemYRotationOffset") != null)
				coordinateSystem.yawOffset = float.Parse(coordinateSystemNode.SelectSingleNode("coordinateSystemYRotationOffset").Attributes["value"].Value);

			if(coordinateSystemNode.SelectSingleNode("coordinateSystemLocationOffset") != null)
			{
				XmlNode translationElement = coordinateSystemNode.SelectSingleNode("coordinateSystemLocationOffset");
				float x = float.Parse(translationElement.Attributes["x"].Value);
				float y = float.Parse(translationElement.Attributes["y"].Value);
				float z = float.Parse(translationElement.Attributes["z"].Value);
				coordinateSystem.positionOffset = new Vector3(x, y, z);
			}
		}

		return true;
	}
	
	public static bool ExportInputManager(RUISInputManager inputManager, string filename){
		XmlDocument xmlDoc = new XmlDocument();
		
		xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
		
		XmlElement inputManagerRootElement = xmlDoc.CreateElement("ns2", "RUISInputManager", "http://ruisystem.net/RUISInputManager");
		xmlDoc.AppendChild(inputManagerRootElement);
		
		XmlComment booleanComment = xmlDoc.CreateComment("Boolean values always with a lower case, e.g. \"true\" or \"false\"");
		inputManagerRootElement.AppendChild(booleanComment);

		XmlElement psMoveSettingsElement = xmlDoc.CreateElement("PSMoveSettings");
		inputManagerRootElement.AppendChild(psMoveSettingsElement);

		XmlElement psMoveEnabledElement = xmlDoc.CreateElement("enabled");
		psMoveEnabledElement.SetAttribute("value", inputManager.enablePSMove.ToString().ToLowerInvariant());
		psMoveSettingsElement.AppendChild(psMoveEnabledElement);
		
		XmlElement psMoveIPElement = xmlDoc.CreateElement("ip");
		psMoveIPElement.SetAttribute("value", inputManager.PSMoveIP.ToString());
		psMoveSettingsElement.AppendChild(psMoveIPElement);
		
		XmlElement psMovePortElement = xmlDoc.CreateElement("port");
		psMovePortElement.SetAttribute("value", inputManager.PSMovePort.ToString());
		psMoveSettingsElement.AppendChild(psMovePortElement);
		
		XmlElement psMoveAutoConnectElement = xmlDoc.CreateElement("autoConnect");
		psMoveAutoConnectElement.SetAttribute("value", inputManager.connectToPSMoveOnStartup.ToString().ToLowerInvariant());
		psMoveSettingsElement.AppendChild(psMoveAutoConnectElement);
		
		XmlElement psMoveEnableInGameCalibration = xmlDoc.CreateElement("enableInGameCalibration");
		psMoveEnableInGameCalibration.SetAttribute("value", inputManager.enableMoveCalibrationDuringPlay.ToString().ToLowerInvariant());
		psMoveSettingsElement.AppendChild(psMoveEnableInGameCalibration);
		
		XmlElement psMoveMaxControllersElement = xmlDoc.CreateElement("maxControllers");
		psMoveMaxControllersElement.SetAttribute("value", inputManager.amountOfPSMoveControllers.ToString());
		psMoveSettingsElement.AppendChild(psMoveMaxControllersElement);
		
		
		
		XmlElement kinectSettingsElement = xmlDoc.CreateElement("KinectSettings");
		inputManagerRootElement.AppendChild(kinectSettingsElement);
		
		XmlElement kinectEnabledElement = xmlDoc.CreateElement("enabled");
		kinectEnabledElement.SetAttribute("value", inputManager.enableKinect.ToString().ToLowerInvariant());
		kinectSettingsElement.AppendChild(kinectEnabledElement);
		
		XmlElement maxKinectPlayersElement = xmlDoc.CreateElement("maxPlayers");
		maxKinectPlayersElement.SetAttribute("value", inputManager.maxNumberOfKinectPlayers.ToString());
		kinectSettingsElement.AppendChild(maxKinectPlayersElement);
		
		XmlElement kinectFloorDetectionElement = xmlDoc.CreateElement("floorDetection");
		kinectFloorDetectionElement.SetAttribute("value", inputManager.kinectFloorDetection.ToString().ToLowerInvariant());
		kinectSettingsElement.AppendChild(kinectFloorDetectionElement);
		
//		XmlElement jumpGestureElement = xmlDoc.CreateElement("jumpGestureEnabled");
//		jumpGestureElement.SetAttribute("value", inputManager.jumpGestureEnabled.ToString().ToLowerInvariant());
//		kinectSettingsElement.AppendChild(jumpGestureElement);

		XmlElement kinect2SettingsElement = xmlDoc.CreateElement("Kinect2Settings");
		inputManagerRootElement.AppendChild(kinect2SettingsElement);
		
		XmlElement kinect2EnabledElement = xmlDoc.CreateElement("enabled");
		kinect2EnabledElement.SetAttribute("value", inputManager.enableKinect2.ToString().ToLowerInvariant());
		kinect2SettingsElement.AppendChild(kinect2EnabledElement);
		
		XmlElement kinect2FloorDetectionElement = xmlDoc.CreateElement("floorDetection");
		kinect2FloorDetectionElement.SetAttribute("value", inputManager.kinect2FloorDetection.ToString().ToLowerInvariant());
		kinect2SettingsElement.AppendChild(kinect2FloorDetectionElement);
		
		XmlElement razerSettingsElement = xmlDoc.CreateElement("RazerSettings");
		inputManagerRootElement.AppendChild(razerSettingsElement);
		
		XmlElement razerEnabledElement = xmlDoc.CreateElement("enabled");
		razerEnabledElement.SetAttribute("value", inputManager.enableRazerHydra.ToString().ToLowerInvariant());
		razerSettingsElement.AppendChild(razerEnabledElement);

		// CustomDevice1
		XmlElement custom1SettingsElement = xmlDoc.CreateElement("Custom1Settings");
		inputManagerRootElement.AppendChild(custom1SettingsElement);
		XmlElement custom1EnabledElement = xmlDoc.CreateElement("enabled");
		custom1EnabledElement.SetAttribute("value", inputManager.enableCustomDevice1.ToString().ToLowerInvariant());
		custom1SettingsElement.AppendChild(custom1EnabledElement);
		XmlElement custom1NameElement = xmlDoc.CreateElement("name");
		custom1NameElement.SetAttribute("value", inputManager.customDevice1Name.ToString());
		custom1SettingsElement.AppendChild(custom1NameElement);
		XmlElement custom1PositionConversionElement = xmlDoc.CreateElement("positionConversion");
		custom1PositionConversionElement.SetAttribute("unitScale", inputManager.customDevice1Conversion.unitScale.ToString());
		custom1PositionConversionElement.SetAttribute("xNegate", inputManager.customDevice1Conversion.xPosNegate.ToString().ToLowerInvariant());
		custom1PositionConversionElement.SetAttribute("yNegate", inputManager.customDevice1Conversion.yPosNegate.ToString().ToLowerInvariant());
		custom1PositionConversionElement.SetAttribute("zNegate", inputManager.customDevice1Conversion.zPosNegate.ToString().ToLowerInvariant());
		custom1SettingsElement.AppendChild(custom1PositionConversionElement);
		XmlElement custom1RotationConversionElement = xmlDoc.CreateElement("rotationConversion");
		custom1RotationConversionElement.SetAttribute("invert", inputManager.customDevice1Conversion.rotationInverse.ToString().ToLowerInvariant());
		custom1RotationConversionElement.SetAttribute("xNegate", inputManager.customDevice1Conversion.xRotNegate.ToString().ToLowerInvariant());
		custom1RotationConversionElement.SetAttribute("yNegate", inputManager.customDevice1Conversion.yRotNegate.ToString().ToLowerInvariant());
		custom1RotationConversionElement.SetAttribute("zNegate", inputManager.customDevice1Conversion.zRotNegate.ToString().ToLowerInvariant());
		custom1RotationConversionElement.SetAttribute("wNegate", inputManager.customDevice1Conversion.wRotNegate.ToString().ToLowerInvariant());
		custom1SettingsElement.AppendChild(custom1RotationConversionElement);

		// CustomDevice2
		XmlElement custom2SettingsElement = xmlDoc.CreateElement("Custom2Settings");
		inputManagerRootElement.AppendChild(custom2SettingsElement);
		XmlElement custom2EnabledElement = xmlDoc.CreateElement("enabled");
		custom2EnabledElement.SetAttribute("value", inputManager.enableCustomDevice2.ToString().ToLowerInvariant());
		custom2SettingsElement.AppendChild(custom2EnabledElement);
		XmlElement custom2NameElement = xmlDoc.CreateElement("name");
		custom2NameElement.SetAttribute("value", inputManager.customDevice2Name.ToString());
		custom2SettingsElement.AppendChild(custom2NameElement);
		XmlElement custom2PositionConversionElement = xmlDoc.CreateElement("positionConversion");
		custom2PositionConversionElement.SetAttribute("unitScale", inputManager.customDevice2Conversion.unitScale.ToString());
		custom2PositionConversionElement.SetAttribute("xNegate", inputManager.customDevice2Conversion.xPosNegate.ToString().ToLowerInvariant());
		custom2PositionConversionElement.SetAttribute("yNegate", inputManager.customDevice2Conversion.yPosNegate.ToString().ToLowerInvariant());
		custom2PositionConversionElement.SetAttribute("zNegate", inputManager.customDevice2Conversion.zPosNegate.ToString().ToLowerInvariant());
		custom2SettingsElement.AppendChild(custom2PositionConversionElement);
		XmlElement custom2RotationConversionElement = xmlDoc.CreateElement("rotationConversion");
		custom2RotationConversionElement.SetAttribute("invert", inputManager.customDevice2Conversion.rotationInverse.ToString().ToLowerInvariant());
		custom2RotationConversionElement.SetAttribute("xNegate", inputManager.customDevice2Conversion.xRotNegate.ToString().ToLowerInvariant());
		custom2RotationConversionElement.SetAttribute("yNegate", inputManager.customDevice2Conversion.yRotNegate.ToString().ToLowerInvariant());
		custom2RotationConversionElement.SetAttribute("zNegate", inputManager.customDevice2Conversion.zRotNegate.ToString().ToLowerInvariant());
		custom2RotationConversionElement.SetAttribute("wNegate", inputManager.customDevice2Conversion.wRotNegate.ToString().ToLowerInvariant());
		custom2SettingsElement.AppendChild(custom2RotationConversionElement);

//		XmlElement riftDriftSettingsElement = xmlDoc.CreateElement("OculusDriftSettings");
//		inputManagerRootElement.AppendChild(riftDriftSettingsElement);

//		XmlElement kinectDriftCorrectionElement = xmlDoc.CreateElement("kinectDriftCorrectionIfAvailable");
//		kinectDriftCorrectionElement.SetAttribute("value", inputManager.kinectDriftCorrectionPreferred.ToString().ToLowerInvariant());
//		riftDriftSettingsElement.AppendChild(kinectDriftCorrectionElement);

		//XmlElement magnetometerDriftCorrectionElement = xmlDoc.CreateElement("magnetometerDriftCorrection");
		//magnetometerDriftCorrectionElement.SetAttribute("value", System.Enum.GetName(typeof(RUISInputManager.RiftMagnetometer), inputManager.riftMagnetometerMode));
		//riftDriftSettingsElement.AppendChild(magnetometerDriftCorrectionElement);

		XmlElement coordinateSystemSettingsElement = xmlDoc.CreateElement("CoordinateSystemSettings");
		inputManagerRootElement.AppendChild(coordinateSystemSettingsElement);

		XmlComment coordinateComment0 = xmlDoc.CreateComment(  "Below values will override the settings made in Unity Editor if you uncomment them.");
		coordinateSystemSettingsElement.AppendChild(coordinateComment0);

		// HACK TODO: Add all coordinate system settings when they can be altered in the menu
		//		XmlElement useMasterCoordinateSystemElement = xmlDoc.CreateElement("useMasterCoordinateSystem");
		//		useMasterCoordinateSystemElement.SetAttribute("value", inputManager.useMasterCoordinateSystem.ToString().ToLowerInvariant());
		//		coordinateSystemSettingsElement.AppendChild(useMasterCoordinateSystemElement);
		// ...
		XmlComment coordinateComment1 = xmlDoc.CreateComment(  "\n    <useMasterCoordinateSystem value=\"true\" /> \n"
															 + "    <masterCoordinateSystemSensor value=\"Kinect_2\" />\n");
		coordinateSystemSettingsElement.AppendChild(coordinateComment1);

		XmlComment coordinateComment2 = xmlDoc.CreateComment(" " + RUISDevice.Kinect_1 + ", " + RUISDevice.Kinect_2 + ", " + RUISDevice.OpenVR + ", " 
																 + RUISDevice.UnityXR  + ", " + RUISDevice.Custom_1 + ", " + RUISDevice.Custom_2 + " ");
		coordinateSystemSettingsElement.AppendChild(coordinateComment2);

		XmlComment coordinateComment3 = xmlDoc.CreateComment(  "\n    <switchMasterToAvailableSensor value=\"true\" />\n"
															 + "    <coordinateSystemLocationOffset x=\"0\" y=\"0\" z=\"0\" />\n"
															 + "    <coordinateSystemYRotationOffset value=\"0\" />\n"
															 + "    <setKinectOriginToFloor value=\"true\" />\n");
		coordinateSystemSettingsElement.AppendChild(coordinateComment3);
		
		XMLUtil.SaveXmlToFile(filename, xmlDoc);
		
		return true;
	}
	
	public static bool ImportDisplay(RUISDisplay display, string filename, TextAsset displaySchema, bool loadFromFileInEditor)
	{
		XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, displaySchema);
		if (xmlDoc == null)
		{
			return false;
		}
		
		if (Application.isEditor && loadFromFileInEditor)
		{
			display.displayCenterPosition = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayCenterPosition").Item(0));
			display.displayUpInternal = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayUp").Item(0));
			display.displayNormalInternal = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayNormal").Item(0));
			display.width = float.Parse(xmlDoc.GetElementsByTagName("displaySize").Item(0).Attributes["width"].Value);
			display.height = float.Parse(xmlDoc.GetElementsByTagName("displaySize").Item(0).Attributes["height"].Value);
			display.resolutionX = int.Parse(xmlDoc.GetElementsByTagName("displayResolution").Item(0).Attributes["width"].Value);
			display.resolutionY = int.Parse(xmlDoc.GetElementsByTagName("displayResolution").Item(0).Attributes["height"].Value);
		}
		
		if(display.linkedCamera)
			display.linkedCamera.LoadKeystoningFromXML(xmlDoc);
		
		return true;
	}
	
	public static bool ExportDisplay(RUISDisplay display, string xmlFilename)
	{
		XmlDocument xmlDoc = new XmlDocument();
		
		xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
		
		XmlElement displayRootElement = xmlDoc.CreateElement("ns2", "ruisDisplay", "http://ruisystem.net/display");
		xmlDoc.AppendChild(displayRootElement);
		
		XmlElement displayCenterPositionElement = xmlDoc.CreateElement("displayCenterPosition");
		XMLUtil.WriteVector3ToXmlElement(displayCenterPositionElement, display.displayCenterPosition);
		displayRootElement.AppendChild(displayCenterPositionElement);
		
		XmlElement displayUpElement = xmlDoc.CreateElement("displayUp");
		XMLUtil.WriteVector3ToXmlElement(displayUpElement, display.displayUpInternal);
		displayRootElement.AppendChild(displayUpElement);
		
		XmlElement displayNormalElement = xmlDoc.CreateElement("displayNormal");
		XMLUtil.WriteVector3ToXmlElement(displayNormalElement, display.displayNormalInternal);
		displayRootElement.AppendChild(displayNormalElement);
		
		XmlElement displaySizeElement = xmlDoc.CreateElement("displaySize");
		displaySizeElement.SetAttribute("width", display.width.ToString());
		displaySizeElement.SetAttribute("height", display.height.ToString());
		displayRootElement.AppendChild(displaySizeElement);
		
		XmlElement displayResolutionElement = xmlDoc.CreateElement("displayResolution");
		displayResolutionElement.SetAttribute("width", display.resolutionX.ToString());
		displayResolutionElement.SetAttribute("height", display.resolutionY.ToString());
		displayRootElement.AppendChild(displayResolutionElement);
		
		display.linkedCamera.SaveKeystoningToXML(displayRootElement);
		
		XMLUtil.SaveXmlToFile(xmlFilename, xmlDoc);
		
		return true;
	}
	
	public static bool ImportKeystoningConfiguration(RUISKeystoningConfiguration keystoningConfiguration, XmlDocument xmlDoc)
	{
		XmlNode centerCornerElement = xmlDoc.GetElementsByTagName("centerKeystone").Item(0);
		keystoningConfiguration.centerCameraCorners = new RUISKeystoning.KeystoningCorners(centerCornerElement);
		
		XmlNode leftCornerElement = xmlDoc.GetElementsByTagName("leftKeystone").Item(0);
		keystoningConfiguration.leftCameraCorners = new RUISKeystoning.KeystoningCorners(leftCornerElement);
		
		XmlNode rightCornerElement = xmlDoc.GetElementsByTagName("rightKeystone").Item(0);
		keystoningConfiguration.rightCameraCorners = new RUISKeystoning.KeystoningCorners(rightCornerElement);
		
		return true;
	}
	
	public static bool ExportKeystoningConfiguration(RUISKeystoningConfiguration keystoningConfiguration, XmlElement displayXmlElement)
	{
		XmlElement centerCornerElement = displayXmlElement.OwnerDocument.CreateElement("centerKeystone");
		keystoningConfiguration.centerCameraCorners.SaveToXML(centerCornerElement);
		displayXmlElement.AppendChild(centerCornerElement);
		
		XmlElement leftCornerElement = displayXmlElement.OwnerDocument.CreateElement("leftKeystone");
		keystoningConfiguration.leftCameraCorners.SaveToXML(leftCornerElement);
		displayXmlElement.AppendChild(leftCornerElement);
		
		XmlElement rightCornerElement = displayXmlElement.OwnerDocument.CreateElement("rightKeystone");
		keystoningConfiguration.rightCameraCorners.SaveToXML(rightCornerElement);
		displayXmlElement.AppendChild(rightCornerElement);
		
		return true;
	}
	
	public static bool ExportKeystoning(RUISKeystoning.KeystoningCorners keystoningCorners, XmlElement xmlElement)
	{
		XmlElement topLeft = xmlElement.OwnerDocument.CreateElement("topLeft");
		XMLUtil.WriteVector2ToXmlElement(topLeft, keystoningCorners[0]);
		xmlElement.AppendChild(topLeft);
		
		XmlElement topRight = xmlElement.OwnerDocument.CreateElement("topRight");
		XMLUtil.WriteVector2ToXmlElement(topRight, keystoningCorners[1]);
		xmlElement.AppendChild(topRight);
		
		XmlElement bottomRight = xmlElement.OwnerDocument.CreateElement("bottomRight");
		XMLUtil.WriteVector2ToXmlElement(bottomRight, keystoningCorners[2]);
		xmlElement.AppendChild(bottomRight);
		
		XmlElement bottomLeft = xmlElement.OwnerDocument.CreateElement("bottomLeft");
		XMLUtil.WriteVector2ToXmlElement(bottomLeft, keystoningCorners[3]);
		xmlElement.AppendChild(bottomLeft);
		
		return true;
	}
	public static bool XmlHandlingFunctionalityAvailable() {
		return true;
	}
	
}
