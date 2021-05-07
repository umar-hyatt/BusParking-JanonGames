using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text levelNo;
    public GameObject pausePanel;
    public GameObject Bus;
    public Transform[] startPoint;
    public GameObject[] levels;
    MainMenu mainMenu;
    int selectedLevel;
    void Start()
    {
        mainMenu = (MainMenu)FindObjectOfType(typeof(MainMenu));
        selectedLevel = PlayerPrefs.GetInt("SelectedLevel");
        levels[selectedLevel - 1].SetActive(true);
        levelNo.text = selectedLevel.ToString();
       // Bus = GameObject.FindGameObjectWithTag("Player");
        Bus.transform.position = startPoint[selectedLevel - 1].position;
        Bus.transform.rotation = startPoint[selectedLevel - 1].localRotation;
    }

    public void Next()
    {
        Time.timeScale = 1;
        mainMenu.selectLeveled++;
        SceneManager.LoadScene("GamePlay");
    }
    public void Home()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("GamePlay");
    }
    public void Pause(bool x)
    {
        if (x)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        pausePanel.SetActive(x);
    }

}
