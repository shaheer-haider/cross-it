using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;
using SgLib;

#if EASY_MOBILE
using EasyMobile;
#endif

public class UIManager : MonoBehaviour
{
    public static bool firstLoad = true;

    [Header("Object References")]
    public GameManager gameManager;
    public CameraController camController;
    public DailyRewardController dailyRewardController;

    public GameObject mainCanvas;
    public GameObject settingsCanvas;
    public GameObject storeCanvas;

    public GameObject blackPanel;
    public GameObject header;
    public Text score;
    public Text bestScore;
    public Text gold;
    public Text title;
    public GameObject tapToStart;
    public GameObject characterSelectBtn;
    public GameObject menuButtons;
    public Text dailyRewardBtnText;
    public GameObject dailyRewardBtn;
    public GameObject rewardUI;
    public GameObject soundOffBtn;
    public GameObject soundOnBtn;
    public GameObject musicOnBtn;
    public GameObject musicOffBtn;

    [Header("Premium Features Only")]
    // public GameObject watchForCoinsBtn;
    public GameObject leaderboardBtn;
    public GameObject iapPurchaseBtn;
    public GameObject removeAdsBtn;
    public GameObject restorePurchaseBtn;

    [Header("Sharing-Specific")]
    public GameObject shareUI;
    public ShareUIController shareUIController;

    Animator scoreAnimator;
    Animator dailyRewardAnimator;
    bool isWatchAdsForCoinBtnActive;

