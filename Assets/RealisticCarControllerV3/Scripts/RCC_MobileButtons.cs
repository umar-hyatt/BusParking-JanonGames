//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Receiving inputs from UI buttons, and feeds active vehicles on your scene.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/Mobile/RCC UI Mobile Buttons")]
public class RCC_MobileButtons : RCC_Core {

	#region Singleton
	private static RCC_MobileButtons instance;
	public static RCC_MobileButtons Instance{	get{if(instance == null) instance = GameObject.FindObjectOfType<RCC_MobileButtons>(); return instance;}}
	#endregion

	// Getting an Instance of Main Shared RCC Settings.
	#region RCC Settings Instance

	private RCC_Settings RCCSettingsInstance;
	private RCC_Settings RCCSettings {
		get {
			if (RCCSettingsInstance == null) {
				RCCSettingsInstance = RCC_Settings.Instance;
				return RCCSettingsInstance;
			}
			return RCCSettingsInstance;
		}
	}

	#endregion

	public RCC_UIController gasButton;
	public RCC_UIController gradualGasButton;
	public RCC_UIController brakeButton;
	public RCC_UIController leftButton;
	public RCC_UIController rightButton;
	public RCC_UISteeringWheelController steeringWheel;
	public RCC_UIController handbrakeButton;
	public RCC_UIController NOSButton;
	public RCC_UIController NOSButtonSteeringWheel;
	public GameObject gearButton;

	public RCC_UIJoystick joystick;

	public RCC_Inputs inputs = new RCC_Inputs ();

	private float throttleInput = 0f;
	private float brakeInput = 0f;
	private float leftInput = 0f;
	private float rightInput = 0f;
	private float steeringWheelInput = 0f;
	private float handbrakeInput = 0f;
	private float boostInput = 1f;
	private float gyroInput = 0f;
	private float joystickInput = 0f;
	private float horizontalInput;
	private float verticalInput;
	private bool canUseNos = false;

	private Vector3 orgBrakeButtonPos;

	void Start(){

		if(brakeButton)
			orgBrakeButtonPos = brakeButton.transform.position;

		CheckController ();

	}

	void OnEnable(){

		RCC_SceneManager.OnControllerChanged += CheckController;
		RCC_SceneManager.OnVehicleChanged += CheckController;

	}

	private void CheckController(){

		if (!RCC_SceneManager.Instance.activePlayerVehicle)
			return;

		if (RCCSettings.selectedControllerType == RCC_Settings.ControllerType.Mobile) {

			EnableButtons ();
			return;

		} else {

			DisableButtons ();
			return;

		}

	}

	void DisableButtons(){

		if (gasButton)
			gasButton.gameObject.SetActive (false);
		if (gradualGasButton)
			gradualGasButton.gameObject.SetActive (false);
		if (leftButton)
			leftButton.gameObject.SetActive (false);
		if (rightButton)
			rightButton.gameObject.SetActive (false);
		if (brakeButton)
			brakeButton.gameObject.SetActive (false);
		if (steeringWheel)
			steeringWheel.gameObject.SetActive (false);
		if (handbrakeButton)
			handbrakeButton.gameObject.SetActive (false);
		if (NOSButton)
			NOSButton.gameObject.SetActive (false);
		if (NOSButtonSteeringWheel)
			NOSButtonSteeringWheel.gameObject.SetActive (false);
		if (gearButton)
			gearButton.gameObject.SetActive (false);
		if (joystick)
			joystick.gameObject.SetActive (false);

	}

	void EnableButtons(){

		if (gasButton)
			gasButton.gameObject.SetActive (true);
		//			if (gradualGasButton)
		//				gradualGasButton.gameObject.SetActive (true);
		if (leftButton)
			leftButton.gameObject.SetActive (true);
		if (rightButton)
			rightButton.gameObject.SetActive (true);
		if (brakeButton)
			brakeButton.gameObject.SetActive (true);
		if (steeringWheel)
			steeringWheel.gameObject.SetActive (true);
		if (handbrakeButton)
			handbrakeButton.gameObject.SetActive (true);

		if (canUseNos) {

			if (NOSButton)
				NOSButton.gameObject.SetActive (true);
			if (NOSButtonSteeringWheel)
				NOSButtonSteeringWheel.gameObject.SetActive (true);

		}

		if (joystick)
			joystick.gameObject.SetActive (true);
		
	}

