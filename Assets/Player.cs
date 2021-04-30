using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
public  GameObject levelCompPanel,levelFailpanel;
public Text timerTxt;
int levelTime;
 void Start()
{
   if(MainMenu.selectLeveled==1)
   {
     levelTime=30;
   }
   else
   {
     levelTime=50;
   }
   StartCoroutine(Timer(levelTime));
}
void OnTriggerEnter(Collider other)
{
  if(other.tag=="Parking")
  {
     LevelComplete();
  }
}
 void LevelComplete()
{
  levelCompPanel.SetActive(true);
}
void LevelFailed()
{
levelFailpanel.SetActive(true);
}

IEnumerator Timer(int T)
{
    while(T>0)
    {
    timerTxt.text=T.ToString();
    T--;
    yield return new WaitForSeconds(1f);
    }
    LevelFailed();
}
}
