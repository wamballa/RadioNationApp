using UnityEngine;
using Gley.MobileAds;


public class Monetisation : MonoBehaviour
{
    public bool logToConsole = true;

    private void Awake()
    {
        API.Initialize(OnInitialized);

        Log("[Awake]  Initialisation =============================================");
        Log("iOS device ID: " + UnityEngine.iOS.Device.advertisingIdentifier);
        Log("[Awake]  Initialisation <<<>>>");

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