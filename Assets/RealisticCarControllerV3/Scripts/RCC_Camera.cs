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
using UnityEngine.EventSystems;

/// <summary>
/// Main RCC Camera controller. Includes 6 different camera modes with many customizable settings. It doesn't use different cameras on your scene like *other* assets. Simply it parents the camera to their positions that's all. No need to be Einstein.
/// Also supports collision detection.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/RCC Camera")]
public class RCC_Camera : MonoBehaviour{

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

	// Currently rendering?
	public bool isRendering = true;

	// The target we are following transform and rigidbody.
	public RCC_CarControllerV3 playerCar;
	private Rigidbody playerRigid;
	private float playerSpeed = 0f;
	private Vector3 playerVelocity = new Vector3 (0f, 0f, 0f);

	public Camera thisCam;			// Camera is not attached to this main gameobject. Camera is parented to pivot gameobject. Therefore, we can apply additional position and rotation changes.
	public GameObject pivot;		// Pivot center of the camera. Used for making offsets and collision movements.

	// Camera Modes.
	public CameraMode cameraMode;
	public enum CameraMode{TPS, FPS, WHEEL, FIXED, CINEMATIC, TOP}
	public CameraMode lastCameraMode;

	public bool useTopCameraMode = false;				// Shall we use top camera mode?
	public bool useHoodCameraMode = true;				// Shall we use hood camera mode?
	public bool useOrbitInTPSCameraMode = true;		// Shall we use orbit control in TPS camera mode?
	public bool useOrbitInHoodCameraMode = true;	// Shall we use orbit control in hood camera mode?
	public bool useWheelCameraMode = true;			// Shall we use wheel camera mode?
	public bool useFixedCameraMode = true;				// Shall we use fixed camera mode?
	public bool useCinematicCameraMode = true;		// Shall we use cinematic camera mode?
	public bool useOrthoForTopCamera = false;			// Shall we use ortho in top camera mode?
	public bool useOcclusion = true;							// Shall we use camera occlusion?
	public LayerMask occlusionLayerMask = -1;

	public bool useAutoChangeCamera = false;			// Shall we change camera mode by auto?
	private float autoChangeCameraTimer = 0f;

	public Vector3 topCameraAngle = new Vector3(45f, 45f, 0f);		// If so, we will use this Vector3 angle for top camera mode.

	private float distanceOffset = 0f;	
	public float maximumZDistanceOffset = 10f;		// Distance offset for top camera mode. Related with vehicle speed. If vehicle speed is higher, camera will move to front of the vehicle.
	public float topCameraDistance = 100f;				// Top camera height / distance.

	// Target position.
	private Vector3 targetPosition = Vector3.zero;

	// Used for resetting orbit values when direction of the vehicle has been changed.
	private int direction = 1;
	private int lastDirection = 1;

	public float TPSDistance = 6f;										// The distance for TPS camera mode.
	public float TPSHeight = 2f;											// The height we want the camera to be above the target for TPS camera mode.
	public float TPSHeightDamping = 10f;							// Height movement damper.
	public float TPSRotationDamping = 5f;							// Rotation movement damper.
	public float TPSTiltMaximum = 15f;								// Maximum tilt angle related with rigidbody local velocity.
	public float TPSTiltMultiplier = 2f;								// Tilt angle multiplier.
	private float TPSTiltAngle = 0f;									// Current tilt angle.
	public float TPSYawAngle = 0f;									// Yaw angle.
	public float TPSPitchAngle = 7f;									// Pitch angle.
	public float TPSOffsetX = 0f;										// Offset X.
	public float TPSOffsetY = .5f;										// Offset Y.
	public bool TPSAutoFocus = true;								// Auto focus to player vehicle.
	public bool TPSAutoReverse = true;								// Auto reverse when player vehicle is at reverse gear.
	public bool TPSCollision = true;									// Collision effect when player vehicle crashes.
	public Vector3 TPSOffset = new Vector3(0f, 0f, .25f);   // Camera will look at front wheel distance.
	public Vector3 TPSStartRotation = new Vector3(0f, 0f, 0f);   // Camera will look at front wheel distance.

