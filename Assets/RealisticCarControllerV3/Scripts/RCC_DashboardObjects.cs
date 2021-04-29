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

/// <summary>
/// Receiving inputs from active vehicle on your scene, and feeds visual dashboard needles.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Visual Dashboard Objects")]
public class RCC_DashboardObjects : MonoBehaviour {

	// Getting an Instance of Main Shared RCC Settings.
	#region RCC Settings Instance

	private RCC_Settings RCCSettingsInstance;
	private RCC_Settings RCCSettings {
		get {
			if (RCCSettingsInstance == null) {
				RCCSettingsInstance = RCC_Settings.Instance;
				return RCCSettingsInstance;
			}
			return RCCSettingsInstance;
		}
	}

	#endregion

	private RCC_CarControllerV3 carController;

	[System.Serializable]
	public class RPMDial{

		public GameObject dial;
		public float multiplier = .05f;
		public RotateAround rotateAround = RotateAround.Z;
		private Quaternion dialOrgRotation = Quaternion.identity;
		public Text text;

		public void Init(){

			if(dial)
				dialOrgRotation = dial.transform.localRotation;
			
		}

		public void Update(float value){

			Vector3 targetAxis = Vector3.forward;

			switch (rotateAround) {

			case RotateAround.X:

				targetAxis = Vector3.right;

				break;

			case RotateAround.Y:

				targetAxis = Vector3.up;

				break;

			case RotateAround.Z:

				targetAxis = Vector3.forward;

				break;

			}

			dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis (-multiplier * value, targetAxis);

			if (text)
				text.text = value.ToString ("F0");

		}

	}

	[System.Serializable]
	public class SpeedoMeterDial{

		public GameObject dial;
		public float multiplier = 1f;
		public RotateAround rotateAround = RotateAround.Z;
		private Quaternion dialOrgRotation = Quaternion.identity;
		public Text text;

		public void Init(){

			if(dial)
				dialOrgRotation = dial.transform.localRotation;

		}

		public void Update(float value){

			Vector3 targetAxis = Vector3.forward;

			switch (rotateAround) {

			case RotateAround.X:

				targetAxis = Vector3.right;

				break;

			case RotateAround.Y:

				targetAxis = Vector3.up;

				break;

			case RotateAround.Z:

				targetAxis = Vector3.forward;

				break;

			}

			dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis (-multiplier * value, targetAxis);

			if (text)
				text.text = value.ToString ("F0");

		}

	}

	[System.Serializable]
	public class FuelDial{

		public GameObject dial;
		public float multiplier = .1f;
		public RotateAround rotateAround = RotateAround.Z;
		private Quaternion dialOrgRotation = Quaternion.identity;
		public Text text;

		public void Init(){

			if(dial)
				dialOrgRotation = dial.transform.localRotation;

		}

		public void Update(float value){

			Vector3 targetAxis = Vector3.forward;

			switch (rotateAround) {

			case RotateAround.X:

				targetAxis = Vector3.right;

				break;

			case RotateAround.Y:

				targetAxis = Vector3.up;

				break;

			case RotateAround.Z:

				targetAxis = Vector3.forward;

				break;

			}

			dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis (-multiplier * value, targetAxis);

			if (text)
				text.text = value.ToString ("F0");

		}

	}

	[System.Serializable]
	public class HeatDial{

		public GameObject dial;
		public float multiplier = .1f;
		public RotateAround rotateAround = RotateAround.Z;
		private Quaternion dialOrgRotation = Quaternion.identity;
		public Text text;

		public void Init(){

			if(dial)
				dialOrgRotation = dial.transform.localRotation;

		}

		public void Update(float value){

			Vector3 targetAxis = Vector3.forward;

			switch (rotateAround) {

			case RotateAround.X:

				targetAxis = Vector3.right;

				break;

			case RotateAround.Y:

				targetAxis = Vector3.up;

				break;

			case RotateAround.Z:

				targetAxis = Vector3.forward;

				break;

			}

			dial.transform.localRotation = dialOrgRotation * Quaternion.AngleAxis (-multiplier * value, targetAxis);

			if (text)
				text.text = value.ToString ("F0");

		}

	}

	[System.Serializable]
	public class InteriorLight{

		public Light light;
		public float intensity = 1f;
		public LightRenderMode renderMode = LightRenderMode.Auto;

		public void Init(){

			if(RCC_Settings.Instance.useLightsAsVertexLights)
				renderMode = LightRenderMode.ForceVertex;
			else
				renderMode = LightRenderMode.ForcePixel;

			light.renderMode = renderMode;

		}

		public void Update(bool state){

			if (!light.enabled)
				light.enabled = true;

			light.intensity = state ? intensity : 0f;

		}

	}

	[Space()]
	public RPMDial rPMDial;
	[Space()]
	public SpeedoMeterDial speedDial;
	[Space()]
	public FuelDial fuelDial;
	[Space()]
	public HeatDial heatDial;
	[Space()]
	public InteriorLight[] interiorLights;

	public enum RotateAround{X, Y, Z}

	void Awake () {

		carController = GetComponentInParent<RCC_CarControllerV3> ();

		rPMDial.Init ();
		speedDial.Init ();
		fuelDial.Init ();
		heatDial.Init ();

		for (int i = 0; i < interiorLights.Length; i++)
			interiorLights [i].Init ();

	}

	void Update(){

		if (!carController)
			return;

		Dials ();
		Lights ();

	}
	
	void Dials () {

		if (rPMDial.dial != null)
			rPMDial.Update (carController.engineRPM);

		if (speedDial.dial != null)
			speedDial.Update (carController.speed);

		if (fuelDial.dial != null)
			fuelDial.Update (carController.fuelTank);

		if (heatDial.dial != null)
			heatDial.Update (carController.engineHeat);

	}

	void Lights (){

		for (int i = 0; i < interiorLights.Length; i++)
			interiorLights [i].Update (carController.lowBeamHeadLightsOn);

	}

}
