using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RUISFistGestureRecognizer : RUISGestureRecognizer {
	
	public float fistClosedDuration = 0.3f; // In seconds
	public float fistOpenDuration = 0.3f; // In seconds
	public int fistClosedSignalLimit = 3;
	public static int fistOpenSignalLimit = 3;
	private float foundValidSignals, validTimeWindow;
	
	// Stores timestamps when open/closed signals arrived 
	private float[] fistClosedSignalTimestampBuffer; // Store closed signals
	private float[] fistOpenSignalTimestampBuffer;// Store open signals
	private int closedBufferIndex = 0;
	private int openBufferIndex = 0;
	
	public float lostFistReleaseDuration = 3; // In seconds
	public float lastClosedSignalTimestamp = 0;
	
	RUISPointTracker pointTracker;
	RUISSkeletonWand skeletonWand;
	RUISSkeletonManager.Skeleton.handState fistStatusInSensor;
	bool gestureEnabled;
	float gestureProgress;
	
	float fistClosedTime, fistOpenTime;
	RUISSkeletonManager.Skeleton.handState leftFistStatusInSensor, rightFistStatusInSensor;
	bool handClosed;
	
	bool closedTriggerHandled, openedTriggerHandled;
	RUISSkeletonManager ruisSkeletonManager;
	
	public fistSide leftOrRightFist;
	public enum fistSide {
		InferFromName,
		RightFist,
		LeftFist
	}
	
	void Awake()
	{
		fistClosedSignalTimestampBuffer = new float[fistClosedSignalLimit];
		fistOpenSignalTimestampBuffer = new float[fistOpenSignalLimit]; 
		ruisSkeletonManager = FindObjectOfType(typeof(RUISSkeletonManager)) as RUISSkeletonManager;
		pointTracker = GetComponent<RUISPointTracker>();
		skeletonWand = GetComponent<RUISSkeletonWand>();
		handClosed = false;
		
		if(leftOrRightFist == fistSide.InferFromName) {
			if(skeletonWand.wandStart.ToString().IndexOf("Right") != -1) leftOrRightFist = fistSide.RightFist;
			if(skeletonWand.wandStart.ToString().IndexOf("Left") != -1) leftOrRightFist = fistSide.LeftFist;
		}
		closedTriggerHandled = true; 
		openedTriggerHandled = true;
	}
	
	void Start()
	{
		ResetData();
	}
	
	void LateUpdate()
	{
		rightFistStatusInSensor = skeletonWand.skeletonManager.skeletons[skeletonWand.bodyTrackingDeviceID, skeletonWand.playerId].rightHandStatus;
		leftFistStatusInSensor = skeletonWand.skeletonManager.skeletons[skeletonWand.bodyTrackingDeviceID, skeletonWand.playerId].leftHandStatus;
		
		if(leftOrRightFist == fistSide.LeftFist) fistStatusInSensor = leftFistStatusInSensor;
		else fistStatusInSensor = rightFistStatusInSensor;
		
		if(!ruisSkeletonManager.isNewKinect2Frame) return;
		
		if(handClosed)
		{
			// If received closed signal, reset buffer
			if(fistStatusInSensor == RUISSkeletonManager.Skeleton.handState.closed) {
				fistOpenSignalTimestampBuffer = new float[fistOpenSignalLimit];
				lastClosedSignalTimestamp = Time.time;
				}
			// If last signal was open, check if array is full of recent enough signals
			else if(fistStatusInSensor == RUISSkeletonManager.Skeleton.handState.open) {
				fistOpenSignalTimestampBuffer[openBufferIndex] = Time.time;
				openBufferIndex = (openBufferIndex + 1) % fistOpenSignalLimit;
				foundValidSignals = 0;
				validTimeWindow = Time.time - fistOpenDuration;
				for(int i = 0; i < fistOpenSignalTimestampBuffer.Length; i++) {
					if(fistOpenSignalTimestampBuffer[i] > validTimeWindow) foundValidSignals++;
				}
				// Trigger opening of hand
				if(foundValidSignals >= fistOpenSignalLimit && handClosed && closedTriggerHandled) 
				{ 
					fistOpenSignalTimestampBuffer = new float[fistOpenSignalLimit];
					fistClosedSignalTimestampBuffer = new float[fistClosedSignalLimit];
					handClosed = false; closedTriggerHandled = false;  
				}
			}	
		}
		else 
		{	
			// If received open signal, reset buffer
			if(fistStatusInSensor == RUISSkeletonManager.Skeleton.handState.open) fistClosedSignalTimestampBuffer = new float[fistClosedSignalLimit];
			// If last signal was open, check if array is full of recent enough signals
			else if(fistStatusInSensor == RUISSkeletonManager.Skeleton.handState.closed) {
				fistClosedSignalTimestampBuffer[closedBufferIndex] = Time.time;
				closedBufferIndex = (closedBufferIndex + 1) % fistClosedSignalLimit;
				foundValidSignals = 0;
				validTimeWindow = Time.time - fistClosedDuration;
				for(int i = 0; i < fistClosedSignalTimestampBuffer.Length; i++) {
					if(fistClosedSignalTimestampBuffer[i] > validTimeWindow) foundValidSignals++;
				}
				// Trigger opening of hand
				if(foundValidSignals >= fistClosedSignalLimit && !handClosed && openedTriggerHandled) { 
					fistOpenSignalTimestampBuffer = new float[fistOpenSignalLimit];
					fistClosedSignalTimestampBuffer = new float[fistClosedSignalLimit];
					handClosed = true; openedTriggerHandled = false; 
					lastClosedSignalTimestamp = Time.time;
				}
			}	
		}
		// If no close signal detected for certaint amount of time, assume open hand.
		
		if(Time.time - lastClosedSignalTimestamp > fistOpenSignalLimit && handClosed && closedTriggerHandled) {
			lastClosedSignalTimestamp = Time.time;
			fistOpenSignalTimestampBuffer = new float[fistOpenSignalLimit];
			fistClosedSignalTimestampBuffer = new float[fistClosedSignalLimit];
			handClosed = false; closedTriggerHandled = false;
		}
		
		// Debug
		//if(handClosed) print ("Gesture enabled");
		//else print ("Gesture disabled");
		
		
	}
	
	public override bool GestureTriggered()
	{
		if((!closedTriggerHandled || !openedTriggerHandled) 
				&& (gestureEnabled && handClosed) 
				&& (!closedTriggerHandled && !openedTriggerHandled)) {
			closedTriggerHandled = true;
			openedTriggerHandled = true;
			return gestureEnabled && handClosed;
		}
		else if(!closedTriggerHandled || !openedTriggerHandled 
				&& (!closedTriggerHandled && !openedTriggerHandled)) {
			closedTriggerHandled = true;
			openedTriggerHandled = true;
			return true;
		}
		else {
			closedTriggerHandled = true;
			openedTriggerHandled = true;
			return false;
		}
	}
	
	public override float GetGestureProgress()
	{
		if(fistClosedTime != 0 && !handClosed) {
			return Mathf.Clamp01(fistClosedTime / fistClosedDuration);
		}
		else if(handClosed) { 
			return Mathf.Clamp01(fistOpenTime / fistOpenDuration);
		}
		else return 0.0f;
	}
	
	public override void ResetProgress()
	{
		gestureProgress = 0;
	}
	
	private void StartTiming()	
	{
		ResetData();
		gestureEnabled = true;
	}
	
	private void ResetData()
	{
		gestureProgress = 0;
		fistClosedTime = 0;
		fistOpenTime = 0;
	}
	
	public override void EnableGesture()
	{
		gestureEnabled = true;
		ResetData();
	}
	
	public override void DisableGesture()
	{	
		gestureEnabled = false;
		ResetData();
	}

}
