/*****************************************************************************

Content    :   A script to modify a collider on the fly to stabilize the rigidbody controlled by kinect
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

//assumes kinect ground is at Y = 0
[RequireComponent(typeof(CapsuleCollider))]
public class RUISCharacterStabilizingCollider : MonoBehaviour {
    public RUISPlainSkeletonController skeletonController;

    RUISSkeletonManager skeletonManager;
    int playerId;

    private CapsuleCollider capsuleCollider;

    public float maxHeightChange = 5f;
    public float maxPositionChange = 10f;
    public float colliderRadiusTweaker = 1.5f;

    private float defaultColliderHeight;
    private Vector3 defaultColliderPosition;

    private float _colliderHeight;
    public float colliderHeight
    {
        get
        {
            return _colliderHeight;
        }
        private set
        {
            _colliderHeight = value;
            capsuleCollider.height = _colliderHeight;
        }
    }

	void Start () {
        skeletonManager = skeletonController.skeletonManager;
        playerId = skeletonController.playerId;

        capsuleCollider = GetComponent<CapsuleCollider>();
        defaultColliderHeight = capsuleCollider.height;
        defaultColliderPosition = transform.localPosition;
	}
	
	void FixedUpdate () {
        if (!skeletonManager.skeletons[playerId].isTracking)
        {
            colliderHeight = defaultColliderHeight;
            transform.localPosition = defaultColliderPosition;

            return;
        }

        Vector3 torsoPos = skeletonManager.skeletons[playerId].torso.position;
        Vector3 newPos = torsoPos;
        newPos.y = torsoPos.y / 2;

        colliderHeight = Mathf.Lerp(capsuleCollider.height, torsoPos.y - capsuleCollider.radius * colliderRadiusTweaker, maxHeightChange * Time.fixedDeltaTime);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, maxPositionChange * Time.fixedDeltaTime);
	}
}