	internal float targetFieldOfView = 60f;	// Camera will adapt its field of view to this target field of view. All field of views below this line will feed this value.

	public float TPSMinimumFOV = 50f;			// Minimum field of view related with vehicle speed.
	public float TPSMaximumFOV = 70f;			// Maximum field of view related with vehicle speed.
	public float hoodCameraFOV = 60f;			// Hood field of view.
	public float wheelCameraFOV = 60f;			// Wheel field of view.
	public float minimumOrtSize = 10f;			// Minimum ortho size related with vehicle speed.
	public float maximumOrtSize = 20f;			// Maximum ortho size related with vehicle speed.

	internal int cameraSwitchCount = 0;					// Used in switch case for running corresponding camera mode method.
	private RCC_HoodCamera hoodCam;					// Hood camera. It's a null script. Just used for finding hood camera parented to our player vehicle.
	private RCC_WheelCamera wheelCam;				// Wheel camera. It's a null script. Just used for finding wheel camera parented to our player vehicle.
	private RCC_FixedCamera fixedCam;					// Fixed Camera System.
	private RCC_CinematicCamera cinematicCam;		// Cinematic Camera System.

	private Vector3 collisionVector = Vector3.zero;				// Collision vector.
	private Vector3 collisionPos = Vector3.zero;					// Collision position.
	private Quaternion collisionRot = Quaternion.identity;	// Collision rotation.

	private Quaternion orbitRotation = Quaternion.identity;		// Orbit rotation.

	// Orbit X and Y inputs.
	internal float orbitX = 0f;
	internal float orbitY = 0f;

	// Minimum and maximum Orbit X, Y degrees.
	public float minOrbitY = -20f;
	public float maxOrbitY = 80f;

	//	Orbit X and Y speeds.
	public float orbitXSpeed = 50f;
	public float orbitYSpeed = 50f;
	public float orbitSmooth = 10f;

	//	Resetting orbits.
	public bool orbitReset = false;
	private float orbitResetTimer = 0f;
	private float oldOrbitX = 0f;
	public float oldOrbitY = 0f;

	// Calculate the current rotation angles for TPS mode.
	private Quaternion currentRotation = Quaternion.identity;
	private Quaternion wantedRotation = Quaternion.identity;
	private float currentHeight = 0f;
	private float wantedHeight = 0f;

	public delegate void onBCGCameraSpawned(GameObject BCGCamera);
	public static event onBCGCameraSpawned OnBCGCameraSpawned;

	void Awake(){
		
		// Getting Camera.
		thisCam = GetComponentInChildren<Camera>();

		GameObject dir = new GameObject ("Camera Direction");
		dir.transform.SetParent (thisCam.transform, false);
		dir.transform.localPosition = new Vector3 (0f, 0f, 10f);
		dir.transform.localRotation = Quaternion.identity;

	}

	void OnEnable(){

		// Calling this event when BCG Camera spawned.
		if(OnBCGCameraSpawned != null)
			OnBCGCameraSpawned (gameObject);

		// Listening player vehicle collisions for crashing effects.
		RCC_CarControllerV3.OnRCCPlayerCollision += RCC_CarControllerV3_OnRCCPlayerCollision;

	}

	void RCC_CarControllerV3_OnRCCPlayerCollision (RCC_CarControllerV3 RCC, Collision collision){

		Collision (collision);
		
	}

	private void GetTarget(){

		// Return if we don't have the player vehicle.
		if(!playerCar)
			return;

		if(TPSAutoFocus)
			StartCoroutine(AutoFocus ());

		// Getting rigid of the player vehicle.
		playerRigid = playerCar.GetComponent<Rigidbody>();

		// Getting camera modes from the player vehicle.
		hoodCam = playerCar.GetComponentInChildren<RCC_HoodCamera>();
		wheelCam = playerCar.GetComponentInChildren<RCC_WheelCamera>();
		fixedCam = GameObject.FindObjectOfType<RCC_FixedCamera>();
		cinematicCam = GameObject.FindObjectOfType<RCC_CinematicCamera>();

		ResetCamera ();

		// Setting transform and position to player vehicle when switched camera target.
//		transform.position = playerCar.transform.position;
//		transform.rotation = playerCar.transform.rotation * Quaternion.AngleAxis(10f, Vector3.right);

	}

