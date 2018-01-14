using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RUISButtonGestureRecognizer : RUISGestureRecognizer {
	
	RUISSkeletonWand skeletonWand;
	bool gestureEnabled = false;

	private bool gestureWasTriggered;

	public KeyCode button = KeyCode.Mouse0;

	public RUISFistGestureRecognizer.fistSide leftOrRightFist;

	public bool animateFist = true;
	public RUISSkeletonController skeletonController;

	// hack for RUISSkeletonController which gets re-instantiated if RUISKinectAndMecanimCombiner is used 
	private bool kinectAndMecanimCombinerExists = false;
	private bool combinerChildrenInstantiated  = false;
	private GameObject skeletonParent;
	
	void Awake()
	{
		skeletonWand = GetComponent<RUISSkeletonWand>();
		gestureWasTriggered = false;
		
		if(leftOrRightFist == RUISFistGestureRecognizer.fistSide.InferFromName) {
			if(skeletonWand.wandStart.ToString().IndexOf("Right") != -1) leftOrRightFist = RUISFistGestureRecognizer.fistSide.RightFist;
			if(skeletonWand.wandStart.ToString().IndexOf("Left") != -1) leftOrRightFist = RUISFistGestureRecognizer.fistSide.LeftFist;
		}

		if(animateFist && skeletonController && this.enabled)
		{
			skeletonController.externalFistTrigger = true;
		}

		// hack for RUISSkeletonController which gets re-instantiated if RUISKinectAndMecanimCombiner is used 
		if(skeletonController && skeletonController.gameObject.transform.parent != null)
		{
			skeletonParent = skeletonController.gameObject.transform.parent.gameObject;
			if(gameObject.transform.parent.GetComponentInChildren<RUISKinectAndMecanimCombiner>())
				kinectAndMecanimCombinerExists = true;
		}

	}

	void LateUpdate()
	{
		if(Input.GetKeyDown(button)) 
			gestureWasTriggered = !gestureWasTriggered;

		if(animateFist && skeletonController)
		{
			if(Input.GetKey(button)) //if(Input.GetKeyDown(button))
			{
				if(leftOrRightFist == RUISFistGestureRecognizer.fistSide.RightFist)
					skeletonController.externalRightStatus = RUISSkeletonManager.Skeleton.handState.closed;
				if(leftOrRightFist == RUISFistGestureRecognizer.fistSide.LeftFist)
					skeletonController.externalLeftStatus = RUISSkeletonManager.Skeleton.handState.closed;
			}
			else //if(Input.GetKeyUp(button))
			{
				if(leftOrRightFist == RUISFistGestureRecognizer.fistSide.RightFist)
					skeletonController.externalRightStatus = RUISSkeletonManager.Skeleton.handState.open;
				if(leftOrRightFist == RUISFistGestureRecognizer.fistSide.LeftFist)
					skeletonController.externalLeftStatus = RUISSkeletonManager.Skeleton.handState.open;
			}
		}

		// Original skeletonController has been destroyed because the GameObject which had
		// it has been split in three parts: Kinect, Mecanim, Blended. Lets fetch the new one.
		if (!combinerChildrenInstantiated && kinectAndMecanimCombinerExists)
		{
			if (skeletonParent)
			{
				RUISKinectAndMecanimCombiner combiner =  skeletonParent.GetComponentInChildren<RUISKinectAndMecanimCombiner>();
				if (combiner && combiner.isChildrenInstantiated())
				{
					skeletonController = combiner.skeletonController;
					
					if(skeletonController == null)
						Debug.LogError(  "Could not find Component " + typeof(RUISSkeletonController) + " from "
						               + "children of " + gameObject.transform.parent.name
						               + ", something is very wrong with this character setup!");

					combinerChildrenInstantiated = true;
				}
			}
		}
	}
	
	public override bool GestureIsTriggered()
	{
		// return handClosed;
		return Input.GetKey(button) && gestureEnabled;
	}
	
	public override bool GestureWasTriggered()
	{
		return gestureWasTriggered;
	}
	
	public override float GetGestureProgress()
	{
		if(Input.GetKey(button))
			return 1;
		else 
			return 0;
	}
	
	public override void ResetProgress()
	{
		// Not used
	}
	
	private void StartTiming()	
	{
		// Not used
	}
	
	private void ResetData()
	{
		// Not used
	}
	
	public override void EnableGesture()
	{
		gestureEnabled = true;
	}
	
	public override void DisableGesture()
	{	
		gestureEnabled = false;
	}

	public override bool IsBinaryGesture()
	{
		return true;
	}

}
