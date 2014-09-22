﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OVR;

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
	
	public bool 	originalEnablePSMove,
					originalEnableKinect, 
					originalEnableKinect2, 
					originalEnableJumpGesture,
					originalEnableHydra, 
					originalKinectDriftCorrection; 
	
	public string 	originalPSMoveIP;
	public int		originalPSMovePort;
	
	private bool ruisMenuButtonDefined = true;
	
	public RUISJumpGestureRecognizer jumpGesture;
	
	public bool menuIsVisible = false;
	
	private int previousSceneId;
	
	private bool calibrationInfoMessageShown = false;
	
	// selectAndConfigureDevices phase initial GUI element positions
	Vector3 psMoveOrigGUIPos, kinectOrigGUIPos, kinect2OrigGUIPos, hydraOrigGUIPos, buttonsOrigGUIPos, infotextOrigGUIPos; 
	
	public string calibrationDropDownSelection;
	
	public bool setOnce;
	
	public bool calibrationReady;
	
	void Awake() 
	{
		// BUTTONS
		// Phase: select and configure devices
		buttons.Add(this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Button - Calibration").gameObject);
		buttons.Add(this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Button - Display Management").gameObject);
		buttons.Add(this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Button - Save Config & Restart Scene").gameObject);
		buttons.Add(this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Button - Discard Configuration").gameObject);
		buttons.Add(this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Button - Quit Application").gameObject);
		
		// Phase: key stone configuration
		buttons.Add(this.transform.Find ("Panel/keyStoneConfiguration/Button - Reset Keystoning").gameObject);
		buttons.Add(this.transform.Find ("Panel/keyStoneConfiguration/Button - Save Configurations").gameObject);
		buttons.Add(this.transform.Find ("Panel/keyStoneConfiguration/Button - Load Old Configurations").gameObject);
		buttons.Add(this.transform.Find ("Panel/keyStoneConfiguration/Button - End Display Editing").gameObject);
		
		// CHECKBOXES
		checkBoxes.Add (this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").gameObject);
		checkBoxes.Add (this.transform.Find ("Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").gameObject);
		checkBoxes.Add (this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject);
		checkBoxes.Add (this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").gameObject);
		checkBoxes.Add (this.transform.Find ("Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").gameObject);
		
		// TEXTFIELDS
		textFields.Add (this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject);
		textFields.Add (this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject);
		
		inputManager = FindObjectOfType(typeof(RUISInputManager)) as RUISInputManager;
		displayManager = FindObjectOfType(typeof(RUISDisplayManager)) as RUISDisplayManager;
		
		psMoveOrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/PSMove").transform.localPosition;
		kinectOrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect").transform.localPosition;
		kinect2OrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect2").transform.localPosition;
		hydraOrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/Hydra").transform.localPosition;
		infotextOrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/Infotexts").transform.localPosition;
		buttonsOrigGUIPos = this.transform.Find ("Panel/selectAndConfigureDevices/Buttons").transform.localPosition;
	}
		
	void Start () 
	{
		setOnce = false;
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
			case "Checkbox - Use Kinect 2":inputManager.enableKinect2 = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Kinect Drift Correction": inputManager.kinectDriftCorrectionPreferred = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use PSMove": inputManager.enablePSMove = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
			case "Checkbox - Use Hydra":inputManager.enableRazerHydra = clickedGameObject.GetComponent<UICheckbox>().isChecked; break;
		}
		updateCalibratableDevices();
	}
	
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
						calibrationDropDownSelection = this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection;
						SaveInputChanges();
						DontDestroyOnLoad(this);
						this.transform.parent = null;
						currentMenuState = RUISMenuStates.calibration;
						previousSceneId = Application.loadedLevel;
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
						Application.Quit();
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
		}
		
		if(!menuIsVisible) 
		{
			this.transform.Find("Panel").gameObject.SetActive(false);
			this.transform.Find("planeCollider").gameObject.SetActive(false);
			//this.transform.Find("guiCameraForRift").gameObject.SetActive(false);
		}
		else 
		{
		
			this.transform.localPosition = new Vector3(displayManager.guiX, displayManager.guiY, displayManager.guiZ);
			this.transform.localScale = new Vector3(displayManager.guiScaleX * 0.002777778f, displayManager.guiScaleY * 0.002777778f, 1 * 0.002777778f);
			
			
			this.transform.Find("Panel").gameObject.SetActive(true);
			this.transform.Find("planeCollider").gameObject.SetActive(true);
			//this.transform.Find("guiCameraForRift").gameObject.SetActive(true);
			
			
			
			switch(currentMenuState) 
			{
				
				case RUISMenuStates.calibration: 
					this.transform.Find("Panel/selectAndConfigureDevices").gameObject.SetActive(false);
					this.transform.Find("Panel/keyStoneConfiguration").gameObject.SetActive(false);
					// Show nothing, use 2d menu to exit calibration scene
				break;
				
				case RUISMenuStates.selectAndConfigureDevices: 
					this.transform.Find("Panel/selectAndConfigureDevices").gameObject.SetActive(true);
					this.transform.Find("Panel/keyStoneConfiguration").gameObject.SetActive(false);
					
					if(this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked) 
					{
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Label - PSMove IP").gameObject.SetActive(true);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Label - PSMove Port").gameObject.SetActive(true);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject.SetActive(true);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject.SetActive(true);	
					}
					else 
					{
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Label - PSMove IP").gameObject.SetActive(false);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Label - PSMove Port").gameObject.SetActive(false);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").gameObject.SetActive(false);
						this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").gameObject.SetActive(false);
					}
					
					if(this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked) 
					{
						this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject.SetActive(true);
					}
					else 
					{
						this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").gameObject.SetActive(false);
					}
					
					handleSelectAndConfigureDevicesGUISpacing();
					
					
				break;
				
				case RUISMenuStates.keyStoneConfiguration:
					this.transform.Find("Panel/selectAndConfigureDevices").gameObject.SetActive(false);
					this.transform.Find("Panel/keyStoneConfiguration").gameObject.SetActive(true);
				break;
			}
			UpdateGUI();
			handleInfotexts();
		}
	}
	
	
	private void handleSelectAndConfigureDevicesGUISpacing() 
	{
		Transform PSMoveGUIObj = this.transform.Find ("Panel/selectAndConfigureDevices/PSMove").transform;
		Transform KinectGUIObj = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect").transform;
		Transform Kinect2GUIObj = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect2").transform;
		Transform HydraGUIObj = this.transform.Find ("Panel/selectAndConfigureDevices/Hydra").transform;
		Transform InfotextsGUIObject = this.transform.Find ("Panel/selectAndConfigureDevices/Infotexts").transform;
		
		Transform ButtonsGUIObj = this.transform.Find ("Panel/selectAndConfigureDevices/Buttons").transform;
		
		
		bool kinectSelected = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked;
		bool kinect2Selected = this.transform.Find ("Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").GetComponent<UICheckbox>().isChecked;
		bool psMoveSelected = this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked;
		bool hydraSelected = this.transform.Find ("Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").GetComponent<UICheckbox>().isChecked;
		
		Vector3 overAllOffset = new Vector3(0, 0, 0);
		Vector3 kinectOffset = new Vector3(0,0,0);
		Vector3 kinect2Offset = new Vector3(0,0,0);
		Vector3 hydraOffset = new Vector3(0,0,0);
		Vector3 infotextsOffset = new Vector3(0,0,0);
		Vector3 buttonsOffset = new Vector3(0,0,0);
		
		if(psMoveSelected) { 
			overAllOffset += new Vector3(0, -70.0f, 0);  
			kinectOffset = overAllOffset;
		}
		if(kinectSelected || psMoveSelected) {
			if(kinectSelected) overAllOffset +=  new Vector3(0, -30.0f, 0); 	
			kinect2Offset = overAllOffset; 	
		}
		if(kinect2Selected || kinectSelected || psMoveSelected) {
			if(kinect2Selected) overAllOffset += new Vector3(0, 0, 0); 	
			hydraOffset = overAllOffset; 	
		}	
		if(hydraSelected || kinect2Selected || kinectSelected || psMoveSelected) {
			if(hydraSelected) overAllOffset += new Vector3(0, 0, 0); 	
			infotextsOffset = overAllOffset; 	
		}
		/*	
		if(hydraSelected || kinect2Selected || kinectSelected || psMoveSelected) {
			if(hydraSelected) overAllOffset += new Vector3(0, 0, 0); 	
			buttonsOffset = overAllOffset; 	
		}
		*/
		
		
		KinectGUIObj.localPosition = kinectOrigGUIPos;
		Kinect2GUIObj.localPosition = kinect2OrigGUIPos;
		HydraGUIObj.localPosition = hydraOrigGUIPos;
		InfotextsGUIObject.localPosition = infotextOrigGUIPos;
		//ButtonsGUIObj.localPosition = buttonsOrigGUIPos;
				
		KinectGUIObj.localPosition = kinectOrigGUIPos + kinectOffset; 
		Kinect2GUIObj.localPosition = kinect2OrigGUIPos + kinect2Offset; 
		HydraGUIObj.localPosition = hydraOrigGUIPos + hydraOffset; 
		InfotextsGUIObject.localPosition = infotextOrigGUIPos + infotextsOffset;
		//ButtonsGUIObj.localPosition = buttonsOrigGUIPos + buttonsOffset;
		
	}
	
	private void handleInfotexts() 
	{
		GameObject 	infotext_Changes_saved = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes saved").gameObject;
		GameObject 	infotext_Changes_are_not_saved_in_free_version = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes are not saved in free version").gameObject;
		GameObject  infotext_Changes_not_saved_yet = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Saving/Label - Changes not saved yet").gameObject;
		GameObject 	infotext_Rift_not_Detected = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Rift/Label - Rift not detected").gameObject;
		GameObject 	infotext_Oculus_DK1_detected = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Rift/Label - Oculus DK1 detected").gameObject;
		GameObject 	infotext_Oculus_DK2_detected = this.transform.Find("Panel/selectAndConfigureDevices/Infotexts/Rift/Label - Oculus DK2 detected").gameObject;
		
		
		var HMD = OVR.Hmd.GetHmd();
		ovrTrackingState riftState = HMD.GetTrackingState();      
		bool isRiftConnected = (riftState.StatusFlags & (uint)ovrStatusBits.ovrStatus_HmdConnected) != 0; // TODO: Use OVR methods when they start to work
		/*
		if(!isRiftConnected) {
			infotext_Rift_not_Detected.SetActive(true);
			infotext_Oculus_DK1_detected.SetActive(false);
			infotext_Oculus_DK2_detected.SetActive(false); 
		}
		else 
		{
		*/
			Hmd oculusHmdObject = Hmd.GetHmd();
			ovrHmdDesc ovrDesc = oculusHmdObject.GetDesc();
			ovrHmdType ovrHmdVersion = ovrDesc.Type;

			if(ovrHmdVersion == ovrHmdType.ovrHmd_DK1) 
			{
				infotext_Rift_not_Detected.SetActive(false);
				infotext_Oculus_DK1_detected.SetActive(true); 
				infotext_Oculus_DK2_detected.SetActive(false);
			}
			else if(ovrHmdVersion == ovrHmdType.ovrHmd_DK2 && isRiftConnected) 
			{
				infotext_Rift_not_Detected.SetActive(false);
				infotext_Oculus_DK2_detected.SetActive(true);
				infotext_Oculus_DK1_detected.SetActive(false);  
			}
			else {
				infotext_Rift_not_Detected.SetActive(true);
				infotext_Oculus_DK1_detected.SetActive(false);
				infotext_Oculus_DK2_detected.SetActive(false); 
			}
			
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
			   originalEnableJumpGesture == inputManager.jumpGestureEnabled && 
			   originalEnableHydra == inputManager.enableRazerHydra && 
			   originalKinectDriftCorrection == inputManager.kinectDriftCorrectionPreferred) 
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
		originalEnableJumpGesture = inputManager.jumpGestureEnabled;
		originalEnableHydra = inputManager.enableRazerHydra;
		originalKinectDriftCorrection = inputManager.kinectDriftCorrectionPreferred;
	}
	
	private void DiscardInputChanges()
	{
		inputManager.enablePSMove = originalEnablePSMove;
		inputManager.enableKinect = originalEnableKinect;
		inputManager.enableKinect2 = originalEnableKinect2;
		if (jumpGesture)
		{
			if (originalEnableJumpGesture)
			{
				jumpGesture.EnableGesture();
			}
			else
			{
				jumpGesture.DisableGesture();
			}
		}
		inputManager.jumpGestureEnabled = originalEnableJumpGesture;
		inputManager.enableRazerHydra = originalEnableHydra;
		inputManager.kinectDriftCorrectionPreferred = originalKinectDriftCorrection;
		inputManager.PSMoveIP = originalPSMoveIP;
		inputManager.PSMovePort = originalPSMovePort;
	}
	
	private void UpdateGUI() 
	{
		if(currentMenuState == RUISMenuStates.selectAndConfigureDevices) 
		{
			if(inputManager.enablePSMove) 
			{
				this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove IP").GetComponent<UIInput>().text = inputManager.PSMoveIP;
				this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Input - PSMove Port").GetComponent<UIInput>().text = inputManager.PSMovePort.ToString();
			}	
			this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect").GetComponent<UICheckbox>().isChecked = inputManager.enableKinect;
			this.transform.Find ("Panel/selectAndConfigureDevices/Kinect2/Checkbox - Use Kinect 2").GetComponent<UICheckbox>().isChecked = inputManager.enableKinect2;
			this.transform.Find ("Panel/selectAndConfigureDevices/PSMove/Checkbox - Use PSMove").GetComponent<UICheckbox>().isChecked = inputManager.enablePSMove;
			this.transform.Find ("Panel/selectAndConfigureDevices/Hydra/Checkbox - Use Hydra").GetComponent<UICheckbox>().isChecked = inputManager.enableRazerHydra;
			if(inputManager.enableKinect) 
			{
				this.transform.Find ("Panel/selectAndConfigureDevices/Kinect/Checkbox - Use Kinect Drift Correction").GetComponent<UICheckbox>().isChecked = inputManager.kinectDriftCorrectionPreferred;
			}
		}
	}
	
	
	private void updateCalibratableDevices() 
	{
		List<string> dropDownChoices = new List<string>();
		
		string currentSelection = this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection;
		
		//dropDownChoices.Add ("Select device(s)");
		if(inputManager.enableKinect) dropDownChoices.Add ("Kinect floor data");
		if(inputManager.enableKinect2) dropDownChoices.Add ("Kinect 2 floor data");
		
		var HMD = OVR.Hmd.GetHmd();
		ovrTrackingState riftState = HMD.GetTrackingState();      
		bool isRiftConnected = (riftState.StatusFlags & (uint)ovrStatusBits.ovrStatus_HmdConnected) != 0; // TODO: Use OVR methods when they start to work
		
		if(inputManager.enableKinect && inputManager.enableKinect2) dropDownChoices.Add ("Kinect - Kinect2");
		if(inputManager.enableKinect && inputManager.enablePSMove) dropDownChoices.Add ("Kinect - PSMove");
		if(inputManager.enableKinect2 && inputManager.enablePSMove) dropDownChoices.Add ("Kinect 2 - PSMove");
		if(isRiftConnected && inputManager.enableKinect2) dropDownChoices.Add ("Kinect 2 - Oculus DK2");
		if(isRiftConnected && inputManager.enableKinect) dropDownChoices.Add ("Kinect - Oculus DK2");
		if(isRiftConnected && inputManager.enablePSMove) dropDownChoices.Add ("PSMove - Oculus DK2");
		
		if(dropDownChoices.Count == 0) 
		{
			dropDownChoices.Add ("Select device(s)");
		}
		
		this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().items = dropDownChoices;
		if(!dropDownChoices.Contains(currentSelection)) 
		{
			this.transform.Find ("Panel/selectAndConfigureDevices/Buttons/Dropdown - Calibration Devices").GetComponent<UIPopupList>().selection = dropDownChoices[0];
		}
		
	}
	
	void OnGUI()
	{
		if(currentMenuState == RUISMenuStates.calibration && menuIsVisible) 
		{
			GUILayout.Window(0, new Rect(50, 50, 150, 200), DrawWindow, "RUIS");
		}
	}
	
	
	void DrawWindow(int windowId)
	{	
		if(calibrationReady) 
		{
			GUILayout.Label("Calibration finnished.");
			GUILayout.Space(20);
			if(GUILayout.Button("Exit calibration"))
			{
				Destroy(this.gameObject);
				Application.LoadLevel(previousSceneId);
			}
		}
		else 
		{
			GUILayout.Label("Calibration not finnisehd yet.");
			GUILayout.Space(20);
			if(GUILayout.Button("Abort calibration"))
			{
				Destroy(this.gameObject);
				Application.LoadLevel(previousSceneId);
			}
		}
	}
}