	public void SetTarget(GameObject player){

		playerCar = player.GetComponent<RCC_CarControllerV3>();
		GetTarget ();

	}

	public void RemoveTarget(){

		transform.SetParent (null);
		playerCar = null;
		playerRigid = null;

	}

	void Update(){

		// If it's active, enable the camera. If it's not, disable the camera.
		if (!isRendering) {

			if(thisCam.gameObject.activeInHierarchy)
				thisCam.gameObject.SetActive (false);

			return;

		} else {

			if(!thisCam.gameObject.activeInHierarchy)
				thisCam.gameObject.SetActive (true);

		}

		// Early out if we don't have the player vehicle.
		if (!playerCar || !playerRigid){

			GetTarget();
			return;

		}

		Inputs ();

		// Speed of the vehicle (smoothed).
		playerSpeed = Mathf.Lerp(playerSpeed, playerCar.speed, Time.deltaTime * 5f);

		// Velocity of the vehicle.
		playerVelocity = playerCar.transform.InverseTransformDirection(playerRigid.velocity);

		// Lerping current field of view to target field of view.
		thisCam.fieldOfView = Mathf.Lerp (thisCam.fieldOfView, targetFieldOfView, Time.deltaTime * 5f);

	}

	void LateUpdate (){

		// Early out if we don't have the player vehicle.
		if (!playerCar || !playerRigid)
			return;

		// Even if we have the player vehicle and it's disabled, return.
		if (!playerCar.gameObject.activeSelf)
			return;

		// Run the corresponding method with choosen camera mode.
		switch(cameraMode){

		case CameraMode.TPS:
			TPS();
			if (useOrbitInTPSCameraMode)
				ORBIT ();
			break;

		case CameraMode.FPS:
			FPS ();
			if (useOrbitInHoodCameraMode)
				ORBIT ();
			break;

		case CameraMode.WHEEL:
			WHEEL();
			break;

		case CameraMode.FIXED:
			FIXED();
			break;

		case CameraMode.CINEMATIC:
			CINEMATIC();
			break;

		case CameraMode.TOP:
			TOP();
			break;

		}

		if (lastCameraMode != cameraMode)
			ResetCamera ();

		lastCameraMode = cameraMode;
		autoChangeCameraTimer += Time.deltaTime;

		if (useAutoChangeCamera && autoChangeCameraTimer > 10) {

			autoChangeCameraTimer = 0f;
			ChangeCamera ();

		}

	}

	private void Inputs(){
		
		switch(RCCSettings.selectedControllerType){

		case RCC_Settings.ControllerType.Keyboard:

			if (Input.GetKeyDown (RCCSettings.changeCameraKB))
				ChangeCamera ();

			orbitX += Input.GetAxis (RCCSettings.mouseXInput) * orbitXSpeed * .02f;
			orbitY -= Input.GetAxis (RCCSettings.mouseYInput) * orbitYSpeed * .02f;

			break;

		case RCC_Settings.ControllerType.XBox360One:

			if (Input.GetButtonDown (RCCSettings.Xbox_changeCameraKB))
				ChangeCamera ();

			orbitX += Input.GetAxis (RCCSettings.Xbox_mouseXInput) * orbitXSpeed * .01f;
			orbitY -= Input.GetAxis (RCCSettings.Xbox_mouseYInput) * orbitYSpeed * .01f;

			break;

		case RCC_Settings.ControllerType.PS4:

			if (Input.GetButtonDown (RCCSettings.PS4_changeCameraKB))
				ChangeCamera ();

			orbitX += Input.GetAxis (RCCSettings.PS4_mouseXInput) * orbitXSpeed * .01f;
			orbitY -= Input.GetAxis (RCCSettings.PS4_mouseYInput) * orbitYSpeed * .01f;

			break;

		case RCC_Settings.ControllerType.LogitechSteeringWheel:
			
			#if BCG_LOGITECH
			if (RCC_LogitechSteeringWheel.GetKeyTriggered (0, RCCSettings.LogiSteeringWheel_changeCameraKB))
				ChangeCamera ();
			#endif

			break;

		}

	}

