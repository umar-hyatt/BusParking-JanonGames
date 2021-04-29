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

/// <summary>
/// Rotates the brake caliper.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Visual Brake Caliper")]
public class RCC_Caliper : MonoBehaviour {

	public RCC_WheelCollider wheelCollider;		//	Actual WheelCollider.
	private GameObject newPivot;						//	Creating new center pivot for correct position.
	private Quaternion defLocalRotation;			//	Default rotation.

	void Start () {

		//	No need to go further if no wheelcollider found.
		if (!wheelCollider){

			Debug.LogError ("WheelCollider is not selected for this caliper named " + transform.name);
			enabled = false;
			return;

		}

		//	Creating new center pivot for correct position.
		newPivot = new GameObject ("Pivot_" + transform.name);
		newPivot.transform.SetParent (wheelCollider.wheelCollider.transform, false);
		transform.SetParent (newPivot.transform, true);

		//	Assigning default rotation.
		defLocalRotation = newPivot.transform.localRotation;
		
	}

	void Update () {

		//	No need to go further if no wheelcollider found.
		if (!wheelCollider.wheelModel || !wheelCollider.wheelCollider)
			return;

		//	Re-positioning camber pivot.
		newPivot.transform.position = new Vector3 (wheelCollider.wheelModel.transform.position.x, wheelCollider.wheelModel.transform.position.y, wheelCollider.wheelModel.transform.position.z);
		//	Re-rotationing camber pivot.
		newPivot.transform.localRotation = defLocalRotation * Quaternion.AngleAxis (wheelCollider.wheelCollider.steerAngle, Vector3.up);
		
	}

}
