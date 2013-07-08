/*****************************************************************************

Content    :   Inspector behaviour for RUISHeadTracker script
Authors    :   Tuukka Takala
Copyright  :   Copyright 2013 Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISHeadTracker))]
[CanEditMultipleObjects]
public class RUISHeadTrackerEditor : Editor
{
	RUISHeadTracker headTrackerScript;
	OVRCameraController oculusCamController;
	bool riftFound = false;
	
	GameObject skeletonManagerGameObject;
	int maxKinectSkeletons = 4;
	int maxPSMoveControllers = 4;
	float minNoiseCovariance = 0.001f;
	float minDriftCorrectionRate = 0.001f;
	float maxDriftCorrectionRate = 1000;
	
    SerializedProperty defaultPosition;
    SerializedProperty skeletonManager;
    SerializedProperty headPositionInput;
    SerializedProperty headRotationInput;
    SerializedProperty riftMagnetometerMode;
    SerializedProperty oculusID;
    SerializedProperty positionPlayerID;
    SerializedProperty rotationPlayerID;
    SerializedProperty positionJoint;
    SerializedProperty rotationJoint;
    SerializedProperty positionPSMoveID;
    SerializedProperty rotationPSMoveID;
	SerializedProperty positionRazerID;
	SerializedProperty rotationRazerID;
    SerializedProperty positionInput;
    SerializedProperty rotationInput;
    SerializedProperty positionOffsetKinect;
    SerializedProperty positionOffsetPSMove;
    SerializedProperty positionOffsetHydra;
    SerializedProperty rotationOffsetKinect;
    SerializedProperty rotationOffsetPSMove;
    SerializedProperty rotationOffsetHydra;
	SerializedProperty filterPositionKinect;
	SerializedProperty filterPositionPSMove;
	SerializedProperty filterPositionHydra;
	SerializedProperty filterPositionTransform;
	SerializedProperty positionNoiseCovarianceKinect;
	SerializedProperty positionNoiseCovariancePSMove;
	SerializedProperty positionNoiseCovarianceHydra;
	SerializedProperty positionNoiseCovarianceTransform;
	SerializedProperty filterRotationKinect;
	SerializedProperty filterRotationPSMove;
	SerializedProperty filterRotationHydra;
	SerializedProperty filterRotationTransform;
	SerializedProperty rotationNoiseCovarianceKinect;
	SerializedProperty rotationNoiseCovariancePSMove;
	SerializedProperty rotationNoiseCovarianceHydra;
	SerializedProperty rotationNoiseCovarianceTransform;
	SerializedProperty isRazerBaseMobile;
	SerializedProperty hydraBasePositionOffsetKinect;
	SerializedProperty hydraBaseRotationOffsetKinect;
    SerializedProperty externalDriftCorrection;
    SerializedProperty compass;
    SerializedProperty compassPlayerID;
    SerializedProperty compassJoint;
    SerializedProperty correctOnlyWhenFacingForward;
    SerializedProperty compassPSMoveID;
	SerializedProperty compassRazerID;
    SerializedProperty compassTransform;
	SerializedProperty driftCorrectionRateKinect;
	SerializedProperty driftCorrectionRatePSMove;
	SerializedProperty driftCorrectionRateHydra;
	SerializedProperty driftCorrectionRateTransform;
    //SerializedProperty driftNoiseCovariance;
	SerializedProperty enableVisualizers;
    SerializedProperty driftingDirectionVisualizer;
    SerializedProperty compassDirectionVisualizer;
    SerializedProperty correctedDirectionVisualizer;
    SerializedProperty driftVisualizerPosition;

    public void OnEnable()
    {
		skeletonManagerGameObject = GameObject.Find("SkeletonManager");
		
		if(skeletonManagerGameObject != null)
		{
			RUISSkeletonManager playerManager = skeletonManagerGameObject.GetComponent<RUISSkeletonManager>();
			if(playerManager != null)
				maxKinectSkeletons = playerManager.skeletonsHardwareLimit;
		}
		
		defaultPosition = serializedObject.FindProperty("defaultPosition");
	    skeletonManager = serializedObject.FindProperty("skeletonManager");
	    headPositionInput = serializedObject.FindProperty("headPositionInput");
	    headRotationInput = serializedObject.FindProperty("headRotationInput");
	    riftMagnetometerMode = serializedObject.FindProperty("riftMagnetometerMode"); //
	    oculusID = serializedObject.FindProperty("oculusID"); //
	    positionPlayerID = serializedObject.FindProperty("positionPlayerID");
	    rotationPlayerID = serializedObject.FindProperty("rotationPlayerID");
	    positionJoint = serializedObject.FindProperty("positionJoint");
	    rotationJoint = serializedObject.FindProperty("rotationJoint");
	    positionPSMoveID = serializedObject.FindProperty("positionPSMoveID");
	    rotationPSMoveID = serializedObject.FindProperty("rotationPSMoveID");
		positionRazerID = serializedObject.FindProperty("positionRazerID");
		rotationRazerID = serializedObject.FindProperty("rotationRazerID");
	    positionInput = serializedObject.FindProperty("positionInput");
	    rotationInput = serializedObject.FindProperty("rotationInput");
	    positionOffsetKinect = serializedObject.FindProperty("positionOffsetKinect");
	    positionOffsetPSMove = serializedObject.FindProperty("positionOffsetPSMove");
	    positionOffsetHydra = serializedObject.FindProperty("positionOffsetHydra");
	    rotationOffsetKinect = serializedObject.FindProperty("rotationOffsetKinect");
	    rotationOffsetPSMove = serializedObject.FindProperty("rotationOffsetPSMove");
	    rotationOffsetHydra = serializedObject.FindProperty("rotationOffsetHydra");
		filterPositionKinect = serializedObject.FindProperty("filterPositionKinect");
		filterPositionPSMove = serializedObject.FindProperty("filterPositionPSMove");
		filterPositionHydra = serializedObject.FindProperty("filterPositionHydra");
		filterPositionTransform = serializedObject.FindProperty("filterPositionTransform");
		positionNoiseCovarianceKinect = serializedObject.FindProperty("positionNoiseCovarianceKinect");
		positionNoiseCovariancePSMove = serializedObject.FindProperty("positionNoiseCovariancePSMove");
		positionNoiseCovarianceHydra = serializedObject.FindProperty("positionNoiseCovarianceHydra");
		positionNoiseCovarianceTransform = serializedObject.FindProperty("positionNoiseCovarianceTransform");
		filterRotationKinect = serializedObject.FindProperty("filterRotationKinect");
		filterRotationPSMove = serializedObject.FindProperty("filterRotationPSMove");
		filterRotationHydra = serializedObject.FindProperty("filterRotationHydra");
		filterRotationTransform = serializedObject.FindProperty("filterRotationTransform");
		rotationNoiseCovarianceKinect = serializedObject.FindProperty("rotationNoiseCovarianceKinect");
		rotationNoiseCovariancePSMove = serializedObject.FindProperty("rotationNoiseCovariancePSMove");
		rotationNoiseCovarianceHydra = serializedObject.FindProperty("rotationNoiseCovarianceHydra");
		rotationNoiseCovarianceTransform = serializedObject.FindProperty("rotationNoiseCovarianceTransform");
		isRazerBaseMobile = serializedObject.FindProperty("isRazerBaseMobile");
		hydraBasePositionOffsetKinect = serializedObject.FindProperty("hydraBasePositionOffsetKinect");
		hydraBaseRotationOffsetKinect = serializedObject.FindProperty("hydraBaseRotationOffsetKinect");
	    externalDriftCorrection = serializedObject.FindProperty("externalDriftCorrection");
	    compass = serializedObject.FindProperty("compass");
	    compassPlayerID = serializedObject.FindProperty("compassPlayerID");
	    compassJoint = serializedObject.FindProperty("compassJoint");
	    correctOnlyWhenFacingForward = serializedObject.FindProperty("correctOnlyWhenFacingForward");
	    compassPSMoveID = serializedObject.FindProperty("compassPSMoveID");
		compassRazerID = serializedObject.FindProperty("compassRazerID");
	    compassTransform = serializedObject.FindProperty("compassTransform");
		driftCorrectionRateKinect 		= serializedObject.FindProperty("driftCorrectionRateKinect");
		driftCorrectionRatePSMove 		= serializedObject.FindProperty("driftCorrectionRatePSMove");
		driftCorrectionRateHydra 		= serializedObject.FindProperty("driftCorrectionRateHydra");
		driftCorrectionRateTransform 	= serializedObject.FindProperty("driftCorrectionRateTransform");
	    //driftNoiseCovariance = serializedObject.FindProperty("driftNoiseCovariance");
		enableVisualizers = serializedObject.FindProperty("enableVisualizers");
	    driftingDirectionVisualizer = serializedObject.FindProperty("driftingDirectionVisualizer");
	    compassDirectionVisualizer = serializedObject.FindProperty("compassDirectionVisualizer");
	    correctedDirectionVisualizer = serializedObject.FindProperty("correctedDirectionVisualizer");
	    driftVisualizerPosition = serializedObject.FindProperty("driftVisualizerPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(defaultPosition, new GUIContent("Default Position", "Head position before tracking starts"));
        EditorGUILayout.PropertyField(skeletonManager, new GUIContent("skeletonManager", "Can be None"));
		
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(headPositionInput, new GUIContent("Position Tracker", "Device that tracks the head position"));
		
        EditorGUI.indentLevel += 2;
        switch (headPositionInput.enumValueIndex)
        {
            case (int)RUISHeadTracker.HeadPositionSource.Kinect:
				positionPlayerID.intValue = Mathf.Clamp(positionPlayerID.intValue, 0, maxKinectSkeletons - 1);
				if(positionNoiseCovarianceKinect.floatValue < minNoiseCovariance)
					positionNoiseCovarianceKinect.floatValue = minNoiseCovariance;
                EditorGUILayout.PropertyField(positionPlayerID, new GUIContent("Kinect Player Id", "Between 0 and 3"));
                EditorGUILayout.PropertyField(positionJoint, new GUIContent("Joint", "Head is the best joint for tracking head position"));
                EditorGUILayout.PropertyField(positionOffsetKinect, new GUIContent("Position Offset", "Position offset from tracked joint to the center "
																			+ "of eyes. With Kinect, zero vector is usually the best choice."));
		        EditorGUILayout.PropertyField(filterPositionKinect, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
																						+ "tracking. Only recommended for Kinect."));
		        EditorGUILayout.PropertyField(positionNoiseCovarianceKinect, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																						+ "a bigger value means smoother results but a slower "
																						+ "response to changes."));
                break;
            case (int)RUISHeadTracker.HeadPositionSource.PSMove:
				positionPSMoveID.intValue = Mathf.Clamp(positionPSMoveID.intValue, 0, maxPSMoveControllers - 1);
				if(positionNoiseCovariancePSMove.floatValue < minNoiseCovariance)
					positionNoiseCovariancePSMove.floatValue = minNoiseCovariance;
                EditorGUILayout.PropertyField(positionPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
                EditorGUILayout.PropertyField(positionOffsetPSMove, new GUIContent("Position Offset", "Position offset from tracked PS Move sphere to "
																			+ "the center of eyes in local coordinates of the Move controller. "
																			+ "Set these values according to where and in which orientation "
																			+ "the Move is attached to your head."));
		        EditorGUILayout.PropertyField(filterPositionPSMove, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
																						+ "tracking. Only recommended for Kinect."));
		        EditorGUILayout.PropertyField(positionNoiseCovariancePSMove, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																						+ "a bigger value means smoother results but a slower "
																						+ "response to changes."));
                break;
            case (int)RUISHeadTracker.HeadPositionSource.RazerHydra:
				if(positionNoiseCovarianceHydra.floatValue < minNoiseCovariance)
					positionNoiseCovarianceHydra.floatValue = minNoiseCovariance;
			    EditorGUILayout.PropertyField(positionRazerID, new GUIContent("Razer Hydra ID", "Either LEFT or RIGHT"));
                EditorGUILayout.PropertyField(positionOffsetHydra, new GUIContent("Position Offset", "Position offset from tracked Razer Hydra "
																			+ "controller to the center of eyes in local coordinates of the Razer "
																			+ "Hydra. Set these values according to where and in which orientation "
																			+ "the Razer Hydra is attached to your head."));
		        EditorGUILayout.PropertyField(isRazerBaseMobile, new GUIContent("Mobile Base", "Enable this if the Razer Hydra base station is "
																			+ "attached to something that is moving (e.g. Kinect tracked player's belt)"));
				
				if(isRazerBaseMobile.boolValue)
				{
        			EditorGUI.indentLevel += 2;
			        EditorGUILayout.PropertyField(hydraBasePositionOffsetKinect, new GUIContent("Base Position Offset", "Position offset from the tracked "
																							+ "Transform to the Razer Hydra base station in local coordinates "
																							+ "of the tracked Transform. Set these values according to where "
																							+ "and in which orientation the Razer Hydra base station is "
																							+ "attached to the tracked Transform."));
			        EditorGUILayout.PropertyField(hydraBaseRotationOffsetKinect, new GUIContent("Base Rotation Offset", "Tracked Razer Hydra base station's "
																				+ "rotation in the tracked Transform's local coordinate system. "
																				+ "Set these euler angles according to the orientation in which "
																				+ "Razer Hydra base station is attached to the tracked Transform."));
        			EditorGUI.indentLevel -= 2;
					
				}
				
		        EditorGUILayout.PropertyField(filterPositionHydra, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
																						+ "tracking. Only recommended for Kinect."));
		        EditorGUILayout.PropertyField(positionNoiseCovarianceHydra, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																						+ "a bigger value means smoother results but a slower "
																						+ "response to changes."));
				break;
            case (int)RUISHeadTracker.HeadPositionSource.InputTransform:
				if(positionNoiseCovarianceTransform.floatValue < minNoiseCovariance)
					positionNoiseCovarianceTransform.floatValue = minNoiseCovariance;
                EditorGUILayout.PropertyField(positionInput, new GUIContent("Input Transform", "All other position trackers are supported "
																			+ "through this transform. Drag and drop here a transform "
																			+ "whose position is controlled by a tracking device."));
		        EditorGUILayout.PropertyField(filterPositionTransform, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
																						+ "tracking. Only recommended for Kinect."));
		        EditorGUILayout.PropertyField(positionNoiseCovarianceTransform, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																						+ "a bigger value means smoother results but a slower "
																						+ "response to changes."));
				break;
        }
		
//		if(headPositionInput.enumValueIndex != (int)RUISHeadTracker.HeadPositionSource.None)
//		{
//	        EditorGUILayout.PropertyField(filterPosition, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
//																					+ "tracking. Only recommended for Kinect."));
//	        EditorGUILayout.PropertyField(positionNoiseCovariance, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
//																					+ "a bigger value means smoother results but a slower "
//																					+ "response to changes."));
//		}
				
        EditorGUI.indentLevel -= 2;
		
		if(serializedObject.targetObject is RUISHeadTracker)
		{
			headTrackerScript = (RUISHeadTracker) serializedObject.targetObject;
			oculusCamController = headTrackerScript.gameObject.GetComponentInChildren(typeof(OVRCameraController)) as OVRCameraController;
			
			if(oculusCamController)
			{
				riftFound = true;
			}
			else
			{
				riftFound = false;
			}
		}
		
        EditorGUILayout.Space();
		
		if(riftFound)
		{
			
			EditorGUILayout.LabelField("Rotation Tracker:    Oculus Rift", EditorStyles.boldLabel);
        	EditorGUI.indentLevel += 2;
			
        	EditorGUILayout.PropertyField(oculusID, new GUIContent("Oculus Rift ID", "Choose which Rift is the source of the head tracking. "
																	+ "Leave this to 0 (multiple Rifts are not supported yet)."));
			
        	EditorGUILayout.PropertyField(riftMagnetometerMode, new GUIContent("Rift Drift Correction", "Choose whether Oculus Rift's "
																	+ "magnetometer is used and how it is calibrated."));
        	EditorGUI.indentLevel += 2;
	        switch (riftMagnetometerMode.enumValueIndex)
	        {
				
	            case (int) RUISHeadTracker.RiftMagnetometer.AutomaticCalibration:
					break;
	            case (int) RUISHeadTracker.RiftMagnetometer.ManualCalibration:
					break;
	            case (int) RUISHeadTracker.RiftMagnetometer.Off:
					break;
			}
        	EditorGUI.indentLevel -= 2;
			
			EditorStyles.textField.wordWrap = true;
			EditorGUILayout.TextArea("OVRCameraController script detected in a child object of this " + headTrackerScript.gameObject.name
										+ ". Assuming that you want to use rotation from Oculus Rift. Disabling other Rotation Tracker "
										+ "options. You can access other rotation trackers when you remove the OVRCameraController "
										+ "component from the child object(s).", GUILayout.Height(110));
		}
		else
		{
        	EditorGUILayout.PropertyField(headRotationInput, new GUIContent("Rotation Tracker", "Device that tracks the head rotation"));
		
        	EditorGUI.indentLevel += 2;
			
	        switch (headRotationInput.enumValueIndex)
	        {
	            case (int)RUISHeadTracker.HeadRotationSource.Kinect:
					rotationPlayerID.intValue = Mathf.Clamp(rotationPlayerID.intValue, 0, maxKinectSkeletons - 1);
					if(rotationNoiseCovarianceKinect.floatValue < minNoiseCovariance)
						rotationNoiseCovarianceKinect.floatValue = minNoiseCovariance;
	                EditorGUILayout.PropertyField(rotationPlayerID, new GUIContent("Kinect Player ID", "Between 0 and 3"));
	                EditorGUILayout.PropertyField(rotationJoint, new GUIContent("Joint", "ATTENTION: Torso has most stable joint rotation " +
																				"for head tracking! Currently OpenNI's head joint rotation " +
																				"is always the same as torso rotation, except its tracking " +
																				"fails more often."));
	                EditorGUILayout.PropertyField(rotationOffsetKinect, new GUIContent("Rotation Offset", "Tracked joint's rotation "
																				+ "in head's local coordinate system. With Kinect "
																				+ "zero vector is usually the best choice."));
			        EditorGUILayout.PropertyField(filterRotationKinect, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
																				+ "tracking. Only recommended for Kinect."));
			        EditorGUILayout.PropertyField(rotationNoiseCovarianceKinect, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																				+ "a bigger value means smoother results but a slower "
																				+ "response to changes."));
	                break;
	            case (int)RUISHeadTracker.HeadRotationSource.PSMove:
					rotationPSMoveID.intValue = Mathf.Clamp(rotationPSMoveID.intValue, 0, maxPSMoveControllers - 1);
					if(rotationNoiseCovariancePSMove.floatValue < minNoiseCovariance)
						rotationNoiseCovariancePSMove.floatValue = minNoiseCovariance;
	                EditorGUILayout.PropertyField(rotationPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
	                EditorGUILayout.PropertyField(rotationOffsetPSMove, new GUIContent("Rotation Offset", "Tracked PS Move controller's "
																				+ "rotation in head's local coordinate system. "
																				+ "Set these euler angles according to the orientation "
																				+ "in which Move is attached to your head."));
			        EditorGUILayout.PropertyField(filterRotationPSMove, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
																				+ "tracking. Only recommended for Kinect."));
			        EditorGUILayout.PropertyField(rotationNoiseCovariancePSMove, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																				+ "a bigger value means smoother results but a slower "
																				+ "response to changes."));
	                break;
	            case (int)RUISHeadTracker.HeadRotationSource.RazerHydra:
					if(rotationNoiseCovarianceHydra.floatValue < minNoiseCovariance)
						rotationNoiseCovarianceHydra.floatValue = minNoiseCovariance;
	                EditorGUILayout.PropertyField(rotationRazerID, new GUIContent("Razer Hydra ID", "Either LEFT or RIGHT"));
	                EditorGUILayout.PropertyField(rotationOffsetHydra, new GUIContent("Rotation Offset", "Tracked Razer Hydra controller's "
																				+ "rotation in head's local coordinate system. "
																				+ "Set these euler angles according to the orientation "
																				+ "in which the Razer Hydra is attached to your head."));
			        EditorGUILayout.PropertyField(filterRotationHydra, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
																				+ "tracking. Only recommended for Kinect."));
			        EditorGUILayout.PropertyField(rotationNoiseCovarianceHydra, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																				+ "a bigger value means smoother results but a slower "
																				+ "response to changes."));
					break;
	            case (int)RUISHeadTracker.HeadRotationSource.InputTransform:
					if(rotationNoiseCovarianceTransform.floatValue < minNoiseCovariance)
						rotationNoiseCovarianceTransform.floatValue = minNoiseCovariance;
	                EditorGUILayout.PropertyField(rotationInput, new GUIContent("Input Transform", "All other rotation trackers are supported "
																				+ "through this transform. Drag and drop here a transform "
																				+ "whose rotation is controlled by a tracking device."));
			        EditorGUILayout.PropertyField(filterRotationTransform, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
																				+ "tracking. Only recommended for Kinect."));
			        EditorGUILayout.PropertyField(rotationNoiseCovarianceTransform, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																				+ "a bigger value means smoother results but a slower "
																				+ "response to changes."));
					break;
	        }
			
//			if(headRotationInput.enumValueIndex != (int)RUISHeadTracker.HeadRotationSource.None)
//			{
//		        EditorGUILayout.PropertyField(filterRotation, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
//																			+ "tracking. Only recommended for Kinect."));
//		        EditorGUILayout.PropertyField(rotationNoiseCovariance, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
//																			+ "a bigger value means smoother results but a slower "
//																			+ "response to changes."));
//			}
		}
		
        EditorGUI.indentLevel -= 2;
				
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(externalDriftCorrection, new GUIContent("Yaw Drift Correction", "Enables external yaw drift correction "
																				+ "using Kinect, PS Move, or some other device"));
		
		if(externalDriftCorrection.boolValue)
		{
	        EditorGUI.indentLevel += 2;
			
	        EditorGUILayout.PropertyField(compass, new GUIContent("Compass Tracker", "Tracker that will be used to correct the yaw drift of "
																	+ "Rotation Tracker"));
			
	        switch (compass.enumValueIndex)
	        {
	            case (int)RUISHeadTracker.CompassSource.Kinect:
					compassPlayerID.intValue = Mathf.Clamp(compassPlayerID.intValue, 0, maxKinectSkeletons - 1);
					driftCorrectionRateKinect.floatValue = Mathf.Clamp(driftCorrectionRateKinect.floatValue, minDriftCorrectionRate, maxDriftCorrectionRate);
	                EditorGUILayout.PropertyField(compassPlayerID, new GUIContent("Kinect Player ID", "Between 0 and 3"));
	                EditorGUILayout.PropertyField(compassJoint, new GUIContent("Joint", "ATTENTION: Torso has most stable joint rotation " +
																				"for drift correction! Currently OpenNI's head joint rotation " +
																				"is always the same as torso rotation, except its tracking " +
																				"fails more often."));
	                EditorGUILayout.PropertyField(correctOnlyWhenFacingForward, new GUIContent("Only Forward Corrections", "Allows drift "
																				+ "correction to occur only when the player is detected as "
																				+ "standing towards Kinect (+-90 degrees). This is useful when "
																				+ "you know that the players will be mostly facing towards Kinect "
																				+ "and you want to improve drift correction by ignoring OpenNI's "
																				+ "tracking errors where the player is detected falsely "
																				+ "standing backwards (happens often)."));
			        EditorGUILayout.PropertyField(driftCorrectionRateKinect, new GUIContent("Correction Rate", "Positive values only. How fast "
																				+ "the drifting rotation is shifted towards the compass' "
																				+ "rotation. Kinect tracked skeleton is quite inaccurate as a " 
																				+ "compass, so 0.01 might be good. You might want to adjust this "
																				+ "to suit your liking."));
	                break;
	            case (int)RUISHeadTracker.CompassSource.PSMove:
					compassPSMoveID.intValue = Mathf.Clamp(compassPSMoveID.intValue, 0, maxPSMoveControllers - 1);
					driftCorrectionRatePSMove.floatValue = Mathf.Clamp(driftCorrectionRatePSMove.floatValue, minDriftCorrectionRate, maxDriftCorrectionRate);
	                EditorGUILayout.PropertyField(compassPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
			        EditorGUILayout.PropertyField(driftCorrectionRatePSMove, new GUIContent("Correction Rate", "Positive values only. How fast "
																				+ "the drifting rotation is shifted towards the compass' "
																				+ "rotation. Default of 0.1 is good."));
	                break;
	            case (int)RUISHeadTracker.CompassSource.RazerHydra:
					driftCorrectionRateHydra.floatValue = Mathf.Clamp(driftCorrectionRateHydra.floatValue, minDriftCorrectionRate, maxDriftCorrectionRate);
	                EditorGUILayout.PropertyField(compassRazerID, new GUIContent("Razer Hydra ID", "Either LEFT or RIGHT"));
			        EditorGUILayout.PropertyField(driftCorrectionRateHydra, new GUIContent("Correction Rate", "Positive values only. How fast "
																				+ "the drifting rotation is shifted towards the compass' "
																				+ "rotation. Default of 0.1 is good."));
	                break;
	            case (int)RUISHeadTracker.CompassSource.InputTransform:
					driftCorrectionRateTransform.floatValue = Mathf.Clamp(driftCorrectionRateTransform.floatValue, minDriftCorrectionRate, maxDriftCorrectionRate);
	                EditorGUILayout.PropertyField(compassTransform, new GUIContent("Input Transform", "Drift correction via all other trackers "
																				+ "is supported through this transform. Drag and drop here a "
																				+ "transform whose rotation cannot drift."));
			        EditorGUILayout.PropertyField(driftCorrectionRateTransform, new GUIContent("Correction Rate", "Positive values only. How fast "
																				+ "the drifting rotation is shifted towards the compass' "
																				+ "rotation."));
					break;
	        }
			
       		EditorGUILayout.Space();
			EditorGUILayout.LabelField("Optional visualizers:");
	        EditorGUILayout.PropertyField(enableVisualizers, new GUIContent("Enable Visualizers", "Below visualizers are optional and meant to"
																						+ "illustrate the performance of the drift correction."));
			if(enableVisualizers.boolValue)
			{
		        EditorGUILayout.PropertyField(driftingDirectionVisualizer, new GUIContent("Drifter Rotation Visualizer", "Drag and drop a Game "
																							+ "Object here to visualize rotation from Rotation Tracker"));
		        EditorGUILayout.PropertyField(compassDirectionVisualizer, new GUIContent("Compass Yaw Visualizer", "Drag and drop a Game Object "
																							+ "here to visualize yaw rotation from Compass Tracker"));
		        EditorGUILayout.PropertyField(correctedDirectionVisualizer, new GUIContent("Corrected Rotation Visualizer", "Drag and drop a Game "
																							+ "Object here to visualize the final, corrected rotation"));
		        EditorGUILayout.PropertyField(driftVisualizerPosition, new GUIContent("Visualizer Position", "Drag and drop a Transform here "
																							+ "that defines the position where the above three "
																							+ "visualizers will appear"));
			}
			
        	EditorGUI.indentLevel -= 2;
		}
		
		
		
		
        serializedObject.ApplyModifiedProperties();
    }
}
