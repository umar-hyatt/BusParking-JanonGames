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

public class RCC_RepairStation : MonoBehaviour {

	private RCC_CarControllerV3 targetVehicle;

	void OnTriggerStay (Collider col) {

		if (targetVehicle == null) {

			if (col.gameObject.GetComponentInParent<RCC_CarControllerV3> ())
				targetVehicle = col.gameObject.GetComponentInParent<RCC_CarControllerV3> ();

		}

		if (targetVehicle)
			targetVehicle.repairNow = true;

	}

	void OnTriggerExit (Collider col) {

		if (col.gameObject.GetComponentInParent<RCC_CarControllerV3> ())
			targetVehicle = null;

	}

}
