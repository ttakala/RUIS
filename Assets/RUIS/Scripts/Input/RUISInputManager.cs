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
                psMoveWrapper.Connect(PSMoveIP, PSMovePort);
            }

            psMoveWrapper.enableDefaultInGameCalibrate = enableMoveCalibrationDuringPlay;

            //disable all controllers that shouldn't be connected
            foreach (RUISPSMoveWand moveController in FindObjectsOfType(typeof(RUISPSMoveWand)) as RUISPSMoveWand[])
            {
                if (moveController.controllerId >= amountOfPSMoveControllers)
                {
                    Debug.LogWarning("Disabling PS Move Controller: " + moveController.name + "... Controller ID was too big!");
                    moveController.gameObject.SetActiveRecursively(false);
                }
            }
        }
        else
        {
            psMoveWrapper.gameObject.SetActiveRecursively(false);

            foreach (RUISPSMoveWand moveController in FindObjectsOfType(typeof(RUISPSMoveWand)) as RUISPSMoveWand[])
            {
                Debug.LogWarning("Disabling PS Move Controller: " + moveController.name + "... PS Move not enabled in InputManager!");
                moveController.gameObject.SetActiveRecursively(false);
            }
        }

        if (enableKinect)
        {
        }
        else
        {
            transform.FindChild("Kinect").gameObject.SetActiveRecursively(false);
            transform.FindChild("SkeletonManager").gameObject.SetActiveRecursively(false);
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
}
