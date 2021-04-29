//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// A simple manager script for all demo scenes. It has an array of spawnable player vehicles, public methods, setting new behavior modes, restart, and quit application.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/UI/RCC Demo Manager")]
public class RCC_Demo : MonoBehaviour {

	internal int selectedVehicleIndex = 0;		// An integer index value used for spawning a new vehicle.
	internal int selectedBehaviorIndex = 0;		// An integer index value used for setting behavior mode.

	// An integer index value used for spawning a new vehicle.
	public void SelectVehicle (int index) {

		selectedVehicleIndex = index;
	
	}

	public void Spawn () {

		// Last known position and rotation of last active vehicle.
		Vector3 lastKnownPos = new Vector3();
		Quaternion lastKnownRot = new Quaternion();

		// Checking if there is a player vehicle on the scene.
		if(RCC_SceneManager.Instance.activePlayerVehicle){

			lastKnownPos = RCC_SceneManager.Instance.activePlayerVehicle.transform.position;
			lastKnownRot = RCC_SceneManager.Instance.activePlayerVehicle.transform.rotation;

		}

		// If last known position and rotation is not assigned, camera's position and rotation will be used.
		if(lastKnownPos == Vector3.zero){
			
			if(RCC_SceneManager.Instance.activePlayerCamera){
				
				lastKnownPos = RCC_SceneManager.Instance.activePlayerCamera.transform.position;
				lastKnownRot = RCC_SceneManager.Instance.activePlayerCamera.transform.rotation;

			}

		}

		// We don't need X and Z rotation angle. Just Y.
		lastKnownRot.x = 0f;
		lastKnownRot.z = 0f;

		RCC_CarControllerV3 lastVehicle = RCC_SceneManager.Instance.activePlayerVehicle;

		#if BCG_ENTEREXIT

		BCG_EnterExitVehicle lastEnterExitVehicle;
		bool enterExitVehicleFound = false;

		if (lastVehicle) {

			lastEnterExitVehicle = lastVehicle.GetComponentInChildren<BCG_EnterExitVehicle> ();

			if(lastEnterExitVehicle && lastEnterExitVehicle.driver){

				enterExitVehicleFound = true;
				BCG_EnterExitManager.Instance.waitTime = 10f;
				lastEnterExitVehicle.driver.GetOut();

			}

		}

		#endif

		// If we have controllable vehicle by player on scene, destroy it.
		if(lastVehicle)
			Destroy(lastVehicle.gameObject);

		// Here we are creating our new vehicle.
		RCC.SpawnRCC(RCC_DemoVehicles.Instance.vehicles[selectedVehicleIndex], lastKnownPos, lastKnownRot, true, true, true);
		 
		#if BCG_ENTEREXIT

		if(enterExitVehicleFound){

			lastEnterExitVehicle = null;

			lastEnterExitVehicle = RCC_SceneManager.Instance.activePlayerVehicle.GetComponentInChildren<BCG_EnterExitVehicle> ();

			if(!lastEnterExitVehicle){
				
				lastEnterExitVehicle = RCC_SceneManager.Instance.activePlayerVehicle.gameObject.AddComponent<BCG_EnterExitVehicle> ();

			}

			if(BCG_EnterExitManager.Instance.BCGCharacterPlayer.characterPlayer && lastEnterExitVehicle && lastEnterExitVehicle.driver == null){
				
				BCG_EnterExitManager.Instance.waitTime = 10f;
				BCG_EnterExitManager.Instance.BCGCharacterPlayer.characterPlayer.GetIn(lastEnterExitVehicle);

			}

		}
		
		#endif

	}

	// An integer index value used for setting behavior mode.
	public void SetBehavior(int index){

		selectedBehaviorIndex = index;

	}

	// Here we are setting new selected behavior to corresponding one.
	public void InitBehavior(){

		RCC.SetBehavior(selectedBehaviorIndex);

	}

	//	Sets the main controller type.
	public void SetController(int index){

		RCC.SetController (index);

	}

	// Sets the mobile controller type.
	public void SetMobileController(int index){

		switch(index){

		case 0:
			RCC.SetMobileController (RCC_Settings.MobileController.TouchScreen);
			break;
		case 1:
			RCC.SetMobileController (RCC_Settings.MobileController.Gyro);
			break;
		case 2:
			RCC.SetMobileController (RCC_Settings.MobileController.SteeringWheel);
			break;
		case 3:
			RCC.SetMobileController (RCC_Settings.MobileController.Joystick);
			break;

		}

	}

	/// <summary>
	/// Sets the quality.
	/// </summary>
	/// <param name="index">Index.</param>
	public void SetQuality(int index){

		QualitySettings.SetQualityLevel (index);

	}

	// Simply restarting the current scene.
	public void RestartScene(){

		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);

	}

	// Simply quit application. Not working on Editor.
	public void Quit(){

		Application.Quit();

	}

}
