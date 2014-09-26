/*****************************************************************************

Content    :   Functionality to control a skeleton using Kinect
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public enum RUISAxis
{
	X, Y, Z
}

[AddComponentMenu("RUIS/Input/RUISSkeletonController")]
public class RUISSkeletonController : MonoBehaviour
{


    public Transform root;
    public Transform head;
    public Transform neck;
    public Transform torso;
    public Transform rightShoulder;
    public Transform rightElbow;
    public Transform rightHand;
    public Transform rightHip;
    public Transform rightKnee;
    public Transform rightFoot;
    public Transform leftShoulder;
    public Transform leftElbow;
    public Transform leftHand;
    public Transform leftHip;
    public Transform leftKnee;
    public Transform leftFoot;
	public Transform leftThumb;
	public Transform rightThumb;

	// Transform sources for custom motion tracking
	public Transform customRoot;
	public Transform customHead;
	public Transform customNeck;
	public Transform customTorso;
	public Transform customRightShoulder;
	public Transform customRightElbow;
	public Transform customRightHand;
	public Transform customRightHip;
	public Transform customRightKnee;
	public Transform customRightFoot;
	public Transform customLeftShoulder;
	public Transform customLeftElbow;
	public Transform customLeftHand;
	public Transform customLeftHip;
	public Transform customLeftKnee;
	public Transform customLeftFoot;
	public Transform customLeftThumb;
	public Transform customRightThumb;

	public bool fistCurlFingers;
	public bool trackThumbs;
	public bool trackAnkle;
	public bool rotateWristFromElbow;
	
	private RUISInputManager inputManager;
    private RUISSkeletonManager skeletonManager;
	private RUISCharacterController characterController;

	public enum bodyTrackingDeviceType
	{
		Kinect1,
		Kinect2,
		GenericMotionTracker
	}
	public bodyTrackingDeviceType bodyTrackingDevice = bodyTrackingDeviceType.Kinect1;

	public int bodyTrackingDeviceID = 0;
    public int playerId = 0;

    private Vector3 skeletonPosition = Vector3.zero;

    public bool updateRootPosition = true;
    public bool updateJointPositions = true;
    public bool updateJointRotations = true;

    public bool useHierarchicalModel = false;
	public bool scaleHierarchicalModelBones = true;
	public bool scaleBoneLengthOnly = false;
	public RUISAxis boneLengthAxis = RUISAxis.X;
	public float maxScaleFactor = 0.01f;

    public float minimumConfidenceToUpdate = 0.5f;
	public float rotationDamping = 15.0f;
	
	public float handRollAngleMinimum = -180; // Constrained between [0, -180] in Unity Editor script
	public float handRollAngleMaximum =  180; // Constrained between [0,  180] in Unity Editor script
	
	public bool followMoveController { get; private set; }
	private int followMoveID = 0;
	private RUISPSMoveWand psmove;
	
	private KalmanFilter positionKalman;
	private double[] measuredPos = {0, 0, 0};
	private double[] pos = {0, 0, 0};
	private float positionNoiseCovariance = 500;
	
    private Dictionary<Transform, Quaternion> jointInitialRotations;
    private Dictionary<KeyValuePair<Transform, Transform>, float> jointInitialDistances;

    [HideInInspector]
    public float torsoOffset = 0.0f;
    [HideInInspector]
    public float torsoScale = 1.0f;

    public float neckHeightTweaker = 0.0f;
    private Vector3 neckOriginalLocalPosition;

    public float forearmLengthRatio = 1.0f;
    private Vector3 originalRightForearmScale;
    private Vector3 originalLeftForearmScale;
	
	public float shinLengthRatio = 1.0f;
	private Vector3 originalRightShinScale;
	private Vector3 originalLeftShinScale;
	
	
	Quaternion[,,] initialFingerRotations = new Quaternion[2,5,3]; // 2 hands, 5 fingers, 3 finger bones
	Transform[,,] fingerTransforms = new Transform[2,5,3]; // For quick access to finger gameobjects
	
	// Thumb phalanges
	Quaternion clenchedRotationThumbTM = Quaternion.Euler (45, 0, 0); 
	Quaternion clenchedRotationThumbMCP = Quaternion.Euler (0, 0, -25 );
	Quaternion clenchedRotationThumbIP = Quaternion.Euler (0, 0, -80);
	
	// Phalanges of other fingers
	Quaternion clenchedRotationMCP = Quaternion.Euler (0, 0, -45);
	Quaternion clenchedRotationPIP = Quaternion.Euler (0, 0, -100);
	Quaternion clenchedRotationDIP = Quaternion.Euler (0, 0, -70);
	
    void Awake()
    {
		if(bodyTrackingDevice == bodyTrackingDeviceType.Kinect1) bodyTrackingDeviceID = 0;
		if(bodyTrackingDevice == bodyTrackingDeviceType.Kinect2)  bodyTrackingDeviceID = 1;
		if(bodyTrackingDevice == bodyTrackingDeviceType.GenericMotionTracker)  bodyTrackingDeviceID = 2;

		followMoveController = false;
		
        jointInitialRotations = new Dictionary<Transform, Quaternion>();
        jointInitialDistances = new Dictionary<KeyValuePair<Transform, Transform>, float>();
		
		positionKalman = new KalmanFilter();
		positionKalman.initialize(3,3);
    }

    void Start()
    {
		
		if (skeletonManager == null)
		{
			skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
			if (!skeletonManager)
				Debug.LogError("The scene is missing " + typeof(RUISSkeletonManager) + " script!");
		}

        if (useHierarchicalModel)
        {
            //fix all shoulder and hip rotations to match the default kinect rotations
            rightShoulder.rotation = FindFixingRotation(rightShoulder.position, rightElbow.position, transform.right) * rightShoulder.rotation;
            leftShoulder.rotation = FindFixingRotation(leftShoulder.position, leftElbow.position, -transform.right) * leftShoulder.rotation;
            rightHip.rotation = FindFixingRotation(rightHip.position, rightFoot.position, -transform.up) * rightHip.rotation;
            leftHip.rotation = FindFixingRotation(leftHip.position, leftFoot.position, -transform.up) * leftHip.rotation;

            Vector3 assumedRootPos = (rightShoulder.position + leftShoulder.position + leftHip.position + rightHip.position) / 4;
            Vector3 realRootPos = torso.position;
            torsoOffset = (realRootPos - assumedRootPos).y;

            if (neck)
            {
                neckOriginalLocalPosition = neck.localPosition;
            }
        }

        SaveInitialRotation(root);
        SaveInitialRotation(head);
        SaveInitialRotation(torso);
        SaveInitialRotation(rightShoulder);
        SaveInitialRotation(rightElbow);
        SaveInitialRotation(rightHand);
        SaveInitialRotation(leftShoulder);
        SaveInitialRotation(leftElbow);
        SaveInitialRotation(leftHand);
        SaveInitialRotation(rightHip);
        SaveInitialRotation(rightKnee);
        SaveInitialRotation(rightFoot);
        SaveInitialRotation(leftHip);
        SaveInitialRotation(leftKnee);
        SaveInitialRotation(leftFoot);

		SaveInitialRotation(leftThumb);
		SaveInitialRotation(rightThumb);
		saveInitialFingerRotations();
		
        SaveInitialDistance(rightShoulder, rightElbow);
        SaveInitialDistance(rightElbow, rightHand);
        SaveInitialDistance(leftShoulder, leftElbow);
        SaveInitialDistance(leftElbow, leftHand);

        SaveInitialDistance(rightHip, rightKnee);
        SaveInitialDistance(rightKnee, rightFoot);
        SaveInitialDistance(leftHip, leftKnee);
        SaveInitialDistance(leftKnee, leftFoot);

        SaveInitialDistance(torso, head);

        SaveInitialDistance(rightShoulder, leftShoulder);
        SaveInitialDistance(rightHip, leftHip);

        if (rightElbow)
        {
            originalRightForearmScale = rightElbow.localScale;
        }

        if (leftElbow)
        {
            originalLeftForearmScale = leftElbow.localScale;
        }

		if(rightKnee)
		{
			originalRightShinScale = rightKnee.localScale;
		}
		
		if(leftKnee)
		{
			originalLeftShinScale = leftKnee.localScale;
		}

		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		if(inputManager)
		{

			if(gameObject.transform.parent != null)
			{
				characterController = gameObject.transform.parent.GetComponent<RUISCharacterController>();
				if(characterController != null)
					if(		characterController.characterPivotType == RUISCharacterController.CharacterPivotType.MoveController
						&&	inputManager.enablePSMove																			)
					{
						followMoveController = true;
						followMoveID = characterController.moveControllerId;
						if(		 gameObject.GetComponent<RUISKinectAndMecanimCombiner>() == null 
							||	!gameObject.GetComponent<RUISKinectAndMecanimCombiner>().enabled )
							Debug.LogWarning(	"Using PS Move controller #" + characterController.moveControllerId + " as a source "
											 +	"for avatar root position of " + gameObject.name + ", because Kinect is disabled "
											 +	"and PS Move is enabled, while that PS Move controller has been assigned as a "
											 +	"Character Pivot in " + gameObject.name + "'s parent GameObject");
					}
			}
		}
    }

    void LateUpdate()
    {
		// If a custom skeleton tracking source is used, save its data into skeletonManager (which is a little 
		// topsy turvy) so we can utilize same code as we did with Kinect 1 and 2
		if(bodyTrackingDevice == bodyTrackingDeviceType.GenericMotionTracker) {
			
			skeletonManager.skeletons [bodyTrackingDeviceID, playerId].isTracking = true;

			if(customRoot) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.rotation = customRoot.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.position = customRoot.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.rotationConfidence = 1;
			}
			if(customHead) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head.rotation = customHead.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head.position = customHead.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].head.rotationConfidence = 1;
			}
			if(customNeck) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].neck.rotation = customNeck.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].neck.position = customNeck.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].neck.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].neck.rotationConfidence = 1;
			}
			if(customTorso) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.rotation = customTorso.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.position = customTorso.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.rotationConfidence = 1;
			}
			if(customRightShoulder) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder.rotation = customRightShoulder.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder.position = customRightShoulder.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder.rotationConfidence = 1;
			}
			if(customLeftShoulder) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder.rotation = customLeftShoulder.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder.position = customLeftShoulder.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder.rotationConfidence = 1;
			}
			if(customRightElbow) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightElbow.rotation = customRightElbow.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightElbow.position = customRightElbow.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightElbow.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightElbow.rotationConfidence = 1;
			}
			if(customLeftElbow) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftElbow.rotation = customLeftElbow.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftElbow.position = customLeftElbow.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftElbow.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftElbow.rotationConfidence = 1;
			}
			if(customRightHand) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand.rotation = customRightHand.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand.position = customRightHand.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand.rotationConfidence = 1;
			}
			if(customLeftHand) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand.rotation = customLeftHand.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand.position = customLeftHand.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand.rotationConfidence = 1;
			}
			if(customRightHip) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip.rotation = customRightHip.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip.position = customRightHip.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip.rotationConfidence = 1;
			}
			if(customLeftHip) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip.rotation = customLeftHip.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip.position = customLeftHip.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip.rotationConfidence = 1;
			}
			if(customRightKnee) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightKnee.rotation = customRightKnee.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightKnee.position = customRightKnee.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightKnee.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightKnee.rotationConfidence = 1;
			}
			if(customLeftKnee) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftKnee.rotation = customLeftKnee.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftKnee.position = customLeftKnee.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftKnee.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftKnee.rotationConfidence = 1;
			}
			if(customRightFoot) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightFoot.rotation = customRightFoot.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightFoot.position = customRightFoot.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightFoot.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightFoot.rotationConfidence = 1;
			}
			if(customLeftFoot) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftFoot.rotation = customLeftFoot.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftFoot.position = customLeftFoot.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftFoot.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftFoot.rotationConfidence = 1;
			}
			if(customRightThumb) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightThumb.rotation = customRightThumb.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightThumb.position = customRightThumb.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightThumb.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightThumb.rotationConfidence = 1;
			}
			if(customLeftThumb) {
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftThumb.rotation = customLeftThumb.rotation;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftThumb.position = customLeftThumb.position;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftThumb.positionConfidence = 1;
				skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftThumb.rotationConfidence = 1;
			}
		}

		// Update skeleton based on data fetched from skeletonManager
		if (	skeletonManager != null && skeletonManager.skeletons [bodyTrackingDeviceID, playerId] != null 
		    &&  skeletonManager.skeletons [bodyTrackingDeviceID, playerId].isTracking) {
						
				UpdateSkeletonPosition ();
				
				UpdateTransform (ref head, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].head);
				UpdateTransform (ref torso, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso);
				UpdateTransform (ref leftShoulder, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder);
				UpdateTransform (ref rightShoulder, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder);
				UpdateTransform (ref leftHand, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand);
				UpdateTransform (ref rightHand, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand);

				UpdateTransform (ref leftHip, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip);
				UpdateTransform (ref rightHip, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip);
				UpdateTransform (ref leftKnee, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftKnee);
				UpdateTransform (ref rightKnee, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightKnee);

				if(rotateWristFromElbow) {
					UpdateTransform (ref rightElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHand);
					UpdateTransform (ref leftElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHand);
				}
				else {
					UpdateTransform (ref rightElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightElbow);
					UpdateTransform (ref leftElbow, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftElbow);
				}
				
				// Hand rotation constraint 
				//if(bodyTrackingDeviceID == 1) {
				//	GameObject.Find("SCube").transform.localRotation = rightElbow.rotation) *
				//		skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightWrist.rotation;
//				rightHand.localRotation = limitZRotation(Quaternion.Inverse(rightElbow.rotation) *
//				                                         skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightWrist.rotation,  
//																				 handRollAngleMinimum,  handRollAngleMaximum);
				//}

				if(trackAnkle || !useHierarchicalModel) {
					UpdateTransform (ref leftFoot, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftFoot);
					UpdateTransform (ref rightFoot, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightFoot);
					
				}
		
				if(fistCurlFingers) handleFingersCurling(trackThumbs);
	
				if(trackThumbs && rightThumb && leftThumb) {
					UpdateTransform (ref rightThumb, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightThumb);
					UpdateTransform (ref leftThumb, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftThumb);
				}
				

				if (!useHierarchicalModel) {
						if (leftHand != null) {
								leftHand.localRotation = leftElbow.localRotation;
						}

						if (rightHand != null) {
								rightHand.localRotation = rightElbow.localRotation;
						}
				} else {
						if (scaleHierarchicalModelBones) {
								UpdateBoneScalings ();

								Vector3 torsoDirection = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.rotation * Vector3.down;
								torso.position = transform.TransformPoint (skeletonManager.skeletons [bodyTrackingDeviceID, playerId].torso.position - skeletonPosition - torsoDirection * torsoOffset * torsoScale);

								ForceUpdatePosition (ref rightShoulder, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightShoulder);
								ForceUpdatePosition (ref leftShoulder, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftShoulder);
								ForceUpdatePosition (ref rightHip, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHip);
								ForceUpdatePosition (ref leftHip, skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHip);
						}
				}

				if (updateRootPosition) {
						
						Vector3 newRootPosition = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].root.position;
		
						measuredPos [0] = newRootPosition.x;
						measuredPos [1] = newRootPosition.y;
						measuredPos [2] = newRootPosition.z;
						positionKalman.setR (Time.deltaTime * positionNoiseCovariance);
						positionKalman.predict ();
						positionKalman.update (measuredPos);
						pos = positionKalman.getState ();
		
						transform.localPosition = new Vector3 ((float)pos [0], (float)pos [1], (float)pos [2]); //newRootPosition;
				}
				
			
			} else { // Kinect is not tracking
					if (followMoveController && characterController && inputManager) {
							psmove = inputManager.GetMoveWand (followMoveID);
							if (psmove) {
			
									Quaternion moveYaw = Quaternion.Euler (0, psmove.localRotation.eulerAngles.y, 0);
			
									skeletonPosition = psmove.localPosition - moveYaw * characterController.psmoveOffset;
									skeletonPosition.y = 0;
				
									if (updateRootPosition)
											transform.localPosition = skeletonPosition;
				
									UpdateTransformWithPSMove (ref head, moveYaw);
									UpdateTransformWithPSMove (ref torso, moveYaw);
									UpdateTransformWithPSMove (ref leftShoulder, moveYaw);
									UpdateTransformWithPSMove (ref leftElbow, moveYaw);
									UpdateTransformWithPSMove (ref leftHand, moveYaw);
									UpdateTransformWithPSMove (ref rightShoulder, moveYaw);
									UpdateTransformWithPSMove (ref rightElbow, moveYaw);
									UpdateTransformWithPSMove (ref rightHand, moveYaw);
									UpdateTransformWithPSMove (ref leftHip, moveYaw);
									UpdateTransformWithPSMove (ref leftKnee, moveYaw);
									UpdateTransformWithPSMove (ref leftFoot, moveYaw);
									UpdateTransformWithPSMove (ref rightHip, moveYaw);
									UpdateTransformWithPSMove (ref rightKnee, moveYaw);
									UpdateTransformWithPSMove (ref rightFoot, moveYaw);
							}
					}
			}
		TweakNeckHeight();
    }

    private void UpdateTransform(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        if (updateJointPositions && jointToGet.positionConfidence >= minimumConfidenceToUpdate)
        {
            transformToUpdate.localPosition = jointToGet.position - skeletonPosition;
        }

        if (updateJointRotations && jointToGet.rotationConfidence >= minimumConfidenceToUpdate)
        {
            if (useHierarchicalModel)
            {
                Quaternion newRotation = transform.rotation * jointToGet.rotation *
                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
                transformToUpdate.rotation = Quaternion.RotateTowards(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDamping);
            }
            else
            {
                transformToUpdate.localRotation = Quaternion.RotateTowards(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDamping);
            }
        }
    }
	
	private void UpdateTransformWithPSMove(ref Transform transformToUpdate, Quaternion moveYaw)
    {
		if (transformToUpdate == null) return;
		
		//if (updateJointPositions) ;
		
		if (updateJointRotations)
		{
			if (useHierarchicalModel)
            {
//                Quaternion newRotation = transform.rotation * jointToGet.rotation *
//                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
//                transformToUpdate.rotation = Quaternion.Slerp(transformToUpdate.rotation, newRotation, Time.deltaTime * rotationDamping);
                Quaternion newRotation = transform.rotation * moveYaw *
                    (jointInitialRotations.ContainsKey(transformToUpdate) ? jointInitialRotations[transformToUpdate] : Quaternion.identity);
                transformToUpdate.rotation = newRotation;
            }
            else
            {
				transformToUpdate.localRotation = moveYaw;
//                transformToUpdate.localRotation = Quaternion.Slerp(transformToUpdate.localRotation, jointToGet.rotation, Time.deltaTime * rotationDamping);
            }
		}
	}

    private void ForceUpdatePosition(ref Transform transformToUpdate, RUISSkeletonManager.JointData jointToGet)
    {
        if (transformToUpdate == null) return;

        transformToUpdate.position = transform.TransformPoint(jointToGet.position - skeletonPosition);//transform.position + transform.rotation * (jointToGet.position - skeletonPosition);
    }

    //gets the main position of the skeleton inside the world, the rest of the joint positions will be calculated in relation to this one
    private void UpdateSkeletonPosition()
    {
		if (skeletonManager.skeletons[bodyTrackingDeviceID, playerId].root.positionConfidence >= minimumConfidenceToUpdate)
        {
			skeletonPosition = skeletonManager.skeletons[bodyTrackingDeviceID, playerId].root.position;
        }
    }

    private void SaveInitialRotation(Transform bodyPart)
    {
        if (bodyPart)
            jointInitialRotations[bodyPart] = GetInitialRotation(bodyPart);
    }

    private void SaveInitialDistance(Transform rootTransform, Transform distanceTo)
    {
        jointInitialDistances.Add(new KeyValuePair<Transform, Transform>(rootTransform, distanceTo), Vector3.Distance(rootTransform.position, distanceTo.position));
    }

    private Quaternion GetInitialRotation(Transform bodyPart)
    {
        return Quaternion.Inverse(transform.rotation) * bodyPart.rotation;
    }

    private void UpdateBoneScalings()
    {
        if (!ConfidenceGoodEnoughForScaling()) return;

        torsoScale = UpdateTorsoScale();

        {
            rightElbow.transform.localScale = originalRightForearmScale;
			float rightArmCumulativeScale = UpdateBoneScaling(rightShoulder, rightElbow, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightShoulder, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightElbow, torsoScale);
			UpdateBoneScaling(rightElbow, rightHand, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightElbow, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightHand, rightArmCumulativeScale);
			if(scaleBoneLengthOnly)
			{
				switch(boneLengthAxis)
				{
					case RUISAxis.X:
						rightElbow.transform.localScale = new Vector3(forearmLengthRatio * rightElbow.transform.localScale.x, rightElbow.transform.localScale.y, 
					                                           		  rightElbow.transform.localScale.z);
						break;
					case RUISAxis.Y:
						rightElbow.transform.localScale = new Vector3(rightElbow.transform.localScale.x, forearmLengthRatio * rightElbow.transform.localScale.y, 
					                                              rightElbow.transform.localScale.z);
						break;
					case RUISAxis.Z:
						rightElbow.transform.localScale = new Vector3(rightElbow.transform.localScale.x, rightElbow.transform.localScale.y, forearmLengthRatio *
					                                              rightElbow.transform.localScale.z);
						break;
				}
			}
			else
			   rightElbow.transform.localScale = rightElbow.transform.localScale * forearmLengthRatio;
        }

        {
            leftElbow.transform.localScale = originalLeftForearmScale;
			float leftArmCumulativeScale = UpdateBoneScaling(leftShoulder, leftElbow, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftShoulder, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftElbow, torsoScale);
			UpdateBoneScaling(leftElbow, leftHand, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftElbow, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftHand, leftArmCumulativeScale);
			if(scaleBoneLengthOnly)
			{
				switch(boneLengthAxis)
				{
					case RUISAxis.X:
						leftElbow.transform.localScale = new Vector3(forearmLengthRatio * leftElbow.transform.localScale.x, leftElbow.transform.localScale.y, 
						                                             leftElbow.transform.localScale.z);
						break;
					case RUISAxis.Y:
						leftElbow.transform.localScale = new Vector3(leftElbow.transform.localScale.x, forearmLengthRatio * leftElbow.transform.localScale.y, 
						                                             leftElbow.transform.localScale.z);
						break;
					case RUISAxis.Z:
						leftElbow.transform.localScale = new Vector3(leftElbow.transform.localScale.x, leftElbow.transform.localScale.y, forearmLengthRatio * 
						                                             leftElbow.transform.localScale.z);
						break;
				}
			}
			else
				leftElbow.transform.localScale = leftElbow.transform.localScale * forearmLengthRatio;
        }

        {
			rightKnee.transform.localScale = originalRightShinScale;
			float rightLegCumulativeScale = UpdateBoneScaling(rightHip, rightKnee, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightHip, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightKnee, torsoScale);
			UpdateBoneScaling(rightKnee, rightFoot, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightKnee, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightFoot, rightLegCumulativeScale);
			if(scaleBoneLengthOnly)
			{
				switch(boneLengthAxis)
				{
					case RUISAxis.X:
						rightKnee.transform.localScale = new Vector3(shinLengthRatio * rightKnee.transform.localScale.x, rightKnee.transform.localScale.y, 
						                                             rightKnee.transform.localScale.z);
						break;
					case RUISAxis.Y:
						rightKnee.transform.localScale = new Vector3(rightKnee.transform.localScale.x, shinLengthRatio * rightKnee.transform.localScale.y, 
						                                             rightKnee.transform.localScale.z);
						break;
					case RUISAxis.Z:
						rightKnee.transform.localScale = new Vector3(rightKnee.transform.localScale.x, rightKnee.transform.localScale.y, shinLengthRatio * 
						                                             rightKnee.transform.localScale.z);
						break;
				}
			}
			else
				rightKnee.transform.localScale = rightKnee.transform.localScale * shinLengthRatio;
        }

        {
			leftKnee.transform.localScale = originalLeftShinScale;
			float leftLegCumulativeScale = UpdateBoneScaling(leftHip, leftKnee, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftHip, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftKnee, torsoScale);
			UpdateBoneScaling(leftKnee, leftFoot, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftKnee, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftFoot, leftLegCumulativeScale);
			if(scaleBoneLengthOnly)
			{
				switch(boneLengthAxis)
				{
					case RUISAxis.X:
						leftKnee.transform.localScale = new Vector3(shinLengthRatio * leftKnee.transform.localScale.x, leftKnee.transform.localScale.y, 
						                                            leftKnee.transform.localScale.z);
						break;
					case RUISAxis.Y:
						leftKnee.transform.localScale = new Vector3(leftKnee.transform.localScale.x, shinLengthRatio * leftKnee.transform.localScale.y, 
						                                            leftKnee.transform.localScale.z);
						break;
					case RUISAxis.Z:
						leftKnee.transform.localScale = new Vector3(leftKnee.transform.localScale.x, leftKnee.transform.localScale.y, 
						                                            shinLengthRatio * leftKnee.transform.localScale.z);
						break;
				}
			}
			else
				leftKnee.transform.localScale = leftKnee.transform.localScale * shinLengthRatio;
        }
    }

    private float UpdateBoneScaling(Transform boneToScale, Transform comparisonBone, RUISSkeletonManager.JointData boneToScaleTracker, RUISSkeletonManager.JointData comparisonBoneTracker, float cumulativeScale)
    {
        float modelBoneLength = jointInitialDistances[new KeyValuePair<Transform, Transform>(boneToScale, comparisonBone)];
        float playerBoneLength = Vector3.Distance(boneToScaleTracker.position, comparisonBoneTracker.position);
        float newScale = playerBoneLength / modelBoneLength / cumulativeScale;

		if(scaleBoneLengthOnly)
		{
			switch(boneLengthAxis)
			{
			case RUISAxis.X:
				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, new Vector3(newScale, boneToScale.localScale.y, boneToScale.localScale.z), 
				                                             maxScaleFactor * Time.deltaTime);
				break;
			case RUISAxis.Y:
				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, new Vector3(boneToScale.localScale.x, newScale, boneToScale.localScale.z), 
				                                             maxScaleFactor * Time.deltaTime);
				break;
			case RUISAxis.Z:
				boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, new Vector3(boneToScale.localScale.x, boneToScale.localScale.y, newScale), 
				                                             maxScaleFactor * Time.deltaTime);
				break;
			}
		}
		else
			boneToScale.localScale = Vector3.MoveTowards(boneToScale.localScale, new Vector3(newScale, newScale, newScale), maxScaleFactor * Time.deltaTime);

        return boneToScale.localScale.x;
    }

    private float UpdateTorsoScale()
    {
        //average hip to shoulder length and compare it to the one found in the model - scale accordingly
        //we can assume hips and shoulders are set quite correctly, while we cannot be sure about the spine positions
        float modelLength = (jointInitialDistances[new KeyValuePair<Transform, Transform>(rightHip, leftHip)] +
                            jointInitialDistances[new KeyValuePair<Transform, Transform>(rightShoulder, leftShoulder)]) / 2;
		float playerLength = (Vector3.Distance(skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightShoulder.position, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftShoulder.position) +
		                      Vector3.Distance(skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightHip.position, skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftHip.position)) / 2;
		
        float newScale = playerLength / modelLength;
		torso.localScale = new Vector3(newScale, newScale, newScale);
        return newScale;
    }

    private Quaternion FindFixingRotation(Vector3 fromJoint, Vector3 toJoint, Vector3 wantedDirection)
    {
        Vector3 boneVector = toJoint - fromJoint;
        return Quaternion.FromToRotation(boneVector, wantedDirection);
    }

    private void TweakNeckHeight()
    {
        if (!neck) return;
        neck.localPosition = neckOriginalLocalPosition - neck.InverseTransformDirection(Vector3.up) * neckHeightTweaker;
    }

    public bool ConfidenceGoodEnoughForScaling()
    {
		return !(skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightShoulder.positionConfidence < minimumConfidenceToUpdate ||
		         skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftShoulder.positionConfidence < minimumConfidenceToUpdate ||
		         skeletonManager.skeletons[bodyTrackingDeviceID, playerId].rightHip.positionConfidence < minimumConfidenceToUpdate ||
		         skeletonManager.skeletons[bodyTrackingDeviceID, playerId].leftHip.positionConfidence < minimumConfidenceToUpdate);
    }

	private void handleFingersCurling(bool trackThumbs) {

		bool closeHand;
		int invert = 1;
		float rotationSpeed = 10.0f; // Per second
		Quaternion clenchedRotationThumbTM_corrected;
		
		for (int i = 0; i < 2; i++)  { // Hands
			
			if (i == 0) {
				closeHand = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].rightHandClosed;
				invert = -1;
			}
			else {
				closeHand = skeletonManager.skeletons [bodyTrackingDeviceID, playerId].leftHandClosed;	
				invert = 1;
			}
			// Thumb rotation correction
			clenchedRotationThumbTM_corrected = Quaternion.Euler(clenchedRotationThumbTM.eulerAngles.x * invert, clenchedRotationThumbTM.eulerAngles.y, clenchedRotationThumbTM.eulerAngles.z);
			for(int a = 0; a < 5; a++) { // Fingers
				if(!closeHand && !(a == 4 && trackThumbs)) {
					fingerTransforms[i, a, 0].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 0].localRotation, initialFingerRotations[i, a, 0], Time.deltaTime * rotationSpeed);
					fingerTransforms[i, a, 1].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 1].localRotation, initialFingerRotations[i, a, 1], Time.deltaTime * rotationSpeed);
					fingerTransforms[i, a, 2].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 2].localRotation, initialFingerRotations[i, a, 2], Time.deltaTime * rotationSpeed);
					}
				else {
					if(a != 4) {
						fingerTransforms[i, a, 0].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 0].localRotation, clenchedRotationMCP, Time.deltaTime * rotationSpeed);
						fingerTransforms[i, a, 1].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 1].localRotation, clenchedRotationPIP, Time.deltaTime * rotationSpeed);
						fingerTransforms[i, a, 2].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 2].localRotation, clenchedRotationDIP, Time.deltaTime * rotationSpeed);
					}
					else if(!trackThumbs) { // Thumbs (if separate thumb  tracking is not enabled)
						fingerTransforms[i, a, 0].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 0].localRotation, clenchedRotationThumbTM_corrected, Time.deltaTime * rotationSpeed);
						fingerTransforms[i, a, 1].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 1].localRotation, clenchedRotationThumbMCP, Time.deltaTime * rotationSpeed);
						fingerTransforms[i, a, 2].localRotation =  Quaternion.Lerp(fingerTransforms[i, a, 2].localRotation, clenchedRotationThumbIP, Time.deltaTime * rotationSpeed);
					}	
				}	
			}
		}
	}

	private void saveInitialFingerRotations() {
		
		Transform handObject;
		
		for (int i = 0; i < 2; i++) { 
			if (i == 0) handObject = rightHand;
			else handObject = leftHand;
			
			Transform[] fingers = handObject.GetComponentsInChildren<Transform> ();
			
			int fingerIndex = 0;
			int index = 0;
			foreach (Transform finger in fingers) {
			
				if (finger.parent.transform.gameObject == handObject.transform.gameObject
				    && (finger.gameObject.name.Contains("finger") || finger.gameObject.name.Contains("Finger"))) {
				
					if(fingerIndex > 4) break; // No mutants allowed!
					
					if(finger == rightThumb || finger == leftThumb) index = 4; // thumb
					else {
						index = fingerIndex;
						fingerIndex++;
					}
				
					// First bone
					initialFingerRotations[i, index, 0] = finger.localRotation;
					fingerTransforms[i, index, 0] = finger;
					Transform[] nextFingerParts = finger.gameObject.GetComponentsInChildren<Transform> ();
					foreach (Transform part1 in nextFingerParts) {
						if (part1.parent.transform.gameObject == finger.gameObject
						    && (part1.gameObject.name.Contains("finger") || part1.gameObject.name.Contains("Finger"))) {
							// Second bone
							initialFingerRotations[i, index, 1] = part1.localRotation;
							fingerTransforms[i, index, 1] = part1;
							Transform[] nextFingerParts2 = finger.gameObject.GetComponentsInChildren<Transform> ();
							foreach (Transform part2 in nextFingerParts2) {
								if (part2.parent.transform.gameObject == part1.gameObject
								    && (part2.gameObject.name.Contains("finger") || part2.gameObject.name.Contains("Finger"))) {
									// Third bone
									initialFingerRotations[i, index, 2] = part2.localRotation;
									fingerTransforms[i, index, 2] = part2; 
								}
							}
						}
					}
				}
			}	
		}
	}
	
	
	private Quaternion limitZRotation(Quaternion inputRotation, float rollMinimum, float rollMaximum)
	{
		/**
		 * Argument inputRotation's roll angle (rotation around Z axis) is clamped between [rollMinimum, rollMaximum].
		 * Works only if effective rotation around Y axis is zero. Rotation around X axis is allowed though.
		 **/
	 
		float rollAngle = 0;
		Vector3 limitedRoll = Vector3.zero;
		Quaternion outputRotation = inputRotation;
		
		// Calculate the rotation of inputRotation where roll (rotation around Z axis) is omitted
		Quaternion rotationWithoutRoll = Quaternion.LookRotation(inputRotation * Vector3.forward, 
		                                                         Vector3.Cross( inputRotation * Vector3.forward, Vector3.right)); 
		rollAngle = Quaternion.Angle(inputRotation, rotationWithoutRoll);
		
		// Is the roll to the left or to the right? Quaternion.Angle returns only positive values and omits rotation "direction"
		if((inputRotation*Vector3.up).x > 0)
			rollAngle *= -1;
		
		if(rollAngle > rollMaximum) // Rolling towards or over maximum angle
		{
			// Clamp to nearest limit
			if(rollAngle - rollMaximum < 0.5f*(360 - rollMaximum + rollMinimum))
				limitedRoll.z = rollMaximum;
			else
				limitedRoll.z = rollMinimum;
			outputRotation = rotationWithoutRoll * Quaternion.Euler(limitedRoll);
		}
		if(rollAngle < rollMinimum) // Rolling towards or below minimum angle
		{
			// Clamp to nearest limit
			if(rollMinimum - rollAngle < 0.5f*(360 - rollMaximum + rollMinimum))
				limitedRoll.z = rollMinimum;
			else
				limitedRoll.z = rollMaximum;
			outputRotation = rotationWithoutRoll * Quaternion.Euler(limitedRoll);
		}
		
		print (rollAngle + " " + rotationWithoutRoll.eulerAngles);
		
		return outputRotation;
	}
	
}
