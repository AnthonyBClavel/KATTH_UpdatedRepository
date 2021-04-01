using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBars : MonoBehaviour
{
    public GameObject topBar;
    public GameObject bottomBar;

    Vector3 originalTopBarPos;
    Vector3 originalBottomBarPos;
    Vector3 topBarDestination;
    Vector3 bottomBarDestination;

    public bool canMoveBars = true;
    private bool hasMovedTopBar = false;
    private bool hasMovedBottomBar = false;
    private bool hasTurnedOnHUD = true;
    private bool canTurnOnHUD = false;

    public float barSpeed;

    private TorchMeterScript torchMeterScript;
    private GameHUD gameHUDScript;

    void Awake()
    {
        torchMeterScript = FindObjectOfType<TorchMeterScript>();
        gameHUDScript = FindObjectOfType<GameHUD>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //originalTopBarPos = topBar.transform.localPosition;
        //originalBottomBarPos = bottomBar.transform.localPosition;

        originalTopBarPos = new Vector3(0, 620, 0);
        originalBottomBarPos = new Vector3(0, -620, 0);

        topBarDestination = new Vector3(0, 465.31f, 0);
        bottomBarDestination = new Vector3(0, -465.31f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //DialogueBarDebugging();
    }

    void LateUpdate()
    {
        if(canMoveBars)
        {
            if (hasMovedTopBar && topBar.transform.localPosition != topBarDestination)
                topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, topBarDestination, barSpeed * Time.deltaTime);

            if (hasMovedBottomBar && bottomBar.transform.localPosition != bottomBarDestination)
                bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, bottomBarDestination, barSpeed * Time.deltaTime);

            if (!hasMovedTopBar && topBar.transform.localPosition != originalTopBarPos)
                topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, originalTopBarPos, barSpeed * Time.deltaTime);        
            
            if (!hasMovedBottomBar && bottomBar.transform.localPosition != originalBottomBarPos)
                bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, originalBottomBarPos, barSpeed * Time.deltaTime);
        }
    }

    // Toggles the dialogue bars on (moves into screen) or off (moves out of screen)
    public void ToggleDialogueBars()
    {
        hasMovedTopBar = !hasMovedTopBar;
        hasMovedBottomBar = !hasMovedBottomBar;
        hasTurnedOnHUD = !hasTurnedOnHUD;
    }

    // Sets all of the UI active
    public void TurnOnHUD()
    {
        //Debug.Log("Has Turned On HUD");
        canTurnOnHUD = true;
        gameHUDScript.gameObject.SetActive(true);
        torchMeterScript.gameObject.SetActive(true);
    }

    //Sets all of the UI inactive
    public void TurnOffHUD()
    {
        //Debug.Log("Has Turned Off HUD");
        canTurnOnHUD = false;
        gameHUDScript.gameObject.SetActive(false);
        torchMeterScript.gameObject.SetActive(false);
    }


    // Sets the dialogue bars to their final position - ONLY USED in the world intro script
    public void SetDialogueBars()
    {
        //topBar.transform.localPosition = topBarDestination;
        //bottomBar.transform.localPosition = bottomBarDestination;

        topBar.transform.localPosition = new Vector3(0, 465.31f, 0);
        bottomBar.transform.localPosition = new Vector3(0, -465.31f, 0);
    }

    // Sets the dialogue bars back to their original position - ONLY USED in the world intro script
    public void ResetDialogueBars()
    {
        //topBar.transform.localPosition = originalTopBarPos;
        //bottomBar.transform.localPosition = originalBottomBarPos;

        topBar.transform.localPosition = new Vector3(0, 620, 0);
        bottomBar.transform.localPosition = new Vector3(0, -620, 0);
    }

    // Only used for debugging the dialogue bar script (this script)
    private void DialogueBarDebugging()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleDialogueBars();
        }

        if (hasTurnedOnHUD && !canTurnOnHUD && canMoveBars)
            TurnOnHUD();

        if(!hasTurnedOnHUD && canTurnOnHUD && canMoveBars)
            TurnOffHUD();
    }

}
