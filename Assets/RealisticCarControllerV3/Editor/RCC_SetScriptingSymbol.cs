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

public class RCC_SetScriptingSymbol{
	
	private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[]{

		BuildTargetGroup.Standalone,
		BuildTargetGroup.Android,
		BuildTargetGroup.iOS,
		BuildTargetGroup.WebGL,
		#if !UNITY_2019_3_OR_NEWER
		BuildTargetGroup.Facebook,
		#endif
		BuildTargetGroup.XboxOne,
		BuildTargetGroup.PS4,
		BuildTargetGroup.tvOS,

	};

	public static void SetEnabled(string defineName, bool enable)	{
		
		//Debug.Log("setting "+defineName+" to "+enable);
		foreach (var group in buildTargetGroups){

			var defines = GetDefinesList(group);

			if (enable){
				
				if (defines.Contains(defineName))
					return;
				
				defines.Add(defineName);

			}else{
				
				if (!defines.Contains(defineName))
					return;
				
				while (defines.Contains (defineName)) {
					defines.Remove (defineName);
				}
				

			}

			string definesString = string.Join(";", defines.ToArray());
			PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);

		}

	}

	public static List<string> GetDefinesList(BuildTargetGroup group){
		
		return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));

	}

}
