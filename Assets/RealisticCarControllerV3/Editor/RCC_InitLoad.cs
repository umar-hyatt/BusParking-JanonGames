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

public class RCC_InitLoad : EditorWindow {

	[InitializeOnLoad]
	public class InitOnLoad {
		
		static InitOnLoad(){
			
			RCC_SetScriptingSymbol.SetEnabled("BCG_RCC", true);
			
			if(!EditorPrefs.HasKey("RCC" + RCC_Settings.RCCVersion.ToString())){
				
				EditorPrefs.SetInt("RCC" + RCC_Settings.RCCVersion.ToString(), 1);
				EditorUtility.DisplayDialog("Regards from BoneCracker Games", "Thank you for purchasing and using Realistic Car Controller. Please read the documentation before use. Also check out the online documentation for updated info. Have fun :)", "Let's get started");

				GetWindow<RCC_WelcomeWindow>(true);

			}

		}

	}

}
