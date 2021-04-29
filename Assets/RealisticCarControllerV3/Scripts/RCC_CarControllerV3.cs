//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Main/RCC Realistic Car Controller V3")]
[RequireComponent (typeof(Rigidbody))]
/// <summary>
/// Main vehicle controller that includes Wheels, Steering, Suspensions, Mechanic Configuration, Stability, Lights, Sounds, and Damage.
/// </summary>
public class RCC_CarControllerV3 : RCC_Core {

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

	public bool canControl = true;					// Enables / Disables controlling the vehicle. If enabled, can receive all inputs from InputManager.
	public bool isGrounded = false;				// Is vehicle grounded completely now?

	#region Wheels
	// Wheel models of the vehicle.
	public Transform FrontLeftWheelTransform;
	public Transform FrontRightWheelTransform;
	public Transform RearLeftWheelTransform;
	public Transform RearRightWheelTransform;
	public Transform[] ExtraRearWheelsTransform;		// Extra Wheels. In case of if your vehicle has extra wheels.
	
	// WheelColliders of the vehicle.
	public RCC_WheelCollider FrontLeftWheelCollider;
	public RCC_WheelCollider FrontRightWheelCollider;
	public RCC_WheelCollider RearLeftWheelCollider;
	public RCC_WheelCollider RearRightWheelCollider;
	public RCC_WheelCollider[] ExtraRearWheelsCollider;		// Extra Wheels. In case of if your vehicle has extra wheels.

	public RCC_WheelCollider[] allWheelColliders;		// All Wheel Colliders.

	public bool overrideWheels = true;		//	Overriding wheel settings such as steer, power, brake, handbrake.
	public int poweredWheels = 0;		//	Total count of powered wheels. Used for dividing total power per each wheel.
	public bool applyEngineTorqueToExtraRearWheelColliders = true;	//Applies engine torque to extra rear wheelcolliders.

	[System.Serializable]
	public class ConfigureVehicleSubsteps{

		public float speedThreshold = 10f;
		public int stepsBelowThreshold = 5;
		public int stepsAboveThreshold = 5;

	}

	public ConfigureVehicleSubsteps configureVehicleSubsteps = new ConfigureVehicleSubsteps();
	#endregion

	#region SteeringWheel
	// Steering wheel model.
	public Transform SteeringWheel;													    	// Driver Steering Wheel. In case of if your vehicle has individual steering wheel model in interior.
	private Quaternion orgSteeringWheelRot;												// Original rotation of Steering Wheel.
	public SteeringWheelRotateAround steeringWheelRotateAround;			// Current rotation of Steering Wheel.
	public enum SteeringWheelRotateAround { XAxis, YAxis, ZAxis }				//	Rotation axis of Steering Wheel.
	public float steeringWheelAngleMultiplier = 11f;									// Angle multiplier of Steering Wheel.
	#endregion

	#region Drivetrain Type
	// Drivetrain type of the vehicle.
	public WheelType wheelTypeChoise = WheelType.RWD;
	public enum WheelType{FWD, RWD, AWD, BIASED}
	#endregion

	#region AI
	public bool externalController = false;		// AI Controller.
	#endregion
	 
	#region Configurations
	public Rigidbody rigid;														// Rigidbody.
	public Transform COM;													// Center of mass.
	public float brakeTorque = 2000f;									// Maximum Brake Torque.,
	public float steerAngle = 40f;											// Maximum Steer Angle Of Your Vehicle.
	public float highspeedsteerAngle = 5f;								// Maximum Steer Angle At Highest Speed.
	public float highspeedsteerAngleAtspeed = 200f;				// Highest Speed For Maximum Steer Angle.
	public float antiRollFrontHorizontal = 1000f;						// Anti Roll Horizontal Force For Preventing Flip Overs And Stability.
	public float antiRollRearHorizontal = 1000f;						// Anti Roll Horizontal Force For Preventing Flip Overs And Stability.
	public float antiRollVertical = 0f;										// Anti Roll Vertical Force For Preventing Flip Overs And Stability. I know it doesn't exist, but it can improve gameplay if you have high COM vehicles like monster trucks.
	public float downForce = 25f;		                        		    // Applies downforce related with vehicle speed.
	public bool useCounterSteering = true;								// Applies counter steering when vehicle is drifting. It helps to keep the control fine of the vehicle.
	[Range(0f, 1f)]public float counterSteeringFactor = .5f;		// Counter steering multiplier.
	public float speed = 0f;													// Vehicle speed.
	public float maxspeed = 240f;											// Maximum speed.
	private float resetTime = 0f;											// Used for resetting the vehicle if upside down.
	private float orgSteerAngle = 0f;										// Original steer angle.
	#endregion

	#region Engine
	public AnimationCurve engineTorqueCurve = new AnimationCurve();		//	Engine torque curve based on RPM.
	public bool autoGenerateEngineRPMCurve = true;		// Auto create engine torque curve.
	public float maxEngineTorque = 250f;						// Maximum engine torque at target RPM.
	public float maxEngineTorqueAtRPM = 4500f;			//	Maximum peek of the engine at this RPM.
	public float minEngineRPM = 1000f;							// Minimum engine RPM.
	public float maxEngineRPM = 7000f;							// Maximum engine RPM.
	public float engineRPM = 0f;									// Current engine RPM.
	[Range(.02f, .4f)]public float engineInertia = .15f;		// Engine inertia. Engine reacts faster on lower values.
	public bool useRevLimiter = true;								// Rev limiter above maximum engine RPM. Cuts gas when RPM exceeds maximum engine RPM.
	public bool useExhaustFlame = true;						// Exhaust blows flame when driver cuts gas at certain RPMs.
	public bool runEngineAtAwake{get{return RCCSettings.runEngineAtAwake;}}		    // Engine running at Awake?
	public bool engineRunning = false;																		// Engine running now?


	private float oldEngineTorque = 0f;				// Old engine torque used for recreating the engine curve.
	private float oldMaxTorqueAtRPM = 0f;			// Old max torque used for recreating the engine curve.
	private float oldMinEngineRPM = 0f;				// Old min RPM used for recreating the engine curve.
	private float oldMaxEngineRPM = 0f;			// Old max RPM used for recreating the engine curve.
	#endregion

	#region Fuel
	// Fuel.
	public bool useFuelConsumption = false;		// Enable / Disable Fuel Consumption.
	public float fuelTankCapacity = 62f;				// Fuel Tank Capacity.
	public float fuelTank = 62f;							// Fuel Amount.
	public float fuelConsumptionRate = .1f;		// Fuel Consumption Rate.
	#endregion

	#region Heat
	// Engine heat.
	public bool useEngineHeat = false;							// Enable / Disable engine heat.
	public float engineHeat = 15f;									// Engine heat.
	public float engineCoolingWaterThreshold = 60f;		// Engine coolign water engage point.
	public float engineHeatRate = 1f;								// Engine heat multiplier.
	public float engineCoolRate = 1f;								// Engine cool multiplier.
	#endregion

	#region Gears
	// Gears.
	[System.Serializable]
	public class Gear{

		public float maxRatio;
		public int maxSpeed;
		public int targetSpeedForNextGear;

		public void SetGear(float ratio, int speed, int targetSpeed){

			maxRatio = ratio;
			maxSpeed = speed;
			targetSpeedForNextGear = targetSpeed;

		}

	}

	public Gear[] gears;
	public int totalGears = 6;			//	Total count of gears.
	public int currentGear = 0;		// Current Gear Of The Vehicle.
	public bool NGear = false;		// N Gear.

	public float finalRatio = 3.23f;												//	Final Drive Gear Ratio. 
	[Range(0f, .5f)]public float gearShiftingDelay = .35f;				//	Gear shifting delay with time.
	[Range(.25f, 1)]public float gearShiftingThreshold = .75f;		//	Shifting gears at lower RPMs at higher values.
	[Range(.1f, .9f)]public float clutchInertia = .25f;					//	Adjusting clutch faster at lower values. Higher values for smooth clutch.

	public float gearShiftUpRPM = 5250f;			//	Shifting up when engine RPM is high enough.
	public float gearShiftDownRPM = 3000f;		//	Shifting up when engine RPM is high enough.
	public bool changingGear = false;				// Changing gear currently?

	public int direction = 1;							// Reverse Gear Currently?
	internal bool canGoReverseNow = false;	//	If speed is low enough and player pushes the brake button, enable this bool to go reverse.
	public float launched = 0f;
	public bool autoReverse{get{if(!externalController) return RCCSettings.autoReverse; else return true;}}                            // Enables / Disables auto reversing when player press brake button. Useful for if you are making parking style game.
	public bool automaticGear{get{if(!externalController) return RCCSettings.useAutomaticGear; else return true;}}                // Enables / Disables automatic gear shifting.
	internal bool semiAutomaticGear = false;			// Enables / Disables semi-automatic gear shifting.
	public bool useAutomaticClutch { get { return RCCSettingsInstance.useAutomaticClutch; } }
	#endregion

	#region Audio
	// How many audio sources we will use for simulating engine sounds?. Usually, all modern driving games have around 6 audio sources per vehicle.
	// Low RPM, Medium RPM, and High RPM. And their off versions. 
	public AudioType audioType;
	public enum AudioType{OneSource, TwoSource, ThreeSource, Off}

	// If you don't have their off versions, generate them.
	public bool autoCreateEngineOffSounds = true;

	// AudioSources and AudioClips.
	private AudioSource engineStartSound;
	public AudioClip engineStartClip;
	internal AudioSource engineSoundHigh;
	public AudioClip engineClipHigh;
	private AudioSource engineSoundMed;
	public AudioClip engineClipMed;
	private AudioSource engineSoundLow;
	public AudioClip engineClipLow;
	private AudioSource engineSoundIdle;
	public AudioClip engineClipIdle;
	private AudioSource gearShiftingSound;

	internal AudioSource engineSoundHighOff;
	public AudioClip engineClipHighOff;
	internal AudioSource engineSoundMedOff;
	public AudioClip engineClipMedOff;
	internal AudioSource engineSoundLowOff;
	public AudioClip engineClipLowOff;

