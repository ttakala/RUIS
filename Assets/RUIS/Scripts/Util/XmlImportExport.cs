using UnityEngine;
using System.Collections;
using System.Xml;

public class XmlImportExport {
    public static bool ImportInputManager(RUISInputManager inputManager, string filename, TextAsset xmlSchema)
    {
        XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, xmlSchema);
        if (xmlDoc == null)
        {
            return false;
        }

        XmlNode inputManagerNode = xmlDoc.GetElementsByTagName("ruisInputManager", "ns2").Item(0);
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
        inputManager.jumpGestureEnabled = bool.Parse(kinectNode.SelectSingleNode("jumpGestureEnabled").Attributes["value"].Value);

        XmlNode razerNode = xmlDoc.GetElementsByTagName("RazerSettings").Item(0);
        inputManager.enableRazerHydra = bool.Parse(razerNode.SelectSingleNode("enabled").Attributes["value"].Value);

        XmlNode riftDriftNode = xmlDoc.GetElementsByTagName("OculusDriftSettings").Item(0);
        string magnetometerMode = riftDriftNode.SelectSingleNode("magnetometerDriftCorrection").Attributes["value"].Value;
        inputManager.riftMagnetometerMode = (RUISInputManager.RiftMagnetometer)System.Enum.Parse(typeof(RUISInputManager.RiftMagnetometer), magnetometerMode);
        inputManager.kinectDriftCorrectionPreferred = bool.Parse(riftDriftNode.SelectSingleNode("kinectDriftCorrectionIfAvailable").Attributes["value"].Value);

        return true;
    }

    public static bool ExportInputManager(RUISInputManager inputManager, string filename){
        XmlDocument xmlDoc = new XmlDocument();

        xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

        XmlElement inputManagerRootElement = xmlDoc.CreateElement("ns2", "ruisInputManager", "http://ruisystem.net/ruisInputManager");
        xmlDoc.AppendChild(inputManagerRootElement);

        XmlElement psMoveSettingsElement = xmlDoc.CreateElement("PSMoveSettings");
        inputManagerRootElement.AppendChild(psMoveSettingsElement);

        XmlElement psMoveEnabledElement = xmlDoc.CreateElement("enabled");
        psMoveEnabledElement.SetAttribute("value", inputManager.enablePSMove.ToString());
        psMoveSettingsElement.AppendChild(psMoveEnabledElement);

        XmlElement psMoveIPElement = xmlDoc.CreateElement("ip");
        psMoveIPElement.SetAttribute("value", inputManager.PSMoveIP.ToString());
        psMoveSettingsElement.AppendChild(psMoveIPElement);

        XmlElement psMovePortElement = xmlDoc.CreateElement("port");
        psMovePortElement.SetAttribute("value", inputManager.PSMovePort.ToString());
        psMoveSettingsElement.AppendChild(psMovePortElement);

        XmlElement psMoveAutoConnectElement = xmlDoc.CreateElement("autoConnect");
        psMoveAutoConnectElement.SetAttribute("value", inputManager.connectToPSMoveOnStartup.ToString());
        psMoveSettingsElement.AppendChild(psMoveAutoConnectElement);

        XmlElement psMoveEnableInGameCalibration = xmlDoc.CreateElement("enableInGameCalibration");
        psMoveEnableInGameCalibration.SetAttribute("value", inputManager.enableMoveCalibrationDuringPlay.ToString());
        psMoveSettingsElement.AppendChild(psMoveEnableInGameCalibration);

        XmlElement psMoveMaxControllersElement = xmlDoc.CreateElement("maxControllers");
        psMoveMaxControllersElement.SetAttribute("value", inputManager.amountOfPSMoveControllers.ToString());
        psMoveSettingsElement.AppendChild(psMoveMaxControllersElement);



        XmlElement kinectSettingsElement = xmlDoc.CreateElement("KinectSettings");
        inputManagerRootElement.AppendChild(kinectSettingsElement);

        XmlElement kinectEnabledElement = xmlDoc.CreateElement("enabled");
        kinectEnabledElement.SetAttribute("value", inputManager.enableKinect.ToString());
        kinectSettingsElement.AppendChild(kinectEnabledElement);

        XmlElement maxKinectPlayersElement = xmlDoc.CreateElement("maxPlayers");
        maxKinectPlayersElement.SetAttribute("value", inputManager.maxNumberOfKinectPlayers.ToString());
        kinectSettingsElement.AppendChild(maxKinectPlayersElement);

        XmlElement kinectFloorDetectionElement = xmlDoc.CreateElement("floorDetection");
        kinectFloorDetectionElement.SetAttribute("value", inputManager.kinectFloorDetection.ToString());
        kinectSettingsElement.AppendChild(kinectFloorDetectionElement);

        XmlElement jumpGestureElement = xmlDoc.CreateElement("jumpGestureEnabled");
        jumpGestureElement.SetAttribute("value", inputManager.jumpGestureEnabled.ToString());
        kinectSettingsElement.AppendChild(jumpGestureElement);



        XmlElement razerSettingsElement = xmlDoc.CreateElement("RazerSettings");
        inputManagerRootElement.AppendChild(razerSettingsElement);

        XmlElement razerEnabledElement = xmlDoc.CreateElement("enabled");
        razerEnabledElement.SetAttribute("value", inputManager.enableRazerHydra.ToString());
        razerSettingsElement.AppendChild(razerEnabledElement);



        XmlElement riftDriftSettingsElement = xmlDoc.CreateElement("OculusDriftSettings");
        inputManagerRootElement.AppendChild(riftDriftSettingsElement);

        XmlElement magnetometerDriftCorrectionElement = xmlDoc.CreateElement("magnetometerDriftCorrection");
        magnetometerDriftCorrectionElement.SetAttribute("value", System.Enum.GetName(typeof(RUISInputManager.RiftMagnetometer), inputManager.riftMagnetometerMode));
        riftDriftSettingsElement.AppendChild(magnetometerDriftCorrectionElement);

        XmlElement kinectDriftCorrectionElement = xmlDoc.CreateElement("kinectDriftCorrectionIfAvailable");
        kinectDriftCorrectionElement.SetAttribute("value", inputManager.kinectDriftCorrectionPreferred.ToString());
        riftDriftSettingsElement.AppendChild(kinectDriftCorrectionElement);

        XMLUtil.SaveXmlToFile(filename, xmlDoc);
        
        return true;
    }

    public static bool ImportDisplay(RUISDisplay display, string filename, TextAsset displaySchema)
    {
        XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, displaySchema);
        if (xmlDoc == null)
        {
            return false;
        }

        display.displayCenterPosition = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayCenterPosition").Item(0));
        display.displayUpInternal = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayUp").Item(0));
        display.displayNormalInternal = XMLUtil.GetVector3FromXmlNode(xmlDoc.GetElementsByTagName("displayNormal").Item(0));

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
}