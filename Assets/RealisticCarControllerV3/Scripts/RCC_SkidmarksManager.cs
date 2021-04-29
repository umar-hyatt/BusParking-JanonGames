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

public class RCC_SkidmarksManager : MonoBehaviour {

	public RCC_Skidmarks[] skidmarks;
	public int[] skidmarksIndexes;
	private int _lastGroundIndex = 0;

	void Start () {
		
		skidmarks = new RCC_Skidmarks[RCC_GroundMaterials.Instance.frictions.Length];
		skidmarksIndexes = new int[skidmarks.Length];

		for (int i = 0; i < skidmarks.Length; i++) {
			
			skidmarks [i] = Instantiate (RCC_GroundMaterials.Instance.frictions [i].skidmark, Vector3.zero, Quaternion.identity);
			skidmarks [i].transform.name = skidmarks[i].transform.name + "_" + RCC_GroundMaterials.Instance.frictions[i].groundMaterial.name;
			skidmarks [i].transform.SetParent (transform, true);

		}
		
	}
	
	// Function called by the wheels that is skidding. Gathers all the information needed to
	// create the mesh later. Sets the intensity of the skidmark section b setting the alpha
	// of the vertex color.
	public int AddSkidMark ( Vector3 pos ,   Vector3 normal ,   float intensity ,   int lastIndex, int groundIndex  ){

		if (_lastGroundIndex != groundIndex){

			_lastGroundIndex = groundIndex;
			return -1;

		}

		skidmarksIndexes[groundIndex] = skidmarks [groundIndex].AddSkidMark (pos, normal, intensity, lastIndex);
		
		return skidmarksIndexes[groundIndex];

	}

	// Function called by the wheels that is skidding. Gathers all the information needed to
	// create the mesh later. Sets the intensity of the skidmark section b setting the alpha
	// of the vertex color.
	public int AddSkidMark ( Vector3 pos ,   Vector3 normal ,   float intensity ,   int lastIndex, int groundIndex, float width){

		if (_lastGroundIndex != groundIndex){

			_lastGroundIndex = groundIndex;
			return -1;

		}

		skidmarks [groundIndex].markWidth = width;
		skidmarksIndexes[groundIndex] = skidmarks [groundIndex].AddSkidMark (pos, normal, intensity, lastIndex);

		return skidmarksIndexes[groundIndex];

	}

}
