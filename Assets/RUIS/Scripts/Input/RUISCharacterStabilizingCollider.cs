using UnityEngine;
using System.Collections;

public class RUISCharacterStabilizingCollider : MonoBehaviour {
    public RUISPlainSkeletonController skeletonController;

	void Start () {
	
	}
	
	void FixedUpdate () {
        Vector3 newPos = skeletonController.transform.localPosition;
        //newPos.y = 0;
        transform.localPosition = newPos;
	}
}
