using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class UnityBannerManager : MonoBehaviour
{
    [SerializeField] string iosGameId = "5819292";
    [SerializeField] string bannerPlacementId = "Banner_iOS";
    [SerializeField] bool testMode = true;

    void Start()
    {
#if UNITY_IOS
        Advertisement.Initialize(iosGameId, testMode);
        StartCoroutine(ShowBannerWhenReady());
#endif
    }

    IEnumerator ShowBannerWhenReady()
    {
        while (!Advertisement.isInitialized)
        {
            Debug.Log("⏳ Waiting for Unity Ads...");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("✅ Unity Ads ready — loading banner");

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
}
