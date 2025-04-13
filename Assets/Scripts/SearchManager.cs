using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public Button clearButton;

    private UIManager uiManager;
    private List<StationButton> allButtons = new List<StationButton>();

    private void Start()
    {
        //uiManager = FindFirstObjectByType<UIManager>();
        allButtons.AddRange(FindObjectsByType<StationButton>(FindObjectsSortMode.None));
        //allButtons.AddRange(FindObjectsOfType<StationButton>());

        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        clearButton.onClick.AddListener(ClearSearch);
    }

    private void OnSearchChanged(string input)
    {
        string query = input.Trim().ToLower();
        bool active = query.Length >= 2;

        foreach (var btn in allButtons)
        {
            bool match = !active || btn.stationName.ToLower().Contains(query);
            btn.gameObject.SetActive(match);
        }
    }

    public void ClearSearch()
    {
        searchInputField.text = "";
        foreach (var btn in allButtons)
        {
            btn.gameObject.SetActive(true);
        }
    }
}
