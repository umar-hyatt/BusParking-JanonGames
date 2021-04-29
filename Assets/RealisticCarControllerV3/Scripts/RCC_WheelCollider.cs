//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------


using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Based on Unity's WheelCollider. Modifies few curves, settings in order to get stable and realistic physics depends on selected behavior in RCC Settings.
/// </summary>
[RequireComponent (typeof(WheelCollider))]
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/RCC Wheel Collider")]
public class RCC_WheelCollider : RCC_Core {
	
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

	// Getting an Instance of Ground Materials.
	#region RCC_GroundMaterials Instance

	private RCC_GroundMaterials RCCGroundMaterialsInstance;
	private RCC_GroundMaterials RCCGroundMaterials {
		get {
			if (RCCGroundMaterialsInstance == null) {
				RCCGroundMaterialsInstance = RCC_GroundMaterials.Instance;
			}
			return RCCGroundMaterialsInstance;
		}
	}

	#endregion

	// WheelCollider.
	private WheelCollider _wheelCollider;
	public WheelCollider wheelCollider{
		get{
			if(_wheelCollider == null)
				_wheelCollider = GetComponent<WheelCollider>();
			return _wheelCollider;
		}
	}

	// Car controller.
	private RCC_CarControllerV3 _carController;
	public RCC_CarControllerV3 carController{
		get{
			if(_carController == null)
				_carController = GetComponentInParent<RCC_CarControllerV3>();
			return _carController;
		}
	}

	// Rigidbody of the vehicle.
	private Rigidbody _rigid;
	public Rigidbody rigid{
		get{
			if(_rigid == null)
				_rigid = carController.gameObject.GetComponent<Rigidbody>();
			return _rigid;
		}
	}

	private List <RCC_WheelCollider> allWheelColliders = new List<RCC_WheelCollider>() ;		// All wheelcolliders attached to this vehicle.
	public Transform wheelModel;		// Wheel model for animating and aligning.

	public WheelHit wheelHit;				//	Wheel Hit data.
	public bool isGrounded = false;		//	Is wheel grounded?

	[Space()]
	public bool canPower = false;		//	Can this wheel power?
	[Range(-1f, 1f)]public float powerMultiplier = 1f;
	public bool canSteer = false;		//	Can this wheel steer?
	[Range(-1f, 1f)]public float steeringMultiplier = 1f;
	public bool canBrake = false;		//	Can this wheel brake?
	[Range(0f, 1f)]public float brakingMultiplier = 1f;
	public bool canHandbrake = false;		//	Can this wheel handbrake?
	[Range(0f, 1f)]public float handbrakeMultiplier = 1f;

	[Space()]
	public float width = .275f;	//	Width.
	public float offset = .05f;		// Offset by X axis.

	internal float wheelRPM2Speed = 0f;     // Wheel RPM to Speed.

	[Range(-5f, 5f)]public float camber = 0f;		// Camber angle.
	[Range(-5f, 5f)]public float caster = 0f;		// Caster angle.
	[Range(-5f, 5f)]public float toe = 0f;          	// Toe angle.

	internal float damagedCamber = 0f;			// Damaged camber angle.
	internal float damagedCaster = 0f;             // Damaged caster angle.
	internal float damagedToe = 0f;                 // Damaged toe angle.

	private RCC_GroundMaterials physicsMaterials{get{return RCCGroundMaterials;}}		// Getting instance of Configurable Ground Materials.
	private RCC_GroundMaterials.GroundMaterialFrictions[] physicsFrictions{get{return RCCGroundMaterials.frictions;}}

	//	Skidmarks
	private RCC_SkidmarksManager skidmarksManager;		// Main Skidmark Managers class.
	private int lastSkidmark = -1;

	//	Slips
	private float wheelSlipAmountForward = 0f;		// Forward slip.
	private float wheelSlipAmountSideways = 0f;	// Sideways slip.
	private float totalSlip = 0f;								// Total amount of forward and sideways slips.

