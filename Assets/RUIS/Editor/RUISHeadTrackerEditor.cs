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
    SerializedProperty positionInput;
    SerializedProperty rotationInput;
    SerializedProperty positionOffsetKinect;
    SerializedProperty positionOffsetPSMove;
    SerializedProperty positionOffsetHydra;
    SerializedProperty rotationOffsetKinect;
    SerializedProperty rotationOffsetPSMove;
    SerializedProperty rotationOffsetHydra;
    SerializedProperty filterPosition;
    SerializedProperty positionNoiseCovariance;
    SerializedProperty filterRotation;
    SerializedProperty rotationNoiseCovariance;
    SerializedProperty externalDriftCorrection;
    SerializedProperty compass;
    SerializedProperty compassPlayerID;
    SerializedProperty compassJoint;
    SerializedProperty correctOnlyWhenFacingForward;
    SerializedProperty compassPSMoveID;
    SerializedProperty compassTransform;
    SerializedProperty driftCorrectionRate;
    SerializedProperty driftNoiseCovariance;
    SerializedProperty driftingDirectionVisualizer;
    SerializedProperty compassDirectionVisualizer;
    SerializedProperty correctedDirectionVisualizer;
    SerializedProperty driftVisualizerPosition;

    public void OnEnable()
    {
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
	    positionInput = serializedObject.FindProperty("positionInput");
	    rotationInput = serializedObject.FindProperty("rotationInput");
	    positionOffsetKinect = serializedObject.FindProperty("positionOffsetKinect");
	    positionOffsetPSMove = serializedObject.FindProperty("positionOffsetPSMove");
	    positionOffsetHydra = serializedObject.FindProperty("positionOffsetHydra");
	    rotationOffsetKinect = serializedObject.FindProperty("rotationOffsetKinect");
	    rotationOffsetPSMove = serializedObject.FindProperty("rotationOffsetPSMove");
	    rotationOffsetHydra = serializedObject.FindProperty("rotationOffsetHydra");
	    filterPosition = serializedObject.FindProperty("filterPosition");
	    positionNoiseCovariance = serializedObject.FindProperty("positionNoiseCovariance");
	    filterRotation = serializedObject.FindProperty("filterRotation");
	    rotationNoiseCovariance = serializedObject.FindProperty("rotationNoiseCovariance");
	    externalDriftCorrection = serializedObject.FindProperty("externalDriftCorrection");
	    compass = serializedObject.FindProperty("compass");
	    compassPlayerID = serializedObject.FindProperty("compassPlayerID");
	    compassJoint = serializedObject.FindProperty("compassJoint");
	    correctOnlyWhenFacingForward = serializedObject.FindProperty("correctOnlyWhenFacingForward");
	    compassPSMoveID = serializedObject.FindProperty("compassPSMoveID");
	    compassTransform = serializedObject.FindProperty("compassTransform");
	    driftCorrectionRate = serializedObject.FindProperty("driftCorrectionRate");
	    driftNoiseCovariance = serializedObject.FindProperty("driftNoiseCovariance");
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
                EditorGUILayout.PropertyField(positionPlayerID, new GUIContent("Kinect Player Id", "Between 0 and 3"));
                EditorGUILayout.PropertyField(positionJoint, new GUIContent("Joint", "Head is the best joint for tracking head position"));
                EditorGUILayout.PropertyField(positionOffsetKinect, new GUIContent("Position Offset", "Position offset from tracked joint to the center "
																			+ "of eyes. With Kinect zero vector is usually the best choice."));
                break;
            case (int)RUISHeadTracker.HeadPositionSource.PSMove:
                EditorGUILayout.PropertyField(positionPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
                EditorGUILayout.PropertyField(positionOffsetPSMove, new GUIContent("Position Offset", "Position offset from tracked PS Move sphere to "
																			+ "center of eyes in local coordinates of the Move controller. "
																			+ "Set these values according to where and in which orientation "
																			+ "the Move is attached to your head."));
                break;
            case (int)RUISHeadTracker.HeadPositionSource.RazerHydra:
				EditorGUILayout.LabelField("Razer Hydra support coming soon");
				break;
            case (int)RUISHeadTracker.HeadPositionSource.InputTransform:
                EditorGUILayout.PropertyField(positionInput, new GUIContent("Input Transform", "All other position trackers are supported "
																			+ "through this transform. Drag and drop here a transform "
																			+ "whose position is controlled by a tracking device."));
				break;
        }
		
		if(headPositionInput.enumValueIndex != (int)RUISHeadTracker.HeadPositionSource.None)
		{
	        EditorGUILayout.PropertyField(filterPosition, new GUIContent("Filter Position", "Enables simple Kalman filtering for position "
																					+ "tracking. Only recommended for Kinect."));
	        EditorGUILayout.PropertyField(positionNoiseCovariance, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																					+ "a bigger value means smoother results but a slower "
																					+ "response to changes."));
		}
				
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
	                EditorGUILayout.PropertyField(rotationPlayerID, new GUIContent("Kinect Player ID", "Between 0 and 3"));
	                EditorGUILayout.PropertyField(rotationJoint, new GUIContent("Joint", "ATTENTION: Torso has most stable joint rotation " +
																				"for head tracking! Currently OpenNI's head joint rotation " +
																				"is always the same as torso rotation, except its tracking " +
																				"fails more often."));
	                EditorGUILayout.PropertyField(rotationOffsetKinect, new GUIContent("Rotation Offset", "Rotation offset between tracked "
																				+ "joint's rotation and head's look rotation. With Kinect "
																				+ "zero vector is usually the best choice."));
	                break;
	            case (int)RUISHeadTracker.HeadRotationSource.PSMove:
	                EditorGUILayout.PropertyField(rotationPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
	                EditorGUILayout.PropertyField(rotationOffsetPSMove, new GUIContent("Rotation Offset", "Rotation offset between tracked "
																				+ "PS Move controller's rotation and head's look rotation. "
																				+ "Set these values according to the orientation "
																				+ "in which Move is attached to your head."));
	                break;
	            case (int)RUISHeadTracker.HeadRotationSource.RazerHydra:
					EditorGUILayout.LabelField("Razer Hydra support coming soon");
					break;
	            case (int)RUISHeadTracker.HeadRotationSource.InputTransform:
	                EditorGUILayout.PropertyField(rotationInput, new GUIContent("Input Transform", "All other rotation trackers are supported "
																				+ "through this transform. Drag and drop here a transform "
																				+ "whose rotation is controlled by a tracking device."));
					break;
	        }
			
			if(headRotationInput.enumValueIndex != (int)RUISHeadTracker.HeadRotationSource.None)
			{
		        EditorGUILayout.PropertyField(filterRotation, new GUIContent("Filter Rotation", "Enables simple Kalman filtering for rotation "
																						+ "tracking. Only recommended for Kinect."));
		        EditorGUILayout.PropertyField(rotationNoiseCovariance, new GUIContent("Filter Strength", "Noise covariance of Kalman filtering: " 
																						+ "a bigger value means smoother results but a slower "
																						+ "response to changes."));
			}
		}
		
        EditorGUI.indentLevel -= 2;
				
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(externalDriftCorrection, new GUIContent("Yaw Drift Correction", "Enables external yaw drift correction "
																				+ "using Kinect, PS Move, or some other device"));
		
		if(externalDriftCorrection.boolValue)
		{
	        EditorGUI.indentLevel += 2;
			
	        EditorGUILayout.PropertyField(driftCorrectionRate, new GUIContent("Correction Rate", "Positive values only. How fast the "
																				+ "drifting rotation is shifted towards the compass' rotation. "
																				+ "Default of 0.1 is good, but if you use Kinect as a compass, "
																				+ "you might want to adjust this to suit your liking."));
			
	        EditorGUILayout.PropertyField(compass, new GUIContent("Compass Tracker", "Tracker that will be used to correct the yaw drift of "
																	+ "Rotation Tracker"));
			
	        switch (compass.enumValueIndex)
	        {
	            case (int)RUISHeadTracker.CompassSource.Kinect:
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
	                break;
	            case (int)RUISHeadTracker.CompassSource.PSMove:
	                EditorGUILayout.PropertyField(compassPSMoveID, new GUIContent("PS Move ID", "Between 0 and 3"));
	                break;
	            case (int)RUISHeadTracker.CompassSource.InputTransform:
	                EditorGUILayout.PropertyField(compassTransform, new GUIContent("Input Transform", "Drift correction via all other trackers "
																				+ "is supported through this transform. Drag and drop here a "
																				+ "transform whose rotation cannot drift."));
					break;
	        }
			
       		EditorGUILayout.Space();
			EditorGUILayout.LabelField("Optional visualizers:");
	        EditorGUILayout.PropertyField(driftingDirectionVisualizer, new GUIContent("Drifter Rotation Visualizer", "Drag and drop a Game "
																						+ "Object here to visualize rotation from Rotation Tracker"));
	        EditorGUILayout.PropertyField(compassDirectionVisualizer, new GUIContent("Compass Yaw Visualizer", "Drag and drop a Game Object "
																						+ "here to visualize yaw rotation from Compass Tracker"));
	        EditorGUILayout.PropertyField(correctedDirectionVisualizer, new GUIContent("Corrected Rotation Visualizer", "Drag and drop a Game "
																						+ "Object here to visualize the final, corrected rotation"));
	        EditorGUILayout.PropertyField(driftVisualizerPosition, new GUIContent("Visualizer Position", "Drag and drop a Transform here "
																						+ "that defines the position where the above three "
																						+ "visualizers will appear"));
			
        	EditorGUI.indentLevel -= 2;
		}
		
		
		
		
        serializedObject.ApplyModifiedProperties();
    }
}
