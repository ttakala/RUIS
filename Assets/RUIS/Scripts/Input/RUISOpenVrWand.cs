/*****************************************************************************

Content    :   A class to manage the information from a OpenVR controller
Authors    :   Tuukka Takala
Copyright  :   Copyright 2018 Tuukka Takala. All Rights reserved.
Licensing  :   LGPL Version 3 license for non-commercial projects. Use
               restricted for commercial projects. Contact tmtakala@gmail.com
               for more information.

******************************************************************************/

using UnityEngine;
using System.Collections;


public class RUISOpenVrWand : RUISWand 
{
	public enum SelectionButton
	{
		Trigger,
		Menu,
		Pad,
		Grip,
		None
	}

	[Header("Controller ID can forced in runtime from below 'Steam VR_Tracked Object'")]
	[Tooltip("OpenVR controller button for selecting GameObjects that have the RUISSelectable component.")]
	public SelectionButton selectionButton;

	private SteamVR_TrackedObject steamVRTrackedObject;

	private Vector3 positionUpdate;
	private Vector3 rotationUpdate;

	protected RUISCoordinateSystem coordinateSystem;

	[Tooltip("Color of the selection ray.")]
	public Color wandColor = Color.white;
	public override Color color { get { return wandColor; } }

	[Tooltip("Opacity of the selection ray.")]
	[Range(0f, 1f)]
	public float rayColorAlpha = 1;

	public void Awake()
	{

		steamVRTrackedObject = GetComponent<SteamVR_TrackedObject>();
		if (steamVRTrackedObject == null)
		{
			Debug.LogError("Could not find " + typeof(SteamVR_TrackedObject) + " in gameObject " + name + "! It is required.");
		}

		if(coordinateSystem == null)
		{
			coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;
			if(!coordinateSystem)
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
		if(	   steamVRTrackedObject && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None 
			&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected						)
			SteamVR_Controller.Input((int)steamVRTrackedObject.index).TriggerHapticPulse((ushort)(duration));
	}


	public override bool SelectionButtonWasPressed()
	{
		switch (selectionButton)
		{
			case SelectionButton.Trigger:
				return TriggerButtonWasPressed;
			case SelectionButton.Menu:
				return MenuButtonWasPressed;
			case SelectionButton.Pad:
				return PadButtonWasPressed;
			case SelectionButton.Grip:
				return GripButtonWasPressed;
			default:
				return false;
		}
	}

	public override bool SelectionButtonWasReleased()
	{
		switch(selectionButton)
		{
			case SelectionButton.Trigger:
				return TriggerButtonWasReleased;
			case SelectionButton.Menu:
				return MenuButtonWasReleased;
			case SelectionButton.Pad:
				return PadButtonWasReleased;
			case SelectionButton.Grip:
				return GripButtonWasReleased;
			default:
				return false;
		}
	}

	public override bool SelectionButtonIsDown()
	{
		switch(selectionButton)
		{
			case SelectionButton.Trigger:
				return TriggerButtonDown;
			case SelectionButton.Menu:
				return MenuButtonDown;
			case SelectionButton.Pad:
				return PadButtonDown;
			case SelectionButton.Grip:
				return GripButtonDown;
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
	public Vector3 Velocity
	{
		get
		{
			if(   steamVRTrackedObject && steamVRTrackedObject.isValid && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None
			   && SteamVR_Controller.Input((int)steamVRTrackedObject.index).valid
			   && SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected													)
				return coordinateSystem.ConvertVelocity(SteamVR_Controller.Input((int)steamVRTrackedObject.index).velocity, RUISDevice.OpenVR);
			return Vector3.zero;
		}
	}

	/// <summary>
	/// Returns angularVelocity in wand's local coordinate system, unaffected my parent's scale
	/// </summary>
	public Vector3 AngularVelocity
	{
		get
		{	
			if(   steamVRTrackedObject && steamVRTrackedObject.isValid && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None
				&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).valid
				&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected														)
				return coordinateSystem.ConvertVelocity(SteamVR_Controller.Input((int)steamVRTrackedObject.index).angularVelocity, RUISDevice.OpenVR);
			return Vector3.zero;
		}
	}

	private bool GetPress(Valve.VR.EVRButtonId buttonId)
	{
		if(	   steamVRTrackedObject && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None 
			&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected						)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPress(buttonId);
		
		return false;
	}

	private bool GetWasPressed(Valve.VR.EVRButtonId buttonId)
	{
		if(	   steamVRTrackedObject && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None 
			&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected						)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPressDown(buttonId);

		return false;
	}

	private bool GetWasReleased(Valve.VR.EVRButtonId buttonId)
	{
		if(	   steamVRTrackedObject && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None 
			&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected						)
			return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetPressUp(buttonId);

		return false;
	}

	public bool GripButtonDown 			{ get { return GetPress(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool MenuButtonDown 			{ get { return GetPress(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool PadButtonDown 			{ get { return GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool TriggerButtonDown 		{ get { return GetPress(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public bool GripButtonWasPressed 	{ get { return GetWasPressed(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool MenuButtonWasPressed 	{ get { return GetWasPressed(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool PadButtonWasPressed 	{ get { return GetWasPressed(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool TriggerButtonWasPressed { get { return GetWasPressed(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public bool GripButtonWasReleased 	{ get { return GetWasReleased(Valve.VR.EVRButtonId.k_EButton_Grip); } }
	public bool MenuButtonWasReleased 	{ get { return GetWasReleased(Valve.VR.EVRButtonId.k_EButton_ApplicationMenu); } }
	public bool PadButtonWasReleased 	{ get { return GetWasReleased(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad); } }
	public bool TriggerButtonWasReleased{ get { return GetWasReleased(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger); } }

	public float TriggerValue 
	{ 
		get 
		{ 
			if(	   steamVRTrackedObject && steamVRTrackedObject.index != SteamVR_TrackedObject.EIndex.None 
				&& SteamVR_Controller.Input((int)steamVRTrackedObject.index).connected						)
				return SteamVR_Controller.Input((int)steamVRTrackedObject.index).GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis1).x;

			return 0;
		}
	}

	// Returns angularVelocity in wand's local coordinate system
	public override Vector3 GetAngularVelocity()
	{
		return AngularVelocity;
	}
}
