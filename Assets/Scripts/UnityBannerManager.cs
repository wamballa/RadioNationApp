using UnityEngine;
using UnityEngine.Advertisements;

public class UnityBannerManager : MonoBehaviour
{
    [SerializeField] string iosGameId = "5819292";
    [SerializeField] string bannerPlacementId = "banner"; // Default Unity placement

    void Start()
    {
#if UNITY_IOS
        Advertisement.Initialize(iosGameId, true); // true = test mode
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(bannerPlacementId);
#endif
    }
}
