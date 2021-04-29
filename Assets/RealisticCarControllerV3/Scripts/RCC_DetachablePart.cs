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

public class RCC_DetachablePart : MonoBehaviour{

	public Transform COM;		//	Center of mass.
	private Rigidbody rigid;		//	Rigidbody.

	public bool lockAtStart = true;		//	Lock all motions of Configurable Joint at start.
	public float strength = 100f;		//	Strength of the part. 
	
	private bool broken = false;			//	Is this part broken?
	public bool isBreakable = true;		//	Can it break at certain damage?
	public bool isDetachable = true;	//	Can it detach from the vehicle at certain damage?

	public int loosePoint = 35;		//	Part will be broken at this point.
	public int detachPoint = 0;     //	Part will be detached at this point.

	public Vector3 addTorqueAfterLoose = Vector3.zero;		//	Adds angular velocity related to speed after the brake point reached.

	[System.Serializable]
	private class DetachableJoint{

		[HideInInspector]
		public ConfigurableJoint joint;

		internal ConfigurableJointMotion jointMotionAngularX;
		internal ConfigurableJointMotion jointMotionAngularY;
		internal ConfigurableJointMotion jointMotionAngularZ;

		internal ConfigurableJointMotion jointMotionX;
		internal ConfigurableJointMotion jointMotionY;
		internal ConfigurableJointMotion jointMotionZ;

	}

	private DetachableJoint detachableJoint = new DetachableJoint();

	void Start(){

		rigid = GetComponent<Rigidbody> ();     //	Getting Rigidbody of the part.
		ConfigurableJoint joint = gameObject.GetComponent<ConfigurableJoint>();		//	Getting Configurable Joint of the part.

		//	Setting center of mass if selected.
		if (COM)
			rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);

		//	If configurable joint found, set it. Otherwise disable the script.
		if (joint){

			detachableJoint.joint = joint;

		}else{

			Debug.LogWarning ("Configurable Joint not found for " + gameObject.name + "!");
			enabled = false;
			return;

		}

		//	Locks all motions of Configurable Joint at start.
		if (lockAtStart)
			StartCoroutine(LockParts ());

	}

	/// <summary>
	/// Locks the parts.
	/// </summary>
	private IEnumerator LockParts(){

		yield return new WaitForFixedUpdate ();

		//	Getting default settings of the part.
		detachableJoint.jointMotionAngularX = detachableJoint.joint.angularXMotion;
		detachableJoint.jointMotionAngularY = detachableJoint.joint.angularYMotion;
		detachableJoint.jointMotionAngularZ = detachableJoint.joint.angularZMotion;

		detachableJoint.jointMotionX = detachableJoint.joint.xMotion;
		detachableJoint.jointMotionY = detachableJoint.joint.yMotion;
		detachableJoint.jointMotionZ = detachableJoint.joint.zMotion;

		//	Locking the part.
		detachableJoint.joint.angularXMotion = ConfigurableJointMotion.Locked;
		detachableJoint.joint.angularYMotion = ConfigurableJointMotion.Locked;
		detachableJoint.joint.angularZMotion = ConfigurableJointMotion.Locked;

		detachableJoint.joint.xMotion = ConfigurableJointMotion.Locked;
		detachableJoint.joint.yMotion = ConfigurableJointMotion.Locked;
		detachableJoint.joint.zMotion = ConfigurableJointMotion.Locked;

	}

	void Update(){

		// If part is broken, return.
		if (broken)
			return;

		//	If part is weak and loosen, apply angular velocity related to speed.
		if (addTorqueAfterLoose != Vector3.zero && strength <= loosePoint) {

			float speed = transform.InverseTransformDirection (rigid.velocity).z;
			rigid.AddRelativeTorque (new Vector3(addTorqueAfterLoose.x * speed, addTorqueAfterLoose.y * speed, addTorqueAfterLoose.z * speed));

		}

	}

	public void OnCollision (Collision collision){

		// If part is broken, return.
		if (broken)
			return;

		Vector3 colRelVel = collision.relativeVelocity;
		colRelVel *= 1f - Mathf.Abs (Vector3.Dot (transform.up, collision.contacts [0].normal));

		float cos = Mathf.Abs (Vector3.Dot (collision.contacts [0].normal, colRelVel.normalized));

		foreach (ContactPoint contact in collision.contacts){

			Vector3 point = transform.InverseTransformPoint(contact.point);

			if (point.magnitude < 2f) {

				strength -= (2f - (point.magnitude)) * cos * 10f;
				strength = Mathf.Clamp (strength, 0f, Mathf.Infinity);

			}

		}

		DamageParts ();		//	Damage parts.
		
	}

	/// <summary>
	/// Damages the parts.
	/// </summary>
	private void DamageParts(){

		// If part is broken, return.
		if (broken)
			return;

		// Unlocking the parts and set their joint configuration to default.
		if (strength <= detachPoint) {

			if (detachableJoint.joint) {

				broken = true;
				Destroy (detachableJoint.joint);
				transform.SetParent (null);
				Destroy(gameObject, 3f);

			}

		}else	if (strength <= loosePoint) {

			if (detachableJoint.joint) {

				detachableJoint.joint.angularXMotion = detachableJoint.jointMotionAngularX;
				detachableJoint.joint.angularYMotion = detachableJoint.jointMotionAngularY;
				detachableJoint.joint.angularZMotion = detachableJoint.jointMotionAngularZ;

				detachableJoint.joint.xMotion = detachableJoint.jointMotionX;
				detachableJoint.joint.yMotion = detachableJoint.jointMotionY;
				detachableJoint.joint.zMotion = detachableJoint.jointMotionZ;

			}

		}

	}

}
