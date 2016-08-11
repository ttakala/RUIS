/*****************************************************************************

Content    :   A class to manage the information from a Vive controller
Authors    :   Tuukka Takala
Copyright  :   Copyright 2016 Tuukka Takala. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;


public class RUISViveWand : RUISWand {
	public enum SelectionButton
	{
		Trigger,
		Menu,
		Pad,
		Grip,
		None
	}

	[Header("Controller ID can forced in runtime from below 'Steam VR_Tracked Object'")]
	public SelectionButton selectionButton;

	private SteamVR_TrackedObject steamVRTrackedObject;

	private Vector3 positionUpdate;
	private Vector3 rotationUpdate;

	protected RUISCoordinateSystem coordinateSystem;

	public Color wandColor = Color.white;
	public override Color color { get { return wandColor; } }

	[Range(0f, 1f)]
	public float rayColorAlpha = 1;

	public void Awake ()
	{

		steamVRTrackedObject = GetComponent<SteamVR_TrackedObject>();
		if (steamVRTrackedObject == null)
		{
			Debug.LogError("Could not find " + typeof(SteamVR_TrackedObject) + " in gameObject " + name + "! It is required.");
		}

		if (coordinateSystem == null)
		{
			coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
			if (!coordinateSystem)
			{
				Debug.LogError("Could not find " + typeof(RUISCoordinateSystem) + " script! It should be located in RUIS->InputManager.");
			}
		}
	}

	/// <summary>
	/// Turn on rumble motor for a duration (seconds) of time
	/// </summary>
	public void Rumble(float duration)
	{
		if(SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
			SteamVR_Controller.Input((int)steamVRTrackedObject.index).TriggerHapticPulse((ushort)(duration * 1000000f));
	}


	public override bool SelectionButtonWasPressed()
	{
		switch (selectionButton)
		{
		case SelectionButton.Trigger:
			return triggerButtonWasPressed;
		case SelectionButton.Menu:
			return menuButtonWasPressed;
		case SelectionButton.Pad:
			return padButtonWasPressed;
		case SelectionButton.Grip:
			return gripButtonWasPressed;
		default:
			return false;
		}
	}

	public override bool SelectionButtonWasReleased()
	{
		switch (selectionButton)
		{
		case SelectionButton.Trigger:
			return triggerButtonWasReleased;
		case SelectionButton.Menu:
			return menuButtonWasReleased;
		case SelectionButton.Pad:
			return padButtonWasReleased;
		case SelectionButton.Grip:
			return gripButtonWasReleased;
		default:
			return false;
		}
	}

	public override bool SelectionButtonIsDown()
	{
		switch (selectionButton)
		{
			case SelectionButton.Trigger:
				return triggerButtonDown;
			case SelectionButton.Menu:
				return menuButtonDown;
			case SelectionButton.Pad:
				return padButtonDown;
			case SelectionButton.Grip:
				return gripButtonDown;
			default:
				return false;
		}
	}

	public override bool IsSelectionButtonStandard()
	{
		return true;
	}
		
	/// <summary>
	/// Returns velocity in wand's local coordinate system, unaffected my parent's scale
	/// </summary>
	public Vector3 velocity
	{
		get
		{
			return coordinateSystem.ConvertVelocity(SteamVR_Controller.Input((int)steamVRTrackedObject.index).velocity, RUISDevice.Vive);
		}
	}

	/// <summary>
	/// Returns angularVelocity in wand's local coordinate system, unaffected my parent's scale
	/// </summary>
	public Vector3 angularVelocity
	{
		get
		{	
			if(   steamVRTrackedObject.isValid
				&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).valid
				&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
				return coordinateSystem.ConvertVelocity(SteamVR_Controller.Input((int)steamVRTrackedObject.index).angularVelocity, RUISDevice.Vive);
			return Vector3.zero;
		}
	}

	private bool getPress(Valve.VR.EVRButtonId buttonId)
	{
		if(SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPress(buttonId);
		
		return false;
	}

	private bool getWasPressed(Valve.VR.EVRButtonId buttonId)
	{
		if(SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPressDown(buttonId);

		return false;
	}

	private bool getWasReleased(Valve.VR.EVRButtonId buttonId)
	{
		if(SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPressUp(buttonId);

		return false;
	}

	public bool gripButtonDown { get { return getPress(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool menuButtonDown { get { return getPress(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool padButtonDown { get { return getPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool triggerButtonDown { get { return getPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public bool gripButtonWasPressed { get { return getWasPressed(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool menuButtonWasPressed { get { return getWasPressed(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool padButtonWasPressed { get { return getWasPressed(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool triggerButtonWasPressed { get { return getWasPressed(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public bool gripButtonWasReleased { get { return getWasReleased(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool menuButtonWasReleased { get { return getWasReleased(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool padButtonWasReleased { get { return getWasReleased(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool triggerButtonWasReleased { get { return getWasReleased(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public float triggerValue 
	{ 
		get 
		{ 
			if(SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected)
				return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x;

			return 0;
		}
	}

	// Returns angularVelocity in wand's local coordinate system
	public override Vector3 GetAngularVelocity()
	{
		return angularVelocity;
	}
}
