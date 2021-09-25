using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackBars : MonoBehaviour
{
    [Range(0.1f, 1.0f)]
    public float animLength = 0.25f;
    [Range(1f, 540f)]
    public float animDistance = 130f;

    private bool canMoveBars = true;
    private bool hasMovedBars = false;
    private bool hasSetVectors = false;
    private bool canDebugBars = false;

    private GameObject blackBars;
    private GameObject topBar;
    private GameObject bottomBar;

    private RectTransform topBarRectTransform;
    private RectTransform bottomBarRectTransform;

    private Vector2 topBarOriginalPosition;
    private Vector2 bottomBarOriginalPosition;
    private Vector2 topBarDestination;
    private Vector2 bottomBarDestination;

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
        SetVectorsCheck();
    }

    void LateUpdate()
    {
        DebuggingCheck();
    }

    // Moves the black bars onto the screen
    public void MoveBlackBarsIn()
    {
        hasMovedBars = true;

        StopAllCoroutines();
        StartCoroutine(LerpBlackBar(topBar, topBarDestination, animLength));
        StartCoroutine(LerpBlackBar(bottomBar, bottomBarDestination, animLength));
    }

    // Moves the black bars out of the screen
    public void MoveBlackBarsOut()
    {
        hasMovedBars = false;

        StopAllCoroutines();
        StartCoroutine(LerpBlackBar(topBar, topBarOriginalPosition, animLength));
        StartCoroutine(LerpBlackBar(bottomBar, bottomBarOriginalPosition, animLength));
    }

    // Sets the black bars' position to their destination - used in the world intro script ONLY
    public void TurnOnBlackBars()
    {
        canMoveBars = false;

        SetVectorsCheck();
        topBarRectTransform.anchoredPosition = topBarDestination;
        bottomBarRectTransform.anchoredPosition = bottomBarDestination;
    }

    // Sets the black bars' position to their original position - used in the world intro script ONLY
    public void TurnOffBlackBars()
    {
        topBarRectTransform.anchoredPosition = topBarOriginalPosition;
        bottomBarRectTransform.anchoredPosition = bottomBarOriginalPosition;

        canMoveBars = true;
    }

    // Sets the original positions and destinations
    private void SetVectorsCheck()
    {
        // Has a check since intro script calls the TurnOnBlackBars() function BEFORE setting vector variables - Unity bug
        if (!hasSetVectors)
        {
            topBarOriginalPosition = topBarRectTransform.anchoredPosition;
            bottomBarOriginalPosition = bottomBarRectTransform.anchoredPosition;

            topBarDestination = new Vector2(topBarOriginalPosition.x, topBarOriginalPosition.y - animDistance);
            bottomBarDestination = new Vector2(bottomBarOriginalPosition.x, bottomBarOriginalPosition.y + animDistance);

            hasSetVectors = true;
        }
    }

    // Lerps the position of an object to another, over a specific duration (endPosition = position to lerp to, duration = seconds)
    private IEnumerator LerpBlackBar(GameObject blackBar, Vector2 endPosition, float duration)
    {
        canDebugBars = false;
        float time = 0;
        Vector2 startPosition = blackBar.GetComponent<RectTransform>().anchoredPosition;

        while (time < duration)
        {
            blackBar.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPosition, endPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        blackBar.GetComponent<RectTransform>().anchoredPosition = endPosition;
        canDebugBars = true;
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
                    RectTransform rectTransform = child02.GetComponent<RectTransform>();

                    if (child02.name == "TopBar")
                    {
                        topBar = child02;
                        topBarRectTransform = rectTransform;
                    }                   
                    if (child02.name == "BottomBar")
                    {
                        bottomBar = child02;
                        bottomBarRectTransform = rectTransform;
                    }
                }    
            }
        }
    }

    // Checks when the black bars can move in/out - For Debugging Purposes Only
    private void ToggleBlackBars()
    {
        // Move bars to their destination
        if (!hasMovedBars)
        {
            StopAllCoroutines();
            StartCoroutine(LerpBlackBar(topBar, topBarDestination, animLength));
            StartCoroutine(LerpBlackBar(bottomBar, bottomBarDestination, animLength));
        }
        // Move bars to their originalPosition
        if (hasMovedBars)
        {
            StopAllCoroutines();
            StartCoroutine(LerpBlackBar(topBar, topBarOriginalPosition, animLength));
            StartCoroutine(LerpBlackBar(bottomBar, bottomBarOriginalPosition, animLength));
        }

        hasMovedBars = !hasMovedBars;
    }

    // Updates the destination if the blackBarDistance is changed - For Debugging Purposes ONLY
    private void DebuggingCheck()
    {
        if (gameManagerScript.isDebugging)
        {
            // Toggles the black bars
            if (Input.GetKeyDown(KeyCode.P) && canMoveBars)
                ToggleBlackBars();

            if (topBarDestination != new Vector2(0, topBarOriginalPosition.y - animDistance))
                topBarDestination = new Vector2(0, topBarOriginalPosition.y - animDistance);

            if (bottomBarDestination != new Vector2(0, bottomBarOriginalPosition.y + animDistance))
                bottomBarDestination = new Vector2(0, bottomBarOriginalPosition.y + animDistance);

            // If their destination changes, move the bars to their new destination
            if (hasMovedBars && canDebugBars)
            {
                if (topBarRectTransform.anchoredPosition != topBarDestination)
                    topBarRectTransform.anchoredPosition = topBarDestination;

                if (bottomBarRectTransform.anchoredPosition != bottomBarDestination)
                    bottomBarRectTransform.anchoredPosition = bottomBarDestination;
            }
        }
    }

    /*** OLD FUNCTIONS START HERE - FOR REFERENCE ***/

    // Checks if the black bars can move - OLD VERSION
    /*private void CanMoveBlackBarsCheck()
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
    }*/

}
