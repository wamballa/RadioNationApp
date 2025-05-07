using UnityEngine;
using UnityEngine.Advertisements;

public class UnityBannerManager : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] string iosGameId = "5819292";
    [SerializeField] string bannerPlacementId = "Banner_iOS";
    [SerializeField] bool testMode = true;

    void Start()
    {
#if UNITY_IOS
        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(iosGameId, testMode, this);
            Debug.Log("🔁 Initializing Unity Ads...");
        }
        else
        {
            OnInitializationComplete();
        }
#endif
    }

    public void OnInitializationComplete()
    {
        Debug.Log("✅ Unity Ads initialized — now loading banner");

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = () =>
            {
                Debug.Log("✅ Banner loaded");
                Advertisement.Banner.Show(bannerPlacementId);
            },
            errorCallback = (message) =>
            {
                Debug.LogWarning("❌ Banner failed to load: " + message);
            }
        };

        Advertisement.Banner.Load(bannerPlacementId, options);
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"❌ Unity Ads init failed: {error.ToString()} - {message}");
    }
}
