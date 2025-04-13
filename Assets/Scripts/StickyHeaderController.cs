using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StickyHeaderController : MonoBehaviour
{
    [Header ("Debug Flag")]
    public bool logToConsole = false;

    public ScrollRect scrollRect;           // Reference to your ScrollRect

    public RectTransform largeHeader;       // Large header inside the scrollable content
    public RectTransform smallHeader;          // Fixed small header (in StickyHeader)

    public RectTransform viewport;
    public RectTransform content;

    public float collapseThreshold = 200f;  // Pixel offset threshold to trigger collapse

    private bool isCollapsed = false;

    public bool isActive;

    public static event Action<bool, Vector2> OnCollapseOrExpandChanged;
    public static event Action<Vector2> OnScrollChanged;

    float height;

    private NestedScrollRect nestedScrollRect;

    //public string currentScreenTitle = "All Adults";
    //public TMP_Text smallHeaderText;

    void Start()
    {
        if (content == null) LogError("[StickyHeaderController] No Content Panel");
        
        nestedScrollRect = GetComponent<NestedScrollRect>();
        if (nestedScrollRect == null) LogError("[SticyControllerHeader] Missing Nested Scroll Rect");

        scrollRect.onValueChanged.AddListener(HandleScrollChanged);

        height = content.rect.height;

        //print($"{transform.name}    Height = " + height);

        SetActive(isActive);

        OnCollapseOrExpandChanged?.Invoke(false, scrollRect.content.anchoredPosition);

    }

    private void Update()
    {
        if (true)
        {

            //print("Helight = +" + height);

            Vector3 position = scrollRect.content.anchoredPosition;

            if (position.y < 0)
            {
                position.y = 0;
                content.anchoredPosition = position;
            }

            if (position.y > height)
            {
                position.y = height;
                scrollRect.content.anchoredPosition = position;
            }
        }

        //if (isActive) smallHeaderText.text = currentScreenTitle;
    }



    public void ResetView()
    {
        Vector3 position = scrollRect.content.anchoredPosition;

        position.y = 0;
        scrollRect.content.anchoredPosition = position;

        largeHeader.transform.GetComponent<Image>().enabled = true;
        smallHeader.transform.GetComponent<Image>().enabled = false;

    }

    void HandleScrollChanged(Vector2 scrollNormalizedPos)
    {
        float scrolledPixels = 0;

        scrolledPixels = scrollRect.content.anchoredPosition.y;

        if (isActive) OnScrollChanged?.Invoke(scrollRect.content.anchoredPosition);

        if (isActive)
        {
            if (!isCollapsed && scrolledPixels > collapseThreshold)
            {
                Log("[StickyHeaderController] Collapse");
                //StartCoroutine(CollapseHeader());
                isCollapsed = true;
                //OnCollapseOrExpandChanged?.Invoke(isCollapsed, scrollRect.content.anchoredPosition);
            }
            else if (isCollapsed && scrolledPixels < collapseThreshold)
            {
                Log("[StickyHeaderController] Expand");
                //StartCoroutine(ExpandHeader());
                isCollapsed = false;
                //OnCollapseOrExpandChanged?.Invoke(isCollapsed, scrollRect.content.anchoredPosition);
            }

            OnCollapseOrExpandChanged?.Invoke(isCollapsed, scrollRect.content.anchoredPosition);
        }


    }

    public void CollapseHeader(Vector2 f)
    {
        // Disable all images in largeHeader
        foreach (Image img in largeHeader.GetComponentsInChildren<Image>(true))
            img.enabled = false;

        foreach (TextMeshProUGUI text in largeHeader.GetComponentsInChildren<TextMeshProUGUI>(true))
            text.enabled = false;

        // Enable all images in smallHeader
        foreach (Image img in smallHeader.GetComponentsInChildren<Image>(true))
            img.enabled = true;

        foreach (TextMeshProUGUI text in smallHeader.GetComponentsInChildren<TextMeshProUGUI>(true))
            text.enabled = true;

        smallHeader.GetComponent<Image>().enabled = true;
    }

    public void ExpandHeader(Vector2 f)
    {

        // Disable all images in largeHeader
        foreach (Image img in largeHeader.GetComponentsInChildren<Image>(true))
            img.enabled = true;

        foreach (TextMeshProUGUI text in largeHeader.GetComponentsInChildren<TextMeshProUGUI>(true))
            text.enabled = true;

        // Enable all images in smallHeader
        foreach (Image img in smallHeader.GetComponentsInChildren<Image>(true))
            img.enabled = false;

        foreach (TextMeshProUGUI text in smallHeader.GetComponentsInChildren<TextMeshProUGUI>(true))
            text.enabled = false;

        smallHeader.GetComponent<Image>().enabled = false ;

    }

    public void SetYPos(Vector2 pos)
    {
        //Vector3 position = scrollRect.content.anchoredPosition;
        //position.y = f;
        content.anchoredPosition = pos;
    }

    public void SetActive(bool b)
    {
        isActive = b;
        Log("[StickyHeadController]: " + transform.name);
        Log("[StickyHeadController]: " + nestedScrollRect.name);
        nestedScrollRect.inertia = b;

    }

    void Log(object message)
    {
        if (logToConsole)
            Debug.Log(message);
    }

    void LogError(object message)
    {
        if (logToConsole)
            Debug.LogError(message);
    }
}
