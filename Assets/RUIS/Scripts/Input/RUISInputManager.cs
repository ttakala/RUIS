using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class RUISInputManager : MonoBehaviour
{
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

    public void Awake()
    {
        if (!Application.isEditor || loadFromTextFileInEditor)
        {
            Import(filename);
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
            psMoveWrapper.gameObject.SetActiveRecursively(false);
        }

        DisableUnneededMoveWands();

        if (enableKinect)
        {
        }
        else
        {
            BroadcastMessage("KinectNotAvailable", SendMessageOptions.DontRequireReceiver);
        } 
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
                BroadcastMessage("KinectNotAvailable", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void OnApplicationQuit()
    {
        if(enablePSMove && psMoveWrapper && psMoveWrapper.isConnected)
            psMoveWrapper.Disconnect(false);
    }

    public bool Import(string filename)
    {
        try
        {
            TextReader textReader = new StreamReader(filename);

            PSMoveIP = textReader.ReadLine();
            PSMovePort = int.Parse(textReader.ReadLine());

            textReader.Close();

            Debug.Log("Imported Input Config");
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
            return false;
        }

        return true;
    }

    public bool Export(string filename)
    {
        try
        {
            TextWriter textWriter = new StreamWriter(filename);

            textWriter.WriteLine(PSMoveIP);
            textWriter.WriteLine(PSMovePort);

            textWriter.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
            return false;
        }


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

    private void DisableUnneededMoveWands()
    {
        foreach (RUISPSMoveWand moveController in FindObjectsOfType(typeof(RUISPSMoveWand)) as RUISPSMoveWand[])
        {
            if (!enablePSMove || !psMoveWrapper.isConnected || moveController.controllerId >= amountOfPSMoveControllers)
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
}