	// Change camera by increasing camera switch counter.
	public void ChangeCamera(){

		// Increasing camera switch counter at each camera changing.
		cameraSwitchCount ++;

		// We have 6 camera modes at total. If camera switch counter is greater than maximum, set it to 0.
		if (cameraSwitchCount >= 6)
			cameraSwitchCount = 0;

		switch(cameraSwitchCount){

		case 0:
			cameraMode = CameraMode.TPS;
			break;

		case 1:
			if(useHoodCameraMode && hoodCam){
				cameraMode = CameraMode.FPS;
			}else{
				ChangeCamera();
			}
			break;

		case 2:
			if(useWheelCameraMode && wheelCam){
				cameraMode = CameraMode.WHEEL;
			}else{
				ChangeCamera();
			}
			break;

		case 3:
			if(useFixedCameraMode && fixedCam){
				cameraMode = CameraMode.FIXED;
			}else{
				ChangeCamera();
			}
			break;

		case 4:
			if(useCinematicCameraMode && cinematicCam){
				cameraMode = CameraMode.CINEMATIC;
			}else{
				ChangeCamera();
			}
			break;

		case 5:
			if(useTopCameraMode){
				cameraMode = CameraMode.TOP;
			}else{
				ChangeCamera();
			}
			break;

		}

	}

	// Change camera by directly setting it to specific mode.
	public void ChangeCamera(CameraMode mode){

		cameraMode = mode;

	}

	private void FPS(){

		// Assigning orbit rotation, and transform rotation.
		transform.rotation = Quaternion.Lerp(transform.rotation, playerCar.transform.rotation * orbitRotation, Time.deltaTime * 50f);

	}

	private void WHEEL(){

		if (useOcclusion && Occluding (playerCar.transform.position))
			ChangeCamera (CameraMode.TPS);

	}

