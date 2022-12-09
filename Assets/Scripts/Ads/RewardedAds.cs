using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using SgLib;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] GameObject _showAdButton;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    [SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null;
    public static InsertialAds instance;
    public static bool isLoaded = false;

    [SerializeField] int rewardValue;

    void Awake()
    {
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOSAdUnitId
            : _androidAdUnitId;
        StartCoroutine(LoadAd());
    }

    void Start()
    {
        StartCoroutine("showAdButton");
    }


    IEnumerator LoadAd()
    {
        while (!Advertisement.isInitialized)
        {
            yield return null;
        }
        Advertisement.Load(_adUnitId, this);
    }


    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        print("Ad loaded");
        isLoaded = true;
    }
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        isLoaded = false;
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
    }




    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        Advertisement.Show(_adUnitId, this);
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        isLoaded = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            GameManager.Instance.rewardPlayerForAds(rewardValue);
            Debug.Log("Unity Ads Rewarded Ad Completed");
            Advertisement.Load(_adUnitId, this);
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        isLoaded = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Time.timeScale = 0;
        AudioListener.pause = true;
    }
    public void OnUnityAdsShowClick(string adUnitId) { }


    void enableAdButton()
    {
        _showAdButton.SetActive(true);
    }


    void disableAdButton()
    {
        _showAdButton.SetActive(false);
    }


    // courotine to show button only if ad is loaded and player is not playing
    IEnumerator showAdButton()
    {
        while (true)
        {
            if (isLoaded && GameManager.Instance.GameState != GameState.Playing)
            {
                enableAdButton();
            }
            else
            {
                disableAdButton();
            }
            yield return new WaitForSeconds(1f);
        }
    }
}
