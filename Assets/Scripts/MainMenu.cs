using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
public GameObject mainPanel,levelPanel,settingPanel,garagePanel,car1,car2;
public static int selectLeveled;
public static int selectedBus;
public Button soundOn,soundOff,steering,Arrow,Tilt;
public Color32 selectedButtonColor;
void Start()
{
    AdsScript.instance.ShowSmartBanner();
}
public void MainPanel(bool x)
{
    SoundManager.instance.Click();
    mainPanel.SetActive(x);
}
public void LevelPanel(bool x)
{
    if(x)
    {
        if(car1.activeInHierarchy)
        {
        selectedBus=1;
        }
        else
        {
        selectedBus=2;
        }
    }
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
    if(x)
    {
      AdsScript.instance.ShowInterstitial();
    }
    SoundManager.instance.Click();
    garagePanel.SetActive(x);
}

public void SelectLevel(int x)
{
    SoundManager.instance.Click();
    AdsScript.instance.ShowInterstitial();
    PlayerPrefs.SetInt("SelectedLevel", x);
    selectLeveled=x;
    SceneManager.LoadScene("GamePlay");
    
}
public void NextCar()
{
    SoundManager.instance.Click();
    if(car1.activeInHierarchy)
    {
        car1.SetActive(false);
        car2.SetActive(true);
    }
    else
    {
       car1.SetActive(true);
       car2.SetActive(false);
    }
}
public void LastCar()
{
    SoundManager.instance.Click();
     if(car1.activeInHierarchy)
    {
        car1.SetActive(false);
        car2.SetActive(true);
    }
    else
    {
       car1.SetActive(true);
       car2.SetActive(false);
    }
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
