using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//using UnityEngine.SceneManagment;

public class MainMenu : MonoBehaviour
{
public GameObject mainPanel,levelPanel,settingPanel,garagePanel;
public  int selectLeveled;
public Button soundOn,soundOff,steering,Arrow,Tilt;
public Color32 selectedButtonColor;
public void MainPanel(bool x)
{
    SoundManager.instance.Click();
    mainPanel.SetActive(x);
    
}
public void LevelPanel(bool x)
{
    SoundManager.instance.Click();
    levelPanel.SetActive(x);
}
public void SettingPanel(bool x)
{
    SoundManager.instance.Click();
    settingPanel.SetActive(x);
    
}
public void GaragePanel(bool x)
{
    SoundManager.instance.Click();
    garagePanel.SetActive(x);
}

public void SelectLevel(int x)
{
    SoundManager.instance.Click();
    PlayerPrefs.SetInt("SelectedLevel", x);
    selectLeveled=x;
    SceneManager.LoadScene("GamePlay");
    
}
public void Sound(string x)
{
    SoundManager.instance.Click();
    if(x=="On")
    {
    SoundManager.instance.click.mute=false;
    //   ColorChange(soundOn);
    //   ColorReset(soundOff);
    }
    else
    {
    //   ColorChange(soundOff);
    SoundManager.instance.click.mute=true;
    //   ColorReset(soundOn);
    }
}
public void Controller(int x)
{
    SoundManager.instance.Click();
    switch(x)
    {
        case 1:
        {
        RCC_Settings.Instance.mobileController=RCC_Settings.MobileController.TouchScreen;
        break;
        }
        case 2:
        {
        RCC_Settings.Instance.mobileController=RCC_Settings.MobileController.SteeringWheel;
        break;
        }
        case 3:
        {
        RCC_Settings.Instance.mobileController=RCC_Settings.MobileController.Gyro;
        break;
        }
    }

}
void ColorChange(Button b)
{
    ColorBlock color =b.colors;
    color.normalColor=selectedButtonColor;
     b.colors=color;
}
void ColorReset(Button b)
{
    ColorBlock color =b.colors;
    color.normalColor=Color.white;
     b.colors=color;
}
}
