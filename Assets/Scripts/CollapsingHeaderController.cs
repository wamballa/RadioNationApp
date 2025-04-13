using UnityEngine;
using UnityEngine.UI;

public class CollapsingHeaderController : MonoBehaviour
{
    [Header("References")]
    public ScrollRect scrollRect;          // The scrollable area containing LargeHeader and radio buttons
    public RectTransform largeHeader;      // The RectTransform of your large header (inside the ScrollRect)
    public GameObject smallHeader;         // The fixed small header (outside the ScrollRect)

    [Header("Settings")]
    public float collapseThreshold = 200f; // Pixel amount scrolled at which to show the small header

    private bool isSticky = false;

    void Start()
    {
        // Make sure small header is initially hidden.
        if (smallHeader != null)
            smallHeader.SetActive(false);

        // Listen for scroll changes.
        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    void OnScrollChanged(Vector2 normalizedPos)
    {
        // The scrollable content's anchoredPosition.y tells us how far we've scrolled.
        // Assuming your content starts with largeHeader at position 0 and scrolls upward (anchoredPosition becomes negative),
        // we use the absolute value.
        float scrolled = Mathf.Abs(largeHeader.anchoredPosition.y);

        if (!isSticky && scrolled >= collapseThreshold)
        {
            // Show the small header when scrolled enough
            smallHeader.SetActive(true);
            isSticky = true;
        }
        else if (isSticky && scrolled < collapseThreshold)
        {
            // Hide the small header when scrolling back down
            smallHeader.SetActive(false);
            isSticky = false;
        }
    }
}
