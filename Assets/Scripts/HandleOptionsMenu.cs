using UnityEngine;
using UnityEngine.UI;

public class HandleOptionsMenu : MonoBehaviour
{
    public bool logToConsole = true;
    public GameObject debugMenu;
    public GameObject optionsMenu;
    public FavouritesManager favouritesManager;

    [Header ("Option Menu Buttons")]
    public Button optionsOpenButton;
    public Button optionsCloseButton;
    public Button toggleDebugButton;
    public Button clearFavouritesButton;

    private bool isDebugActive = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (optionsOpenButton != null) optionsOpenButton.onClick.AddListener(OnOptionsOpen);
        if (optionsCloseButton != null) optionsCloseButton.onClick.AddListener(OnOptionsClose);
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (debugMenu != null) debugMenu.SetActive(isDebugActive);
        if (toggleDebugButton != null) toggleDebugButton.onClick.AddListener(OnToggleDebugMenu);
        if (clearFavouritesButton != null) clearFavouritesButton.onClick.AddListener(favouritesManager.OnClearFavourites);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnToggleDebugMenu()
    {
        Log("[HandleOptionsMenu::OnToggleDebugMenu]");
        isDebugActive = !isDebugActive;
        debugMenu.SetActive(isDebugActive);
    }

    private void OnOptionsOpen()
    {
        Log("[HandleOptionsMenu::OnButtonPressed]");
        optionsMenu.SetActive(true);
    }
    private void OnOptionsClose()
    {
        Log("[HandleOptionsMenu::OnButtonPressed]");
        optionsMenu.SetActive(false);
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