	void Update(){

		if (RCCSettings.selectedControllerType != RCC_Settings.ControllerType.Mobile)
			return;

		switch (RCCSettings.mobileController) {

		case RCC_Settings.MobileController.TouchScreen:

			gyroInput = 0f;

			if(steeringWheel && steeringWheel.gameObject.activeInHierarchy)
				steeringWheel.gameObject.SetActive(false);

			if(NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
				NOSButton.gameObject.SetActive(canUseNos);

			if(joystick && joystick.gameObject.activeInHierarchy)
				joystick.gameObject.SetActive(false);

			if(!leftButton.gameObject.activeInHierarchy){

				brakeButton.transform.position = orgBrakeButtonPos;
				leftButton.gameObject.SetActive(true);

			}

			if(!rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(true);

			break;

		case RCC_Settings.MobileController.Gyro:

			gyroInput = Mathf.Lerp(gyroInput, Input.acceleration.x * RCCSettings.gyroSensitivity, Time.deltaTime * 5f);
			brakeButton.transform.position = leftButton.transform.position;

			if(steeringWheel && steeringWheel.gameObject.activeInHierarchy)
				steeringWheel.gameObject.SetActive(false);

			if(NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
				NOSButton.gameObject.SetActive(canUseNos);

			if(joystick && joystick.gameObject.activeInHierarchy)
				joystick.gameObject.SetActive(false);
			
			if(leftButton.gameObject.activeInHierarchy)
				leftButton.gameObject.SetActive(false);

			if(rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(false);

			break;

		case RCC_Settings.MobileController.SteeringWheel:

			gyroInput = 0f;

			if(steeringWheel && !steeringWheel.gameObject.activeInHierarchy){
				steeringWheel.gameObject.SetActive(true);
				brakeButton.transform.position = orgBrakeButtonPos;
			}

			if(NOSButton && NOSButton.gameObject.activeInHierarchy)
				NOSButton.gameObject.SetActive(false);

			if(NOSButtonSteeringWheel && NOSButtonSteeringWheel.gameObject.activeInHierarchy != canUseNos)
				NOSButtonSteeringWheel.gameObject.SetActive(canUseNos);

			if(joystick && joystick.gameObject.activeInHierarchy)
				joystick.gameObject.SetActive(false);
			
			if(leftButton.gameObject.activeInHierarchy)
				leftButton.gameObject.SetActive(false);
			if(rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(false);

			break;

		case RCC_Settings.MobileController.Joystick:

			gyroInput = 0f;

			if (steeringWheel && steeringWheel.gameObject.activeInHierarchy)
				steeringWheel.gameObject.SetActive (false);

			if (NOSButton && NOSButton.gameObject.activeInHierarchy != canUseNos)
				NOSButton.gameObject.SetActive (canUseNos);

			if (joystick && !joystick.gameObject.activeInHierarchy) {
				joystick.gameObject.SetActive (true);
				brakeButton.transform.position = orgBrakeButtonPos;
			}
			
			if(leftButton.gameObject.activeInHierarchy)
				leftButton.gameObject.SetActive(false);

			if(rightButton.gameObject.activeInHierarchy)
				rightButton.gameObject.SetActive(false);

			break;

		}

		throttleInput = GetInput(gasButton) + GetInput(gradualGasButton);
		brakeInput = GetInput (brakeButton);
		leftInput = GetInput(leftButton);
		rightInput = GetInput(rightButton);
		handbrakeInput = GetInput(handbrakeButton);
		boostInput = Mathf.Clamp((GetInput(NOSButton) + GetInput(NOSButtonSteeringWheel)), 0f, 1f);

		if(steeringWheel)
			steeringWheelInput = steeringWheel.input;

		if(joystick)
			joystickInput = joystick.inputHorizontal;
		
		FeedRCC ();

	}

	private void FeedRCC(){

		if (!RCC_SceneManager.Instance.activePlayerVehicle)
			return;

		canUseNos = RCC_SceneManager.Instance.activePlayerVehicle.useNOS;

		inputs.throttleInput = throttleInput;
		inputs.brakeInput = brakeInput;
		inputs.steerInput = -leftInput + rightInput + steeringWheelInput + gyroInput + joystickInput;
		inputs.handbrakeInput = handbrakeInput;
		inputs.boostInput = boostInput;

	}

	// Gets input from button.
	float GetInput(RCC_UIController button){

		if(button == null)
			return 0f;

		return(button.input);

	}

	// Gets input from joystick.
	Vector2 GetInput(RCC_UIJoystick joystick){

		if(joystick == null)
			return Vector2.zero;

		return(joystick.inputVector);

	}

	void OnDisable(){

		RCC_SceneManager.OnControllerChanged -= CheckController;
		RCC_SceneManager.OnVehicleChanged -= CheckController;

	}

}
