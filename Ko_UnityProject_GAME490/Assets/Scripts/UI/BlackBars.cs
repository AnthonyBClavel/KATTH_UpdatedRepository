using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackBars : MonoBehaviour
{
    private bool canMoveBars = true;
    private bool hasMovedBars = false;

    public float blackBarsSpeed = 400f;
    public float blackBarsDistance = 130f;

    private GameObject blackBars;
    private GameObject topBar;
    private GameObject bottomBar;

    private Vector3 topBarOriginalPosition;
    private Vector3 bottomBarOriginalPosition;
    private Vector3 topBarDestination;
    private Vector3 bottomBarDestination;

    private GameHUD gameHUDScript;
    private GameManager gameManagerScript;

    void Awake()
    {
        gameHUDScript = FindObjectOfType<GameHUD>();
        gameManagerScript = FindObjectOfType<GameManager>();

        SetElements();
    }

    // Start is called before the first frame update
    void Start()
    {
        topBarOriginalPosition = topBar.transform.localPosition;
        bottomBarOriginalPosition = bottomBar.transform.localPosition;

        topBarDestination = new Vector3(topBarOriginalPosition.x, topBarOriginalPosition.y - blackBarsDistance, topBarOriginalPosition.z);
        bottomBarDestination = new Vector3(bottomBarOriginalPosition.x, bottomBarOriginalPosition.y + blackBarsDistance, bottomBarOriginalPosition.z);
    }

    void LateUpdate()
    {
        DebuggingCheck();
        CanMoveBlackBarsCheck();
    }

    // Moves the black bars onto the screen
    public void MoveBlackBarsIn()
    {
        hasMovedBars = true;
    }

    // Moves the black bars out of the screen
    public void MoveBlackBarsOut()
    {
        hasMovedBars = false;
    }

    // Sets the black bars' position to their destination - used in the world intro script ONLY
    public void TurnOnBlackBars()
    {
        canMoveBars = false;

        topBar.transform.localPosition = topBarDestination;
        bottomBar.transform.localPosition = bottomBarDestination;
    }

    // Sets the black bars' position to their original position - used in the world intro script ONLY
    public void TurnOffBlackBars()
    {
        topBar.transform.localPosition = topBarOriginalPosition;
        bottomBar.transform.localPosition = bottomBarOriginalPosition;

        canMoveBars = true;
    }

    // Checks if the black bars can move
    private void CanMoveBlackBarsCheck()
    {
        if (canMoveBars)
        {
            // Move to original position if false
            if (!hasMovedBars)
            {
                if (topBar.transform.localPosition != topBarOriginalPosition)
                    topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, topBarOriginalPosition, blackBarsSpeed * Time.deltaTime);

                if (bottomBar.transform.localPosition != bottomBarOriginalPosition)
                    bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, bottomBarOriginalPosition, blackBarsSpeed * Time.deltaTime);
            }

            // Move to destination if true
            if (hasMovedBars)
            {
                if (topBar.transform.localPosition != topBarDestination)
                    topBar.transform.localPosition = Vector3.MoveTowards(topBar.transform.localPosition, topBarDestination, blackBarsSpeed * Time.deltaTime);

                if (bottomBar.transform.localPosition != bottomBarDestination)
                    bottomBar.transform.localPosition = Vector3.MoveTowards(bottomBar.transform.localPosition, bottomBarDestination, blackBarsSpeed * Time.deltaTime);
            }
        }
    }

    // Sets private variables, objects, and components
    private void SetElements()
    {
        // Sets the game objects by looking at names of children
        for (int i = 0; i < gameHUDScript.transform.parent.childCount; i++)
        {
            GameObject child = gameHUDScript.transform.parent.GetChild(i).gameObject;

            if (child.name == "BlackBars")
            {
                blackBars = child;

                for (int j = 0; j < blackBars.transform.childCount; j++)
                {
                    GameObject child02 = blackBars.transform.GetChild(j).gameObject;

                    if (child02.name == "TopBar")
                        topBar = child02;
                    if (child02.name == "BottomBar")
                        bottomBar = child02;
                }    
            }
        }
    }

    // Updates the destination if the blackBarDistance is changed - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            // Toggles the black bars
            if (Input.GetKeyDown(KeyCode.P) && canMoveBars)
                hasMovedBars = !hasMovedBars;

            if (topBarDestination != new Vector3(0, topBarOriginalPosition.y - blackBarsDistance, 0))
                topBarDestination = new Vector3(0, topBarOriginalPosition.y - blackBarsDistance, 0);

            if (bottomBarDestination != new Vector3(0, bottomBarOriginalPosition.y + blackBarsDistance, 0))
                bottomBarDestination = new Vector3(0, bottomBarOriginalPosition.y + blackBarsDistance, 0);
        }
    }

}
