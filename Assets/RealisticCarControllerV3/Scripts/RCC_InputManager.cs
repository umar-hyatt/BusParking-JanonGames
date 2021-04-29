//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RCC_InputManager : MonoBehaviour{

	private static RCC_Inputs inputs = new RCC_Inputs();

	private enum InputState { None, Pressed, Held, Released };

	public static RCC_Inputs GetInputs(){

		switch (RCC_Settings.Instance.selectedControllerType) {

		case RCC_Settings.ControllerType.Keyboard:

			inputs.throttleInput = Mathf.Clamp01(Input.GetAxis (RCC_Settings.Instance.verticalInput));
			inputs.brakeInput = Mathf.Abs(Mathf.Clamp(Input.GetAxis (RCC_Settings.Instance.verticalInput), -1f, 0f));
			inputs.steerInput = Mathf.Clamp(Input.GetAxis (RCC_Settings.Instance.horizontalInput), -1f, 1f);
			inputs.handbrakeInput = Mathf.Clamp01(Input.GetKey (RCC_Settings.Instance.handbrakeKB) ? 1f : 0f);
			inputs.boostInput = Mathf.Clamp01(Input.GetKey (RCC_Settings.Instance.boostKB) ? 1f : 0f);

			break;

		case RCC_Settings.ControllerType.XBox360One:

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.Xbox_triggerRightInput))
				inputs.throttleInput = Input.GetAxis (RCC_Settings.Instance.Xbox_triggerRightInput);

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.Xbox_triggerLeftInput))
				inputs.brakeInput = Input.GetAxis (RCC_Settings.Instance.Xbox_triggerLeftInput);

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.Xbox_horizontalInput))
				inputs.steerInput = Input.GetAxis (RCC_Settings.Instance.Xbox_horizontalInput);

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.Xbox_handbrakeKB))
				inputs.handbrakeInput = Input.GetButton (RCC_Settings.Instance.Xbox_handbrakeKB) ? 1f : 0f;

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.Xbox_boostKB))
				inputs.boostInput = Input.GetButton(RCC_Settings.Instance.Xbox_boostKB) ? 1f : 0f;

			break;

		case RCC_Settings.ControllerType.PS4:

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.PS4_triggerRightInput))
				inputs.throttleInput = Mathf.Clamp01(Input.GetAxis(RCC_Settings.Instance.PS4_triggerRightInput));

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.PS4_triggerLeftInput))
				inputs.brakeInput = Input.GetAxis(RCC_Settings.Instance.PS4_triggerLeftInput);

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.PS4_horizontalInput))
				inputs.steerInput = Input.GetAxis(RCC_Settings.Instance.PS4_horizontalInput);

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.PS4_handbrakeKB))
				inputs.handbrakeInput = Input.GetButton(RCC_Settings.Instance.PS4_handbrakeKB) ? 1f : 0f;

			if(!string.IsNullOrEmpty(RCC_Settings.Instance.PS4_boostKB))
				inputs.boostInput = Input.GetButton(RCC_Settings.Instance.PS4_boostKB) ? 1f : 0f;

			break;

			case RCC_Settings.ControllerType.Mobile:

			RCC_MobileButtons mobileInput = RCC_MobileButtons.Instance;

			if (mobileInput) {
				
				inputs.throttleInput = RCC_MobileButtons.Instance.inputs.throttleInput;
				inputs.brakeInput = RCC_MobileButtons.Instance.inputs.brakeInput;
				inputs.steerInput = RCC_MobileButtons.Instance.inputs.steerInput;
				inputs.handbrakeInput = RCC_MobileButtons.Instance.inputs.handbrakeInput;
				inputs.boostInput = RCC_MobileButtons.Instance.inputs.boostInput;

			}

			break;

		case RCC_Settings.ControllerType.LogitechSteeringWheel:

			#if BCG_LOGITECH
			RCC_LogitechSteeringWheel log = RCC_LogitechSteeringWheel.Instance;

			if (log) {

				inputs.throttleInput = log.inputs.throttleInput;
				inputs.brakeInput = log.inputs.brakeInput;
				inputs.steerInput = log.inputs.steerInput;
				inputs.clutchInput = log.inputs.clutchInput;
				inputs.handbrakeInput = log.inputs.handbrakeInput;

			}
			#endif

			break;

		case RCC_Settings.ControllerType.Custom:

			// You can use your own inputs with Custom controller type here.

//			inputs.throttleInput = "yourValue";
//			inputs.brakeInput = "yourValue";
//			inputs.steerInput = "yourValue";
//			inputs.boostInput = "yourValue";
//			inputs.clutchInput = "yourValue";
//			inputs.handbrakeInput = "yourValue";

			break;

		}

		return inputs;

	}

	public static bool GetKeyDown(KeyCode keyCode){

		if (Input.GetKeyDown (keyCode))
			return true;

		return false;

	}

	public static bool GetKeyUp(KeyCode keyCode){

		if (Input.GetKeyUp (keyCode))
			return true;

		return false;

	}

	public static bool GetKey(KeyCode keyCode){

		if (Input.GetKey (keyCode))
			return true;

		return false;

	}

	public static bool GetButtonDown(string buttonCode){

		if (Input.GetButtonDown (buttonCode))
			return true;

		return false;

	}

	public static bool GetButtonUp(string buttonCode){

		if (Input.GetButtonUp (buttonCode))
			return true;

		return false;

	}

	public static bool GetButton(string buttonCode){

		if (Input.GetButton (buttonCode))
			return true;

		return false;

	}

}