    public static UIManager Instance;



    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }





    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
        ScoreManager.ScoreUpdated -= OnScoreUpdated;
    }

    // Use this for initialization
    void Start()
    {
        scoreAnimator = score.GetComponent<Animator>();
        dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

        Reset();
        ShowStartUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCanvas.activeSelf)
        {
            score.text = ScoreManager.Instance.Score.ToString();
            bestScore.text = ScoreManager.Instance.HighScore.ToString();
            gold.text = CoinManager.Instance.Coins.ToString();

            if (!DailyRewardController.Instance.disable && dailyRewardBtn.gameObject.activeSelf)
            {
                TimeSpan timeToReward = DailyRewardController.Instance.TimeUntilReward;

                if (timeToReward <= TimeSpan.Zero)
                {
                    dailyRewardBtnText.text = "GRAB YOUR REWARD!";
                    dailyRewardAnimator.SetTrigger("activate");
                }
                else
                {
                    dailyRewardBtnText.text = string.Format("REWARD IN {0:00}:{1:00}:{2:00}", timeToReward.Hours, timeToReward.Minutes, timeToReward.Seconds);
                    dailyRewardAnimator.SetTrigger("deactivate");
                }
            }
        }

        if (settingsCanvas.activeSelf)
        {
            UpdateMuteButtons();
            UpdateMusicButtons();
        }
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing)
        {              
            ShowGameUI();
        }
        else if (newState == GameState.PreGameOver)
        {
            // Before game over, i.e. game potentially will be recovered
        }
        else if (newState == GameState.GameOver)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
    }

    void OnScoreUpdated(int newScore)
    {
        scoreAnimator.Play("NewScore");
    }

    void Reset()
    {
        mainCanvas.SetActive(true);
        settingsCanvas.SetActive(false);

        blackPanel.SetActive(false);
        header.SetActive(false);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(false);
        tapToStart.SetActive(false);
        characterSelectBtn.SetActive(false);
        menuButtons.SetActive(false);
        dailyRewardBtn.SetActive(false);
        settingsCanvas.SetActive(false);

        // Enable or disable premium stuff
        bool enablePremium = PremiumFeaturesManager.Instance.enablePremiumFeatures;
        leaderboardBtn.SetActive(enablePremium);
        iapPurchaseBtn.SetActive(enablePremium);
        removeAdsBtn.SetActive(enablePremium);
        restorePurchaseBtn.SetActive(enablePremium);

        // Hide Share screnenshot by default
        shareUI.SetActive(false);

        // These premium feature buttons are hidden by default
        // and shown when certain criteria are met (e.g. rewarded ad is loaded)
        // watchForCoinsBtn.gameObject.SetActive(false);
    }

    public void ShowStartUI()
    {
        mainCanvas.SetActive(true);
        settingsCanvas.SetActive(false);

        header.SetActive(true);
        title.gameObject.SetActive(true);
        tapToStart.SetActive(true);
        characterSelectBtn.SetActive(true);    
    }

    public void ShowGameUI()
    {
        header.SetActive(true);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(true);
        tapToStart.SetActive(false);
        characterSelectBtn.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        blackPanel.SetActive(true);
        header.SetActive(true);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(true);
        tapToStart.SetActive(false);
        menuButtons.SetActive(true);

        // watchForCoinsBtn.gameObject.SetActive(false);
        settingsCanvas.SetActive(false);

        // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
        #if EASY_MOBILE
        if (gameManager.enablePremiumFeatures && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        {
            // watchForCoinsBtn.SetActive(true);
            // watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
        }
        else
        {
            // watchForCoinsBtn.SetActive(false);
        }
        #endif

        // Not showing the daily reward button if the feature is disabled
        if (!DailyRewardController.Instance.disable)
        {
            dailyRewardBtn.SetActive(true);
        }

        if (IsPremiumFeaturesEnabled())
            ShowShareUI();


        // Blur the background
    }

    public void ShowSettingsUI()
    {
        mainCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }

    public void HideSettingsUI()
    {
        mainCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
    }

    public void ShowStoreUI()
    {
        mainCanvas.SetActive(false);
        storeCanvas.SetActive(true);
    }

    public void HideStoreUI()
    {
        mainCanvas.SetActive(true);
        storeCanvas.SetActive(false);
    }

    public void StartGame()
    {
        gameManager.StartGame();
    }

    public void EndGame()
    {
        gameManager.GameOver();
    }

    public void RestartGame()
    {
        gameManager.RestartGame(0.2f);
    }

    public void WatchRewardedAdForCoins()
    {
        #if EASY_MOBILE
        // Hide the button
        // watchForCoinsBtn.SetActive(false);

        AdDisplayer.CompleteRewardedAdToEarnCoins += OnCompleteRewardedAdToEarnCoins;
        AdDisplayer.Instance.ShowRewardedAdToEarnCoins();
        #endif
    }

    void OnCompleteRewardedAdToEarnCoins()
    {
        #if EASY_MOBILE
        // Unsubscribe
        AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

        // Give the coins!
        ShowRewardUI(AdDisplayer.Instance.rewardedCoins);
        #endif
    }

    public void GrabDailyReward()
    {
        if (DailyRewardController.Instance.TimeUntilReward <= TimeSpan.Zero)
        {
            float reward = UnityEngine.Random.Range(dailyRewardController.minRewardValue, dailyRewardController.maxRewardValue);

            // Round the number and make it mutiplies of 5 only.
            int roundedReward = (Mathf.RoundToInt(reward) / 5) * 5;

            // Show the reward UI
            ShowRewardUI(roundedReward);

            // Update next time for the reward
            DailyRewardController.Instance.SetNextRewardTime(dailyRewardController.rewardIntervalHours, dailyRewardController.rewardIntervalMinutes, dailyRewardController.rewardIntervalSeconds);
        }
    }

    public void ShowRewardUI(int reward)
    {
        rewardUI.SetActive(true);
        rewardUI.GetComponent<RewardUIController>().Reward(reward);
    }

    public void HideRewardUI()
    {
        rewardUI.SetActive(false);
    }

    public void ShowLeaderboardUI()
    {
        #if EASY_MOBILE
        if (GameServices.IsInitialized())
        {
            GameServices.ShowLeaderboardUI();
        }
        else
        {
            #if UNITY_IOS
            NativeUI.Alert("Service Unavailable", "The user is not logged in to Game Center.");
            #elif UNITY_ANDROID
            GameServices.Init();
            #endif
        }
        #endif
    }

    public void PurchaseRemoveAds()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.Purchase(InAppPurchaser.Instance.removeAds);
        #endif
    }

    public void RestorePurchase()
    {
        #if EASY_MOBILE
        InAppPurchaser.Instance.RestorePurchase();
        #endif
    }

    public void ShowShareUI()
    {
        StartCoroutine(SetUSphareUI());
    }
    IEnumerator SetUSphareUI()
    {
        yield return new WaitForSeconds(0.4F);
        if (!ScreenshotSharer.Instance.disableSharing)
        {
            Texture2D texture = ScreenshotSharer.Instance.CapturedScreenshot;
            shareUIController.ImgTex = texture;

#if EASY_MOBILE
            AnimatedClip clip = ScreenshotSharer.Instance.RecordedClip;
            shareUIController.AnimClip = clip;
#endif
            shareUI.SetActive(true);

        }
    }

    public void HideShareUI()
    {
        shareUI.SetActive(false);
    }

    public void ToggleSound()
    {
        SoundManager.Instance.ToggleMute();
    }

    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void SelectCharacter()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void RateApp()
    {
        Utilities.RateApp();
    }

    public void OpenTwitterPage()
    {
        Utilities.OpenTwitterPage();
    }

    public void OpenFacebookPage()
    {
        Utilities.OpenFacebookPage();
    }

    public void ButtonClickSound()
    {
        Utilities.ButtonClickSound();
    }

    void UpdateMuteButtons()
    {
        if (SoundManager.Instance.IsMuted())
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
        }
        else
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
        }
    }

    void UpdateMusicButtons()
    {
        if (SoundManager.Instance.IsMusicOff())
        {
            musicOffBtn.gameObject.SetActive(true);
            musicOnBtn.gameObject.SetActive(false);
        }
        else
        {
            musicOffBtn.gameObject.SetActive(false);
            musicOnBtn.gameObject.SetActive(true);
        }
    }

    bool IsPremiumFeaturesEnabled()
    {
        return PremiumFeaturesManager.Instance != null && PremiumFeaturesManager.Instance.enablePremiumFeatures;
    }

}
