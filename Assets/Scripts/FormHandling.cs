using UnityEngine;
using UnityEngine.Networking;

public class FormHandling : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenNominateForm()
    {
        string formUrl = "https://forms.gle/sPiCJYj1vQfcknNK6";
        Application.OpenURL(formUrl);
    }

    public void OpenFeedbackForm()
    {
        string feedbackFormUrl = "https://forms.gle/VYb2whfB9YaopSEz9";
        Application.OpenURL(feedbackFormUrl);
    }
}
