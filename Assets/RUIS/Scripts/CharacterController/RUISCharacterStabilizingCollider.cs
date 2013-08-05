/*****************************************************************************

Content    :   A script to modify a collider on the fly to stabilize the rigidbody controlled by kinect
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;

//assumes kinect ground is at Y = 0
[RequireComponent(typeof(CapsuleCollider))]
public class RUISCharacterStabilizingCollider : MonoBehaviour 
{
    public RUISPlainSkeletonController skeletonController;

    RUISSkeletonManager skeletonManager;
    int playerId = 0;

    private CapsuleCollider capsuleCollider;
	
    public float maxHeightChange = 5f;
    public float maxPositionChange = 10f;
    public float colliderHeightTweaker = 0.0f;

    private float defaultColliderHeight;
    private Vector3 defaultColliderPosition;
	
	private bool kinectAndMecanimCombinerExists = false;
	private bool combinerChildrenInstantiated = false;
	
	private KalmanFilter positionKalman;
	private double[] measuredPos = {0, 0, 0};
	private double[] pos = {0, 0, 0};
	private float positionNoiseCovariance = 1500;
	
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

	void Awake () 
	{
        skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		if(skeletonController != null)
	        playerId = skeletonController.playerId;
		else
			Debug.LogError(   "The public variable 'Skeleton Controller' is not assigned! Using skeleton "
							+ "#0 as the torso position source.");
		
		if(gameObject.transform.parent != null)
		{
			if(gameObject.transform.parent.GetComponentInChildren<RUISKinectAndMecanimCombiner>())
				kinectAndMecanimCombinerExists = true;
		}

        capsuleCollider = GetComponent<CapsuleCollider>();
        defaultColliderHeight = capsuleCollider.height;
        defaultColliderPosition = transform.localPosition;
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3,3);
		positionKalman.skipIdenticalMeasurements = true;
	}
	
	void FixedUpdate () 
	{
		Vector3 torsoPos;
        if (!skeletonManager || !skeletonManager.skeletons[playerId].isTracking)
        {
            colliderHeight = defaultColliderHeight;
            // Tuukka:
            // Original skeletonController has been destroyed because the GameObject which had
            // it has been split in three parts: Kinect, Mecanim, Blended. Lets fetch the new one.
            if (!combinerChildrenInstantiated && kinectAndMecanimCombinerExists)
            {
                if (gameObject.transform.parent != null)
                {
                    RUISKinectAndMecanimCombiner combiner =
                                gameObject.transform.parent.GetComponentInChildren<RUISKinectAndMecanimCombiner>();
                    if (combiner && combiner.isChildrenInstantiated())
                    {
                        skeletonController = combiner.skeletonController;
                        combinerChildrenInstantiated = true;
                    }
                }
            }

            if (combinerChildrenInstantiated)
            {
                if (skeletonController.followMoveController)
                {
                    //transform.localPosition = skeletonController.transform.localPosition;// + 0.5f*colliderHeight*Vector3.up;
                    torsoPos = skeletonController.transform.localPosition + defaultColliderHeight * Vector3.up;
					
					measuredPos[0] = torsoPos.x;
					measuredPos[1] = torsoPos.y;
					measuredPos[2] = torsoPos.z;
					positionKalman.setR(Time.fixedDeltaTime * positionNoiseCovariance);
				    positionKalman.predict();
				    positionKalman.update(measuredPos);
					pos = positionKalman.getState();
					torsoPos.x = (float) pos[0];
					torsoPos.y = (float) pos[1];
					torsoPos.z = (float) pos[2];
                }
                else
                {
                    colliderHeight = defaultColliderHeight;
                    transform.localPosition = defaultColliderPosition;
                    return;
                }
            }
            else
            {
                colliderHeight = defaultColliderHeight;
                transform.localPosition = defaultColliderPosition;
                return;
            }
        }
        else
        {
            torsoPos = skeletonManager.skeletons[playerId].torso.position;
			
			measuredPos[0] = torsoPos.x;
			measuredPos[1] = torsoPos.y;
			measuredPos[2] = torsoPos.z;
			positionKalman.setR(Time.fixedDeltaTime * positionNoiseCovariance);
		    positionKalman.predict();
		    positionKalman.update(measuredPos);
			pos = positionKalman.getState();
			torsoPos.x = (float) pos[0];
			torsoPos.y = (float) pos[1];
			torsoPos.z = (float) pos[2];
        }
		
        Vector3 newPos = torsoPos;
        newPos.y = torsoPos.y / 2;

        colliderHeight = Mathf.Lerp(capsuleCollider.height, torsoPos.y + colliderHeightTweaker, maxHeightChange * Time.fixedDeltaTime);
        
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, newPos, maxPositionChange * Time.fixedDeltaTime);
	}
	
}
