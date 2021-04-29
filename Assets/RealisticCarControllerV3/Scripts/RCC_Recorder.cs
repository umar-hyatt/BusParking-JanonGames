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
/// Record / Replay system. Saves player's input on record, and replays it when on playback.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Misc/RCC Recorder")]
public class RCC_Recorder : MonoBehaviour {

	[System.Serializable]
	public class Recorded{

		public string recordName = "New Record";

		[HideInInspector]public PlayerInput[] inputs;
		[HideInInspector]public PlayerTransform[] transforms;
		[HideInInspector]public PlayerRigidBody[] rigids;

		public Recorded (PlayerInput[] _inputs, PlayerTransform[] _transforms, PlayerRigidBody[] _rigids, string _recordName){

			inputs = _inputs;
			transforms = _transforms;
			rigids = _rigids;
			recordName = _recordName;

		}

	}

	public Recorded recorded;

	public RCC_CarControllerV3 carController;

	public List <PlayerInput> Inputs;
	public List <PlayerTransform> Transforms;
	public List <PlayerRigidBody> Rigidbodies;

	[System.Serializable]
	public class PlayerInput{

		public float throttleInput = 0f;
		public float brakeInput = 0f;
		public float steerInput = 0f;
		public float handbrakeInput = 0f;
		public float clutchInput = 0f;
		public float boostInput = 0f;
		public float fuelInput = 0f;
		public int direction = 1;
		public bool canGoReverse = false;
		public int currentGear = 0;
		public bool changingGear = false;

		public RCC_CarControllerV3.IndicatorsOn indicatorsOn = RCC_CarControllerV3.IndicatorsOn.Off;
		public bool lowBeamHeadLightsOn = false;
		public bool highBeamHeadLightsOn = false;

		public PlayerInput(float _gasInput, float _brakeInput, float _steerInput, float _handbrakeInput, float _clutchInput, float _boostInput, float _fuelInput, int _direction, bool _canGoReverse, int _currentGear, bool _changingGear, RCC_CarControllerV3.IndicatorsOn _indicatorsOn, bool _lowBeamHeadLightsOn, bool _highBeamHeadLightsOn){

			throttleInput = _gasInput;
			brakeInput = _brakeInput;
			steerInput = _steerInput;
			handbrakeInput = _handbrakeInput;
			clutchInput = _clutchInput;
			boostInput = _boostInput;
			fuelInput = _fuelInput;
			direction = _direction;
			canGoReverse = _canGoReverse;
			currentGear = _currentGear;
			changingGear = _changingGear;

			indicatorsOn = _indicatorsOn;
			lowBeamHeadLightsOn = _lowBeamHeadLightsOn;
			highBeamHeadLightsOn = _highBeamHeadLightsOn;

		}

	}

	[System.Serializable]
	public class PlayerTransform{

		public Vector3 position;
		public Quaternion rotation;

		public PlayerTransform(Vector3 _pos, Quaternion _rot){

			position = _pos;
			rotation = _rot;

		}

	}

	[System.Serializable]
	public class PlayerRigidBody{

		public Vector3 velocity;
		public Vector3 angularVelocity;

		public PlayerRigidBody(Vector3 _vel, Vector3 _angVel){

			velocity = _vel;
			angularVelocity = _angVel;

		}

	}

	public enum Mode{Neutral, Play, Record}
	public Mode mode;

    private void Awake() {

		Inputs = new List<PlayerInput>();
		Transforms = new List<PlayerTransform>();
		Rigidbodies = new List<PlayerRigidBody>();

	}

    public void Record(){

		if (mode != Mode.Record) {
			mode = Mode.Record;
		} else {
			mode = Mode.Neutral;
			SaveRecord ();
		}

		if(mode == Mode.Record){

			Inputs.Clear();
			Transforms.Clear ();
			Rigidbodies.Clear ();

		}

	}

	public void SaveRecord(){

		print ("Record saved!");
		recorded = new Recorded(Inputs.ToArray(), Transforms.ToArray(), Rigidbodies.ToArray(), RCC_Records.Instance.records.Count.ToString() + "_" + carController.transform.name);
		RCC_Records.Instance.records.Add (recorded);

	}

	//	public static void createNewRecipe(RecipeType type)
	//	{
	//		AssetDatabase.CreateAsset (type, "Assets/Resources/RecipeObject/"+type.name.Replace(" ", "")+".asset");
	//		AssetDatabase.SaveAssets ();
	//		EditorUtility.FocusProjectWindow ();
	//		Selection.activeObject = type;
	//	}

