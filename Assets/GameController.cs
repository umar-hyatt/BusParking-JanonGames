using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
 GameObject Bus;
 Transform[] startPoint;
 
 void Start()
 {
     Bus=GameObject.FindGameObjectWithTag("Player");
     Bus.transform.position=startPoint[MainMenu.selectLeveled].position;
     Bus.transform.rotation=startPoint[MainMenu.selectLeveled].rotation;
 }
}
