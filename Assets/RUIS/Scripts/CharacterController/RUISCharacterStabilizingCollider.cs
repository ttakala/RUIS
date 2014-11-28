/*****************************************************************************

Content    :   A script to modify a collider on the fly to stabilize the rigidbody controlled by kinect
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using OVR;

// Assumes that Kinect ground is at Y = 0
[RequireComponent(typeof(CapsuleCollider))]
public class RUISCharacterStabilizingCollider : MonoBehaviour 
{
	RUISSkeletonManager skeletonManager;
    RUISSkeletonController skeletonController;
	RUISCharacterController characterController;

	private RUISCoordinateSystem coordinateSystem;
	private float coordinateYOffset = 0;

    int playerId = 0;
	int bodyTrackingDeviceID = 0;
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
	
	Vector3 headPosition = Vector3.zero;
	Vector3 torsoPosition = Vector3.zero;
	Vector3 newLocalPosition = Vector3.zero;
	
	Hmd oculusHmdObject;

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
		
		if(gameObject.transform.parent != null)
		{
			if(gameObject.transform.parent.GetComponentInChildren<RUISKinectAndMecanimCombiner>())
				kinectAndMecanimCombinerExists = true;
		}

        capsuleCollider = GetComponent<CapsuleCollider>();
		if(capsuleCollider == null)
			Debug.LogError("GameObject " + gameObject.name + " must have a CapsuleCollider!");
        defaultColliderHeight = capsuleCollider.height;
        defaultColliderPosition = transform.localPosition;
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3,3);
		positionKalman.skipIdenticalMeasurements = true;
	}

	void Start()
	{
		coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
		
		if(transform.parent)
		{
			skeletonController = transform.parent.gameObject.GetComponentInChildren(typeof(RUISSkeletonController)) as RUISSkeletonController;
			if(skeletonController)
			{
				playerId = skeletonController.playerId;
				bodyTrackingDeviceID = skeletonController.bodyTrackingDeviceID;
			}
		}

		if(transform.parent)
			characterController = transform.parent.GetComponent<RUISCharacterController>();

		if(UnityEditorInternal.InternalEditorUtility.HasPro()) // TODO: remove when Oculus works in free version
		{
			try
			{
				oculusHmdObject = Hmd.GetHmd();
			}
			catch(UnityException e)
			{
				Debug.LogError(e);
			}
		}
	}
	
	void FixedUpdate () 
	{
		if(coordinateSystem)
		{
			coordinateYOffset = coordinateSystem.positionOffset.y;
		}

//		if(characterController != null && characterController.useOculusPositionalTracking && UnityEditorInternal.InternalEditorUtility.HasPro()) // TODO: remove when Oculus works in free version
//		{
//			if(OVRDevice.IsCameraTracking() && oculusHmdObject != null)
//			{
//				ovrTrackingState trackingState = oculusHmdObject.GetTrackingState(Hmd.GetTimeInSeconds());
//				
//				headPosition = new Vector3(trackingState.HeadPose.ThePose.Position.x, trackingState.HeadPose.ThePose.Position.y, trackingState.HeadPose.ThePose.Position.z);
//				torsoPosition = coordinateSystem.ConvertLocation(coordinateSystem.ConvertRawOculusDK2Location(headPosition), RUISDevice.Oculus_DK2);
//
//				measuredPos[0] = torsoPosition.x;
//				measuredPos[1] = torsoPosition.y;
//				measuredPos[2] = torsoPosition.z;
//				positionKalman.setR(Time.fixedDeltaTime * positionNoiseCovariance);
//				positionKalman.predict();
//				positionKalman.update(measuredPos);
//				pos = positionKalman.getState();
//				torsoPosition.x = (float) pos[0];
//				torsoPosition.y = (float) pos[1] - coordinateYOffset;
//				torsoPosition.z = (float) pos[2];
//
//				newLocalPosition = torsoPosition;
//				newLocalPosition.y = (torsoPosition.y)/ 2 + coordinateYOffset; 
//			}
//		}

		if (!skeletonManager || !skeletonManager.skeletons [bodyTrackingDeviceID, playerId].isTracking) 
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

						if(skeletonController == null)
							Debug.LogError(  "Could not find Component " + typeof(RUISSkeletonController) + " from "
							               + "children of " + gameObject.transform.parent.name
							               + ", something is very wrong with this character setup!");

						bodyTrackingDeviceID = skeletonController.bodyTrackingDeviceID;
						playerId = skeletonController.playerId;
                        combinerChildrenInstantiated = true;
                    }
                }
            }

            if (combinerChildrenInstantiated)
            {
                if (skeletonController.followMoveController)
                {
					// TODO *** Check that this works with other models. Before with grandma model torsoPos value was:
                    //torsoPos = skeletonController.transform.localPosition + defaultColliderHeight * Vector3.up;
					torsoPosition = skeletonController.transform.localPosition;
					torsoPosition.y = defaultColliderHeight; // torsoPos.y is lerped and 0 doesn't seem to work
					newLocalPosition = torsoPosition;
					newLocalPosition.y = defaultColliderPosition.y;
					
					measuredPos[0] = torsoPosition.x;
					measuredPos[1] = torsoPosition.y;
					measuredPos[2] = torsoPosition.z;
					positionKalman.setR(Time.fixedDeltaTime * positionNoiseCovariance);
				    positionKalman.predict();
				    positionKalman.update(measuredPos);
					pos = positionKalman.getState();
					torsoPosition.x = (float) pos[0];
					torsoPosition.y = (float) pos[1];
					torsoPosition.z = (float) pos[2];
                }
                else
                {
                    //colliderHeight = defaultColliderHeight;
                    //transform.localPosition = defaultColliderPosition;
                    return;
                }
            }
            else
            {
                //colliderHeight = defaultColliderHeight;
                //transform.localPosition = defaultColliderPosition;
                return;
            }
        }
        else
        {
        	torsoPosition = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.position;
			
			measuredPos[0] = torsoPosition.x;
			measuredPos[1] = torsoPosition.y;
			measuredPos[2] = torsoPosition.z;
			positionKalman.setR(Time.fixedDeltaTime * positionNoiseCovariance);
		    positionKalman.predict();
		    positionKalman.update(measuredPos);
			pos = positionKalman.getState();
			torsoPosition.x = (float) pos[0];
			torsoPosition.y = (float) pos[1] - coordinateYOffset;
			torsoPosition.z = (float) pos[2];

			// Capsule collider is from floor up till torsoPos, therefore the capsule's center point is half of that
			newLocalPosition = torsoPosition;
			newLocalPosition.y = (torsoPosition.y)/ 2 + coordinateYOffset; 
        }

		// Updated collider height (from floor to torsoPos)
		colliderHeight = Mathf.Lerp(capsuleCollider.height, torsoPosition.y + colliderHeightTweaker, maxHeightChange * Time.fixedDeltaTime);

		// Updated collider position
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, newLocalPosition, maxPositionChange * Time.fixedDeltaTime);
	}
	
}
