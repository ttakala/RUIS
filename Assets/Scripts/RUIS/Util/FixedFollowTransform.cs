using UnityEngine;
using System.Collections;

public class FixedFollowTransform : MonoBehaviour {
    public Transform transformToFollow;
    public Vector3 offset;
    public bool lookAt;
	
	void LateUpdate () {
        transform.position = transformToFollow.position + offset;
        if (lookAt)
        {
            transform.rotation = Quaternion.LookRotation(transformToFollow.position - transform.position);
        }
	}
}
