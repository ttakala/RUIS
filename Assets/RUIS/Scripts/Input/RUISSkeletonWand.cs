/*****************************************************************************

Content    :   A basic wand to use with a Kinect-tracked skeleton
Authors    :   Mikael Matveinen
Copyright  :   Copyright 2013 Tuukka Takala, Mikael Matveinen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RUIS/Input/RUISSkeletonWand")]
public class RUISSkeletonWand : RUISWand
{

	public int playerId = 0;
	public RUISSkeletonController.bodyTrackingDeviceType bodyTrackingDevice = 0;
	public int bodyTrackingDeviceID;
	public int gestureSelectionMethod;
	public string gestureSelectionScriptName;
    public RUISSkeletonManager.Joint wandStart = RUISSkeletonManager.Joint.RightElbow;
    public RUISSkeletonManager.Joint wandEnd = RUISSkeletonManager.Joint.RightHand;
    private KalmanFilteredRotation rotationFilter;
    public float rotationNoiseCovariance = 500;
	public float visualizerThreshold = 0.25f;
	public int visualizerWidth = 32;
	public int visualizerHeight = 32;
	public Color wandColor = Color.white;
	private RUISGestureRecognizer gestureRecognizer;
	public GameObject wandPositionVisualizer;
	public bool showVisualizer = true;
	
	private Texture2D[] selectionVisualizers;
	public RUISSkeletonManager skeletonManager;
    private RUISDisplayManager displayManager;
    private const int amountOfSelectionVisualizerImages = 8;
   
    private RUISWandSelector wandSelector;
    private bool isTracking = false;
    private RUISSelectable highlightStartObject;
	
	
	
	// Tuukka:
	private Quaternion tempRotation;
	private Quaternion filteredRotation;

    public void Awake()
    {
		rotationFilter = new KalmanFilteredRotation();
		rotationFilter.skipIdenticalMeasurements = true;
		rotationFilter.rotationNoiseCovariance = rotationNoiseCovariance;
		
		bodyTrackingDeviceID = (int)bodyTrackingDevice;
		RUISGestureRecognizer[] gestureRecognizerScripts = GetComponents<RUISGestureRecognizer>();
		
		foreach(RUISGestureRecognizer script in gestureRecognizerScripts) {
			if(script.ToString() != gestureSelectionScriptName) script.enabled = false;
			else gestureRecognizer = script;
		}
		
        if (!skeletonManager)
        {
            skeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
        }

        selectionVisualizers = new Texture2D[8];
        for (int i = 0; i < amountOfSelectionVisualizerImages; i++)
        {
            selectionVisualizers[i] = Resources.Load("RUIS/Graphics/Selection/visualizer" + (i + 1)) as Texture2D;
        }

        displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;

        if (!gestureRecognizer)
        {
            Debug.LogWarning("Please set a gesture recognizer for wand: " + name + " if you want to use gestures.");
        }

        wandSelector = GetComponent<RUISWandSelector>();

        PlayerLost();
    }

    public void Update()
    {
        if (!isTracking && skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking)
        {
            PlayerFound();
        }
        else if (isTracking && !skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking)
        {
            PlayerLost();
        }
        else if (!skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking)
        {
            return;
        }

        if (!highlightStartObject && wandSelector.HighlightedObject)
        {
            highlightStartObject = wandSelector.HighlightedObject;
            gestureRecognizer.EnableGesture();
        }
        else if (!wandSelector.HighlightedObject)
        {
            highlightStartObject = null;

            if (!wandSelector.Selection)
            {
                gestureRecognizer.DisableGesture();
            }
        }

        visualizerThreshold = Mathf.Clamp01(visualizerThreshold);

		RUISSkeletonManager.JointData startData = skeletonManager.GetJointData(wandStart, playerId, bodyTrackingDeviceID);
		RUISSkeletonManager.JointData endData = skeletonManager.GetJointData(wandEnd, playerId, bodyTrackingDeviceID);

        if (endData.positionConfidence >= 0.5f)
        {
			
			// TUUKKA: Original code
//            transform.localPosition = endData.position;
//
//            if (startData != null && startData.positionConfidence >= 0.5f)
//            {
//                transform.localRotation = Quaternion.LookRotation(endData.position - startData.position);
//            }
//            else if (endData.rotationConfidence >= 0.5f)
//            {
//                transform.localRotation = endData.rotation;
//            }
			
			// First calculate local rotation
            if (startData != null && startData.positionConfidence >= 0.5f)
            {
				tempRotation = Quaternion.LookRotation(endData.position - startData.position);
				filteredRotation = rotationFilter.Update(tempRotation, Time.deltaTime);
            }
            else if (endData.rotationConfidence >= 0.5f)
            {
				tempRotation = endData.rotation;
				filteredRotation = rotationFilter.Update(tempRotation, Time.deltaTime);
            }
            
			if (rigidbody)
	        {
				// TUUKKA:
				if (transform.parent)
				{
					// If the wand has a parent, we need to apply its transformation first
	            	rigidbody.MovePosition(transform.parent.TransformPoint(endData.position));
					rigidbody.MoveRotation(transform.parent.rotation * filteredRotation);
				}
				else
				{
	            	rigidbody.MovePosition(endData.position);
					rigidbody.MoveRotation(filteredRotation);
				}
	        }
			else
	        {
				// If there is no rigidBody, then just change localPosition & localRotation
				transform.localPosition = endData.position;
				transform.localRotation = filteredRotation;
	        }
			
        }
    }

    public void OnGUI()
    {
		if(!showVisualizer) return;
        if (!skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking || !gestureRecognizer) return;

        float gestureProgress = gestureRecognizer.GetGestureProgress();

        if (gestureProgress >= visualizerThreshold)
        {
            float visualizerPhase = (gestureProgress - visualizerThreshold) / (1 - visualizerThreshold);
            int selectionVisualizerIndex = (int)(amountOfSelectionVisualizerImages * visualizerPhase);
            selectionVisualizerIndex = Mathf.Clamp(selectionVisualizerIndex, 0, amountOfSelectionVisualizerImages - 1);

            List<RUISDisplayManager.ScreenPoint> screenPoints = displayManager.WorldPointToScreenPoints(transform.position);

            foreach (RUISDisplayManager.ScreenPoint screenPoint in screenPoints)
            {
                RUISGUI.DrawTextureViewportSafe(new Rect(screenPoint.coordinates.x - visualizerWidth / 2, screenPoint.coordinates.y - visualizerHeight / 2, visualizerWidth, visualizerHeight),
                    screenPoint.camera, selectionVisualizers[selectionVisualizerIndex]);
                //GUI.DrawTexture(new Rect(screenPoint.x - visualizerWidth / 2, screenPoint.y - visualizerHeight / 2, visualizerWidth, visualizerHeight), selectionVisualizers[selectionVisualizerIndex]);
            }
        }

    }

    public override bool SelectionButtonWasPressed()
    {
        if (!skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking || !gestureRecognizer) return false;
        
		if(gestureRecognizer.IsBinaryGesture())
		{
			return gestureRecognizer.GestureTriggered();
		}
		else 
		{
           if (gestureRecognizer.GestureTriggered() && wandSelector.HighlightedObject)
	        {
	            gestureRecognizer.ResetProgress();
	            return true;
	        }
	        else 
	        {
	        	return false;
	        }
		}
    }

    public override bool SelectionButtonWasReleased()
    {
        if (!skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking || !gestureRecognizer) 
        {
			if(   !wandSelector.toggleSelection
			   //|| (wandSelector.toggleSelection && wandSelector.selectionButtonReleasedAfterSelection)
			   )
	        	return true;
	        else
				return false;
        }
        
		if(gestureRecognizer.IsBinaryGesture())
		{
			return !gestureRecognizer.GestureTriggered();
		}
		else
		{
	        if (gestureRecognizer.GestureTriggered())
	        {
	            gestureRecognizer.ResetProgress();
	            return true;
	        }
	        else
	        { 
	        	return false;
	        }
        }
    }

    public override bool SelectionButtonIsDown()
    {
        if (!skeletonManager.skeletons[bodyTrackingDeviceID, playerId].isTracking || !gestureRecognizer) return false;
		
		if(gestureRecognizer.IsBinaryGesture())
		{
			return gestureRecognizer.GestureTriggered();
		}
		else 
		{
        	return gestureRecognizer.GestureTriggered();
        }
    }

    public override bool IsSelectionButtonStandard()
    {
        return false;
    }

    public override Vector3 GetAngularVelocity()
    {
        return Vector3.zero;
    }

    public override Color color { get { return wandColor; } }

    private void PlayerFound()
    {
        isTracking = true;
        gestureRecognizer.EnableGesture();
        GetComponent<LineRenderer>().enabled = true;
        if (wandPositionVisualizer)
        {
            wandPositionVisualizer.SetActive(true);
        }
    }

    private void PlayerLost()
    {
        isTracking = false;
        gestureRecognizer.DisableGesture();
        GetComponent<LineRenderer>().enabled = false;
        if (wandPositionVisualizer)
        {
            wandPositionVisualizer.SetActive(false);
        }
    }
}
