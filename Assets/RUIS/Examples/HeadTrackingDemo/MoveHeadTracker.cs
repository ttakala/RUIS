using UnityEngine;
using System.Collections;

public class MoveHeadTracker : MonoBehaviour {

    public float speedScaler = 5.0f;
    public float rotationScaler = 90.0f;

	void Update () {
        transform.Translate(Vector3.up * Input.GetAxis("Vertical") * speedScaler * Time.deltaTime);
        transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * speedScaler * Time.deltaTime);
        transform.Translate(Vector3.forward * (Input.GetKey(KeyCode.E) ? 1 : 0) * speedScaler * Time.deltaTime);
        transform.Translate(Vector3.forward * (Input.GetKey(KeyCode.Q) ? -1 : 0) * speedScaler * Time.deltaTime);
        //transform.rotation = Quaternion.Euler(Vector3.up * rotationScaler * Mathf.Sin(Time.timeSinceLevelLoad));
	}
}
