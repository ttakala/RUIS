/*****************************************************************************

Content    :   Class for managing input devices of RUIS
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Mikael Matveinen, Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Xml;

public class RUISInputManager : MonoBehaviour
{
    public TextAsset xmlSchema;
    public string filename = "inputConfig.txt";

    public bool loadFromTextFileInEditor = false;

    public bool enablePSMove = true;
    public string PSMoveIP = "130.233.46.217";
    public int PSMovePort = 7899;
    public bool connectToPSMoveOnStartup = true;
    public PSMoveWrapper psMoveWrapper;
    public int amountOfPSMoveControllers = 4;
    public bool enableMoveCalibrationDuringPlay = false;

    public bool enableKinect = true;
    public int maxNumberOfKinectPlayers = 2;
    public bool kinectFloorDetection = true;
	
	private RUISCoordinateSystem coordinateSystem = null;
    private OpenNI.SceneAnalyzer sceneAnalyzer = null;
	private RUISM2KCalibration moveKinectCalibration;
	//private bool usingExistingSceneAnalyzer = false;
	
    public RUISPSMoveWand[] moveControllers;

    public void Awake()
    {
        if (!Application.isEditor || loadFromTextFileInEditor)
        {
            if (!Import(filename))
            {
                Debug.LogError("Could not load input configuration file. Creating file based on current settings.");
                Export(filename);
            }
        }

        if (!enableKinect)
        {
            Debug.Log("Kinect is disabled from RUISInputManager.");
            GetComponentInChildren<RUISKinectDisabler>().KinectNotAvailable();
        }
        else
        {
            GetComponentInChildren<NIPlayerManagerCOMSelection>().m_MaxNumberOfPlayers = maxNumberOfKinectPlayers;
        }

        psMoveWrapper = GetComponentInChildren<PSMoveWrapper>();
        if (enablePSMove)
        {

            if (psMoveWrapper && connectToPSMoveOnStartup)
            {
                StartCoroutine("CheckForMoveConnection");

                psMoveWrapper.Connect(PSMoveIP, PSMovePort);

                psMoveWrapper.enableDefaultInGameCalibrate = enableMoveCalibrationDuringPlay;

            }
        }
        else
        {
			Debug.Log("PS Move is disabled from RUISInputManager.");
            //psMoveWrapper.gameObject.SetActiveRecursively(false);
        }

        DisableUnneededMoveWands();

        
    }

    void Start()
    {
        //check whether the kinect camera is actually connected
        if (enableKinect)
        {
            OpenNISettingsManager settingsManager = FindObjectOfType(typeof(OpenNISettingsManager)) as OpenNISettingsManager;
            if (settingsManager.UserGenrator == null || !settingsManager.UserGenrator.Valid)
            {
                Debug.LogError("Could not start OpenNI! Check your Kinect connection.");
                GetComponentInChildren<RUISKinectDisabler>().KinectNotAvailable();
            }
			else // Tuukka:
			{
				if(kinectFloorDetection)
				{
					StartFloorDetection();
				}
			}
        }

        if (enablePSMove)
        {
            RUISPSMoveWand[] controllers = GetComponentsInChildren<RUISPSMoveWand>();
            moveControllers = new RUISPSMoveWand[controllers.Length];
            foreach (RUISPSMoveWand controller in controllers)
            {
                moveControllers[controller.controllerId] = controller;
            }
        }
    }

    public void OnApplicationQuit()
    {
        if(enablePSMove && psMoveWrapper && psMoveWrapper.isConnected)
            psMoveWrapper.Disconnect(false);
    }
	
	public void StartFloorDetection()
	{
		if (enableKinect)
		{
			kinectFloorDetection = true;
			
			moveKinectCalibration = FindObjectOfType(typeof(RUISM2KCalibration)) as RUISM2KCalibration;
			if(!moveKinectCalibration)
			{
				if(sceneAnalyzer == null)
				{
					sceneAnalyzer = new OpenNI.SceneAnalyzer((FindObjectOfType(typeof(OpenNISettingsManager)) 
															as OpenNISettingsManager).CurrentContext.BasicContext);
					sceneAnalyzer.StartGenerating();
					Debug.Log ("Creating sceneAnalyzer");
				}
			}
			else
	    		StartCoroutine("attemptStartingSceneAnalyzer");
			
	    	StartCoroutine("attemptUpdatingFloorNormal");
		}
		else
			Debug.LogError("Kinect is not enabled! You can enable it from RUIS InputManager.");
	}

    public bool Import(string filename)
    {
        XmlDocument xmlDoc = XMLUtil.LoadAndValidateXml(filename, xmlSchema);
        if (xmlDoc == null)
        {
            return false;
        }

        XmlNode inputManagerNode = xmlDoc.GetElementsByTagName("ruisInputManager", "ns2").Item(0);
        XmlNode psMoveNode = xmlDoc.GetElementsByTagName("PSMoveSettings").Item(0);
        enablePSMove = bool.Parse(psMoveNode.SelectSingleNode("enabled").Attributes["value"].Value);
        PSMoveIP = psMoveNode.SelectSingleNode("ip").Attributes["value"].Value;
        PSMovePort = int.Parse(psMoveNode.SelectSingleNode("port").Attributes["value"].Value);
        connectToPSMoveOnStartup = bool.Parse(psMoveNode.SelectSingleNode("autoConnect").Attributes["value"].Value);
        enableMoveCalibrationDuringPlay = bool.Parse(psMoveNode.SelectSingleNode("enableInGameCalibration").Attributes["value"].Value);
        amountOfPSMoveControllers = int.Parse(psMoveNode.SelectSingleNode("maxControllers").Attributes["value"].Value);

        XmlNode kinectNode = xmlDoc.GetElementsByTagName("KinectSettings").Item(0);
        enableKinect = bool.Parse(kinectNode.SelectSingleNode("enabled").Attributes["value"].Value);
        maxNumberOfKinectPlayers = int.Parse(kinectNode.SelectSingleNode("maxPlayers").Attributes["value"].Value);
        kinectFloorDetection = bool.Parse(kinectNode.SelectSingleNode("floorDetection").Attributes["value"].Value);

        return true;
    }

    public bool Export(string filename)
    {
        /*
  <GestureSettings>
	<jumpEnabled value="False" />
  </GestureSettings> */
        XmlDocument xmlDoc = new XmlDocument();

        xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

        XmlElement inputManagerRootElement = xmlDoc.CreateElement("ns2", "ruisInputManager", "http://ruisystem.net/ruisInputManager");
        xmlDoc.AppendChild(inputManagerRootElement);

        XmlElement psMoveSettingsElement = xmlDoc.CreateElement("PSMoveSettings");
        inputManagerRootElement.AppendChild(psMoveSettingsElement);

        XmlElement psMoveEnabledElement = xmlDoc.CreateElement("enabled");
        psMoveEnabledElement.SetAttribute("value", enablePSMove.ToString());
        psMoveSettingsElement.AppendChild(psMoveEnabledElement);

        XmlElement psMoveIPElement = xmlDoc.CreateElement("ip");
        psMoveIPElement.SetAttribute("value", PSMoveIP.ToString());
        psMoveSettingsElement.AppendChild(psMoveIPElement);

        XmlElement psMovePortElement = xmlDoc.CreateElement("port");
        psMovePortElement.SetAttribute("value", PSMovePort.ToString());
        psMoveSettingsElement.AppendChild(psMovePortElement);

        XmlElement psMoveAutoConnectElement = xmlDoc.CreateElement("autoConnect");
        psMoveAutoConnectElement.SetAttribute("value", connectToPSMoveOnStartup.ToString());
        psMoveSettingsElement.AppendChild(psMoveAutoConnectElement);

        XmlElement psMoveEnableInGameCalibration = xmlDoc.CreateElement("enableInGameCalibration");
        psMoveEnableInGameCalibration.SetAttribute("value", enableMoveCalibrationDuringPlay.ToString());
        psMoveSettingsElement.AppendChild(psMoveEnableInGameCalibration);

        XmlElement psMoveMaxControllersElement = xmlDoc.CreateElement("maxControllers");
        psMoveMaxControllersElement.SetAttribute("value", amountOfPSMoveControllers.ToString());
        psMoveSettingsElement.AppendChild(psMoveMaxControllersElement);



        XmlElement kinectSettingsElement = xmlDoc.CreateElement("KinectSettings");
        inputManagerRootElement.AppendChild(kinectSettingsElement);

        XmlElement kinectEnabledElement = xmlDoc.CreateElement("enabled");
        kinectEnabledElement.SetAttribute("value", enableKinect.ToString());
        kinectSettingsElement.AppendChild(kinectEnabledElement);

        XmlElement maxKinectPlayersElement = xmlDoc.CreateElement("maxPlayers");
        maxKinectPlayersElement.SetAttribute("value", maxNumberOfKinectPlayers.ToString());
        kinectSettingsElement.AppendChild(maxKinectPlayersElement);

        XmlElement kinectFloorDetectionElement = xmlDoc.CreateElement("floorDetection");
        kinectFloorDetectionElement.SetAttribute("value", kinectFloorDetection.ToString());
        kinectSettingsElement.AppendChild(kinectFloorDetectionElement);

        /*
        XmlElement displayUpElement = xmlDoc.CreateElement("displayUp");
        XMLUtil.WriteVector3ToXmlElement(displayUpElement, displayUpInternal);
        inputManagerRootElement.AppendChild(displayUpElement);

        XmlElement displayNormalElement = xmlDoc.CreateElement("displayNormal");
        XMLUtil.WriteVector3ToXmlElement(displayNormalElement, displayNormalInternal);
        inputManagerRootElement.AppendChild(displayNormalElement);

        linkedCamera.SaveKeystoningToXML(inputManagerRootElement);
        */
        XMLUtil.SaveXmlToFile(filename, xmlDoc);
        
        return true;
    }

    private IEnumerator CheckForMoveConnection()
    {
        yield return new WaitForSeconds(5.0f);
        if (!psMoveWrapper.isConnected)
        {
            Debug.LogError("Could not connect to PS Move server at: " + PSMoveIP + ":" + PSMovePort);
        }
    }
	
    private IEnumerator attemptStartingSceneAnalyzer()
    {
        if (kinectFloorDetection)
        {	
        	yield return new WaitForSeconds(2.0f);
			
			if(sceneAnalyzer == null)
			{
				Debug.Log ("Using existing sceneAnalyzer");
				sceneAnalyzer = moveKinectCalibration.sceneAnalyzer;
				//usingExistingSceneAnalyzer = true;
				//if(!sceneAnalyzer.IsGenerating) // Seems to be always on
	    		//	sceneAnalyzer.StartGenerating();
			}
		}
	}
	
    private IEnumerator attemptUpdatingFloorNormal()
    {
        if (kinectFloorDetection)
        {
        	yield return new WaitForSeconds(5.0f);
			coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
            if (!coordinateSystem)
            {
                Debug.LogError("Could not find coordinate system!");
            }
			else if(sceneAnalyzer == null)
				Debug.LogError("Failed to access OpenNI sceneAnalyzer!");
			else
			{
				Debug.Log ("Updating Kinect floor normal");
				coordinateSystem.ResetKinectFloorNormal();
				coordinateSystem.ResetKinectDistanceFromFloor();
		
		        OpenNI.Plane3D floor = sceneAnalyzer.Floor;
		        Vector3 newFloorNormal = new Vector3(floor.Normal.X, floor.Normal.Y, floor.Normal.Z).normalized;
		        Vector3 newFloorPosition = coordinateSystem.ConvertKinectPosition(floor.Point);
		        
		        //Project the position of the kinect camera onto the floor
		        //http://en.wikipedia.org/wiki/Point_on_plane_closest_to_origin
		        //http://en.wikipedia.org/wiki/Plane_(geometry)
		        float d = newFloorNormal.x * newFloorPosition.x + newFloorNormal.y * newFloorPosition.y + newFloorNormal.z * newFloorPosition.z;
		        Vector3 closestFloorPointToKinect = new Vector3(newFloorNormal.x, newFloorNormal.y, newFloorNormal.z);
		        closestFloorPointToKinect = (closestFloorPointToKinect * d) / closestFloorPointToKinect.sqrMagnitude;
		
		        //transform the point from Kinect's coordinate system rotation to Unity's rotation
		        closestFloorPointToKinect = Quaternion.FromToRotation(newFloorNormal, Vector3.up)  * closestFloorPointToKinect;
		
		        //floorPlane.transform.position = closestFloorPointToKinect;
		
		
		        coordinateSystem.SetKinectFloorNormal(newFloorNormal);
		        //floorNormal = newFloorNormal.normalized;
		        coordinateSystem.SetKinectDistanceFromFloor(closestFloorPointToKinect.magnitude);
				
				//if(!usingExistingSceneAnalyzer)
				//	sceneAnalyzer.StopGenerating();
			}
        }
    }
	
    private void DisableUnneededMoveWands()
    {
        List<RUISPSMoveWand> childWands = new List<RUISPSMoveWand>(GetComponentsInChildren<RUISPSMoveWand>());


        foreach (RUISPSMoveWand moveController in FindObjectsOfType(typeof(RUISPSMoveWand)) as RUISPSMoveWand[])
        {
            if (!childWands.Contains(moveController) && (!enablePSMove || !psMoveWrapper.isConnected || moveController.controllerId >= amountOfPSMoveControllers))
            {
                Debug.LogWarning("Disabling PS Move Controller: " + moveController.name);
                moveController.enabled = false;
                RUISWandSelector wandSelector = moveController.GetComponent<RUISWandSelector>();
                if (wandSelector)
                {
                    wandSelector.enabled = false;
                    LineRenderer lineRenderer = wandSelector.GetComponent<LineRenderer>();
                    if (lineRenderer)
                    {
                        lineRenderer.enabled = false;
                    }
                }
            }
        }
    }

    public RUISPSMoveWand GetMoveWand(int i)
    {
		if(psMoveWrapper.isConnected)
		{
	        if (i < 0 || i + 1 > amountOfPSMoveControllers || i >= moveControllers.Length)
	        {
				Debug.LogError("RUISPSMoveWand with ID " + i + " was not found in RUISInputManager");
	            return null;
	        }

        	return moveControllers[i];
		}
		else
			return null;
    }
}