	//	WheelFriction Curves and Stiffness.
	private WheelFrictionCurve forwardFrictionCurve;		//	Forward friction curve.
	private WheelFrictionCurve sidewaysFrictionCurve;	//	Sideways friction curve.

	//	Audio
	private AudioSource audioSource;		// Audiosource for tire skid SFX.
	private AudioClip audioClip;					// Audioclip for tire skid SFX.
	private float audioVolume = 1f;			//	Maximum volume for tire skid SFX.

	private int groundIndex = 0;		// Current ground physic material index.

	// List for all particle systems.
	internal List<ParticleSystem> allWheelParticles = new List<ParticleSystem>();
	internal ParticleSystem.EmissionModule emission;

	//	Tractions used for smooth drifting.
	internal float tractionHelpedSidewaysStiffness = 1f;
	private float minForwardStiffness = .75f;
	private float maxForwardStiffness  = 1f;
	private float minSidewaysStiffness = .75f;
	private float maxSidewaysStiffness = 1f;

	//	Terrain data.
	private TerrainData mTerrainData;
	private int alphamapWidth;
	private int alphamapHeight;

	private float[,,] mSplatmapData;
	private float mNumTextures;

	// Getting bump force.
	public float bumpForce;
	public float oldForce;

	void Start (){

		// Getting all WheelColliders attached to this vehicle (Except this).
		allWheelColliders = carController.GetComponentsInChildren<RCC_WheelCollider>().ToList();

		GetTerrainData ();		//	Getting terrain datas on scene.
		CheckBehavior ();		//	Checks selected behavior in RCC Settings.

		// Are we going to use skidmarks? If we do, get or create SkidmarksManager on scene.
		if (!RCCSettings.dontUseSkidmarks) {

			if (GameObject.FindObjectOfType<RCC_SkidmarksManager> ())
				skidmarksManager = GameObject.FindObjectOfType<RCC_SkidmarksManager> ();
			else
				skidmarksManager = GameObject.Instantiate (RCCSettings.skidmarksManager, Vector3.zero, Quaternion.identity);

		}

		// Increasing WheelCollider mass for avoiding unstable behavior.
		if (RCCSettings.useFixedWheelColliders)
			wheelCollider.mass = rigid.mass / 15f;

		// Creating audiosource for skid SFX.
		audioSource = NewAudioSource(RCCSettings.audioMixer, carController.gameObject, "Skid Sound AudioSource", 5f, 50f, 0f, audioClip, true, true, false);
		audioSource.transform.position = transform.position;

		// Creating all ground particles, and adding them to list.
		if (!RCCSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < RCCGroundMaterials.frictions.Length; i++) {

				GameObject ps = (GameObject)Instantiate (RCCGroundMaterials.frictions [i].groundParticles, transform.position, transform.rotation) as GameObject;
				emission = ps.GetComponent<ParticleSystem> ().emission;
				emission.enabled = false;
				ps.transform.SetParent (transform, false);
				ps.transform.localPosition = Vector3.zero;
				ps.transform.localRotation = Quaternion.identity;
				allWheelParticles.Add (ps.GetComponent<ParticleSystem> ());

			}

		}

		//	Creating pivot position of the wheel at correct position and rotation.
		GameObject newPivot = new GameObject ("Pivot_" + wheelModel.transform.name);
		newPivot.transform.position = RCC_GetBounds.GetBoundsCenter (wheelModel.transform);
		newPivot.transform.rotation = transform.rotation;
		newPivot.transform.SetParent (wheelModel.transform.parent, true);

		//	Settings offsets.
		if (newPivot.transform.localPosition.x > 0)
			wheelModel.transform.position += transform.right * offset;
		else
			wheelModel.transform.position -= transform.right * offset;

		//	Assigning temporary created wheel to actual wheel.
		wheelModel.SetParent (newPivot.transform, true);
		wheelModel = newPivot.transform;

