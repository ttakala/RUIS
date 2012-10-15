using UnityEngine;
using System.Collections;

public class RUISCalibrationDepthMap : MonoBehaviour {
    NIDepthmapViewerUtility depthMapViewer;
	
	void Start () {
        depthMapViewer = GetComponent<NIDepthmapViewerUtility>();
        depthMapViewer.m_placeToDraw.height = Screen.height / 2;
        depthMapViewer.m_placeToDraw.width = Screen.width / 2;
	}
}
