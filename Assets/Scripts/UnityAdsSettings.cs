using UnityEngine;

/// <summary>
/// Unity Ads game IDs for your own Unity Ads project.
///
/// The IDs are kept out of the code and out of git so that forks of this project do not
/// show ads for (or earn money for) someone else's account. To enable ads in your build:
///   1. Create the asset via Assets > Create > Kamikaze Hero > Unity Ads Settings.
///   2. Save it as Assets/Resources/UnityAdsSettings.asset (this exact path; it is git-ignored).
///   3. Fill in the game IDs from your Unity Ads dashboard.
/// Without the asset the game runs normally, just without ads.
/// </summary>
[CreateAssetMenu(fileName = ResourceName, menuName = "Kamikaze Hero/Unity Ads Settings")]
public class UnityAdsSettings : ScriptableObject
{
    public const string ResourceName = "UnityAdsSettings";

    public string AndroidGameId;
    public string IosGameId;

    /// <summary>Loads the settings from a Resources folder, or returns null if they are missing.</summary>
    public static UnityAdsSettings Load()
    {
        return Resources.Load<UnityAdsSettings>(ResourceName);
    }

    public string GetGameIdForCurrentPlatform()
    {
        return Application.platform == RuntimePlatform.IPhonePlayer ? IosGameId : AndroidGameId;
    }
}
