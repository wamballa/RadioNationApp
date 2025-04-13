using UnityEngine;
using TMPro;

public class DebugPanel : MonoBehaviour
{
    public TMP_Text stateText;
    public RadioPlayer radioPlayer;


    // Update is called once per frame
    void Update()
    {
        if (stateText != null) stateText.text = "State = "+radioPlayer.androidState;
    }
}