	public void Play(){

		if (recorded == null)
			return;

		if (mode != Mode.Play)
			mode = Mode.Play;
		else
			mode = Mode.Neutral;

		if (mode == Mode.Play)
			carController.externalController = true;
		else
			carController.externalController = false;

		if(mode == Mode.Play){

			StartCoroutine(Replay());

			if (recorded != null && recorded.transforms.Length > 0) {

				carController.transform.position = recorded.transforms [0].position;
				carController.transform.rotation = recorded.transforms [0].rotation;

			}

			StartCoroutine(Revel());

		}

	}

	public void Play(Recorded _recorded){

		recorded = _recorded;

		print ("Replaying record " + recorded.recordName);

		if (recorded == null)
			return;

		if (mode != Mode.Play)
			mode = Mode.Play;
		else
			mode = Mode.Neutral;

		if (mode == Mode.Play)
			carController.externalController = true;
		else
			carController.externalController = false;

		if(mode == Mode.Play){

			StartCoroutine(Replay());

			if (recorded != null && recorded.transforms.Length > 0) {

				carController.transform.position = recorded.transforms [0].position;
				carController.transform.rotation = recorded.transforms [0].rotation;

			}

			StartCoroutine(Revel());

		}

	}

	public void Stop(){

		mode = Mode.Neutral;
		carController.externalController = false;

	}

	private IEnumerator Replay(){

		for(int i = 0; i<recorded.inputs.Length && mode == Mode.Play; i++){

			carController.externalController = true;
			carController.throttleInput = recorded.inputs[i].throttleInput;
			carController.brakeInput = recorded.inputs[i].brakeInput;
			carController.steerInput = recorded.inputs[i].steerInput;
			carController.handbrakeInput = recorded.inputs[i].handbrakeInput;
			carController.clutchInput = recorded.inputs[i].clutchInput;
			carController.boostInput = recorded.inputs[i].boostInput;
			carController.fuelInput = recorded.inputs[i].fuelInput;
			carController.direction = recorded.inputs[i].direction;
			carController.canGoReverseNow = recorded.inputs[i].canGoReverse;
			carController.currentGear = recorded.inputs[i].currentGear;
			carController.changingGear = recorded.inputs[i].changingGear;

			carController.indicatorsOn = recorded.inputs[i].indicatorsOn;
			carController.lowBeamHeadLightsOn = recorded.inputs[i].lowBeamHeadLightsOn;
			carController.highBeamHeadLightsOn = recorded.inputs[i].highBeamHeadLightsOn;

			yield return new WaitForFixedUpdate();

		}

		mode = Mode.Neutral;

		carController.externalController = false;

	}

	private IEnumerator Repos(){

		for(int i = 0; i<recorded.transforms.Length && mode == Mode.Play; i++){

			carController.transform.position = recorded.transforms [i].position;
			carController.transform.rotation = recorded.transforms [i].rotation;

			yield return new WaitForEndOfFrame();

		}

		mode = Mode.Neutral;

		carController.externalController = false;

	}

	private IEnumerator Revel(){

		for(int i = 0; i<recorded.rigids.Length && mode == Mode.Play; i++){

			carController.rigid.velocity = recorded.rigids [i].velocity;
			carController.rigid.angularVelocity = recorded.rigids [i].angularVelocity;

			yield return new WaitForFixedUpdate();

		}

		mode = Mode.Neutral;

		carController.externalController = false;

	}

	void FixedUpdate () {

		if (!carController)
			return;

		switch (mode) {

		case Mode.Neutral:

			break;

		case Mode.Play:

			carController.externalController = true;

			break;

		case Mode.Record:

			Inputs.Add(new PlayerInput(carController.throttleInput, carController.brakeInput, carController.steerInput, carController.handbrakeInput, carController.clutchInput, carController.boostInput, carController.fuelInput, carController.direction, carController.canGoReverseNow, carController.currentGear, carController.changingGear, carController.indicatorsOn, carController.lowBeamHeadLightsOn, carController.highBeamHeadLightsOn));
			Transforms.Add (new PlayerTransform(carController.transform.position, carController.transform.rotation));
			Rigidbodies.Add(new PlayerRigidBody(carController.rigid.velocity, carController.rigid.angularVelocity));

			break;

		}

	}

}
