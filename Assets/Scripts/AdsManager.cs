using UnityEngine;
using UnityEngine.Advertisements;

// Unity Ads 4.x replacement for the built-in Ads service of Unity 5.4 (Advertisement.IsReady/Show()).
public class AdsManager : IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private const string AndroidGameId = "1246863";
    private const string IosGameId = "1246864";
    // "video" is the default skippable placement created for Unity Ads games of this age.
    private const string InterstitialPlacementId = "video";

    private static readonly AdsManager instance = new AdsManager();
    private bool isAdLoaded;

    public static void Initialize()
    {
        if (!Advertisement.isSupported || Advertisement.isInitialized)
        {
            return;
        }

        var gameId = Application.platform == RuntimePlatform.IPhonePlayer ? IosGameId : AndroidGameId;
        Advertisement.Initialize(gameId, Debug.isDebugBuild, instance);
    }

    public static void ShowInterstitial()
    {
        if (Advertisement.isInitialized && instance.isAdLoaded)
        {
            instance.isAdLoaded = false;
            Advertisement.Show(InterstitialPlacementId, instance);
        }
    }

    private void LoadInterstitial()
    {
        Advertisement.Load(InterstitialPlacementId, this);
    }

    public void OnInitializationComplete()
    {
        LoadInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log("Unity Ads initialization failed: " + error + " - " + message);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        isAdLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log("Unity Ads failed to load " + placementId + ": " + error + " - " + message);
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log("Unity Ads failed to show " + placementId + ": " + error + " - " + message);
        LoadInterstitial();
    }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        LoadInterstitial();
    }
}
