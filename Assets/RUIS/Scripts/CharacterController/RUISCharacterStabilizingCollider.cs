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
    int playerId;

    private CapsuleCollider capsuleCollider;
	
    public float maxHeightChange = 5f;
    public float maxPositionChange = 10f;
    public float colliderHeightTweaker = 0.0f;

    private float defaultColliderHeight;
    private Vector3 defaultColliderPosition;
	
	private bool kinectAndMecanimCombinerExists = false;
	private bool combinerChildrenInstantiated = false;
	
	private KalmanFilter heightKalman;
	private double[] measuredHeight = {0};
	private double[] colHeight = {0};
	private float heightNoiseCovariance = 500;
	
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
        playerId = skeletonController.playerId;
		
		if(gameObject.transform.parent != null)
		{
			if(gameObject.transform.parent.GetComponentInChildren<RUISKinectAndMecanimCombiner>())
				kinectAndMecanimCombinerExists = true;
		}

        capsuleCollider = GetComponent<CapsuleCollider>();
        defaultColliderHeight = capsuleCollider.height;
        defaultColliderPosition = transform.localPosition;
		
		heightKalman = new KalmanFilter();
		heightKalman.initialize(1,1);
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
			
			measuredHeight[0] = torsoPos.y;
			heightKalman.setR(Time.fixedDeltaTime * heightNoiseCovariance);
		    heightKalman.predict();
		    heightKalman.update(measuredHeight);
			colHeight = heightKalman.getState();
			torsoPos.y = (float) colHeight[0];
        }
		
        Vector3 newPos = torsoPos;
        newPos.y = torsoPos.y / 2;

        colliderHeight = Mathf.Lerp(capsuleCollider.height, torsoPos.y + colliderHeightTweaker, maxHeightChange * Time.fixedDeltaTime); //TUUKKA ************************************
        
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, newPos, maxPositionChange * Time.fixedDeltaTime);
	}
	
}
