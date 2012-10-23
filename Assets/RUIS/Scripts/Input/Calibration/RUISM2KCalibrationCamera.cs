using UnityEngine;
using System.Collections;

public class RUISM2KCalibrationCamera : MonoBehaviour {
    public GameObject calibrationVisualizationCube;

    private float currentTime;
    private float theta = Mathf.PI / 3;
    private float phi = 0;
    public float r = 2;
    public float rotationSpeed = 0.1f;

    void Start()
    {
        currentTime = 0;
    }

	// Update is called once per frame
	void Update () {
        currentTime += Time.deltaTime;
        phi = currentTime * rotationSpeed;
        float newX = r * Mathf.Sin(theta) * Mathf.Cos(phi);
        float newZ = r * Mathf.Sin(theta) * Mathf.Sin(phi);
        float newY = r * Mathf.Cos(theta);
        this.transform.position = new Vector3(newX, newY, newZ);
        transform.LookAt(calibrationVisualizationCube.transform);
	}
}
