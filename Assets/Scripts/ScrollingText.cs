using UnityEngine;
using TMPro;

public class ScrollingText : MonoBehaviour
{
    public TMP_Text textMeshPro;
    public float scrollSpeed = 50f; // pixels per second
    public float checkInterval = 1f;

    private RectTransform textRect;
    private RectTransform maskRect;
    private float textWidth;
    private float maskWidth;
    private bool shouldScroll = false;
    private bool scrollingLeft = true;
    private Vector2 startPos;

    private void Start()
    {
        if (textMeshPro == null)
        {
            Debug.LogError("ScrollingText: TMP_Text is not assigned!");
            return;
        }

        textRect = textMeshPro.rectTransform;
        maskRect = textRect.parent.GetComponent<RectTransform>();
        startPos = textRect.anchoredPosition;

        InvokeRepeating(nameof(CheckTextWidth), 0f, checkInterval);
    }

    public void SetText(string newText)
    {
        textMeshPro.text = newText;
        textMeshPro.ForceMeshUpdate();
        CheckTextWidth();
    }

    private void CheckTextWidth()
    {
        textMeshPro.ForceMeshUpdate();
        textWidth = textMeshPro.preferredWidth;
        maskWidth = maskRect.rect.width;

        shouldScroll = textWidth > maskWidth;

        if (!shouldScroll)
        {
            textRect.anchoredPosition = startPos;
        }
    }

    private void FixedUpdate()
    {
        if (!shouldScroll) return;

        float direction = scrollingLeft ? -1f : 1f;
        textRect.anchoredPosition += Vector2.right * direction * scrollSpeed * Time.deltaTime;

        float minX = -(textWidth - maskWidth); // far left
        float maxX = 0f + 2f;                       // original start

        if (textRect.anchoredPosition.x <= minX)
        {
            textRect.anchoredPosition = new Vector2(minX, textRect.anchoredPosition.y);
            scrollingLeft = false;
        }
        else if (textRect.anchoredPosition.x >= maxX)
        {
            textRect.anchoredPosition = new Vector2(maxX, textRect.anchoredPosition.y);
            scrollingLeft = true;
        }
    }
}
