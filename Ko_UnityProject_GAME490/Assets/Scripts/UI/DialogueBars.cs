using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueBars : MonoBehaviour
{
    public bool canMoveBars = true;
    private bool hasMovedTopBar = false;
    private bool hasMovedBottomBar = false;

    private float blackBarsSpeed;

    private GameObject topBar;
    private GameObject bottomBar;
    private GameObject skipTutorialButton;

    Vector3 originalTopBarPos;
    Vector3 originalBottomBarPos;
    Vector3 topBarDestination;
    Vector3 bottomBarDestination;

    private TorchMeterScript torchMeterScript;
    private GameHUD gameHUDScript;
    private SkipButton skipButtonScript;
    private GameManager gameManagerScript;
    private SaveManagerScript saveManagerScript;

    void Awake()
    {
        SetScripts();
        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        //originalTopBarPos = topBar.transform.localPosition;
        //originalBottomBarPos = bottomBar.transform.localPosition;

        originalTopBarPos = new Vector3(0, 620, 0); 
        originalBottomBarPos = new Vector3(0, -620, 0);

        topBarDestination = new Vector3(0, 490, 0); //465.31f
        bottomBarDestination = new Vector3(0, -490, 0); //-465.31f
    }

    void LateUpdate()
    {
        BlackBarsDebuggingCheck();

        if (canMoveBars)
        {
            if (hasMovedTopBar && topBar.transform.localPosition != topBarDestination)
                topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, topBarDestination, blackBarsSpeed * Time.deltaTime);

            if (hasMovedBottomBar && bottomBar.transform.localPosition != bottomBarDestination)
                bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, bottomBarDestination, blackBarsSpeed * Time.deltaTime);

            if (!hasMovedTopBar && topBar.transform.localPosition != originalTopBarPos)
                topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, originalTopBarPos, blackBarsSpeed * Time.deltaTime);        
            
            if (!hasMovedBottomBar && bottomBar.transform.localPosition != originalBottomBarPos)
                bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, originalBottomBarPos, blackBarsSpeed * Time.deltaTime);
        }
    }

    // Toggles the dialogue bars on (moves into screen) or off (moves out of screen)
    public void ToggleDialogueBars()
    {
        hasMovedTopBar = !hasMovedTopBar;
        hasMovedBottomBar = !hasMovedBottomBar;
    }

    // Sets all of the UI active
    public void TurnOnHUD()
    {
        torchMeterScript.gameObject.SetActive(true);
        gameHUDScript.notificationBubblesHolder.SetActive(true);
        gameHUDScript.EnableNotificationsToggle();

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            skipTutorialButton.SetActive(true);
    }

    //Sets all of the UI inactive
    public void TurnOffHUD()
    {
        torchMeterScript.gameObject.SetActive(false);
        gameHUDScript.notificationBubblesHolder.SetActive(false);

        if (SceneManager.GetActiveScene().name == "TutorialMap")
            skipTutorialButton.SetActive(false);
    }

    // Sets the dialogue bars to their final position - ONLY USED in the world intro script
    public void SetDialogueBars()
    {
        //topBar.transform.localPosition = topBarDestination;
        //bottomBar.transform.localPosition = bottomBarDestination;

        topBar.transform.localPosition = new Vector3(0, 490, 0); //465.31f
        bottomBar.transform.localPosition = new Vector3(0, -490, 0); //-465.31f
    }

    // Sets the dialogue bars back to their original position - ONLY USED in the world intro script
    public void ResetDialogueBars()
    {
        //topBar.transform.localPosition = originalTopBarPos;
        //bottomBar.transform.localPosition = originalBottomBarPos;

        topBar.transform.localPosition = new Vector3(0, 620, 0);
        bottomBar.transform.localPosition = new Vector3(0, -620, 0);
    }

    // Sets the script to find
    private void SetScripts()
    {
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
        gameManagerScript = FindObjectOfType<GameManager>();
        saveManagerScript = FindObjectOfType<SaveManagerScript>();

        if (SceneManager.GetActiveScene().name == "TutorialMap")
        {
            skipButtonScript = FindObjectOfType<SkipButton>();
            skipTutorialButton = skipButtonScript.gameObject;
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "TopBar")
                topBar = child.gameObject;
            if (child.name == "BottomBar")
                bottomBar = child.gameObject;
        }

        blackBarsSpeed = gameManagerScript.blackBarsSpeed;
    }

    // Toggles the Black Bars - For Debugging Purposes ONLY
    private void BlackBarsDebuggingCheck()
    {
        if (Input.GetKeyDown(KeyCode.P) && saveManagerScript.isDebugging)
            ToggleDialogueBars();
    }

}
