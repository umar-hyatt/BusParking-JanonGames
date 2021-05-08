using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text levelNo;
    public GameObject pausePanel;
    public GameObject Bus,bus2;
    public Transform[] startPoint;
    public GameObject[] levels;
    MainMenu mainMenu;
    int selectedLevel;
    void Awake()
    {
        mainMenu = (MainMenu)FindObjectOfType(typeof(MainMenu));
    }
    void Start()
    {
        Debug.Log(MainMenu.selectedBus);
        if(MainMenu.selectedBus==1)
        {
            Bus.SetActive(true);
        }
        else
        {
            bus2.SetActive(true);
        }
        selectedLevel = PlayerPrefs.GetInt("SelectedLevel");
        levels[selectedLevel - 1].SetActive(true);
        levelNo.text = selectedLevel.ToString();
       // Bus = GameObject.FindGameObjectWithTag("Player");
        Bus.transform.position = startPoint[selectedLevel - 1].position;
        Bus.transform.rotation = startPoint[selectedLevel - 1].localRotation;
        bus2.transform.position = startPoint[selectedLevel - 1].position;
        bus2.transform.rotation = startPoint[selectedLevel - 1].localRotation;
    }

    public void Next()
    {
        AdsScript.instance.AdmobThenUnity();
        Time.timeScale = 1;
        MainMenu.selectLeveled++;
        PlayerPrefs.SetInt("SelectedLevel",PlayerPrefs.GetInt("SelectedLevel")+1);
        SceneManager.LoadScene("GamePlay");
    }
    public void Home()
    {
        AdsScript.instance.AdmobThenUnity();
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    public void Restart()
    {
        AdsScript.instance.AdmobThenUnity();
        Time.timeScale = 1;
        SceneManager.LoadScene("GamePlay");
    }
    public void Pause(bool x)
    {
        if (x)
        {
            AdsScript.instance.UnityThenAdmob();
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        pausePanel.SetActive(x);
    }

}
