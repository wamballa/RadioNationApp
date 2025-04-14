using UnityEngine;
using TMPro;

public class DebugPanel : MonoBehaviour
{
    public TMP_Text stateText;
    public RadioPlayer radioPlayer;
    private string androidState;


    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        androidState = AndroidRadioLauncher.CheckAndroidPlaybackState();
        if (stateText != null) stateText.text = "State = "+ androidState;
#endif
    }
}
