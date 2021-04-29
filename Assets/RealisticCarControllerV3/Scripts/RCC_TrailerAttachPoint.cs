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

public class RCC_TrailerAttachPoint : MonoBehaviour {

	void OnTriggerEnter(Collider col){

		RCC_TrailerAttachPoint otherAttacher = col.gameObject.GetComponent<RCC_TrailerAttachPoint> ();

		if (!otherAttacher)
			return;

		RCC_CarControllerV3 otherVehicle = otherAttacher.gameObject.GetComponentInParent<RCC_CarControllerV3> ();

		if (!otherVehicle)
			return;

		transform.root.SendMessage ("AttachTrailer", otherVehicle, SendMessageOptions.DontRequireReceiver);

	}

}
