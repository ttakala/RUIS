using UnityEngine;
using System.Collections;

public class RUISMoveCalibrationVisualizer : MonoBehaviour {
    public GameObject kinectCalibrationSphere;
    private LineRenderer lineRenderer;

	void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(2);
	}
	
	void Update () {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, kinectCalibrationSphere.transform.position);
	}
}
