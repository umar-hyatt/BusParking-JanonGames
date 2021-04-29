//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class RCC_EditorWindows : Editor {

	private static RCC_CarControllerV3 SelectedCar(){

		if (Selection.activeGameObject == null)
			return null;

		return Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV3> ();

	}

	#region Edit Settings
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Edit RCC Settings", false, -100)]
	public static void OpenRCCSettings(){
		Selection.activeObject =RCC_Settings.Instance;
	}
	#endregion

	#region Configure
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Demo Vehicles", false, -65)]
	public static void OpenDemoVehiclesSettings(){
		Selection.activeObject = RCC_DemoVehicles.Instance;
	}

	#if BCG_PHOTON && PHOTON_UNITY_NETWORKING
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Photon Demo Vehicles", false, -65)]
	public static void OpenPhotonDemoVehiclesSettings(){
		Selection.activeObject = RCC_PhotonDemoVehicles.Instance;
	}
	#endif

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Ground Materials", false, -65)]
	public static void OpenGroundMaterialsSettings(){
		Selection.activeObject =RCC_GroundMaterials.Instance;
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Changable Wheels", false, -65)]
	public static void OpenChangableWheelSettings(){
		Selection.activeObject = RCC_ChangableWheels.Instance;
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Configure Recorded Clips", false, -65)]
	public static void OpenRecordSettings(){
		Selection.activeObject = RCC_Records.Instance;
	}
	#endregion

	#region Add Cameras
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add RCC Camera To Scene", false, -50)]
	public static void CreateRCCCamera(){

		if (GameObject.FindObjectOfType<RCC_Camera> ()) {

			EditorUtility.DisplayDialog ("Scene has RCC Camera already!", "Scene has RCC Camera already!", "Close");
			Selection.activeGameObject = GameObject.FindObjectOfType<RCC_Camera>().gameObject;

		} else {

			GameObject cam = Instantiate (RCC_Settings.Instance.RCCMainCamera.gameObject);
			cam.name = RCC_Settings.Instance.RCCMainCamera.name;
			Selection.activeGameObject = cam.gameObject;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", false, -50)]
	public static void CreateHoodCamera(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			if(SelectedCar().gameObject.GetComponentInChildren<RCC_HoodCamera>()){
				
				EditorUtility.DisplayDialog("Your Vehicle Has Hood Camera Already!", "Your vehicle has hood camera already!", "Close");
				Selection.activeGameObject = SelectedCar ().gameObject.GetComponentInChildren<RCC_HoodCamera> ().gameObject;
				return;

			}

			GameObject hoodCam = (GameObject)Instantiate(RCC_Settings.Instance.hoodCamera, SelectedCar().transform.position, SelectedCar().transform.rotation);
			hoodCam.name = RCC_Settings.Instance.hoodCamera.name;
			hoodCam.transform.SetParent(SelectedCar().transform, true);
			hoodCam.GetComponent<ConfigurableJoint>().connectedBody = SelectedCar().gameObject.GetComponent<Rigidbody>();
			RCC_LabelEditor.SetIcon(hoodCam, RCC_LabelEditor.LabelIcon.Purple);
			Selection.activeGameObject = hoodCam;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Hood Camera To Vehicle", true)]
	public static bool CheckCreateHoodCamera(){
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", false, -50)]
	public static void CreateWheelCamera(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			if(SelectedCar().gameObject.GetComponentInChildren<RCC_WheelCamera>()){
				EditorUtility.DisplayDialog("Your Vehicle Has Wheel Camera Already!", "Your vehicle has wheel camera already!", "Close");
				Selection.activeGameObject = SelectedCar ().gameObject.GetComponentInChildren<RCC_WheelCamera> ().gameObject;
				return;
			}

			GameObject wheelCam = new GameObject("WheelCamera");
			wheelCam.transform.SetParent(SelectedCar().transform, false);
			wheelCam.AddComponent<RCC_WheelCamera>();
			RCC_LabelEditor.SetIcon(wheelCam, RCC_LabelEditor.LabelIcon.Purple);
			Selection.activeGameObject = wheelCam;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Cameras/Add Wheel Camera To Vehicle", true)]
	public static bool CheckCreateWheelCamera() {
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
	}
	#endregion

	#region Add Lights
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", false, -50)]
	public static void CreateHeadLight(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			GameObject lightsMain;

			if(!SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights")){
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(SelectedCar().transform, false);
			}else{
				lightsMain = SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights").gameObject;
			}

			GameObject headLight = GameObject.Instantiate (RCC_Settings.Instance.headLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			headLight.name = RCC_Settings.Instance.headLights.name;
			headLight.transform.SetParent(lightsMain.transform);
			headLight.transform.localRotation = Quaternion.identity;
			headLight.transform.localPosition = new Vector3(0f, 0f, 2f);
			RCC_LabelEditor.SetIcon(headLight, RCC_LabelEditor.Icon.CircleTeal);
			Selection.activeGameObject = headLight;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/HeadLight", true)]
	public static bool CheckHeadLight() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", false, -50)]
	public static void CreateBrakeLight(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			GameObject lightsMain;

			if(!SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights")){
				
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(SelectedCar().transform, false);

			}else{
				
				lightsMain = SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights").gameObject;

			}

			GameObject brakeLight = GameObject.Instantiate (RCC_Settings.Instance.brakeLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			brakeLight.name = RCC_Settings.Instance.brakeLights.name;
			brakeLight.transform.SetParent(lightsMain.transform);
			brakeLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			brakeLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			RCC_LabelEditor.SetIcon(brakeLight, RCC_LabelEditor.Icon.CircleRed);
			Selection.activeGameObject = brakeLight;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Brake", true)]
	public static bool CheckBrakeLight() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", false, -50)]
	public static void CreateReverseLight(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			GameObject lightsMain;

			if(!SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights")){
				
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(SelectedCar().transform, false);

			}else{
				
				lightsMain = SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights").gameObject;

			}

			GameObject reverseLight = GameObject.Instantiate (RCC_Settings.Instance.reverseLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			reverseLight.name = RCC_Settings.Instance.reverseLights.name;
			reverseLight.transform.SetParent(lightsMain.transform);
			reverseLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			reverseLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			RCC_LabelEditor.SetIcon(reverseLight, RCC_LabelEditor.Icon.CircleGray);
			Selection.activeGameObject = reverseLight;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Reverse", true)]
	public static bool CheckReverseLight() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", false, -50)]
	public static void CreateIndicatorLight(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			GameObject lightsMain;

			if(!SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights")){
				
				lightsMain = new GameObject("Lights");
				lightsMain.transform.SetParent(SelectedCar().transform, false);

			}else{
				
				lightsMain = SelectedCar().transform.Find(SelectedCar().transform.name+"/Lights").gameObject;

			}

			GameObject indicatorLight = GameObject.Instantiate (RCC_Settings.Instance.indicatorLights, lightsMain.transform.position, lightsMain.transform.rotation) as GameObject;
			Vector3 relativePos = SelectedCar().transform.InverseTransformPoint (indicatorLight.transform.position);
			indicatorLight.name = RCC_Settings.Instance.indicatorLights.name;
			indicatorLight.transform.SetParent(lightsMain.transform);

			if (relativePos.z > 0f)
				indicatorLight.transform.localRotation = Quaternion.identity;
			else
				indicatorLight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

			indicatorLight.transform.localPosition = new Vector3(0f, 0f, -2f);
			RCC_LabelEditor.SetIcon(indicatorLight, RCC_LabelEditor.Icon.CircleOrange);
			Selection.activeGameObject = indicatorLight;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Lights/Add Lights To Vehicle/Indicator", true)]
	public static bool CheckIndicatorLight() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}
	#endregion

	#region Add UI
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/UI/Add RCC Canvas To Scene", false, -50)]
	public static void CreateRCCCanvas(){

		if (GameObject.FindObjectOfType<RCC_DashboardInputs> ()) {

			EditorUtility.DisplayDialog ("Scene has RCC Canvas already!", "Scene has RCC Canvas already!", "Close");
			Selection.activeGameObject = GameObject.FindObjectOfType<RCC_DashboardInputs> ().gameObject;

		} else {

			GameObject canvas = Instantiate (RCC_Settings.Instance.RCCCanvas);
			canvas.name = RCC_Settings.Instance.RCCCanvas.name;
			Selection.activeGameObject = canvas;

		}

	}
	#endregion

	#region Add Exhausts
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", false, -50)]
	public static void CreateExhaust(){

		if(SelectedCar() == null){

			EditorUtility.DisplayDialog("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");

		}else{

			GameObject exhaustsMain;

			if(!SelectedCar().transform.Find(SelectedCar().transform.name+"/Exhausts")){
				exhaustsMain = new GameObject("Exhausts");
				exhaustsMain.transform.SetParent(SelectedCar().transform, false);
			}else{
				exhaustsMain = SelectedCar().transform.Find(SelectedCar().transform.name+"/Exhausts").gameObject;
			}

			GameObject exhaust = (GameObject)Instantiate(RCC_Settings.Instance.exhaustGas, SelectedCar().transform.position, SelectedCar().transform.rotation * Quaternion.Euler(0f, 180f, 0f));
			exhaust.name = RCC_Settings.Instance.exhaustGas.name;
			exhaust.transform.SetParent(exhaustsMain.transform);
			exhaust.transform.localPosition = new Vector3(1f, 0f, -2f);
			RCC_LabelEditor.SetIcon(exhaust, RCC_LabelEditor.Icon.DiamondGray);
			Selection.activeGameObject = exhaust;

		}

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Exhaust To Vehicle", true)]
	public static bool CheckCreateExhaust() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}
	#endregion

	#region Add Mirrors
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", false, -50)]
	public static void CreateBehavior(){

		if (SelectedCar () == null)
			EditorUtility.DisplayDialog ("Select a vehicle controlled by Realistic Car Controller!", "Select a vehicle controlled by Realistic Car Controller!", "Close");
		else
			CreateMirrors(SelectedCar().gameObject);

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Create/Misc/Add Mirrors To Vehicle", true)]
	public static bool CheckCreateBehavior() {
		
		if(Selection.gameObjects.Length > 1 || !Selection.activeTransform)
			return false;
		else
			return true;
		
	}
	#endregion

	#region Help
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Help", false, 1000)]
	public static void Help(){

		EditorUtility.DisplayDialog("Contact", "Please include your invoice number while sending a contact form.", "Close");

		string url = "http://www.bonecrackergames.com/contact/";
		Application.OpenURL (url);

	}

	#endregion Help

	#region Static Methods
	public static void CreateMirrors (GameObject vehicle) {

		if(!vehicle.transform.GetComponentInChildren<RCC_Mirror>()){

			GameObject mirrors = (GameObject)Instantiate(RCC_Settings.Instance.mirrors, vehicle.transform.position, vehicle.transform.rotation);
			mirrors.transform.SetParent(vehicle.GetComponent<RCC_CarControllerV3>().transform, true);
			mirrors.name = "Mirrors";
			RCC_LabelEditor.SetIcon(mirrors.transform.GetChild(0).gameObject, RCC_LabelEditor.Icon.DiamondRed);
			RCC_LabelEditor.SetIcon(mirrors.transform.GetChild(1).gameObject, RCC_LabelEditor.Icon.DiamondBlue);
			RCC_LabelEditor.SetIcon(mirrors.transform.GetChild(2).gameObject, RCC_LabelEditor.Icon.DiamondTeal);
			Selection.activeGameObject = mirrors;
			EditorUtility.DisplayDialog("Created Mirrors!", "Created mirrors. Adjust their positions.", "Close");

		}else{

			EditorUtility.DisplayDialog("Vehicle Has Mirrors Already", "Vehicle has mirrors already!", "Close");

		}

	}
	#endregion

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Export Project Settings", false, 1000)]
	public static void ExportProjectSettings(){

		string[] projectContent = new string[] {"ProjectSettings/TagManager.asset","ProjectSettings/InputManager.asset"};
		AssetDatabase.ExportPackage(projectContent, "RCC_ProjectSettings.unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse |ExportPackageOptions.IncludeDependencies);
		Debug.Log ("Project Exported");

	}

}
