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

public class RCC_Telemetry : MonoBehaviour{

	private RCC_CarControllerV3 carController;
	public GameObject mainPanel;
	
	public Text RPM_WheelFL;
	public Text RPM_WheelFR;
	public Text RPM_WheelRL;
	public Text RPM_WheelRR;

	public Text Torque_WheelFL;
	public Text Torque_WheelFR;
	public Text Torque_WheelRL;
	public Text Torque_WheelRR;

	public Text Brake_WheelFL;
	public Text Brake_WheelFR;
	public Text Brake_WheelRL;
	public Text Brake_WheelRR;

	public Text Force_WheelFL;
	public Text Force_WheelFR;
	public Text Force_WheelRL;
	public Text Force_WheelRR;

	public Text Angle_WheelFL;
	public Text Angle_WheelFR;
	public Text Angle_WheelRL;
	public Text Angle_WheelRR;

	public Text Sideways_WheelFL;
	public Text Sideways_WheelFR;
	public Text Sideways_WheelRL;
	public Text Sideways_WheelRR;

	public Text Forward_WheelFL;
	public Text Forward_WheelFR;
	public Text Forward_WheelRL;
	public Text Forward_WheelRR;

	public Text ABS;
	public Text ESP;
	public Text TCS;

	public Text GroundHit_WheelFL;
	public Text GroundHit_WheelFR;
	public Text GroundHit_WheelRL;
	public Text GroundHit_WheelRR;

	public Text speed;
	public Text engineRPM;
	public Text gear;
	public Text finalTorque;
	public Text drivetrain;
	public Text angularVelocity;
	public Text controllable;

	public Text throttle;
	public Text steer;
	public Text brake;
	public Text handbrake;
	public Text clutch;

    // Update is called once per frame
    void Update(){

		mainPanel.SetActive (RCC_Settings.Instance.useTelemetry);

		carController = RCC_SceneManager.Instance.activePlayerVehicle;

		if (!carController)
			return;

		RPM_WheelFL.text = carController.FrontLeftWheelCollider.wheelCollider.rpm.ToString ("F0");
		RPM_WheelFR.text = carController.FrontRightWheelCollider.wheelCollider.rpm.ToString ("F0");
		RPM_WheelRL.text = carController.RearLeftWheelCollider.wheelCollider.rpm.ToString ("F0");
		RPM_WheelRR.text = carController.RearRightWheelCollider.wheelCollider.rpm.ToString ("F0");

		Torque_WheelFL.text = carController.FrontLeftWheelCollider.wheelCollider.motorTorque.ToString ("F0");
		Torque_WheelFR.text = carController.FrontRightWheelCollider.wheelCollider.motorTorque.ToString ("F0");
		Torque_WheelRL.text = carController.RearLeftWheelCollider.wheelCollider.motorTorque.ToString ("F0");
		Torque_WheelRR.text = carController.RearRightWheelCollider.wheelCollider.motorTorque.ToString ("F0");

		Brake_WheelFL.text = carController.FrontLeftWheelCollider.wheelCollider.brakeTorque.ToString ("F0");
		Brake_WheelFR.text = carController.FrontRightWheelCollider.wheelCollider.brakeTorque.ToString ("F0");
		Brake_WheelRL.text = carController.RearLeftWheelCollider.wheelCollider.brakeTorque.ToString ("F0");
		Brake_WheelRR.text = carController.RearRightWheelCollider.wheelCollider.brakeTorque.ToString ("F0");

		Force_WheelFL.text = carController.FrontLeftWheelCollider.oldForce.ToString ("F0");
		Force_WheelFR.text = carController.FrontRightWheelCollider.oldForce.ToString ("F0");
		Force_WheelRL.text = carController.RearLeftWheelCollider.oldForce.ToString ("F0");
		Force_WheelRR.text = carController.RearRightWheelCollider.oldForce.ToString ("F0");

		Angle_WheelFL.text = carController.FrontLeftWheelCollider.wheelCollider.steerAngle.ToString ("F0");
		Angle_WheelFR.text = carController.FrontRightWheelCollider.wheelCollider.steerAngle.ToString ("F0");
		Angle_WheelRL.text = carController.RearLeftWheelCollider.wheelCollider.steerAngle.ToString ("F0");
		Angle_WheelRR.text = carController.RearRightWheelCollider.wheelCollider.steerAngle.ToString ("F0");

		Sideways_WheelFL.text = carController.FrontLeftWheelCollider.wheelHit.sidewaysSlip.ToString ("F");
		Sideways_WheelFR.text = carController.FrontRightWheelCollider.wheelHit.sidewaysSlip.ToString ("F");
		Sideways_WheelRL.text = carController.RearLeftWheelCollider.wheelHit.sidewaysSlip.ToString ("F");
		Sideways_WheelRR.text = carController.RearRightWheelCollider.wheelHit.sidewaysSlip.ToString ("F");

		Forward_WheelFL.text = carController.FrontLeftWheelCollider.wheelHit.forwardSlip.ToString ("F");
		Forward_WheelFR.text = carController.FrontRightWheelCollider.wheelHit.forwardSlip.ToString ("F");
		Forward_WheelRL.text = carController.RearLeftWheelCollider.wheelHit.forwardSlip.ToString ("F");
		Forward_WheelRR.text = carController.RearRightWheelCollider.wheelHit.forwardSlip.ToString ("F");

		ABS.text = carController.ABSAct ? "Engaged" : "Not Engaged";
		ESP.text = carController.ESPAct ? "Engaged" : "Not Engaged";
		TCS.text = carController.TCSAct ? "Engaged" : "Not Engaged";

		GroundHit_WheelFL.text = carController.FrontLeftWheelCollider.isGrounded ? carController.FrontLeftWheelCollider.wheelHit.collider.name : "";
		GroundHit_WheelFR.text = carController.FrontRightWheelCollider.isGrounded ? carController.FrontRightWheelCollider.wheelHit.collider.name : "";
		GroundHit_WheelRL.text = carController.RearLeftWheelCollider.isGrounded ? carController.RearLeftWheelCollider.wheelHit.collider.name : "";
		GroundHit_WheelRR.text = carController.RearRightWheelCollider.isGrounded ? carController.RearRightWheelCollider.wheelHit.collider.name : "";

		speed.text = carController.speed.ToString ("F0");
		engineRPM.text = carController.engineRPM.ToString ("F0");
		gear.text = carController.currentGear.ToString ("F0");

		switch(carController.wheelTypeChoise){

		case RCC_CarControllerV3.WheelType.FWD:

			drivetrain.text = "FWD";
			break;

		case RCC_CarControllerV3.WheelType.RWD:

			drivetrain.text = "RWD";
			break;

		case RCC_CarControllerV3.WheelType.AWD:

			drivetrain.text = "AWD";
			break;

		}

		angularVelocity.text = carController.rigid.angularVelocity.ToString ();
		controllable.text = carController.canControl ? "True" : "False";

		throttle.text = carController.throttleInput.ToString ("F");
		steer.text = carController.steerInput.ToString ("F");
		brake.text = carController.brakeInput.ToString ("F");
		handbrake.text = carController.handbrakeInput.ToString ("F");
		clutch.text = carController.clutchInput.ToString ("F");
        
    }

}
