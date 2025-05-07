using UnityEngine;
using Gley.MobileAds;


public class Monetisation : MonoBehaviour
{
    public bool logToConsole = true;

    private void Awake()
    {
        // TEST DEVICES - Get the GAID
        //IronSource.Agent.validateIntegration(); // ✅ OK on GAID-capable devices
        //string s = IronSource.Agent.getAdvertiserId();
        //Log("[Awake] GAID = " + s);

        API.Initialize(OnInitialized);

        Log("[Awake]  Initialisation =============================================");

    }


    void OnInitialized()
    {
        Log("[OnInitialized] OnInitialized =============================================");
        ShowBanner();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ShowBanner()
    {
        // Push banner 100 pixels down from top of screen
        //Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, bannerAnchor.position);
        //Gley.MobileAds.API.ShowBanner((int)screenPos.x, (int)screenPos.y, BannerType.Banner);
        //API.ShowBanner(bannerPositionX:0, bannerPositionY:500, bannerType:BannerType.Banner);
        Log("[ShowBanner] ShowBanner =============================================");

        API.ShowBanner(BannerPosition.Bottom, BannerType.Banner);
    }

    void Log(object message)
    {
        if (logToConsole)
            Debug.Log("[Monetisation] " + message);
    }

    void LogError(object message)
    {
        if (logToConsole)
            Debug.LogError("[Monetisation] " + message);
    }
}