		// Override wheels automatically if enabled.
		if (carController.overrideWheels) {

			// Overriding canPower, canSteer, canBrake, canHandbrake.
			if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider) {

				canSteer = true;
				canBrake = true;
				brakingMultiplier = 1f;

			}

			if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider) {

				canHandbrake = true;
				canBrake = true;
				brakingMultiplier = .5f;

			}

		}

	}

	void OnEnable(){

		// Listening an event when main behavior changed.
		RCC_SceneManager.OnBehaviorChanged += CheckBehavior;

	}

	private void CheckBehavior(){

		// Getting friction curves.
		forwardFrictionCurve = wheelCollider.forwardFriction;
		sidewaysFrictionCurve = wheelCollider.sidewaysFriction;

		//	Getting behavior if selected.
		RCC_Settings.BehaviorType behavior = RCCSettings.selectedBehaviorType;

		//	If there is a selected behavior, override friction curves.
		if (behavior != null) {

			forwardFrictionCurve = SetFrictionCurves (forwardFrictionCurve, behavior.forwardExtremumSlip, behavior.forwardExtremumValue, behavior.forwardAsymptoteSlip, behavior.forwardAsymptoteValue);
			sidewaysFrictionCurve = SetFrictionCurves (sidewaysFrictionCurve, behavior.sidewaysExtremumSlip, behavior.sidewaysExtremumValue, behavior.sidewaysAsymptoteSlip, behavior.sidewaysAsymptoteValue);

		}
			
		// Assigning new frictons.
		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

	}

	void Update(){

		// Return if RCC is disabled.
		if (!carController.enabled)
			return;

		// Setting position and rotation of the wheel model.
		WheelAlign ();

	}
	
	void FixedUpdate (){

		isGrounded = wheelCollider.GetGroundHit (out wheelHit);
		groundIndex = GetGroundMaterialIndex ();

		float circumFerence = 2.0f * 3.14f * wheelCollider.radius; // Finding circumFerence 2 Pi R.
		wheelRPM2Speed = (circumFerence * wheelCollider.rpm)*60; // Finding KMH.
		wheelRPM2Speed = Mathf.Clamp(wheelRPM2Speed / 1000f, 0f, Mathf.Infinity);

		// Setting power state of the wheels depending on drivetrain mode. Only overrides them if overrideWheels is enabled for the vehicle.
		if (carController.overrideWheels) {

			switch (carController.wheelTypeChoise) {

			case RCC_CarControllerV3.WheelType.AWD:
				canPower = true;

				break;

			case RCC_CarControllerV3.WheelType.BIASED:
				canPower = true;

				break;

			case RCC_CarControllerV3.WheelType.FWD:

				if (this == carController.FrontLeftWheelCollider || this == carController.FrontRightWheelCollider)
					canPower = true;
				else
					canPower = false;

				break;

			case RCC_CarControllerV3.WheelType.RWD:

				if (this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider)
					canPower = true;
				else
					canPower = false;

				break;

			}

		}

		Frictions ();
		SkidMarks ();
		Audio ();
		Particles ();

		// Return if RCC is disabled.
		if (!carController.enabled)
			return;

		#region ESP.

		// ESP System. All wheels have individual brakes. In case of loosing control of the vehicle, corresponding wheel will brake for gaining the control again.
		if (carController.ESP && carController.brakeInput < .5f) {

			if (carController.handbrakeInput < .5f) {

				if (carController.underSteering) {

					if (this == carController.FrontLeftWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (-carController.rearSlip, 0f, Mathf.Infinity));

					if (this == carController.FrontRightWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (carController.rearSlip, 0f, Mathf.Infinity));

				}

				if (carController.overSteering) {

					if (this == carController.RearLeftWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (-carController.frontSlip, 0f, Mathf.Infinity));

					if (this == carController.RearRightWheelCollider)
						ApplyBrakeTorque ((carController.brakeTorque * carController.ESPStrength) * Mathf.Clamp (carController.frontSlip, 0f, Mathf.Infinity));

				}

			}

		}

		#endregion

	}

	// Aligning wheel model position and rotation.
	private void WheelAlign (){
		
		// Return if no wheel model selected.
		if(!wheelModel){
			
			Debug.LogError(transform.name + " wheel of the " + carController.transform.name + " is missing wheel model. This wheel is disabled");
			enabled = false;
			return;

		}

		// Locating correct position and rotation for the wheel.
		Vector3 wheelPosition = Vector3.zero;
		Quaternion wheelRotation = Quaternion.identity;
		wheelCollider.GetWorldPose (out wheelPosition, out wheelRotation);

		//	Assigning position and rotation to the wheel model.
		wheelModel.transform.position = wheelPosition;
		wheelModel.transform.rotation = wheelRotation;

		//	Adjusting offset by X axis.
		if (transform.localPosition.x < 0f)
			wheelModel.transform.position += transform.right * offset;
		else
			wheelModel.transform.position -= transform.right * offset;

        // Adjusting camber angle by Z axis.
        if (transform.localPosition.x < 0f)
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, -camber - damagedCamber);
        else
            wheelModel.transform.RotateAround(wheelModel.transform.position, transform.forward, camber + damagedCamber);

		// Adjusting caster angle by X axis.
		if (transform.localPosition.x < 0f)
			wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, -caster - damagedCaster);
		else
			wheelModel.transform.RotateAround(wheelModel.transform.position, transform.right, caster + damagedCaster);

		//transform.rotation = carController.transform.rotation * Quaternion.Euler(caster, toe, camber);

	}

	/// <summary>
	/// Skidmarks.
	/// </summary>
	private void SkidMarks(){

		// Forward, sideways, and total slips.
		if (isGrounded) {
			wheelSlipAmountForward = Mathf.Abs (wheelHit.forwardSlip);
			wheelSlipAmountSideways = Mathf.Abs (wheelHit.sidewaysSlip);
		} else {
			wheelSlipAmountForward = 0f;
			wheelSlipAmountSideways = 0f;
		}	
			
		totalSlip = Mathf.Lerp(totalSlip, ((wheelSlipAmountSideways + wheelSlipAmountForward) / 2f), Time.fixedDeltaTime * 5f);

		// If scene has skidmarks manager...
		if(!RCCSettings.dontUseSkidmarks){

			// If slips are bigger than target value...
			if (totalSlip > physicsFrictions [groundIndex].slip){

				Vector3 skidPoint = wheelHit.point + 2f * (rigid.velocity) * Time.deltaTime;

				if (rigid.velocity.magnitude > 1f)
					lastSkidmark = skidmarksManager.AddSkidMark (skidPoint, wheelHit.normal, totalSlip - physicsFrictions [groundIndex].slip, lastSkidmark, groundIndex, width);
				else
					lastSkidmark = -1;

			}else{
				
				lastSkidmark = -1;

			}

		}

	}

	/// <summary>
	/// Sets forward and sideways frictions.
	/// </summary>
	private void Frictions(){

		// Handbrake input clamped 0f - 1f.
		float hbInput = carController.handbrakeInput;

		if ((this == carController.RearLeftWheelCollider || this == carController.RearRightWheelCollider) && hbInput > .75f)
			hbInput = .5f;
		else
			hbInput = 1;

		// Setting wheel stiffness to ground physic material stiffness.
		forwardFrictionCurve.stiffness = physicsFrictions[groundIndex].forwardStiffness;
		sidewaysFrictionCurve.stiffness = (physicsFrictions[groundIndex].sidewaysStiffness * hbInput * tractionHelpedSidewaysStiffness);

		// If drift mode is selected, apply specific frictions.
		if(RCCSettings.selectedBehaviorType != null && RCCSettings.selectedBehaviorType.applyExternalWheelFrictions)
			Drift();

		// Setting new friction curves to wheels.
		wheelCollider.forwardFriction = forwardFrictionCurve;
		wheelCollider.sidewaysFriction = sidewaysFrictionCurve;

		// Also damp too.
		wheelCollider.wheelDampingRate = physicsFrictions[groundIndex].damp;

		// Set audioclip to ground physic material sound.
		audioClip = physicsFrictions[groundIndex].groundSound;
		audioVolume = physicsFrictions [groundIndex].volume;

	}

	/// <summary>
	/// Particles.
	/// </summary>
	private void Particles(){

		// If wheel slip is bigger than ground physic material slip, enable particles. Otherwise, disable particles.
		if (!RCCSettings.dontUseAnyParticleEffects) {

			for (int i = 0; i < allWheelParticles.Count; i++) {

				if (totalSlip > physicsFrictions [groundIndex].slip) {

					if (i != groundIndex) {

						ParticleSystem.EmissionModule em;

						em = allWheelParticles [i].emission;
						em.enabled = false;

					} else {

						ParticleSystem.EmissionModule em;

						em = allWheelParticles [i].emission;
						em.enabled = true;

					}

				} else {

					ParticleSystem.EmissionModule em;

					em = allWheelParticles [i].emission;
					em.enabled = false;

				}

			}

		}

	}

	/// <summary>
	/// Drift.
	/// </summary>
	private void Drift(){

		Vector3 relativeVelocity = transform.InverseTransformDirection(rigid.velocity);

		float sqrVel = (relativeVelocity.x * relativeVelocity.x) / 10f;
		sqrVel += (Mathf.Abs(wheelHit.forwardSlip * wheelHit.forwardSlip) * 1f);

		// Forward
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .5f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .5f, minForwardStiffness);
		}else{
			forwardFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, 1f, maxForwardStiffness);
			forwardFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), 1.2f, minForwardStiffness);
		}

		// Sideways
		if(wheelCollider == carController.FrontLeftWheelCollider.wheelCollider || wheelCollider == carController.FrontRightWheelCollider.wheelCollider){
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .4f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .4f, minSidewaysStiffness);
		}else{
			sidewaysFrictionCurve.extremumValue = Mathf.Clamp(1f - sqrVel, .375f, maxSidewaysStiffness);
			sidewaysFrictionCurve.asymptoteValue = Mathf.Clamp(.75f - (sqrVel / 2f), .375f, minSidewaysStiffness);
		}

	}

	/// <summary>
	/// Audio.
	/// </summary>
	private void Audio(){

		// If total slip is high enough...
		if(totalSlip > physicsFrictions [groundIndex].slip){

			// Assigning corresponding audio clip.
			if(audioSource.clip != audioClip)
				audioSource.clip = audioClip;

			// Playing it.
			if(!audioSource.isPlaying)
				audioSource.Play();

			// If vehicle is moving, set volume and pitch. Otherwise set them to 0.
			if(rigid.velocity.magnitude > 1f){
				
				audioSource.volume = Mathf.Lerp(0f, audioVolume, totalSlip - 0);
				audioSource.pitch = Mathf.Lerp(1f, .8f, audioSource.volume);

			}else{
				
				audioSource.volume = 0f;

			}
			
		}else{
			
			audioSource.volume = 0f;

			// If volume is minimal and audio is still playing, stop.
			if(audioSource.volume <= .05f && audioSource.isPlaying)
				audioSource.Stop();
			
		}

		// Calculating bump force.
		bumpForce = wheelHit.force - oldForce;

		//	If bump force is high enough, play bump SFX.
		if ((bumpForce) >= 5000f) {

			// Creating and playing audiosource for bump SFX.
			AudioSource bumpSound = NewAudioSource(RCCSettings.audioMixer, carController.gameObject, "Bump Sound AudioSource", 5f, 50f, (bumpForce - 5000f) / 3000f, RCCSettingsInstance.bumpClip, false, true, true);
			bumpSound.pitch = Random.Range (.9f, 1.1f);

		}

		oldForce = wheelHit.force;

	}

	/// <summary>
	/// Returns true if one of the wheel is slipping.
	/// </summary>
	/// <returns><c>true</c>, if skidding was ised, <c>false</c> otherwise.</returns>
	private bool isSkidding(){

		for (int i = 0; i < allWheelColliders.Count; i++) {

			if(allWheelColliders[i].totalSlip > physicsFrictions [groundIndex].slip)
				return true;

		}

		return false;

	}

	/// <summary>
	/// Applies the motor torque.
	/// </summary>
	/// <param name="torque">Torque.</param>
	public void ApplyMotorTorque(float torque){

		//	If TCS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
		if(carController.TCS){

			if(Mathf.Abs(wheelCollider.rpm) >= 100){
				
				if(wheelHit.forwardSlip > physicsFrictions [groundIndex].slip){
					
					carController.TCSAct = true;
					torque -= Mathf.Clamp(torque * (wheelHit.forwardSlip) * carController.TCSStrength, 0f, Mathf.Infinity);

				}else{
					
					carController.TCSAct = false;
					torque += Mathf.Clamp(torque * (wheelHit.forwardSlip) * carController.TCSStrength, -Mathf.Infinity, 0f);

				}

			}else{
				
				carController.TCSAct = false;

			}

		}
			
		if(OverTorque())
			torque = 0;

		if(Mathf.Abs(torque) > 1f)
			wheelCollider.motorTorque = torque;
		else
			wheelCollider.motorTorque = 0f;

	}

	/// <summary>
	/// Applies the steering.
	/// </summary>
	/// <param name="steerInput">Steer input.</param>
	/// <param name="angle">Angle.</param>
	public void ApplySteering(float steerInput, float angle){

		//	Ackerman steering formula.
		if (steerInput > 0f) {

			if (transform.localPosition.x < 0)
				wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan (2.55f / (6 + (1.5f / 2))) * steerInput);
			else
				wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan (2.55f  / (6 - (1.5f / 2))) * steerInput);

		} else if (steerInput < 0f) {

			if (transform.localPosition.x < 0)
				wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan (2.55f / (6 - (1.5f / 2))) * steerInput);
			else
				wheelCollider.steerAngle = (Mathf.Deg2Rad * angle * 2.55f) * (Mathf.Rad2Deg * Mathf.Atan (2.55f  / (6 + (1.5f / 2))) * steerInput);

		} else {

			wheelCollider.steerAngle = 0f;

		}

		if (transform.localPosition.x < 0)
			wheelCollider.steerAngle += toe + damagedToe;
		else
			wheelCollider.steerAngle -= toe + damagedToe;

	}

	/// <summary>
	/// Applies the brake torque.
	/// </summary>
	/// <param name="torque">Torque.</param>
	public void ApplyBrakeTorque(float torque){

		//	If ABS is enabled, checks forward slip. If wheel is losing traction, don't apply torque.
		if(carController.ABS && carController.handbrakeInput <= .1f){

			if((Mathf.Abs(wheelHit.forwardSlip) * Mathf.Clamp01(torque)) >= carController.ABSThreshold){
				
				carController.ABSAct = true;
				torque = 0;

			}else{
				
				carController.ABSAct = false;

			}

		}

		if(Mathf.Abs(torque) > 1f)
			wheelCollider.brakeTorque = torque;
		else
			wheelCollider.brakeTorque = 0f;

	}

	/// <summary>
	/// Checks if overtorque applying.
	/// </summary>
	/// <returns><c>true</c>, if torque was overed, <c>false</c> otherwise.</returns>
	private bool OverTorque(){

		if(carController.speed > carController.maxspeed || !carController.engineRunning)
			return true;

		return false;

	}

	/// <summary>
	/// Gets the terrain data.
	/// </summary>
	private void GetTerrainData(){

		if (!Terrain.activeTerrain)
			return;
		
		mTerrainData = Terrain.activeTerrain.terrainData;
		alphamapWidth = mTerrainData.alphamapWidth;
		alphamapHeight = mTerrainData.alphamapHeight;

		mSplatmapData = mTerrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
		mNumTextures = mSplatmapData.Length / (alphamapWidth * alphamapHeight);

	}

	/// <summary>
	/// Converts to splat map coordinate.
	/// </summary>
	/// <returns>The to splat map coordinate.</returns>
	/// <param name="playerPos">Player position.</param>
	private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos){
		
		Vector3 vecRet = new Vector3();
		Terrain ter = Terrain.activeTerrain;
		Vector3 terPosition = ter.transform.position;
		vecRet.x = ((playerPos.x - terPosition.x) / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
		vecRet.z = ((playerPos.z - terPosition.z) / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;
		return vecRet;

	}

	/// <summary>
	/// Gets the index of the ground material.
	/// </summary>
	/// <returns>The ground material index.</returns>
	private int GetGroundMaterialIndex(){

		// Contacted any physic material in Configurable Ground Materials yet?
		bool contacted = false;

		if (wheelHit.point == Vector3.zero)
			return 0;

		int ret = 0;
		
		for (int i = 0; i < physicsFrictions.Length; i++) {

			if (wheelHit.collider.sharedMaterial == physicsFrictions [i].groundMaterial) {

				contacted = true;
				ret = i;

			}

		}

		// If ground pyhsic material is not one of the ground material in Configurable Ground Materials, check if we are on terrain collider...
		if(!contacted){

			for (int i = 0; i < RCCGroundMaterials.terrainFrictions.Length; i++) {
				
				if (wheelHit.collider.sharedMaterial == RCCGroundMaterials.terrainFrictions [i].groundMaterial) {

					Vector3 playerPos = transform.position;
					Vector3 TerrainCord = ConvertToSplatMapCoordinate(playerPos);
					float comp = 0f;

					for (int k = 0; k < mNumTextures; k++){

						if (comp < mSplatmapData[(int)TerrainCord.z, (int)TerrainCord.x, k])
							ret = k;

					}

					ret = RCCGroundMaterialsInstance.terrainFrictions [i].splatmapIndexes [ret].index;

				}
					
			}

		}

		return ret;

	}

	/// <summary>
	/// Sets a new friction to WheelCollider.
	/// </summary>
	/// <returns>The friction curves.</returns>
	/// <param name="curve">Curve.</param>
	/// <param name="extremumSlip">Extremum slip.</param>
	/// <param name="extremumValue">Extremum value.</param>
	/// <param name="asymptoteSlip">Asymptote slip.</param>
	/// <param name="asymptoteValue">Asymptote value.</param>
	public WheelFrictionCurve SetFrictionCurves(WheelFrictionCurve curve, float extremumSlip, float extremumValue, float asymptoteSlip, float asymptoteValue){

		WheelFrictionCurve newCurve = curve;

		newCurve.extremumSlip = extremumSlip;
		newCurve.extremumValue = extremumValue;
		newCurve.asymptoteSlip = asymptoteSlip;
		newCurve.asymptoteValue = asymptoteValue;

		return newCurve;

	}

	void OnDisable(){

		RCC_SceneManager.OnBehaviorChanged -= CheckBehavior;

	}

	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos(){

		#if UNITY_EDITOR
		if(Application.isPlaying){

			WheelHit hit;
			wheelCollider.GetGroundHit (out hit);

			// Drawing gizmos for wheel forces and slips.
			float extension = (-wheelCollider.transform.InverseTransformPoint(hit.point).y - (wheelCollider.radius * transform.lossyScale.y)) / wheelCollider.suspensionDistance;
			Debug.DrawLine(hit.point, hit.point + transform.up * (hit.force / rigid.mass), extension <= 0.0 ? Color.magenta : Color.white);
			Debug.DrawLine(hit.point, hit.point - transform.forward * hit.forwardSlip * 2f, Color.green);
			Debug.DrawLine(hit.point, hit.point - transform.right * hit.sidewaysSlip * 2f, Color.red);

		}
		#endif

	}

}