	// Shared AudioSources and AudioClips.
	private AudioClip[] gearShiftingClips{get{return RCCSettings.gearShiftingClips;}}
	private AudioSource crashSound;
	private AudioClip[] crashClips{get{return RCCSettings.crashClips;}}
	private AudioSource reversingSound;
	private AudioClip reversingClip{get{return RCCSettings.reversingClip;}}
	private AudioSource windSound;
	private AudioClip windClip{get{return RCCSettings.windClip;}}
	private AudioSource brakeSound;
	private AudioClip brakeClip{get{return RCCSettings.brakeClip;}}
	private AudioSource NOSSound;
	private AudioClip NOSClip{get{return RCCSettings.NOSClip;}}
	private AudioSource turboSound;
	private AudioClip turboClip{get{return RCCSettings.turboClip;}}
	private AudioSource blowSound;
	private AudioClip[] blowClip{get{return RCCSettings.blowoutClip;}}

	// Min / Max sound pitches and volumes.
	[Range(0f, 1f)]public float minEngineSoundPitch = .75f;
	[Range(1f, 2f)]public float maxEngineSoundPitch = 1.75f;
	[Range(0f, 1f)]public float minEngineSoundVolume = .05f;
	[Range(0f, 1f)]public float maxEngineSoundVolume = .85f;
	[Range(0f, 1f)]public float idleEngineSoundVolume = .85f;

	public Vector3 engineSoundPosition = new Vector3(0f, 0f, 1.5f);
	public Vector3 gearSoundPosition = new Vector3(0f, -.5f, .5f);
	public Vector3 turboSoundPosition = new Vector3(0f, 0f, 1.5f);
	public Vector3 exhaustSoundPosition = new Vector3(0f, -.5f, 2f);
	public Vector3 windSoundPosition = new Vector3(0f, 0f, 2f);
	#endregion

	#region Inputs
	// Inputs. All values are clamped 0f - 1f.
	public RCC_Inputs inputs;

	[HideInInspector]public float throttleInput = 0f;
	[HideInInspector]public float brakeInput = 0f;
	[HideInInspector]public float steerInput = 0f;
	[HideInInspector]public float clutchInput = 0f;
	[HideInInspector]public float handbrakeInput = 0f;
	[HideInInspector]public float boostInput = 0f;
	[HideInInspector]public float fuelInput = 0f;
	[HideInInspector]public bool cutGas = false;
	private bool permanentGas = false;
	#endregion

	#region Head Lights
	// Lights.
	public bool lowBeamHeadLightsOn = false;	// Low beam head lights.
	public bool highBeamHeadLightsOn = false;	// High beam head lights.
	#endregion

	#region Indicator Lights
	// For Indicators.
	public IndicatorsOn indicatorsOn;						// Indicator system.
	public enum IndicatorsOn{Off, Right, Left, All}	//	Current indicator mode.
	public float indicatorTimer = 0f;						// Used timer for indicator on / off sequence.
	#endregion

	#region Damage
	// Damage.
	public bool useDamage = true;												// Use Damage.
	public bool useWheelDamage = false;                                      // Use Wheel Damage.
	public float wheelDamageRadius = 1f;										//	Wheel damage radius.
	public float wheelDamageMultiplier = .1f;									//	Wheel damage multiplier.
	private struct originalMeshVerts{public Vector3[] meshVerts;}	// Struct for Original Mesh Verticies positions.
	private originalMeshVerts[] originalMeshData;							// Array for struct above.
	public MeshFilter[] deformableMeshFilters;								// Deformable Meshes.
	public LayerMask damageFilter = -1;										// LayerMask filter for not taking any damage.
	public float randomizeVertices = 1f;											// Randomize Verticies on Collisions for more complex deforms.
	public float damageRadius = .5f;												// Verticies in this radius will be effected on collisions.
	private float minimumVertDistanceForDamagedMesh = .002f;		// Comparing Original Vertex Positions Between Last Vertex Positions To Decide Mesh Is Repaired Or Not.
	public bool repairNow = false;													// Repair Now.
	public bool repaired = true;														// Returns true if vehicle is repaired.

	public float maximumDamage = .5f;				// Maximum Vert Distance For Limiting Damage. 0 Value Will Disable The Limit.
	private float minimumCollisionForce = 5f;		// Minimum collision force.
	public float damageMultiplier = 1f;				// Damage multiplier.
	
	public GameObject contactSparkle{get{return RCCSettings.contactParticles;}}		// Contact Particles for collisions. It must be Particle System.
	public int maximumContactSparkle = 5;															//	Contact Particles will be ready to use for collisions in pool. 
	private List<ParticleSystem> contactSparkeList = new List<ParticleSystem>();	// Array for Contact Particles.
	private GameObject allContactParticles;															// Main particle gameobject for keep the hierarchy clean and organized.
	public RCC_DetachablePart[] detachableParts;
	#endregion

	#region Helpers
	// Used for Angular and Linear Steering Helper.
	private Vector3 localVector;
	private Quaternion rot = Quaternion.identity;
	private float oldRotation;
	public Transform velocityDirection;
	public Transform steeringDirection;
	public float velocityAngle;
	private float angle;
	private float angularVelo;
	#endregion

	#region Driving Assistances
	// Driving Assistances.
	public bool ABS = true;
	public bool TCS = true;
	public bool ESP = true;
	public bool steeringHelper = true;
	public bool tractionHelper = true;
	public bool angularDragHelper = false;

	// Driving Assistance thresholds.
	[Range(.05f, .5f)]public float ABSThreshold = .35f;
	[Range(.05f, 1f)]public float TCSStrength = 1f;
	[Range(.05f, .5f)]public float ESPThreshold = .5f;
	[Range(.05f, 1f)]public float ESPStrength = .25f;
	[Range(0f, 1f)] public float steerHelperLinearVelStrength = .1f;
	[Range(0f, 1f)] public float steerHelperAngularVelStrength = .1f;
	[Range(0f, 1f)] public float tractionHelperStrength = .1f;
	[Range(0f, 1f)] public float angularDragHelperStrength = .1f;

	// Is Driving Assistance is in action now?
	public bool ABSAct = false;
	public bool TCSAct = false;
	public bool ESPAct = false;

	// Used For ESP.
	public float frontSlip = 0f;
	public float rearSlip = 0f;

	// ESP Bools.
	public bool underSteering = false;
	public bool overSteering = false;
	#endregion

	#region Drift
	// Drift Variables.
	internal bool driftingNow = false;		// Currently drifting?
	internal float driftAngle = 0f;			// If we do, what's the drift angle?
	#endregion

	#region Turbo / NOS / Boost
	// Turbo and NOS.
	public float turboBoost = 0f;
	public float NoS = 100f;
	private float NoSConsumption = 25f;
	private float NoSRegenerateTime = 10f;

	public bool useNOS = false;
	public bool useTurbo = false;
	#endregion

	#region Events
	/// <summary>
	/// On RCC player vehicle spawned.
	/// </summary>
	public delegate void onRCCPlayerSpawned(RCC_CarControllerV3 RCC);
	public static event onRCCPlayerSpawned OnRCCPlayerSpawned;

	/// <summary>
	/// On RCC player vehicle destroyed.
	/// </summary>
	public delegate void onRCCPlayerDestroyed(RCC_CarControllerV3 RCC);
	public static event onRCCPlayerDestroyed OnRCCPlayerDestroyed;

	/// <summary>
	/// On RCC player vehicle collision.
	/// </summary>
	public delegate void onRCCPlayerCollision(RCC_CarControllerV3 RCC, Collision collision);
	public static event onRCCPlayerCollision OnRCCPlayerCollision;
    #endregion

    public RCC_TruckTrailer attachedTrailer;

	void Awake (){
		
		// Overriding Fixed TimeStep.
		if(RCCSettings.overrideFixedTimeStep)
			Time.fixedDeltaTime = RCCSettings.fixedTimeStep;

		// Getting Rigidbody and settings.
		rigid = GetComponent<Rigidbody>();
		rigid.maxAngularVelocity = RCCSettings.maxAngularVelocity;

		// Checks the important parameters. Normally, editor script limits them, but your old prefabs may still use out of range.
		gearShiftingThreshold = Mathf.Clamp (gearShiftingThreshold, .25f, 1f);

		if (engineInertia > .4f)
			engineInertia = .15f;
		
		engineInertia = Mathf.Clamp (engineInertia, .02f, .4f);

		// Recreates engine torque based on RPM.
		if(autoGenerateEngineRPMCurve)
			CreateEngineCurve ();

		// You can configurate wheels for best behavior, but Unity doesn't have docs about it.
		allWheelColliders = GetComponentsInChildren<RCC_WheelCollider>();

		GetComponentInChildren<WheelCollider>().ConfigureVehicleSubsteps(configureVehicleSubsteps.speedThreshold, configureVehicleSubsteps.stepsBelowThreshold, configureVehicleSubsteps.stepsAboveThreshold);

		// Assigning wheel models of the wheelcolliders.
		FrontLeftWheelCollider.wheelModel = FrontLeftWheelTransform;
		FrontRightWheelCollider.wheelModel = FrontRightWheelTransform;
		RearLeftWheelCollider.wheelModel = RearLeftWheelTransform;
		RearRightWheelCollider.wheelModel = RearRightWheelTransform;

		// If vehicle has extra rear wheels, assign them too.
		for (int i = 0; i < ExtraRearWheelsCollider.Length; i++) 
			ExtraRearWheelsCollider[i].wheelModel = ExtraRearWheelsTransform[i];

		// Default Steer Angle. Using it for lerping current steer angle between default steer angle and high speed steer angle.
		orgSteerAngle = steerAngle;

		// Collecting all contact particles in same parent gameobject for clean hierarchy.
		allContactParticles = new GameObject("All Contact Particles");
		allContactParticles.transform.SetParent(transform, false);

		// Creating and initializing all audio sources.
		CreateAudios();

		// Initializes damage.
		if(useDamage)
			InitDamage();

		// Checks the current selected behavior in RCC Settings. If any behavior selected, apply changes to the vehicle.
		CheckBehavior ();

		// And lastly, starting the engine.
		if (runEngineAtAwake || externalController) {

			engineRunning = true;
			fuelInput = 1f;

		}

	}

