using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ReselectLastSelected : MonoBehaviour
{
    private string mainMenu = "MainMenu";
    private string sceneName;

    public static ReselectLastSelected instance;
    private GameObject lastSelectedObject;
    private EventSystem eventSystemScript;

    // Awake is called before Start()
    void Awake()
    {
        sceneName = SceneManager.GetActiveScene().name;

        SetScripts();
    }

    // Start is called before the first frame update
    void Start()
    {
        DisableScriptCheck();
    }

    // Update is called once per frame
    void Update()
    {
        ReselectCheck();
    }

    // Checks if the event system has selected an object - returns true if so, false otherwise
    private bool HasSelectedObject()
    {
        GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
        if (currentSelectedGameObject == null) return false;

        lastSelectedObject = currentSelectedGameObject;
        return true;
    }

    // Checks to set the last selected object as the current selected object
    private void ReselectLastSelectedObject()
    {
        if (!gameObject.activeInHierarchy) return;

        eventSystemScript.SetSelectedGameObject(null);
        eventSystemScript.SetSelectedGameObject(lastSelectedObject);  
    }

    // Checks to disable the script at the start of the scene
    private void DisableScriptCheck()
    {
        if (sceneName == mainMenu) return;

        enabled = false;
    }

    // Checks to reselect the last select object
    private void ReselectCheck()
    {
        if (HasSelectedObject()) return;

        ReselectLastSelectedObject();
    }

    // Sets the scripts to use
    private void SetScripts()
    {
        eventSystemScript = FindObjectOfType<EventSystem>();
        instance = this;
    }

}