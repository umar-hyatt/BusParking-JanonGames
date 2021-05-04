using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
 public Text levelNo;
 public GameObject pausePanel;
 GameObject Bus;
 public Transform[] startPoint;
 public GameObject[] levels;
 
 void Start()
 {
     levels[MainMenu.selectLeveled-1].SetActive(true);
     levelNo.text=MainMenu.selectLeveled.ToString();
     Bus=GameObject.FindGameObjectWithTag("Player");
     Bus.transform.position=startPoint[MainMenu.selectLeveled-1].position;
     Bus.transform.rotation=startPoint[MainMenu.selectLeveled-1].localRotation; 
 }

 public void Next()
 {
         Time.timeScale=1;
     MainMenu.selectLeveled++;
     SceneManager.LoadScene("GamePlay");
 }
 public void Home()
 {
         Time.timeScale=1;
     SceneManager.LoadScene("MainMenu");
 }
  public void Restart()
 {
         Time.timeScale=1;
     SceneManager.LoadScene("GamePlay");
 }
  public void Pause(bool x)
 {
     if(x)
     {
         Time.timeScale=0;
     }
     else
     {
         Time.timeScale=1;
     }
     pausePanel.SetActive(x);
 }
 
}
