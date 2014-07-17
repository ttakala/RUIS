using UnityEngine;
using System.Collections;

public class FollowRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		GameObject a = GameObject.Find("a");
		GameObject b = GameObject.Find("b");
		GameObject vectorUp = GameObject.Find("vectorUp");

		Vector3 rotationVector = a.transform.position - b.transform.position;
		Quaternion elbowRotation = a.transform.rotation;

		vectorUp.transform.rotation = Quaternion.LookRotation (elbowRotation * Vector3.up);
		this.transform.rotation = Quaternion.LookRotation (rotationVector, elbowRotation * Vector3.up);


	}
}