	void OnEnable(){

		changingGear = false;	
		// Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_MobileButtons.cs, RCC_DashboardInputs.cs.
		StartCoroutine (RCCPlayerSpawned());
		// Listening an event when main behavior changed.
		RCC_SceneManager.OnBehaviorChanged += CheckBehavior;

	}

	/// <summary>
	/// Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_MobileButtons.cs, RCC_DashboardInputs.cs.
	/// </summary>
	/// <returns>The player spawned.</returns>
	private IEnumerator RCCPlayerSpawned(){

		yield return new WaitForEndOfFrame ();

		// Firing an event when each RCC car spawned / enabled. This event has been listening by RCC_SceneManager.
		if (!externalController) {

			if (OnRCCPlayerSpawned != null)
				OnRCCPlayerSpawned (this);

		}

	}
		
	/// <summary>
	/// Creates all wheelcolliders. Only editor script calls this when you click "Create WheelColliders" button.
	/// </summary>
	public void CreateWheelColliders (){

		CreateWheelColliders (this);
		
	}

    /// <summary>
    /// Creates all audio sources and assigns corresponding audio clips with proper names and clean hierarchy.
    /// </summary>
    private void CreateAudios(){

        switch (audioType){

            case AudioType.OneSource:

			engineSoundHigh = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);

                if (autoCreateEngineOffSounds){

				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);

                    NewLowPassFilter(engineSoundHighOff, 3000f);

                }else{

				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);

                }

                break;

            case AudioType.TwoSource:

			engineSoundHigh = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
			engineSoundLow = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                if (autoCreateEngineOffSounds){
				
				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
				engineSoundLowOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                    NewLowPassFilter(engineSoundHighOff, 3000f);
                    NewLowPassFilter(engineSoundLowOff, 3000f);

                }else{

				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);
				engineSoundLowOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLowOff, true, true, false);

                }

                break;

            case AudioType.ThreeSource:

			engineSoundHigh = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
			engineSoundMed = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium AudioSource", 5, 50, 0, engineClipMed, true, true, false);
			engineSoundLow = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                if (autoCreateEngineOffSounds){

				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHigh, true, true, false);
				engineSoundMedOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium Off AudioSource", 5, 50, 0, engineClipMed, true, true, false);
				engineSoundLowOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLow, true, true, false);

                    if (engineSoundHighOff)
                        NewLowPassFilter(engineSoundHighOff, 3000f);
                    if (engineSoundMedOff)
                        NewLowPassFilter(engineSoundMedOff, 3000f);
                    if (engineSoundLowOff)
                        NewLowPassFilter(engineSoundLowOff, 3000f);

                }else{

				engineSoundHighOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound High Off AudioSource", 5, 50, 0, engineClipHighOff, true, true, false);
				engineSoundMedOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Medium Off AudioSource", 5, 50, 0, engineClipMedOff, true, true, false);
				engineSoundLowOff = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Low Off AudioSource", 5, 25, 0, engineClipLowOff, true, true, false);

                }

                break;

        }

		engineSoundIdle = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Sound Idle AudioSource", 5, 25, 0, engineClipIdle, true, true, false);
		reversingSound = NewAudioSource(RCCSettings.audioMixer, gameObject, gearSoundPosition, "Reverse Sound AudioSource", 10, 50, 0, reversingClip, true, false, false);
		windSound = NewAudioSource(RCCSettings.audioMixer, gameObject, windSoundPosition, "Wind Sound AudioSource", 1, 10, 0, windClip, true, true, false);
		brakeSound = NewAudioSource(RCCSettings.audioMixer, gameObject, "Brake Sound AudioSource", 1, 10, 0, brakeClip, true, true, false);

        if (useNOS)
			NOSSound = NewAudioSource(RCCSettings.audioMixer, gameObject, exhaustSoundPosition, "NOS Sound AudioSource", 5, 10, .5f, NOSClip, true, false, false);
        if (useNOS || useTurbo)
			blowSound = NewAudioSource(RCCSettings.audioMixer, gameObject, exhaustSoundPosition, "NOS Blow", 1f, 10f, .5f, null, false, false, false);
        if (useTurbo){
			turboSound = NewAudioSource(RCCSettings.audioMixer, gameObject, turboSoundPosition, "Turbo Sound AudioSource", .1f, .5f, 0f, turboClip, true, true, false);
            NewHighPassFilter(turboSound, 10000f, 10);
        }

    }

	/// <summary>
	/// Overrides the behavior.
	/// </summary>
	private void CheckBehavior (){

		if (RCCSettings.selectedBehaviorType == null)
			return;

		// If any behavior is selected in RCC Settings, apply changes.
		SetBehavior(this);

	}

	/// <summary>
	/// Creates the engine curve.
	/// </summary>
	public void CreateEngineCurve(){

		engineTorqueCurve = new AnimationCurve ();
		engineTorqueCurve.AddKey (0f, 0f);																//	First index of the curve.
		engineTorqueCurve.AddKey (maxEngineTorqueAtRPM, maxEngineTorque);		//	Second index of the curve at max.
		engineTorqueCurve.AddKey (maxEngineRPM, maxEngineTorque / 1.5f);			// Last index of the curve at maximum RPM.

		oldEngineTorque = maxEngineTorque;
		oldMaxTorqueAtRPM = maxEngineTorqueAtRPM;
		oldMinEngineRPM = minEngineRPM;
		oldMaxEngineRPM = maxEngineRPM;

	}
		
	/// <summary>
	/// Inits the gears.
	/// </summary>
	public void InitGears (){

		gears = new Gear[totalGears];

		float[] gearRatio = new float[gears.Length];
		int[] maxSpeedForGear = new int[gears.Length];
		int[] targetSpeedForGear = new int[gears.Length];

		if (gears.Length == 1)
			gearRatio = new float[]{1.0f};

		if (gears.Length == 2)
			gearRatio = new float[]{2.0f, 1.0f};

		if (gears.Length == 3)
			gearRatio = new float[]{2.0f, 1.5f, 1.0f};

		if (gears.Length == 4)
			gearRatio = new float[]{2.86f, 1.62f, 1.0f, .72f};

		if (gears.Length == 5)
			gearRatio = new float[] {4.23f, 2.52f, 1.66f, 1.22f, 1.0f,};

		if (gears.Length == 6)
			gearRatio = new float[]{4.35f, 2.5f, 1.66f, 1.23f, 1.0f, .85f};

		if (gears.Length == 7)
			gearRatio = new float[]{4.5f, 2.5f, 1.66f, 1.23f, 1.0f, .9f, .8f};

		if (gears.Length == 8)
			gearRatio = new float[]{4.6f, 2.5f, 1.86f, 1.43f, 1.23f, 1.05f, .9f, .72f};

		for (int i = 0; i < gears.Length; i++) {

			maxSpeedForGear[i] = (int)((maxspeed / gears.Length) * (i+1));
			targetSpeedForGear [i] = (int)(Mathf.Lerp (0, maxspeed * Mathf.Lerp (0f, 1f, gearShiftingThreshold), ((float)(i + 1) / (float)(gears.Length))));

		}

		for (int i = 0; i < gears.Length; i++) {

			gears [i] = new Gear ();
			gears [i].SetGear (gearRatio [i], maxSpeedForGear [i], targetSpeedForGear[i]);

		}

	}

	/// <summary>
	/// Collecting all meshes for damage.
	/// </summary>
	private void InitDamage (){

		if (deformableMeshFilters.Length == 0){

			MeshFilter[] allMeshFilters = GetComponentsInChildren<MeshFilter>();
			List <MeshFilter> properMeshFilters = new List<MeshFilter>();

			foreach(MeshFilter mf in allMeshFilters){

				if (!mf.mesh.isReadable)
					Debug.LogError ("Not deformable mesh detected. Mesh of the " + mf.transform.name + " isReadable is false; Read/Write must be enabled in import settings for this model!");
				else if(!mf.transform.IsChildOf(FrontLeftWheelTransform) && !mf.transform.IsChildOf(FrontRightWheelTransform) && !mf.transform.IsChildOf(RearLeftWheelTransform) && !mf.transform.IsChildOf(RearRightWheelTransform))
					properMeshFilters.Add(mf);

			}

			deformableMeshFilters = properMeshFilters.ToArray();

		}

		LoadOriginalMeshData();

		// Particle System used for collision effects. Creating it at start. We will use this when we collide something.
		if(contactSparkle){

			for(int i = 0; i < maximumContactSparkle; i++){
				GameObject sparks = (GameObject)Instantiate(contactSparkle, transform.position, Quaternion.identity) as GameObject;
				sparks.transform.SetParent(allContactParticles.transform);
				contactSparkeList.Add(sparks.GetComponent<ParticleSystem>());
				ParticleSystem.EmissionModule em = sparks.GetComponent<ParticleSystem>().emission;
				em.enabled = false;
			}

		}

		detachableParts = gameObject.GetComponentsInChildren<RCC_DetachablePart> ();

	}
		
	/// <summary>
	/// Kills or start engine.
	/// </summary>
	public void KillOrStartEngine (){
		
		if(engineRunning)
			KillEngine ();
		else
			StartEngine();

	}

	/// <summary>
	/// Starts the engine.
	/// </summary>
	public void StartEngine (){

		if(!engineRunning)
			StartCoroutine (StartEngineDelayed());

	}

	/// <summary>
	/// Starts the engine.
	/// </summary>
	/// <param name="instantStart">If set to <c>true</c> instant start.</param>
	public void StartEngine (bool instantStart){

		if (instantStart) {
			
			fuelInput = 1f;
			engineRunning = true;

		} else {

			StartCoroutine (StartEngineDelayed());

		}

	}

	/// <summary>
	/// Starts the engine delayed.
	/// </summary>
	/// <returns>The engine delayed.</returns>
	public IEnumerator StartEngineDelayed (){

		if (!engineRunning) {

			engineStartSound = NewAudioSource(RCCSettings.audioMixer, gameObject, engineSoundPosition, "Engine Start AudioSource", 1, 10, 1, engineStartClip, false, true, true);

			if(engineStartSound.isPlaying)
				engineStartSound.Play();
			
			yield return new WaitForSeconds(1f);

			engineRunning = true;
			fuelInput = 1f;

		}

		yield return new WaitForSeconds(1f);

	}

	/// <summary>
	/// Kills the engine.
	/// </summary>
	public void KillEngine (){

		fuelInput = 0f;
		engineRunning = false;

	}

	/// <summary>
	/// Default mesh vertices positions. Used for repairing the vehicle body.
	/// </summary>
	private void LoadOriginalMeshData(){

		originalMeshData = new originalMeshVerts[deformableMeshFilters.Length];

		for (int i = 0; i < deformableMeshFilters.Length; i++)
			originalMeshData[i].meshVerts = deformableMeshFilters[i].mesh.vertices;

	}

	/// <summary>
	/// Moving deformed vertices to their original positions while repairing.
	/// </summary>
	public void Repair(){

		if (!repaired && repairNow){
			
			int k;
			repaired = true;

			for(k = 0; k < deformableMeshFilters.Length; k++){

				if (deformableMeshFilters[k] != null){

					Vector3[] vertices = deformableMeshFilters[k].mesh.vertices;

					if (originalMeshData == null)
						LoadOriginalMeshData();

					for (int i = 0; i < vertices.Length; i++)
					{

						vertices[i] += (originalMeshData[k].meshVerts[i] - vertices[i]) * (Time.deltaTime * 2f);
						if ((originalMeshData[k].meshVerts[i] - vertices[i]).magnitude >= minimumVertDistanceForDamagedMesh)
							repaired = false;

					}

					deformableMeshFilters[k].mesh.vertices = vertices;
					deformableMeshFilters[k].mesh.RecalculateNormals();
					deformableMeshFilters[k].mesh.RecalculateBounds();

				}

			}

            for (int i = 0; i < allWheelColliders.Length; i++){

				allWheelColliders[i].damagedCamber = Mathf.Lerp(allWheelColliders[i].damagedCamber, 0f, Time.deltaTime * 2f);
				allWheelColliders[i].damagedCaster = Mathf.Lerp(allWheelColliders[i].damagedCaster, 0f, Time.deltaTime * 2f);
				allWheelColliders[i].damagedToe = Mathf.Lerp(allWheelColliders[i].damagedToe, 0f, Time.deltaTime * 2f);

				if (Mathf.Abs(allWheelColliders[i].damagedCamber) > .05f || Mathf.Abs(allWheelColliders[i].damagedToe) > .05f || Mathf.Abs(allWheelColliders[i].damagedCaster) > .05f){

					repaired = false;

                }else{

					allWheelColliders[i].damagedCamber = 0f;
					allWheelColliders[i].damagedCaster = 0f;
					allWheelColliders[i].damagedToe = 0f;

				}

			}
			
			if(repaired)
				repairNow = false;
			
		}

	}

	/// <summary>
	/// Actual mesh deformation on collision.	/// </summary>
	/// <param name="mesh">Mesh.</param>
	/// <param name="originalMesh">Original mesh.</param>
	/// <param name="collision">Collision.</param>
	/// <param name="cos">Cos.</param>
	/// <param name="meshTransform">Mesh transform.</param>
	/// <param name="rot">Rot.</param>
	private void DeformMesh(Mesh mesh, Vector3[] originalMesh, Collision collision, float cos, Transform meshTransform, Quaternion rot){
		
		Vector3[] vertices = mesh.vertices;
		
		foreach (ContactPoint contact in collision.contacts){
			
			Vector3 point = meshTransform.InverseTransformPoint(contact.point);
			 
			for (int i = 0; i < vertices.Length; i++){

				if ((point - vertices[i]).magnitude < damageRadius){
					vertices[i] += rot * ((localVector * (damageRadius - (point - vertices[i]).magnitude) / damageRadius) * cos + (new Vector3(Mathf.Sin(vertices[i].y * 1000), Mathf.Sin(vertices[i].z * 1000), Mathf.Sin(vertices[i].x * 100)).normalized * (randomizeVertices / 500f)));
					if (maximumDamage > 0 && ((vertices[i] - originalMesh[i]).magnitude) > maximumDamage){
						vertices[i] = originalMesh[i] + (vertices[i] - originalMesh[i]).normalized * (maximumDamage);
					}
				}
					
			}
			
		}
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		
	}

	/// <summary>
	/// Enabling contact particles on collision.
	/// </summary>
	/// <param name="contactPoint">Contact point.</param>
	private void CollisionParticles(Vector3 contactPoint){
		
		for(int i = 0; i < contactSparkeList.Count; i++){

			if (!contactSparkeList[i].isPlaying){

				contactSparkeList[i].transform.position = contactPoint;
				ParticleSystem.EmissionModule em = contactSparkeList[i].emission;
				em.enabled = true;
				contactSparkeList[i].Play();
				break;

			}

		}
		
	}

	/// <summary>
	/// Other visuals.
	/// </summary>
	private void OtherVisuals(){

		//Driver SteeringWheel Transform.
		if (SteeringWheel) {

			if (orgSteeringWheelRot.eulerAngles == Vector3.zero)
				orgSteeringWheelRot = SteeringWheel.transform.localRotation;
			
			switch (steeringWheelRotateAround) {

			case SteeringWheelRotateAround.XAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.right);
				break;

			case SteeringWheelRotateAround.YAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.up);
				break;

			case SteeringWheelRotateAround.ZAxis:
				SteeringWheel.transform.localRotation = orgSteeringWheelRot * Quaternion.AngleAxis(((steerInput * steerAngle) * -steeringWheelAngleMultiplier), Vector3.forward);
				break;

			}

		}

	}
	
	void Update (){

		Inputs();

		//Reversing Bool.
		//if (!externalController){

		if(brakeInput > .9f  && transform.InverseTransformDirection(rigid.velocity).z < 1f && canGoReverseNow && automaticGear && !semiAutomaticGear && !changingGear && direction != -1)
			StartCoroutine(ChangeGear(-1));
		else if (throttleInput < .1f && transform.InverseTransformDirection(rigid.velocity).z > -1f && direction == -1 && !changingGear && automaticGear && !semiAutomaticGear)
			StartCoroutine(ChangeGear(0));

        //}

        Audio();
		ResetCar();

		if(useDamage)
			Repair();

		OtherVisuals ();

		indicatorTimer += Time.deltaTime;

		if (throttleInput >= .1f)
			launched += throttleInput * Time.deltaTime;
		else
			launched -= Time.deltaTime;
		
		launched = Mathf.Clamp01 (launched);

	}

	private void Inputs() {

		if (canControl) {

			if (!externalController) {

				inputs = RCC_InputManager.GetInputs();

				if (!automaticGear || semiAutomaticGear) {
					if (!changingGear && !cutGas)
						throttleInput = inputs.throttleInput;
					else
						throttleInput = 0f;
				} else {
					if (!changingGear && !cutGas)
						throttleInput = (direction == 1 ? Mathf.Clamp01(inputs.throttleInput) : Mathf.Clamp01(inputs.brakeInput));
					else
						throttleInput = 0f;
				}

				if (!automaticGear || semiAutomaticGear) {
					brakeInput = Mathf.Clamp01(inputs.brakeInput);
				} else {
					if (!cutGas)
						brakeInput = (direction == 1 ? Mathf.Clamp01(inputs.brakeInput) : Mathf.Clamp01(inputs.throttleInput));
					else
						brakeInput = 0f;
				}

				steerInput = inputs.steerInput;
				boostInput = inputs.boostInput;
				handbrakeInput = inputs.handbrakeInput;

				if(!useAutomaticClutch)
					clutchInput = inputs.clutchInput;

                GetExternalInputs();

			}

		} else if (!externalController) {

			throttleInput = 0f;
			brakeInput = 0f;
			steerInput = 0f;
			boostInput = 0f;
			handbrakeInput = 1f;

		}

		if (fuelInput <= 0f)
			throttleInput = 0f;

		if (changingGear || cutGas)
			throttleInput = 0f;

		if (!useNOS || NoS < 5 || throttleInput < .75f)
			boostInput = 0f;

		if (useCounterSteering)
			steerInput += driftAngle * counterSteeringFactor;

		throttleInput = Mathf.Clamp01(throttleInput);
		brakeInput = Mathf.Clamp01(brakeInput);
		steerInput = Mathf.Clamp(steerInput, -1f, 1f);
		boostInput = Mathf.Clamp01(boostInput);
		handbrakeInput = Mathf.Clamp01(handbrakeInput);

	}

	/// <summary>
	/// Inputs this instance.
	/// </summary>
	private void GetExternalInputs(){
		
		switch(RCCSettings.selectedControllerType){

		case RCC_Settings.ControllerType.Keyboard:
			
			if(RCC_InputManager.GetKeyDown(RCCSettings.lowBeamHeadlightsKB))
				lowBeamHeadLightsOn = !lowBeamHeadLightsOn;

			if(RCC_InputManager.GetKeyDown(RCCSettings.highBeamHeadlightsKB))
				highBeamHeadLightsOn = true;
			else if(RCC_InputManager.GetKeyUp(RCCSettings.highBeamHeadlightsKB))
				highBeamHeadLightsOn = false;

			if(RCC_InputManager.GetKeyDown(RCCSettings.startEngineKB))
				KillOrStartEngine();

			if(RCC_InputManager.GetKeyDown(RCCSettings.trailerAttachDetach))
				DetachTrailer();

			if(RCC_InputManager.GetKeyDown(RCCSettings.rightIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.Right)
					indicatorsOn = IndicatorsOn.Right;
				else
					indicatorsOn = IndicatorsOn.Off;
			}

			if(RCC_InputManager.GetKeyDown(RCCSettings.leftIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.Left)
					indicatorsOn = IndicatorsOn.Left;
				else
					indicatorsOn = IndicatorsOn.Off;
			}

			if(RCC_InputManager.GetKeyDown(RCCSettings.hazardIndicatorKB)){
				if(indicatorsOn != IndicatorsOn.All){
					indicatorsOn = IndicatorsOn.Off;
					indicatorsOn = IndicatorsOn.All;
				}else{
					indicatorsOn = IndicatorsOn.Off;
				}
			}

			if (RCC_InputManager.GetKeyDown (RCCSettings.NGear))
				NGear = true;

			if (RCC_InputManager.GetKeyUp (RCCSettings.NGear))
				NGear = false;

			if(!automaticGear){

				if (RCC_InputManager.GetKeyDown (RCCSettings.shiftGearUp))
					GearShiftUp ();

				if(RCC_InputManager.GetKeyDown(RCCSettings.shiftGearDown))
					GearShiftDown();	

			}

			break;

		case RCC_Settings.ControllerType.XBox360One:

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_lowBeamHeadlightsKB)) {

				if (RCC_InputManager.GetButtonDown (RCCSettings.Xbox_lowBeamHeadlightsKB))
					lowBeamHeadLightsOn = !lowBeamHeadLightsOn;

			}

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_highBeamHeadlightsKB)) {

				if (RCC_InputManager.GetButtonDown (RCCSettings.Xbox_highBeamHeadlightsKB))
					highBeamHeadLightsOn = true;
				else if (RCC_InputManager.GetButtonUp (RCCSettings.Xbox_highBeamHeadlightsKB))
					highBeamHeadLightsOn = false;

			}

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_startEngineKB) && RCC_InputManager.GetButtonDown (RCCSettings.Xbox_startEngineKB))
				KillOrStartEngine ();

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_trailerAttachDetach) && RCC_InputManager.GetButtonDown (RCCSettings.Xbox_trailerAttachDetach))
				DetachTrailer ();

			float indicator = 0f;

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_indicatorKB))
				indicator = Input.GetAxis (RCCSettings.Xbox_indicatorKB);

			float indicatorHazard = 0f;

			if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_hazardIndicatorKB))
				Input.GetAxis (RCCSettings.Xbox_hazardIndicatorKB);

			if (indicatorHazard >= .5f) {

				if (indicatorsOn != IndicatorsOn.All)
					indicatorsOn = IndicatorsOn.All;

			}else if (indicator >= .5f) {
				
				if (indicatorsOn != IndicatorsOn.Right)
					indicatorsOn = IndicatorsOn.Right;
				
			} else if (indicator <= -.5f) {
				
				if (indicatorsOn != IndicatorsOn.Left)
					indicatorsOn = IndicatorsOn.Left;

			} else {

				indicatorsOn = IndicatorsOn.Off;

			}

			if (!automaticGear) {

				if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_shiftGearUp) && RCC_InputManager.GetButtonDown (RCCSettings.Xbox_shiftGearUp))
					GearShiftUp ();

				if (!string.IsNullOrEmpty (RCCSettingsInstance.Xbox_shiftGearDown) && RCC_InputManager.GetButtonDown (RCCSettings.Xbox_shiftGearDown))
					GearShiftDown ();	

			}
			
			break;

		case RCC_Settings.ControllerType.PS4:

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_lowBeamHeadlightsKB)) {

				if (RCC_InputManager.GetButtonDown (RCCSettings.PS4_lowBeamHeadlightsKB))
					lowBeamHeadLightsOn = !lowBeamHeadLightsOn;

			}

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_highBeamHeadlightsKB)) {

				if (RCC_InputManager.GetButtonDown (RCCSettings.PS4_highBeamHeadlightsKB))
					highBeamHeadLightsOn = true;
				else if (RCC_InputManager.GetButtonUp (RCCSettings.PS4_highBeamHeadlightsKB))
					highBeamHeadLightsOn = false;

			}

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_startEngineKB) && RCC_InputManager.GetButtonDown (RCCSettings.PS4_startEngineKB))
				KillOrStartEngine ();

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_trailerAttachDetach) && RCC_InputManager.GetButtonDown (RCCSettings.PS4_trailerAttachDetach))
				DetachTrailer ();

			float indicatorPS4 = 0f;

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_indicatorKB))
				indicatorPS4 = Input.GetAxis (RCCSettings.PS4_indicatorKB);
			
			float indicatorHazardPS4 = 0f;

			if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_hazardIndicatorKB))
				indicatorHazardPS4 = Input.GetAxis(RCCSettings.PS4_hazardIndicatorKB);

			if (indicatorHazardPS4 >= .5f) {

				if (indicatorsOn != IndicatorsOn.All)
					indicatorsOn = IndicatorsOn.All;

			} else if (indicatorPS4 >= .5f) {

				if (indicatorsOn != IndicatorsOn.Right)
					indicatorsOn = IndicatorsOn.Right;

			} else if (indicatorPS4 <= -.5f) {

				if (indicatorsOn != IndicatorsOn.Left)
					indicatorsOn = IndicatorsOn.Left;

			} else {

				indicatorsOn = IndicatorsOn.Off;

			}

			if (!automaticGear) {

				if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_shiftGearUp) && RCC_InputManager.GetButtonDown(RCCSettings.PS4_shiftGearUp))
					GearShiftUp();

				if (!string.IsNullOrEmpty (RCCSettingsInstance.PS4_shiftGearDown) && RCC_InputManager.GetButtonDown(RCCSettings.PS4_shiftGearDown))
					GearShiftDown();

			}

			break;

			case RCC_Settings.ControllerType.Mobile:

			// Based on UI Dashboard buttons.

			break;

		case RCC_Settings.ControllerType.LogitechSteeringWheel:

			#if BCG_LOGITECH
			RCC_LogitechSteeringWheel log = RCC_LogitechSteeringWheel.Instance;
			useCounterSteering = false;
			//useAutomaticClutch = false;

			if (log) {

				if(RCC_LogitechSteeringWheel.GetKeyTriggered(0, RCCSettingsInstance.LogiSteeringWheel_lowBeamHeadlightsKB))
					lowBeamHeadLightsOn = !lowBeamHeadLightsOn;

				if(RCC_LogitechSteeringWheel.GetKeyPressed(0, RCCSettings.LogiSteeringWheel_highBeamHeadlightsKB))
					highBeamHeadLightsOn = true;
				else if(RCC_LogitechSteeringWheel.GetKeyReleased(0, RCCSettings.LogiSteeringWheel_highBeamHeadlightsKB))
					highBeamHeadLightsOn = false;

				if(RCC_LogitechSteeringWheel.GetKeyTriggered(0, RCCSettings.LogiSteeringWheel_startEngineKB))
					KillOrStartEngine();

				if(RCC_LogitechSteeringWheel.GetKeyTriggered(0, RCCSettings.LogiSteeringWheel_hazardIndicatorKB)){
					if(indicatorsOn != IndicatorsOn.All){
						indicatorsOn = IndicatorsOn.Off;
						indicatorsOn = IndicatorsOn.All;
					}else{
						indicatorsOn = IndicatorsOn.Off;
					}
				}

				if(!automaticGear){

					if (RCC_LogitechSteeringWheel.GetKeyTriggered (0, RCCSettings.LogiSteeringWheel_shiftGearUp))
						GearShiftUp ();

					if(RCC_LogitechSteeringWheel.GetKeyTriggered(0, RCCSettings.LogiSteeringWheel_shiftGearDown))
						GearShiftDown();	

				}

			}
			#endif

			break;

		}

		if (permanentGas)
			throttleInput = 1f;

	}
	
	void FixedUpdate (){

		if ((autoGenerateEngineRPMCurve) && oldEngineTorque != maxEngineTorque || oldMaxTorqueAtRPM != maxEngineTorqueAtRPM || minEngineRPM != oldMinEngineRPM || maxEngineRPM != oldMaxEngineRPM)
			CreateEngineCurve ();

		oldEngineTorque = maxEngineTorque;
		oldMaxTorqueAtRPM = maxEngineTorqueAtRPM;
		oldMinEngineRPM = minEngineRPM;
		oldMaxEngineRPM = maxEngineRPM;

		if (gears == null || gears.Length == 0) {

			print ("Gear can not be 0! Recreating gears...");
			gears = new Gear[totalGears];
			InitGears ();

		}

		int currentPoweredWheels = 0;

		for (int i = 0; i < allWheelColliders.Length; i++) {

			if (allWheelColliders [i].canPower)
				currentPoweredWheels++;

		}

		poweredWheels = currentPoweredWheels;
		
		Engine();
		EngineSounds ();
		Wheels ();

		if (canControl) {
			
			GearBox ();

			if(useAutomaticClutch)
				Clutch ();

		}

		AntiRollBars();
		CheckGrounded ();
		DriftVariables();
		RevLimiter();
		Turbo();
		NOS();

		if(useFuelConsumption)
			Fuel();

		if (useEngineHeat)
			EngineHeat ();

		if(steeringHelper)
			SteerHelper();
		
		if(tractionHelper)
			TractionHelper();

		if (angularDragHelper)
			AngularDragHelper ();

		if(ESP)
			ESPCheck(FrontLeftWheelCollider.wheelCollider.steerAngle);

		if(RCCSettings.selectedBehaviorType != null && RCCSettings.selectedBehaviorType.applyRelativeTorque){

			// If current selected behavior has apply relative torque enabled, and wheel is grounded, apply it.
			if (isGrounded)
				rigid.AddRelativeTorque (Vector3.up * (((steerInput * throttleInput) * direction)) * Mathf.Lerp(1.5f, .5f, speed / 100f), ForceMode.Acceleration);
			 
		}

		// Setting centre of mass.
		rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);
		// Applying downforce.
		rigid.AddRelativeForce (Vector3.down * (speed * downForce), ForceMode.Force);

	}

	/// <summary>
	/// Engine.
	/// </summary>
	private void Engine (){
		
		//Speed.
		speed = rigid.velocity.magnitude * 3.6f;

		//Steer Limit.
		steerAngle = Mathf.Lerp(orgSteerAngle, highspeedsteerAngle, (speed / highspeedsteerAngleAtspeed));

		float wheelRPM = 0;

		for (int i = 0; i < allWheelColliders.Length; i++) {

			if (allWheelColliders [i].canPower)
				wheelRPM += allWheelColliders [i].wheelCollider.rpm;

		}

		float velocity = 0f;

		engineRPM = Mathf.SmoothDamp(engineRPM, (Mathf.Lerp(minEngineRPM, maxEngineRPM + 500f, (clutchInput * throttleInput)) + ((Mathf.Abs(wheelRPM / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity)) * finalRatio * (gears[currentGear].maxRatio)) * (1f - clutchInput))) * fuelInput, ref velocity , engineInertia);
		engineRPM = Mathf.Clamp (engineRPM, 0f, maxEngineRPM + 500f);
		
		//Auto Reverse Bool.
		if(autoReverse){
			
			canGoReverseNow = true;

		}else{
			
			if(brakeInput < .5f && speed < 5)
				canGoReverseNow = true;
			else if(brakeInput > 0 && transform.InverseTransformDirection(rigid.velocity).z > 1f)
				canGoReverseNow = false;
			
		}

	}

	/// <summary>
	/// Audio.
	/// </summary>
	private void Audio(){

		windSound.volume = Mathf.Lerp (0f, RCCSettings.maxWindSoundVolume, speed / 300f);
		windSound.pitch = UnityEngine.Random.Range(.9f, 1f);
		
		if(direction == 1)
			brakeSound.volume = Mathf.Lerp (0f, RCCSettings.maxBrakeSoundVolume, Mathf.Clamp01((FrontLeftWheelCollider.wheelCollider.brakeTorque + FrontRightWheelCollider.wheelCollider.brakeTorque) / (brakeTorque * 2f)) * Mathf.Lerp(0f, 1f, FrontLeftWheelCollider.wheelCollider.rpm / 50f));
		else
			brakeSound.volume = 0f;

	}

	/// <summary>
	/// ESPs the check.
	/// </summary>
	/// <param name="steering">Steering.</param>
	private void ESPCheck(float steering){

		frontSlip = FrontLeftWheelCollider.wheelHit.sidewaysSlip + FrontRightWheelCollider.wheelHit.sidewaysSlip;

		rearSlip = RearLeftWheelCollider.wheelHit.sidewaysSlip + RearRightWheelCollider.wheelHit.sidewaysSlip;

		if(Mathf.Abs(frontSlip) >= ESPThreshold)
			underSteering = true;
		else
			underSteering = false;

		if(Mathf.Abs(rearSlip) >= ESPThreshold)
			overSteering = true;
		else
			overSteering = false;

		if(overSteering || underSteering)
			ESPAct = true;
		else
			ESPAct = false;
			
	}

	/// <summary>
	/// Engine sounds.
	/// </summary>
	private void EngineSounds(){

		float lowRPM = 0f;
		float medRPM = 0f;
		float highRPM = 0f;

		if(engineRPM < ((maxEngineRPM) / 2f))
			lowRPM = Mathf.Lerp(0f, 1f, engineRPM / ((maxEngineRPM) / 2f));
		else
			lowRPM = Mathf.Lerp(1f, .25f, engineRPM / maxEngineRPM);

		if(engineRPM < ((maxEngineRPM) / 2f))
			medRPM = Mathf.Lerp(-.5f, 1f, engineRPM / ((maxEngineRPM) / 2f));
		else
			medRPM = Mathf.Lerp(1f, .5f, engineRPM / maxEngineRPM);

		highRPM = Mathf.Lerp(-1f, 1f, engineRPM / maxEngineRPM);

		lowRPM = Mathf.Clamp01 (lowRPM) * maxEngineSoundVolume;
		medRPM = Mathf.Clamp01 (medRPM) * maxEngineSoundVolume;
		highRPM = Mathf.Clamp01 (highRPM) * maxEngineSoundVolume;

		float volumeLevel = Mathf.Clamp (throttleInput, 0f, 1f);
		float pitchLevel = Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, engineRPM / maxEngineRPM) * (engineRunning ? 1f : 0f);

		switch (audioType) {

		case RCC_CarControllerV3.AudioType.OneSource:

			engineSoundHigh.volume = volumeLevel * maxEngineSoundVolume;
			engineSoundHigh.pitch = pitchLevel;

			engineSoundHighOff.volume = (1f - volumeLevel) * maxEngineSoundVolume;
			engineSoundHighOff.pitch = pitchLevel;

			if(engineSoundIdle){

				engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
				engineSoundIdle.pitch = pitchLevel;

			}

			if(!engineSoundHigh.isPlaying)
				engineSoundHigh.Play();
			if(!engineSoundIdle.isPlaying)
				engineSoundIdle.Play();

			break;

		case RCC_CarControllerV3.AudioType.TwoSource:
			
			engineSoundHigh.volume = highRPM * volumeLevel;
			engineSoundHigh.pitch = pitchLevel;
			engineSoundLow.volume = lowRPM * volumeLevel;
			engineSoundLow.pitch = pitchLevel;

			engineSoundHighOff.volume = highRPM * (1f - volumeLevel);
			engineSoundHighOff.pitch = pitchLevel;
			engineSoundLowOff.volume = lowRPM * (1f - volumeLevel);
			engineSoundLowOff.pitch = pitchLevel;

			if(engineSoundIdle){

				engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
				engineSoundIdle.pitch = pitchLevel;

			}

			if(!engineSoundLow.isPlaying)
				engineSoundLow.Play();
			if(!engineSoundHigh.isPlaying)
				engineSoundHigh.Play();
			if(!engineSoundIdle.isPlaying)
				engineSoundIdle.Play();

			break;

		case RCC_CarControllerV3.AudioType.ThreeSource:

			engineSoundHigh.volume = highRPM * volumeLevel;
			engineSoundHigh.pitch = pitchLevel;
			engineSoundMed.volume = medRPM * volumeLevel;
			engineSoundMed.pitch = pitchLevel;
			engineSoundLow.volume = lowRPM * volumeLevel;
			engineSoundLow.pitch = pitchLevel;

			engineSoundHighOff.volume = highRPM * (1f - volumeLevel);
			engineSoundHighOff.pitch = pitchLevel;
			engineSoundMedOff.volume = medRPM * (1f - volumeLevel);
			engineSoundMedOff.pitch = pitchLevel;
			engineSoundLowOff.volume = lowRPM * (1f - volumeLevel);
			engineSoundLowOff.pitch = pitchLevel;

			if(engineSoundIdle){

				engineSoundIdle.volume = Mathf.Lerp(engineRunning ? idleEngineSoundVolume : 0f, 0f, engineRPM / maxEngineRPM);
				engineSoundIdle.pitch = pitchLevel;

			}

			if(!engineSoundLow.isPlaying)
				engineSoundLow.Play();
			if(!engineSoundMed.isPlaying)
				engineSoundMed.Play();
			if(!engineSoundHigh.isPlaying)
				engineSoundHigh.Play();
			if(!engineSoundIdle.isPlaying)
				engineSoundIdle.Play();
			
			break;

		}

	}

	private void Wheels(){

		for (int i = 0; i < allWheelColliders.Length; i++) {

			if (allWheelColliders [i].canPower)
				allWheelColliders [i].ApplyMotorTorque ((direction * allWheelColliders[i].powerMultiplier * (1f - clutchInput) * throttleInput * (1f + boostInput) * (engineTorqueCurve.Evaluate(engineRPM) * gears[currentGear].maxRatio * finalRatio)) / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity));

			if (allWheelColliders [i].canSteer)
				allWheelColliders [i].ApplySteering (steerInput * allWheelColliders[i].steeringMultiplier, steerAngle);

			bool appliedBrake = false;

			if (!appliedBrake && handbrakeInput > .5f) {

				appliedBrake = true;

				if (allWheelColliders [i].canHandbrake)
					allWheelColliders [i].ApplyBrakeTorque ((brakeTorque * handbrakeInput) * allWheelColliders [i].handbrakeMultiplier);
				
			}

			if (!appliedBrake && brakeInput >= .05f) {

				appliedBrake = true;

				if (allWheelColliders [i].canBrake)
					allWheelColliders [i].ApplyBrakeTorque ((brakeInput * brakeTorque) * allWheelColliders [i].brakingMultiplier);
				
			}

			if (ESPAct)
				appliedBrake = true;

			if(!appliedBrake)
				allWheelColliders [i].ApplyBrakeTorque (0f);

			//	Checking all wheels. If one of them is not powered, reset.
			if (!allWheelColliders [i].canPower)
				allWheelColliders [i].ApplyMotorTorque (0f);
			if (!allWheelColliders [i].canBrake)
				allWheelColliders [i].ApplyBrakeTorque (0f);
			if (!allWheelColliders [i].canSteer)
				allWheelColliders [i].ApplySteering (0f, 0f);

		}

	}

	/// <summary>
	/// Antiroll bars.
	/// </summary>
	private void AntiRollBars (){

		#region Horizontal
		
		float travelFL = 1f;
		float travelFR = 1f;
		
		bool groundedFL = FrontLeftWheelCollider.isGrounded;
		
		if (groundedFL)
			travelFL = (-FrontLeftWheelCollider.transform.InverseTransformPoint(FrontLeftWheelCollider.wheelHit.point).y - FrontLeftWheelCollider.wheelCollider.radius) / FrontLeftWheelCollider.wheelCollider.suspensionDistance;
		
		bool groundedFR = FrontRightWheelCollider.isGrounded;
		
		if (groundedFR)
			travelFR = (-FrontRightWheelCollider.transform.InverseTransformPoint(FrontRightWheelCollider.wheelHit.point).y - FrontRightWheelCollider.wheelCollider.radius) / FrontRightWheelCollider.wheelCollider.suspensionDistance;
		
		float antiRollForceFrontHorizontal= (travelFL - travelFR) * antiRollFrontHorizontal;
		
		if (groundedFL)
			rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontHorizontal, FrontLeftWheelCollider.transform.position); 
		if (groundedFR)
			rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * antiRollForceFrontHorizontal, FrontRightWheelCollider.transform.position); 

		float travelRL = 1f;
		float travelRR = 1f;
		
		bool groundedRL = RearLeftWheelCollider.isGrounded;
		
		if (groundedRL)
			travelRL = (-RearLeftWheelCollider.transform.InverseTransformPoint(RearLeftWheelCollider.wheelHit.point).y - RearLeftWheelCollider.wheelCollider.radius) / RearLeftWheelCollider.wheelCollider.suspensionDistance;
		
		bool groundedRR = RearRightWheelCollider.isGrounded;
		
		if (groundedRR)
			travelRR = (-RearRightWheelCollider.transform.InverseTransformPoint(RearRightWheelCollider.wheelHit.point).y - RearRightWheelCollider.wheelCollider.radius) / RearRightWheelCollider.wheelCollider.suspensionDistance;
		
		float antiRollForceRearHorizontal= (travelRL - travelRR) * antiRollRearHorizontal;
		
		if (groundedRL)
			rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * -antiRollForceRearHorizontal, RearLeftWheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearHorizontal, RearRightWheelCollider.transform.position);
		
		#endregion

		#region Vertical

		float antiRollForceFrontVertical = (travelFL - travelRL) * antiRollVertical;

		if (groundedFL)
			rigid.AddForceAtPosition(FrontLeftWheelCollider.transform.up * -antiRollForceFrontVertical, FrontLeftWheelCollider.transform.position); 
		if (groundedRL)
			rigid.AddForceAtPosition(RearLeftWheelCollider.transform.up * antiRollForceFrontVertical, RearLeftWheelCollider.transform.position); 

		float antiRollForceRearVertical= (travelFR - travelRR) * antiRollVertical;

		if (groundedFR)
			rigid.AddForceAtPosition(FrontRightWheelCollider.transform.up * -antiRollForceRearVertical, FrontRightWheelCollider.transform.position); 
		if (groundedRR)
			rigid.AddForceAtPosition(RearRightWheelCollider.transform.up * antiRollForceRearVertical, RearRightWheelCollider.transform.position); 

		#endregion

	}

	public void CheckGrounded(){

		bool grounded = true;

		for (int i = 0; i < allWheelColliders.Length; i++) {
			
			if (!allWheelColliders [i].wheelCollider.isGrounded)
				grounded = false;
			
		}

		isGrounded = grounded;

	}

	/// <summary>
	/// Steering helper.
	/// </summary>
	private void SteerHelper(){

		if (!isGrounded)
			return;

		if (!steeringDirection || !velocityDirection) {

			if (!steeringDirection) {

				GameObject steeringDirectionGO = new GameObject ("Steering Direction");
				steeringDirectionGO.transform.SetParent (transform, false);
				steeringDirection = steeringDirectionGO.transform;
				steeringDirectionGO.transform.localPosition = new Vector3 (1f, 2f, 0f);
				steeringDirectionGO.transform.localScale = new Vector3 (.1f, .1f, 3f);

			}

			if (!velocityDirection) {

				GameObject velocityDirectionGO = new GameObject ("Velocity Direction");
				velocityDirectionGO.transform.SetParent (transform, false);
				velocityDirection = velocityDirectionGO.transform;
				velocityDirectionGO.transform.localPosition = new Vector3 (-1f, 2f, 0f);
				velocityDirectionGO.transform.localScale = new Vector3 (.1f, .1f, 3f);

			}

			return;

		}

		for (int i = 0; i < allWheelColliders.Length; i++){

			if (allWheelColliders[i].wheelHit.normal == Vector3.zero)
				return;

		}

		Vector3 v = rigid.angularVelocity;
		velocityAngle = (v.y * Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -1f, 1f)) * Mathf.Rad2Deg;
		velocityDirection.localRotation = Quaternion.Lerp(velocityDirection.localRotation, Quaternion.AngleAxis(Mathf.Clamp(velocityAngle / 3f, -45f, 45f), Vector3.up), Time.fixedDeltaTime * 20f);
		steeringDirection.localRotation = Quaternion.Euler (0f, FrontLeftWheelCollider.wheelCollider.steerAngle, 0f);

		int normalizer = 1;

		if (steeringDirection.localRotation.y > velocityDirection.localRotation.y)
			normalizer = 1;
		else
			normalizer = -1;

		float angle2 = Quaternion.Angle (velocityDirection.localRotation, steeringDirection.localRotation) * (normalizer);

		rigid.AddRelativeTorque (Vector3.up * ((angle2 * (Mathf.Clamp(transform.InverseTransformDirection(rigid.velocity).z, -10f, 10f) / 1000f)) * steerHelperAngularVelStrength), ForceMode.VelocityChange);

		if (Mathf.Abs(oldRotation - transform.eulerAngles.y) < 10f){

			float turnadjust = (transform.eulerAngles.y - oldRotation) * (steerHelperLinearVelStrength / 2f);
			Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
			rigid.velocity = (velRotation * rigid.velocity);

		}

		oldRotation = transform.eulerAngles.y;

	}

	/// <summary>
	/// Traction helper.
	/// </summary>
	private void TractionHelper(){

		if (!isGrounded)
			return;

		Vector3 velocity = rigid.velocity;
		velocity -= transform.up * Vector3.Dot(velocity, transform.up);
		velocity.Normalize();

		angle = -Mathf.Asin(Vector3.Dot(Vector3.Cross(transform.forward, velocity), transform.up));

		angularVelo = rigid.angularVelocity.y;

		if (angle * FrontLeftWheelCollider.wheelCollider.steerAngle < 0) {
			FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01 (tractionHelperStrength * Mathf.Abs (angularVelo)));
		} else {
			FrontLeftWheelCollider.tractionHelpedSidewaysStiffness = 1f;
		}

		if (angle * FrontRightWheelCollider.wheelCollider.steerAngle < 0) {
			FrontRightWheelCollider.tractionHelpedSidewaysStiffness = (1f - Mathf.Clamp01 (tractionHelperStrength * Mathf.Abs (angularVelo)));
		} else {
			FrontRightWheelCollider.tractionHelpedSidewaysStiffness = 1f;
		}

	}

	/// <summary>
	/// Angular drag helper.
	/// </summary>
	private void AngularDragHelper(){

		rigid.angularDrag = Mathf.Lerp (0f, 10f, (speed * angularDragHelperStrength) / 1000f);

	}

	/// <summary>
	/// Clutch.
	/// </summary>
	private void Clutch(){

		float wheelRPM = 0;

		for (int i = 0; i < allWheelColliders.Length; i++) {

			if (allWheelColliders [i].canPower)
				wheelRPM += allWheelColliders [i].wheelCollider.rpm;

		}

		if (currentGear == 0) {

			if (launched >= .25f)
				clutchInput = Mathf.Lerp (clutchInput, (Mathf.Lerp (1f, (Mathf.Lerp (clutchInertia, 0f, ((wheelRPM) / Mathf.Clamp(poweredWheels, 1, Mathf.Infinity)) / gears [0].targetSpeedForNextGear)), Mathf.Abs (throttleInput))), Time.fixedDeltaTime * 5f);
			else
				clutchInput = Mathf.Lerp (clutchInput, 1f / speed, Time.fixedDeltaTime * 5f);
			
		} else {
			
			if (changingGear)
				clutchInput = Mathf.Lerp (clutchInput, 1, Time.fixedDeltaTime * 5f);
			else
				clutchInput = Mathf.Lerp (clutchInput, 0, Time.fixedDeltaTime * 5f);

		} 

		if(cutGas || handbrakeInput >= .1f)
			clutchInput = 1f;

		if (NGear)
			clutchInput = 1f;

		clutchInput = Mathf.Clamp01(clutchInput);

	}

	/// <summary>
	/// Gearbox.
	/// </summary>
	private void GearBox (){

		if(automaticGear){

			if(currentGear < gears.Length - 1 && !changingGear){
				
				if(direction == 1 && speed >= gears[currentGear].targetSpeedForNextGear && engineRPM >= gearShiftUpRPM){
					
					if(!semiAutomaticGear)
						StartCoroutine(ChangeGear(currentGear + 1));
					else if(semiAutomaticGear && direction != -1)
						StartCoroutine(ChangeGear(currentGear + 1));
					
				}

			}
			
			if(currentGear > 0){

				if(!changingGear){

					if(direction != -1 && speed < (gears[currentGear - 1].targetSpeedForNextGear) && engineRPM <= gearShiftDownRPM)
						StartCoroutine(ChangeGear(currentGear - 1));

				}

			}
			
		}

		if(direction == -1){
			
			if(!reversingSound.isPlaying)
				reversingSound.Play();
			
			reversingSound.volume = Mathf.Lerp(0f, 1f, speed / gears[0].maxSpeed);
			reversingSound.pitch = reversingSound.volume;

		}else{
			
			if(reversingSound.isPlaying)
				reversingSound.Stop();
			
			reversingSound.volume = 0f;
			reversingSound.pitch = 0f;

		}
		
	}

	/// <summary>
	/// Changes the gear.
	/// </summary>
	/// <returns>The gear.</returns>
	/// <param name="gear">Gear.</param>
	public IEnumerator ChangeGear(int gear){

		changingGear = true;

		if(RCCSettings.useTelemetry)
			print ("Shifted to: " + (gear).ToString()); 

		if(gearShiftingClips.Length > 0){
			
			gearShiftingSound = NewAudioSource(RCCSettings.audioMixer, gameObject, gearSoundPosition, "Gear Shifting AudioSource", 1f, 5f, RCCSettings.maxGearShiftingSoundVolume, gearShiftingClips[UnityEngine.Random.Range(0, gearShiftingClips.Length)], false, true, true);

			if(!gearShiftingSound.isPlaying)
				gearShiftingSound.Play();
			
		}
		
		yield return new WaitForSeconds(gearShiftingDelay);

		if(gear == -1){
			
			currentGear = 0;

			if(!NGear)
				direction = -1;
			else
				direction = 0;

		}else{
			
			currentGear = gear;

			if(!NGear)
				direction = 1;
			else
				direction = 0;

		}

		changingGear = false;

	}

	/// <summary>
	/// Gears the shift up.
	/// </summary>
	public void GearShiftUp(){

		if(currentGear < gears.Length - 1 && !changingGear){

			if(direction != -1)
				StartCoroutine(ChangeGear(currentGear + 1));
			else
				StartCoroutine(ChangeGear(0));

		}

	}

	/// <summary>
	/// Gears the shift to.
	/// </summary>
	public void GearShiftTo(int gear){

		if (gear < -1 || gear >= gears.Length)
			return;

		if (gear == currentGear)
			return;

		StartCoroutine(ChangeGear(gear));

	}

	/// <summary>
	/// Gears the shift down.
	/// </summary>
	public void GearShiftDown(){

		if(currentGear >= 0)
			StartCoroutine(ChangeGear(currentGear - 1));	

	}

	/// <summary>
	/// Rev limiter.
	/// </summary>
	private void RevLimiter(){

		if((useRevLimiter && engineRPM >= maxEngineRPM))
			cutGas = true;
		else if(engineRPM < (maxEngineRPM * .95f))
			cutGas = false;
		
	}

	/// <summary>
	/// NOS.
	/// </summary>
	private void NOS(){

		if(!useNOS)
			return;

		if(!NOSSound)
			NOSSound = NewAudioSource(RCCSettings.audioMixer, gameObject, exhaustSoundPosition, "NOS Sound AudioSource", 5f, 10f, .5f, NOSClip, true, false, false);

		if(!blowSound)
			blowSound = NewAudioSource(RCCSettings.audioMixer, gameObject, exhaustSoundPosition, "NOS Blow", 1f, 10f, .5f, null, false, false, false);

		if(boostInput >= .8f && throttleInput >= .8f && NoS > 5){
			
			NoS -= NoSConsumption * Time.fixedDeltaTime;
			NoSRegenerateTime = 0f;

			if(!NOSSound.isPlaying)
				NOSSound.Play();
			
		}else{
			
			if(NoS < 100 && NoSRegenerateTime > 3)
				NoS += (NoSConsumption / 1.5f) * Time.fixedDeltaTime;
			
			NoSRegenerateTime += Time.fixedDeltaTime;

			if(NOSSound.isPlaying){
				
				NOSSound.Stop();
				blowSound.clip = blowClip[UnityEngine.Random.Range(0, blowClip.Length)];
				blowSound.Play();

			}

		}

	}

	/// <summary>
	/// Turbo.
	/// </summary>
	private void Turbo(){

		if(!useTurbo)
			return;

		if (!turboSound) {
			
			turboSound = NewAudioSource (RCCSettings.audioMixer, gameObject, turboSoundPosition, "Turbo Sound AudioSource", .1f, .5f, 0, turboClip, true, true, false);
			NewHighPassFilter (turboSound, 10000f, 10);

		}

		turboBoost = Mathf.Lerp(turboBoost, Mathf.Clamp(Mathf.Pow(throttleInput, 10) * 30f + Mathf.Pow(engineRPM / maxEngineRPM, 10) * 30f, 0f, 30f), Time.fixedDeltaTime * 10f);

		if(turboBoost >= 25f){
			
			if(turboBoost < (turboSound.volume * 30f)){
				
				if(!blowSound.isPlaying){
					
					blowSound.clip = RCCSettings.blowoutClip[UnityEngine.Random.Range(0, RCCSettings.blowoutClip.Length)];
					blowSound.Play();

				}

			}

		}

		turboSound.volume = Mathf.Lerp(turboSound.volume, turboBoost / 30f, Time.fixedDeltaTime * 5f);
		turboSound.pitch = Mathf.Lerp(Mathf.Clamp(turboSound.pitch, 2f, 3f), (turboBoost / 30f) * 2f, Time.fixedDeltaTime * 5f);

	}

	/// <summary>
	/// Fuel.
	/// </summary>
	private void Fuel(){

		fuelTank -= ((engineRPM / 10000f) * fuelConsumptionRate) * Time.fixedDeltaTime;
		fuelTank = Mathf.Clamp (fuelTank, 0f, fuelTankCapacity);

	}

	/// <summary>
	/// Engine heat.
	/// </summary>
	private void EngineHeat(){

		engineHeat += ((engineRPM / 10000f) * engineHeatRate) * Time.fixedDeltaTime;

		if (engineHeat > engineCoolingWaterThreshold)
			engineHeat -= engineCoolRate * Time.fixedDeltaTime;

		engineHeat -= (engineCoolRate / 10f) * Time.fixedDeltaTime;

		engineHeat = Mathf.Clamp (engineHeat, 15f, 120f);

	}

	/// <summary>
	/// Drift variables.
	/// </summary>
	private void DriftVariables(){

		if(Mathf.Abs(RearRightWheelCollider.wheelHit.sidewaysSlip) > .25f)
			driftingNow = true;
		else
			driftingNow = false;
		
		if(speed > 10f)
			driftAngle = RearRightWheelCollider.wheelHit.sidewaysSlip * .75f;
		else
			driftAngle = 0f;
		
	}

	/// <summary>
	/// Resets the car.
	/// </summary>
	private void ResetCar (){
		
		if(speed < 5 && !rigid.isKinematic){

			if (!RCCSettings.autoReset)
				return; 
			
			if(transform.eulerAngles.z < 300 && transform.eulerAngles.z > 60){
				resetTime += Time.deltaTime;
				if(resetTime > 3){
					transform.rotation = Quaternion.Euler (0f, transform.eulerAngles.y, 0f);
					transform.position = new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
					resetTime = 0f;
				}
			}
			
		}
		
	}

	/// <summary>
	/// Raises the collision enter event.
	/// </summary>
	/// <param name="collision">Collision.</param>
	void OnCollisionEnter (Collision collision){

		CollisionParticles (collision.contacts [0].point);
		
		if (collision.contacts.Length < 1 || collision.relativeVelocity.magnitude < minimumCollisionForce)
			return;

		if(OnRCCPlayerCollision != null && this == RCC_SceneManager.Instance.activePlayerVehicle)
			OnRCCPlayerCollision (this, collision);

		if (!useDamage)
			return;

		if (((1 << collision.gameObject.layer) & damageFilter) != 0) {
			
			CollisionParticles (collision.contacts [0].point);
		
			Vector3 colRelVel = collision.relativeVelocity;
			colRelVel *= 1f - Mathf.Abs (Vector3.Dot (transform.up, collision.contacts [0].normal));
		
			float cos = Mathf.Abs (Vector3.Dot (collision.contacts [0].normal, colRelVel.normalized));

			if (colRelVel.magnitude * cos >= minimumCollisionForce) {
			
				repaired = false;
				localVector = transform.InverseTransformDirection (colRelVel) * (damageMultiplier / 50f);

				if (originalMeshData == null)
					LoadOriginalMeshData ();

				for (int i = 0; i < deformableMeshFilters.Length; i++){

					if(deformableMeshFilters[i] != null)
						DeformMesh(deformableMeshFilters[i].mesh, originalMeshData[i].meshVerts, collision, cos, deformableMeshFilters[i].transform, rot);

				}

				if (useWheelDamage){

					for (int i = 0; i < allWheelColliders.Length; i++){

						Vector3 point = allWheelColliders[i].transform.InverseTransformPoint(collision.contacts[0].point);

						if (point.magnitude <= wheelDamageRadius){

							allWheelColliders[i].damagedToe += (UnityEngine.Random.Range(-(colRelVel.magnitude * cos), (colRelVel.magnitude * cos))) * wheelDamageMultiplier;
							allWheelColliders[i].damagedCamber += (UnityEngine.Random.Range(-(colRelVel.magnitude * cos), (colRelVel.magnitude * cos))) * wheelDamageMultiplier;
							allWheelColliders[i].damagedCaster += (UnityEngine.Random.Range(-(colRelVel.magnitude * cos), (colRelVel.magnitude * cos))) * wheelDamageMultiplier;

						}
                    }

				}

				if (detachableParts != null && detachableParts.Length >= 1) {

					for (int i = 0; i < detachableParts.Length; i++)
						detachableParts [i].OnCollision (collision);

				}

				if (crashClips.Length > 0){

					if (collision.contacts[0].thisCollider.gameObject.transform != transform.parent){

						crashSound = NewAudioSource(RCCSettings.audioMixer, gameObject, "Crash Sound AudioSource", 5, 20, RCCSettings.maxCrashSoundVolume, crashClips[UnityEngine.Random.Range(0, crashClips.Length)], false, true, true);

						if(!crashSound.isPlaying)
							crashSound.Play();

					}

				}
			
			}

		}

	}

	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos(){
#if UNITY_EDITOR
		if(Application.isPlaying){

			WheelHit hit;

			for(int i = 0; i < allWheelColliders.Length; i++){

				allWheelColliders[i].wheelCollider.GetGroundHit(out hit);

				Matrix4x4 temp = Gizmos.matrix;
				Gizmos.matrix = Matrix4x4.TRS(allWheelColliders[i].transform.position, Quaternion.AngleAxis(-90, Vector3.right), Vector3.one);
				Gizmos.color = new Color((hit.force / rigid.mass) / 5f, (-hit.force / rigid.mass) / 5f, 0f);
				Gizmos.DrawFrustum(Vector3.zero, 2f, hit.force / rigid.mass, .1f, 1f);
				Gizmos.matrix = temp;

			}

		}
#endif
	}
		
	/// <summary>
	/// Previews the smoke particle.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void PreviewSmokeParticle(bool state){

		canControl = state;
		permanentGas = state;
		rigid.isKinematic = state;

	}

	/// <summary>
	/// Detachs the trailer.
	/// </summary>
	public void DetachTrailer(){

		if (!attachedTrailer)
			return;

		attachedTrailer.DetachTrailer ();

	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	void OnDestroy(){

		if (OnRCCPlayerDestroyed != null)
			OnRCCPlayerDestroyed (this);

		if(canControl){
			
			if(gameObject.GetComponentInChildren<RCC_Camera>())
				gameObject.GetComponentInChildren<RCC_Camera>().transform.SetParent(null);
			
		}

	}

	/// <summary>
	/// Sets the can control.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void SetCanControl(bool state){

		canControl = state;

	}

	/// <summary>
	/// Sets the engine state.
	/// </summary>
	/// <param name="state">If set to <c>true</c> state.</param>
	public void SetEngine(bool state){

		if (state)
			StartEngine ();
		else
			KillEngine ();

	}

	void OnDisable(){

		RCC_SceneManager.OnBehaviorChanged -= CheckBehavior;

	}
	
} 
