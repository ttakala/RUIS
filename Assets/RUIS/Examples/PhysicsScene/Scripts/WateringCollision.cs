/*****************************************************************************

Content    :   Script to grow the PhysicsScene tree when hit by water particles
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

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
