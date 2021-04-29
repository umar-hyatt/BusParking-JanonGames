//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2020 BoneCracker Games
// http://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class RCC_WelcomeWindow : EditorWindow{

	public class ToolBar{
		
		public string title;
		public UnityEngine.Events.UnityAction Draw;

		/// <summary>
		/// Create New Toolbar
		/// </summary>
		/// <param name="title">Title</param>
		/// <param name="onDraw">Method to draw when toolbar is selected</param>
		public ToolBar(string title, UnityEngine.Events.UnityAction onDraw){
			
			this.title = title;
			this.Draw = onDraw;

		}

		public static implicit operator string(ToolBar tool){
			return tool.title;
		}

	}

	/// <summary>
	/// Index of selected toolbar.
	/// </summary>
	public int toolBarIndex = 0;

	/// <summary>
	/// List of Toolbars
	/// </summary>
	public ToolBar[] toolBars = new ToolBar[]{
		
		new ToolBar("Welcome", WelcomePageContent),
		new ToolBar("Demos", DemosPageContent),
		new ToolBar("Updates", UpdatePageContent),
		new ToolBar("PUN 2", PhotonPUN2),
		new ToolBar("DOCS", Documentations)

	};

	public static Texture2D bannerTexture = null;

	private GUISkin skin;

	private const int windowWidth = 600;
	private const int windowHeight = 550;      

	[MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Welcome Window", false, 10000)]
	public static void OpenWindow(){
		
		GetWindow<RCC_WelcomeWindow>(true);

	}

	void OnEnable(){
		
		titleContent = new GUIContent("Realistic Car Controller");
		maxSize = new Vector2(windowWidth, windowHeight);
		minSize = maxSize;

		InitStyle();

	}

	private void InitStyle(){
		
		if (!skin)
			skin = Resources.Load("RCC_WindowSkin") as GUISkin;

		bannerTexture = (Texture2D)Resources.Load("Editor/RCCBanner", typeof(Texture2D));

	}

	void OnGUI(){
		
		GUI.skin = skin;

		DrawHeader();
		DrawMenuButtons();
		DrawToolBar();
		DrawFooter();

	}

	private void DrawHeader(){
		
		GUILayout.Label(bannerTexture, GUILayout.Height(120));

	}

	private void DrawMenuButtons(){
		
		GUILayout.Space(-10);
		toolBarIndex = GUILayout.Toolbar(toolBarIndex, ToolbarNames());

	}

	#region ToolBars

	public static void WelcomePageContent(){
		
		GUILayout.BeginVertical("window");

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>Thank you for purchasing and using Realistic Car Controller. Please read the documentation before use. Also check out the online documentation for updated info. Have fun :)</b>");
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator ();

		EditorGUILayout.HelpBox("Realistic Car Controller needs configured Tags & Layers and Input Manager in your Project Settings. Importing them will overwrite your Project Settings!", MessageType.Warning, true);
		EditorGUILayout.Separator ();

		if (GUILayout.Button("Import Project Settings (Input Manager, Tags & Layers)"))
			AssetDatabase.ImportPackage(RCC_AssetPaths.projectSettingsPath, true);

		EditorGUILayout.Separator ();

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>If you don't want to overwrite your project settings, you can create a new layer named ''RCC'' and select it in RCC Settings (Tools --> BCG --> RCC --> Edit Settings --> Tags & Layers section.) More info can be found in documentation (First To Do).</b>");
		GUILayout.EndHorizontal();

		EditorGUILayout.Separator ();

		GUILayout.FlexibleSpace();

		GUI.color = Color.red;

		if (GUILayout.Button ("Delete all demo contents from the project")) {
			
			if(EditorUtility.DisplayDialog("Warning", "You are about to delete all demo contents such as vehicle models, vehicle prefabs, vehicle textures, all scenes, scene models, scene prefabs, scene textures!", "Delete", "Cancel"))
				DeleteDemoContent();

		}

		GUI.color = Color.white;

		GUILayout.EndVertical();

	}

	public static void UpdatePageContent(){

		GUILayout.BeginVertical("window");

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>Installed Version: </b>" + RCC_Settings.RCCVersion.ToString());
		GUILayout.EndHorizontal();
		GUILayout.Space(6);

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>1</b>- Always backup your project before updating RCC or any asset in your project!");
		GUILayout.EndHorizontal();
		GUILayout.Space(6);

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>2</b>- If you have own assets such as prefabs, audioclips, models, scripts in RealisticCarControllerV3 folder, keep your own asset outside from RealisticCarControllerV3 folder.");  
		GUILayout.EndHorizontal();
		GUILayout.Space(6);

		GUILayout.BeginHorizontal("box");
		GUILayout.Label("<b>3</b>- Delete RealisticCarControllerV3 folder, and import latest version to your project.");  
		GUILayout.EndHorizontal();
		GUILayout.Space(6);

		if (GUILayout.Button("Check Updates"))
			Application.OpenURL(RCC_AssetPaths.assetStorePath);

		GUILayout.Space(6);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

	}

	public static void DemosPageContent(){

		GUILayout.BeginVertical("box");
		GUILayout.Label("Demo Scenes");

		bool BCGInstalled = false;

		#if BCG_ENTEREXIT
		BCGInstalled = true;
		#endif

		bool photonInstalled = false;

		#if BCG_PHOTON && PHOTON_UNITY_NETWORKING
		photonInstalled = true;
		#endif

		GUILayout.BeginVertical("box");

		if (GUILayout.Button ("RCC City AIO"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_AIO, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC City"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_City, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC City Car Selection"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_CarSelection, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC City Car Selection with Load Next Scene"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_CarSelectionLoadNextScene, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC City Car Selection with Loaded Scene"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_CarSelectionLoadedScene, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC Blank API"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_APIBlank, OpenSceneMode.Single);

		if (GUILayout.Button ("RCC Blank Test Scene"))
			EditorSceneManager.OpenScene (RCC_AssetPaths.demo_BlankMobile, OpenSceneMode.Single);

		GUILayout.EndVertical ();

		GUILayout.BeginVertical("box");

		if (BCGInstalled) {

			if (GUILayout.Button ("RCC City Enter-Exit FPS"))
				EditorSceneManager.OpenScene (RCC_AssetPaths.demo_CityFPS, OpenSceneMode.Single);

			if (GUILayout.Button ("RCC City Enter-Exit TPS"))
				EditorSceneManager.OpenScene (RCC_AssetPaths.demo_CityTPS, OpenSceneMode.Single);

		} else {

			EditorGUILayout.HelpBox ("You have to import latest BCG Shared Assets to your project first.", MessageType.Warning);

			if (GUILayout.Button ("Download and import BCG Shared Assets"))
				AssetDatabase.ImportPackage(RCC_AssetPaths.BCGSharedAssetsPath, true);

		}

		GUILayout.EndVertical ();
		GUILayout.BeginVertical("box");

		if (photonInstalled) {

			if (GUILayout.Button ("RCC City Photon PUN 2"))
				EditorSceneManager.OpenScene (RCC_AssetPaths.demo_PUN2, OpenSceneMode.Single);

		} else {

			EditorGUILayout.HelpBox ("You have to import latest Photon PUN2 to your project first.", MessageType.Warning);

			if (GUILayout.Button ("Download and import Photon PUN2"))
				Application.OpenURL (RCC_AssetPaths.photonPUN2);

		}

		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

	}

	public static void BCGSharedAssetsPageContent(){
		
		GUILayout.BeginVertical("window");

		bool BCGInstalled = false;

		#if BCG_ENTEREXIT
		BCGInstalled = true;
		#endif

		GUILayout.BeginVertical("box");

		if (!BCGInstalled) {

			EditorGUILayout.HelpBox ("You have to import latest BCG Shared Assets to your project first.", MessageType.Warning);

			if (GUILayout.Button ("Download and import BCG Shared Assets"))
				AssetDatabase.ImportPackage(RCC_AssetPaths.BCGSharedAssetsPath, true);

		} else {

			EditorGUILayout.HelpBox ("Found BCG Shared Assets, no need to import it again. You can open FPS/TPS Enter-Exit demo scenes now.", MessageType.Info);

		}

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

	}

	public static void PhotonPUN2(){

		GUILayout.BeginVertical("window");

		GUILayout.BeginVertical("box");

		bool photonInstalled = false;

		#if BCG_PHOTON && PHOTON_UNITY_NETWORKING
		photonInstalled = true;
		#endif

		if (!photonInstalled) {

			EditorGUILayout.HelpBox ("You have to import latest Photon PUN2 to your project first.", MessageType.Warning);

			if (GUILayout.Button ("Download and import Photon PUN2"))
				Application.OpenURL (RCC_AssetPaths.photonPUN2);

		} else {

			EditorGUILayout.HelpBox ("Found Photon PUN2, You can import integration package and open Photon demo scenes now.", MessageType.Info);

            if (GUILayout.Button("Import Photon PUN2 Integration"))
                AssetDatabase.ImportPackage(RCC_AssetPaths.PUN2AssetsPath, true);

        }

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

	}

	public static void Documentations(){
		
		GUILayout.BeginVertical("window");

		GUILayout.BeginVertical("box");
		EditorGUILayout.HelpBox("Latest online documentations for scripts, settings, setup, how to do, and API.", MessageType.Info);

		if (GUILayout.Button("Documentation"))
			Application.OpenURL(RCC_AssetPaths.documentations);

		if (GUILayout.Button("Scripts"))
			Application.OpenURL(RCC_AssetPaths.scripts);

		if (GUILayout.Button("API"))
			Application.OpenURL(RCC_AssetPaths.API);

		if (GUILayout.Button("Youtube Tutorial Videos"))           
			Application.OpenURL(RCC_AssetPaths.YTVideos);

		if (GUILayout.Button("Other Assets"))           
			Application.OpenURL(RCC_AssetPaths.otherAssets);

		GUILayout.EndVertical();

		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

	}

	#endregion

	private string[] ToolbarNames(){
		
		string[] names = new string[toolBars.Length];

		for (int i = 0; i < toolBars.Length; i++)
			names[i] = toolBars[i];
		
		return names;

	}

	private void DrawToolBar(){
		
		GUILayout.BeginArea(new Rect(4, 140, 592, 340));

		toolBars[toolBarIndex].Draw();       

		GUILayout.EndArea();           

		GUILayout.FlexibleSpace();

	}

	private void DrawFooter(){
		
		GUILayout.BeginHorizontal("box");

		GUILayout.EndHorizontal();

	}

	private static void ImportPackage(string package){
		
		try	{
			AssetDatabase.ImportPackage(package, true);
		}catch (Exception){
			Debug.LogError("Failed to import package: " + package);
			throw;
		}

	}

	private static void DeleteDemoContent(){

		Debug.LogWarning("Deleting demo contents...");

		foreach (var item in RCC_AssetPaths.demoAssetPaths)
			FileUtil.DeleteFileOrDirectory (item);

		AssetDatabase.Refresh ();

		Debug.LogWarning("Deleted demo contents!");

	}

}
