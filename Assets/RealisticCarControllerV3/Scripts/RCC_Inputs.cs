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

[System.Serializable]
public class RCC_Inputs{

	public float throttleInput = 0f;
	public float brakeInput = 0f;
	public float steerInput = 0f;
	public float clutchInput = 0f;
	public float handbrakeInput = 0f;
	public float boostInput = 0f;
	public int gearInput = 0;

	public void SetInput(float _throttleInput, float _brakeInput, float _steerInput, float _clutchInput, float _handbrakeInput, float _boostInput){

		throttleInput = _throttleInput;
		brakeInput = _brakeInput;
		steerInput = _steerInput;
		clutchInput = _clutchInput;
		handbrakeInput = _handbrakeInput;
		boostInput = _boostInput;

	}

	public void SetInput(float _throttleInput, float _brakeInput, float _steerInput, float _clutchInput, float _handbrakeInput){

		throttleInput = _throttleInput;
		brakeInput = _brakeInput;
		steerInput = _steerInput;
		clutchInput = _clutchInput;
		handbrakeInput = _handbrakeInput;

	}

//	public void SetInput(float _throttleInput, float _brakeInput, float _steerInput, float _handbrakeInput, float _boostInput){
//
//		throttleInput = _throttleInput;
//		brakeInput = _brakeInput;
//		steerInput = _steerInput;
//		handbrakeInput = _handbrakeInput;
//		boostInput = _boostInput;
//
//	}

	public void SetInput(float _throttleInput, float _brakeInput, float _steerInput, float _handbrakeInput){

		throttleInput = _throttleInput;
		brakeInput = _brakeInput;
		steerInput = _steerInput;
		handbrakeInput = _handbrakeInput;

	}

	public void SetInput(float _throttleInput, float _brakeInput, float _steerInput){

		throttleInput = _throttleInput;
		brakeInput = _brakeInput;
		steerInput = _steerInput;

	}
    
}
