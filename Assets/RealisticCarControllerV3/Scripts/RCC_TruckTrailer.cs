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
using System.Collections.Generic;

/// <summary>
/// Truck trailer has additional wheelcolliders. This script handles center of mass of the trailer, wheelcolliders, and antiroll.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Truck Trailer")]
[RequireComponent (typeof(Rigidbody))]
public class RCC_TruckTrailer : MonoBehaviour {

	private RCC_CarControllerV3 carController;
	private Rigidbody rigid;
	private ConfigurableJoint joint;

	public Transform COM;
	private bool isSleeping = false;

	[System.Serializable]
	public class TrailerWheel{

		public WheelCollider wheelCollider;
		public Transform wheelModel;

		public void Torque(float torque){

			wheelCollider.motorTorque = torque;

		}

		public void Brake(float torque){

			wheelCollider.brakeTorque = torque;

		}

	}

	//Extra Wheels.
	public TrailerWheel[] trailerWheels;

	public bool attached = false;

	public class JointRestrictions{

		public ConfigurableJointMotion motionX;
		public ConfigurableJointMotion motionY;
		public ConfigurableJointMotion motionZ;

		public ConfigurableJointMotion angularMotionX;
		public ConfigurableJointMotion angularMotionY;
		public ConfigurableJointMotion angularMotionZ;

		public void Get(ConfigurableJoint configurableJoint){

			motionX = configurableJoint.xMotion;
			motionY = configurableJoint.yMotion;
			motionZ = configurableJoint.zMotion;

			angularMotionX = configurableJoint.angularXMotion;
			angularMotionY = configurableJoint.angularYMotion;
			angularMotionZ = configurableJoint.angularZMotion;

		}

		public void Set(ConfigurableJoint configurableJoint){

			configurableJoint.xMotion = motionX;
			configurableJoint.yMotion = motionY;
			configurableJoint.zMotion = motionZ;

			configurableJoint.angularXMotion = angularMotionX;
			configurableJoint.angularYMotion = angularMotionY;
			configurableJoint.angularZMotion = angularMotionZ;

		}

		public void Reset(ConfigurableJoint configurableJoint){

			configurableJoint.xMotion = ConfigurableJointMotion.Free;
			configurableJoint.yMotion = ConfigurableJointMotion.Free;
			configurableJoint.zMotion = ConfigurableJointMotion.Free;

			configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
			configurableJoint.angularZMotion = ConfigurableJointMotion.Free;

		}

	}

	public JointRestrictions jointRestrictions = new JointRestrictions();

	void Start () {

		rigid = GetComponent<Rigidbody>();
		joint = GetComponentInParent<ConfigurableJoint> ();
		jointRestrictions.Get (joint);

		rigid.interpolation = RigidbodyInterpolation.None;
		rigid.interpolation = RigidbodyInterpolation.Interpolate;
		joint.configuredInWorldSpace = true;

		if (joint.connectedBody) {
			
			AttachTrailer (joint.connectedBody.gameObject.GetComponent<RCC_CarControllerV3> ());

		} else {
			
			carController = null;
			joint.connectedBody = null;
			jointRestrictions.Reset (joint);

		}

	}

	void FixedUpdate(){

		attached = joint.connectedBody;
		
		rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);

		if (!carController)
			return;

		for (int i = 0; i < trailerWheels.Length; i++) {
			
			trailerWheels [i].Torque (carController.throttleInput * (attached ? 1f : 0f));
			trailerWheels [i].Brake ((attached ? 0f : 5000f));

		}

	}

	void Update(){

		if(rigid.velocity.magnitude < .01f && Mathf.Abs(rigid.angularVelocity.magnitude) < .01f)
			isSleeping = true;
		else
			isSleeping = false;
		for (int i = 0; i < trailerWheels.Length; i++) {

			trailerWheels [i].Torque (carController.throttleInput * (attached ? 1f : 0f));
			trailerWheels [i].Brake ((attached ? 0f : 5000f));

		}
		WheelAlign ();

	}

	// Aligning wheel model position and rotation.
	public void WheelAlign (){
		
		if (isSleeping)
			return;

		for (int i = 0; i < trailerWheels.Length; i++) {

			// Return if no wheel model selected.
			if(!trailerWheels[i].wheelModel){

				Debug.LogError(transform.name + " wheel of the " + transform.name + " is missing wheel model. This wheel is disabled");
				enabled = false;
				return;

			}

			// Locating correct position and rotation for the wheel.
			Vector3 wheelPosition = Vector3.zero;
			Quaternion wheelRotation = Quaternion.identity;
			trailerWheels[i].wheelCollider.GetWorldPose (out wheelPosition, out wheelRotation);

			//	Assigning position and rotation to the wheel model.
			trailerWheels[i].wheelModel.transform.position = wheelPosition;
			trailerWheels[i].wheelModel.transform.rotation = wheelRotation;

		}

	}

	public void DetachTrailer(){

		carController = null;
		joint.connectedBody = null;
		jointRestrictions.Reset (joint);

		if (RCC_SceneManager.Instance.activePlayerCamera)
			StartCoroutine(RCC_SceneManager.Instance.activePlayerCamera.AutoFocus ());

	}

	public void AttachTrailer(RCC_CarControllerV3 vehicle){

		carController = vehicle;

		joint.connectedBody = vehicle.rigid;
		jointRestrictions.Set (joint);

		vehicle.attachedTrailer = this;

		if (RCC_SceneManager.Instance.activePlayerCamera)
			StartCoroutine(RCC_SceneManager.Instance.activePlayerCamera.AutoFocus (transform, carController.transform));

	}

	void OnTriggerEnter(Collider col){

//		RCC_TrailerAttachPoint attacher = col.gameObject.GetComponent<RCC_TrailerAttachPoint> ();
//
//		if (!attacher)
//			return;
//
//		RCC_CarControllerV3 vehicle = attacher.gameObject.GetComponentInParent<RCC_CarControllerV3> ();
//
//		if (!vehicle || !attacher)
//			return;
//		
//		AttachTrailer (vehicle, attacher);

	}

}
