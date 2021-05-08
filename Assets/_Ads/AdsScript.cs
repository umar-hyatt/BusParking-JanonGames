using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Advertisements;

public class AdsScript : MonoBehaviour
{
    public static AdsScript instance;
    public bool isTest;
    string AppId, BannerId, InterstitialID;
    public string MainAppId, MainBannerId, MainInterstitialID,UnityId;
    BannerView SmartBanner, TopLeftBanner, TopRightBanner, TopRightLargeBanner, TopCenterBanner,BottomLeftBanner,
        BottomLeftLargeBanner, BottomRightBanner, BottomRightLargeBanner, BottomCenterBanner, TopLeftLargeBanner;
    InterstitialAd Interstitial;
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (isTest)
        {
            BannerId = "ca-app-pub-3940256099942544/6300978111";
            InterstitialID = "ca-app-pub-3940256099942544/1033173712";
            AppId = "ca-app-pub-3940256099942544~3347511713";
        }
        else
        {
            BannerId = MainBannerId;
            InterstitialID = MainInterstitialID;
            AppId = MainAppId;
        }
        MobileAds.Initialize(AppId);
        LoadAllAds();
        Advertisement.Initialize(UnityId, isTest);
    }

    public void RequestSmartBanner()
    {
        SmartBanner = new BannerView(BannerId, AdSize.SmartBanner, AdPosition.Top);
        AdRequest requestSmartBanner = new AdRequest.Builder().Build();
        SmartBanner.LoadAd(requestSmartBanner);
        SmartBanner.Hide();
    }
    public void RequestTopLeftBanner()
    {
        TopLeftBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.TopLeft);
        AdRequest requestTopLeftBanner = new AdRequest.Builder().Build();
        TopLeftBanner.LoadAd(requestTopLeftBanner);
        TopLeftBanner.Hide();
    }
    public void RequestTopRightBanner()
    {
        TopRightBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.TopRight);
        AdRequest requestTopRightBanner = new AdRequest.Builder().Build();
        TopRightBanner.LoadAd(requestTopRightBanner);
        TopRightBanner.Hide();
    }
    public void RequestTopCenterBanner()
    {
        TopCenterBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.Top);
        AdRequest requestTopCenterBanner = new AdRequest.Builder().Build();
        TopCenterBanner.LoadAd(requestTopCenterBanner);
        TopCenterBanner.Hide();
    }
    public void RequestBottomLeftBanner()
    {
        BottomLeftBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.BottomLeft);
        AdRequest requestBottomLeftBanner = new AdRequest.Builder().Build();
        BottomLeftBanner.LoadAd(requestBottomLeftBanner);
        BottomLeftBanner.Hide();
    }
    public void RequestBottomRightBanner()
    {
        BottomRightBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.BottomRight);
        AdRequest requestBottomRightBanner = new AdRequest.Builder().Build();
        BottomRightBanner.LoadAd(requestBottomRightBanner);
        BottomRightBanner.Hide();
    }
    public void RequestBottomCenterBanner()
    {
        BottomCenterBanner = new BannerView(BannerId, AdSize.Banner, AdPosition.Bottom);
        AdRequest requestBottomCenterBanner = new AdRequest.Builder().Build();
        BottomCenterBanner.LoadAd(requestBottomCenterBanner);
        BottomCenterBanner.Hide();
    }
    /// <summary>
    /// Large Banner load
    /// </summary>
    public void RequestTopLeftLargeBanner()
    {
        TopLeftLargeBanner = new BannerView(BannerId, AdSize.MediumRectangle, AdPosition.TopLeft);
        AdRequest requestTopLeftLargeBanner = new AdRequest.Builder().Build();
        TopLeftLargeBanner.LoadAd(requestTopLeftLargeBanner);
        TopLeftLargeBanner.Hide();
    }
    public void RequestTopRightLargeBanner()
    {
        TopRightLargeBanner = new BannerView(BannerId, AdSize.MediumRectangle, AdPosition.TopRight);
        AdRequest requestTopRightLargeBanner = new AdRequest.Builder().Build();
        TopRightLargeBanner.LoadAd(requestTopRightLargeBanner);
        TopRightLargeBanner.Hide();
    }
    public void RequestBottomLeftLargeBanner()
    {
        BottomLeftLargeBanner = new BannerView(BannerId, AdSize.MediumRectangle, AdPosition.BottomLeft);
        AdRequest requestBottomLeftLargeBanner = new AdRequest.Builder().Build();
        BottomLeftLargeBanner.LoadAd(requestBottomLeftLargeBanner);
        BottomLeftLargeBanner.Hide();
    }
    public void RequestBottomRightLargeBanner()
    {
        BottomRightLargeBanner = new BannerView(BannerId, AdSize.MediumRectangle, AdPosition.BottomRight);
        AdRequest requestBottomRightLargeBanner = new AdRequest.Builder().Build();
        BottomRightLargeBanner.LoadAd(requestBottomRightLargeBanner);
        BottomRightLargeBanner.Hide();
    }
    /// <summary>
    /// call Interstitial
    /// </summary>
    public void RequestInterstitial()
    {
        Interstitial = new InterstitialAd(InterstitialID);
        AdRequest requestInterstitial = new AdRequest.Builder().Build();
        Interstitial.LoadAd(requestInterstitial);
    }
    /// <summary>
    /// show ads complete fail pause
    /// </summary>
    public void LevelComplete()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Advertisement.IsReady())
                Advertisement.Show();
            else
                ShowInterstitial();
        }
    }
    public void LevelPause()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Advertisement.IsReady())
                Advertisement.Show();
        }
    }
    public void LevelFail()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Advertisement.IsReady())
                Advertisement.Show();
            else
                ShowInterstitial();
        }
    }
    /// <summary>
    /// show banner ads
    /// </summary>
    public void ShowTopLeftBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            TopLeftBanner.Show();
        }
    } 
    public void ShowTopRighttBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            TopRightBanner.Show();
        }
    }
    public void ShowTopCentertBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            TopCenterBanner.Show();
        }
    } 
    public void ShowBottomLeftBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            BottomLeftBanner.Show();
        }
    } 
    public void ShowBottomRighttBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            BottomRightBanner.Show();
        }
    }
    public void ShowBottomCentertBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            BottomCenterBanner.Show();
        }
    } 
    public void ShowSmartBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            SmartBanner.Show();
        }
    }
    /// <summary>
    /// show large banner
    /// </summary>
    public void ShowTopLeftLargeBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            TopLeftLargeBanner.Show();
        }
    }
    public void ShowTopRightLargeBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            TopRightLargeBanner.Show();
        }
    }
    public void ShowBottomLeftLargeBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            BottomLeftLargeBanner.Show();
        }
    }
    public void ShowBottomRightLargeBanner()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            BottomRightLargeBanner.Show();
        }
    }
    /// <summary>
    /// show Interstitial
    /// </summary>
    public void ShowInterstitial()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Interstitial.IsLoaded())
            {
                Interstitial.Show();
                RequestInterstitial();
            }
            else
            {
                RequestInterstitial();
            }
        }
        //RequestInterstitial();

    }
    public void DestroyInterstitial()
    {
        if (PlayerPrefs.GetInt("RemoveAD") == 0)
        {
            Interstitial.Destroy();
        }
    }
    /// <summary>
    /// remove banner
    /// </summary>
    public void HideSmartBanner()
    {
        SmartBanner.Hide();
    }
    public void HideTopLeftBanner()
    {
        TopLeftBanner.Hide();
    }
    public void HideTopRightBanner()
    {
        TopRightBanner.Hide();
    } 
    public void HideTopCenterBanner()
    {
        TopCenterBanner.Hide();
    }
    public void HideBottomLeftBanner()
    {
        BottomLeftBanner.Hide();
    }
    public void HideBottomRightBanner()
    {
        BottomRightBanner.Hide();
    }  
    public void HideBottomCenterBanner()
    {
        BottomCenterBanner.Hide();
    }
    /// <summary>
    /// remove large banner
    /// </summary>
    public void HideTopLeftLargeBanner()
    {
        TopLeftLargeBanner.Hide();
    }
    public void HideTopRightLargeBanner()
    {
        TopRightLargeBanner.Hide();
    }
    public void HideBottomLeftLargeBanner()
    {
        BottomLeftLargeBanner.Hide();
    }
    public void HideBottomRightLargeBanner()
    {
        BottomRightLargeBanner.Hide();
    }
    /// <summary>
    /// load all ads
    /// </summary>
    public void LoadAllAds()
    {
        RequestSmartBanner();
     //   RequestTopLeftBanner();
        RequestTopRightBanner();
      //  RequestTopCenterBanner();
     //   RequestBottomLeftBanner();
    //    RequestBottomRightBanner();
    //    RequestBottomCenterBanner();
      //  RequestTopLeftLargeBanner();
     //   RequestTopRightLargeBanner();
    //    RequestBottomLeftLargeBanner();
        RequestBottomRightLargeBanner();
        RequestInterstitial();
    }
    public void RemoveAllAds()
    {
        HideSmartBanner();
        HideTopLeftBanner();
        HideTopRightBanner();
        HideTopCenterBanner();
        HideBottomLeftBanner();
        HideBottomRightBanner();
        HideBottomCenterBanner();
        HideTopLeftLargeBanner();
        HideTopRightLargeBanner();
        HideBottomLeftLargeBanner();
        HideBottomRightLargeBanner();
    }
    public void AdmobThenUnity() 
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Interstitial.IsLoaded())
            {
                Interstitial.Show();
                RequestInterstitial();
            }
            else if (Advertisement.IsReady())
            {
                Advertisement.Show();
            }
            else
            {
                RequestInterstitial();
            }
        }

    }
    public void UnityThenAdmob()
    {
        if (PlayerPrefs.GetInt("RemoveAds") != 1)
        {
            if (Advertisement.IsReady())
                Advertisement.Show();
            else
                ShowInterstitial();
        }
    }
}