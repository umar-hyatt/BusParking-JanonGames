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

public class RCC_Useless : MonoBehaviour {

	public Useless useless;
	public enum Useless{MainController, MobileControllers, Behavior, Graphics}

	// Use this for initialization
	void Awake () {

		int type = 0;

		if(useless == Useless.Behavior){

			type = RCC_Settings.Instance.behaviorSelectedIndex;

		}if(useless == Useless.MainController){

			type = RCC_Settings.Instance.controllerSelectedIndex;

		}if(useless == Useless.MobileControllers){

			switch (RCC_Settings.Instance.mobileController) {

			case RCC_Settings.MobileController.TouchScreen:

				type = 0;

				break;

			case RCC_Settings.MobileController.Gyro:

				type = 1;

				break;

			case RCC_Settings.MobileController.SteeringWheel:

				type = 2;

				break;

			case RCC_Settings.MobileController.Joystick:

				type = 3;

				break;

			}

		}if(useless == Useless.Graphics){

			type = QualitySettings.GetQualityLevel ();

		}

		GetComponent<Dropdown>().value = type;
		GetComponent<Dropdown>().RefreshShownValue();
	
	}

}
