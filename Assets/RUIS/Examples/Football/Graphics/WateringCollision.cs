using UnityEngine;
using System.Collections;

public class WateringCollision : MonoBehaviour {

	public GameObject movingUp;
	private Vector3 move = new Vector3(0, 0.02f, 0);
	
	
	void OnCollisionEnter(Collision collision) {
		if(collision.gameObject.tag=="WaterParticle")
		{ 
			if(movingUp.transform.position.y < 0.3f)
				movingUp.transform.position += move;
			print (movingUp.transform.position.y);
			Destroy(collision.gameObject);
		}
	}
}
