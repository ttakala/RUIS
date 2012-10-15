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

    public bool enableKinect = true;

    public RUISInputManager()
    {
    }

    public void Awake()
    {
        if (!Application.isEditor || loadFromTextFileInEditor)
        {
            Import(filename);
        }

        if (enablePSMove)
        {
            psMoveWrapper = GetComponentInChildren<PSMoveWrapper>();

            if (psMoveWrapper && connectToPSMoveOnStartup)
            {
                psMoveWrapper.Connect(PSMoveIP, PSMovePort);
            }
        }
    }

    public void OnApplicationQuit()
    {
        if(psMoveWrapper && psMoveWrapper.isConnected)
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

        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
            return false;
        }

        Debug.Log("Imported Input Config");

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
