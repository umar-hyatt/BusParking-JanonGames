using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
public GameObject mainPanel,levelPanel,settingPanel,garagePanel;
public static int selectLeveled;
public Button soundOn,soundOff,steering,Arrow,Tilt;
public Color32 selectedButtonColor;
public void MainPanel(bool x)
{
    mainPanel.SetActive(x);
}
public void LevelPanel(bool x)
{
    levelPanel.SetActive(x);
}
public void SettingPanel(bool x)
{
    settingPanel.SetActive(x);
}
public void GaragePanel(bool x)
{
    garagePanel.SetActive(x);
}

public void SelectLevel(int x)
{
    selectLeveled=x;
}
public void Sound(string x)
{
    if(x=="On")
    {
    //   ColorChange(soundOn);
    //   ColorReset(soundOff);
    }
    else
    {
    //   ColorChange(soundOff);
    //   ColorReset(soundOn);
    }
}
public void Controller(int x)
{
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
