//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using UnityEditor;
using UnityEngine;

public class RCC_SceneGUI : EditorWindow{

	static GUISkin skin;

	static Texture2D cameraIcon;
	static Texture2D canvasIcon;
	static Texture2D hoodCameraIcon;
	static Texture2D wheelCameraIcon;
	static Texture2D headlightIcon;
	static Texture2D brakelightIcon;
	static Texture2D reverselightIcon;
	static Texture2D indicatorlightIcon;
	static Texture2D exhaustIcon;
	static Texture2D mirrorIcon;

	public static void GetImages(){

		skin = Resources.Load("RCC_WindowSkin") as GUISkin;

		cameraIcon = Resources.Load("Editor/CameraIcon", typeof(Texture2D)) as Texture2D;
		canvasIcon = Resources.Load("Editor/CanvasIcon", typeof(Texture2D)) as Texture2D;
		hoodCameraIcon = Resources.Load("Editor/HoodCameraIcon", typeof(Texture2D)) as Texture2D;
		wheelCameraIcon = Resources.Load("Editor/WheelCameraIcon", typeof(Texture2D)) as Texture2D;
		headlightIcon = Resources.Load("Editor/HeadlightIcon", typeof(Texture2D)) as Texture2D;
		brakelightIcon = Resources.Load("Editor/BrakelightIcon", typeof(Texture2D)) as Texture2D;
		reverselightIcon = Resources.Load("Editor/ReverselightIcon", typeof(Texture2D)) as Texture2D;
		indicatorlightIcon = Resources.Load("Editor/IndicatorlightIcon", typeof(Texture2D)) as Texture2D;
		exhaustIcon = Resources.Load("Editor/ExhaustIcon", typeof(Texture2D)) as Texture2D;
		mirrorIcon = Resources.Load("Editor/MirrorIcon", typeof(Texture2D)) as Texture2D;

	}
	
	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Enable In-Scene Buttons", false, 5000)]
	public static void Enable(){

		GetImages ();
		#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui += OnScene;
		#else
		SceneView.onSceneGUIDelegate += OnScene;
		#endif

	}

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Disable In-Scene Buttons", false, 5000)]
	public static void Disable(){
		
		#if UNITY_2019_1_OR_NEWER
		SceneView.duringSceneGui -= OnScene;
		#else
		SceneView.onSceneGUIDelegate -= OnScene;
		#endif

	}

	private static void OnScene(SceneView sceneview){

		GUI.skin = skin;
		
		Handles.BeginGUI();

		//	Scene buttons panel.
		GUILayout.BeginArea (new Rect(10f, 10f, 75f, 200f));

		GUILayout.BeginVertical("window");

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Scene");
		GUILayout.EndHorizontal();
		GUILayout.Space(4);

		if (GUILayout.Button (new GUIContent(cameraIcon, "Add/Select RCC Camera")))
			RCC_EditorWindows.CreateRCCCamera ();

		if (GUILayout.Button (new GUIContent(canvasIcon, "Add/Select RCC Canvas")))
			RCC_EditorWindows.CreateRCCCanvas ();



		Color defColor = GUI.color;
		GUI.color = Color.red;

		if (GUILayout.Button (new GUIContent(" X ", "Close the in-scene window. You can re-enable in Tools --> BCG --> RCC")))
			Disable ();

		GUI.color = defColor;

		GUILayout.EndVertical();

		GUILayout.EndArea ();

		//	Vehicle buttons panel
		GUILayout.BeginArea (new Rect(10f, 200f, 75f, 1000f));

		GUILayout.BeginVertical("window");

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("Vehicle");
		GUILayout.EndHorizontal();

		if (Selection.activeGameObject != null) {

			if (Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV3> ()) {

				if (GUILayout.Button (new GUIContent (hoodCameraIcon, "Add/Select Hood Camera attached to selected vehicle")))
					RCC_EditorWindows.CreateHoodCamera ();

				if (GUILayout.Button (new GUIContent (wheelCameraIcon, "Add/Select Wheel Camera attached to selected vehicle")))
					RCC_EditorWindows.CreateWheelCamera ();

				if (GUILayout.Button (new GUIContent (headlightIcon, "Add headlights to selected vehicle")))
					RCC_EditorWindows.CreateHeadLight ();

				if (GUILayout.Button (new GUIContent (brakelightIcon, "Add brakelights to selected vehicle")))
					RCC_EditorWindows.CreateBrakeLight ();

				if (GUILayout.Button (new GUIContent (reverselightIcon, "Add reverselights to selected vehicle")))
					RCC_EditorWindows.CreateReverseLight ();

				if (GUILayout.Button (new GUIContent (indicatorlightIcon, "Add Indicatorlights to selected vehicle")))
					RCC_EditorWindows.CreateIndicatorLight ();

				if (GUILayout.Button (new GUIContent (exhaustIcon, "Add exhaust attached to selected vehicle")))
					RCC_EditorWindows.CreateExhaust ();

				if (GUILayout.Button (new GUIContent (mirrorIcon, "Add/Select mirrors attached to selected vehicle")))
					RCC_EditorWindows.CreateMirrors (Selection.activeGameObject.GetComponentInParent<RCC_CarControllerV3> ().gameObject);

				if (Selection.activeGameObject.GetComponent<RCC_Light> ()) {

					if (GUILayout.Button (new GUIContent (indicatorlightIcon, "Duplicate light to opposite direction"))) {
						
						GameObject duplicatedLight = PrefabUtility.InstantiateAttachedAsset (Selection.activeObject) as GameObject;

						duplicatedLight.transform.name = Selection.activeGameObject.transform.name + "_D";
						duplicatedLight.transform.SetParent (Selection.activeGameObject.transform.parent);
						duplicatedLight.transform.localPosition = new Vector3 (-Selection.activeGameObject.transform.localPosition.x, Selection.activeGameObject.transform.localPosition.y, Selection.activeGameObject.transform.localPosition.z);
						duplicatedLight.transform.localRotation = Selection.activeGameObject.transform.localRotation;
						duplicatedLight.transform.localScale = Selection.activeGameObject.transform.localScale;

						Selection.activeGameObject = duplicatedLight;

					}

				}

			} else {

				GUILayout.Label ("Select a vehicle first");

			}

		} else {

			GUILayout.Label ("Select a vehicle first");

		}

		GUILayout.EndVertical ();

		GUILayout.EndArea ();
		
		Handles.EndGUI();

	}

}
