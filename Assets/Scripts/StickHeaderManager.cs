using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StickHeaderManager : MonoBehaviour
{

    //public bool isCollapsed = false;
    public bool logToConsole = false;
    public GameObject navigationContentPanel;
    //public List<GameObject> navigationPanels = new List<GameObject>();
    public List<StickyHeaderController> stickyControllers = new List<StickyHeaderController>();
    public SearchManager searchManager;

    int currentPanelIndex = 0;
    Vector2 currentPos;


    private void Start()
    {
        foreach (Transform child in navigationContentPanel.transform)
        {
            //navigationPanels.Add(child.gameObject);
            stickyControllers.Add(child.GetComponent<StickyHeaderController>());
        }
        //stickyControllers[0].SetActive(true);
        if (searchManager == null) LogError("Missing SearchManager.");
    }

    void OnEnable()
    {
        StickyHeaderController.OnCollapseOrExpandChanged += HandleCollapseEvent;
        StickyHeaderController.OnScrollChanged += HandleScrollEvent;
    }

    void OnDisable()
    {
        StickyHeaderController.OnCollapseOrExpandChanged -= HandleCollapseEvent;
        StickyHeaderController.OnScrollChanged -= HandleScrollEvent;
    }

    private void HandleCollapseEvent(bool b, Vector2 yPos)
    {
        //Debug.Log("[StickHeaderManager] HandleScrollEvent");
        foreach (StickyHeaderController controller in stickyControllers)
        {
            if (b)
            {
                //StartCoroutine(controller.CollapseHeader(yPos));
                controller.CollapseHeader(yPos);
            }
            else
            {
                //StartCoroutine(controller.ExpandHeader(yPos));
                controller.ExpandHeader(yPos);

            }
        }
    }

    private void HandleScrollEvent(Vector2 pos)
    {
        foreach (StickyHeaderController controller in stickyControllers)
        {
                controller.SetYPos(pos);
        }
    }

    public void OnPanelCentred(int currentButtonIndex, int previousButtonIndex)
    {
        Log("OnPanelCentred: " + currentButtonIndex + " " + stickyControllers[currentButtonIndex].name);
        currentPanelIndex = currentButtonIndex; 

        foreach (StickyHeaderController controller in stickyControllers)
        {
            controller.SetActive(false);
        }
        if (currentButtonIndex == 1) stickyControllers[currentButtonIndex].ResetView();
        stickyControllers[currentButtonIndex].SetActive(true);

    }

    public void OnPanelSelected(int currentButtonIndex)
    {
        Log("OnPanelSelected");
        searchManager.ClearSearch();
    }

    public void OnValueChanged(Vector2 v)
    {
        // potential to optimise by only updating YPos of Scroll Rects when swiped horizontally
        // accessed via On Value Change in Horizontal Scroll View scroll rect
        if (Mathf.Abs(v.x) > 0) 
        {
            

        }
    }

    void Log(object message)
    {
        if (logToConsole)
            Debug.Log("[StickyHeaderManager] "+message);
    }

    void LogError(object message)
    {
        if (logToConsole)
            Debug.LogError("[StickyHeaderManager] "+message);
    }

}
