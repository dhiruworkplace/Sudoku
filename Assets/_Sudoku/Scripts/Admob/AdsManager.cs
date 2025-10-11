using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System;
using UnityEngine;
using UnityEngine.Advertisements;

namespace ArtboxGames
{
    public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public static AdsManager Instance;

        private BannerView bannerView;
        private AdRequest banner_request;

        private InterstitialAd interstitial;
        private AdRequest interstitial_request;

        private RewardedAd rewardedAd;
        private AdRequest rewarded_request;

        private AppOpenAd appOpenAd;
        private bool showAppOpen = true;

        private string gameId = "3796719";
        private string interstitialAdUnitId_Unity = "Android_Interstitial";
        private string rewardAdUnitId_Unity = "Android_Rewarded";

        //private string appID = "ca-app-pub-6163322720080156~7424761582";
        private string bannerUnitId = "ca-app-pub-6163322720080156/5920108221";
        private string openAdUnitId = "ca-app-pub-6163322720080156/7348485014";
        private string interstitialAdUnitId = "ca-app-pub-6163322720080156/9667781549";
        private string rewardAdUnitId = "ca-app-pub-6163322720080156/2380511984";

        private bool giveReward = false;
        private int giftAmount = 5; // 3 hint
        public bool testAd = false;

        void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        // Start is called before the first frame update
        public void Start()
        {
            if (testAd)
            {
                bannerUnitId = "ca-app-pub-3940256099942544/6300978111";
                openAdUnitId = "ca-app-pub-3940256099942544/9257395921";
                interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
                rewardAdUnitId = "ca-app-pub-3940256099942544/5224354917";
            }

            Advertisement.Initialize(gameId, false, this);

            // Listen to application foreground and background events.
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus =>
            {
                Debug.Log("=== MobileAds init success");
                RequestBanner();
                RequestInterstitial();
                RequestRewarded();
                LoadAppOpenAd();
            });
        }

        private void Update()
        {
            if (giveReward)
            {
                giveReward = false;
                GiveReward();
            }
        }

        private void GiveReward()
        {
            FindFirstObjectByType<GameManager>().ShowMessage("Congratulations!\nYou got " + giftAmount + " hints");
            CurrencyManager.Instance.Give("hints", giftAmount);
        }

        private void RequestBanner()
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
                bannerView = null;
            }

            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(bannerUnitId, AdSize.Banner, AdPosition.Bottom);

            // Create an empty ad request.
            banner_request = new AdRequest();

            // Raised when an ad is loaded into the banner view.
            bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("Banner view loaded an ad with response : "
                    + this.bannerView.GetResponseInfo());
            };
            // Raised when an ad fails to load into the banner view.
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError("Banner view failed to load an ad with error : "
                    + error);
            };
            // Raised when the ad is estimated to have earned money.
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Banner view paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            bannerView.OnAdClicked += () =>
            {
                Debug.Log("Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Banner view full screen content closed.");
            };

            // Load the banner with the request.
            bannerView.LoadAd(banner_request);
        }

        private void RequestInterstitial()
        {
            if (interstitial != null)
            {
                interstitial.Destroy();
                interstitial = null;
            }

            // create our request used to load the ad.
            interstitial_request = new AdRequest();
            // Initialize an InterstitialAd.
            InterstitialAd.Load(interstitialAdUnitId, interstitial_request, (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitial = ad;
            });
        }

        private void RequestRewarded()
        {
            // Clean up the old ad before loading a new one.
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }

            // create our request used to load the ad.
            rewarded_request = new AdRequest();
            // send the request to load the ad.
            RewardedAd.Load(rewardAdUnitId, rewarded_request,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    rewardedAd = ad;
                });
        }

        public void LoadAppOpenAd()
        {
            // Clean up the old ad before loading a new one.
            if (appOpenAd != null)
            {
                appOpenAd.Destroy();
                appOpenAd = null;
            }

            Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            AdRequest adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(openAdUnitId, adRequest,
                (AppOpenAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("app open ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("App open ad loaded with response : "
                              + ad.GetResponseInfo());

                    appOpenAd = ad;
                    RegisterEventHandlers(ad);

                    if (showAppOpen)
                    {
                        showAppOpen = false;
                        ShowAppOpenAd();
                    }
                });
        }

        private void RegisterEventHandlers(AppOpenAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("App open ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("App open ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                Debug.Log("App open ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("App open ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("App open ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                LoadAppOpenAd();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("App open ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        public bool IsAdAvailable
        {
            get
            {
                return appOpenAd != null;
            }
        }

        public void ShowAppOpenAd()
        {
            if (appOpenAd != null && appOpenAd.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                appOpenAd.Show();
            }
            else
            {
                Debug.LogError("App open ad is not ready yet.");
            }
        }

        // Returns an ad request with custom ad targeting.
        private AdRequest CreateAdRequest()
        {
            return new AdRequest();
        }

        private void OnAppStateChanged(AppState state)
        {
            // Display the app open ad when the app is foregrounded.
            //Debug.Log("======= App State is " + state);
            if (state == AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        }

        public void OnInitializationComplete()
        {
            Debug.Log("=== OnInitializationComplete");

            LoadUnityInterstitialAd();
            LoadUnityRewardedAds();
        }

        public void LoadUnityInterstitialAd()
        {
            // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
            Advertisement.Load(interstitialAdUnitId_Unity, this);
        }

        // Show the loaded content in the Ad Unit: 
        public void ShowUnityInterstitialAd()
        {
            // Note that if the ad content wasn't previously loaded, this method will fail
            Advertisement.Show(interstitialAdUnitId_Unity, this);
        }

        public void LoadUnityRewardedAds()
        {
            Advertisement.Load(rewardAdUnitId_Unity, this);
        }

        public bool ShowUnityRewardedVideo()
        {
            Advertisement.Show(rewardAdUnitId_Unity, this);
            return true;
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log("OnUnityAdsAdLoaded : " + placementId);
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.Log("OnUnityAdsFailedToLoad : " + placementId);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.Log("OnUnityAdsShowFailure : " + placementId);
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log("OnUnityAdsShowStart : " + placementId);
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log("OnUnityAdsShowClick : " + placementId);
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log("OnUnityAdsShowComplete : " + placementId);

            if (placementId.Equals(interstitialAdUnitId_Unity))
            {
                LoadUnityInterstitialAd();
            }
            else if (placementId.Equals(rewardAdUnitId_Unity))
            {
                giveReward = true;
                LoadUnityRewardedAds();
            }
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log("OnInitializationFailed : " + message);
        }

        public bool ShowRewardVideo()
        {
            const string rewardMsg = "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
            if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                rewardedAd.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                    giveReward = true;
                });
                return true;
            }
            else
            {
                RequestRewarded();
                return ShowUnityRewardedVideo(); ;
            }
        }

        public void ShowInterstitial()
        {
            Debug.Log("ok =============");
            if (interstitial != null && interstitial.CanShowAd())
                interstitial.Show();
            else
            {
                RequestInterstitial();
                ShowUnityInterstitialAd();
            }
        }

        public void HideBanner()
        {
            if (this.bannerView != null)
                this.bannerView.Hide();
        }

        public void ShowBanner()
        {
            if (this.bannerView != null)
                this.bannerView.Show();
        }
    }
}