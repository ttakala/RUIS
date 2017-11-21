/*****************************************************************************

Content    :   A manager for display configurations
Authors    :   Heikki Heiskanen, Tuukka Takala
Copyright  :   Copyright 2016 Tuukka Takala, Heikki Heiskanen. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Ovr;


// RESTRICTION: THERE SHOULD BE ONLY ONE RUISMenuNGUI INSTANCE WHICH RUISDisplayManager HANDLES

public class RUISMenuNGUI : MonoBehaviour {

	// Menu states
	public enum RUISMenuStates {
		selectAndConfigureDevices,
		keyStoneConfiguration,
		calibration
	};
	public RUISMenuStates currentMenuState;
	private List<GameObject> buttons = new List<GameObject>();
	private List<GameObject> checkBoxes = new List<GameObject>();
	private List<GameObject> textFields = new List<GameObject>();
	
	private bool keystoneConfigurationLayerActive;
	
	private RUISInputManager inputManager;
	private RUISDisplayManager displayManager;
	private RUISCoordinateSystem coordinateSystem;
	
	public bool 	originalEnablePSMove,
					originalEnableKinect, 
					originalEnableKinect2, 
					originalEnableCustom1,
					originalEnableCustom2,
					originalEnableHydra;
//					originalEnableJumpGesture,
//					originalKinectDriftCorrection; 
	
	public string 	originalPSMoveIP;
	public int		originalPSMovePort;

	private bool ruisMenuButtonDefined = true;
	
	public RUISJumpGestureRecognizer jumpGesture;
	
	public bool menuIsVisible = false;
	
//	private int previousSceneId;
	
//	private bool calibrationInfoMessageShown = false;
	
	// selectAndConfigureDevices phase initial GUI element positions
	Vector3 kinectOrigGUIPos, kinect2OrigGUIPos, hydraOrigGUIPos, infotextOrigGUIPos, custom1OrigGUIPos, custom2OrigGUIPos;
//	Vector3 psMoveOrigGUIPos, buttonsOrigGUIPos;
	
	public string calibrationDropDownSelection;
	
	public bool calibrationReady;

//	Ovr.HmdType ovrHmdVersion; //06to08
	
	void Awake() 
	{
		// BUTTONS
		// Phase: select and configure devices
		buttons.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Button - Calibration").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Button - Display Management").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Button - Save Config & Restart Scene").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Button - Discard Configuration").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Button - Quit Application").gameObject);
		
		// Phase: key stone configuration
		buttons.Add(this.transform.Find ("NGUIControls/Panel/keyStoneConfiguration/Button - Reset Keystoning").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/keyStoneConfiguration/Button - Save Configurations").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/keyStoneConfiguration/Button - Load Old Configurations").gameObject);
		buttons.Add(this.transform.Find ("NGUIControls/Panel/keyStoneConfiguration/Button - End Display Editing").gameObject);
		
		// CHECKBOXES
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").gameObject);
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").gameObject);
//		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject);
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").gameObject);
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").gameObject);
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Custom1/Checkbox - Use Custom 1").gameObject);
		checkBoxes.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Custom2/Checkbox - Use Custom 2").gameObject);
		
		// TEXTFIELDS
		textFields.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject);
		textFields.Add(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject);
		
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
		coordinateSystem = FindObjectOfType(typeof(RUISCoordinateSystem)) as RUISCoordinateSystem;

		kinectOrigGUIPos 	= this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect").transform.localPosition;
		kinect2OrigGUIPos 	= this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect2").transform.localPosition;
		custom1OrigGUIPos   = this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Custom1").transform.localPosition;
		custom2OrigGUIPos   = this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Custom2").transform.localPosition;
		hydraOrigGUIPos 	= this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Hydra").transform.localPosition;
		infotextOrigGUIPos 	= this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Infotexts").transform.localPosition;
//		psMoveOrigGUIPos = this.transform.Find  ("NGUIControls/Panel/selectAndConfigureDevices/PSMove").transform.localPosition;
//		buttonsOrigGUIPos = this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons").transform.localPosition;
	}
		
	void Start () 
	{
		if(this.transform.parent == null) 
		{
			this.currentMenuState = RUISMenuStates.calibration;
		}
		
		try
		{
			Input.GetButtonDown("RUISMenu");
		}
		catch (UnityException)
		{
			ruisMenuButtonDefined = false;
		}
		
		foreach(GameObject button in buttons) 
		{
			UIEventListener.Get(button).onClick += buttonPressed;
		}
		
		foreach(GameObject checkBox in checkBoxes) 
		{
			UIEventListener.Get (checkBox).onClick += checkBoxClicked;
		}
		
		foreach(GameObject textField in textFields) 
		{
			UIEventListener.Get (textField).onInput += textFieldChanged;
		}
		
		
		currentMenuState = RUISMenuStates.selectAndConfigureDevices;
		
		jumpGesture = FindObjectOfType(typeof(RUISJumpGestureRecognizer)) as RUISJumpGestureRecognizer;
		
		SaveInputChanges(); // Save initial settings
		updateCalibratableDevices();
		
		UpdateGUI();
		handleInfotexts();
		handleSelectAndConfigureDevicesGUISpacing();
		
		// Menu is hidden upon init
		Hide3DGUI();

		this.transform.localPosition = new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ);
		this.transform.localScale = new Vector3(displayManager.guiScaleX, displayManager.guiScaleY, 1);
		
		if(	   displayManager.displays[displayManager.guiDisplayChoice].isObliqueFrustum 
			&& displayManager.displays[displayManager.guiDisplayChoice].linkedCamera 
			&& !displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.isHmdCamera)
		{
			this.transform.localRotation = Quaternion.LookRotation(-displayManager.displays[displayManager.guiDisplayChoice].DisplayNormal, 
			                                                        displayManager.displays[displayManager.guiDisplayChoice].DisplayUp     );
			this.transform.localPosition = displayManager.displays[displayManager.guiDisplayChoice].displayCenterPosition
											+ this.transform.localRotation * new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ);
		}
	}
	
	public void Show3DGUI()
	{
		this.transform.Find("NGUIControls/Panel").gameObject.SetActive(true);
		this.transform.Find("NGUIControls/planeCollider").gameObject.SetActive(true);
	}

	public void Hide3DGUI()
	{
		this.transform.Find("NGUIControls/Panel").gameObject.SetActive(false);
		this.transform.Find("NGUIControls/planeCollider").gameObject.SetActive(false);
	}
	
	void textFieldChanged(GameObject textFieldObject, string text)  
	{
		
		switch(textFieldObject.name) 
		{
			case "Input - PSMove Port": 
				int num;
				if(int.TryParse(textFieldObject.GetComponent<UIInput>().text, out num))
					inputManager.PSMovePort = num; 
				break;
			case "Input - PSMove IP": inputManager.PSMoveIP = textFieldObject.GetComponent<UIInput>().text; break;
		}	
	}
	
	void checkBoxClicked(GameObject clickedGameObject) 
	{
		switch(clickedGameObject.name) 
		{
			case "Checkbox - Use Kinect": inputManager.enableKinect = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Kinect 2": inputManager.enableKinect2 = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Custom 1": inputManager.enableCustomDevice1 = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Custom 2": inputManager.enableCustomDevice2 = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use PSMove": inputManager.enablePSMove = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Hydra": inputManager.enableRazerHydra = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
//			case "Checkbox - Use Kinect Drift Correction": inputManager.kinectDriftCorrectionPreferred = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
		}
		updateCalibratableDevices();
	}

//	void enableOculusPositionalTracking()
//	{
//		OVRManager ovrManager = FindObjectOfType<OVRManager>();
//		if(ovrManager)
//			ovrManager.usePositionTracking = true;
//		if(OVRManager.tracker != null)
//			OVRManager.tracker.isEnabled = true;
//	}
	
	void buttonPressed(GameObject clickedGameObject) 
	{ 
	
		switch(currentMenuState) 
		{
			
			case RUISMenuStates.selectAndConfigureDevices:
				switch(clickedGameObject.name) 
				{
					case "Button - Calibration": 
						calibrationReady = false;
						menuIsVisible = false;
						inputManager.Export(inputManager.filename);
						calibrationDropDownSelection = this.transform.Find(
							"NGUIControls/Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection;
												
//						if(calibrationDropDownSelection.Contains("Oculus")) // *** HACK TODO: Not the best way to be sure that we will calibrate Oculus Rift
//									enableOculusPositionalTracking();

						SaveInputChanges();
						this.transform.parent = null;
//						DontDestroyOnLoad(this);
						currentMenuState = RUISMenuStates.calibration;
//						previousSceneId = Application.loadedLevel;
						Hide3DGUI();

						// NOTE: The calibrationDropDownSelection determines the device pair to be calibrated, and it must follow the format "Device A - Device B"
						//       The string must correspond the options in the Awake() method of RUISCoordinateCalibration script

						// Trim out the " (HMD)" and " (controller)" from the OpenVR option string
//						string dropDownSelectionSubstring = this.calibrationDropDownSelection;
//						if(dropDownSelectionSubstring != null)
//						{
//							int stringIndex = dropDownSelectionSubstring.IndexOf("OpenVR (");
//							if(stringIndex >= 0 && stringIndex + 6 < dropDownSelectionSubstring.Length)
//							{
//									dropDownSelectionSubstring = dropDownSelectionSubstring.Substring(0, stringIndex + 6);
//							}
//						}

						// Hacky way to pass information between loading scenes
						RUISCalibrationProcessSettings.isCalibrating = true;
						RUISCalibrationProcessSettings.devicePair = this.calibrationDropDownSelection; // dropDownSelectionSubstring
						RUISCalibrationProcessSettings.previousSceneId = Application.loadedLevel;
						RUISCalibrationProcessSettings.enablePSMove 		= inputManager.enablePSMove;
						RUISCalibrationProcessSettings.enableKinect 		= inputManager.enableKinect;
						RUISCalibrationProcessSettings.enableKinect2 		= inputManager.enableKinect2;
						RUISCalibrationProcessSettings.enableRazerHydra 	= inputManager.enableRazerHydra;
						RUISCalibrationProcessSettings.enableCustomDevice1 	= inputManager.enableCustomDevice1;
						RUISCalibrationProcessSettings.enableCustomDevice2 	= inputManager.enableCustomDevice2;
						RUISCalibrationProcessSettings.customDevice1Name	= inputManager.customDevice1Name;
						RUISCalibrationProcessSettings.customDevice2Name	= inputManager.customDevice2Name;
						RUISCalibrationProcessSettings.customDevice1Conversion = inputManager.customDevice1Conversion;
						RUISCalibrationProcessSettings.customDevice2Conversion = inputManager.customDevice2Conversion;
						RUISCalibrationProcessSettings.PSMoveIP 			= inputManager.PSMoveIP;
						RUISCalibrationProcessSettings.PSMovePort 			= inputManager.PSMovePort;
//						RUISCalibrationProcessSettings.jumpGestureEnabled = inputManager.jumpGestureEnabled;
						if(coordinateSystem)
						{
							RUISCalibrationProcessSettings.yawOffset = coordinateSystem.yawOffset;
							RUISCalibrationProcessSettings.positionOffset = coordinateSystem.positionOffset;
							RUISCalibrationProcessSettings.originalMasterCoordinateSystem = coordinateSystem.rootDevice;
						}

						Application.LoadLevel("calibration");
					break;
					
					case "Button - Display Management": 
						toggleKeystoneConfigurationLayer();
						currentMenuState = RUISMenuStates.keyStoneConfiguration;
					break;
					
					case "Button - Save Config & Restart Scene":  
						inputManager.Export(inputManager.filename);
						SaveInputChanges();
						Application.LoadLevel(Application.loadedLevel);
					break;
					
					case "Button - Discard Configuration":  
						DiscardInputChanges();
					break;
					
					case "Button - Quit Application": 
						if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill(); 
						else Application.Quit();
					break;
			
			}
			break;
			
			case RUISMenuStates.keyStoneConfiguration:
				switch(clickedGameObject.name)
				{
					
					case "Button - Reset Keystoning":
						foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
						{
							keystoningConfiguration.ResetConfiguration();
						}
					break;	
					
					case "Button - Save Configurations":
						(FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).SaveDisplaysToXML();
					break;
					
					case "Button - Load Old Configurations":	
						(FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager).LoadDisplaysFromXML();
					break;
					
					case "Button - End Display Editing":
						toggleKeystoneConfigurationLayer();
						currentMenuState = RUISMenuStates.selectAndConfigureDevices;
					break;
				}
			break;	
		}
	}
	
	
	void Update () 
	{
		
		
		
		if ((!ruisMenuButtonDefined && Input.GetKeyDown(KeyCode.Escape)) || (ruisMenuButtonDefined && Input.GetButtonDown("RUISMenu"))) 
		{
			menuIsVisible = !menuIsVisible;

			if(!menuIsVisible) 
			{
				Hide3DGUI();
			}
			else 
			{
				Show3DGUI();
			}
		}

		if(menuIsVisible) 
		{
			#if UNITY_EDITOR
			// For interactively adjusting the menu position and scale in Unity Editor
			this.transform.localPosition = this.transform.localRotation * new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ);
//			if(displayManager.displays[displayManager.guiDisplayChoice].isObliqueFrustum)
//			{
//				this.transform.localPosition += displayManager.displays[displayManager.guiDisplayChoice].displayCenterPosition;
//			}
			this.transform.localScale = new Vector3(displayManager.guiScaleX, displayManager.guiScaleY, 1);
			#endif

			if(	   displayManager.displays[displayManager.guiDisplayChoice].isObliqueFrustum 
				&& displayManager.displays[displayManager.guiDisplayChoice].linkedCamera 
				&& !displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.isHmdCamera)
			{
				Quaternion outerRot = displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.transform.rotation;
				Quaternion wallOrientation = Quaternion.LookRotation(
					(-displayManager.displays[displayManager.guiDisplayChoice].DisplayNormal), 
					 displayManager.displays[displayManager.guiDisplayChoice].DisplayUp);

				// *** HACK why is this sign flip necessary to keep the menu at right place??
				outerRot = new Quaternion(outerRot.x, outerRot.y, -outerRot.z, outerRot.w);

				this.transform.rotation = outerRot * wallOrientation;

				this.transform.position = displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.transform.position
						+ outerRot * (displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.KeystoningHeadTrackerPosition
						+ wallOrientation * new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ));
						
				// Vector3 eulerAngles = this.transform.rotation.eulerAngles;
				// eulerAngles.z = -eulerAngles.z; // *** HACK TODO
				// this.transform.position = displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.transform.position
					// + Quaternion.Euler(eulerAngles) * (	  displayManager.displays[displayManager.guiDisplayChoice].linkedCamera.KeystoningHeadTrackerPosition
													// + new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ));

			}

			switch(currentMenuState) 
			{
				
				case RUISMenuStates.calibration: 
					this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices").gameObject.SetActive(false);
					this.transform.Find("NGUIControls/Panel/keyStoneConfiguration").gameObject.SetActive(false);
					// Show nothing, use 2d menu to exit calibration scene
				break;
				
				case RUISMenuStates.selectAndConfigureDevices: 
					this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices").gameObject.SetActive(true);
					this.transform.Find("NGUIControls/Panel/keyStoneConfiguration").gameObject.SetActive(false);
					
					if(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked) 
					{
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Label - PSMove IP").gameObject.SetActive(true);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Label - PSMove Port").gameObject.SetActive(true);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject.SetActive(true);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject.SetActive(true);	
					}
					else 
					{
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Label - PSMove IP").gameObject.SetActive(false);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Label - PSMove Port").gameObject.SetActive(false);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject.SetActive(false);
						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject.SetActive(false);
					}
					
//					if(this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked) 
//					{
//						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject.SetActive(true);
//					}
//					else 
//					{
//						this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject.SetActive(false);
//					}
					
					handleSelectAndConfigureDevicesGUISpacing();
					
					
				break;
				
				case RUISMenuStates.keyStoneConfiguration:
					this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices").gameObject.SetActive(false);
					this.transform.Find("NGUIControls/Panel/keyStoneConfiguration").gameObject.SetActive(true);
				break;
			}
			UpdateGUI();
			handleInfotexts();
		}
	}
	
	
	private void handleSelectAndConfigureDevicesGUISpacing() 
	{
//		Transform PSMoveGUIObj  	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/PSMove").transform;
		Transform KinectGUIObj  	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect").transform;
		Transform Kinect2GUIObj 	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect2").transform;
		Transform Custom1GUIObj 	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom1").transform;
		Transform Custom2GUIObj 	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom2").transform;
		Transform HydraGUIObj   	 = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Hydra").transform;
		Transform InfotextsGUIObject = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Infotexts").transform;
		
//		Transform ButtonsGUIObj = this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons").transform;

		bool kinectSelected   = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked;
		bool kinect2Selected = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").GetComponent<UICheckbox>().isChecked;
		bool custom1Selected = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom1/Checkbox - Use Custom 1").GetComponent<UICheckbox>().isChecked;
		bool custom2Selected = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom2/Checkbox - Use Custom 2").GetComponent<UICheckbox>().isChecked;
		bool psMoveSelected   = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked;
		bool hydraSelected     = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").GetComponent<UICheckbox>().isChecked;
		
		Vector3 overAllOffset = new Vector3(0, 0, 0);
		Vector3 kinectOffset = new Vector3(0,0,0);
		Vector3 kinect2Offset = new Vector3(0,0,0);
		Vector3 hydraOffset = new Vector3(0,0,0);
		Vector3 infotextsOffset = new Vector3(0,0,0);
		
		if(psMoveSelected) { 
			overAllOffset += new Vector3(0, -70.0f, 0);  
			kinectOffset = overAllOffset;
		}
		if(kinectSelected || psMoveSelected) {
//			if(kinectSelected) overAllOffset +=  new Vector3(0, -30.0f, 0); 	
			kinect2Offset = overAllOffset; 	
		}
		if(kinect2Selected || kinectSelected || psMoveSelected) {
//			if(kinect2Selected) overAllOffset += new Vector3(0, 0, 0); 	
			hydraOffset = overAllOffset; 	
		}	
		if(hydraSelected || kinect2Selected || kinectSelected || psMoveSelected) {
//			if(hydraSelected) overAllOffset += new Vector3(0, 0, 0);
			infotextsOffset = overAllOffset; 	
		}

		KinectGUIObj.localPosition 	= kinectOrigGUIPos 	+ kinectOffset; 
		Kinect2GUIObj.localPosition = kinect2OrigGUIPos + kinect2Offset; 
		HydraGUIObj.localPosition 	= hydraOrigGUIPos 	+ hydraOffset; 
		Custom1GUIObj.localPosition = custom1OrigGUIPos + overAllOffset;
		Custom2GUIObj.localPosition = custom2OrigGUIPos + overAllOffset;
		InfotextsGUIObject.localPosition = infotextOrigGUIPos + infotextsOffset;
	}
	
	private void handleInfotexts() 
	{
		GameObject 	infotext_Changes_saved = this.transform.Find(        "NGUIControls/Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes saved").gameObject;
		GameObject 	infotext_Changes_are_not_saved_in_free_version = this.transform.Find(
																		 "NGUIControls/Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes are not saved in free version").gameObject;
		GameObject  infotext_Changes_not_saved_yet = this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes not saved yet").gameObject;
		GameObject 	infotext_Hmd_Not_Detected = this.transform.Find(    "NGUIControls/Panel/selectAndConfigureDevices/Infotexts/HMD/Label - HMD not detected").gameObject; // Was "Rift not detected"
		GameObject 	infotext_Hmd_Detected = this.transform.Find(  "NGUIControls/Panel/selectAndConfigureDevices/Infotexts/HMD/Label - HMD detected").gameObject; // Was "Oculus DK1 detected"
		
		if(RUISDisplayManager.IsHmdPresent())
		{
			if(infotext_Hmd_Detected)
			{
				UILabel hmdDetectedLabel = infotext_Hmd_Detected.GetComponent<UILabel>();
				if(hmdDetectedLabel)
					hmdDetectedLabel.text = RUISDisplayManager.GetHmdModel() + " detected";
			}
			infotext_Hmd_Not_Detected.SetActive(false);
			infotext_Hmd_Detected.SetActive(true); 
		}
		else
			if(infotext_Hmd_Not_Detected)
			{
				infotext_Hmd_Not_Detected.SetActive(true); 
				infotext_Hmd_Detected.SetActive(false);  
			}

//		bool isRiftConnected = false;
//		{
//			try
//			{
//				if(OVRManager.display != null) //06to08
//					isRiftConnected = OVRManager.display.isPresent; //06to08
//				if(OVRManager.capiHmd != null)
//					ovrHmdVersion = OVRManager.capiHmd.GetDesc().Type; //06to08
//
//
//			}
//			catch(UnityException e)
//			{
//				Debug.LogError(e);
//			}
//		}
//		if(isRiftConnected && ovrHmdVersion == Ovr.HmdType.DK1) //06to08
//		{
//			infotext_Rift_not_Detected.SetActive(false);
//			infotext_Oculus_DK1_detected.SetActive(true); 
//			infotext_Oculus_DK2_detected.SetActive(false);
//		}
//		else if(isRiftConnected && (ovrHmdVersion == Ovr.HmdType.DK2 || ovrHmdVersion == Ovr.HmdType.Other)) //06to08
//		{
//			infotext_Rift_not_Detected.SetActive(false);
//			infotext_Oculus_DK2_detected.SetActive(true);
//			infotext_Oculus_DK1_detected.SetActive(false);  
//		}
//		else {
//			infotext_Rift_not_Detected.SetActive(true);
//			infotext_Oculus_DK1_detected.SetActive(false);
//			infotext_Oculus_DK2_detected.SetActive(false); 
//		}
			
		if(!XmlImportExport.XmlHandlingFunctionalityAvailable()) 
		{
			infotext_Changes_are_not_saved_in_free_version.SetActive(true);
			infotext_Changes_saved.SetActive(false);
			infotext_Changes_not_saved_yet.SetActive(false);
		}
		else 
		{
			infotext_Changes_are_not_saved_in_free_version.SetActive(false);
			if(originalPSMoveIP == inputManager.PSMoveIP && 
			   originalPSMovePort == inputManager.PSMovePort && 
			   originalEnablePSMove == inputManager.enablePSMove && 
			   originalEnableKinect == inputManager.enableKinect && 
			   originalEnableKinect2 == inputManager.enableKinect2 && 
			   originalEnableHydra == inputManager.enableRazerHydra)
//			   originalEnableJumpGesture == inputManager.jumpGestureEnabled && 
//			   originalKinectDriftCorrection == inputManager.kinectDriftCorrectionPreferred) 
			   {
		   			infotext_Changes_saved.SetActive(true);
					infotext_Changes_not_saved_yet.SetActive(false);
			   }
			else 
			{
				infotext_Changes_not_saved_yet.SetActive(true);
				infotext_Changes_saved.SetActive(false);
			}
		}

	}
	
	private void toggleKeystoneConfigurationLayer()
	{
		foreach (RUISKeystoningConfiguration keystoningConfiguration in FindObjectsOfType(typeof(RUISKeystoningConfiguration)) as RUISKeystoningConfiguration[])
		{
			if (keystoneConfigurationLayerActive)
			{
				keystoningConfiguration.EndEditing();
			}
			else
			{
				keystoningConfiguration.StartEditing();
			}
		}
		keystoneConfigurationLayerActive = !keystoneConfigurationLayerActive;
	}
	
	private void SaveInputChanges()
	{
		originalPSMoveIP = inputManager.PSMoveIP;
		originalPSMovePort = inputManager.PSMovePort;
		originalEnablePSMove = inputManager.enablePSMove;
		originalEnableKinect = inputManager.enableKinect;
		originalEnableKinect2 = inputManager.enableKinect2;
		originalEnableHydra = inputManager.enableRazerHydra;
//		originalEnableJumpGesture = inputManager.jumpGestureEnabled;
//		originalKinectDriftCorrection = inputManager.kinectDriftCorrectionPreferred;
	}
	
	private void DiscardInputChanges()
	{
		inputManager.enablePSMove = originalEnablePSMove;
		inputManager.enableKinect = originalEnableKinect;
		inputManager.enableKinect2 = originalEnableKinect2;
//		if (jumpGesture)
//		{
//			if (originalEnableJumpGesture)
//			{
//				jumpGesture.EnableGesture();
//			}
//			else
//			{
//				jumpGesture.DisableGesture();
//			}
//		}
		inputManager.enableRazerHydra = originalEnableHydra;
		inputManager.PSMoveIP = originalPSMoveIP;
		inputManager.PSMovePort = originalPSMovePort;
//		inputManager.jumpGestureEnabled = originalEnableJumpGesture;
//		inputManager.kinectDriftCorrectionPreferred = originalKinectDriftCorrection;
	}
	
	private void UpdateGUI() 
	{
		if(currentMenuState == RUISMenuStates.selectAndConfigureDevices) 
		{
			if(inputManager.enablePSMove) 
			{
				this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").GetComponent<UIInput>().text = inputManager.PSMoveIP;
				this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").GetComponent<UIInput>().text = inputManager.PSMovePort.ToString();
			}	
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked = inputManager.enableKinect;
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").GetComponent<UICheckbox>().isChecked = inputManager.enableKinect2;
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom1/Checkbox - Use Custom 1").GetComponent<UICheckbox>().isChecked = inputManager.enableCustomDevice1;
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Custom2/Checkbox - Use Custom 2").GetComponent<UICheckbox>().isChecked = inputManager.enableCustomDevice2;
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked = inputManager.enablePSMove;
			this.transform.Find("NGUIControls/Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").GetComponent<UICheckbox>().isChecked = inputManager.enableRazerHydra;
//			if(inputManager.enableKinect) 
//			{
//				this.transform.Find(
//					"NGUIControls/Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").GetComponent<UICheckbox>().isChecked = inputManager.kinectDriftCorrectionPreferred;
//			}
		}
	}
	
	
	private void updateCalibratableDevices() 
	{
		List<string> dropDownChoices = new List<string>();
		
		string currentSelection = this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection;
		
		//dropDownChoices.Add ("Select device(s)");
		if(inputManager.enableKinect) dropDownChoices.Add ("Kinect 1 floor data");
		if(inputManager.enableKinect2) dropDownChoices.Add ("Kinect 2 floor data");
		    
		// NOTE: The dropDownChoices determine the device pair to be calibrated, and they must follow the format "Device A - Device B". 
		//       The string must correspond the options in the Awake() method of RUISCoordinateCalibration script
		if(inputManager.enableKinect 			   && inputManager.enableKinect2) 		dropDownChoices.Add("Kinect 1 - Kinect2");
		if(inputManager.enableKinect 			   && inputManager.enablePSMove)  		dropDownChoices.Add("Kinect 1 - PSMove");
		if(inputManager.enableKinect2 			   && inputManager.enablePSMove)  		dropDownChoices.Add("Kinect 2 - PSMove");
		if(inputManager.enableCustomDevice1 	   && inputManager.enableCustomDevice2) dropDownChoices.Add("Custom 1 - Custom 2");
		if(RUISDisplayManager.IsOpenVrAccessible() && inputManager.enableCustomDevice1)	dropDownChoices.Add("Custom 1 - OpenVR (controller)");
		if(RUISDisplayManager.IsOpenVrAccessible() && inputManager.enableCustomDevice2)	dropDownChoices.Add("Custom 2 - OpenVR (controller)");
		if(RUISDisplayManager.IsOpenVrAccessible() && inputManager.enableKinect2) 		dropDownChoices.Add("Kinect 2 - OpenVR (controller)");
		if(RUISDisplayManager.IsHmdPresent() 	   && inputManager.enableKinect2) 		dropDownChoices.Add("Kinect 2 - OpenVR (HMD)");
		if(RUISDisplayManager.IsHmdPresent() 	   && inputManager.enableKinect)  		dropDownChoices.Add("Kinect 1 - OpenVR (HMD)");
//		if(RUISDisplayManager.IsHmdPresent() 	   && inputManager.enableCustomDevice1) dropDownChoices.Add("Custom 1 - OpenVR (HMD)"); //TODO
//		if(RUISDisplayManager.IsHmdPresent() 	   && inputManager.enableCustomDevice2) dropDownChoices.Add("Custom 2 - OpenVR (HMD)"); //TODO
		if(RUISDisplayManager.IsHmdPresent() 	   && inputManager.enablePSMove)  		dropDownChoices.Add("PSMove - OpenVR (HMD)");
//		if(RUISDisplayManager.IsOpenVrAccessible() && inputManager.enableKinect) 		dropDownChoices.Add("Kinect 1 - OpenVR (controller)"); //TODO
//		if(RUISDisplayManager.enableKinect		   && inputManager.enableCustomDevice1) dropDownChoices.Add("Custom 1 - Kinect 1");
//		if(RUISDisplayManager.enableKinect2		   && inputManager.enableCustomDevice2) dropDownChoices.Add("Custom 2 - Kinect 2)"); 
//		if(RUISDisplayManager.enableKinect2 	   && inputManager.enableCustomDevice1) dropDownChoices.Add("Custom 1 - Kinect 2");
//		if(RUISDisplayManager.enableKinect		   && inputManager.enableCustomDevice2) dropDownChoices.Add("Custom 2 - Kinect 1)"); //TODO

		
		if(dropDownChoices.Count == 0) 
		{
			dropDownChoices.Add ("Select device(s)");
		}
		
		this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().items = dropDownChoices;
		if(!dropDownChoices.Contains(currentSelection)) 
		{
			this.transform.Find ("NGUIControls/Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection = dropDownChoices[0];
		}
		
	}
	
//	void OnGUI()
//	{
//		if(currentMenuState == RUISMenuStates.calibration && menuIsVisible) 
//		{
//			GUILayout.Window(0, new Rect(50, 50, 150, 200), DrawWindow, "RUIS");
//		}
//	}
//
//	void DrawWindow(int windowId)
//	{	
//		if(calibrationReady) 
//		{
//			GUILayout.Label("Calibration finished.");
//			GUILayout.Space(20);
//			if(GUILayout.Button("Exit calibration"))
//			{
//				RetrieveOriginalCoordinateSystemSettings();
//				Destroy(this.gameObject);
//				Application.LoadLevel(previousSceneId);
//			}
//		}
//		else 
//		{
//			GUILayout.Label("Calibration not finished yet.");
//			GUILayout.Space(20);
//			if(GUILayout.Button("Abort calibration"))
//			{
//				RetrieveOriginalCoordinateSystemSettings();
//				Destroy(this.gameObject);
//				Application.LoadLevel(previousSceneId);
//			}
//		}
//	}
//
//	void RetrieveOriginalCoordinateSystemSettings()
//	{
//		// Retrieve original coordinateSystem settings
//		if(coordinateSystem)
//		{
//			coordinateSystem.rootDevice = orignalMasterDevice;
//			coordinateSystem.applyToRootCoordinates = originalUseMasterCoordinateSystem;
//		}
//	}
}
