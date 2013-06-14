using UnityEngine;
using System.Collections;

public class RUISCharacterStabilizingCollider : MonoBehaviour {
    public Transform transformToFollow;

	void Start () {
	
	}
	
	void FixedUpdate () {
        Vector3 newPos = transformToFollow.localPosition;
        //newPos.y = 0;
        transform.localPosition = newPos;
	}
}
