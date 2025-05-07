using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class UnityBannerManager : MonoBehaviour
{
    [SerializeField] string iosGameId = "5849536";
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
        float timeout = 10f;
        float waited = 0f;

        while (!Advertisement.isInitialized)
        {
            Debug.Log($"⏳ Waiting... isInitialized: {Advertisement.isInitialized} (waited {waited}s)");

            yield return new WaitForSeconds(0.5f);

            waited += 0.5f;

            if (waited >= timeout)
            {
                Debug.LogError("❌ Timeout: Unity Ads never initialized!");
                yield break;
            }

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
