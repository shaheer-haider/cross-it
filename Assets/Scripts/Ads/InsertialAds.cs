using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Advertisements;

public class InsertialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] string _iOsAdUnitId = "Interstitial_iOS";

    // skip add this time using boolean
    public static bool isAddRunning = false;
    string _adUnitId;
    public bool isLoaded = false;
    public static InsertialAds instance;

    public int addLessGameCount = 0;

    public int playAdAfterGameOvers;

    void Awake()
    {
        addLessGameCount = PlayerPrefs.GetInt("addLessGameCount", 0);
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;
        StartCoroutine(LoadAd());
    }



    // courotine to load add after initilize
    IEnumerator LoadAd()
    {
        if (!isLoaded)
        {
            while (!Advertisement.isInitialized)
            {
                print("Waiting for ads to initialize");
                yield return null;
            }

            Advertisement.Load(_adUnitId, this);
        }
    }


    public void ShowAd()
    {
        if (addLessGameCount > playAdAfterGameOvers)
        {
            Advertisement.Show(_adUnitId, this);
        }
        addLessGameCount++;
        PlayerPrefs.SetInt("addLessGameCount", addLessGameCount);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        print("Ad loaded: " + adUnitId);
        isLoaded = true;
    }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        isLoaded = false;
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
    }


    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Time.timeScale = 1;
        AudioListener.pause = false;
        isLoaded = false;
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        print("Ad started: " + adUnitId);
        Time.timeScale = 0;
        AudioListener.pause = true;
    }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        addLessGameCount = 0;
        PlayerPrefs.SetInt("addLessGameCount", 0);
        Time.timeScale = 1;
        AudioListener.pause = false;
        isLoaded = false;
    }
}