	private void TPS(){

		if (lastDirection != playerCar.direction) {

			direction = playerCar.direction;
			orbitX = 0f;
			orbitY = 0f;

		}

		lastDirection = playerCar.direction;

		// Calculate the current rotation angles for TPS mode.
		if(TPSAutoReverse)
			wantedRotation = playerCar.transform.rotation * Quaternion.AngleAxis ((direction == 1 ? 0 : 180), Vector3.up);
		else
			wantedRotation = playerCar.transform.rotation;

		switch (RCCSettings.selectedControllerType){

		case RCC_Settings.ControllerType.Keyboard:

			if(Input.GetKey(RCCSettings.lookBackKB))
				wantedRotation = wantedRotation * Quaternion.AngleAxis ((180), Vector3.up);

			break;

		case RCC_Settings.ControllerType.XBox360One:

			if(Input.GetButton(RCCSettings.Xbox_lookBackKB))
				wantedRotation = wantedRotation * Quaternion.AngleAxis ((180), Vector3.up);

			break;

		case RCC_Settings.ControllerType.LogitechSteeringWheel:

			#if BCG_LOGITECH
			if(RCC_LogitechSteeringWheel.GetKeyPressed(0, RCCSettings.LogiSteeringWheel_lookBackKB))
				wantedRotation = wantedRotation * Quaternion.AngleAxis ((180), Vector3.up);
			#endif

			break;

		}

		// Wanted height.
		wantedHeight = playerCar.transform.position.y + TPSHeight + TPSOffsetY;

		// Damp the height.
		currentHeight = Mathf.Lerp (currentHeight, wantedHeight, TPSHeightDamping * Time.fixedDeltaTime);

		// Damp the rotation around the y-axis.
		if(Time.time > 1)
			currentRotation = Quaternion.Lerp(currentRotation, wantedRotation, 1f - Mathf.Exp(-TPSRotationDamping * Time.deltaTime));
//			currentRotation = Quaternion.Lerp(currentRotation, wantedRotation, TPSRotationDamping * Time.deltaTime);
		else
			currentRotation = wantedRotation;

		// Rotates camera by Z axis for tilt effect.
		TPSTiltAngle = Mathf.Lerp(0f, TPSTiltMaximum * Mathf.Clamp(-playerVelocity.x, -1f, 1f), Mathf.Abs(playerVelocity.x) / 50f);
		TPSTiltAngle *= TPSTiltMultiplier;

		// Set the position of the camera on the x-z plane to distance meters behind the target.
		targetPosition = playerCar.transform.position;
		targetPosition -= (currentRotation * orbitRotation) * Vector3.forward * TPSDistance;
		targetPosition += Vector3.up * (TPSHeight * Mathf.Lerp(1f, .75f, (playerRigid.velocity.magnitude * 3.6f) / 100f));

		transform.position = targetPosition;

//		thisCam.transform.localPosition = Vector3.Lerp(thisCam.transform.localPosition, new Vector3 (TPSTiltAngle / 10f, 0f, 0f), Time.deltaTime * 3f);

		// Always look at the target.
		Vector3 lookAtPosition = playerCar.transform.position;

		if (TPSOffset != Vector3.zero)
			lookAtPosition += playerCar.transform.rotation * TPSOffset;

		transform.LookAt (lookAtPosition);

		transform.position = transform.position + (transform.right * TPSOffsetX) + (transform.up * TPSOffsetY);
		transform.rotation *= Quaternion.Euler (TPSPitchAngle * Mathf.Lerp(1f, .75f, (playerSpeed) / 100f), 0, Mathf.Clamp(-TPSTiltAngle, -TPSTiltMaximum, TPSTiltMaximum) + TPSYawAngle);

		// Collision positions and rotations that affects pivot of the camera.
		collisionPos = Vector3.Lerp(new Vector3(collisionPos.x, collisionPos.y, collisionPos.z), Vector3.zero, Time.deltaTime * 5f);

		if(Time.deltaTime != 0)
			collisionRot = Quaternion.Lerp(collisionRot, Quaternion.identity, Time.deltaTime * 5f);

		// Lerping position and rotation of the pivot to collision.
		pivot.transform.localPosition = Vector3.Lerp(pivot.transform.localPosition, collisionPos, Time.deltaTime * 10f);
		pivot.transform.localRotation = Quaternion.Lerp(pivot.transform.localRotation, collisionRot, Time.deltaTime * 10f);

		// Lerping targetFieldOfView from TPSMinimumFOV to TPSMaximumFOV related with vehicle speed.
		targetFieldOfView = Mathf.Lerp(TPSMinimumFOV, TPSMaximumFOV, Mathf.Abs(playerSpeed) / 150f);

		if(useOcclusion)
			OccludeRay (playerCar.transform.position);

	}

	void FIXED(){

		if(fixedCam.transform.parent != null)
			fixedCam.transform.SetParent(null);

		if (useOcclusion && Occluding (playerCar.transform.position))
			fixedCam.ChangePosition ();

	}

	private void TOP(){

		// Early out if we don't have the player vehicle.
		if (!playerCar || !playerRigid)
			return;

		// Setting ortho or perspective?
		thisCam.orthographic = useOrthoForTopCamera;

		distanceOffset = Mathf.Lerp (0f, maximumZDistanceOffset, Mathf.Abs(playerSpeed) / 100f);
		targetFieldOfView = Mathf.Lerp (minimumOrtSize, maximumOrtSize, Mathf.Abs(playerSpeed) / 100f);
		thisCam.orthographicSize = targetFieldOfView;

		// Setting proper targetPosition for top camera mode.
		targetPosition = playerCar.transform.position;
		targetPosition += playerCar.transform.rotation * Vector3.forward * distanceOffset;

		// Assigning position and rotation.
		transform.position = targetPosition;
		transform.rotation = Quaternion.Euler (topCameraAngle);

		// Pivot position.
		pivot.transform.localPosition = new Vector3 (0f, 0f, -topCameraDistance);

	}

	private void ORBIT(){

		// Clamping Y.
		orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

		if (orbitX < -360f)
			orbitX += 360f;
		if (orbitX > 360f)
			orbitX -= 360f;

		orbitRotation = Quaternion.Lerp(orbitRotation, Quaternion.Euler (orbitY, orbitX, 0f), orbitSmooth * Time.deltaTime);

		if (oldOrbitX != orbitX) {

			oldOrbitX = orbitX;
			orbitResetTimer = 2f;

		}

		if (oldOrbitY != orbitY) {

			oldOrbitY = orbitY;
			orbitResetTimer = 2f;

		}

		if(orbitResetTimer > 0)
			orbitResetTimer -= Time.deltaTime;

		Mathf.Clamp (orbitResetTimer, 0f, 2f);

		if (orbitReset && playerSpeed >= 25f && orbitResetTimer <= 0f) {

			orbitX = 0f;
			orbitY = 0f;

		}

	}

	public void OnDrag(PointerEventData pointerData){

		// Receiving drag input from UI.
		orbitX += pointerData.delta.x * orbitXSpeed / 1000f;
		orbitY -= pointerData.delta.y * orbitYSpeed / 1000f;

		orbitResetTimer = 0f;

	}

	public void OnDrag(float x, float y){

		// Receiving drag input from UI.
		orbitX += x * orbitXSpeed / 10f;
		orbitY -= y * orbitYSpeed / 10f;

		orbitResetTimer = 0f;

	}

	private void CINEMATIC(){

		if(cinematicCam.transform.parent != null)
			cinematicCam.transform.SetParent(null);

		targetFieldOfView = cinematicCam.targetFOV;

		if (useOcclusion && Occluding (playerCar.transform.position))
			ChangeCamera (CameraMode.TPS);

	}

	public void Collision(Collision collision){

		// If it's not enable or camera mode is TPS, return.
		if(!enabled || !isRendering || cameraMode != CameraMode.TPS || !TPSCollision)
			return;

		// Local relative velocity.
		Vector3 colRelVel = collision.relativeVelocity;
		colRelVel *= 1f - Mathf.Abs(Vector3.Dot(transform.up, collision.contacts[0].normal));

		float cos = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, colRelVel.normalized));

		if (colRelVel.magnitude * cos >= 5f){

			collisionVector = transform.InverseTransformDirection(colRelVel) / (30f);

			collisionPos -= collisionVector * 5f;
			collisionRot = Quaternion.Euler(new Vector3(-collisionVector.z * 10f, -collisionVector.y * 10f, -collisionVector.x * 10f));
			targetFieldOfView = thisCam.fieldOfView - Mathf.Clamp(collision.relativeVelocity.magnitude, 0f, 15f);

		}

	}

	private void ResetCamera(){
		
		if(fixedCam)
			fixedCam.canTrackNow = false;

		TPSTiltAngle = 0f;

		collisionPos = Vector3.zero;
		collisionRot = Quaternion.identity;

		thisCam.transform.localPosition = Vector3.zero;
		thisCam.transform.localRotation = Quaternion.identity;

		pivot.transform.localPosition = collisionPos;
		pivot.transform.localRotation = collisionRot;

		orbitX = TPSStartRotation.y;
		orbitY = TPSStartRotation.x;

		if (TPSStartRotation != Vector3.zero)
			TPSStartRotation = Vector3.zero;

		thisCam.orthographic = false;

		switch (cameraMode) {

		case CameraMode.TPS:
			transform.SetParent(null);
			targetFieldOfView = TPSMinimumFOV;
			break;

		case CameraMode.FPS:
			transform.SetParent (hoodCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = hoodCameraFOV;
			hoodCam.FixShake ();
			break;

		case CameraMode.WHEEL:
			transform.SetParent(wheelCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = wheelCameraFOV;
			break;

		case CameraMode.FIXED:
			transform.SetParent(fixedCam.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = 60;
			fixedCam.canTrackNow = true;
			break;

		case CameraMode.CINEMATIC:
			transform.SetParent(cinematicCam.pivot.transform, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			targetFieldOfView = 30f;
			break;

		case CameraMode.TOP:
			transform.SetParent(null);
			targetFieldOfView = minimumOrtSize;
			pivot.transform.localPosition = Vector3.zero;
			pivot.transform.localRotation = Quaternion.identity;
			targetPosition = playerCar.transform.position;
			targetPosition += playerCar.transform.rotation * Vector3.forward * distanceOffset;
			transform.position = playerCar.transform.position;
			break;

		}

	}

	public void ToggleCamera(bool state){

		// Enabling / disabling activity.
		isRendering = state;

	}

	private void OccludeRay(Vector3 targetFollow){

		//declare a new raycast hit.
		RaycastHit wallHit = new RaycastHit();

		if (Physics.Linecast (targetFollow, transform.position, out wallHit, occlusionLayerMask)) {

			if (!wallHit.collider.isTrigger && !wallHit.transform.IsChildOf (playerCar.transform)) {

				//the x and z coordinates are pushed away from the wall by hit.normal.
				//the y coordinate stays the same.
				Vector3 occludedPosition = new Vector3 (wallHit.point.x + wallHit.normal.x * .2f, wallHit.point.y + wallHit.normal.y * .2f, wallHit.point.z + wallHit.normal.z * .2f);

				transform.position = occludedPosition;

			}

		}

	}

	private bool Occluding(Vector3 targetFollow){

		//declare a new raycast hit.
		RaycastHit wallHit = new RaycastHit();

		//linecast from your player (targetFollow) to your cameras mask (camMask) to find collisions.
		if (Physics.Linecast (targetFollow, transform.position, out wallHit, ~(1 << LayerMask.NameToLayer(RCCSettings.RCCLayer)))) {

			if (!wallHit.collider.isTrigger && !wallHit.transform.IsChildOf (playerCar.transform))
				return true;

		}

		return false;

	}

	public IEnumerator AutoFocus(){

		float timer = 3f;

		while (timer > 0f) {
			
			timer -= Time.deltaTime;
			TPSDistance = Mathf.Lerp(TPSDistance, RCC_GetBounds.MaxBoundsExtent (playerCar.transform) * 2.55f, Time.deltaTime);
			TPSHeight = Mathf.Lerp (TPSHeight, RCC_GetBounds.MaxBoundsExtent (playerCar.transform) * .75f, Time.deltaTime);
			yield return null;

		}

	}

	public IEnumerator AutoFocus(Transform bounds){

		float timer = 3f;

		while (timer > 0f) {

			timer -= Time.deltaTime;
			TPSDistance = Mathf.Lerp(TPSDistance, RCC_GetBounds.MaxBoundsExtent (bounds) * 2.55f, Time.deltaTime);
			TPSHeight = Mathf.Lerp(TPSHeight, RCC_GetBounds.MaxBoundsExtent (bounds) * .75f, Time.deltaTime);
			yield return null;

		}

	}

	public IEnumerator AutoFocus(Transform bounds1, Transform bounds2){

		float timer = 3f;

		while (timer > 0f) {

			timer -= Time.deltaTime;
			TPSDistance = Mathf.Lerp(TPSDistance, ((RCC_GetBounds.MaxBoundsExtent (bounds1) * 2.55f) + (RCC_GetBounds.MaxBoundsExtent (bounds2) * 2.75f)), Time.deltaTime);
			TPSHeight = Mathf.Lerp(TPSHeight, ((RCC_GetBounds.MaxBoundsExtent (bounds1) * .75f) + (RCC_GetBounds.MaxBoundsExtent (bounds2) * .6f)), Time.deltaTime);
			yield return null;

		}

	}

	public IEnumerator AutoFocus(Transform bounds1, Transform bounds2, Transform bounds3){

		float timer = 5f;

		while (timer > 0f) {

			timer -= Time.deltaTime;
			TPSDistance = Mathf.Lerp(TPSDistance, ((RCC_GetBounds.MaxBoundsExtent (bounds1) * 2.75f) + (RCC_GetBounds.MaxBoundsExtent (bounds2) * 2.75f) + (RCC_GetBounds.MaxBoundsExtent (bounds3) * 2.75f)), Time.deltaTime);
			TPSHeight = Mathf.Lerp(TPSHeight, ((RCC_GetBounds.MaxBoundsExtent (bounds1) * .6f) + (RCC_GetBounds.MaxBoundsExtent (bounds2) * .6f) + (RCC_GetBounds.MaxBoundsExtent (bounds3) * .6f)), Time.deltaTime);
			yield return null;

		}

	}

	void OnDisable(){

		RCC_CarControllerV3.OnRCCPlayerCollision -= RCC_CarControllerV3_OnRCCPlayerCollision;

	}

}